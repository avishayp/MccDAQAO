using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HWGlobals;

namespace HwTesters
{
    public partial class frmMccTester : Form
    {
        public frmMccTester()
        {
            InitializeComponent();
        }

        MccDAQWrapper.MccDIO MccDaq; 

        private void frmMccTester_Load(object sender, EventArgs e)
        {
            MccDaq = new MccDAQWrapper.MccDIO();
            if (MccDaq.Init(0))
            {
                this.ucMcc1.Init(MccDaq as IMultiChannelClock);
            }
        }

        private void CloseDevice()
        {
            MccDaq.StopAll();
        }
    }
}
