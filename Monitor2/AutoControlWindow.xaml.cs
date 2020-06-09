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
using System.Windows.Shapes;
using Monitor2.CAN;
using System.Threading;
using Monitor2.Models;
using System.ComponentModel;

namespace Monitor2
{
    /// <summary>
    /// Interaction logic for AutoControlWindow.xaml
    /// </summary>
    public partial class AutoControlWindow : Window
    {
        public CANQueueManager manager = CANQueueManager.GetInstance();
        public StatusModel statusModel;
        public List<ParaModel> ParaList;
        public List<ParaModel> StatusList;

        Action ChangetoGenerate;
        Action StartUp;
        Action Cancel;
        Action StartGenerate;
        Action Relocation;
        Task CurrentTask;
        public AutoControlWindow()
        {
            InitializeComponent();
            InitlizeAction();
            PageGrid.DataContext = statusModel;

        }

        private void SwitchGenModeButton_Click(object sender, RoutedEventArgs e)
        {
            if (CANController.Started || true)
            {
                //CurrentTask = Task.Run(ChangetoGenerate);
                Task tmp = new Task(ChangetoGenerate);
                CurrentTask = tmp;
                tmp.Start();
            }
        }

        private void StartUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (CANController.Started || true)
            {
                //CurrentTask = Task.Run(StartUp);
                Task tmp = new Task(StartUp);
                CurrentTask = tmp;
                tmp.Start();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (CANController.Started || true)
            {
                //CurrentTask = Task.Run(StartUp);
                Task tmp = new Task(Cancel);
                CurrentTask = tmp;
                tmp.Start();
            }
        }

        public void DCRelay(bool status)
        {
            ParaModel model = new ParaModel
            {
                Index = 32,
                Type = 1,
                Value = status ? 1 : 0
            };
            manager.ConstractMessage(model, CANFrameType.Control);
            manager.RaiseSendQueueChanged();
        }

        public void LoadRelay(bool status)
        {
            ParaModel model = new ParaModel
            {
                Index = 33,
                Type = 2,
                Value = status ? 1 : 0
            };
            manager.ConstractMessage(model, CANFrameType.Control);
            manager.RaiseSendQueueChanged();
        }

        public void DissRelay(bool status)
        {
            ParaModel model = new ParaModel
            {
                Index = 34,
                Type = 1,
                Value = status ? 1 : 0
            };
            manager.ConstractMessage(model, CANFrameType.Control);
            manager.RaiseSendQueueChanged();
        }

        public void ChangeRefMode(RefMode mode)
        {
            ParaModel model = new Models.ParaModel
            {
                Index = 20,
                Type = 1,
                Value = (int)mode
            };
            manager.ConstractMessage(model, CANFrameType.Control);
            manager.RaiseSendQueueChanged();
        }

        public void SetSpeed(double speed)
        {
            ParaModel model = new ParaModel
            {
                Index = 19,
                Type = 2,
                Value = (float)speed
            };
            manager.ConstractMessage(model, CANFrameType.Control);
            manager.RaiseSendQueueChanged();
        }

        public void QueryParas()
        {
            manager.ConstractMessage(CANFrameType.Query);
            manager.RaiseSendQueueChanged();
        }

        public void SetRunMode(bool RunMode)
        {
            if (RunMode == true)
            {
                manager.ConstractMessage(CANFrameType.Start);
            }
            else
            {
                manager.ConstractMessage(CANFrameType.Stop);
            }
        }

        private void InitlizeAction()
        {
            ChangetoGenerate = new Action(() =>
            {
                ActionStart();
                DissRelay(false);
                Thread.Sleep(10);
                SetRunMode(false);
                Thread.Sleep(10);
                DCRelay(false);
                ActionProgressValue(30);
                Thread.Sleep(500);
                LoadRelay(true);
                ActionProgressValue(60);
                Thread.Sleep(500);
                ChangeRefMode(RefMode.Generate);
                Thread.Sleep(10);
                SetRunMode(true);
                ActionProgressValue(90);
                ActionEnd();
            });
            StartUp = new Action(() =>
            {
                ActionStart();
                DissRelay(false);
                Thread.Sleep(10);
                ChangeRefMode(RefMode.Speed);
                Thread.Sleep(10);
                LoadRelay(false);
                Thread.Sleep(10);
                DCRelay(true);
                Thread.Sleep(10);
                SetSpeed(200);
                Thread.Sleep(10);
                SetRunMode(true);
                ActionProgressValue(10);
                Thread.Sleep(60 * 1000);
                SetSpeed(4020);
                ActionProgressValue(90);
                Thread.Sleep(1000);
                ActionEnd();
            });
            Cancel = new Action(() =>
            {
                ActionStart();
                SetRunMode(false);
                Thread.Sleep(10);
                DCRelay(false);
                Thread.Sleep(300);
                ActionProgressValue(50);
                LoadRelay(false);
                Thread.Sleep(300);
                DissRelay(false);
                ActionEnd();
            });
            StartGenerate = new Action(() =>
            {
                ActionStart();
                DissRelay(false);
                Thread.Sleep(10);
                DCRelay(false);
                ActionProgressValue(30);
                Thread.Sleep(500);
                LoadRelay(true);
                ActionProgressValue(60);
                Thread.Sleep(500);
                ChangeRefMode(RefMode.Generate);
                Thread.Sleep(10);
                SetRunMode(true);
                ActionEnd();
            });
            Relocation = new Action(() =>
            {
                ActionStart();
                DissRelay(false);
                Thread.Sleep(10);
                LoadRelay(false);
                Thread.Sleep(10);
                DCRelay(true);
                ActionProgressValue(10);
                ChangeRefMode(RefMode.Init);
                Thread.Sleep(10);
                SetRunMode(true);
                ActionProgressValue(20);
                Thread.Sleep(40000);              
                ActionProgressValue(90);
                SetRunMode(false);
                Thread.Sleep(10);
                DCRelay(false);
                Thread.Sleep(10);
                QueryParas();
                Thread.Sleep(1000);
                var value = ((from m in ParaList where m.Index == 35 select m).First() as ParaModel)?.Value;
                if (value > 900 || value < 890)
                {
                    MessageBox.Show("定位错误，请重试");
                }
                ActionEnd();
            });
            statusModel = new Monitor2.StatusModel
            {
                IsActionOver = true,
                ActionProgress = 0
            };
        }

        public void ActionProgressValue(double value)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                statusModel.ActionProgress = value;
            }));
        }

        public void ActionStart()
        {
            statusModel.IsActionOver = false;
            statusModel.ActionProgress = 0;
        }

        public void ActionEnd()
        {
            statusModel.IsActionOver = true;
            statusModel.ActionProgress = 100;
        }



        public enum RefMode {Stop = 0, Speed = 1, Torque = 2, Init = 3, Generate = 4}

        private void StartGenerateButton_Click(object sender, RoutedEventArgs e)
        {
            if (CANController.Started || true)
            {
                //CurrentTask = Task.Run(StartUp);
                Task tmp = new Task(StartGenerate);
                CurrentTask = tmp;
                tmp.Start();
            }
        }

        private void LocationButton_Click(object sender, RoutedEventArgs e)
        {
            if (CANController.Started || true)
            {
                //CurrentTask = Task.Run(StartUp);
                Task tmp = new Task(Relocation);
                CurrentTask = tmp;
                tmp.Start();
            }
        }
    }

    public class StatusModel : INotifyPropertyChanged
    {
        private bool _isActionOver;
        private double _actionProgress;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsActionOver
        {
            get
            {
                return _isActionOver;
            }
            set
            {
                _isActionOver = value;
                OnPropertyChanged("IsActionOver");
            }
        }

        public double ActionProgress
        {
            get
            {
                return _actionProgress;
            }
            set
            {
                _actionProgress = value;
                OnPropertyChanged("ActionProgress");
            }
        }

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
