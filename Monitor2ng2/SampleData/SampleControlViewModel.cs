using Monitor2ng.CAN;
using Monitor2ng.Models;
using Monitor2ng.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Monitor2ng.SampleData
{
    public class SampleControlViewModel : ControlWindowViewModel
    {
        public SampleControlViewModel() : base()
        {
            WindowTitle = "测试标题";
            
            ControlModel.Modes.Add("转速模式");

            VariableModel param = new VariableModel() { Name = "TestTest", Type = "Parameter", Value = 1 };
            VariableModel state = new VariableModel() { Name = "TestState", Type = "State", Value = 2 };
            VariableModel command = new VariableModel() { Name = "TestCommand", Type = "Command", Value = 2, ReturnValue = 10 };

            for (int i = 1; i < 100; i++)
            {
                Parameters.Add(param);
                States.Add(state);
                Commands.Add(command);
            }

            GraphDataModel.BindVariables(States);
            GraphDataModel.AddChannel("Channel: 0");
            GraphDataModel.AddChannel("Channel: 1");

            GraphDataModel.ChannelData[0].Add(new Tuple<int, float>(123, 1.0f));
            GraphDataModel.ChannelData[0].Add(new Tuple<int, float>(123, 1.0f));
            GraphDataModel.ChannelData[1].Add(new Tuple<int, float>(123, 1.0f));
            GraphDataModel.ChannelData[1].Add(new Tuple<int, float>(123, 1.0f));
            GraphDataModel.ChannelData[1].Add(new Tuple<int, float>(123, 1.0f));

            LogModel.EnableLog = true;
            LogModel.LogReceiveFrame(MonitorFrameBuilder.ValueFrame(new VariableModel() { Name = "TestReceive", Id = 1 }));
            LogModel.LogSendFrame(MonitorFrameBuilder.ValueFrame(new VariableModel() { Name = "TestSend", Id = 2 }));
        }
    }
}
