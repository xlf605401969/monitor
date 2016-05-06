using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonitorV3.Models;
using MonitorV3.CANDriver;

namespace MonitorV3.ViewModels
{
    class CANStatusViewModel
    {
        public CANStatusModel CANStatus { get; private set; }
        public CANStatusViewModel()
        {
            CANStatus = new CANStatusModel();
            CANStatus.IsCANStarted = CANController.m_canstart > 0;
        }

        public void SetCANStatus(bool staus)
        {
            CANStatus.IsCANStarted = staus;
        }

        public void ToggleCANStatus()
        {
            CANStatus.IsCANStarted = !CANStatus.IsCANStarted;
        }
    }
}
