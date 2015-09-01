﻿using System;
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
        public XmlSerializer xmlControlParasFormat = new XmlSerializer(typeof(List<ParaModelWithCommand>));
        public XmlSerializer xmlStatusParasFormat = new XmlSerializer(typeof(ObservableCollection<ParaModel>));

        public ParaModelWithCommand Para1 { get; private set; }
        public ParaModelWithCommand Para2 { get; private set; }
        public ParaModelWithCommand Para3 { get; private set; }
        public ParaModelWithCommand Para4 { get; private set; }
        public ParaModelWithCommand ParaMode { get; private set; }
        public ParaModelWithCommand ParaDirection { get; private set; }

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

        public int missCount; 

        public List<ParaModelWithCommand> ControlParasList { get; private set; }

        public ObservableCollection<ParaModel> StatusParasList { get; private set; }

        public ControlTabViewModel()
        {
            Para1 = new ParaModelWithCommand();
            Para2 = new ParaModelWithCommand();
            Para3 = new ParaModelWithCommand();
            Para4 = new ParaModelWithCommand();
            ParaDirection = new ParaModelWithCommand();
            ParaMode = new ParaModelWithCommand();

            ControlParasList = new List<ParaModelWithCommand>();
            ControlParasList.Add(Para1);
            ControlParasList.Add(Para2);
            ControlParasList.Add(Para3);
            ControlParasList.Add(Para4);
            ControlParasList.Add(ParaDirection);
            ControlParasList.Add(ParaMode);

            StatusParasList = new ObservableCollection<ParaModel>();

            AutoCheckTimer.Elapsed += AutoCheckTimer_Elapsed;
            AutoCheckTimer.AutoReset = true;
        }

        private void AutoCheckTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (CANController.m_canstart == 1)
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
                if (IsAutoCheckStatus)
                {
                    AutoCheckTimer.Start();
                }
            }
        }

        public void ReveicedACK(CANFrame frame)
        {
            AutoCheckTimer.Stop();
            if ((CANACKIndex)frame.FrameIndex == CANACKIndex.Status)
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    ConnectStatus = 0;
                }));
                missCount = 0;
            }
            if (IsAutoCheckStatus)
            {
                AutoCheckTimer.Start();
            }
        }

        public void LoadControlParasList(string fileName)
        {
            using (FileStream fStream = File.Open(fileName, FileMode.Open))
            {
                ControlParasList = xmlControlParasFormat.Deserialize(fStream) as List<ParaModelWithCommand>;
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
