using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Monitor2.ViewModels;
using Monitor2.Models;
using Monitor2.CAN;
using InteractiveDataDisplay.WPF;

namespace Monitor2
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public ParasListViewModel ParasListVM { get; set; }
        public ControlTabViewModel ControlTabVM { get; set; }

        public GraphTabViewModel GraphTabVM { get; set; }

        public Dictionary<int,double> GraphXY { get; set; }
        private LineGraph ScopeLine = new LineGraph();

        CANQueueManager canQueueManager = new CANQueueManager();
        AutoControlWindow AutoControlWindow = new Monitor2.AutoControlWindow();

        public MainWindow()
        {
            InitializeComponent();

            Closing += ((object sender, System.ComponentModel.CancelEventArgs e) => { AutoControlWindow.Close(); });
            ParasListVM = new ParasListViewModel();
            ParasListVM.LoadParasList("Paras.config");

            ControlTabVM = new ControlTabViewModel();
            ControlTabVM.LoadControlParasList("Command.config");
            ControlTabVM.LoadStatusParasList("Status.config");

            GraphTabVM = new GraphTabViewModel();

            AutoControlWindow.ParaList = ParasListVM.parasList.ToList();
            AutoControlWindow.StatusList = ControlTabVM.StatusParasList.ToList();

            //绑定参数列表
            ParasListView.DataContext = ParasListVM.parasList;

            ControlTabView.DataContext = ControlTabVM;

            SwitchStackPanel.DataContext = ParasListVM;

            GraphTabView.DataContext = GraphTabVM;

            //绑定日志面板
            LogTable.DataContext = canQueueManager;

            GraphXY = new Dictionary<int, double>();
            ScopeLine.Stroke = new SolidColorBrush(Color.FromRgb(29, 80, 162));
            GraphScope.Content = ScopeLine;

            //设置波特率选项
            foreach (string s in ControlCAN.BaudrateDic.Keys)
            {
                BaudRateSelection.Items.Add(s);
            }
            BaudRateSelection.SelectedIndex = 10;

            //绑定接受队列事件处理程序
            canQueueManager.OnReceiveQueueChanged += HandleReceiveMessage;
            GraphTabVM.ChannelData.CollectionChanged += ChannelData_CollectionChanged;
        }

        private void ChannelData_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateGraph();
        }

        private void ConnectDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            CANController.FilterId = AccCodeInput.Text;
            CANController.FilterMask = AccMaskInput.Text;
            CANController.TargetMode = (CanMode)ModeSelection.SelectedIndex;
            CANController.MessageId = IDInput.Text;
            CANController.Baudrate = BaudRateSelection.Text;
            
            if (!CANController.Initialized)
            {
                CANController.Initialize();
                CANController.Configuration();
            }
            else
            {
                CANController.Close();
            }

            //当设备处于连接状态时无法更改参数
            if (CANController.Initialized == false)
            {
                ConnectDeviceButton.Content = "连接";
                DeviceSelection.IsEnabled = true;
                IndexSelection.IsEnabled = true;
                PortSelection.IsEnabled = true;
                ModeSelection.IsEnabled = true;
                AccCodeInput.IsEnabled = true;
                AccMaskInput.IsEnabled = true;
                BaudRateSelection.IsEnabled = true;
            }
            else
            {
                ConnectDeviceButton.Content = "断开";
                DeviceSelection.IsEnabled = false;
                IndexSelection.IsEnabled = false;
                PortSelection.IsEnabled = false;
                ModeSelection.IsEnabled = false;
                AccCodeInput.IsEnabled = false;
                AccMaskInput.IsEnabled = false;
                BaudRateSelection.IsEnabled = false;
            }
            ConnectDeviceButton.Foreground = ControlCAN.m_bOpen == 0 ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
            StartCANButton_Click(sender, e);
        }

        private void StartCANButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CANController.Started)
            {
                if (CANController.Start() == 0)
                { 
                    StartCANButton.Content = "停止";
                }
            }
            else
            {
                if (CANController.Stop() == 0)
                {
                    StartCANButton.Content = "启动";
                }
            }
        }

        private void DeviceSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (DeviceSelection.SelectedIndex)
            {
                case 0:
                    CANController.TargetDevice = CanDevice.CANUSB;
                    AccCodeInput.Text = "0x20000000";
                    AccMaskInput.Text = "0x00000000";
                    break;
                case 1:
                    CANController.TargetDevice = CanDevice.CANUSB2;
                    if (AccCodeInput != null)
                    {
                        AccCodeInput.Text = "0x20000000";
                        AccMaskInput.Text = "0x00000000";
                    }
                    break;
                case 2:
                    CANController.TargetDevice = CanDevice.PCAN;
                    AccCodeInput.Text = "0x04000000";
                    AccMaskInput.Text = "0x00000000";
                    break;
            } 
        }

        private void PortSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (PortSelection.SelectedIndex)
            {
                case 0:
                    CANController.DeviceId = 0;
                    break;
            }
        }

        private void IndexSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CANController.ChannelId = (uint)IndexSelection.SelectedIndex;
        }

        private void UpdateParaListButton_Click(object sender, RoutedEventArgs e)
        {
            if (CANController.Started)
            {
                int count = 0;
                foreach (Models.ParaModel m in ParasListVM.parasList)
                {
                    if (m.IsValueChanged)
                    {
                        canQueueManager.ConstractMessage(m, CANFrameType.Control);
                        count++;
                    }
                    m.IsValueChanged = false;
                }
                if (count != 0)
                {
                    canQueueManager.RaiseSendQueueChanged();
                }
            }
            else
            {
                MessageBox.Show("请先启动CAN控制器");
            }
        }

        private void IDInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void ReadParaListButton_Click(object sender, RoutedEventArgs e)
        {
            if (CANController.Started)
            {
                canQueueManager.ConstractMessage(CANFrameType.Query);
                canQueueManager.RaiseSendQueueChanged();
            }
            else
            {
                MessageBox.Show("请先启动CAN控制器");
            }
        }

        private void CleanLogButton_Click(object sender, RoutedEventArgs e)
        {
            canQueueManager.LogList.Clear();
            canQueueManager.ReceiveCount = 0;
            canQueueManager.SendCount = 0;
        }

        //事件处理程序，当接收队列有新元素时触发；
        private void HandleReceiveMessage(object sender, EventArgs e)
        {
            while(canQueueManager.ReceiveQueue.Count != 0)
            {
                CANFrame frame = canQueueManager.ReceiveQueue.Dequeue();
                if (ControlCAN.m_canmode != 2)
                {
                    HandleCANFrame(frame);
                }

            }
        }

        public void HandleCANFrame(CANFrame frame)
        {
            switch((CANFrameType)frame.FrameType)
            {
                case CANFrameType.Para:
                    App.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        foreach (ParaModel m in ParasListVM.parasList)
                        {
                            if (m.Index == frame.FrameIndex)
                            {
                                if (m.Type == 3)
                                    m.Value = frame.IntValue;
                                if (m.Type == 2)
                                    m.Value = frame.Value;
                                if (m.Type == 1)
                                    m.Value = frame.IndexValue;
                                m.IsValueChanged = false;
                            }
                        }
                        foreach (ParaModel m in ControlTabVM.StatusParasList)
                        {
                            if (m.Index == frame.FrameIndex)
                            {
                                if (m.Type == 3)
                                    m.Value = frame.IntValue;
                                if (m.Type == 2)
                                    m.Value = frame.Value;
                                if (m.Type == 1)
                                    m.Value = frame.IndexValue;
                                m.IsValueChanged = false;
                            }
                        }
                        foreach (ParaModelWithCommandAndReturn m in ControlTabVM.ControlParasList)
                        {
                            if (m.Index == frame.FrameIndex)
                            {
                                if (m.Type == 3)
                                    m.Value = frame.IntValue;
                                if (m.Type == 2)
                                    m.Value = frame.Value;
                                if (m.Type == 1)
                                    m.Value = frame.IndexValue;
                                m.ReturnValue = m.Value;
                                m.IsValueChanged = false;
                            }
                        }
                    }));
                    break;
                case CANFrameType.Graph:
                    App.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        int index = ((frame.FrameIndex & 0x0f) << 8) + frame.IndexValue;
                        if (index == 0)
                        {
                            return;
                        }
                        else
                        {
                            if (frame.DataType == 0x02)
                            {
                                GraphTabVM.ChannelData.Add(new GraphDataModel(index, frame.Value));
                            }
                            else if (frame.DataType == 0x03)
                            {
                                GraphTabVM.ChannelData.Add(new GraphDataModel(index, frame.IntValue));
                            }
                        }
                    }));
                    break;
                case CANFrameType.Control:
                    throw new NotImplementedException();
                case CANFrameType.Query:
                    throw new NotImplementedException();
                case CANFrameType.Status:
                    break;
                case CANFrameType.Start:
                    throw new NotImplementedException();
                case CANFrameType.Stop:
                    throw new NotImplementedException();
                case CANFrameType.ACK:
                    App.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        ControlTabVM.DeviceStatus = frame.IndexValue;
                    }));
                    ControlTabVM.ReveicedACK(frame);
                    break;
            }
        }

        private void ParaMinusButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            ParaModelWithCommand m = button.DataContext as ParaModelWithCommand;
            if (m.ValueChangeStep != 0)
            {
                m.Value -= m.ValueChangeStep;
            }
            if(ControlTabVM.IsAutoSend && CANController.Started)
            {
                canQueueManager.ConstractMessage(m, CANFrameType.Control);
                canQueueManager.RaiseSendQueueChanged();
                m.IsValueChanged = false;
            }
        }

        private void ParaPlusButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            ParaModelWithCommand m = button.DataContext as ParaModelWithCommand;
            if (m.ValueChangeStep != 0)
            {
                m.Value += m.ValueChangeStep;
            }
            if(ControlTabVM.IsAutoSend && CANController.Started)
            {
                canQueueManager.ConstractMessage(m, CANFrameType.Control);
                canQueueManager.RaiseSendQueueChanged();
                m.IsValueChanged = false;
            }
        }

        private void ParasPanelSaveButton_Click(object sender, RoutedEventArgs e)
        {
            ControlTabVM.SaveParasList("paras2.config");
            MessageBox.Show("成功");
        }

        private void ParasPanelConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (CANController.Started)
            {
                int count = 0;
                foreach (Models.ParaModel m in ControlTabVM.ControlParasList)
                {
                    if (m.IsValueChanged || true)
                    {
                        canQueueManager.ConstractMessage(m, CANFrameType.Control);
                        count++;
                    }
                    m.IsValueChanged = false;
                }
                if (count != 0)
                {
                    canQueueManager.RaiseSendQueueChanged();
                }
            }
            else
            {
                MessageBox.Show("请先启动CAN控制器", "错误");
            }
        }

        private void ModeSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ControlCAN.m_canmode = (uint)ModeSelection.SelectedIndex;
        }

        private void MotorStartButton_Click(object sender, RoutedEventArgs e)
        {
            if (CANController.Started)
            {
                canQueueManager.ConstractMessage(CANFrameType.Start);
                canQueueManager.RaiseSendQueueChanged();
            }
            else
            {
                MessageBox.Show("请先启动CAN控制器", "错误");
            }
        }

        private void MotorStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (CANController.Started)
            {
                canQueueManager.ConstractMessage(CANFrameType.Stop);
                canQueueManager.RaiseSendQueueChanged();
            }
            else
            {
                MessageBox.Show("请先启动CAN控制器", "错误");
            }           
        }

        private void RunModeButton_Click(object sender, RoutedEventArgs e)
        {
            if (CANController.Started)
            {
                canQueueManager.ConstractMessage(ControlTabVM.ControlParasList[4], CANFrameType.Control);
                canQueueManager.RaiseSendQueueChanged();
            }
            else
            {
                MessageBox.Show("请先启动CAN控制器", "错误");
            }
        }

        private void StatusCheckButton_Click(object sender, RoutedEventArgs e)
        {
            if (CANController.Started)
            {
                CANQueueManager manager = CANQueueManager.GetInstance();
                manager.ConstractMessage(CANFrameType.Status, index: (byte)CANACKIndex.Status);
                manager.RaiseSendQueueChanged();
            }
            else
            {
                MessageBox.Show("请先启动CAN控制器", "错误");
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (CANController.Started)
            {
                CANQueueManager manager = CANQueueManager.GetInstance();
                ParaModel m = (from p in ParasListVM.parasList where p.Name == "Reset" select p).First();
                m.Value = 1;
                manager.ConstractMessage(m, CANFrameType.Control);
                manager.RaiseSendQueueChanged();
            }
        }

        private void RunModeSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CANController.Started)
            {
                canQueueManager.ConstractMessage(ControlTabVM.ControlParasList[4], CANFrameType.Control);
                canQueueManager.RaiseSendQueueChanged();
            }
        }

        private void OpenAC_Click(object sender, RoutedEventArgs e)
        {
            AutoControlWindow.Show();
        }

        private void ReadGraph_Click(object sender, RoutedEventArgs e)
        {
            canQueueManager.ConstractGraphMessage(GraphTabVM.SelectedChannel, GraphCmd.Read);
            canQueueManager.RaiseSendQueueChanged();
            GraphTabVM.ChannelData.Clear();
        }

        private void LockGraph_Click(object sender, RoutedEventArgs e)
        {
            canQueueManager.ConstractGraphMessage(GraphTabVM.SelectedChannel, GraphCmd.Lock);
            canQueueManager.RaiseSendQueueChanged();
        }

        private void UnlockGraph_Click(object sender, RoutedEventArgs e)
        {
            canQueueManager.ConstractGraphMessage(GraphTabVM.SelectedChannel, GraphCmd.Unlock);
            canQueueManager.RaiseSendQueueChanged();
        }

        private void SaveGraphData_Click(object sender, RoutedEventArgs e)
        {
            GraphTabVM.SaveGraphData();
        }

        private void GraphScale_Click(object sender, RoutedEventArgs e)
        {
            //double marginX, marginY, minX, minY, maxX, maxY;
            //marginX = Math.Abs(GraphXY.Keys.Max() - GraphXY.Keys.Min()) * 0.05;
            //marginY = Math.Abs(GraphXY.Values.Max() - GraphXY.Values.Min()) * 0.05;
            //minX = GraphXY.Keys.Min() - marginX;
            //minY = GraphXY.Values.Min() - marginY;
            //maxX = GraphXY.Keys.Max() + marginX;
            //maxY = GraphXY.Values.Max() + marginY;
            //ScopeLine.SetPlotRect(new DataRect(minX, minY, maxX, maxY));
            ScopeLine.IsAutoFitEnabled = true;
        }

        private void UpdateGraph()
        {
            GraphXY.Clear();
            foreach (var m in GraphTabVM.ChannelData)
            {
                GraphXY.Add(m.Index, m.Value);
            }
            if (GraphXY.Count == 0)
            {
                ScopeLine.IsAutoFitEnabled = true;
            }
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ScopeLine.Plot(GraphXY.Keys, GraphXY.Values);
            }));
        }
    }
}
