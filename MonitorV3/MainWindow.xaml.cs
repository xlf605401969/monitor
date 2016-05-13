using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MonitorV3.ViewModels;
using MonitorV3.Models;
using MonitorV3.CANDriver;
using System.IO;
using Microsoft.Win32;
using MonitorV3.Wondows;

namespace MonitorV3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel MainVM = new MainViewModel();
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = MainVM;
            MainVM.CANConfigVM.LoadCANConfig("can.xml");
            MainVM.ControlDataVM.LoadDataFormat("data.xml");
            MainVM.CustomButtonVM.LoadButtonConfig("button.xml");
            foreach (KeyValuePair<string, byte[]> s in CANController.BaudrateList)
            {
                BaudRateSelection.Items.Add(s.Key);
            }
            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainVM.CANConfigVM.SaveCANConfig("can.xml");
            MainVM.CustomButtonVM.SaveButtonConfig("button.xml");
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (MainVM.CANStatusVM.CANStatus.IsCANStarted == false)
            {
                MainVM.CANStatusVM.CANStatus.IsCANStarted = MainVM.InitCAN(true);
#if DEBUG2
                MainVM.CANStatusVM.CANStatus.IsCANStarted = true;
                CANController.m_bOpen = 1;
                CANController.m_canstart = 1;
#endif
            }
            else if (MainVM.CANStatusVM.CANStatus.IsCANStarted == true)
            {
                MainVM.CANStatusVM.CANStatus.IsCANStarted = MainVM.InitCAN(false);
#if DEBUG2
                MainVM.CANStatusVM.CANStatus.IsCANStarted = false;
                CANController.m_bOpen = 0;
                CANController.m_canstart = 0;
#endif
            }
            MainVM.CustomButtonVM.ButtonCommand.UpdateCommandStatus();
        }

        private void ControlDataLoadFromFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Title = "选择文件";
            open.Filter = "文件（.xml）|*.xml|所有文件|*.*";
            open.Multiselect = false;
            if (open.ShowDialog().GetValueOrDefault())
            {
                MainVM.ControlDataVM.LoadDataFormat(open.FileName);
            }
        }

        private void ControlDataSaveToFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Title = "选择文件";
            save.Filter = "文件（.xml）|*.xml|所有文件|*.*";
            save.CheckFileExists = false;
            if (save.ShowDialog().GetValueOrDefault())
            {
                MainVM.ControlDataVM.SaveDataFormat(save.FileName);
            }
        }

        private void ControlDataLoadFromDSP_Click(object sender, RoutedEventArgs e)
        {
            MainVM.LoadDefinitionsFromDSP();
        }

        private void ControlDataDelet_Click(object sender, RoutedEventArgs e)
        {
            MainVM.ControlDataVM.DeleteSelectedControlDataItem();
        }

        private void ControlValue_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox senderTextBox = sender as TextBox;
                ControlDataModel cdm = senderTextBox.DataContext as ControlDataModel;
                cdm.Value = Convert.ToSingle(senderTextBox.Text);
                CANManager.M0(cdm);
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox senderCheckBox = sender as CheckBox;
            ControlDataModel cdm = senderCheckBox.DataContext as ControlDataModel;
            CANManager.R3(cdm);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox senderCheckBox = sender as CheckBox;
            ControlDataModel cdm = senderCheckBox.DataContext as ControlDataModel;
            CANManager.R4(cdm);
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CANController.EnQueueString(ManualCommandBox.Text + (char)0xff);
            }
        }

        private void ControlDataListView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                MainVM.ControlDataVM.DeleteSelectedControlDataItem();
            }
        }

        private void GridViewHeader_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is GridViewColumnHeader)
            {
                GridViewColumnHeader gvc = e.OriginalSource as GridViewColumnHeader;
                if (gvc != null)
                {
                    List<ControlDataModel> list = MainVM.ControlDataVM.ControlDataCollection.ToList();
                    list.Sort((ControlDataModel x, ControlDataModel y) =>
                    {
                        if (MainVM.ControlDataVM.SortDirection)
                        {
                            switch (gvc.Content.ToString())
                            {
                                case ("ID"):
                                    return x.ID.CompareTo(y.ID);
                                case ("Name"):
                                    return x.Name.CompareTo(y.Name);
                                case ("Value"):
                                    return x.IsEditable.CompareTo(y.IsEditable);
                                default:
                                    return 0;
                            }
                        }
                        else
                        {
                            switch (gvc.Content.ToString())
                            {
                                case ("ID"):
                                    return y.ID.CompareTo(x.ID);
                                case ("Name"):
                                    return y.Name.CompareTo(x.Name);
                                case ("Value"):
                                    return y.IsEditable.CompareTo(x.IsEditable);
                                default:
                                    return 0;
                            }
                        }
                    });
                    MainVM.ControlDataVM.SortDirection = !MainVM.ControlDataVM.SortDirection;
                    MainVM.ControlDataVM.ControlDataCollection.Clear();
                    foreach (ControlDataModel cdm in list)
                    {
                        MainVM.ControlDataVM.ControlDataCollection.Add(cdm);
                    }
                }
            }
        }

        private void CustomButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            AddCustomButton addWindow = new AddCustomButton();
            addWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            addWindow.Show();
            addWindow.ConfirmEvent += ((object _o, EventArgs _e) =>
            {
                MainVM.CustomButtonVM.AddButton(addWindow.CustomButtonName, addWindow.CustomButtonID);
            });
        }

        private void CustomButtonDelet_Click(object sender, RoutedEventArgs e)
        {
            MainVM.CustomButtonVM.DeletCustomButton(CustomButtonListView.SelectedIndex);
        }

        private void LoadAllButton_Click(object sender, RoutedEventArgs e)
        {
            MainVM.LoadAllControlData();
        }

        private void SendAllButton_Click(object sender, RoutedEventArgs e)
        {
            MainVM.SendAllControlData();
        }
    }
}
