using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Media;
using System.Globalization;
using System.Windows.Input;

namespace MonitorV3.Models
{
    [Serializable]
    public class CANConfigModel:INotifyPropertyChanged
    {

        private int _dev;
        private int _index;
        private int _port;
        private int _mode;
        private string _accCode;
        private string _accMask;
        private int _baudrate;
        private string _mid;

        public int Dev
        {
            get { return _dev; }
            set
            {
                _dev = value;
                OnPropertyChanged("Dev");
            }
        }

        public int Index
        {
            get { return _index; }
            set
            {
                _index = value;
                OnPropertyChanged("Index");
            }
        }

        public int Port
        {
            get { return _port; }
            set
            {
                _port = value;
                OnPropertyChanged("Port");
            }
        }

        public int Mode
        {
            get { return _mode; }
            set
            {
                _mode = value;
                OnPropertyChanged("Mode");
            }
        }

        public string AccCode
        {
            get { return _accCode; }
            set
            {
                _accCode = value;
                OnPropertyChanged("AccCode");
            }
        }

        public string AccMask
        {
            get { return _accMask; }
            set
            {
                _accMask = value;
                OnPropertyChanged("AccMask");
            }
        }

        public int Baudrate
        {
            get { return _baudrate; }
            set
            {
                _baudrate = value;
                OnPropertyChanged("Baudrate");
            }
        }

        public string MID
        {
            get { return _mid; }
            set
            {
                _mid = value;
                OnPropertyChanged("MID");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertityName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertityName));
        }
    }
}
