using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using MonitorV3.Models;
using MonitorV3.CANDriver;

namespace MonitorV3.ViewModels
{
    class MainViewModel
    {
        public CANConfigViewModel CANConfigVM { get; private set; }
        public CANStatusViewModel CANStatusVM { get; private set; }
        public ControlDataViewModel ControlDataVM { get; private set; }
        public CustomButtonViewModel CustomButtonVM { get; private set; }

        public MainViewModel()
        {
            CANConfigVM = new CANConfigViewModel();
            CANStatusVM = new CANStatusViewModel();
            ControlDataVM = new ControlDataViewModel();
            CustomButtonVM = new CustomButtonViewModel();
            CANController.ReceivedEOF += HandleCANMessage;
        }

        public void HandleCANMessage(object sender, EventArgs e)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(()=>{
                string msg = "";
                while (CANController.Avaliable() > 0)
                {
                    char c = (char)CANController.Read();
                    CANStatusVM.CANStatus.ReceiveCount++;
                    if (c == 0xff) break;
                    msg += c;
                }
                try
                {
                    switch (msg[0])
                    {
                        case ('R'):
                            HandleR(msg);
                            break;
                        case ('F'):
                            HandleF(msg);
                            break;
                        case ('H'):
                            HandleH(msg);
                            break;
                    }
                }
                catch (FormatException)
                {
                    return;
                }
            }));
        }

        private void HandleR(string msg)
        {
            int codeNum;
            codeNum = CANManager.GetIntValue(msg, 'R');
            switch (codeNum)
            {
                case (1):
                    HandleR1(msg);
                    break;
            }

        }

        private void HandleF(string msg)
        {
            int codeNum;
            codeNum = CANManager.GetIntValue(msg, 'F');
            switch (codeNum)
            {
                case (1):
                    HandleF1(msg);
                    break;
                case (3):
                    HandleF3(msg);
                    break;
            }
        }

        private void HandleH(string msg)
        {
            int codeNum;
            codeNum = CANManager.GetIntValue(msg, 'H');
            switch (codeNum)
            {
                case (1):
                    HandleH1(msg);
                    break;
            }
        }

        private void HandleR1(string msg)
        {
            ControlDataModel targetcdm = CANManager.ReadMessage(msg);
            if (targetcdm != null)
            {
                var sourcecmd = from ControlDataModel in ControlDataVM.ControlDataCollection
                                where ControlDataModel.ID == targetcdm.ID
                                select ControlDataModel;
                foreach (ControlDataModel m in sourcecmd)
                {
                    m.ReturnValue = targetcdm.ReturnValue;
                }
            }
        }

        private void HandleF1(string msg)
        {
            ControlDataModel targetcmd = CANManager.ReadMessage(msg);
            if (targetcmd != null)
            {
                ControlDataVM.ControlDataCollection.Add(targetcmd);
            }
        }

        private void HandleF3(string msg)
        {
            ControlDataModel targetcmd = CANManager.ReadMessage(msg);
            if (targetcmd != null)
            {
                CustomButtonModel button = new CustomButtonModel(targetcmd.Name, targetcmd.ID);
                CustomButtonVM.CustomButtonCollection.Add(button);
            }
        }

        private void HandleH1(string msg)
        {

        }

        public bool InitCAN(bool open)
        {
            if (open)
            {
                return StartCAN();
            }
            else
            {
                return StopCAN();
            }
        }

        public bool StartCAN()
        {
            CANController.VCI_INIT_CONFIG cfg = new CANController.VCI_INIT_CONFIG();
            switch (CANConfigVM.CANConfig.Dev)
            {
                case (0):
                    CANController.m_devtype = CANController.DEV_USBCAN;
                    break;
                case (1):
                    CANController.m_devtype = CANController.DEV_USBCAN2;
                    break;
            }
            CANController.m_devind = (uint)CANConfigVM.CANConfig.Index;
            CANController.m_canind = (uint)CANConfigVM.CANConfig.Port;
            CANController.m_canmode = (uint)CANConfigVM.CANConfig.Mode;
            cfg.Mode = (byte)CANController.m_canmode;
            try
            {
                cfg.AccCode = Convert.ToUInt32(CANConfigVM.CANConfig.AccCode, 16);
                cfg.AccMask = Convert.ToUInt32(CANConfigVM.CANConfig.AccMask, 16);
                cfg.Timing0 = CANController.BaudrateList[CANConfigVM.CANConfig.Baudrate].Value[0];
                cfg.Timing1 = CANController.BaudrateList[CANConfigVM.CANConfig.Baudrate].Value[1];
                CANController.m_ID = Convert.ToUInt32(CANConfigVM.CANConfig.MID, 16);
            }
            catch (Exception)
            {
                return false;
            }
            if (CANController.VCI_OpenDevice(CANController.m_devtype, CANController.m_devind, 0) != 0)
            {
                try
                {
                    CANController.VCI_InitCAN(CANController.m_devtype, CANController.m_devind, CANController.m_canind, ref cfg);
                    CANController.VCI_StartCAN(CANController.m_devtype, CANController.m_devind, CANController.m_canind);
                    CANController.m_bOpen = 1;
                    CANController.m_canstart = 1;
                    CANController.ReceiveTimer.Start();
                    return true; ;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Initialize Error");
                    Console.WriteLine("Error Infomation: {0}", e.Message);
                    CANController.ReceiveTimer.Stop();
                    return false;
                }
            }
            else
            {
                CANController.m_bOpen = 0;
                CANController.m_canstart = 0;
                return false;
            }
        }

        public bool StopCAN()
        {
            CANController.VCI_CloseDevice(CANController.m_devtype, CANController.m_devind);
            CANController.m_bOpen = 0;
            CANController.m_canstart = 0;
            CANController.ReceiveTimer.Stop();
            return false;
        }

        public void LoadDefinitionsFromDSP()
        {
            CANManager.F2();
        }

        public void LoadAllControlData()
        {
            CANManager.R2();
        }

        public void SendAllControlData()
        {
            foreach(ControlDataModel cdm in ControlDataVM.ControlDataCollection)
            {
                CANManager.M0(cdm);
            }
        }
    }
}
