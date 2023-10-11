///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Windows.Forms;

namespace AcmeScanner
{
    internal class CustomMessageDialog : Form
    {

        private MaterialSkin.Controls.MaterialMultiLineTextBox2 tbMessage;
        private MaterialSkin.Controls.MaterialButton btnCancelClose;
        private MaterialSkin.Controls.MaterialButton btnOK;
        public bool questionResult;

        private void InitializeComponent()
        {
            this.tbMessage = new MaterialSkin.Controls.MaterialMultiLineTextBox2();
            this.btnCancelClose = new MaterialSkin.Controls.MaterialButton();
            this.btnOK = new MaterialSkin.Controls.MaterialButton();
            this.SuspendLayout();
            // 
            // tbMessage
            // 
            this.tbMessage.AnimateReadOnly = false;
            this.tbMessage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.tbMessage.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.tbMessage.Depth = 0;
            this.tbMessage.Enabled = false;
            this.tbMessage.HideSelection = true;
            this.tbMessage.Location = new System.Drawing.Point(12, 12);
            this.tbMessage.MaxLength = 32767;
            this.tbMessage.MouseState = MaterialSkin.MouseState.OUT;
            this.tbMessage.Name = "tbMessage";
            this.tbMessage.PasswordChar = '\0';
            this.tbMessage.ReadOnly = false;
            this.tbMessage.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.tbMessage.SelectedText = "";
            this.tbMessage.SelectionLength = 0;
            this.tbMessage.SelectionStart = 0;
            this.tbMessage.ShortcutsEnabled = true;
            this.tbMessage.Size = new System.Drawing.Size(432, 192);
            this.tbMessage.TabIndex = 0;
            this.tbMessage.TabStop = false;
            this.tbMessage.Text = "tbMessage";
            this.tbMessage.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.tbMessage.UseSystemPasswordChar = false;
            // 
            // btnCancelClose
            // 
            this.btnCancelClose.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnCancelClose.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnCancelClose.Depth = 0;
            this.btnCancelClose.HighEmphasis = true;
            this.btnCancelClose.Icon = null;
            this.btnCancelClose.Location = new System.Drawing.Point(377, 213);
            this.btnCancelClose.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnCancelClose.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnCancelClose.Name = "btnCancelClose";
            this.btnCancelClose.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnCancelClose.Size = new System.Drawing.Size(65, 36);
            this.btnCancelClose.TabIndex = 1;
            this.btnCancelClose.Text = "Close";
            this.btnCancelClose.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnCancelClose.UseAccentColor = false;
            this.btnCancelClose.UseVisualStyleBackColor = true;
            this.btnCancelClose.Click += new System.EventHandler(this.btnCancelClose_Click);
            // 
            // btnOK
            // 
            this.btnOK.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnOK.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnOK.Depth = 0;
            this.btnOK.HighEmphasis = true;
            this.btnOK.Icon = null;
            this.btnOK.Location = new System.Drawing.Point(305, 213);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnOK.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnOK.Name = "btnOK";
            this.btnOK.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnOK.Size = new System.Drawing.Size(64, 36);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnOK.UseAccentColor = false;
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // CustomMessageDialog
            // 
            this.ClientSize = new System.Drawing.Size(456, 261);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancelClose);
            this.Controls.Add(this.tbMessage);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CustomMessageDialog";
            this.ShowIcon = false;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        public CustomMessageDialog(string message, bool question)
        {
            InitializeComponent();
            tbMessage.Text = message;

            if(!question)
            {
                btnOK.Visible = false;
            }
        }

        public bool IsSuccess()
        {
            return questionResult;
        }

        private void btnCancelClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            questionResult = true;
            Close();
        }
    }
}
