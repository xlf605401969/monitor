using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
