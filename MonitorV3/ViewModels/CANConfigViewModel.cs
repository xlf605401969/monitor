using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonitorV3.Models;
using MonitorV3.CANDriver;
using System.Xml.Serialization;
using System.IO;

namespace MonitorV3.ViewModels
{
    class CANConfigViewModel
    {
        public CANConfigModel CANConfig { get; private set; }
        public XmlSerializer xmlFormatter = new XmlSerializer(typeof(CANConfigModel));
        public CANConfigViewModel()
        {
            CANConfig = new CANConfigModel();
        }

        public void LoadCANConfig(string fileName)
        {
            try
            {
                using (FileStream fs = File.Open(fileName, FileMode.Open))
                {
                    CANConfig = xmlFormatter.Deserialize(fs) as CANConfigModel;
                }
            }
            catch (FileNotFoundException e)
            {
                CANConfig = new CANConfigModel();
            }
            catch (FileFormatException e)
            {
                CANConfig = new CANConfigModel();
            }
            finally
            {
                SaveCANConfig(fileName);
            }
        }

        public void SaveCANConfig(string fileName)
        {
            using (FileStream fs = File.Open(fileName, FileMode.OpenOrCreate))
            {
                xmlFormatter.Serialize(fs, CANConfig);
            }
        }
    }
}
