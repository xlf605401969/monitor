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
        public MainViewModel()
        {
            CANConfigVM = new CANConfigViewModel();
            CANStatusVM = new CANStatusViewModel();
            ControlDataVM = new ControlDataViewModel();
            CANController.ReceivedEOF += HandleCANMessage;
        }

        public void HandleCANMessage(object sender, EventArgs e)
        {
            string msg = "";
            while (CANController.Avaliable() > 0)
            {
                char c = (char)CANController.Read();
                CANStatusVM.CANStatus.ReceiveCount++;
                if (c == 0xff) break;
                msg += c;
            }
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

        private void HandleR(string msg)
        {
            int codeNum;
            try
            {
                codeNum = CANManager.GetIntValue(msg, 'R');
            }
            catch (FormatException e)
            {
                return;
            }
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
            try
            {
                codeNum = CANManager.GetIntValue(msg, 'F');
            }
            catch (FormatException e)
            {
                return;
            }
            switch (codeNum)
            {
                case (1):
                    HandleF1(msg);
                    break;
            }
        }

        private void HandleH(string msg)
        {
            int codeNum;
            try
            {
                codeNum = CANManager.GetIntValue(msg, 'H');
            }
            catch (FormatException e)
            {
                return;
            }
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

        private void HandleH1(string msg)
        {

        }
    }
}
