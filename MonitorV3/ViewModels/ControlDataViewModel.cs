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

namespace MonitorV3.ViewModels
{
    class ControlDataViewModel
    {
        public ObservableCollection<ControlDataModel> ControlDataCollection { get; private set; }
        public XmlSerializer ControlDataCollectionSerializer = new XmlSerializer(typeof(ObservableCollection<ControlDataModel>));
        public ControlDataViewModel()
        {
            ControlDataCollection = new ObservableCollection<ControlDataModel>();
            ControlDataCollection.Add(new ControlDataModel());
            
        }

        public void SaveDataFormat(string name)
        {
            ControlDataCollectionSerializer.Serialize(File.Open(name, FileMode.Create), ControlDataCollection);
        }

        public void LoadDataFormat(string name)
        {
            ControlDataCollection.Clear();
            try
            {
                ObservableCollection<ControlDataModel> cdc = ControlDataCollectionSerializer.Deserialize(File.Open(name, FileMode.Open)) as ObservableCollection<ControlDataModel>;
                foreach (ControlDataModel cdce in cdc)
                {
                    ControlDataCollection.Add(cdce);
                }
            }
            catch(FileFormatException e)
            {
                
            }
        }

        public void DeletControlDataItem(int index)
        {
            try
            {
                ControlDataCollection.RemoveAt(index);
            }
            catch
            {

            }
        }

        
    }
}
