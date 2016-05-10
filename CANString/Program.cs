using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using CANString.CANDriver;

namespace CANString
{
    class Program
    {
        public static NameValueCollection ArgsPair = new NameValueCollection();
        public static CANController.VCI_INIT_CONFIG GlobalCANConfig = new CANController.VCI_INIT_CONFIG();
        public static bool GlobalErr = false;
        static void Main(string[] args)
        {
            SetDefaultConfig();
            if (ParseArgs(args))
            {
                PrintHelp();
                Console.ReadKey();
                return;
            }
            PrintArgs();
            if (InitCAN(ref GlobalCANConfig))
            {
                Console.ReadKey();
                return;
            }
            CANController.m_bOpen = 1;
            CANController.m_canstart = 1;
            CANController.ReceiveTimer.Start();
            Console.Write(">");
            string strLine;
            do
            {
                strLine = Console.ReadLine();
                if (strLine == "exit")
                    break;
                CANController.Write(strLine);
                while (CANController.Avaliable() > 0)
                {
                    Console.Write((char)CANController.Read());
                }
                Console.Write(">");
            } while (true);
        }

        static bool ParseArgs(string[] args)
        {
            bool err = false;
            List<string> args2 = new List<string>();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Contains('-'))
                {
                    if (i == args.Length - 1)
                    {
                        args2.Add(args[i]);
                    }
                    else
                    {
                        if (!args[i + 1].Contains('-'))
                        {
                            args2.Add(args[i] + ':' + args[i + 1]);
                            i++;
                        }
                        else
                        {
                            args2.Add(args[i]);
                        }
                    }
                }
            }
            foreach (string str in args2)
            {
                int index = str.IndexOf(':');
                string command, value;
                if (index > 0)
                {
                    command = str.Substring(1, index - 1);
                    value = str.Substring(index + 1);
                }
                else
                {
                    command = str.Substring(1);
                    value = "";
                }
                switch (command)
                {
                    case ("dev"):
                        if (value != "")
                        {
                            if (value == "CAN")
                            {
                                CANController.m_devtype = CANController.DEV_USBCAN;
                            }
                            else if (value == "CAN2")
                            {
                                CANController.m_devtype = CANController.DEV_USBCAN2;
                            }
                            else
                            {
                                try
                                {
                                    CANController.m_devtype = UInt32.Parse(value);
                                }
                                catch (Exception e)
                                {
                                    err = true;
                                }
                            }
                        }
                        break;
                    case ("index"):
                        if (value != "")
                        {
                            try
                            {
                                CANController.m_devind = UInt32.Parse(value);
                            }
                            catch (Exception e)
                            {
                                err = true;
                            }
                        }
                        break;
                    case ("port"):
                        if (value != "")
                        {
                            try
                            {
                                CANController.m_canind = UInt32.Parse(value);
                            }
                            catch (Exception e)
                            {
                                err = true;
                            }
                        }
                        break;
                    case ("mode"):
                        try
                        {
                            CANController.m_canmode = Convert.ToUInt32(value);
                            GlobalCANConfig.Mode = (byte)CANController.m_canmode;
                        }
                        catch (Exception e)
                        {
                            err = true;
                        }
                        break;
                    case ("acccode"):
                        try
                        {
                            GlobalCANConfig.AccCode = Convert.ToUInt32(value, 16);
                        }
                        catch (Exception e)
                        {
                            err = true;
                        }
                        break;
                    case ("accmask"):
                        try
                        {
                            GlobalCANConfig.AccMask = Convert.ToUInt32(value, 16);
                        }
                        catch (Exception e)
                        {
                            err = true;
                        }
                        break;
                    case ("baudrate"):
                        try
                        {
                            GlobalCANConfig.Timing0 = CANController.BaudrateDic[value][0];
                            GlobalCANConfig.Timing1 = CANController.BaudrateDic[value][1];
                        }
                        catch (Exception e)
                        {
                            err = true;
                        }
                        break;
                    case ("mid"):
                        try
                        {
                            CANController.m_ID = Convert.ToUInt32(value, 16);
                        }
                        catch (Exception e)
                        {
                            err = true;
                        }
                        break;
                    case ("h"):
                        PrintHelp();
                        break;
                }
                ArgsPair.Add(command, value);
            }
            if (err == true)
            {
                Console.WriteLine("Wrong Arguments!");
            }
            return err;
        }
        public static void SetDefaultConfig()
        {
            GlobalCANConfig.Mode = 0;
            GlobalCANConfig.AccCode = 0;
            GlobalCANConfig.AccMask = 1;
            GlobalCANConfig.Filter = 1;
            GlobalCANConfig.Timing0 = CANController.BaudrateDic["500K"][0];
            GlobalCANConfig.Timing0 = CANController.BaudrateDic["500K"][1];
            CANController.m_devtype = CANController.DEV_USBCAN2;
            CANController.m_devind = 0;
            CANController.m_canind = 0;
            CANController.m_canmode = 0;
            CANController.m_bOpen = 0;
            CANController.m_canstart = 0;
            CANController.m_ID = 0x00000100;
        }
        public static void PrintHelp()
        {
            Console.Write("Usage:\n\t-dev\t\tDevice Type, CAN or CAN2\n\t-index\t\tDevice Index, start from 0\n\t-port\t\tDevice port, 0 or 1\n\t-mode\t\tDevice mode, 0-normal, 1-listen, 2-selftest\n\t-acccode\tFor Package Filter\n\t-accmask\tFor Package Filter\n\t-baudrate\tSpeed Selection, Default 500K\n\t-mid\t\tPackage Id\n\t-h\t\tHelp\n");
        }
        public static void PrintArgs()
        {
            foreach (string key in ArgsPair.Keys)
            {
                Console.WriteLine("{0}:{1}", key, ArgsPair[key]);
            }
        }
        public static bool InitCAN(ref CANController.VCI_INIT_CONFIG config)
        {
            try
            {
                CANController.VCI_OpenDevice(CANController.m_devtype, CANController.m_devind, 0);
                CANController.VCI_InitCAN(CANController.m_devtype, CANController.m_devind, CANController.m_canind, ref config);
                CANController.VCI_StartCAN(CANController.m_devtype, CANController.m_devind, CANController.m_canind);
                CANController.m_bOpen = 1;
                CANController.m_canstart = 1;
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine("Initialize Error");
                Console.WriteLine("Error Infomation: {0}", e.Message);
                GlobalErr = true;
                return true;
            }
        }
    }
}
