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

namespace Monitor2
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public ParasListViewModel ParasListVM { get; set; }
        public ControlTabViewModel ControlTabVM { get; set; }

        CANQueueManager canQueueManager = new CANQueueManager();

        public MainWindow()
        {
            InitializeComponent();
            ParasListVM = new ParasListViewModel();
            ParasListVM.LoadParasList("paras.config");

            ControlTabVM = new ControlTabViewModel();
            ControlTabVM.LoadControlParasList("paras2.config");
            ControlTabVM.LoadStatusParasList("statuspara.config");

            //绑定参数列表
            ParasListView.DataContext = ParasListVM.parasList;

            ControlTabView.DataContext = ControlTabVM;

            //绑定日志面板
            LogTable.DataContext = canQueueManager;

            //设置波特率选项
            foreach(string s in CANController.BaudrateDic.Keys)
            {
                BaudRateSelection.Items.Add(s);
            }
            BaudRateSelection.SelectedIndex = 10;

            //绑定接受队列事件处理程序
            canQueueManager.OnReceiveQueueChanged += HandleReceiveMessage;

        }

        private void ConnectDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            //操作硬件前需先获得互斥信号量，以避免冲突
            CANController.CANControllerMutex.WaitOne();
            if (CANController.m_bOpen == 1)
            {
                CANController.VCI_CloseDevice(CANController.m_devtype, CANController.m_devind);
                CANController.m_bOpen = 0;
            }
            else
            {
                CANController.m_canind = (UInt32)PortSelection.SelectedIndex;
                if (CANController.VCI_OpenDevice(CANController.m_devtype, CANController.m_devind, 0) != 0)
                {
                    CANController.m_bOpen = 1;
                    CANController.VCI_INIT_CONFIG config = new CANController.VCI_INIT_CONFIG();
                    try
                    {
                        config.AccCode = System.Convert.ToUInt32(AccCodeInput.Text, 16);
                        config.AccMask = System.Convert.ToUInt32(AccMaskInput.Text, 16);
                        config.Filter = (Byte)1;
                        config.Mode = (Byte)ModeSelection.SelectedIndex;
                        config.Timing0 = CANController.BaudrateDic[BaudRateSelection.SelectedItem as string][0];
                        config.Timing1 = CANController.BaudrateDic[BaudRateSelection.SelectedItem as string][1];
                        CANController.VCI_InitCAN(CANController.m_devtype, CANController.m_devind, CANController.m_canind, ref config);
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show("初始化设备失败,请检参数是否正确", "错误",
                                MessageBoxButton.OK);
                        CANController.VCI_CloseDevice(CANController.m_devtype, CANController.m_devind);
                        CANController.m_bOpen = 0;
                    }
                }
                else
                {
                    MessageBox.Show("打开设备失败，请检查设备类型及索引号是否正确", "错误",
                        MessageBoxButton.OK);
                }
            }
            CANController.CANControllerMutex.ReleaseMutex();

            //当设备处于连接状态时无法更改参数
            if (CANController.m_bOpen == 0)
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
            ConnectDeviceButton.Foreground = CANController.m_bOpen == 0 ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
            StartCANButton_Click(sender, e);
        }

        private void StartCANButton_Click(object sender, RoutedEventArgs e)
        {
            //获取互斥信号量
            CANController.CANControllerMutex.WaitOne();
            if (CANController.m_bOpen == 0)
            {
                CANController.m_canstart = 0;

                //设备启动时无法更改发送参数
                IDInput.IsEnabled = true;
            }
            else
            {
                if (CANController.m_canstart == 0)
                {
                    CANController.VCI_StartCAN(CANController.m_devtype, CANController.m_devind, CANController.m_canind);
                    CANController.m_canstart = 1;
                    IDInput.IsEnabled = false;

                }
                else
                {
                    CANController.VCI_ResetCAN(CANController.m_devtype, CANController.m_devind, CANController.m_canind);
                    CANController.m_canstart = 0;
                    IDInput.IsEnabled = true;
                }
            }
            CANController.CANControllerMutex.ReleaseMutex();
            StartCANButton.Content = CANController.m_canstart == 0 ? "启动" : "停止";
            if (CANController.m_bOpen == 1 && CANController.m_canstart == 1)
            {
                CANController.ReceiveTimer.Enabled = true;
            }
            else
            {
                CANController.ReceiveTimer.Enabled = false;
            }
        }

        private void DeviceSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (DeviceSelection.SelectedIndex)
            {
                case 0:
                    CANController.m_devtype = CANController.DEV_USBCAN;
                    break;
                case 1:
                    CANController.m_devtype = CANController.DEV_USBCAN2;
                    break;
            } 
        }

        private void PortSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CANController.m_canind = (uint)PortSelection.SelectedIndex;
        }

        private void IndexSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CANController.m_devind = (uint)IndexSelection.SelectedIndex;
        }

        private void UpdateParaListButton_Click(object sender, RoutedEventArgs e)
        {
            if (CANController.m_canstart == 1)
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
            CANController.m_ID = Convert.ToUInt32(IDInput.Text, 16);
        }

        private void ReadParaListButton_Click(object sender, RoutedEventArgs e)
        {
            if (CANController.m_canstart == 1)
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
                if (CANController.m_canmode != 2)
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
                                m.Value = frame.Value;
                                m.IsValueChanged = false;
                            }
                        }
                        foreach (ParaModel m in ControlTabVM.StatusParasList)
                        {
                            if (m.Index == frame.FrameIndex)
                            {
                                m.Value = frame.Value;
                                m.IsValueChanged = false;
                            }
                        }
                        foreach (ParaModelWithCommand m in ControlTabVM.ControlParasList)
                        {
                            if (m.Index == frame.FrameIndex)
                            {
                                m.Value = frame.Value;
                                m.IsValueChanged = false;
                            }
                        }
                    }));
                    break;
                case CANFrameType.Graph:
                    throw new NotImplementedException();
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
            if(ControlTabVM.IsAutoSend && CANController.m_canstart == 1)
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
            if(ControlTabVM.IsAutoSend && CANController.m_canstart == 1)
            {
                canQueueManager.ConstractMessage(m, CANFrameType.Control);
                canQueueManager.RaiseSendQueueChanged();
                m.IsValueChanged = false;
            }
        }

        private void ParasPanelSaveButton_Click(object sender, RoutedEventArgs e)
        {
            ControlTabVM.SaveParasList("paras2.config");
        }

        private void ParasPanelConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (CANController.m_canstart == 1)
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
            CANController.m_canmode = (uint)ModeSelection.SelectedIndex;
        }

        private void MotorStartButton_Click(object sender, RoutedEventArgs e)
        {
            if (CANController.m_canstart == 1)
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
            if (CANController.m_canstart == 1)
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
            if (CANController.m_canstart == 1)
            {
                canQueueManager.ConstractMessage(ControlTabVM.ParaMode, (DeviceModeIndex)(RunModeSelection.SelectedIndex + 1));
                canQueueManager.RaiseSendQueueChanged();
            }
            else
            {
                MessageBox.Show("请先启动CAN控制器", "错误");
            }
        }

        private void StatusCheckButton_Click(object sender, RoutedEventArgs e)
        {
            CANQueueManager manager = CANQueueManager.GetInstance();
            manager.ConstractMessage(CANFrameType.Status, index: (byte)CANACKIndex.Status);
            manager.RaiseSendQueueChanged();
        }
    }
}
