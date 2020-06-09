using Monitor2ng.CAN;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.Text;
using System.Threading.Channels;

namespace Monitor2ng.Models
{
    public class PeakCanConfigModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<string> Channels { get; private set; }
        public int SelectedChannelIndex
        {
            get
            {
                return selectedChannelIndex;
            }
            set
            {
                selectedChannelIndex = value;
                OnPropertityChanged("SelectedChannelIndex");
            }
        }
        private int selectedChannelIndex;
        public string SelectedChannel { get { return Channels[selectedChannelIndex]; } }

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

        public PeakCanConfigModel()
        {
            Channels = new ObservableCollection<string>();
            Channels.Add("PCAN_USBBUS1");
            Channels.Add("PCAN_USBBUS2");
            Channels.Add("PCAN_USBBUS3");
            Channels.Add("PCAN_USBBUS4");


            Baudrates = new ObservableCollection<string>();
            Baudrates.Add("250 kbps");
            Baudrates.Add("500 kbps");

            CommunicationId = "0x100";
        }

        protected void OnPropertityChanged(string propertityName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertityName));
        }
    }
}
