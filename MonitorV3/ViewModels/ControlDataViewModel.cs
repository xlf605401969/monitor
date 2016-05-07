using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using MonitorV3.Models;
using System.Xml.Serialization;
using System.IO;
using MonitorV3.CANDriver;
using System.Windows;
using System.Data;
using System.Windows.Data;
using System.Globalization;

namespace MonitorV3.ViewModels
{
    class ControlDataViewModel
    {
        public ObservableCollection<ControlDataModel> ControlDataCollection { get; private set; }
        public XmlSerializer ControlDataCollectionSerializer = new XmlSerializer(typeof(ObservableCollection<ControlDataModel>));
        public int SelectedControlDataIndex { get; set; }
        public bool SortDirection { get; set; }
        public ControlDataViewModel()
        {
            ControlDataCollection = new ObservableCollection<ControlDataModel>();
            ControlDataCollection.Add(new ControlDataModel());
            SelectedControlDataIndex = -1;
        }

        public void SaveDataFormat(string name)
        {
            ControlDataCollectionSerializer.Serialize(File.Open(name, FileMode.Create), ControlDataCollection);
        }

        public void LoadDataFormat(string name)
        {          
            try
            {
                ObservableCollection<ControlDataModel> cdc = ControlDataCollectionSerializer.Deserialize(File.Open(name, FileMode.Open)) as ObservableCollection<ControlDataModel>;
                ControlDataCollection.Clear();
                foreach (ControlDataModel cdce in cdc)
                {
                    ControlDataCollection.Add(cdce);
                }
            }
            catch(Exception)
            {
                MessageBox.Show("数据格式配置文件格式错误", "错误",
                                MessageBoxButton.OK);
            }
        }

        public void DeleteControlDataItem(int index)
        {
            if (index >= 0)
                ControlDataCollection.RemoveAt(index);
        }

        public void DeleteSelectedControlDataItem()
        {
            DeleteControlDataItem(this.SelectedControlDataIndex);
        }
    }

    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((int)value >= 0)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
