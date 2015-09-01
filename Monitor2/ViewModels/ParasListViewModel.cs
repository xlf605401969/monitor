using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Monitor2.Models;
using System.ComponentModel;
using System.Xml.Serialization;
using System.IO;

namespace Monitor2.ViewModels
{
    public class ParasListViewModel
    {
        XmlSerializer xmlFormat = new XmlSerializer(typeof(ObservableCollection<ParaModel>), new Type[] { typeof(ParaModel) });

        public ObservableCollection<ParaModel> parasList = new ObservableCollection<ParaModel>();

        public void LoadParasList()
        {
            parasList.Add(new ParaModel());
            parasList.Add(new ParaModel());
            parasList.Add(new ParaModel());
        }

        public void LoadParasList(string fileName)
        {
            using (FileStream fStream = File.Open(fileName, FileMode.Open))
            {
                parasList = xmlFormat.Deserialize(fStream) as ObservableCollection<ParaModel>;
                foreach(ParaModel m in parasList)
                {
                    m.IsValueChanged = false;
                }
            }
        }

        public void SaveParasList(string fileName)
        {
            using (FileStream fStream = File.Open(fileName, FileMode.Create))
            {
                xmlFormat.Serialize(fStream, parasList);
            }
        }

    }
}
