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
    internal class InstallPatchMessageDialog : Form, IScannerMessageDialog
    {

        private MaterialSkin.Controls.MaterialMultiLineTextBox2 tbMessage;
        private MaterialSkin.Controls.MaterialButton btnCancelClose;
        private MaterialSkin.Controls.MaterialButton btnOK;
        private CheckBox cbBackgroundInstall;
        private CheckBox cbValidateInstaller;
        private CheckBox cbForceClose;
        private CheckBox cbUsePatchId;
        public bool questionResult;

        private void InitializeComponent()
        {
            this.tbMessage = new MaterialSkin.Controls.MaterialMultiLineTextBox2();
            this.btnCancelClose = new MaterialSkin.Controls.MaterialButton();
            this.btnOK = new MaterialSkin.Controls.MaterialButton();
            this.cbBackgroundInstall = new System.Windows.Forms.CheckBox();
            this.cbValidateInstaller = new System.Windows.Forms.CheckBox();
            this.cbForceClose = new System.Windows.Forms.CheckBox();
            this.cbUsePatchId = new System.Windows.Forms.CheckBox();
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
            this.tbMessage.Size = new System.Drawing.Size(432, 141);
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
            this.btnCancelClose.Location = new System.Drawing.Point(379, 159);
            this.btnCancelClose.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnCancelClose.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnCancelClose.Name = "btnCancelClose";
            this.btnCancelClose.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnCancelClose.Size = new System.Drawing.Size(66, 36);
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
            this.btnOK.Location = new System.Drawing.Point(304, 159);
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
            // cbBackgroundInstall
            // 
            this.cbBackgroundInstall.AutoSize = true;
            this.cbBackgroundInstall.Location = new System.Drawing.Point(12, 159);
            this.cbBackgroundInstall.Name = "cbBackgroundInstall";
            this.cbBackgroundInstall.Size = new System.Drawing.Size(137, 19);
            this.cbBackgroundInstall.TabIndex = 3;
            this.cbBackgroundInstall.Text = "Install in Background";
            this.cbBackgroundInstall.UseVisualStyleBackColor = true;
            // 
            // cbValidateInstaller
            // 
            this.cbValidateInstaller.AutoSize = true;
            this.cbValidateInstaller.Location = new System.Drawing.Point(155, 159);
            this.cbValidateInstaller.Name = "cbValidateInstaller";
            this.cbValidateInstaller.Size = new System.Drawing.Size(111, 19);
            this.cbValidateInstaller.TabIndex = 4;
            this.cbValidateInstaller.Text = "Validate Installer";
            this.cbValidateInstaller.UseVisualStyleBackColor = true;
            // 
            // cbForceClose
            // 
            this.cbForceClose.AutoSize = true;
            this.cbForceClose.Location = new System.Drawing.Point(12, 177);
            this.cbForceClose.Name = "cbForceClose";
            this.cbForceClose.Size = new System.Drawing.Size(87, 19);
            this.cbForceClose.TabIndex = 5;
            this.cbForceClose.Text = "Force Close";
            this.cbForceClose.UseVisualStyleBackColor = true;
            // 
            // cbUsePatchId
            // 
            this.cbUsePatchId.AutoSize = true;
            this.cbUsePatchId.Location = new System.Drawing.Point(155, 177);
            this.cbUsePatchId.Name = "cbUsePatchId";
            this.cbUsePatchId.Size = new System.Drawing.Size(91, 19);
            this.cbUsePatchId.TabIndex = 6;
            this.cbUsePatchId.Text = "Use Patch Id";
            this.cbUsePatchId.UseVisualStyleBackColor = true;
            // 
            // InstallPatchMessageDialog
            // 
            this.ClientSize = new System.Drawing.Size(456, 208);
            this.Controls.Add(this.cbUsePatchId);
            this.Controls.Add(this.cbForceClose);
            this.Controls.Add(this.cbValidateInstaller);
            this.Controls.Add(this.cbBackgroundInstall);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancelClose);
            this.Controls.Add(this.tbMessage);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InstallPatchMessageDialog";
            this.ShowIcon = false;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        public InstallPatchMessageDialog(string message, bool question)
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

        public void SetStartPosition(FormStartPosition startPosition)
        {
            this.StartPosition = startPosition;
        }

        void IScannerMessageDialog.ShowDialog()
        {
            this.ShowDialog();
        }

        public bool IsBackgroundInstall()
        {
            return cbBackgroundInstall.Checked;
        }

        public bool IsValidateInstaller()
        {
            return cbValidateInstaller.Checked;
        }

        public bool IsForceClose()
        {
            return cbForceClose.Checked;
        }

        public bool UsePatchId()
        {
            return cbUsePatchId.Checked;
        }

    }
}
