using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Threading.Channels;
using System.Linq;

namespace Monitor2ng.Models
{
    public class GraphDataModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<string> Channels { get; set; }

        public List<ObservableCollection<Tuple<int,float>>> ChannelData { get; set; }

        public List<int> ChannelSamplingRatio { get; set; }

        public List<int> ChannelLogVariableIndexes { get; set; }

        public ObservableCollection<VariableModel> LogVariables { get; set; }

        public int SamplingRatio
        {
            get
            {
                return ChannelSamplingRatio[SelectedChannelIndex];
            }
            set
            {
                ChannelSamplingRatio[selectedChannelIndex] = value;
                OnPropertityChanged("SamplingRatio");
            }
        }

        public int SelectedLogVariableIndex
        {
            get
            {
                return ChannelLogVariableIndexes[SelectedChannelIndex];
            }
            set
            {
                ChannelLogVariableIndexes[SelectedChannelIndex] = value;
                OnPropertityChanged("SelectedLogVariableIndex");
            }
        }
        public VariableModel SelectedLogVariable { get { return LogVariables[ChannelLogVariableIndexes[SelectedChannelIndex]]; } }

        public ObservableCollection<Tuple<int,float>> DisplayChannelData
        {
            get
            {
                return ChannelData[SelectedChannelIndex];
            }
        }

        public int SelectedChannelIndex
        {
            get
            {
                return selectedChannelIndex;
            }
            set
            {
                selectedChannelIndex = value;
                OnPropertityChanged("SelectedChannelIndex");
                OnPropertityChanged("DisplayChannelData");
                OnPropertityChanged("SamplingRatio");
                OnPropertityChanged("SelectedLogVariableIndex");
                OnPropertityChanged("DisplayChannelData");
                OnPropertityChanged("SelectedChannel");
            }
        }
        private int selectedChannelIndex;
        public string SelectedChannel { get { return Channels[SelectedChannelIndex]; } }
        public ObservableCollection<Tuple<int, float>> SelectedChannelData
        {
            get
            {
                return ChannelData[SelectedChannelIndex];
            }
        }

        public GraphDataModel()
        {
            Channels = new ObservableCollection<string>();
            ChannelData = new List<ObservableCollection<Tuple<int, float>>>();
            ChannelSamplingRatio = new List<int>();
            ChannelLogVariableIndexes = new List<int>();
            LogVariables = new ObservableCollection<VariableModel>();
            LogVariables.Add(new VariableModel() { Name = "Default", Id = -1 }); ;
        }

        public void BindVariables(IList<VariableModel> variableModels)
        {
            foreach (var v in variableModels)
            {
                LogVariables.Add(v);
            }
        }

        public void AddChannel(string name)
        {
            Channels.Add(name);
            ChannelData.Add(new ObservableCollection<Tuple<int, float>>());
            ChannelSamplingRatio.Add(1);
            ChannelLogVariableIndexes.Add(0);
        }

        protected void OnPropertityChanged(string propertityName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertityName));
        }
    }
}
