using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Monitor2.Models;
using System.Windows;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;

namespace Monitor2.CAN
{
    public class CANQueueManager:INotifyPropertyChanged
    {
        public static CANQueueManager Manager;

        public Queue<CANFrame> SendQueue { get; set; }
        public Queue<CANFrame> ReceiveQueue { get; set; }

        //发送和接收队列变动事件
        public event EventHandler OnSendQueueChanged;
        public event EventHandler OnReceiveQueueChanged;


        public event PropertyChangedEventHandler PropertyChanged;

        //用于绑定的属性
        private ObservableCollection<LogModel> _logList = new ObservableCollection<LogModel>();
        public ObservableCollection<LogModel> LogList
        {
            get { return _logList; }
        }

        private bool _isLogEnable;
        public bool IsLogEnable
        {
            get { return _isLogEnable; }
            set
            {
                _isLogEnable = value;
                OnPropertyChanged("IsLogEnable");
            }
        }

        private int _sendCount;
        public int SendCount
        {
            get { return _sendCount; }
            set
            {
                _sendCount = value;
                OnPropertyChanged("SendCount");
            }
        }

        private int _errCount;
        public int ErrCount
        {
            get { return _sendCount; }
            set
            {
                _errCount = value;
                OnPropertyChanged("ErrCount");
            }
        }

        private int _receiveCount;
        public int ReceiveCount
        {
            get { return _receiveCount; }
            set
            {
                _receiveCount = value;
                OnPropertyChanged("ReceiveCount");
            }
        }
       

        public CANQueueManager()
        {
            SendQueue = new Queue<CANFrame>();
            ReceiveQueue = new Queue<CANFrame>();
            CANController.OnMessageArrive += ReceiveMessage;
            OnSendQueueChanged += SendMessage;
            IsLogEnable = false;
            CANQueueManager.Manager = this;
        }

        public void ConstractMessage(ParaModel para, CANFrameType type)
        {
            if (type == CANFrameType.Control)
            {
                CANFrame message = new CANFrame();
                message.FrameType = (byte)type;
                message.FrameIndex = para.Index;
                message.DataType = para.Type;
                if (para.Type == 1)
                    message.IndexValue = (byte)Math.Round(para.Value);
                else if (para.Type == 2)
                {
                    message.Value = para.Value;
                    message.IntValue = (int)para.Value;
                    byte[] value = BitConverter.GetBytes(message.Value);
                    //value.Reverse();
                    for (int i = 0; i < 4; i++)
                    {
                        message.data[i + 4] = value[3 - i];
                    }
                }
                else if (para.Type == 3)
                {
                    message.Value = para.Value;
                    message.IntValue = (int)para.Value;
                    byte[] value = BitConverter.GetBytes(message.IntValue);
                    //value.Reverse();
                    for (int i = 0; i < 4; i++)
                    {
                        message.data[i + 4] = value[3 - i];
                    }
                }
                SendQueue.Enqueue(message);
            }
        }

        public void RaiseSendQueueChanged()
        {
            if (OnSendQueueChanged != null)
            {
                OnSendQueueChanged(this, null);
            }
        }

        public void SendMessage(object sender, EventArgs e)
        {
            if (CANController.m_canstart == 1)
            {
                CANController.CANControllerMutex.WaitOne();
                CANController.ReceiveTimer.Enabled = false;
                while (SendQueue.Count != 0)
                {
                    CANController.VCI_CAN_OBJ obj = CANController.ConvertCANFrameToCANObj(SendQueue.Dequeue());
                    if (CANController.VCI_Transmit(CANController.m_devtype, CANController.m_devind, CANController.m_canind,
                        ref obj, 1) == 0)
                    {
                        App.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            ErrCount++;
                        }));
                        //MessageBox.Show("发送失败", "错误", MessageBoxButton.OK);
                    }

                    App.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        SendCount++;
                    }));

                }
                CANController.ReceiveTimer.Enabled = true;
                CANController.CANControllerMutex.ReleaseMutex();
            }
        }

        public void ConstractMessage(CANFrameType type, byte index = 0xff)
        {
            CANFrame message = new CANFrame();
            message.FrameType = (byte)type;
            message.FrameIndex = index;
            SendQueue.Enqueue(message);
        }

        public void ConstractMessage(ParaModel model, DeviceModeIndex modeIndex)
        {
            CANFrame message = new CANFrame();
            message.FrameType = (byte)CANFrameType.Control;
            message.FrameIndex = model.Index;
            message.IndexValue = (byte)modeIndex;
            SendQueue.Enqueue(message);
        }


        public string LogCANFrame(CANFrame frame)
        {
            string str = "[";
            str += DateTime.Now.ToLongTimeString() + " " + DateTime.Now.Millisecond + "]";
            str += "解码数据 ";
            str += "帧类型:";
            switch(frame.FrameType)
            {
                case ((byte)CANFrameType.Start):
                    str += "启动 ";
                    break;
                case ((byte)CANFrameType.Stop):
                    str += "停止 ";
                    break;
                case ((byte)CANFrameType.Query):
                    str += "查询 ";
                    break;
                case ((byte)CANFrameType.ACK):
                    str += "答应 ";
                    break;
                case ((byte)CANFrameType.Para):
                    str += "数据 ";
                    str += "ID:";
                    str += "0x" + Convert.ToString(frame.FrameIndex, 16);
                    str += " 值:";
                    str += Convert.ToString(frame.Value);
                    break;
                case ((byte)CANFrameType.Control):
                    str += "控制 ";
                    str += "ID:";
                    str += "0x" + Convert.ToString(frame.FrameIndex, 16);
                    str += " 选项值:";
                    str += "0x" + Convert.ToString(frame.IndexValue, 16);
                    str += " 数据值:";
                    str += Convert.ToString(frame.Value);
                    break;
                case ((byte)CANFrameType.Graph):
                    str += "图像 ";
                    str += "ID:";
                    str += "0x" + Convert.ToString(frame.FrameIndex, 16);
                    str += " 值:";
                    str += Convert.ToString(frame.Value);
                    break;
                case ((byte)CANFrameType.Status):
                    str += "状态 ";
                    break;
            }
            str += " 元数据: [";
            for (int i = 0; i < 8; i++)
            {
                str += "0x" + Convert.ToString(frame.data[i], 16) + " "; 
            }
            str += "]";
            return str;
        }

        public void ReceiveMessage(object sender, EventArgs e)
        {
            CANController.CANControllerMutex.WaitOne();
            for (int i = 0; i < CANController.recobj_count; i++)
            {
                CANFrame frame = CANController.ConvertCANObjToCANFrame(CANController.m_recobj[i]);
                //Console.WriteLine(CANController.LogCANObj(CANController.m_recobj[i]));
                //Console.WriteLine(LogCANFrame(frame));

                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (IsLogEnable)
                    {
                        LogList.Add(new LogModel(LogCANFrame(frame)));
                    }
                    ReceiveCount++;
                }));
                this.ReceiveQueue.Enqueue(frame);
            }
            CANController.CANControllerMutex.ReleaseMutex();
            if (OnReceiveQueueChanged != null)
            {
                OnReceiveQueueChanged(this, null);
            }
        }

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public static CANQueueManager GetInstance()
        {
            return CANQueueManager.Manager;
        }


    }

    public class LogModel:INotifyPropertyChanged
    {
        private string _log;

        public string Log
        {
            get
            {
                return _log;
            }
            set
            {
                _log = value;
                OnPropertyChanged("Log");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public LogModel() { }

        public LogModel(string str)
        {
            Log = str;
        }

    }
}
