using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HWGlobals
{
    public enum TriggerState
    {
        OFF,
        ON
    }

    public class TriggerEventArgs : EventArgs
    {
        public TriggerState State {get;set;}
    }

    public delegate void TriggerDelegate(object sender, TriggerEventArgs args);

    public interface ISync
    {
        event TriggerDelegate _TriggerState;

        void Config(ISyncCfg config);
        void Start();
        void Stop();
        void LoadSignal(uint samples, String file);
    }

    public class ISync_moq : ISync
    {
        public event TriggerDelegate _TriggerState;

        public void Config(ISyncCfg config) { }
        public void Start() { }
        public void Stop() { }
        public void LoadSignal(uint samples, String file) { }
    }

    public struct ISyncCfg
    {
        public uint DelayA; // in ns
        public uint DelayT0; // in ns
        public uint DelayT1; // in ns
        public uint NOC; // in ns
        public uint AP; // in ns
        public TrigType TriggerMode;    // 1 = FreeRunning 
        public uint CamTrigMode;

        public static ISyncCfg Default
        {
            get
            {
                return new ISyncCfg()
                {
                    DelayA = 25,
                    DelayT0 = 25,
                    DelayT1 = 25,
                    NOC = 1,
                    AP = 1,
                    TriggerMode = TrigType.FreeRun,
                    CamTrigMode = 0
                };
            }
        }
    }
}
