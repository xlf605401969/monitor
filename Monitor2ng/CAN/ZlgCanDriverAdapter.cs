using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using ZLGCAN;

namespace Monitor2ng.CAN
{
    public class ZlgCanDriverAdapter : ICanDriver
    {
        public static Dictionary<Tuple<uint, uint>, ZlgCanDriverAdapter> ZlgCanInstances { get; set; } = new Dictionary<Tuple<uint, uint>, ZlgCanDriverAdapter>();
        public static Dictionary<uint, int> DeviceUseCount { get; set; } = new Dictionary<uint, int>();

        private static Dictionary<uint, object> deviceLocks = new Dictionary<uint, object>();
        public static ZlgCanDriverAdapter GetChannelDriverInstance(Tuple<uint, uint> ids)
        {
            lock (ZlgCanInstances)
            {
                if (ZlgCanInstances.ContainsKey(ids))
                {
                    return ZlgCanInstances[ids];
                }
                else
                {
                    ZlgCanDriverAdapter tmp = new ZlgCanDriverAdapter();
                    tmp.deviceId = (uint)ids.Item1;
                    tmp.channelId = (uint)ids.Item2;
                    tmp.Configs.Add("DeviceId", ids.Item1.ToString());
                    tmp.Configs.Add("ChannelId", ids.Item2.ToString());
                    ZlgCanInstances.Add(ids, tmp);
                    DeviceUseCount.Add(ids.Item1, 0);
                    deviceLocks.Add(ids.Item1, new object());
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

        private const UInt32 deviceTypeIndex = 1;
        private uint deviceId;
        private uint channelId;

        IntPtr device_handle;
        IntPtr channel_handle;

        private UInt32 filterCode = 0x00000000;
        private UInt32 filterMask = 0x00000000;

        private ZLGCAN.ZCAN_Receive_Data[] receiveBuffer = new ZLGCAN.ZCAN_Receive_Data[2500];
        private Queue<RawCanFrame> ReceiveQueue { get; set; }
        private Timer receiveQueryTimer;

        ZLGCAN.DeviceInfo[] DeviceType =
        {
            new ZLGCAN.DeviceInfo(ZLGCAN.Define.ZCAN_USBCAN1, 1),
            new ZLGCAN.DeviceInfo(ZLGCAN.Define.ZCAN_USBCAN2, 2),
            new ZLGCAN.DeviceInfo(ZLGCAN.Define.ZCAN_USBCAN_E_U, 1),
            new ZLGCAN.DeviceInfo(ZLGCAN.Define.ZCAN_USBCAN_2E_U, 2),
            new ZLGCAN.DeviceInfo(ZLGCAN.Define.ZCAN_PCIECANFD_100U, 1),
            new ZLGCAN.DeviceInfo(ZLGCAN.Define.ZCAN_PCIECANFD_200U, 2),
            new ZLGCAN.DeviceInfo(ZLGCAN.Define.ZCAN_PCIECANFD_200U_EX,2),
            new ZLGCAN.DeviceInfo(ZLGCAN.Define.ZCAN_PCIECANFD_400U, 4),
            new ZLGCAN.DeviceInfo(ZLGCAN.Define.ZCAN_USBCANFD_200U, 2),
            new ZLGCAN.DeviceInfo(ZLGCAN.Define.ZCAN_USBCANFD_100U, 1),
            new ZLGCAN.DeviceInfo(ZLGCAN.Define.ZCAN_USBCANFD_MINI, 1),
            new ZLGCAN.DeviceInfo(ZLGCAN.Define.ZCAN_CANETTCP, 1),
            new ZLGCAN.DeviceInfo(ZLGCAN.Define.ZCAN_CANETUDP, 1),
            new ZLGCAN.DeviceInfo(ZLGCAN.Define.ZCAN_CANFDNET_200U_TCP, 2),
            new ZLGCAN.DeviceInfo(ZLGCAN.Define.ZCAN_CANFDNET_200U_UDP, 2),
            new ZLGCAN.DeviceInfo(ZLGCAN.Define.ZCAN_CANFDNET_400U_TCP, 4),
            new ZLGCAN.DeviceInfo(ZLGCAN.Define.ZCAN_CANFDNET_400U_UDP, 4),
            new ZLGCAN.DeviceInfo(ZLGCAN.Define.ZCAN_CANFDNET_800U_TCP, 8),
            new ZLGCAN.DeviceInfo(ZLGCAN.Define.ZCAN_CANFDNET_800U_UDP, 8),
            new ZLGCAN.DeviceInfo(ZLGCAN.Define.ZCAN_CLOUD, 1)
        };

        private static Dictionary<string, uint> baudrateDic = new Dictionary<string, uint>
        {
                { "10 kbps", 10000},
                { "20 kbps", 20000},
                { "50 kbps", 50000},
                { "100 kbps", 100000},
                { "125 kbps", 125000},
                { "250 kbps", 250000},
                { "500 kbps", 500000},
                { "800 kbps", 800000},
                { "1000 kbps", 1000000},
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
                        MessageBox.Show("UsbCAN运行时无法修改通道设置!");
                    }
                }
            }
        }

        public void Initialize()
        {
            if (!IsInitialized)
            {
                lock (deviceLocks[deviceId])
                {
                    if (!DeviceUseCount.ContainsKey(deviceId))
                    {
                        DeviceUseCount.Add(deviceId, 0);
                    }
                    var useCount = DeviceUseCount[deviceId];
                    if (useCount == 0)
                    {
                        device_handle = ZLGCAN.Method.ZCAN_OpenDevice(DeviceType[deviceTypeIndex].device_type, deviceId, 0);
                        if (0 == (int)device_handle)
                        {
                            MessageBox.Show("设备连接错误");
                            return;
                        }

                        var baudRateConfig = baudrateDic[Configs["Baudrate"]];

                        if (!setBaudrate(baudRateConfig))
                        {
                            MessageBox.Show("设置波特率失败", "提示");
                            return;
                        }

                        ZCAN_CHANNEL_INIT_CONFIG channel_config = new ZCAN_CHANNEL_INIT_CONFIG();

                        channel_config.can_type = Define.TYPE_CAN;
                        channel_config.can.acc_code = 0;
                        channel_config.can.acc_mask = 0xffffffff;
                        channel_config.can.filter = 0;
                        channel_config.can.mode = 0;

                        IntPtr p_channel_config = Marshal.AllocHGlobal(Marshal.SizeOf(channel_config));
                        Marshal.StructureToPtr(channel_config, p_channel_config, true);

                        channel_handle = ZLGCAN.Method.ZCAN_InitCAN(device_handle, channelId, p_channel_config);

                        if (0 == (int)channel_handle)
                        {
                            MessageBox.Show("通道初始化错误");
                            return;
                        }

                        DeviceUseCount[deviceId] += 1;
                        IsInitialized = true;
                    }
                }
            }
        }

        public void Uninitialize()
        {
            if (IsInitialized)
            {
                lock (deviceLocks[deviceId])
                {
#if !NO_DEVICE_TEST
                    string path = channelId + "/clear_auto_send";
                    string value = "0";
                    uint result = Method.ZCAN_SetValue(device_handle, path, Encoding.ASCII.GetBytes(value));
                    Method.ZCAN_ResetCAN(channel_handle);
#endif
                    DeviceUseCount[deviceId] -= 1;
                    filterCode = 0;
                    filterMask = 0;
                    if (DeviceUseCount[deviceId] <= 0)
                    {
                        DeviceUseCount[deviceId] = 0;
#if !NO_DEVICE_TEST
                        Method.ZCAN_CloseDevice(device_handle);
#endif
                    }
                    IsInitialized = false;
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
            lock (deviceLocks[deviceId])
            {
                if (EnableHardwareFilter)
                {
                    setFilter(filter.FromId, filter.ToId);
                }
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
            if (IsInitialized && !IsStarted)
            {
                lock (deviceLocks[deviceId])
                {
#if !NO_DEVICE_TEST
                    if (Method.ZCAN_StartCAN(channel_handle) != Define.STATUS_OK)
                    {
                        MessageBox.Show("启动CAN失败", "提示");
                        return;
                    }
#endif
                    IsStarted = true;
                }
            }
        }

        public void Stop()
        {
            if (IsInitialized && IsStarted)
            {
                lock (deviceLocks[deviceId])
                {
#if !NO_DEVICE_TEST
                    if (Method.ZCAN_ResetCAN(channel_handle) != Define.STATUS_OK)
                    {
                        MessageBox.Show("复位失败", "提示");
                        return;
                    }
#endif
                    IsStarted = false;
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
                IsListening = false;
            }
        }

        public int Transmit(RawCanFrame frame)
        {
            if (IsStarted)
            {
                uint result;

                Debug.WriteLine("Send: " + frame.ToString());
                ZCAN_Transmit_Data can_data = new ZCAN_Transmit_Data();
                can_data = ConverToZlgCanData(frame);
#if !NO_DEVICE_TEST
                lock (deviceLocks[deviceId])
                {

                    IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(can_data));
                    Marshal.StructureToPtr(can_data, ptr, true);
                    result = Method.ZCAN_Transmit(channel_handle, ptr, 1);
                    Marshal.FreeHGlobal(ptr);
                    if (result == 1)
                    {
                        return 0;
                    }
                    else
                    {
                        return -1;
                    }
                }
#else
                testController.ReceiveFrame(frame);
                return -1;
#endif
            }
            else
            {
                return -1;
            }
        }

        private void ReceiveToBuffer()
        {
            if (IsStarted)
            {
                lock (deviceLocks[deviceId])
                {
#if !NO_DEVICE_TEST
                    var len = Method.ZCAN_GetReceiveNum(channel_handle, 0); //0:type can 1:type canfd
                    if (len > 0)
                    {
                        var objCount = len;
                        int size = Marshal.SizeOf(typeof(ZCAN_Receive_Data));
                        IntPtr ptr = Marshal.AllocHGlobal((int)objCount * size);
                        len = Method.ZCAN_Receive(channel_handle, ptr, len, 50);
                        lock (ReceiveQueue)
                        {
                            for (int i = 0; i < len; i++)
                            {
                                ReceiveQueue.Enqueue(ConvertToRawFrame((ZCAN_Receive_Data)Marshal.PtrToStructure(
                                (IntPtr)((Int64)ptr + i * size), typeof(ZCAN_Receive_Data))));
                            }
                        }
                    }
#else
                    lock (testController.SendQueue)
                    {
                        lock (ReceiveQueue)
                        {
                            while (testController.SendQueue.Count > 0)
                            {
                                RawCanFrame tmp = testController.SendQueue.Dequeue();
                                tmp.id = testController.CommunicationId;
                                ReceiveQueue.Enqueue(tmp);
                            }
                        }
                    }
#endif
                }
            }
        }

        public int Receive(out RawCanFrame frame, int timeout)
        {
            if (IsStarted)
            {
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
                    ReceiveToBuffer();
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

        private ZCAN_Transmit_Data ConverToZlgCanData(RawCanFrame frame)
        {
            ZCAN_Transmit_Data data = new ZCAN_Transmit_Data();
            {
                data.frame.can_id = frame.id;
                data.frame.can_id = MakeCanId(frame.id, 0, 0, 0);   //1:extend frame 0:standard frame
                data.frame.can_dlc = 8;
                data.transmit_type = 0;                             //0:正常发送 1:单次发送 2:自发自收 3:单次自发自收
                data.frame.data = new byte[8];
                
            };

            for (int i = 0; i < 8; i++)
            {
                data.frame.data[i] = frame.data[i];
            }

            return data;
        }

        private unsafe RawCanFrame ConvertToRawFrame(ZCAN_Receive_Data data)
        {
            RawCanFrame frame = new RawCanFrame();
            frame.id = data.frame.can_id & 0x1fffffff;
            frame.FrameType = ((data.frame.can_id & 0x80000000) >> 31) switch
            {
                1 => CanFrameType.EXT,
                0 => CanFrameType.STD,
                _ => CanFrameType.STD
            };
            for (int i = 0; i < 8; i++)
            {
                frame.data[i] = data.frame.data[i];
            }

            return frame;
        }

        public uint MakeCanId(uint id, int eff, int rtr, int err)//1:extend frame 0:standard frame
        {
            uint ueff = (uint)(!!(Convert.ToBoolean(eff)) ? 1 : 0);
            uint urtr = (uint)(!!(Convert.ToBoolean(rtr)) ? 1 : 0);
            uint uerr = (uint)(!!(Convert.ToBoolean(err)) ? 1 : 0);
            return id | ueff << 31 | urtr << 30 | uerr << 29;
        }

        private bool setBaudrate(UInt32 baud)
        {
            string path = channelId + "/baud_rate";
            string value = baud.ToString();
            //char* pathCh = (char*)System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(path).ToPointer();
            //char* valueCh = (char*)System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(value).ToPointer();
            return 1 == ZLGCAN.Method.ZCAN_SetValue(device_handle, path, Encoding.ASCII.GetBytes(value));
        }

        private bool setFilter(uint fromId, uint toId)
        {
            ////如果要设置多条滤波，在清除滤波和滤波生效之间设置多条滤波即可
            return true;
        }

        public ZlgCanDriverAdapter()
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
#if NO_DEVICE_TEST
            testController.LoadDefault();
#endif
        }
    }
}
