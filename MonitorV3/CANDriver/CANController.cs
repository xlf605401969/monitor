using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Timers;
using System.Threading;

namespace MonitorV3.CANDriver
{
    class CANController
    {
        public const int DEV_USBCAN = 3;
        public const int DEV_USBCAN2 = 4;
        /*------------兼容ZLG的数据类型---------------------------------*/

        //1.ZLGCAN系列接口卡信息的数据类型。
        public struct VCI_BOARD_INFO
        {
            public UInt16 hw_Version;
            public UInt16 fw_Version;
            public UInt16 dr_Version;
            public UInt16 in_Version;
            public UInt16 irq_Num;
            public byte can_Num;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public byte[] str_Serial_Num;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public byte[] str_hw_Type;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] Reserved;
        }

        /////////////////////////////////////////////////////
        //2.定义CAN信息帧的数据类型。
        unsafe public struct VCI_CAN_OBJ  //使用不安全代码
        {
            public uint ID;
            public uint TimeStamp;        //时间标识
            public byte TimeFlag;         //是否使用时间标识
            public byte SendType;         //发送标志。保留，未用
            public byte RemoteFlag;       //是否是远程帧
            public byte ExternFlag;       //是否是扩展帧
            public byte DataLen;

            public fixed byte Data[8];

            public fixed byte Reserved[3];

        }

        //3.定义CAN控制器状态的数据类型。
        public struct VCI_CAN_STATUS
        {
            public byte ErrInterrupt;
            public byte regMode;
            public byte regStatus;
            public byte regALCapture;
            public byte regECCapture;
            public byte regEWLimit;
            public byte regRECounter;
            public byte regTECounter;
            public uint Reserved;
        }

        //4.定义错误信息的数据类型。
        public struct VCI_ERR_INFO
        {
            public uint ErrCode;
            public byte Passive_ErrData1;
            public byte Passive_ErrData2;
            public byte Passive_ErrData3;
            public byte ArLost_ErrData;
        }

        //5.定义初始化CAN的数据类型
        public struct VCI_INIT_CONFIG
        {
            public UInt32 AccCode;
            public UInt32 AccMask;
            public UInt32 Reserved;
            public byte Filter;   //1接收所有帧。2标准帧滤波，3是扩展帧滤波。
            public byte Timing0;
            public byte Timing1;
            public byte Mode;     //模式，0表示正常模式，1表示只听模式,2自测模式
        }

        /*------------其他数据结构描述---------------------------------*/
        //6.USB-CAN总线适配器板卡信息的数据类型1，该类型为VCI_FindUsbDevice函数的返回参数。
        public struct VCI_BOARD_INFO1
        {
            public UInt16 hw_Version;
            public UInt16 fw_Version;
            public UInt16 dr_Version;
            public UInt16 in_Version;
            public UInt16 irq_Num;
            public byte can_Num;
            public byte Reserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] str_Serial_Num;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] str_hw_Type;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[][] str_Usb_Serial;
        }

        //7.定义常规参数类型
        public struct VCI_REF_NORMAL
        {
            public byte Mode;     //模式，0表示正常模式，1表示只听模式,2自测模式
            public byte Filter;   //1接收所有帧。2标准帧滤波，3是扩展帧滤波。
            public UInt32 AccCode;//接收滤波验收码
            public UInt32 AccMask;//接收滤波屏蔽码
            public byte kBaudRate;//波特率索引号，0-SelfDefine,1-5K(未用),2-18依次为：10k,20k,40k,50k,80k,100k,125k,200k,250k,400k,500k,666k,800k,1000k,33.33k,66.66k,83.33k
            public byte Timing0;
            public byte Timing1;
            public byte CANRX_EN;//保留，未用
            public byte UARTBAUD;//保留，未用
        }

        //8.定义波特率设置参数类型
        public struct VCI_BAUD_TYPE
        {
            public UInt32 Baud;             //存储波特率实际值
            public byte SJW;                //同步跳转宽度，取值1-4
            public byte BRP;                //预分频值，取值1-64
            public byte SAM;                //采样点，取值0=采样一次，1=采样三次
            public byte PHSEG2_SEL;         //相位缓冲段2选择位，取值0=由相位缓冲段1时间决定,1=可编程
            public byte PRSEG;              //传播时间段，取值1-8
            public byte PHSEG1;             //相位缓冲段1，取值1-8
            public byte PHSEG2;             //相位缓冲段2，取值1-8

        }

        //9.定义Reference参数类型
        public struct VCI_REF_STRUCT
        {
            public VCI_REF_NORMAL RefNormal;
            public byte Reserved;
            public VCI_BAUD_TYPE BaudType;
        }

        /*------------数据结构描述完成---------------------------------*/

        public struct CHGDESIPANDPORT
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public byte[] szpwd;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public byte[] szdesip;
            public Int32 desport;

            public void Init()
            {
                szpwd = new byte[10];
                szdesip = new byte[20];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="DeviceType"></param>
        /// <param name="DeviceInd"></param>
        /// <param name="Reserved"></param>
        /// <returns></returns>
        /*------------兼容ZLG的函数描述---------------------------------*/
        [DllImport("controlcan.dll")]
        public static extern UInt32 VCI_OpenDevice(UInt32 DeviceType, UInt32 DeviceInd, UInt32 Reserved);
        [DllImport("controlcan.dll")]
        public static extern UInt32 VCI_CloseDevice(UInt32 DeviceType, UInt32 DeviceInd);
        [DllImport("controlcan.dll")]
        public static extern UInt32 VCI_InitCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_INIT_CONFIG pInitConfig);

        [DllImport("controlcan.dll")]
        public static extern UInt32 VCI_ReadBoardInfo(UInt32 DeviceType, UInt32 DeviceInd, ref VCI_BOARD_INFO pInfo);
        [DllImport("controlcan.dll")]
        public static extern UInt32 VCI_ReadErrInfo(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_ERR_INFO pErrInfo);
        [DllImport("controlcan.dll")]
        public static extern UInt32 VCI_ReadCANStatus(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_CAN_STATUS pCANStatus);

        [DllImport("controlcan.dll")]
        public static extern UInt32 VCI_GetReference(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, UInt32 RefType, ref byte pData);
        [DllImport("controlcan.dll")]
        public static extern UInt32 VCI_SetReference(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, UInt32 RefType, ref byte pData);

        [DllImport("controlcan.dll")]
        public static extern UInt32 VCI_GetReceiveNum(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);
        [DllImport("controlcan.dll")]
        public static extern UInt32 VCI_ClearBuffer(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);

        [DllImport("controlcan.dll")]
        public static extern UInt32 VCI_StartCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);
        [DllImport("controlcan.dll")]
        public static extern UInt32 VCI_ResetCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);

        [DllImport("controlcan.dll")]
        public static extern UInt32 VCI_Transmit(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_CAN_OBJ pSend, UInt32 Len);

        [DllImport("controlcan.dll")]
        public static extern UInt32 VCI_Receive(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_CAN_OBJ pReceive, UInt32 Len, Int32 WaitTime);

        // [DllImport("controlcan.dll", CharSet = CharSet.Ansi)]
        //public static extern UInt32 VCI_Receive(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, IntPtr pReceive, UInt32 Len, Int32 WaitTime);

        /*------------其他函数描述---------------------------------*/
        [DllImport("controlcan.dll")]
        public static extern UInt32 VCI_GetReference2(UInt32 DevType, UInt32 DevIndex, UInt32 CANIndex, UInt32 Reserved, ref VCI_REF_STRUCT pRefStruct);
        [DllImport("controlcan.dll")]
        public static extern UInt32 VCI_SetReference2(UInt32 DevType, UInt32 DevIndex, UInt32 CANIndex, UInt32 RefType, ref byte pData);
        [DllImport("controlcan.dll")]
        public static extern UInt32 VCI_ResumeConfig(UInt32 DevType, UInt32 DevIndex, UInt32 CANIndex);

        [DllImport("controlcan.dll")]
        public static extern UInt32 VCI_ConnectDevice(UInt32 DevType, UInt32 DevIndex);
        [DllImport("controlcan.dll")]
        public static extern UInt32 VCI_UsbDeviceReset(UInt32 DevType, UInt32 DevIndex, UInt32 Reserved);
        [DllImport("controlcan.dll")]
        public static extern UInt32 VCI_FindUsbDevice(ref VCI_BOARD_INFO1 pInfo);
        /*------------函数描述结束---------------------------------*/

        public static UInt32 m_devtype = 4;//USBCAN2

        public static UInt32 m_bOpen = 0;
        public static UInt32 m_devind = 0;
        public static UInt32 m_canind = 0;

        public static UInt32 m_canstart = 0;

        public static UInt32 m_canmode = 0;

        public static VCI_CAN_OBJ[] m_recobj = new VCI_CAN_OBJ[1000];

        public static uint recobj_count = 0;

        public static UInt32 m_ID = 0x00001234;

        public static Dictionary<string, byte[]> BaudrateDic;

        public static System.Timers.Timer ReceiveTimer;

        public static Mutex CANControllerMutex;

        public static Queue<byte> CANSendQueue, CANReceiveQueue;

        public static event EventHandler ReceivedEOF;

        public static TimeSpan ReceiveTimeSpan;

        public static long ReceivedBytesCount = 0;

        public static long SentBytesCount = 0;

        unsafe public static void EnQueueCANObjData(VCI_CAN_OBJ obj)
        {
            for (int i = 0; i < 8; i++)
            {
                if (obj.Data[i] == 0x04) break;
                CANReceiveQueue.Enqueue(obj.Data[i]);
                ReceivedBytesCount++;
                if (obj.Data[i] == 0xff)
                {
                    ReceivedEOF?.Invoke(null, new EventArgs());
                }
            }
        }

        public static void EnQueueString(string str)
        {
            foreach (char c in str)
            {
                if (c == 0x04) break;
                CANReceiveQueue.Enqueue((byte)c);
                if (c == 0xff)
                {
                    ReceivedEOF?.Invoke(null, new EventArgs());
                }
            }
        }

        public static void CANControllerTimerHandler(object sender, ElapsedEventArgs e)
        {
            if (CANControllerMutex.WaitOne(30) == true)
            {
                UInt32 res = new UInt32();
                res = VCI_GetReceiveNum(m_devtype, m_devind, m_canind);
                if (res > 1000)
                    res = 0;
                recobj_count = res;
                if (res != 0)
                {
                    res = VCI_Receive(m_devtype, m_devind, m_canind, ref m_recobj[0], 1000, 100);
                    ReceiveTimeSpan = new TimeSpan(0);
                }
                else
                {
                    ReceiveTimeSpan.Add(new TimeSpan(0, 0, 0, 0, 30));
                }
                for (int i = 0; i < res; i++)
                {
                    EnQueueCANObjData(m_recobj[i]);
                }
                CANControllerMutex.ReleaseMutex();
            }
        }

        public static void Write(String str, byte EndChar = 0x00)
        {
            foreach(byte c in str)
            {
                CANSendQueue.Enqueue(c);
            }
            if (EndChar != 0x00)
            {
                CANSendQueue.Enqueue(EndChar);
            }
            SendQueueData();
        }

        public static void Write(byte[] bs, byte EndChar = 0x00)
        {
            foreach (byte b in bs)
            {
                CANSendQueue.Enqueue(b);
            }
            if (EndChar != 0x00)
            {
                CANSendQueue.Enqueue(EndChar);
            }
            SendQueueData();
        }

        public static void Write(byte val)
        {
            CANSendQueue.Enqueue(val);
            SendQueueData();
        }

        public static void WriteEOT()
        {
            CANSendQueue.Enqueue(0xff);
            SendQueueData();
        }

        public static int Avaliable()
        {
            return CANReceiveQueue.Count;
        }

        public static byte Read()
        {
            return CANReceiveQueue.Dequeue();
        }

        public static int ReadBytes(ref byte[] b, int length)
        {
            int i = 0;
            for (i = 0; i < length && CANReceiveQueue.Count > 0; i++)
            {
                b[i] = CANReceiveQueue.Dequeue();
            }
            return i;
        }

        public unsafe static void SendQueueData()
        {
            if (m_canstart == 0)
            {
                CANSendQueue.Clear();
            }
            while(CANSendQueue.Count > 0)
            {
                VCI_CAN_OBJ obj = new VCI_CAN_OBJ();
                obj.ID = m_ID;
                obj.RemoteFlag = 0;
                obj.ExternFlag = 0;
                obj.DataLen = 8;
                int tmpCount = CANSendQueue.Count;
                if (tmpCount >= 8)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        obj.Data[i] = CANSendQueue.Dequeue();
                        SentBytesCount++;
                    }
                }
                else
                {
                    for (int i = 0; i < tmpCount; i++)
                    {
                        obj.Data[i] = CANSendQueue.Dequeue();
                        SentBytesCount++;
                    }
                    obj.Data[tmpCount] = 0x04;
                }
                CANControllerMutex.WaitOne();
                CANController.VCI_Transmit(CANController.m_devtype, CANController.m_devind, CANController.m_canind, ref obj, 1);
                CANControllerMutex.ReleaseMutex();
            }
        }

        static CANController()
        {
            BaudrateDic = new Dictionary<string, byte[]>();
            BaudrateDic.Add("10K", new byte[] { 0x9f, 0xff });
            BaudrateDic.Add("20K", new byte[] { 0x18, 0x1c });
            BaudrateDic.Add("40K", new byte[] { 0x87, 0xff });
            BaudrateDic.Add("50K", new byte[] { 0x09, 0x1c });
            BaudrateDic.Add("80K", new byte[] { 0x83, 0xff });
            BaudrateDic.Add("100K", new byte[] { 0x04, 0x1c });
            BaudrateDic.Add("124K", new byte[] { 0x03, 0x1c });
            BaudrateDic.Add("200K", new byte[] { 0x81, 0xfa });
            BaudrateDic.Add("250K", new byte[] { 0x01, 0x1c });
            BaudrateDic.Add("400K", new byte[] { 0x80, 0xfa });
            BaudrateDic.Add("500K", new byte[] { 0x00, 0x1c });
            BaudrateDic.Add("666K", new byte[] { 0x80, 0xb6 });
            BaudrateDic.Add("800K", new byte[] { 0x00, 0x16 });
            BaudrateDic.Add("1000K", new byte[] { 0x00, 0x14 });
            BaudrateDic.Add("33.33K", new byte[] { 0x09, 0x6f });
            BaudrateDic.Add("66.6K", new byte[] { 0x04, 0x6f });
            BaudrateDic.Add("83.33K", new byte[] { 0x03, 0x6f });

            CANSendQueue = new Queue<byte>();
            CANReceiveQueue = new Queue<byte>();

            ReceivedEOF += CANController_ReceivedEOF;

            ReceiveTimer = new System.Timers.Timer(30);
            ReceiveTimer.Enabled = false;
            ReceiveTimer.AutoReset = true;
            ReceiveTimer.Elapsed += CANControllerTimerHandler;

            ReceiveTimeSpan = new TimeSpan(0);

            CANControllerMutex = new Mutex(false);
        }

        private static void CANController_ReceivedEOF(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }
    }
}
