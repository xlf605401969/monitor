using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;

namespace MonitorV3.Models
{
    class CANStatusModel:INotifyPropertyChanged
    {
        private bool _isCANStarted;
        private long _receiveCount;
        private long _sendCount;

        public bool IsCANStarted
        {
            get { return _isCANStarted; }
            set
            {
                _isCANStarted = value;
                OnPropertyChanged("IsCANStarted");
            }
        }

        public long ReceiveCount
        {
            get { return _receiveCount; }
            set
            {
                _receiveCount = value;
                OnPropertyChanged("ReceiveCount");
            }
        }

        public long SendCount
        {
            get { return _sendCount; }
            set
            {
                _sendCount = value;
                OnPropertyChanged("SendCount");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertityName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertityName));
        }
    }

    class CANStatusToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value == true)
            {
                return Brushes.Red;
            }
            else
            {
                return Brushes.Green;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class CANStatusToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value == true)
            {
                return "断开";
            }
            else
            {
                return "连接";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
