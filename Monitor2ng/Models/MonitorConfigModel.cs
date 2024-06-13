using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.Text;
using System.Windows.Input;

namespace Monitor2ng.Models
{
    public class MonitorConfigModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<string> Devices { get; private set; }
        public int SelectedDeviceIndex
        {
            get
            {
                return selectedDeviceIndex;
            }
            set
            {
                selectedDeviceIndex = value;
                OnPropertityChanged("SelectedDeviceIndex");
            }
        }
        private int selectedDeviceIndex;
        public string SelectedDevice { get { return Devices[selectedDeviceIndex]; } }

        public ObservableCollection<string> ConfigFiles { get; private set; }
        public int SelectedConfigFileIndex
        {
            get
            {
                return selectedConfigFileIndex;
            }
            set
            {
                selectedConfigFileIndex = value;
                OnPropertityChanged("selectedConfigFileIndex");
            }
        }
        private int selectedConfigFileIndex;
        public string SelectedConfigFile { get { return ConfigFiles[selectedConfigFileIndex]; } }

        public RelayCommand StartCommand { get; set; }

        public MonitorConfigModel()
        {
            Devices = new ObservableCollection<string>();
            Devices.Add("PCAN");
            Devices.Add("USBCAN");
            Devices.Add("ZLGCAN-I");
            Devices.Add("RS485");

            ConfigFiles = new ObservableCollection<string>();
        }

        protected void OnPropertityChanged(string propertityName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertityName));
        }
    }
}
