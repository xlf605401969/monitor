using Monitor2ng.CAN;
using Monitor2ng.ConfigFilePraser;
using Monitor2ng.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Linq;
using System.Windows.Input;
using System.ComponentModel;
using System.Timers;
using System.Diagnostics;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;
using System.Collections.Specialized;
using System.Collections;
using InteractiveDataDisplay.WPF;
using Microsoft.Win32;

namespace Monitor2ng.ViewModels
{
    public class ControlWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ConfigFileModel configFile;

        private Timer AutoCheckTimer;

        public string WindowTitle { get; set; }

        public int StateCheckInterval { get; set; }
        public string ConfigFilePath { get; set; }
        public CanClient CanClient { get; set; }

        public uint CommunicationId { get; set; }

        public MachineControlModel ControlModel { get; set; }
        public ObservableCollection<VariableModel> States { get; private set; }
        public ObservableCollection<VariableModel> Commands { get; private set; }
        public ObservableCollection<VariableModel> Parameters { get; private set; }
        public GraphDataModel GraphDataModel { get; private set; }
        public LogModel LogModel { get; set; }

        public RelayCommand StartControllerCommand { get; private set; }
        public RelayCommand StopControllerCommand { get; private set; }
        public RelayCommand QueryStateCommand { get; private set; }
        public RelayCommand SendControlCommand { get; private set; }
        public RelayCommand QueryParameterCommand { get; private set; }
        public RelayCommand SendParameterCommand { get; private set; }

        public RelayCommand GraphSettingCommand { get; private set; }
        public RelayCommand GraphReadCommand { get; private set; }
        public RelayCommand GraphLockCommand { get; private set; }
        public RelayCommand GraphUnLockCommand { get; private set; }
        public RelayCommand GraphSaveCommand { get; private set; }

        public RelayCommand ClearLogCommand { get; private set; }

        public bool IsGraphControlEnabled { get { return !GraphReadingFlag; } }
        public bool GraphReadingFlag { get => graphReadingFlag; set { graphReadingFlag = value; OnPropertityChanged("GraphReadingFlag"); OnPropertityChanged("IsGraphControlEnabled"); } }
        private bool graphReadingFlag;
        private List<Tuple<int, float>> tmpGraphData;
        private Timer graphReadTimer = new Timer();
        public LineGraph LineGraph { get; set; }

        private bool IsReceiveStatekAck { get; set; }

        public bool IsAutoCheckEnabled
        {
            get => isAutoCheckEnabled;
            set
            {
                isAutoCheckEnabled = value;
                OnPropertityChanged("IsAutoCheckEnabled");
                OnPropertityChanged("ConnectionState");
            }
        }
        private bool isAutoCheckEnabled;

        public bool IsAutoSendEnabled
        {
            get => isAutoSendEnabled;
            set
            {
                isAutoSendEnabled = value;
                OnPropertityChanged("IsAutoSendEnabled");
            }
        }
        private bool isAutoSendEnabled;

        public ConnectionState ConnectionState
        {
            get
            {
                if (IsAutoCheckEnabled)
                {
                    return connectionState;
                }
                else
                {
                    return ConnectionState.NotConfigured;
                }
            }
            set
            {
                connectionState = value;
                OnPropertityChanged("ConnectionState");
            }
        }
        private ConnectionState connectionState;

        public RunState RunState
        {
            get
            {
                var states = from state in States where state.Id == configFile.StateVariableId select state;
                if (states.Count() > 0)
                {
                    var res = states.First().ReturnValue switch
                    {
                        0 => RunState.Stoped,
                        _ => RunState.Running,
                    };
                    return res;
                }
                else
                {
                    return RunState.NotConfigured;
                }
            }
        }

        public ControlWindowViewModel()
        {
            ControlModel = new MachineControlModel();
            States = new ObservableCollection<VariableModel>();
            Commands = new ObservableCollection<VariableModel>();
            Parameters = new ObservableCollection<VariableModel>();
            GraphDataModel = new GraphDataModel();
            AutoCheckTimer = new Timer();
            LogModel = new LogModel();

            AutoCheckTimer.Elapsed += AutoCheckTimer_Elapsed;
            SendControlCommand = new RelayCommand(SendControlAction);
            QueryStateCommand = new RelayCommand(QueryStatesAction);
            StartControllerCommand = new RelayCommand(StartAction);
            StopControllerCommand = new RelayCommand(StopAction);
            QueryParameterCommand = new RelayCommand(QueryParameterAction);
            SendParameterCommand = new RelayCommand(SendParameterAction);

            GraphSettingCommand = new RelayCommand(GraphSettingAction);
            GraphReadCommand = new RelayCommand(GraphReadAction);
            GraphLockCommand = new RelayCommand(GraphLockAction);
            GraphUnLockCommand = new RelayCommand(GraphUnLockAction);
            GraphSaveCommand = new RelayCommand(GraphSaveAction);
            tmpGraphData = new List<Tuple<int, float>>();
            LineGraph = new LineGraph();
            graphReadTimer.AutoReset = false;
            graphReadTimer.Elapsed += GraphReadTimer_Elapsed;

            ClearLogCommand = new RelayCommand((object para) => { LogModel.Clear(); });
        }

        private void AutoCheckTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (isAutoCheckEnabled)
            {
                if (IsReceiveStatekAck == true)
                {
                    ConnectionState = ConnectionState.Ok;
                }
                else
                {
                    ConnectionState = ConnectionState.Error;
                }
                IsReceiveStatekAck = false;
                QueryStatesAction(null);
            }
        }

        public void LoadConfig(string path)
        {
            string json = File.ReadAllText(path);
            configFile = JsonConvert.DeserializeObject<ConfigFileModel>(json, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Include, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate });
            if (configFile.Modes != null)
            {
                foreach (var pairs in configFile.Modes)
                {
                    ControlModel.Modes.Add(pairs.Key);
                }
                ControlModel.ModeDict = configFile.Modes;
            }

            AutoCheckTimer.Interval = configFile.StateCheckInterval;

            if (configFile.Variables != null)
            {
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
                        model.PropertyChanged += Command_PropertyChanged;
                    }
                    else if (v.Type == "Parameter")
                    {
                        VariableModel model = new VariableModel(v);
                        Parameters.Add(model);
                    }
                }
            }
            
            ControlModel.PropertyChanged += ControlModel_PropertyChanged;

            GraphDataModel.BindVariables(States);
            if (configFile.MaxGraphChannel > 0)
            {
                for (int i = 0; i < configFile.MaxGraphChannel; i++)
                {
                    GraphDataModel.AddChannel(string.Format("Channel: {0}", i));
                }
            }
            else
            {
                GraphDataModel.AddChannel(string.Format("Channel: {0}", 0));
            }
            graphReadTimer.Interval = configFile.GraphReadingTimeout;
            GraphDataModel.PropertyChanged += GraphDataModel_PropertyChanged;
        }

        private void ControlModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var models = from v in Commands where v.Id == configFile.ModeVariableId select v;
            foreach (VariableModel model in models)
            {
                model.Value = ControlModel.SelectedModeValue;
            }
        }

        private void Command_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (isAutoSendEnabled && e.PropertyName == "Value")
            {
                var v = sender as VariableModel;
                Debug.WriteLine("Command Changed: " + v.Name);
                MonitorFrame frame = MonitorFrameBuilder.ControlFrame(v);
                CanClient.Transmit(frame, CommunicationId);
            }
        }

        public void StartCanJob()
        {
            CanClient.Register(CommunicationId, (RawCanFrame frame) =>
            {
                Debug.WriteLine("Recceive: " + frame.ToString());
                HandleReceiveFrame(MonitorFrameBuilder.FromRawFrame(frame));
            });
            CanClient.SendHook += (RawCanFrame frame) => { MonitorFrame f1 = MonitorFrameBuilder.FromRawFrame(frame); LogModel.LogSendFrame(f1); };
            CanClient.Start();
            AutoCheckTimer.Start();
        }

        public void HandleReceiveFrame(MonitorFrame frame)
        {
            LogModel.LogReceiveFrame(frame);
            switch (frame.Type)
            {
                case FrameType.Control:
                    break;
                case FrameType.Value:
                    ApplyReturnValue(frame);
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
                    HandleAck(frame);
                    break;
                case FrameType.Query:
                    break;
                case FrameType.GraphData:
                    HandleGraphDataFrame(frame);
                    break;
            }
        }

        private void ApplyReturnValue(MonitorFrame frame)
        {
            var models = (from model in States.Concat(Commands).Concat(Parameters) where model.Id == frame.Index select model);
            foreach (VariableModel v in models)
            {
                v.ReturnValue = frame.AutoValue;
                if (v.Type == "Parameter")
                {
                    v.Value = v.ReturnValue;
                }
                if (v.Type == "State" || v.Id == configFile.StateVariableId)
                {
                    OnPropertityChanged("RunState");
                }
                v.IsValueModified = false;
            }
        }

        private void HandleGraphControlFrame(MonitorFrame frame)
        {
            if (GraphReadingFlag)
            {
                switch ((GraphCmd)frame.Command)
                {
                    case GraphCmd.DataInfo:
                        break;
                    case GraphCmd.EndOfData:
                        EndGraphReading();
                        break;
                }
            }
        }

        private void HandleGraphDataFrame(MonitorFrame frame)
        {
            if (GraphReadingFlag)
            {
                if (frame.Index == GraphDataModel.SelectedChannelIndex)
                {
                    lock (tmpGraphData)
                    {
                        tmpGraphData.Add(new Tuple<int, float>(frame.GraphDataIndex, frame.GraphData));
                    }
                }
            }
        }

        private void HandleAck(MonitorFrame frame)
        {
            switch ((AckCmd)frame.Command)
            {
                case AckCmd.StateAck:
                    IsReceiveStatekAck = true;
                    break;
            }
        }

        private void SendControlAction(object parameter)
        {
            foreach (VariableModel v in Commands)
            {
                if (v.IsValueModified)
                {
                    MonitorFrame frame = MonitorFrameBuilder.ControlFrame(v);
                    CanClient.Transmit(frame, CommunicationId);
                    v.IsValueModified = false;
                }
            }
        }

        private void StartAction(object parameter)
        {
            CanClient.Transmit(MonitorFrameBuilder.StartFrame(), CommunicationId);
        }

        private void StopAction(object parameter)
        {
            CanClient.Transmit(MonitorFrameBuilder.StopFrame(), CommunicationId);
        }

        private void QueryStatesAction(object parameter)
        {
            MonitorFrame frame = MonitorFrameBuilder.QueryFrame(QueryCmd.StateQuery);
            CanClient.Transmit(frame, CommunicationId);
        }

        private void SendParameterAction(object parameter)
        {
            foreach (VariableModel v in Parameters)
            {
                if (v.IsValueModified == true)
                {
                    var frame = MonitorFrameBuilder.ValueFrame(v);
                    CanClient.Transmit(frame, CommunicationId);
                    v.IsValueModified = false;
                }
            }
        }

        private void QueryParameterAction(object parameter)
        {
            MonitorFrame frame = MonitorFrameBuilder.QueryFrame(QueryCmd.ParameterQuery);
            CanClient.Transmit(frame, CommunicationId);
        }

        private void GraphSettingAction(object parameter)
        {
            MonitorFrame frame = MonitorFrameBuilder.GraphControlFrame((byte)GraphDataModel.SelectedChannelIndex, GraphCmd.SetRatio, GraphDataModel.SamplingRatio);
            CanClient.Transmit(frame, CommunicationId);
            frame = MonitorFrameBuilder.GraphControlFrame((byte)GraphDataModel.SelectedChannelIndex, GraphCmd.SetLogVariable, GraphDataModel.SelectedLogVariable.Id);
            CanClient.Transmit(frame, CommunicationId);
        }

        private void GraphReadAction(object parameter)
        {
            if (GraphReadingFlag == false)
            {
                GraphReadingFlag = true;
                GraphDataModel.SelectedChannelData.Clear();
                MonitorFrame frame = MonitorFrameBuilder.GraphControlFrame((byte)GraphDataModel.SelectedChannelIndex, GraphCmd.Read);
                CanClient.Transmit(frame, CommunicationId);
                graphReadTimer.Start();
            }
        }

        private void GraphLockAction(object sender)
        {
            MonitorFrame frame = MonitorFrameBuilder.GraphControlFrame((byte)GraphDataModel.SelectedChannelIndex, GraphCmd.Lock);
            CanClient.Transmit(frame, CommunicationId);
        }

        private void GraphUnLockAction(object sender)
        {
            MonitorFrame frame = MonitorFrameBuilder.GraphControlFrame((byte)GraphDataModel.SelectedChannelIndex, GraphCmd.UnLock);
            CanClient.Transmit(frame, CommunicationId);
        }

        private void GraphSaveAction(object sender)
        {
            if (GraphDataModel.DisplayChannelData.Count > 0)
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Filter = "csv files|*.csv";
                dialog.DefaultExt = "csv";
                if (dialog.ShowDialog() == true)
                {
                    string fileName = dialog.FileName;
                    if (File.Exists(fileName))
                    {
                        try
                        {
                            File.Delete(fileName);
                        }
                        catch
                        {

                        }
                    }
                    using (StreamWriter writer = new StreamWriter(fileName, false))
                    {
                        try
                        {
                            foreach (var v in GraphDataModel.DisplayChannelData)
                            {
                                writer.WriteLine("{0},\t\t{1},", v.Item1, v.Item2);
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }
            
        }

        private void GraphReadTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            EndGraphReading();
        }

        private void EndGraphReading()
        {
            graphReadTimer.Stop();
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                GraphReadingFlag = false;
                lock (tmpGraphData)
                {
                    tmpGraphData.Sort((x, y) => { return x.Item1.CompareTo(y.Item1); });

                    foreach (var v in tmpGraphData)
                    {
                        GraphDataModel.SelectedChannelData.Add(v);
                    }
                    tmpGraphData.Clear();
                }
                var x = from v in GraphDataModel.SelectedChannelData select v.Item1;
                var y = from v in GraphDataModel.SelectedChannelData select v.Item2;
                LineGraph.Plot(x, y);
            }));
        }

        private void GraphDataModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedChannelIndex")
            {
                var x = from v in GraphDataModel.SelectedChannelData select v.Item1;
                var y = from v in GraphDataModel.SelectedChannelData select v.Item2;
                LineGraph.Plot(x, y);
                LineGraph.Description = GraphDataModel.SelectedChannel;
            }
        }

        public void Close()
        {
            CanClient.DisConnect();
            AutoCheckTimer.Stop();
        }

        protected void OnPropertityChanged(string propertityName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertityName));
        }
    }

    public enum ConnectionState
    {
        NotConfigured = 0,
        Ok = 1,
        Error = 2,
    }

    public enum RunState
    {
        NotConfigured = 0,
        Stoped = 1,
        Running = 2,
    }

    public class ValueModificationColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool modified = (bool)value;
            if (modified)
            {
                return Brushes.Red;
            }
            else
            {
                var b = new SolidColorBrush(Color.FromArgb((byte)(87.0 / 100 * 255), 0, 0, 0));
                return b;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ConnectionStateColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ConnectionState state = (ConnectionState) value;
            Brush brush = state switch
            {
                ConnectionState.Ok => Brushes.Green,
                ConnectionState.Error => Brushes.Red,
                _ => Brushes.Gray,
            };
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RunStateColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            RunState state = (RunState)value;
            Brush brush = state switch
            {
                RunState.Stoped => Brushes.Green,
                RunState.Running => Brushes.Red,
                _ => Brushes.Gray,
            };
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IncreaseCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            VariableModel v = parameter as VariableModel;
            v.Value += v.DeltaStep;
        }
    }

    public class DecreaseCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            VariableModel v = parameter as VariableModel;
            v.Value -= v.DeltaStep;
        }
    }
}
