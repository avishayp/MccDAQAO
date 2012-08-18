namespace HwTesters
{
    partial class frmMccTester
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
            CloseDevice();
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ucMcc1 = new HwUCTesters.ucMcc();
            this.SuspendLayout();
            // 
            // ucMcc1
            // 
            this.ucMcc1.Location = new System.Drawing.Point(2, 2);
            this.ucMcc1.Name = "ucMcc1";
            this.ucMcc1.Size = new System.Drawing.Size(221, 255);
            this.ucMcc1.TabIndex = 0;
            // 
            // frmMccTester
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(220, 256);
            this.Controls.Add(this.ucMcc1);
            this.Name = "frmMccTester";
            this.Text = "frmMccTester";
            this.Load += new System.EventHandler(this.frmMccTester_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private HwUCTesters.ucMcc ucMcc1;
    }
}