using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monitor2.Models;
using System.IO;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Timers;
using Monitor2.CAN;
using System.Windows.Data;
using System.Windows.Media;
using System.Globalization;
using System.Collections.ObjectModel;

namespace Monitor2.ViewModels
{
    public class ControlTabViewModel:INotifyPropertyChanged
    {
        public XmlSerializer xmlControlParasFormat = new XmlSerializer(typeof(List<ParaModelWithCommandAndReturn>));
        public XmlSerializer xmlStatusParasFormat = new XmlSerializer(typeof(ObservableCollection<ParaModel>));

        public ParaModelWithCommandAndReturn Para1 { get; private set; }
        public ParaModelWithCommandAndReturn Para2 { get; private set; }
        public ParaModelWithCommandAndReturn Para3 { get; private set; }
        public ParaModelWithCommandAndReturn Para4 { get; private set; }
        public ParaModelWithCommandAndReturn ParaMode { get; private set; }
        public ParaModelWithCommandAndReturn ParaDirection { get; private set; }

        public Timer AutoCheckTimer = new Timer(300);


        private bool _isAutoSend;
        public bool IsAutoSend
        {
            get
            {
                return _isAutoSend;
            }
            set
            {
                _isAutoSend = value;
                OnPropertityChanged("IsAutoSend");
            }
        }

        private bool _isAutoCheckStatus;
        public bool IsAutoCheckStatus
        {
            get
            {
                return _isAutoCheckStatus;
            }
            set
            {
                _isAutoCheckStatus = value;
                AutoCheckTimer.Enabled = value;
                OnPropertityChanged("IsAutoCheckStatus");
            }
        }

        private int _connectStatus = -1;
        public int ConnectStatus
        {
            get
            {
                return _connectStatus;
            }
            set
            {
                _connectStatus = value;
                OnPropertityChanged("ConnectStatus");
            }
        }

        private int _deviceStatus = -1;
        public int DeviceStatus
        {
            get
            {
                return _deviceStatus;
            }
            set
            {
                _deviceStatus = value;
                OnPropertityChanged("DeviceStatus");
            }
        }

        public int missCount; 

        public List<ParaModelWithCommandAndReturn> ControlParasList { get; private set; }

        public ObservableCollection<ParaModel> StatusParasList { get; private set; }

        public ControlTabViewModel()
        {
            Para1 = new ParaModelWithCommandAndReturn();
            Para2 = new ParaModelWithCommandAndReturn();
            Para3 = new ParaModelWithCommandAndReturn();
            Para4 = new ParaModelWithCommandAndReturn();
            ParaDirection = new ParaModelWithCommandAndReturn();
            ParaMode = new ParaModelWithCommandAndReturn();

            ControlParasList = new List<ParaModelWithCommandAndReturn>();
            ControlParasList.Add(Para1);
            ControlParasList.Add(Para2);
            ControlParasList.Add(Para3);
            ControlParasList.Add(Para4);
            ControlParasList.Add(ParaDirection);
            ControlParasList.Add(ParaMode);

            StatusParasList = new ObservableCollection<ParaModel>();

            IsAutoCheckStatus = true;
            AutoCheckTimer.Elapsed += AutoCheckTimer_Elapsed;
            AutoCheckTimer.AutoReset = true;
        }

        private void AutoCheckTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (CANController.m_canstart == 1)
            {
                if (IsAutoCheckStatus)
                {
                    AutoCheckTimer.Stop();
                    if (missCount != 0)
                    {
                        App.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            ConnectStatus = 1;
                        }));
                    }
                    missCount = 1;
                    CANQueueManager manager = CANQueueManager.GetInstance();
                    manager.ConstractMessage(CANFrameType.Status, index: (byte)CANACKIndex.Status);
                    manager.RaiseSendQueueChanged();
                    AutoCheckTimer.Start();
                }
            }
        }

        public void ReveicedACK(CANFrame frame)
        {
            if (IsAutoCheckStatus)
            {
                AutoCheckTimer.Stop();
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    ConnectStatus = 0;
                }));
                missCount = 0;
                AutoCheckTimer.Start();
            }
        }

        public void LoadControlParasList(string fileName)
        {
            using (FileStream fStream = File.Open(fileName, FileMode.Open))
            {
                ControlParasList = xmlControlParasFormat.Deserialize(fStream) as List<ParaModelWithCommandAndReturn>;
                foreach (ParaModel m in ControlParasList)
                {
                    m.IsValueChanged = false;
                }
            }
        }

        public void LoadStatusParasList(string fileName)
        {
            using (FileStream fstream = File.Open(fileName, FileMode.Open))
            {
                StatusParasList = xmlStatusParasFormat.Deserialize(fstream) as ObservableCollection<ParaModel>;
            }
        }

        public void SaveParasList(string fileName)
        {
            using (FileStream fStream = File.Open(fileName, FileMode.Create))
            {
                xmlControlParasFormat.Serialize(fStream, ControlParasList);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertityChanged(string propertityName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertityName));
            }
        }
    }

    class ConnectStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((int)value == 0)
            {
                return Brushes.Green;
            }
            else if ((int)value > 0)
            {
                return Brushes.Red;
            }
            else
            {
                return Brushes.Gray;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class DeviceStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (((DeviceStatusIndex)value) == DeviceStatusIndex.Run)
            {
                return Brushes.Red;
            }
            else if ((DeviceStatusIndex)value == DeviceStatusIndex.Stop)
            {
                return Brushes.Green;
            }
            else if ((DeviceStatusIndex)value == DeviceStatusIndex.Init)
            {
                return Brushes.Yellow;
            }
            else if ((DeviceStatusIndex)value == DeviceStatusIndex.Error)
            {
                return Brushes.Black;
            }
            else
            {
                return Brushes.Gray;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class DeviceModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToInt32(value) - 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToSingle(value) + 1;
        }
    }

}
