using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Serialization;
using MonitorV3.Models;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using MonitorV3.CANDriver;

namespace MonitorV3.ViewModels
{
    class CustomButtonViewModel
    {
        public ObservableCollection<CustomButtonModel> CustomButtonCollection { get; private set; }
        public XmlSerializer CunstomButtonSerializer = new XmlSerializer(typeof(ObservableCollection<CustomButtonModel>));
        public CustomButtonCommand ButtonCommand { get; private set; }
        public CustomButtonViewModel()
        {
            CustomButtonCollection = new ObservableCollection<CustomButtonModel>();
            CustomButtonCollection.Add(new CustomButtonModel());
            ButtonCommand = new CustomButtonCommand();
        }

        public void SaveButtonConfig(string name)
        {
            try
            {
                using (FileStream fs = File.Open(name, FileMode.Create))
                {
                    CunstomButtonSerializer.Serialize(fs, CustomButtonCollection);
                }
            }
            catch(Exception)
            {
                MessageBox.Show("保存配置文件失败", "警告",
                                MessageBoxButton.OK);
            }
        }

        public void LoadButtonConfig(string name)
        {
            try
            {
                using (FileStream fs = File.Open(name, FileMode.Open))
                {
                    ObservableCollection<CustomButtonModel> cbc = CunstomButtonSerializer.Deserialize(fs) as ObservableCollection<CustomButtonModel>;

                    CustomButtonCollection.Clear();
                    foreach (CustomButtonModel cbce in cbc)
                    {
                        CustomButtonCollection.Add(cbce);
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("自定义按钮配置文件格式错误", "错误",
                                MessageBoxButton.OK);
            }
        }

        public void DeletCustomButton(int index)
        {
            if (index >=0 )
            {
                CustomButtonCollection.RemoveAt(index);
            }
        }

        public void AddButton(string name, int id)
        {
            this.CustomButtonCollection.Add(new CustomButtonModel(name, id));
        }
    }

    class CustomButtonCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return CANController.m_canstart > 0;
        }

        public void Execute(object parameter)
        {
            CANManager.S(Convert.ToInt32(parameter.ToString()));
        }

        public void UpdateCommandStatus()
        {
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }
    }
}
