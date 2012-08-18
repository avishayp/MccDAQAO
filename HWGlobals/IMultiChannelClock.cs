using System;
using System.Collections.Generic;

namespace HWGlobals
{    
    public struct Trig
    { // for finite config
        public int Frequency;
        public int Pulses;
        public bool IsHigh;

        public static readonly int TTL_NORMAL = 3;
        public static readonly int TTL_HIGH = 5;
    }

    public interface IMultiChannelClock
    {
        bool Init(Object condata);
        bool ConfigContTrig(int[] freqs);   // config all channels for continuous triggering
        bool ConfigFiniteTrig(Trig[] trigs);   // config all channels for finite triggering
        bool StartAll();
        void StopAll();
        bool IsRunning { get; }
        bool IsExtTrig { get; set; }
    }

    public class IMultiChannelClock_moq : IMultiChannelClock
    {
        private const bool retval = true;

        public bool Init(Object condata) { return retval; }
        public bool ConfigContTrig(int[] freqs) { return retval; }
        public bool ConfigFiniteTrig(Trig[] trigs) { return retval; }
        public bool StartAll() { return retval; }
        public void StopAll() { }
        public bool IsRunning { get { return retval; } }
        public bool IsExtTrig { get; set; }
    }

}