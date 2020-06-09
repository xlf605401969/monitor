using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitor2.CAN
{
    public class CANFrame
    {
        public byte[] data = new byte[8];

        public byte FrameType
        {
            get { return data[0]; }
            set
            {
                data[0] = value;
            }
        }

        public byte FrameIndex
        {
            get { return data[1]; }
            set
            {
                data[1] = value;
            }
        }

        public byte DataType
        {
            get { return data[2]; }
            set
            {
                data[2] = value;
            }
        }

        public byte IndexValue
        {
            get { return data[3]; }
            set
            {
                data[3] = value;
            }
        }
        
        public float Value { get; set; }
        public int IntValue { get; set; }

    }

    public enum CANFrameType
    {
        Control = 1,
        Para,
        Graph,
        Status,
        Start,
        Stop,
        Query,
        ACK,
    }

    public enum CANQueryIndex
    {
        Status,
        Para,
    }

    public enum CANACKIndex
    {
        Status,
        Start,
        Stop,
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

    public enum GraphCmd
    {
        Read = 0,
        Lock = 1,
        Unlock = 2,
    }
}
