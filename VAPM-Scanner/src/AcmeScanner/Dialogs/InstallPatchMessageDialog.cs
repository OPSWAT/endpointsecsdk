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
            tbMessage = new MaterialSkin.Controls.MaterialMultiLineTextBox2();
            btnCancelClose = new MaterialSkin.Controls.MaterialButton();
            btnOK = new MaterialSkin.Controls.MaterialButton();
            cbBackgroundInstall = new CheckBox();
            cbValidateInstaller = new CheckBox();
            cbForceClose = new CheckBox();
            cbUsePatchId = new CheckBox();
            SuspendLayout();
            // 
            // tbMessage
            // 
            tbMessage.AnimateReadOnly = false;
            tbMessage.BackgroundImageLayout = ImageLayout.None;
            tbMessage.CharacterCasing = CharacterCasing.Normal;
            tbMessage.Depth = 0;
            tbMessage.Enabled = false;
            tbMessage.HideSelection = true;
            tbMessage.Location = new System.Drawing.Point(12, 12);
            tbMessage.MaxLength = 32767;
            tbMessage.MouseState = MaterialSkin.MouseState.OUT;
            tbMessage.Name = "tbMessage";
            tbMessage.PasswordChar = '\0';
            tbMessage.ReadOnly = false;
            tbMessage.ScrollBars = ScrollBars.None;
            tbMessage.SelectedText = "";
            tbMessage.SelectionLength = 0;
            tbMessage.SelectionStart = 0;
            tbMessage.ShortcutsEnabled = true;
            tbMessage.Size = new System.Drawing.Size(432, 141);
            tbMessage.TabIndex = 0;
            tbMessage.TabStop = false;
            tbMessage.Text = "tbMessage";
            tbMessage.TextAlign = HorizontalAlignment.Left;
            tbMessage.UseSystemPasswordChar = false;
            // 
            // btnCancelClose
            // 
            btnCancelClose.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnCancelClose.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnCancelClose.Depth = 0;
            btnCancelClose.HighEmphasis = true;
            btnCancelClose.Icon = null;
            btnCancelClose.Location = new System.Drawing.Point(379, 159);
            btnCancelClose.Margin = new Padding(4, 6, 4, 6);
            btnCancelClose.MouseState = MaterialSkin.MouseState.HOVER;
            btnCancelClose.Name = "btnCancelClose";
            btnCancelClose.NoAccentTextColor = System.Drawing.Color.Empty;
            btnCancelClose.Size = new System.Drawing.Size(66, 36);
            btnCancelClose.TabIndex = 1;
            btnCancelClose.Text = "Close";
            btnCancelClose.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnCancelClose.UseAccentColor = false;
            btnCancelClose.UseVisualStyleBackColor = true;
            btnCancelClose.Click += new EventHandler(btnCancelClose_Click);
            // 
            // btnOK
            // 
            btnOK.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnOK.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnOK.Depth = 0;
            btnOK.HighEmphasis = true;
            btnOK.Icon = null;
            btnOK.Location = new System.Drawing.Point(304, 159);
            btnOK.Margin = new Padding(4, 6, 4, 6);
            btnOK.MouseState = MaterialSkin.MouseState.HOVER;
            btnOK.Name = "btnOK";
            btnOK.NoAccentTextColor = System.Drawing.Color.Empty;
            btnOK.Size = new System.Drawing.Size(64, 36);
            btnOK.TabIndex = 2;
            btnOK.Text = "OK";
            btnOK.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnOK.UseAccentColor = false;
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += new EventHandler(btnOK_Click);
            // 
            // cbBackgroundInstall
            // 
            cbBackgroundInstall.AutoSize = true;
            cbBackgroundInstall.Location = new System.Drawing.Point(12, 159);
            cbBackgroundInstall.Name = "cbBackgroundInstall";
            cbBackgroundInstall.Size = new System.Drawing.Size(137, 19);
            cbBackgroundInstall.TabIndex = 3;
            cbBackgroundInstall.Text = "Install in Background";
            cbBackgroundInstall.UseVisualStyleBackColor = true;
            // 
            // cbValidateInstaller
            // 
            cbValidateInstaller.AutoSize = true;
            cbValidateInstaller.Location = new System.Drawing.Point(155, 159);
            cbValidateInstaller.Name = "cbValidateInstaller";
            cbValidateInstaller.Size = new System.Drawing.Size(111, 19);
            cbValidateInstaller.TabIndex = 4;
            cbValidateInstaller.Text = "Validate Installer";
            cbValidateInstaller.UseVisualStyleBackColor = true;
            // 
            // cbForceClose
            // 
            cbForceClose.AutoSize = true;
            cbForceClose.Location = new System.Drawing.Point(12, 177);
            cbForceClose.Name = "cbForceClose";
            cbForceClose.Size = new System.Drawing.Size(87, 19);
            cbForceClose.TabIndex = 5;
            cbForceClose.Text = "Force Close";
            cbForceClose.UseVisualStyleBackColor = true;
            // 
            // cbUsePatchId
            // 
            cbUsePatchId.AutoSize = true;
            cbUsePatchId.Location = new System.Drawing.Point(155, 177);
            cbUsePatchId.Name = "cbUsePatchId";
            cbUsePatchId.Size = new System.Drawing.Size(91, 19);
            cbUsePatchId.TabIndex = 6;
            cbUsePatchId.Text = "Use Patch Id";
            cbUsePatchId.UseVisualStyleBackColor = true;
            // 
            // InstallPatchMessageDialog
            // 
            ClientSize = new System.Drawing.Size(456, 208);
            Controls.Add(cbUsePatchId);
            Controls.Add(cbForceClose);
            Controls.Add(cbValidateInstaller);
            Controls.Add(cbBackgroundInstall);
            Controls.Add(btnOK);
            Controls.Add(btnCancelClose);
            Controls.Add(tbMessage);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "InstallPatchMessageDialog";
            ShowIcon = false;
            ResumeLayout(false);
            PerformLayout();

        }

        public InstallPatchMessageDialog(string message, bool question)
        {
            InitializeComponent();
            tbMessage.Text = message;

            if (!question)
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
            StartPosition = startPosition;
        }

        void IScannerMessageDialog.ShowDialog()
        {
            ShowDialog();
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
