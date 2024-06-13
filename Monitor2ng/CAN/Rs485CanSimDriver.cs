using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO.Ports;
using System.Timers;
using System.Diagnostics;
using NPOI.SS.Formula.Functions;

namespace Monitor2ng.CAN
{
    public class Rs485CanSimDriver : ICanDriver
    {
        public Dictionary<string, string> Configs { get; set; } = new Dictionary<string, string>();
        public bool EnableHardwareFilter { get; set; } = false;
        public int ClientCount { get; set; } = 0;
        public bool IsInitialized { get; private set; } = false;
        public bool IsStarted { get; set; } = false;
        public bool IsListening { get; set; } = false;
        public Dictionary<Guid, Tuple<CanFrameFilter, ICanDriver.FrameHandler>> FrameDispatchTable { get; set; } = new Dictionary<Guid, Tuple<CanFrameFilter, ICanDriver.FrameHandler>>();

        public event ICanDriver.FrameHandler FrameArriveEvent;

        SerialPort serialPort = new SerialPort();

        private Timer receiveQueryTimer;

        private object channelLock = new object();

        public void EditConfig(string key, string value)
        {
            if (!IsInitialized)
            {
                if (Configs.ContainsKey(key))
                {
                    Configs[key] = value;
                }
                else
                {
                    Configs.Add(key, value);
                }
            }
            else
            {
                if (Configs.ContainsKey(key))
                {
                    if (Configs[key] != value)
                    {
                        MessageBox.Show("运行时无法修改通道设置!");
                    }
                }
            }
        }

        public void Initialize()
        {
            var port = Configs["Port"];
            string[] available_port = SerialPort.GetPortNames();
            if (port == null || !available_port.Contains(port))
            {
                MessageBox.Show("端口不存在!");
                return;
            }

            var baudrate_str = Configs["Baudrate"];
            int baudrate;
            if (!int.TryParse(baudrate_str, out baudrate))
            {
                MessageBox.Show("波特率未设置!");
                return;
            }

            serialPort.PortName = port;
            serialPort.BaudRate = baudrate;
            serialPort.DataBits = 8;
            serialPort.StopBits = StopBits.One;
            serialPort.Parity = Parity.None;

            IsInitialized = true;
        }

        public int Receive(out RawCanFrame frame, int timeout)
        {
            if (!IsInitialized)
            {
                MessageBox.Show("未初始化!");
                frame = new RawCanFrame();
                return -1;
            } 
            else
            {
                if (!IsStarted)
                {
                    MessageBox.Show("未启动!");
                    frame = new RawCanFrame();
                    return -1;
                }
                else
                {
                    if (!IsListening)
                    {
                        MessageBox.Show("未监听!");
                        frame = new RawCanFrame();
                        return -1;
                    }
                    else
                    {
                        if (serialPort.BytesToRead > 10)
                        { 
                            byte[] buffer = new byte[10];
                            do 
                            {    
                                serialPort.Read(buffer, 0, 1);
                            } while (buffer[0] != 0x55 && serialPort.BytesToRead > 9);
                            if (serialPort.BytesToRead < 9)
                            {
                                frame = new RawCanFrame();
                                return -1;
                            }
                            serialPort.Read(buffer, 1, 9);

                            byte sum = 0;
                            for (int i = 0; i < 9; i++)
                            {
                                sum += buffer[i];
                            }
                            if (sum == buffer[9])
                            {
                                frame = new RawCanFrame();
                                frame.id = 0x100;
                                Array.Copy(buffer, 1, frame.data, 0, 8);
                                return 0;
                            }
                            else
                            {
                                frame = new RawCanFrame();
                                return -1;
                            }
                        }
                        else
                        {
                            frame = new RawCanFrame();
                            return -1;
                        }
                    }
                }
            }
        }

        public void RegisterClient()
        {
            if (ClientCount == 0)
            {
                Initialize();
                ClientCount++;
            }
            else
            {
                MessageBox.Show("串口只支持一个设备");
            }
        }

        public Guid RegisterHandler(CanFrameFilter filter, ICanDriver.FrameHandler handler)
        {
            Guid guid = new Guid();
            FrameDispatchTable.Add(guid, new Tuple<CanFrameFilter, ICanDriver.FrameHandler>(filter, handler));
            return guid;
        }

        public void Start()
        {
            if (!IsInitialized)
            {
                MessageBox.Show("未初始化!");
            }
            else
            {
                if (IsStarted)
                {
                    MessageBox.Show("已启动!");
                }
                else
                {
                    serialPort.Open();
                    IsStarted = true;
                }
            }
        }

        public void StartListen()
        {
            if (!IsStarted)
            {
                MessageBox.Show("未启动!");
            }
            else
            {
                if (IsListening)
                {
                    MessageBox.Show("已监听!");
                }
                else
                {
                    receiveQueryTimer.Start();
                    IsListening = true;
                }
            }
        }

        public void Stop()
        {
            serialPort.Close();
        }

        public void StopListen()
        {
            if (IsListening)
            {
                receiveQueryTimer.Stop();
                IsListening = false;
            }
        }

        public int Transmit(RawCanFrame frame)
        {
            if (!IsStarted)
            {
                MessageBox.Show("未启动!");
                return -1;
            }
            else
            {
                Debug.WriteLine("Send: " + frame.ToString());
                lock(channelLock)
                {
                    serialPort.Write(ConverToRs485Frame(frame), 0, 10);
                    return 0;
                }
            }
        }

        public void Uninitialize()
        {
            if (IsInitialized)
            {
                serialPort.Close();
                IsInitialized = false;
            }
        }

        public void UnRegisterClient()
        {
            ClientCount = 0;
            Uninitialize();
        }

        public void UnRegisterHandler(Guid guid)
        {
            if (FrameDispatchTable.ContainsKey(guid))
            {
                FrameDispatchTable.Remove(guid);
            }
        }

        private byte[] ConverToRs485Frame(RawCanFrame frame)
        {
            byte[] buffer = new byte[10];
            buffer[0] = 0x55;
            Array.Copy(frame.data, 0, buffer, 1, 8);
            byte sum = 0;
            for (int i = 0; i < 9; i++)
            {
                sum += buffer[i];
            }
            buffer[9] = sum;
            return buffer;
        }

        public Rs485CanSimDriver()
        {
            receiveQueryTimer = new Timer(30);
            receiveQueryTimer.Elapsed += new ElapsedEventHandler((object sender, ElapsedEventArgs e) => {
                receiveQueryTimer.Stop();
                if (IsListening)
                {
                    RawCanFrame frame;
                    while (true)
                    {
                        int res = Receive(out frame, 0);
                        if (res != 0)
                        {
                            break;
                        }
                        foreach (var dispatcher in FrameDispatchTable.Values)
                        {
                            if (dispatcher.Item1.Match(frame.id))
                            {
                                dispatcher.Item2(this, frame);
                            }
                        }
                    }
                }
                receiveQueryTimer.Start();
            });
        }
    }
}
