//#define NO_DEVICE_TEST

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using Monitor2.CAN;
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
        public static Dictionary<string, PeakCanDriverAdapter> PeakCanDriverInstances { get; set; } = new Dictionary<string, PeakCanDriverAdapter>();

        public static PeakCanDriverAdapter GetChannelDriverInstance(string channel)
        {
            lock (PeakCanDriverInstances)
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
        }

        public event ICanDriver.FrameHandler FrameArriveEvent;

        public Dictionary<string, string> Configs { get; set; }

        public ushort CurrentChannel { get; set; }

        public bool EnableHardwareFilter { get; set; } = true;

        public int ClientCount { get; set; } = 0;

        public bool IsInitialized { get; private set; }
        public bool IsStarted { get; set; }
        public bool IsListening { get; set; }
        public Dictionary<Guid, Tuple<CanFrameFilter, ICanDriver.FrameHandler>> FrameDispatchTable { get; private set; }

        private Timer receiveQueryTimer;

        private TestController testController = new TestController();

        private object channelLock = new object();

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
                lock (channelLock)
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
                lock (channelLock)
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
                TPCANMsg msg = ConvertToPcanMessage(frame);
                lock (channelLock)
                {
                    var res = PCANBasic.Write(CurrentChannel, ref msg);

                    if (res == TPCANStatus.PCAN_ERROR_OK)
                    {
                        return 0;
                    }
                    else
                    {
                        return -1;
                    }
                }
#if NO_DEVICE_TEST
                testController.ReceiveFrame(frame);
#endif
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Receive a CAN frame within specified time.
        /// </summary>
        /// <param name="frame">Received CAN frame</param>
        /// <param name="timeout">Timeout</param>
        /// <returns></returns>
        public int Receive(out RawCanFrame frame, int timeout)
        {
            if (IsStarted)
            {
                lock (channelLock)
                {
                    var res = PCANBasic.Read(CurrentChannel, out TPCANMsg msg);

                    if (res == TPCANStatus.PCAN_ERROR_OK)
                    {
                        frame = ConvertToRawCanFrame(msg);
                        return 0;
                    }
                    else
                    {
                        frame = null;
                        return -1;
                    }
                }
#if NO_DEVICE_TEST
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
#endif
            }
            else
            {
                frame = null;
                return -1;
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
                IsListening = false;
            }
        }

        public Guid RegisterHandler(CanFrameFilter filter, ICanDriver.FrameHandler handler)
        {
            Guid guid = Guid.NewGuid();

            FrameDispatchTable.Add(guid, new Tuple<CanFrameFilter, ICanDriver.FrameHandler>(filter, handler));
#if !NO_DEVICE_TEST
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

        private TPCANMsg ConvertToPcanMessage(RawCanFrame frame)
        {
            TPCANMsg msg = new TPCANMsg();
            msg.MSGTYPE = frame.FrameType switch
            {
                CanFrameType.STD => TPCANMessageType.PCAN_MESSAGE_STANDARD,
                CanFrameType.EXT => TPCANMessageType.PCAN_MESSAGE_EXTENDED,
                _ => TPCANMessageType.PCAN_MESSAGE_STANDARD
            };
            msg.ID = frame.id;
            msg.LEN = 8;
            msg.DATA = frame.data;
            return msg;
        }

        private RawCanFrame ConvertToRawCanFrame(TPCANMsg msg)
        {
            RawCanFrame frame = new RawCanFrame();
            frame.FrameType = msg.MSGTYPE switch
            {
                TPCANMessageType.PCAN_MESSAGE_STANDARD => CanFrameType.STD,
                TPCANMessageType.PCAN_MESSAGE_EXTENDED => CanFrameType.EXT,
                _ => CanFrameType.STD
            };
            frame.id = msg.ID;
            frame.data = msg.DATA;
            return frame;   
        }

        private int inTimer = 0;

        public PeakCanDriverAdapter()
        {
            Configs = new Dictionary<string, string>();
            FrameDispatchTable = new Dictionary<Guid, Tuple<CanFrameFilter, ICanDriver.FrameHandler>>();
            receiveQueryTimer = new Timer(30);
            receiveQueryTimer.Elapsed += new ElapsedEventHandler((object sender, ElapsedEventArgs e) => {
                if (inTimer == 0)
                {
                    System.Threading.Interlocked.Exchange(ref inTimer, 1);
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
                    System.Threading.Interlocked.Exchange(ref inTimer, 0);
                }
            });
#if NO_DEVICE_TEST
            testController.LoadDefault();
#endif
        }
    }

    public class UsbCanDriverAdapter : ICanDriver
    {
        public static Dictionary<Tuple<uint,uint>, UsbCanDriverAdapter> UsbCanInstances { get; set; } = new Dictionary<Tuple<uint, uint>, UsbCanDriverAdapter>();

        public static Dictionary<uint, int> DeviceUseCount { get; set; } = new Dictionary<uint, int>();

        public static UsbCanDriverAdapter GetChannelDriverInstance(Tuple<uint,uint> ids)
        {
            lock (UsbCanInstances)
            {
                if (UsbCanInstances.ContainsKey(ids))
                {
                    return UsbCanInstances[ids];
                }
                else
                {
                    UsbCanDriverAdapter tmp = new UsbCanDriverAdapter();
                    tmp.deviceId = ids.Item1;
                    tmp.channelId = ids.Item2;
                    tmp.Configs.Add("DeviceId", ids.Item1.ToString());
                    tmp.Configs.Add("ChannelId", ids.Item2.ToString());
                    DeviceUseCount.Add(ids.Item1, 0);
                    return tmp;
                }
            }
        }

        public Dictionary<string, string> Configs { get; set; }
        public bool EnableHardwareFilter { get; set; } = true;
        public int ClientCount { get; set; }

        public bool IsInitialized { get; private set; }

        public bool IsStarted { get; set; }
        public bool IsListening { get; set; }

        public Dictionary<Guid, Tuple<CanFrameFilter, ICanDriver.FrameHandler>> FrameDispatchTable { get; set; }

        public event ICanDriver.FrameHandler FrameArriveEvent;

        private const UInt32 deviceType = ControlCAN.DEV_USBCAN2;
        private uint deviceId;
        private uint channelId;

        private UInt32 filterCode = 0x00000000;
        private UInt32 filterMask = 0x00000000;

        private ControlCAN.VCI_CAN_OBJ[] receiveBuffer = new ControlCAN.VCI_CAN_OBJ[2500];
        private Queue<RawCanFrame> ReceiveQueue { get; set; }
        private Timer receiveQueryTimer;

        private static Dictionary<string, byte[]> baudrateDic = new Dictionary<string, byte[]>
        {
                { "10 kbps", new byte[] { 0x9f, 0xff } },
                { "20 kbps", new byte[] { 0x18, 0x1c } },
                { "40 kbps", new byte[] { 0x87, 0xff } },
                { "50 kbps", new byte[] { 0x09, 0x1c } },
                { "80 kbps", new byte[] { 0x83, 0xff } },
                { "100 kbps", new byte[] { 0x04, 0x1c } },
                { "124 kbps", new byte[] { 0x03, 0x1c } },
                { "200 kbps", new byte[] { 0x81, 0xfa } },
                { "250 kbps", new byte[] { 0x01, 0x1c } },
                { "400 kbps", new byte[] { 0x80, 0xfa } },
                { "500 kbps", new byte[] { 0x00, 0x1c } },
                { "666 kbps", new byte[] { 0x80, 0xb6 } },
                { "800 kbps", new byte[] { 0x00, 0x16 } },
                { "1000 kbps", new byte[] { 0x00, 0x14 } },
                { "33.33 kbps", new byte[] { 0x09, 0x6f } },
                { "66.6 kbps", new byte[] { 0x04, 0x6f } },
                { "83.33 kbps", new byte[] { 0x03, 0x6f } }
        };

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

        public void Initialize()
        {
            if (!IsInitialized)
            {
                if (!DeviceUseCount.ContainsKey(deviceId))
                {
                    DeviceUseCount.Add(deviceId, 0);
                }
                var useCount = DeviceUseCount[deviceId];
                if (useCount == 0)
                {
                    var res = ControlCAN.VCI_OpenDevice(deviceType, deviceId, 0);
                    if (res != 0)
                    {

                    }
                    else
                    {
                        MessageBox.Show("设备连接错误");
                        return;
                    }
                }

                var baudRateConfig = baudrateDic["Baudrate"];
                ControlCAN.VCI_INIT_CONFIG config = new ControlCAN.VCI_INIT_CONFIG();
                config.AccCode = 0;
                if (EnableHardwareFilter)
                {
                    config.AccMask = 0;
                }
                else
                {
                    config.AccMask = 0xffffffff;
                }
                config.Filter = 2;
                config.Mode = (Byte)0;
                config.Timing0 = baudRateConfig[0];
                config.Timing1 = baudRateConfig[1];
                var res2 = ControlCAN.VCI_InitCAN(deviceType, deviceId, channelId, ref config);
                if (res2 != 1)
                {
                    MessageBox.Show("设备初始化错误");
                    return;
                }
                DeviceUseCount[deviceId] += 1;
                IsInitialized = true;
            }
        }

        public void Uninitialize()
        {
            if (IsInitialized)
            {
                var res = ControlCAN.VCI_ResetCAN(deviceType, deviceId, channelId);
                DeviceUseCount[deviceId] -= 1;
                filterCode = 0;
                filterMask = 0;
                if (DeviceUseCount[deviceId] <= 0)
                {
                    DeviceUseCount[deviceId] = 0;
                    ControlCAN.VCI_CloseDevice(deviceType, deviceId);
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

        public Guid RegisterHandler(CanFrameFilter filter, ICanDriver.FrameHandler handler)
        {
            Guid guid = Guid.NewGuid();

            FrameDispatchTable.Add(guid, new Tuple<CanFrameFilter, ICanDriver.FrameHandler>(filter, handler));
#if !NO_DEVICE_TEST
            if (EnableHardwareFilter)
            {
                UpdateFilter(filter.FromId, filter.ToId);
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

        public void Start()
        {
            if (IsInitialized)
            {

            }
        }

        public void StartListen()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void StopListen()
        {
            throw new NotImplementedException();
        }

        public int Transmit(RawCanFrame frame)
        {
            throw new NotImplementedException();
        }

        private void ReceiveToBuffer()
        {
            if (IsStarted)
            {
                var res = ControlCAN.VCI_GetReceiveNum(deviceType, deviceId, channelId);
                if (res > 0)
                {
                    var objCount = res;
                    res = ControlCAN.VCI_Receive(deviceType, deviceId, channelId, ref this.receiveBuffer[0], 2500, 10);
                    lock (ReceiveQueue)
                    {
                        for (int i = 0; i < ControlCAN.recobj_count; i++)
                        {
                            ReceiveQueue.Enqueue(ConvertToRawFrame(receiveBuffer[i]));
                        }
                    }
                }
            }
        }

        public int Receive(out RawCanFrame frame, int timeout)
        {
            if (IsStarted)
            {
                if (ReceiveQueue.Count > 0)
                {
                    lock(ReceiveQueue)
                    {
                        frame = ReceiveQueue.Dequeue();
                        return 0;
                    }
                }
                else
                {
                    var res = ControlCAN.VCI_GetReceiveNum(deviceType, deviceId, channelId);
                    if (res > 0)
                    {
                        var objCount = res;
                        res = ControlCAN.VCI_Receive(deviceType, deviceId, channelId, ref this.receiveBuffer[0], 2500, 20);
                        lock (ReceiveQueue)
                        {
                            for (int i = 0; i < ControlCAN.recobj_count; i++)
                            {
                                ReceiveQueue.Enqueue(ConvertToRawFrame(receiveBuffer[i]));
                            }
                        }
                    }
                    if (ReceiveQueue.Count > 0)
                    {
                        lock (ReceiveQueue)
                        {
                            frame = ReceiveQueue.Dequeue();
                            return 0;
                        }
                    }
                    else
                    {
                        frame = null;
                        return -1;
                    }
                }
            }
            else
            {
                frame = null;
                return -1;
            }
        }

        private unsafe RawCanFrame ConvertToRawFrame(ControlCAN.VCI_CAN_OBJ oBJ)
        {
            RawCanFrame frame = new RawCanFrame();
            frame.id = oBJ.ID;
            frame.FrameType = oBJ.ExternFlag switch
            {
                1 => CanFrameType.EXT,
                0 => CanFrameType.STD,
                _ => CanFrameType.STD
            };
            for (int i = 0; i < 8; i++)
            {
                frame.data[i] = oBJ.Data[i];
            }

            return frame;
        }

        private void UpdateFilter(UInt32 Id)
        {
            UInt32 laId = Id << 21;
            if (filterCode == 0)
            {
                filterCode = laId;
            }
            else
            {
                filterMask |= (laId ^ filterCode);
            }

            var codeByte = BitConverter.GetBytes(filterCode);
            byte[] codeByteRevert = new byte[4];
            for (int i=0; i < 4; i++)
            {
                codeByteRevert[i] = codeByte[3 - i];
            }

            var maskByte = BitConverter.GetBytes(filterMask);
            byte[] maskByteRevert = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                maskByteRevert[i] = maskByte[3 - i];
            }

            ControlCAN.VCI_SetReference2(deviceType, deviceId, channelId, 2, ref codeByte[0]);
            ControlCAN.VCI_SetReference2(deviceType, deviceId, channelId, 6, ref maskByte[0]);
        }

        private void UpdateFilter(UInt32 fromId, UInt32 toId)
        {
            for (UInt32 i = fromId; i <= toId; i++)
            {
                UInt32 laId = i << 21;
                if (filterCode == 0)
                {
                    filterCode = laId;
                    filterMask = 0;
                }
                else
                {
                    filterMask |= (laId ^ filterCode);
                }
            }

            var codeByte = BitConverter.GetBytes(filterCode);
            byte[] codeByteRevert = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                codeByteRevert[i] = codeByte[3 - i];
            }

            var maskByte = BitConverter.GetBytes(filterMask);
            byte[] maskByteRevert = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                maskByteRevert[i] = maskByte[3 - i];
            }

            ControlCAN.VCI_SetReference2(deviceType, deviceId, channelId, 2, ref codeByte[0]);
            ControlCAN.VCI_SetReference2(deviceType, deviceId, channelId, 6, ref maskByte[0]);
        }

        public UsbCanDriverAdapter()
        {
            Configs = new Dictionary<string, string>();
            FrameDispatchTable = new Dictionary<Guid, Tuple<CanFrameFilter, ICanDriver.FrameHandler>>();
            ReceiveQueue = new Queue<RawCanFrame>();
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
