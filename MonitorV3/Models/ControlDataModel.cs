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
        private List<KeyValuePair<DateTime, float>> _log;

        public int ID
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertityChanged("ID");
            }
        }

        public float Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertityChanged("Value");
                OnPropertityChanged("IsValueEqual");
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertityChanged("Name");
            }
        }

        public ControlDataType Type
        {
            get { return _type; }
            set
            {
                _type = value;
                OnPropertityChanged("Type");
            }
        }

        public bool IsLog
        {
            get { return _isLog; }
            set
            {
                _isLog = value;
                OnPropertityChanged("IsLog");
            }
        }

        public int AutoCheckTimeSpan
        {
            get { return _autoCheckTimeSpan; }
            set
            {
                _autoCheckTimeSpan = value;
                OnPropertityChanged("AutoCheckTimeSpan");
            }
        }

        public bool IsEditable
        {
            get { return _isEditable; }
            set
            {
                _isEditable = value;
                OnPropertityChanged("IsReadOnly");
            }
        }

        public bool IsAutoCheck
        {
            get { return _isAutoCheck; }
            set
            {
                _isAutoCheck = value;
                OnPropertityChanged("IsAutoCheck");
            }
        }

        public float ReturnValue
        {
            get { return _returnValue; }
            set
            {
                _returnValue = value;
                OnPropertityChanged("ReturnValue");
                OnPropertityChanged("IsValueEqual");
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertityChanged(string propertityName)
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
        INT16 = 1,
        FLOAT = 2,
    }
}
