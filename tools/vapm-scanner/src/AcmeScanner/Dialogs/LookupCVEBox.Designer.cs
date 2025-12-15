namespace AcmeScanner
{
    partial class LookupCVEBox
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
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            TB_CVE = new MaterialSkin.Controls.MaterialTextBox2();
            CallLookup = new MaterialSkin.Controls.MaterialButton();
            download_json = new MaterialSkin.Controls.MaterialButton();
            SuspendLayout();
            // 
            // TB_CVE
            // 
            TB_CVE.AnimateReadOnly = false;
            TB_CVE.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.None;
            TB_CVE.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.None;
            TB_CVE.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            TB_CVE.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            TB_CVE.Depth = 0;
            TB_CVE.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            TB_CVE.HideSelection = true;
            TB_CVE.LeadingIcon = null;
            TB_CVE.Location = new System.Drawing.Point(247, 69);
            TB_CVE.MaxLength = 32767;
            TB_CVE.MouseState = MaterialSkin.MouseState.OUT;
            TB_CVE.Name = "TB_CVE";
            TB_CVE.PasswordChar = '\0';
            TB_CVE.PrefixSuffixText = null;
            TB_CVE.ReadOnly = false;
            TB_CVE.RightToLeft = System.Windows.Forms.RightToLeft.No;
            TB_CVE.SelectedText = "";
            TB_CVE.SelectionLength = 0;
            TB_CVE.SelectionStart = 0;
            TB_CVE.ShortcutsEnabled = true;
            TB_CVE.Size = new System.Drawing.Size(296, 48);
            TB_CVE.TabIndex = 0;
            TB_CVE.TabStop = false;
            TB_CVE.Text = "Enter CVE here";
            TB_CVE.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            TB_CVE.TrailingIcon = null;
            TB_CVE.UseSystemPasswordChar = false;
            // 
            // CallLookup
            // 
            CallLookup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            CallLookup.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            CallLookup.Depth = 0;
            CallLookup.HighEmphasis = true;
            CallLookup.Icon = null;
            CallLookup.Location = new System.Drawing.Point(474, 221);
            CallLookup.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            CallLookup.MouseState = MaterialSkin.MouseState.HOVER;
            CallLookup.Name = "CallLookup";
            CallLookup.NoAccentTextColor = System.Drawing.Color.Empty;
            CallLookup.Size = new System.Drawing.Size(78, 36);
            CallLookup.TabIndex = 1;
            CallLookup.Text = "Lookup";
            CallLookup.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            CallLookup.UseAccentColor = false;
            CallLookup.UseVisualStyleBackColor = true;
            CallLookup.Click += CallLookup_Click;
            // 
            // download_json
            // 
            download_json.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            download_json.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            download_json.Depth = 0;
            download_json.HighEmphasis = true;
            download_json.Icon = null;
            download_json.Location = new System.Drawing.Point(575, 221);
            download_json.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            download_json.MouseState = MaterialSkin.MouseState.HOVER;
            download_json.Name = "download_json";
            download_json.NoAccentTextColor = System.Drawing.Color.Empty;
            download_json.Size = new System.Drawing.Size(158, 36);
            download_json.TabIndex = 2;
            download_json.Text = "Download .txt file";
            download_json.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            download_json.UseAccentColor = false;
            download_json.UseVisualStyleBackColor = true;
            download_json.Click += Download_json_Click;
            // 
            // LookupCVEBox
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(download_json);
            Controls.Add(CallLookup);
            Controls.Add(TB_CVE);
            Name = "LookupCVEBox";
            Text = "LookupCVEBox";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MaterialSkin.Controls.MaterialTextBox2 TB_CVE;
        private MaterialSkin.Controls.MaterialButton CallLookup;
        private MaterialSkin.Controls.MaterialButton download_json;
    }
}