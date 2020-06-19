using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using System.Threading.Channels;

namespace Monitor2ng.Models
{
    public class UsbCanConfigModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<string> Devices { get; private set; }
        public int SelectedDeviceIndex
        {
            get => selectedDeviceIndex;
            set
            {
                selectedDeviceIndex = value;
                OnPropertityChanged("SelectedDeviceIndex");
            }
        }
        private int selectedDeviceIndex;
        public string SelectedDevice { get => Devices[SelectedDeviceIndex]; }

        public ObservableCollection<string> Channels { get; private set; }
        public int SelectedChannelIndex
        {
            get => selectedChannelIndex;
            set
            {
                selectedChannelIndex = value;
                OnPropertityChanged("SelectedChannelIndex");
            }
        }
        private int selectedChannelIndex;
        public string SelectedChannel { get => Channels[selectedChannelIndex]; }

        public ObservableCollection<string> Baudrates { get; private set; }
        public int SelectedBaudrateIndex
        {
            get
            {
                return selectedBaudrateIndex;
            }
            set
            {
                selectedBaudrateIndex = value;
                OnPropertityChanged("SelectedBaudrateIndex");
            }
        }
        private int selectedBaudrateIndex;
        public string SelectedBaudrate { get { return Baudrates[SelectedBaudrateIndex]; } }

        public string CommunicationId
        {
            get
            {
                return communicationId;
            }
            set
            {
                communicationId = value;
                OnPropertityChanged("CommunicationId");
            }
        }
        private string communicationId;

        public UsbCanConfigModel()
        {
            Devices = new ObservableCollection<string>();
            Devices.Add("0");
            Devices.Add("1");
            Devices.Add("2");
            Devices.Add("3");

            Channels = new ObservableCollection<string>();
            Channels.Add("0");
            Channels.Add("1");

            Baudrates = new ObservableCollection<string>();
            Baudrates.Add("50 kbps");
            Baudrates.Add("100 kbps");
            Baudrates.Add("250 kbps");
            Baudrates.Add("500 kbps");
            Baudrates.Add("1 Mbps");

            SelectedBaudrateIndex = Baudrates.IndexOf("500 kbps");

            CommunicationId = "0x100";
        }

        protected void OnPropertityChanged(string propertityName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertityName));
        }

    }
}
