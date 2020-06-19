using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Monitor2ng.Models
{
    public class MachineControlModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Dictionary<string, int> ModeDict;

        public int SelectedModeValue
        {
            get
            {
                if (selectedModeIndex != -1)
                {
                    return ModeDict[Modes[selectedModeIndex]];
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                var res = from v in ModeDict where v.Value == value select v;
                if (res.Count() > 0)
                {
                    var first = res.First();
                    SelectedModeIndex = Modes.IndexOf(first.Key);
                }
            }
        }

        public ObservableCollection<string> Modes { get; private set; }
        public int SelectedModeIndex
        {
            get
            {
                return selectedModeIndex;
            }
            set
            {
                selectedModeIndex = value;
                OnPropertityChanged("SelectedModeIndex");
            }
        }
        private int selectedModeIndex;


        public MachineControlModel()
        {
            Modes = new ObservableCollection<string>();
        }

        protected void OnPropertityChanged(string propertityName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertityName));
        }
    }
}
