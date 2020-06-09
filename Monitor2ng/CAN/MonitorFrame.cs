using Monitor2ng.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Controls;

namespace Monitor2ng.CAN
{
    public class MonitorFrame : RawCanFrame
    {
        public FrameType Type
        {
            get
            {
                return (FrameType)data[0];
            }
            set
            {
                data[0] = (byte)value;
            }
        }
        public byte Index
        {
            get
            {
                return data[1];
            }
            set
            {
                data[1] = value;
            }
        }
        public byte Command
        {
            get
            {
                return data[1];
            }
            set
            {
                data[1] = value;
            }
        }
        public FrameDataType DataType
        {
            get
            {
                return (FrameDataType)data[2];
            }
            set
            {
                data[2] = (byte)value;
            }
        }

        public byte GraphChannel
        {
            get
            {
                return data[3];
            }
            set
            {
                data[3] = (byte)value;
            }
        }

        public UInt16 GraphDataIndex
        {
            get
            {
                byte[] a = new byte[2];
                for (int i = 0; i < 2; i++)
                {
                    a[i] = data[3 - i];
                }
                return BitConverter.ToUInt16(a);
            }
            set
            {
                byte[] b = BitConverter.GetBytes(value);
                for (int i = 0; i < 2; i++)
                {
                    data[3 - i] = b[i];
                }
            }
        }

        public float GraphData
        {
            get => FloatValue;
            set
            {
                FloatValue = value;
            }
        }

        public byte IntIndexValue
        {
            get
            {
                return data[3];
            }
            set
            {
                data[3] = (byte)value;
            }
        }
        public int Int32Value
        {
            get
            {
                byte[] a = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    a[i] = data[4 + i];
                }
                return BitConverter.ToInt32(a);
            }
            set
            {
                byte[] b = BitConverter.GetBytes(value);
                for (int i = 0; i < 4; i++)
                {
                    data[4 + i] = b[i];
                }
            }
        }
        public Int16 Int16Value
        {
            get
            {
                byte[] a = new byte[2];
                for (int i = 0; i < 2; i++)
                {
                    a[i] = data[4 + i];
                }
                return BitConverter.ToInt16(a);
            }
            set
            {
                byte[] b = BitConverter.GetBytes(value);
                for (int i = 0; i < 2; i++)
                {
                    data[4 + i] = b[i];
                }
            }
        }
        public float FloatValue
        {
            get
            {
                byte[] a = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    a[i] = data[4 + i];
                }
                return BitConverter.ToSingle(a);
            }
            set
            {
                byte[] b = BitConverter.GetBytes(value);
                for (int i = 0; i < 4; i++)
                {
                    data[4 + i] = b[i];
                }
            }
        }
        public float AutoValue
        {
            get
            {
                float v = DataType switch
                {
                    FrameDataType.IntIndex => IntIndexValue,
                    FrameDataType.Int16 => Int16Value,
                    FrameDataType.Int32 => Int32Value,
                    FrameDataType.Float => FloatValue,
                    _ => 0
                };
                return v;
            }
        }
        public string RawData
        {
            get
            {
                return ToString();
            }
        }
    }

    public static class MonitorFrameBuilder
    {
        public static MonitorFrame ControlFrame(VariableModel model)
        {
            MonitorFrame frame = new MonitorFrame();
            frame.Type = FrameType.Control;
            frame.Index = (byte)model.Id;
            switch (model.ValueType)
            {
                case "IntIndex":
                    frame.DataType = FrameDataType.IntIndex;
                    frame.IntIndexValue = (byte)model.Value;
                    break;
                case "Int16":
                    frame.DataType = FrameDataType.Int16;
                    frame.Int16Value = (Int16)model.Value;
                    break;
                case "Int32":
                    frame.DataType = FrameDataType.Int32;
                    frame.Int32Value = (Int32)model.Value;
                    break;
                case "Float":
                    frame.DataType = FrameDataType.Float;
                    frame.FloatValue = model.Value;
                    break;
                default:
                    frame.Type = FrameType.Invalid;
                    break;
            }
            return frame;
        }

        public static MonitorFrame ValueFrame(VariableModel model)
        {
            MonitorFrame frame = new MonitorFrame();
            frame.Type = FrameType.Value;
            frame.Index = (byte)model.Id;
            switch (model.ValueType)
            {
                case "IntIndex":
                    frame.DataType = FrameDataType.IntIndex;
                    frame.IntIndexValue = (byte)model.Value;
                    break;
                case "Int16":
                    frame.DataType = FrameDataType.Int16;
                    frame.Int16Value = (Int16)model.Value;
                    break;
                case "Int32":
                    frame.DataType = FrameDataType.Int32;
                    frame.Int32Value = (Int32)model.Value;
                    break;
                case "Float":
                    frame.DataType = FrameDataType.Float;
                    frame.FloatValue = model.Value;
                    break;
                default:
                    frame.Type = FrameType.Invalid;
                    break;
            }
            return frame;
        }

        public static MonitorFrame QueryFrame(QueryCmd cmd)
        {
            MonitorFrame frame = new MonitorFrame();

            frame.Type = FrameType.Query;
            frame.Index = (byte)cmd;

            return frame;
        }

        public static MonitorFrame GraphControlFrame(byte channel, GraphCmd cmd, int para = 1)
        {
            MonitorFrame frame = new MonitorFrame();
            frame.Type = FrameType.GraphControl;
            frame.Command = (byte)cmd;
            frame.GraphChannel = channel;
            switch (cmd)
            {
                case GraphCmd.SetRatio: 
                case GraphCmd.SetLogVariable:
                    frame.DataType = FrameDataType.Int32;
                    frame.Int32Value = para;
                    break;
                case GraphCmd.EndOfData:
                    frame.DataType = FrameDataType.IntIndex;
                    break;
            }
            return frame;
        }

        public static MonitorFrame GraphDataFrame(UInt16 index, int channel, float data)
        {
            MonitorFrame frame = new MonitorFrame();
            frame.Type = FrameType.GraphData;
            frame.Index = (byte)channel;
            frame.GraphDataIndex = index;
            frame.GraphData = data;
            return frame;
        }

        public static MonitorFrame FromRawFrame(RawCanFrame raw)
        {
            MonitorFrame frame = new MonitorFrame();
            frame.FrameType = raw.FrameType;
            frame.id = raw.id;
            frame.data = raw.data;
            return frame;
        }

        public static MonitorFrame StartFrame()
        {
            MonitorFrame frame = new MonitorFrame();
            frame.Type = FrameType.Start;
            return frame;
        }

        public static MonitorFrame StopFrame()
        {
            MonitorFrame frame = new MonitorFrame();
            frame.Type = FrameType.Stop;
            return frame;
        }

        public static MonitorFrame AckFrame(QueryCmd cmd)
        {
            MonitorFrame frame = new MonitorFrame();
            frame.Type = FrameType.Ack;
            frame.Command = (byte)cmd;
            return frame;
        }
    }

    public enum FrameType
    {
        Control = 1,
        Value,
        GraphControl,
        StateQuery,
        Start,
        Stop,
        ParameterQuery,
        Ack,
        Query,
        GraphData,
        Invalid = 0xff,
    }

    public enum QueryCmd
    {
        StateQuery = 0,
        ParameterQuery = 1,
        CommandQuery = 2,
    }

    public enum AckCmd
    {
        StateAck = 10,
        StartAck = 11,
        StopAck = 12,
    }

    public enum GraphCmd
    {
        Read = 20,
        Lock = 21,
        UnLock = 22,
        Data = 23,
        SetRatio = 24,
        SetLogVariable = 25,
        EndOfData = 26
    }

    public enum FrameDataType
    {
        IntIndex = 1,
        Float = 2,
        Int32 = 3,
        Int16 = 4
    }

    public enum DeviceStatusIndex
    {
        Stop,
        Run,
        Init,
        Error,
    }

    public enum DeviceModeIndex
    {
        Speed = 1,
        Torque,
        Position,
    }

    public enum DeviceDirectionIndex
    {
        CW = 1,
        CCW,
    }
}
