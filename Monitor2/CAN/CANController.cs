using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Timers;
using System.Threading;
using System.Windows;
using Peak.Can.Basic;

namespace Monitor2.CAN
{
    public static class CANController
    {
        public static System.Timers.Timer ReceiveTimer;

        public static event EventHandler OnMessageArrive;

        public static Mutex HardwareMutex;

        public static Queue<CANFrame> RecvFrames;

        public static bool Started = false;

        public static bool Initialized = false;

        public static CanDevice TargetDevice = CanDevice.CANUSB2;

        public static uint DeviceId = 0;

        public static uint ChannelId = 0;

        public static string FilterId = "0x000000000";

        public static string FilterMask = "0xffffffff";

        public static string Baudrate = "500Kbps";

        public static string MessageId = "0x00000100";

        public static CanMode TargetMode;
        public static int Initialize()
        {
            ConvertSettings();
            CANController.HardwareMutex.WaitOne();
            int err = 0;
            if (TargetDevice == CanDevice.CANUSB2 || TargetDevice == CanDevice.CANUSB)
            {
                if (ControlCAN.m_bOpen == 0)
                {
                    if (ControlCAN.VCI_OpenDevice(ControlCAN.m_devtype, ControlCAN.m_devind, 0) != 0)
                    {
                        ControlCAN.m_bOpen = 1;
                    }
                    else
                    {
                        err = -1;
                        MessageBox.Show("打开设备失败，请检查设备类型及索引号是否正确", "错误",
                            MessageBoxButton.OK);
                    }
                }
            }
            else if (TargetDevice == CanDevice.PCAN)
            {
                TPCANBaudrate rate;
                if (Baudrate == "250Kbps")
                {
                    rate = TPCANBaudrate.PCAN_BAUD_250K;
                }
                else
                {
                    rate = TPCANBaudrate.PCAN_BAUD_500K;
                }
                var res = PCANBasic.Initialize(PCANBasic.PCAN_USBBUS1, rate);
                if (res != TPCANStatus.PCAN_ERROR_OK)
                {
                    err = -1;
                    PCANBasic.Uninitialize(PCANBasic.PCAN_USBBUS1);
                    MessageBox.Show("打开设备失败，Err Code:" + res.ToString(), "错误",
                            MessageBoxButton.OK);
                }
            }
            CANController.HardwareMutex.ReleaseMutex();
            return err;
        }
        public static int Close()
        {
            ConvertSettings();
            CANController.HardwareMutex.WaitOne();
            if (TargetDevice == CanDevice.CANUSB2)
            {
                if (ControlCAN.m_bOpen == 1)
                {
                    ControlCAN.VCI_CloseDevice(ControlCAN.m_devtype, ControlCAN.m_devind);
                    ControlCAN.m_bOpen = 0;
                }
            }
            else if (TargetDevice == CanDevice.PCAN)
            {
                PCANBasic.Uninitialize(PCANBasic.PCAN_USBBUS1);
            }
            CANController.HardwareMutex.ReleaseMutex();
            Initialized = false;
            return 0;
        }
        public static int Configuration()
        {
            ConvertSettings();
            CANController.HardwareMutex.WaitOne();
            int err = 0;
            if (TargetDevice == CanDevice.CANUSB || TargetDevice == CanDevice.CANUSB2)
            {
                if (ControlCAN.m_bOpen == 1)
                {
                    ControlCAN.VCI_INIT_CONFIG config = new ControlCAN.VCI_INIT_CONFIG();
                    try
                    {
                        config.AccCode = System.Convert.ToUInt32(FilterId, 16);
                        config.AccMask = System.Convert.ToUInt32(FilterMask, 16);
                        config.Filter = (Byte)1;
                        config.Mode = (Byte)TargetMode;
                        config.Timing0 = ControlCAN.BaudrateDic[Baudrate][0];
                        config.Timing1 = ControlCAN.BaudrateDic[Baudrate][1];
                        ControlCAN.VCI_InitCAN(ControlCAN.m_devtype, ControlCAN.m_devind, ControlCAN.m_canind, ref config);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("初始化设备失败,请检参数是否正确", "错误",
                                MessageBoxButton.OK);
                        ControlCAN.VCI_CloseDevice(ControlCAN.m_devtype, ControlCAN.m_devind);
                        ControlCAN.m_bOpen = 0;
                        err = -1;
                    }
                }
                else
                {
                    err = -1;
                }
            }
            else if (TargetDevice == CanDevice.PCAN)
            {
                UInt64 iBuff;
                iBuff = (System.Convert.ToUInt32(FilterId, 16) << 32) | System.Convert.ToUInt32(FilterMask, 16);
                var res = PCANBasic.SetValue(PCANBasic.PCAN_USBBUS1, TPCANParameter.PCAN_ACCEPTANCE_FILTER_29BIT, ref iBuff, sizeof(UInt64));
                if (res != TPCANStatus.PCAN_ERROR_OK)
                {
                    err = -1;
                    MessageBox.Show("配置设备失败，Err Code:" + res.ToString(), "错误",
                            MessageBoxButton.OK);
                }
            }
            CANController.HardwareMutex.ReleaseMutex();
            if (err == 0)
            {
                Initialized = true;
            }
            else
            {
                Initialized = false;
            }
            return err;
        }
        public static int Start()
        {
            ConvertSettings();
            CANController.HardwareMutex.WaitOne();
            int err = 0;
            if (TargetDevice == CanDevice.CANUSB || TargetDevice == CanDevice.CANUSB2)
            {
                if (Initialized)
                {
                    ControlCAN.VCI_StartCAN(ControlCAN.m_devtype, ControlCAN.m_devind, ControlCAN.m_canind);
                    ControlCAN.m_canstart = 1;
                }
                else
                {
                    err = -1;
                }
            }
            else if (TargetDevice == CanDevice.PCAN)
            {
                PCANBasic.Reset(PCANBasic.PCAN_USBBUS1);
                var res = PCANBasic.GetStatus(PCANBasic.PCAN_USBBUS1);
                if (res != TPCANStatus.PCAN_ERROR_OK)
                {
                    err = -1;
                }
            }
            CANController.HardwareMutex.ReleaseMutex();
            if (err == 0)
            {
                Started = true;
                ReceiveTimer.Enabled = true;
            }
            else
            {
                Started = false;
                ReceiveTimer.Enabled = false;
            }
            return err;
        }
        public static int Stop()
        {
            CANController.HardwareMutex.WaitOne();
            int err = 0;
            if (TargetDevice == CanDevice.CANUSB || TargetDevice == CanDevice.CANUSB2)
            {
                if (Initialized)
                {
                    ControlCAN.VCI_ResetCAN(ControlCAN.m_devtype, ControlCAN.m_devind, ControlCAN.m_canind);
                    ControlCAN.m_canstart = 0;
                }
            }
            else if (TargetDevice == CanDevice.PCAN)
            {
                PCANBasic.Reset(PCANBasic.PCAN_USBBUS1);
            }
            CANController.HardwareMutex.ReleaseMutex();
            Started = false;
            return err;
        }
        public static int ReadBuffered(out CANFrame frame)
        {
            int err = 0;
            frame = new CANFrame();
            if (RecvFrames.Count > 0)
            {
                frame = RecvFrames.Dequeue();
            }
            else
            {
                err = -1;
            }
            return err;
        }
        public static int Write(CANFrame frame)
        {
            CANController.HardwareMutex.WaitOne();
            int err = 0;
            if (Started)
            {
                if (TargetDevice == CanDevice.CANUSB || TargetDevice == CanDevice.CANUSB2)
                {
                    ControlCAN.VCI_CAN_OBJ obj = CanFrameAdapter.ConvertCANFrameToCANObj(frame);
                    obj.ID = Convert.ToUInt32(MessageId, 16);
                    if (ControlCAN.VCI_Transmit(ControlCAN.m_devtype, ControlCAN.m_devind, ControlCAN.m_canind,
                        ref obj, 1) == 0)
                    {
                        err = -1;
                    }
                }
                else if (TargetDevice == CanDevice.PCAN)
                {
                    TPCANMsg msg;
                    msg = CanFrameAdapter.ConvertFrameToPCAN(frame);
                    msg.ID = Convert.ToUInt32(MessageId, 16);

                    var res = PCANBasic.Write(PCANBasic.PCAN_USBBUS1, ref msg);
                    if (res != TPCANStatus.PCAN_ERROR_OK)
                    {
                        err = -1;
                    }
                }
            }
            else
            {
                err = -1;
            }
            CANController.HardwareMutex.ReleaseMutex();
            return err;
        }

        public static int ConvertSettings()
        {
            if (TargetDevice == CanDevice.CANUSB)
            {
                ControlCAN.m_devtype = ControlCAN.DEV_USBCAN;
            
            }
            else if (TargetDevice == CanDevice.CANUSB2)
            {
                ControlCAN.m_devtype = ControlCAN.DEV_USBCAN2;
            }
            else if (TargetDevice == CanDevice.PCAN)
            {

            }

            ControlCAN.m_devind = DeviceId;
            ControlCAN.m_canmode = (uint)TargetMode;
            ControlCAN.m_ID = System.Convert.ToUInt32(MessageId, 16);

            return 0;
        }
        static CANController()
        {
            Started = false;
            Initialized = false;
            TargetDevice = CanDevice.CANUSB2;
            RecvFrames = new Queue<CANFrame>();

            ReceiveTimer = new System.Timers.Timer(30)
            {
                Enabled = false,
                AutoReset = true
            };
            ReceiveTimer.Elapsed += (object sender, ElapsedEventArgs e) =>
            {

                HardwareMutex.WaitOne();
                ReceiveTimer.Stop();
                if (Started)
                {
                    if (TargetDevice == CanDevice.CANUSB || TargetDevice == CanDevice.CANUSB2)
                    {
                        UInt32 res = new UInt32();
                        res = ControlCAN.VCI_GetReceiveNum(ControlCAN.m_devtype, ControlCAN.m_devind, ControlCAN.m_canind);
                        if (res > 1000)
                            res = 0;
                        ControlCAN.recobj_count = res;
                        if (res != 0)
                        {
                            res = ControlCAN.VCI_Receive(ControlCAN.m_devtype, ControlCAN.m_devind, ControlCAN.m_canind, ref ControlCAN.m_recobj[0], 1000, 100);
                            for (int i = 0; i < ControlCAN.recobj_count; i++)
                            {
                                CANFrame frame = CanFrameAdapter.ConvertCANObjToCANFrame(ControlCAN.m_recobj[i]);
                                RecvFrames.Enqueue(frame);
                            }
                            OnMessageArrive?.Invoke(null, null);
                        }
                    }
                    else if (TargetDevice == CanDevice.PCAN)
                    {
                        TPCANStatus res;
                        while ((res = PCANBasic.Read(PCANBasic.PCAN_USBBUS1, out TPCANMsg msg)) == TPCANStatus.PCAN_ERROR_OK)
                        {
                            CANFrame frame = CanFrameAdapter.ConvertPCANToFrame(msg);
                            RecvFrames.Enqueue(frame);
                        }
                        OnMessageArrive?.Invoke(null, null);
                        if (res != TPCANStatus.PCAN_ERROR_QRCVEMPTY)
                        {

                        }
                    }
                }
                ReceiveTimer.Start();
                HardwareMutex.ReleaseMutex();
            };

            HardwareMutex = new Mutex(false);
        }
    }

    public enum CanDevice
    {
        CANUSB,
        CANUSB2,
        PCAN,
    }

    public enum CanMode
    {
        Normal = 0,
        ListenOnly,
        LoopBack,
    }
    public static class CanFrameAdapter
    {
        public unsafe static CANFrame ConvertCANObjToCANFrame(ControlCAN.VCI_CAN_OBJ obj)
        {
            CANFrame message = new CANFrame();
            for (int i = 0; i < 8; i++)
            {
                message.data[i] = obj.Data[i];
            }
            byte[] value = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                value[i] = message.data[7 - i];
            }
            value.Reverse();
            if (message.DataType == 2)
                message.Value = BitConverter.ToSingle(value, 0);
            else if (message.DataType == 3)
                message.IntValue = BitConverter.ToInt32(value, 0);
            return message;
        }

        public unsafe static ControlCAN.VCI_CAN_OBJ ConvertCANFrameToCANObj(CANFrame message)
        {
            ControlCAN.VCI_CAN_OBJ obj = new ControlCAN.VCI_CAN_OBJ
            {
                ID = ControlCAN.m_ID,
                RemoteFlag = 0,
                ExternFlag = 0,
                DataLen = 8
            };

            for (int i = 0; i < 8; i++)
            {
                obj.Data[i] = message.data[i];
            }
            return obj;
        }

        public static TPCANMsg ConvertFrameToPCAN(CANFrame frame)
        {
            TPCANMsg msg = new TPCANMsg
            {
                DATA = new byte[8],
                LEN = 8,
                MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD
            };
            for (int i = 0; i < 8; i++)
            {
                msg.DATA[i] = frame.data[i];
            }

            return msg;
        }

        public static CANFrame ConvertPCANToFrame(TPCANMsg msg)
        {
            CANFrame frame = new CANFrame();
            for (int i = 0; i < 8; i++)
            {
                frame.data[i] = msg.DATA[i];
            }
            byte[] value = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                value[i] = frame.data[7 - i];
            }
            value.Reverse();
            if (frame.DataType == 2)
                frame.Value = BitConverter.ToSingle(value, 0);
            else if (frame.DataType == 3)
                frame.IntValue = BitConverter.ToInt32(value, 0);
            return frame;
        }
    }
}
