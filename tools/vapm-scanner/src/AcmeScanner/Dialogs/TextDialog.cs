///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Windows.Forms;

namespace AcmeScanner.Dialogs
{
    internal class TextDialog : Form
    {
        private Panel panel1;
        private MaterialSkin.Controls.MaterialMultiLineTextBox rtbText;
        private MaterialSkin.Controls.MaterialButton btnClose;

        private void InitializeComponent()
        {
            panel1 = new Panel();
            rtbText = new MaterialSkin.Controls.MaterialMultiLineTextBox();
            btnClose = new MaterialSkin.Controls.MaterialButton();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BorderStyle = BorderStyle.Fixed3D;
            panel1.Controls.Add(rtbText);
            panel1.Location = new System.Drawing.Point(12, 12);
            panel1.Name = "panel1";
            panel1.Padding = new Padding(10);
            panel1.Size = new System.Drawing.Size(1032, 555);
            panel1.TabIndex = 0;
            // 
            // rtbText
            // 
            rtbText.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
            rtbText.BorderStyle = BorderStyle.None;
            rtbText.Depth = 0;
            rtbText.Dock = DockStyle.Fill;
            rtbText.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            rtbText.ForeColor = System.Drawing.Color.FromArgb(222, 0, 0, 0);
            rtbText.Location = new System.Drawing.Point(10, 10);
            rtbText.MouseState = MaterialSkin.MouseState.HOVER;
            rtbText.Name = "rtbText";
            rtbText.Size = new System.Drawing.Size(1008, 531);
            rtbText.TabIndex = 0;
            rtbText.Text = "";
            // 
            // btnClose
            // 
            btnClose.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnClose.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnClose.Depth = 0;
            btnClose.HighEmphasis = true;
            btnClose.Icon = null;
            btnClose.Location = new System.Drawing.Point(978, 576);
            btnClose.Margin = new Padding(4, 6, 4, 6);
            btnClose.MouseState = MaterialSkin.MouseState.HOVER;
            btnClose.Name = "btnClose";
            btnClose.NoAccentTextColor = System.Drawing.Color.Empty;
            btnClose.Size = new System.Drawing.Size(66, 36);
            btnClose.TabIndex = 1;
            btnClose.Text = "Close";
            btnClose.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnClose.UseAccentColor = false;
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += new EventHandler(btnClose_Click);
            // 
            // TextDialog
            // 
            ClientSize = new System.Drawing.Size(1056, 629);
            Controls.Add(btnClose);
            Controls.Add(panel1);
            Name = "TextDialog";
            panel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }

        public TextDialog(string text)
        {
            InitializeComponent();
            rtbText.Text = text;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
