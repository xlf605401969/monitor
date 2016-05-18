using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;
using System.Collections.ObjectModel;

namespace MonitorV3.Models
{
    [Serializable]
    public class ControlDataModel:INotifyPropertyChanged
    {
        private int _id;
        private float _value;
        private float _returnValue;
        private string _name;
        private ControlDataType _type;
        private bool _isLog;
        private bool _isAutoCheck;
        private int _autoCheckTimeSpan = 1000;
        private bool _isEditable = true;

        [NonSerialized]
        private ObservableCollection<LogData> _log;

        public int ID
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged("ID");
            }
        }

        public float Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged("Value");
                OnPropertyChanged("IsValueEqual");
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        public ControlDataType Type
        {
            get { return _type; }
            set
            {
                _type = value;
                OnPropertyChanged("Type");
            }
        }

        public bool IsLog
        {
            get { return _isLog; }
            set
            {
                _isLog = value;
                OnPropertyChanged("IsLog");
            }
        }

        public int AutoCheckTimeSpan
        {
            get { return _autoCheckTimeSpan; }
            set
            {
                _autoCheckTimeSpan = value;
                OnPropertyChanged("AutoCheckTimeSpan");
            }
        }

        public bool IsEditable
        {
            get { return _isEditable; }
            set
            {
                _isEditable = value;
                OnPropertyChanged("IsReadOnly");
            }
        }

        public bool IsAutoCheck
        {
            get { return _isAutoCheck; }
            set
            {
                _isAutoCheck = value;
                OnPropertyChanged("IsAutoCheck");
            }
        }

        public float ReturnValue
        {
            get { return _returnValue; }
            set
            {
                _returnValue = value;
                OnPropertyChanged("ReturnValue");
                OnPropertyChanged("IsValueEqual");
            }
        }

        public bool IsValueEqual
        {
            get
            {
                if (_isEditable && Value != ReturnValue)
                    return false;
                else
                    return true;
            }
        }

        public ObservableCollection<LogData> Log
        {
            get
            {
                return _log;
            }
            set
            {
                _log = value;
                OnPropertyChanged("Log");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertityName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertityName));
        }

        public ControlDataModel()
        {
            this._autoCheckTimeSpan = 1000;
        }
    }

    public class LogData:INotifyPropertyChanged
    {
        private DateTime _time;
        private float _value;

        public DateTime Time
        {
            get { return _time; }
            set
            {
                _time = value;
                OnPropertyChanged("Time");
            }
        }

        public float Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged("Value");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertityName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertityName));
        }
    }

    class IsValueEqualToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value == false)
            {
                return Brushes.Red;
            }
            else
            {
                return Brushes.Black;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public enum ControlDataType
    {
        NONE = 0,
        INT32 = 1,
        FLOAT = 2,
        INT16 = 3
    }
}
