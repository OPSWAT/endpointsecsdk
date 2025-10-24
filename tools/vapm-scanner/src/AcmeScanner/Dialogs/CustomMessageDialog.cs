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
    internal class CustomMessageDialog : Form, IScannerMessageDialog
    {

        private MaterialSkin.Controls.MaterialMultiLineTextBox2 tbMessage;
        private MaterialSkin.Controls.MaterialButton btnCancelClose;
        private MaterialSkin.Controls.MaterialButton btnOK;
        public bool questionResult;

        private void InitializeComponent()
        {
            tbMessage = new MaterialSkin.Controls.MaterialMultiLineTextBox2();
            btnCancelClose = new MaterialSkin.Controls.MaterialButton();
            btnOK = new MaterialSkin.Controls.MaterialButton();
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
            tbMessage.Size = new System.Drawing.Size(432, 192);
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
            btnCancelClose.Location = new System.Drawing.Point(377, 213);
            btnCancelClose.Margin = new Padding(4, 6, 4, 6);
            btnCancelClose.MouseState = MaterialSkin.MouseState.HOVER;
            btnCancelClose.Name = "btnCancelClose";
            btnCancelClose.NoAccentTextColor = System.Drawing.Color.Empty;
            btnCancelClose.Size = new System.Drawing.Size(65, 36);
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
            btnOK.Location = new System.Drawing.Point(305, 213);
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
            // CustomMessageDialog
            // 
            ClientSize = new System.Drawing.Size(456, 261);
            Controls.Add(btnOK);
            Controls.Add(btnCancelClose);
            Controls.Add(tbMessage);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "CustomMessageDialog";
            ShowIcon = false;
            ResumeLayout(false);
            PerformLayout();

        }

        public CustomMessageDialog(string message, bool question)
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
    }
}
