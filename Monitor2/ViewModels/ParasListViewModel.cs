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
        public ParaModel DCRelay { get; set; }
        public ParaModel LoadRelay { get; set; }
        public ParaModel DisRelay { get; set; }

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
            DCRelay = (from p in parasList where p.Index == 32 select p).First() as ParaModel;
            LoadRelay = (from p in parasList where p.Index == 33 select p).First() as ParaModel;
            DisRelay = (from p in parasList where p.Index == 34 select p).First() as ParaModel;
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
