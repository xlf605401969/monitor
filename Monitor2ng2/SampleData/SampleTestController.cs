using Monitor2ng.TestMcu;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monitor2ng.SampleData
{
    class SampleTestController : TestController
    {
        public SampleTestController() : base()
        {
            Commands.Add(new Models.VariableModel() { Name = "Command1", Id = 1, Type = "Command", ValueType = "Float", Value = 0 });
            Commands.Add(new Models.VariableModel() { Name = "Command2", Id = 1, Type = "Command", ValueType = "Int", Value = 0 });
            Commands.Add(new Models.VariableModel() { Name = "Command3", Id = 1, Type = "Command", ValueType = "IntIndex", Value = 0 });
            States.Add(new Models.VariableModel() { Name = "State1", Id = 1, Type = "State", ValueType = "Float", Value = 0 });
            States.Add(new Models.VariableModel() { Name = "State2", Id = 1, Type = "State", ValueType = "Int", Value = 0 });
            States.Add(new Models.VariableModel() { Name = "State3", Id = 1, Type = "State", ValueType = "IntIndex", Value = 0 });
            Parameters.Add(new Models.VariableModel() { Name = "Parameter1", Id = 1, Type = "Parameter", ValueType = "Float", Value = 0 });
            Parameters.Add(new Models.VariableModel() { Name = "Parameter2", Id = 1, Type = "Parameter", ValueType = "Int", Value = 0 });
            Parameters.Add(new Models.VariableModel() { Name = "Parameter3", Id = 1, Type = "Parameter", ValueType = "IntIndex", Value = 0 });
        }
    }
}
