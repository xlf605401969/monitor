#define NO_DEVICE_TEST

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using Monitor2ng.TestMcu;
using Peak.Can.Basic;

namespace Monitor2ng.CAN
{
    public class RawCanFrame
    {
        public UInt32 id;
        public CanFrameType FrameType = CanFrameType.STD;
        public byte[] data = new byte[8];

        public override string ToString()
        {
            string s = "";
            foreach (var d in data)
            {
                s += string.Format("{0:X2}-", d);
            }
            var s2 = s.Remove(s.Length - 1);
            return String.Format("{0:X4}:[{1}]", id, s2);
        }
    }

    public enum CanFrameType
    {
        STD,
        EXT
    }

    public class CanFrameFilter
    {
        public UInt32 FromId = 0x0000;
        public UInt32 ToId = 0x03FFF;

        public bool Match(UInt32 id)
        {
            if (id >= FromId && id <= ToId)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public CanFrameFilter(UInt32 fromId, UInt32 toId)
        {
            FromId = fromId;
            ToId = toId;
        }
    }

    public interface ICanDriver
    {
        public Dictionary<string, string> Configs { get; set; }

        public delegate void FrameHandler(object sender, RawCanFrame frame);

        public event FrameHandler FrameArriveEvent;

        public bool EnableHardwareFilter { get; set; }

        public int ClientCount { get; set; }

        public bool IsInitialized { get; }

        public bool IsStarted { get; set; }

        public bool IsListening { get; set; }

        public Dictionary<Guid,Tuple<CanFrameFilter,FrameHandler>> FrameDispatchTable { get; }

        public void EditConfig(string key, string value);
        public void Initialize();
        public void Uninitialize();
        public void Start();
        public void StartListen();
        public void Stop();
        public void StopListen();
        public int Transmit(RawCanFrame frame);
        public int Receive(out RawCanFrame frame, int timeout);
        public Guid RegisterHandler(CanFrameFilter filter, FrameHandler handler);
        public void UnRegisterHandler(Guid guid);
        public void RegisterClient();
        public void UnRegisterClient();
    }

    public class PeakCanDriverAdapter : ICanDriver
    {
        public static Dictionary<string, PeakCanDriverAdapter> PeakCanDriverInstances = new Dictionary<string, PeakCanDriverAdapter>();

        public static PeakCanDriverAdapter GetChannelDriverInstance(string channel)
        {
            if (PeakCanDriverInstances.ContainsKey(channel))
            {
                return PeakCanDriverInstances[channel];
            }
            else
            {
                PeakCanDriverAdapter driver = new PeakCanDriverAdapter();
                driver.EditConfig("Channel", channel);
                PeakCanDriverInstances.Add(channel, driver);
                return driver;
            }
        }

        public event ICanDriver.FrameHandler FrameArriveEvent;

        public Dictionary<string, string> Configs { get; set; } = new Dictionary<string, string>();

        public ushort CurrentChannel { get; set; }

        public bool EnableHardwareFilter { get; set; } = true;

        public int ClientCount { get; set; } = 0;

        public bool IsInitialized { get; private set; }
        public bool IsStarted { get; set; }
        public bool IsListening { get; set; }
        public Dictionary<Guid, Tuple<CanFrameFilter, ICanDriver.FrameHandler>> FrameDispatchTable { get; private set; }

        private Timer receiveQueryTimer;

        private TestController testController = new TestController();

        public void Initialize()
        {
            TPCANBaudrate rate = Configs["Baudrate"] switch
            {
                "100 kbps" => TPCANBaudrate.PCAN_BAUD_100K,
                "250 kbps" => TPCANBaudrate.PCAN_BAUD_250K,
                "500 kbps" => TPCANBaudrate.PCAN_BAUD_500K,
                "1 Mbps" => TPCANBaudrate.PCAN_BAUD_100K,
                _ => TPCANBaudrate.PCAN_BAUD_500K,
            };

            var channel = Configs["Channel"] switch
            {
                "PCAN_USBBUS1" => PCANBasic.PCAN_USBBUS1,
                "PCAN_USBBUS2" => PCANBasic.PCAN_USBBUS2,
                _ => PCANBasic.PCAN_USBBUS1,
            };

            CurrentChannel = channel;

            if (!IsInitialized)
            {
#if (!NO_DEVICE_TEST)
                var res = PCANBasic.Initialize(channel, rate);
                if (res != TPCANStatus.PCAN_ERROR_OK)
                {
                    MessageBox.Show("打开设备失败，Err Code:" + res.ToString(), "错误", MessageBoxButton.OK);
                    return;
                }

                UInt32 buff = PCANBasic.PCAN_PARAMETER_ON;
                PCANBasic.SetValue(channel, TPCANParameter.PCAN_BUSOFF_AUTORESET, ref buff, sizeof(UInt32));
                buff = PCANBasic.PCAN_FILTER_CLOSE;
                PCANBasic.SetValue(channel, TPCANParameter.PCAN_MESSAGE_FILTER, ref buff, sizeof(UInt32));
#endif
                IsInitialized = true;
            }
        }

        public void Uninitialize()
        {
            var channel = Configs["Channel"] switch
            {
                "PCAN_USBBUS1" => PCANBasic.PCAN_USBBUS1,
                "PCAN_USBBUS2" => PCANBasic.PCAN_USBBUS2,
                _ => PCANBasic.PCAN_USBBUS1,
            };

            if (IsInitialized)
            {
                StopListen();
                Stop();
#if (!NO_DEVICE_TEST)
                var res = PCANBasic.Uninitialize(channel);
                if (res != TPCANStatus.PCAN_ERROR_OK)
                {
                    MessageBox.Show("关闭设备失败，Err Code:" + res.ToString(), "错误", MessageBoxButton.OK);
                    return;
                }
#endif
                IsInitialized = false;
            }
        }

        public void Start()
        {
            if (IsInitialized)
            {
                IsStarted = true;
            }
        }

        public void Stop()
        {
            StopListen();
            IsStarted = false;
        }

        public int Transmit(RawCanFrame frame)
        {
            if (IsStarted)
            {
                Debug.WriteLine("Send: " + frame.ToString());
                testController.ReceiveFrame(frame);
            }
            return 0;
        }

        /// <summary>
        /// Receive a CAN frame within specified time.
        /// </summary>
        /// <param name="frame">Received CAN frame</param>
        /// <param name="timeout">Timeout</param>
        /// <returns></returns>
        public int Receive(out RawCanFrame frame, int timeout)
        {
            lock (testController.SendQueue)
            {
                if (testController.SendQueue.Count > 0)
                {
                    RawCanFrame tmp = testController.SendQueue.Dequeue();
                    tmp.id = testController.CommunicationId;
                    frame = tmp;
                    return 0;
                }
                else
                {
                    frame = null;
                    return -1;
                }
            }
        }

        public void StartListen()
        {
            if (IsStarted)
            {
                if (!IsListening)
                {
                    receiveQueryTimer.Start();
                    IsListening = true;
                }
            }
        }

        public void StopListen()
        {
            if (IsListening)
            {
                receiveQueryTimer.Stop();
                IsListening = true;
            }
        }

        public Guid RegisterHandler(CanFrameFilter filter, ICanDriver.FrameHandler handler)
        {
            Guid guid = Guid.NewGuid();

            FrameDispatchTable.Add(guid, new Tuple<CanFrameFilter, ICanDriver.FrameHandler>(filter, handler));
#if (!NO_DEVICE_TEST)
            if (EnableHardwareFilter)
            {
                PCANBasic.FilterMessages(CurrentChannel, filter.FromId, filter.ToId, TPCANMode.PCAN_MODE_STANDARD);
            }
#endif
            return guid;
        }

        public void UnRegisterHandler(Guid guid)
        {
            if (FrameDispatchTable.ContainsKey(guid))
            {
                FrameDispatchTable.Remove(guid);
            }
        }

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
                        MessageBox.Show("PeakCAN运行时无法修改通道设置!");
                    }    
                }
            }
        }

        public void RegisterClient()
        {
            ClientCount += 1;
            if (ClientCount == 1)
            {
                Initialize();
            }
        }

        public void UnRegisterClient()
        {
            ClientCount -= 1;
            if (ClientCount == 0)
            {
                Uninitialize();
            }
        }

        public PeakCanDriverAdapter()
        {
            FrameDispatchTable = new Dictionary<Guid, Tuple<CanFrameFilter, ICanDriver.FrameHandler>>();
            receiveQueryTimer = new Timer(30);
            receiveQueryTimer.Elapsed += new ElapsedEventHandler((object sender, ElapsedEventArgs e) => {
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
            });

            testController.LoadDefault();
        }
    }

    public class CanClient
    {
        public ICanDriver targetDriver;

        public bool IsStarted = false;

        public UInt32 DefaultId = 0x100;

        public Dictionary<Guid, CanFrameFilter> Filters { get; set; } = new Dictionary<Guid, CanFrameFilter>();

        public Action<RawCanFrame> SendHook { get; set; }

        public void Connect(ICanDriver driver)
        {
            targetDriver = driver;
            driver.RegisterClient();
        }

        public void DisConnect()
        {
            Stop();
            foreach (var uid in Filters.Keys)
            {
                targetDriver.UnRegisterHandler(uid);
            }
            targetDriver.UnRegisterClient();
        }

        public delegate void ClientFrameHandler(RawCanFrame frame);

        public void Register(UInt32 id, ClientFrameHandler handler)
        {
            CanFrameFilter filter = new CanFrameFilter(id, id);
            var guid = targetDriver.RegisterHandler(filter, (object sender, RawCanFrame frame) => {
                if (IsStarted)
                {
                    handler(frame);
                }
            });
            Filters.Add(guid, filter);
        }

        public void Transmit(RawCanFrame frame)
        {
            if (IsStarted)
            {
                SendHook?.Invoke(frame);
                targetDriver.Transmit(frame);
            }
        }

        public int Transmit(RawCanFrame frame, uint frameId)
        {
            if (IsStarted)
            {
                frame.id = frameId;
                SendHook?.Invoke(frame);
                var res = targetDriver.Transmit(frame);
                if (res != 0)
                {
                    return -1;
                }
            }
            return 0;
        }

        public void Start()
        {
            IsStarted = true;
            targetDriver.Start();
            targetDriver.StartListen();
        }

        public void Stop()
        {
            IsStarted = false;
        }
    }
}
