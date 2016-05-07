using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonitorV3.CANDriver;
using MonitorV3.Models;

namespace MonitorV3.CANDriver
{
    class CANManager
    {
        public static void R0(ControlDataModel cdm)
        {
            CANController.Write("R0 I" + cdm.ID.ToString(), 0xff);
        }

        public static void R2()
        {
            CANController.Write("R2", 0xff);
        }

        public static void R3(ControlDataModel cdm)
        {
            CANController.Write("R3 I" + cdm.ID.ToString() + " T" + (cdm.AutoCheckTimeSpan < 100 ? 100 : cdm.AutoCheckTimeSpan).ToString(), 0xff);
        }

        public static void R4(ControlDataModel cdm)
        {
            CANController.Write("R4 I" + cdm.ID.ToString(), 0xff);
        }

        public static void M0(ControlDataModel cdm)
        {
            if (cdm.Type == ControlDataType.FLOAT)
            {
                CANController.Write("M0 I" + cdm.ID.ToString() + " V" + cdm.Value.ToString("G8"), 0xff);
            }
            else if (cdm.Type == ControlDataType.INT || cdm.Type == ControlDataType.INTINDEX)
            {
                CANController.Write("M0 I" + cdm.ID.ToString() + " V" + cdm.Value.ToString("F0"), 0xff);
            }
        }

        public static void F2()
        {
            CANController.Write("F2", 0xff);
        }

        public static void S(int i)
        {
            CANController.Write("S" + i.ToString(), 0xff);
        }

        public static void H0()
        {
            CANController.Write("H0", 0xff);
        }

        public static ControlDataModel ReadMessage(string msg)
        {
            ControlDataModel cdm = new ControlDataModel();
            bool err = false;
            try
            {
                switch (msg[0])
                {
                    case ('R'):
                        err = ReadR(msg, cdm);
                        break;
                    case ('F'):
                        err = ReadF(msg, cdm);
                        break;
                }
            }
            catch (FormatException)
            {
                err = true;
            }
            return err ? null : cdm;
        }

        private static bool ReadR(string msg, ControlDataModel cdm)
        {
            int codeNum;
            codeNum = GetIntValue(msg, 'R');
            switch(codeNum)
            {
                case (1):
                    return ReadR1(msg, cdm);
                default:
                    return true;
            }
        }

        private static bool ReadR1(string msg, ControlDataModel cdm)
        {
            if (FindCode(msg, 'I'))
            {
                cdm.ID = GetIntValue(msg, 'I');
            }
            if (FindCode(msg, 'V'))
            {
                cdm.ReturnValue = GetFloatValue(msg, 'V');
            }
            return false;
        }

        private static bool ReadF(string msg, ControlDataModel cdm)
        {
            int codeNum;
            codeNum = GetIntValue(msg, 'F');
            switch (codeNum)
            {
                case (1):
                    return ReadF1(msg, cdm);
                default:
                    return true;
            }
        }

        private static bool ReadF1(string msg, ControlDataModel cdm)
        {
            if (FindCode(msg, 'I'))
            {
                cdm.ID = GetIntValue(msg, 'I');
            }

            if (FindCode(msg, 'T'))
            {

                cdm.Type = (ControlDataType)GetIntValue(msg, 'T');
            }

            if (FindCode(msg, 'W'))
            {

                cdm.IsEditable = GetIntValue(msg, 'W') > 0;
            }

            if (FindCode(msg, 'N'))
            {
                cdm.Name = GetStringValue(msg, 'N');
            }
            return false;
        }

        public static float GetFloatValue(string str, char code)
        {
            return Convert.ToSingle(GetStringValue(str, code));
        }

        public static int GetIntValue(string str, char code)
        {
            return (int)GetFloatValue(str, code);
        }

        public static string GetStringValue(string str, char code)
        {
            int i, j;
            string temp = "";
            i = str.IndexOf(code);
            if (code != 'N')
            {
                if (i >= 0)
                {
                    j = str.IndexOf(' ', i);
                    if (j >= 0)
                        temp = str.Substring(i + 1, j - i);
                    else
                        temp = str.Substring(i + 1);
                }
            }
            else
            {
                temp = str.Substring(i + 1);
            }
            return temp;
        }

        public static bool FindCode(string str, char code)
        {
            int i;
            string temp = "";
            i = str.IndexOf('N');
            if (code == 'N' && i >= 0)
                return true;
            if (i >= 0)
                temp = str.Substring(0, i);
            else
                temp = str;
            if (temp.IndexOf(code) >= 0)
                return true;
            else
                return false;
        }
    }
}
