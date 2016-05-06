﻿using System;
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
            foreach (string s in CANController.BaudrateDic.Keys)
            {
                BaudRateSelection.Items.Add(s);
            }
            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private bool InitCAN(bool open)
        {
            if (open)
            {
                CANController.VCI_INIT_CONFIG cfg = new CANController.VCI_INIT_CONFIG();
                switch ((DeviceSelection.SelectedItem as ComboBoxItem).Content as string)
                {
                    case ("USB_CAN"):
                        CANController.m_devtype = CANController.DEV_USBCAN;
                        break;
                    case ("USB_CAN2"):
                        CANController.m_devtype = CANController.DEV_USBCAN2;
                        break;
                }
                CANController.m_devind = (uint)IndexSelection.SelectedIndex;
                CANController.m_canind = (uint)PortSelection.SelectedIndex;
                CANController.m_canmode = (uint)ModeSelection.SelectedIndex;
                cfg.Mode = (byte)CANController.m_canmode;
                cfg.AccCode = Convert.ToUInt32(AccCodeInput.Text, 16);
                cfg.AccMask = Convert.ToUInt32(AccMaskInput.Text, 16);
                cfg.Timing0 = CANController.BaudrateDic[BaudRateSelection.SelectedItem as string][0];
                cfg.Timing1 = CANController.BaudrateDic[BaudRateSelection.SelectedItem as string][1];
                CANController.m_ID = Convert.ToUInt32(IDInput.Text, 16);
                if (CANController.VCI_OpenDevice(CANController.m_devtype, CANController.m_devind, 0) != 0)
                {
                    try
                    {
                        CANController.VCI_InitCAN(CANController.m_devtype, CANController.m_devind, CANController.m_canind, ref cfg);
                        CANController.VCI_StartCAN(CANController.m_devtype, CANController.m_devind, CANController.m_canind);
                        CANController.m_bOpen = 1;
                        CANController.m_canstart = 1;
                        return true; ;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Initialize Error");
                        Console.WriteLine("Error Infomation: {0}", e.Message);
                        return false;
                    }
                }
                else
                {
                    CANController.m_bOpen = 0;
                    CANController.m_canstart = 0;
                    return false;
                }
            }
            else
            {
                CANController.VCI_CloseDevice(CANController.m_devtype, CANController.m_devind);
                CANController.m_bOpen = 0;
                CANController.m_canstart = 0;
                return false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (MainVM.CANStatusVM.CANStatus.IsCANStarted == false)
            {
                MainVM.CANStatusVM.CANStatus.IsCANStarted =InitCAN(true);
            }
            else if (MainVM.CANStatusVM.CANStatus.IsCANStarted == true)
            {
                MainVM.CANStatusVM.CANStatus.IsCANStarted = InitCAN(false);
            }
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
            OpenFileDialog open = new OpenFileDialog();
            open.Title = "选择文件";
            open.Filter = "文件（.xml）|*.xml|所有文件|*.*";
            open.Multiselect = false;
            if (open.ShowDialog().GetValueOrDefault())
            {
                MainVM.ControlDataVM.SaveDataFormat(open.FileName);
            }
        }

        private void ControlDataDelet_Click(object sender, RoutedEventArgs e)
        {
            MainVM.ControlDataVM.DeletControlDataItem(ControlDataListView.SelectedIndex);
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
                MainVM.ControlDataVM.DeletControlDataItem(ControlDataListView.SelectedIndex);
            }
        } 
    }
}
