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
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Monitor2ng.ViewModels
{
    public class MainWindowsViewModel
    {
        public MonitorConfigModel MonitorConfigModel { get; set; }

        public PeakCanConfigModel PeakCanConfigModel { get; set; }

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
            RefreshConfigFiles();

            RefreshCommand = new RelayCommand((object o) =>
            {
                RefreshConfigFiles();
            });
            StartCommand = new RelayCommand((object o) => {

                //ConfigFileModel configFileModel = new ConfigFileModel();
                //configFileModel.Modes.Add("tesmode1", 1);
                //configFileModel.Modes.Add("tesmode2", 2);
                //configFileModel.Variables.Add(new MachineVariableBase("testparam", 1, "command", 0));
                //string json = JsonConvert.SerializeObject(configFileModel, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                //using (TextWriter writer = File.CreateText(@".\cofig1.json"))
                //{
                //    writer.Write(json);
                //}

                //string json = File.ReadAllText(@".\cofig1.json");
                //ConfigFileModel configFileModel = JsonConvert.DeserializeObject<ConfigFileModel>(json);

                ControlWindow controlWindow = new ControlWindow();
                ControlWindowViewModel controlWindowViewModel = new ControlWindowViewModel();
                controlWindowViewModel.LoadConfig(MonitorConfigModel.SelectedConfigFile);

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
