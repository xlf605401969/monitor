using Monitor2ng.CAN;
using Monitor2ng.ConfigFilePraser;
using Monitor2ng.Models;
using Monitor2ng.SampleData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Monitor2ng.ViewModels
{
    public class MainWindowsViewModel
    {
        public MonitorConfigModel MonitorConfigModel { get; set; }

        public PeakCanConfigModel PeakCanConfigModel { get; set; }

        public UsbCanConfigModel UsbCanConfigModel { get; set; }

        public ZlgCanConfigModel ZlgCanConfigModel { get; set; }

        public Rs485ConfigModel Rs485ConfigModel { get; set; }


        public RelayCommand RefreshCommand { get; set; }

        public RelayCommand StartCommand { get; set; }

        public List<ControlWindow> ControlWindows { get; set; }

        public void RefreshConfigFiles()
        {
            MonitorConfigModel.ConfigFiles.Clear();
            string[] files = Directory.GetFiles(@".\");
            foreach (var str in files)
            {
                if (Regex.Match(str, @".*\.json").Success)
                {
                    MonitorConfigModel.ConfigFiles.Add(str);
                }
            }
            if (MonitorConfigModel.ConfigFiles.Count > 0)
            {
                MonitorConfigModel.SelectedConfigFileIndex = 0;
            }
            else
            {
                MonitorConfigModel.SelectedConfigFileIndex = -1;
            }
        }

        public MainWindowsViewModel()
        {
            ControlWindows = new List<ControlWindow>();

            MonitorConfigModel = new MonitorConfigModel();
            PeakCanConfigModel = new PeakCanConfigModel();
            UsbCanConfigModel = new UsbCanConfigModel();
            ZlgCanConfigModel = new ZlgCanConfigModel();
            Rs485ConfigModel = new Rs485ConfigModel();
            RefreshConfigFiles();

            RefreshCommand = new RelayCommand((object o) =>
            {
                RefreshConfigFiles();
            });
            StartCommand = new RelayCommand((object o) => {

                ControlWindow controlWindow = new ControlWindow();
                ControlWindowViewModel controlWindowViewModel = new ControlWindowViewModel();
                controlWindowViewModel.LoadConfig(MonitorConfigModel.SelectedConfigFile);

                if (MonitorConfigModel.SelectedDevice == "PCAN")
                {
                    PeakCanDriverAdapter peakCanDriver = PeakCanDriverAdapter.GetChannelDriverInstance(PeakCanConfigModel.SelectedChannel);
                    peakCanDriver.EditConfig("Baudrate", PeakCanConfigModel.SelectedBaudrate);

                    try
                    {
                        if (Regex.Match(PeakCanConfigModel.CommunicationId, @"^0x.*").Success)
                        {
                            controlWindowViewModel.CommunicationId = Convert.ToUInt32(PeakCanConfigModel.CommunicationId, 16);
                        }
                        else
                        {
                            controlWindowViewModel.CommunicationId = Convert.ToUInt32(PeakCanConfigModel.CommunicationId, 10);
                        }
                    }
                    catch (FormatException)
                    {
                        controlWindowViewModel.CommunicationId = 0x100;
                        MessageBox.Show("Id格式错误，使用默认id:0x100");
                    }
                    finally
                    {
                        if (controlWindowViewModel.CommunicationId > 0x7ff)
                        {
                            controlWindowViewModel.CommunicationId = 0x100;
                            MessageBox.Show("Id超出标准帧限制，使用默认id:0x100");
                        }
                    }
                    controlWindowViewModel.WindowTitle = String.Format("{0}@{2}-[{1}]:0x{3}", MonitorConfigModel.SelectedDevice, MonitorConfigModel.SelectedConfigFile, PeakCanConfigModel.SelectedBaudrate, controlWindowViewModel.CommunicationId.ToString("X"));

                    CanClient client = new CanClient();
                    client.Connect(peakCanDriver);
                    controlWindowViewModel.CanClient = client;
                }
                else if (MonitorConfigModel.SelectedDevice == "USBCAN")
                {
                    UsbCanDriverAdapter usbCanDriverAdapter = UsbCanDriverAdapter.GetChannelDriverInstance(new Tuple<uint, uint>((uint)UsbCanConfigModel.SelectedDeviceIndex, (uint)UsbCanConfigModel.SelectedChannelIndex));
                    usbCanDriverAdapter.EditConfig("Baudrate", UsbCanConfigModel.SelectedBaudrate);

                    try
                    {
                        if (Regex.Match(UsbCanConfigModel.CommunicationId, @"^0x.*").Success)
                        {
                            controlWindowViewModel.CommunicationId = Convert.ToUInt32(UsbCanConfigModel.CommunicationId, 16);
                        }
                        else
                        {
                            controlWindowViewModel.CommunicationId = Convert.ToUInt32(UsbCanConfigModel.CommunicationId, 10);
                        }
                    }
                    catch (FormatException)
                    {
                        controlWindowViewModel.CommunicationId = 0x100;
                        MessageBox.Show("Id格式错误，使用默认id:0x100");
                    }
                    finally
                    {
                        if (controlWindowViewModel.CommunicationId > 0x7ff)
                        {
                            controlWindowViewModel.CommunicationId = 0x100;
                            MessageBox.Show("Id超出标准帧限制，使用默认id:0x100");
                        }
                    }
                    controlWindowViewModel.WindowTitle = String.Format("{0}@{2}-[{1}]:0x{3}", MonitorConfigModel.SelectedDevice, MonitorConfigModel.SelectedConfigFile, PeakCanConfigModel.SelectedBaudrate, controlWindowViewModel.CommunicationId.ToString("X"));

                    CanClient client = new CanClient();
                    client.Connect(usbCanDriverAdapter);
                    controlWindowViewModel.CanClient = client;
                }
                else if (MonitorConfigModel.SelectedDevice == "ZLGCAN-I")
                {
                    ZlgCanDriverAdapter zlgCanDriverAdapter = ZlgCanDriverAdapter.GetChannelDriverInstance(new Tuple<uint, uint>((uint)UsbCanConfigModel.SelectedDeviceIndex, (uint)UsbCanConfigModel.SelectedChannelIndex));
                    zlgCanDriverAdapter.EditConfig("Baudrate", ZlgCanConfigModel.SelectedBaudrate);

                    try
                    {
                        if (Regex.Match(ZlgCanConfigModel.CommunicationId, @"^0x.*").Success)
                        {
                            controlWindowViewModel.CommunicationId = Convert.ToUInt32(ZlgCanConfigModel.CommunicationId, 16);
                        }
                        else
                        {
                            controlWindowViewModel.CommunicationId = Convert.ToUInt32(ZlgCanConfigModel.CommunicationId, 10);
                        }
                    }
                    catch (FormatException)
                    {
                        controlWindowViewModel.CommunicationId = 0x100;
                        MessageBox.Show("Id格式错误，使用默认id:0x100");
                    }
                    finally
                    {
                        if (controlWindowViewModel.CommunicationId > 0x7ff)
                        {
                            controlWindowViewModel.CommunicationId = 0x100;
                            MessageBox.Show("Id超出标准帧限制，使用默认id:0x100");
                        }
                    }
                    controlWindowViewModel.WindowTitle = String.Format("{0}@{2}-[{1}]:0x{3}", MonitorConfigModel.SelectedDevice, MonitorConfigModel.SelectedConfigFile, PeakCanConfigModel.SelectedBaudrate, controlWindowViewModel.CommunicationId.ToString("X"));

                    CanClient client = new CanClient();
                    client.Connect(zlgCanDriverAdapter);
                    controlWindowViewModel.CanClient = client;
                }
                else if (MonitorConfigModel.SelectedDevice == "RS485")
                {
                    Rs485CanSimDriver rs485CanSimDriver = new Rs485CanSimDriver();
                    rs485CanSimDriver.EditConfig("Port", Rs485ConfigModel.SelectedPort);
                    rs485CanSimDriver.EditConfig("Baudrate", Rs485ConfigModel.Baudrate);
                    controlWindowViewModel.CommunicationId = 0x100;

                    controlWindowViewModel.WindowTitle = String.Format("{0}@{1}", MonitorConfigModel.SelectedDevice, MonitorConfigModel.SelectedConfigFile);

                    CanClient client = new CanClient();
                    client.Connect(rs485CanSimDriver);
                    controlWindowViewModel.CanClient = client;
                }

                controlWindowViewModel.StartCanJob();
                controlWindow.GraphScope.Content = controlWindowViewModel.LineGraph;
                controlWindow.DataContext = controlWindowViewModel;

                controlWindow.Show();
                ControlWindows.Add(controlWindow);

                SampleControlViewModel s = new SampleControlViewModel();
            });
        }

        public void CloseAllControlWindow()
        {
            foreach (var v in ControlWindows)
            {
                v.Close();
            }
        }
    }

    public class TabVisiablityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var v = (int)value;
            var p = int.Parse(parameter.ToString());

            if (v == p)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
