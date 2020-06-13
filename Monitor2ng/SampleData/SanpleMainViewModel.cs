using Monitor2ng.Models;
using Monitor2ng.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monitor2ng.SampleData
{
    public class SampleMainViewModel : MainWindowsViewModel
    {
        public SampleMainViewModel() : base()
        {
            MonitorConfigModel = new MonitorConfigModel();
            MonitorConfigModel.SelectedDeviceIndex = 1;
        }
    }
}
