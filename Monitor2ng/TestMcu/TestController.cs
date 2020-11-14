using Monitor2ng.CAN;
using Monitor2ng.ConfigFilePraser;
using Monitor2ng.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Security.Cryptography;

namespace Monitor2ng.TestMcu
{
    public class TestController : INotifyPropertyChanged
    {
        public static uint StartCommunicationId = 0x100;

        public uint CommunicationId = 0x100;

        public uint DataSampleRatio = 1;
        public uint DataLogId = 0xff;

        public ObservableCollection<VariableModel> Commands { get; set; }
        public ObservableCollection<VariableModel> States { get; set; }
        public ObservableCollection<VariableModel> Parameters { get; set; }

        public string ControlMode
        {
            get => controlMode;
            set
            {
                controlMode = value;
                OnPropertityChanged("ControlMode");
            }
        }
        private string controlMode;

        public string RunMode
        {
            get => runMode;
            set
            {
                runMode = value;
                OnPropertityChanged("ControlMode");
            }
        }
        private string runMode;

        public Queue<MonitorFrame> SendQueue { get; set; }

        public ConfigFileModel configFile;

        public event PropertyChangedEventHandler PropertyChanged;

        public void ReceiveFrame(RawCanFrame rawFrame)
        {
            lock (SendQueue)
            {
                MonitorFrame frame = MonitorFrameBuilder.FromRawFrame(rawFrame);
                switch (frame.Type)
                {
                    case FrameType.Control:
                        HandleControlFrame(frame);
                        break;
                    case FrameType.Value:
                        HandleParameterFrame(frame);
                        break;
                    case FrameType.GraphControl:
                        HandleGraphControlFrame(frame);
                        break;
                    case FrameType.StateQuery:
                        break;
                    case FrameType.Start:
                        break;
                    case FrameType.Stop:
                        break;
                    case FrameType.ParameterQuery:
                        break;
                    case FrameType.Ack:
                        break;
                    case FrameType.Query:
                        HandleQueryFrame(frame);
                        break;
                }
            }
        }

        public void HandleParameterFrame(MonitorFrame frame)
        {
            var models = from v in Parameters where v.Id == frame.Index select v;
            foreach (VariableModel model in models)
            {
                model.Value = frame.AutoValue;
            }
        }

        public void HandleControlFrame(MonitorFrame frame)
        {
            var models = from v in Commands where v.Id == frame.Index select v;
            foreach (VariableModel model in models)
            {
                model.Value = frame.AutoValue;
                MonitorFrame f2 = MonitorFrameBuilder.ValueFrame(model);
                SendQueue.Enqueue(f2);
            }
        }

        public void HandleQueryFrame(MonitorFrame frame)
        {
            switch((QueryCmd)frame.Command)
            {
                case QueryCmd.CommandQuery:
                    SendQueue.Enqueue(MonitorFrameBuilder.AckFrame(QueryCmd.CommandQuery));
                    foreach (VariableModel model in Commands)
                    {
                        MonitorFrame f2 = MonitorFrameBuilder.ValueFrame(model);
                        SendQueue.Enqueue(f2);
                    }
                    break;
                case QueryCmd.ParameterQuery:
                    SendQueue.Enqueue(MonitorFrameBuilder.AckFrame(QueryCmd.ParameterQuery));
                    foreach (VariableModel model in Parameters)
                    {
                        MonitorFrame f2 = MonitorFrameBuilder.ValueFrame(model);
                        SendQueue.Enqueue(f2);
                    }
                    break;
                case QueryCmd.StateQuery:
                    SendQueue.Enqueue(MonitorFrameBuilder.AckFrame(QueryCmd.StateQuery));
                    foreach (VariableModel model in States)
                    {
                        MonitorFrame f2 = MonitorFrameBuilder.ValueFrame(model);
                        SendQueue.Enqueue(f2);
                    }
                    break;
            }
        }

        public void HandleGraphControlFrame(MonitorFrame frame)
        {
            GraphCmd cmd = (GraphCmd)frame.Command;
            switch (cmd)
            {
                case GraphCmd.Read:
                    for (int i = 0; i < 100; i++)
                    {
                        MonitorFrame f;
                        if (DataLogId == 1)
                        {
                            f = MonitorFrameBuilder.GraphDataFrame((ushort)i, frame.IntIndexValue, MathF.Sin(0.02F*MathF.PI*DataSampleRatio*i));
                        }
                        else
                        {
                            f = MonitorFrameBuilder.GraphDataFrame((ushort)i, frame.IntIndexValue, RandomNumberGenerator.GetInt32(100));
                        }
                        SendQueue.Enqueue(f);
                    }
                    var f2 = MonitorFrameBuilder.GraphControlFrame(frame.IntIndexValue, GraphCmd.EndOfData);
                    SendQueue.Enqueue(f2);
                    break;
                case GraphCmd.SetRatio:
                    DataSampleRatio = (uint)frame.Int32Value;
                    break;
                case GraphCmd.SetLogVariable:
                    DataLogId = (uint)frame.Int32Value;
                    break;
            }
        }

        public void LoadDefault()
        {
            string json = File.ReadAllText("./config1.json");
            configFile = JsonConvert.DeserializeObject<ConfigFileModel>(json, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Populate });

            foreach (var v in configFile.Variables)
            {
                if (v.Type == "State")
                {
                    VariableModel model = new VariableModel(v);
                    States.Add(model);
                }
                else if (v.Type == "Command")
                {
                    VariableModel model = new VariableModel(v);
                    Commands.Add(model);
                }
                else if (v.Type == "Parameter")
                {
                    VariableModel model = new VariableModel(v);
                    Parameters.Add(model);
                }
            }

            CommunicationId = StartCommunicationId;
            StartCommunicationId += 1;

            TestControllerWindow testControllerWindow = new TestControllerWindow();
            testControllerWindow.DataContext = this;
            testControllerWindow.Show();
        }

        public TestController()
        {
            Commands = new ObservableCollection<VariableModel>();
            States = new ObservableCollection<VariableModel>();
            Parameters = new ObservableCollection<VariableModel>();
            SendQueue = new Queue<MonitorFrame>();
        }

        protected void OnPropertityChanged(string propertityName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertityName));
        }
    }
}
