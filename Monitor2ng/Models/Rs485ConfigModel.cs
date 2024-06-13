using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitor2ng.Models
{
    public class Rs485ConfigModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<string> Ports { get; private set; }
        public int SelectedPortIndex
        {
            get => selectedPortIndex;
            set
            {
                selectedPortIndex = value;
                OnPropertityChanged("SelectedPortIndex");
            }
        }
        private int selectedPortIndex;
        public string SelectedPort { get => Ports[SelectedPortIndex]; }

        public string Baudrate
        {
            get
            {
                return baudrate;
            }
            set
            {
                baudrate = value;
                OnPropertityChanged("Baudrate");
            }
        }
        private string baudrate;

        public Rs485ConfigModel()
        {
            Ports = new ObservableCollection<string>();
            var available_ports = SerialPort.GetPortNames();
            foreach (var port in available_ports)
            {
                Ports.Add(port);
            }
            Baudrate = "115200";
        }

        protected void OnPropertityChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
