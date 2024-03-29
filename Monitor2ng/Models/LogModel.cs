﻿using Monitor2ng.CAN;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace Monitor2ng.Models
{
    public class LogModel : INotifyPropertyChanged
    {
        public ObservableCollection<Tuple<DateTime,MonitorFrame>> ReceivedFrames { get; set; }
        public ObservableCollection<Tuple<DateTime, MonitorFrame>> SentFrames { get; set; }
        public bool EnableLog
        {
            get => enableLog;
            set
            {
                enableLog = value;
                OnPropertityChanged("EnableLog");
            }
        }
        private bool enableLog;

        public int ReceiveCount
        {
            get => receiveCount;
            set
            {
                receiveCount = value;
                OnPropertityChanged("ReceiveCount");
            }
        }
        private int receiveCount;

        public int SendCount
        {
            get => sendCount;
            set
            {
                sendCount = value;
                OnPropertityChanged("SendCount");
            }
        }
        private int sendCount;

        public int ErrorCount
        {
            get => errorCount;
            set
            {
                errorCount = value;
                OnPropertityChanged("ErrorCount");
            }
        }
        private int errorCount;

        public void LogReceiveFrame(MonitorFrame frame)
        {
            if (EnableLog)
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    ReceivedFrames.Add(new Tuple<DateTime, MonitorFrame>(DateTime.Now, frame));
                }));
            }
            ReceiveCount += 1;
            if (frame.Type == FrameType.Invalid)
            {
                ErrorCount += 1;
            }
        }

        public void LogSendFrame(MonitorFrame frame)
        {
            if (EnableLog)
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    SentFrames.Add(new Tuple<DateTime, MonitorFrame>(DateTime.Now, frame));
                }));
            }
            SendCount += 1;
            if (frame.Type == FrameType.Invalid)
            {
                ErrorCount += 1;
            }
        }

        public void Clear()
        {
            ReceivedFrames.Clear();
            SentFrames.Clear();
            ReceiveCount = 0;
            SendCount = 0;
            ErrorCount = 0;
        }

        public LogModel()
        {
            ReceivedFrames = new ObservableCollection<Tuple<DateTime, MonitorFrame>>();
            SentFrames = new ObservableCollection<Tuple<DateTime, MonitorFrame>>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertityChanged(string propertityName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertityName));
        }
    }
}
