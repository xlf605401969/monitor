using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Monitor2ng.ConfigFilePraser
{
    public class ConfigFileModel
    {
        public string Name { get; set; }
        public int MaxGraphChannel { get; set; }

        [DefaultValue(3000)]
        public int GraphReadingTimeout { get; set; }

        [DefaultValue(300)]
        public int StateCheckInterval { get; set; }
        public int StateVariableId { get; set; }
        public int ModeVariableId { get; set; }
        public Dictionary<string, int> Modes { get; set; }
        public List<MachineVariableBase> Variables { get; set; }

        public ConfigFileModel()
        {
            Modes = new Dictionary<string, int>();
            Variables = new List<MachineVariableBase>();
        }
    }

    public class MachineMode
    {
        public string ModeName { get; set; }
        public int ModeIndex { get; set; }

        public MachineMode(string name, int index)
        {
            ModeName = name;
            ModeIndex = index;
        }
    }

    public class MachineVariableBase
    {
        [DefaultValue("Variable")]
        public virtual string Name { get => name; set => name = value; }
        [DefaultValue(0xff)]
        public virtual int Id { get => id; set => id = value; }
        [DefaultValue("State")]
        public virtual string Type { get => type; set => type = value; }
        [DefaultValue("IntIndex")]
        public virtual string ValueType { get => valueType; set => valueType = value; }
        public virtual float Value { get => pvalue; set => pvalue = value; }
        public virtual float ReturnValue { get => returnValue; set => returnValue = value; }
        public virtual float DeltaStep { get => deltaStep; set => deltaStep = value; }

        protected string name;
        protected int id;
        protected string type;
        protected string valueType;
        protected float pvalue;
        protected float returnValue;
        protected float deltaStep;

        public MachineVariableBase()
        {
        }

        public MachineVariableBase(MachineVariableBase b)
        {
            Name = b.Name;
            Id = b.Id;
            Type = b.Type;
            Value = b.Value;
            ValueType = b.ValueType;
            ReturnValue = b.ReturnValue;
            DeltaStep = b.DeltaStep;
        }

        public MachineVariableBase(string name, int id, string type, float value)
        {
            Name = name;
            Id = id;
            Type = type;
            Value = value;
        }
    }
}
