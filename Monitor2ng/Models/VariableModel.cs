using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Monitor2ng.ConfigFilePraser;

namespace Monitor2ng.Models
{
    public class VariableModel : MachineVariableBase, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public override string Name { get => name; set { name = value; OnPropertityChanged("Name"); } }
        public override int Id { get => id; set { id = value; OnPropertityChanged("Id"); } }
        public override string Type { get => type; set { type = value; OnPropertityChanged("Type"); } }
        public override float Value { get => pvalue; set { pvalue = value; IsValueModified = true; OnPropertityChanged("Value"); } }
        public override float ReturnValue { get => returnValue; set { returnValue = value; OnPropertityChanged("ReturnValue"); } }
        public override float DeltaStep { get => deltaStep; set { deltaStep = value; OnPropertityChanged("DeltaStep"); } }

        public bool IsValueModified
        {
            get => isValueModified;
            set
            {
                isValueModified = value;
                OnPropertityChanged("IsValueModified");
            }
        }
        private bool isValueModified;        

        protected void OnPropertityChanged(string propertityName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertityName));
        }

        public VariableModel(MachineVariableBase b) : base(b) { }

        public VariableModel() { }

        public override string ToString()
        {
            return Name;
        }
    }
}
