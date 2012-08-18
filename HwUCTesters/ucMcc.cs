using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HWGlobals;

namespace HwUCTesters
{
    public partial class ucMcc : UserControl
    {
        public ucMcc()
        {
            InitializeComponent();
            worker = new MccWorker();
            InitTimer();
        }

        private void InitTimer()
        {
            runTimer = new Timer();
            runTimer.Interval = 50;
            runTimer.Tick += new EventHandler(timer_Tick);
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (!worker.Syncer.IsRunning)
            {
                isrunning = false;
                SetButtons();
                StopTimer();
            }
        }

        private void StopTimer()
        {
            runTimer.Stop();
        }

        private void SetButtons()
        {
            btnRunCont.Text = isrunning ? "Stop All" : "Run All";
            btnRunCont.BackColor = isrunning ? Color.Red : Color.Lime;
        }

        public void Init(IMultiChannelClock syncer)
        {
            worker.Init(syncer);
            this.grpAnalogOut.Enabled = true;
        }

        private MccWorker worker;
        private Timer runTimer;
        private bool isrunning;

        private void btnRunCont_Click(object sender, EventArgs e)
        {
            RunAOCont();
        }

        private void RunAOCont()
        {
            if (isrunning)
            {
                worker.Syncer.StopAll();
                isrunning = false;
            }
            else
            {
                worker.Syncer.IsExtTrig = chckExtTrig.Checked;
                if (chckCont.Checked)
                {
                    worker.Syncer.ConfigContTrig(new[] { (int)nudF0.Value, (int)nudF1.Value, (int)nudF2.Value, (int)nudF3.Value });
                }
                else
                {
                    worker.Syncer.ConfigFiniteTrig(new[] { 
                        new Trig() { Frequency = (int)nudF0.Value, Pulses = (int)nudT0.Value },
                        new Trig() { Frequency = (int)nudF1.Value, Pulses = (int)nudT1.Value },
                        new Trig() { Frequency = (int)nudF2.Value, Pulses = (int)nudT2.Value },
                        new Trig() { Frequency = (int)nudF3.Value, Pulses = (int)nudT3.Value },
                    });
                }

                isrunning = worker.Syncer.StartAll();
                if (isrunning)
                {
                    runTimer.Start();
                }
            }
            SetButtons();
        }
    }

    public class MccWorker
    {
        public IMultiChannelClock Syncer { get; private set; }

        public MccWorker()
        {
            InitMoq();
        }

        public void Init(IMultiChannelClock syncer)
        {
            Syncer = syncer;
        }

        private void InitMoq()
        {
            Syncer = new IMultiChannelClock_moq();
        }
    }
}
