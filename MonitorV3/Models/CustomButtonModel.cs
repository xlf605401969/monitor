using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Input;

namespace MonitorV3.Models
{
    [Serializable]
    public class CustomButtonModel:INotifyPropertyChanged
    {
        private string _name;
        private int _id;
        public CustomButtonModel() { }
        public CustomButtonModel(string name, int id)
        {
            this._name = name;
            this._id = id;
        }

        public int ID
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertityChanged("ID");
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertityChanged("Name");
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertityChanged(string propertityName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertityName));
        }
    }
}
