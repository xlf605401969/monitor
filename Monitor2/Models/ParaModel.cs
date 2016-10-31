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

namespace Monitor2.Models
{
    [Serializable]
    public class ParaModel:INotifyPropertyChanged
    {
        private byte _paraIndex = 1;
        private byte _paraType = 1;
        private string _paraName = "test";
        private float _paraValue = 2.3f;
        private bool _isValueChanged = false;

        public bool IsValueChanged
        {
            get
            {
                return _isValueChanged;
            }
            set
            {
                _isValueChanged = value;
                OnPropertityChanged("IsValueChanged");
            }
        }

        public byte Index
        {
            get { return _paraIndex; }
            set
            {
                _paraIndex = value;
                OnPropertityChanged("Index");
            }
        }

        public byte Type
        {
            get { return _paraType; }
            set
            {
                _paraType = value;
                OnPropertityChanged("Type");
            }
        }

        public string Name
        {
            get { return _paraName; }
            set
            {
                _paraName = value;
                OnPropertityChanged("Name");
            }
        }

        public float Value
        {
            get { return _paraValue; }
            set
            {
                _paraValue = value;
                _isValueChanged = true;
                OnPropertityChanged("Value");
                OnPropertityChanged("IsValueChanged");
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertityChanged(string propertityName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertityName));
        }
    }

    public class ParaModelWarningLevel:INotifyPropertyChanged
    {
        private byte _paraIndex = 1;
        private string _paraName = "test";
        private float _paraValue = 2.3f;

        public byte Index
        {
            get
            {
                return _paraIndex;
            }
            set
            {
                _paraIndex = value;
                OnPropertityChanged("Index");
            }
        }

        public string Name
        {
            get
            {
                return _paraName;
            }
            set
            {
                _paraName = value;
                OnPropertityChanged("Name");
            }
        }

        public float WarnningValue
        {
            get
            {
                return _paraValue;
            }
            set
            {
                _paraValue = value;
                OnPropertityChanged("WarnningValue");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertityChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    class ValueChangedConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value == true)
            {
                return Brushes.Blue;
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

    [Serializable]
    public class ParaModelWithCommand:ParaModel
    {
        private float _valueChangeStep;
        public float ValueChangeStep
        {
            get { return _valueChangeStep; }
            set
            {
                _valueChangeStep = value;
                OnPropertityChanged("ValueChangeStep");
            }
        }
    }

    [Serializable]
    public class ParaModelWithCommandAndReturn:ParaModelWithCommand
    {
        private float _returnValue;
        public float ReturnValue
        {
            get { return _returnValue; }
            set
            {
                _returnValue = value;
                OnPropertityChanged("ReturnValue");
            }
        }
    }

    public class PlusCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return parameter == null ? false : true;
        }

        public void Execute(object parameter)
        {
            ParaModelWithCommand c = parameter as ParaModelWithCommand;
            c.Value += c.ValueChangeStep;
        }
    }

    public class MinusCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return parameter == null ? false : true;
        }

        public void Execute(object parameter)
        {
            ParaModelWithCommand c = parameter as ParaModelWithCommand;
            c.Value -= c.ValueChangeStep;
        }
    }

}
