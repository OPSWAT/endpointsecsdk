namespace AcmeScanner
{
    partial class ViewMobyJsonDialog
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
            TB_MOBY_JSON = new System.Windows.Forms.TextBox();
            btnDownloadMobyJson = new MaterialSkin.Controls.MaterialButton();
            btnCloseViewJsonDialog = new MaterialSkin.Controls.MaterialButton();
            SuspendLayout();
            // 
            // TB_MOBY_JSON
            // 
            TB_MOBY_JSON.Location = new System.Drawing.Point(4, 4);
            TB_MOBY_JSON.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            TB_MOBY_JSON.Multiline = true;
            TB_MOBY_JSON.Name = "TB_MOBY_JSON";
            TB_MOBY_JSON.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            TB_MOBY_JSON.Size = new System.Drawing.Size(701, 356);
            TB_MOBY_JSON.TabIndex = 0;
            // 
            // btnDownloadMobyJson
            // 
            btnDownloadMobyJson.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnDownloadMobyJson.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnDownloadMobyJson.Depth = 0;
            btnDownloadMobyJson.HighEmphasis = true;
            btnDownloadMobyJson.Icon = null;
            btnDownloadMobyJson.Location = new System.Drawing.Point(388, 378);
            btnDownloadMobyJson.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            btnDownloadMobyJson.MouseState = MaterialSkin.MouseState.HOVER;
            btnDownloadMobyJson.Name = "btnDownloadMobyJson";
            btnDownloadMobyJson.NoAccentTextColor = System.Drawing.Color.Empty;
            btnDownloadMobyJson.Size = new System.Drawing.Size(176, 36);
            btnDownloadMobyJson.TabIndex = 1;
            btnDownloadMobyJson.Text = "Download JSON file";
            btnDownloadMobyJson.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnDownloadMobyJson.UseAccentColor = false;
            btnDownloadMobyJson.UseVisualStyleBackColor = true;
            btnDownloadMobyJson.Click += btnDownloadMobyJson_Click;
            // 
            // btnCloseViewJsonDialog
            // 
            btnCloseViewJsonDialog.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnCloseViewJsonDialog.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnCloseViewJsonDialog.Depth = 0;
            btnCloseViewJsonDialog.HighEmphasis = true;
            btnCloseViewJsonDialog.Icon = null;
            btnCloseViewJsonDialog.Location = new System.Drawing.Point(574, 378);
            btnCloseViewJsonDialog.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            btnCloseViewJsonDialog.MouseState = MaterialSkin.MouseState.HOVER;
            btnCloseViewJsonDialog.Name = "btnCloseViewJsonDialog";
            btnCloseViewJsonDialog.NoAccentTextColor = System.Drawing.Color.Empty;
            btnCloseViewJsonDialog.Size = new System.Drawing.Size(66, 36);
            btnCloseViewJsonDialog.TabIndex = 2;
            btnCloseViewJsonDialog.Text = "Close";
            btnCloseViewJsonDialog.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnCloseViewJsonDialog.UseAccentColor = false;
            btnCloseViewJsonDialog.UseVisualStyleBackColor = true;
            btnCloseViewJsonDialog.Click += btnCloseViewJsonDialog_Click;
            // 
            // ViewMobyJsonDialog
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(712, 423);
            Controls.Add(btnCloseViewJsonDialog);
            Controls.Add(btnDownloadMobyJson);
            Controls.Add(TB_MOBY_JSON);
            Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            Name = "ViewMobyJsonDialog";
            Text = "ViewMobyJsonDialog";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox TB_MOBY_JSON;
        private MaterialSkin.Controls.MaterialButton btnDownloadMobyJson;
        private MaterialSkin.Controls.MaterialButton btnCloseViewJsonDialog;
    }
}