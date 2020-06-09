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
using System.Windows;

namespace Monitor2.ViewModels
{
    public class GraphTabViewModel:INotifyPropertyChanged
    {
        private int _selectedChannel;

        private int _fileIndex = 0;
        public int SelectedChannel
        {
            get
            {
                return _selectedChannel;
            }
            set
            {
                _selectedChannel = value;
                OnPropertityChanged("SelectedChannel");
            }
        }
        public ObservableCollection<GraphDataModel> ChannelData { get; private set; }

        public void SaveGraphData()
        {
            while (File.Exists("g" + _fileIndex.ToString() + ".csv"))
            {
                _fileIndex += 1;
            }

            var path = "g" + _fileIndex.ToString() + ".csv";

            using (FileStream fStream = File.Open(path, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(fStream))
                {
                    foreach (GraphDataModel m in ChannelData)
                    {
                        writer.WriteLine("{0},{1},", m.Index, m.Value);
                    }
                }
            }
            MessageBox.Show("数据保存至：" + path, "信息", MessageBoxButton.OK);
        }

        public GraphTabViewModel()
        {
            _selectedChannel = 0;
            ChannelData = new ObservableCollection<GraphDataModel>
            {
                new GraphDataModel(0, 100)
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertityChanged(string propertityName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertityName));
        }
    }

    public class GraphDataModel:INotifyPropertyChanged
    {
        private int _index;
        public int Index
        {
            get
            {
                return _index;
            }
            set
            {
                _index = value;
                OnPropertityChanged("Index");
            }
        }

        private float _value;
        public float Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                OnPropertityChanged("Value");
            }
        }

        public GraphDataModel(int index, float value)
        {
            Index = index;
            Value = value;
        }

        public GraphDataModel()
        {
            Index = 0;
            Value = 0;
        }

        public static explicit operator KeyValuePair<int,double>(GraphDataModel model)
        {
            return new KeyValuePair<int, double>(model.Index, model.Value);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertityChanged(string propertityName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertityName));
        }
    }
}
