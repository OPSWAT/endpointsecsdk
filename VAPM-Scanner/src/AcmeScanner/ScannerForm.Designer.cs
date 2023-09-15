using System.Windows.Forms;

namespace AcmeScanner
{
    partial class ScannerForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScannerForm));
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnUpdate = new MaterialSkin.Controls.MaterialButton();
            this.btnInstall = new MaterialSkin.Controls.MaterialButton();
            this.btnCVEJSON = new MaterialSkin.Controls.MaterialButton();
            this.cbScanOSCVEs = new System.Windows.Forms.CheckBox();
            this.btnScan = new MaterialSkin.Controls.MaterialButton();
            this.panel3 = new System.Windows.Forms.Panel();
            this.pbLoading = new System.Windows.Forms.PictureBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.lvScanResults = new AcmeScanner.ScannerListView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.lvOrchestrationScanResult = new AcmeScanner.ScannerListView();
            this.panel4 = new System.Windows.Forms.Panel();
            this.btnUpdateSDK = new MaterialSkin.Controls.MaterialButton();
            this.btnInstallOrchestration = new MaterialSkin.Controls.MaterialButton();
            this.btnScanOrchestration = new MaterialSkin.Controls.MaterialButton();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbLoading)).BeginInit();
            this.panel2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.btnUpdate);
            this.panel1.Controls.Add(this.btnInstall);
            this.panel1.Controls.Add(this.btnCVEJSON);
            this.panel1.Controls.Add(this.cbScanOSCVEs);
            this.panel1.Controls.Add(this.btnScan);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Margin = new System.Windows.Forms.Padding(15);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(15);
            this.panel1.Size = new System.Drawing.Size(1052, 77);
            this.panel1.TabIndex = 1;
            // 
            // btnUpdate
            // 
            this.btnUpdate.AutoSize = false;
            this.btnUpdate.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnUpdate.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnUpdate.Depth = 0;
            this.btnUpdate.HighEmphasis = true;
            this.btnUpdate.Icon = null;
            this.btnUpdate.Location = new System.Drawing.Point(925, 21);
            this.btnUpdate.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnUpdate.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnUpdate.Size = new System.Drawing.Size(106, 36);
            this.btnUpdate.TabIndex = 4;
            this.btnUpdate.Text = "Update DB";
            this.btnUpdate.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnUpdate.UseAccentColor = false;
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // btnInstall
            // 
            this.btnInstall.AutoSize = false;
            this.btnInstall.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnInstall.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnInstall.Depth = 0;
            this.btnInstall.HighEmphasis = true;
            this.btnInstall.Icon = null;
            this.btnInstall.Location = new System.Drawing.Point(811, 21);
            this.btnInstall.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnInstall.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnInstall.Name = "btnInstall";
            this.btnInstall.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnInstall.Size = new System.Drawing.Size(106, 36);
            this.btnInstall.TabIndex = 3;
            this.btnInstall.Text = "Install";
            this.btnInstall.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnInstall.UseAccentColor = false;
            this.btnInstall.UseVisualStyleBackColor = true;
            this.btnInstall.Click += new System.EventHandler(this.btnInstall_Click);
            // 
            // btnCVEJSON
            // 
            this.btnCVEJSON.AutoSize = false;
            this.btnCVEJSON.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnCVEJSON.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnCVEJSON.Depth = 0;
            this.btnCVEJSON.HighEmphasis = true;
            this.btnCVEJSON.Icon = null;
            this.btnCVEJSON.Location = new System.Drawing.Point(446, 21);
            this.btnCVEJSON.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnCVEJSON.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnCVEJSON.Name = "btnCVEJSON";
            this.btnCVEJSON.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnCVEJSON.Size = new System.Drawing.Size(106, 36);
            this.btnCVEJSON.TabIndex = 2;
            this.btnCVEJSON.Text = "CVE Json";
            this.btnCVEJSON.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnCVEJSON.UseAccentColor = false;
            this.btnCVEJSON.UseVisualStyleBackColor = true;
            this.btnCVEJSON.Click += new System.EventHandler(this.btnCVEJSON_Click);
            // 
            // cbScanOSCVEs
            // 
            this.cbScanOSCVEs.AutoSize = true;
            this.cbScanOSCVEs.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cbScanOSCVEs.Location = new System.Drawing.Point(149, 23);
            this.cbScanOSCVEs.Name = "cbScanOSCVEs";
            this.cbScanOSCVEs.Size = new System.Drawing.Size(271, 29);
            this.cbScanOSCVEs.TabIndex = 1;
            this.cbScanOSCVEs.Text = "Scan Operating System CVEs";
            this.cbScanOSCVEs.UseVisualStyleBackColor = true;
            // 
            // btnScan
            // 
            this.btnScan.AutoSize = false;
            this.btnScan.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnScan.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnScan.Depth = 0;
            this.btnScan.HighEmphasis = true;
            this.btnScan.Icon = null;
            this.btnScan.Location = new System.Drawing.Point(19, 21);
            this.btnScan.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnScan.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnScan.Name = "btnScan";
            this.btnScan.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnScan.Size = new System.Drawing.Size(106, 36);
            this.btnScan.TabIndex = 0;
            this.btnScan.Text = "Scan";
            this.btnScan.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnScan.UseAccentColor = false;
            this.btnScan.UseVisualStyleBackColor = true;
            this.btnScan.Click += new System.EventHandler(this.btnScan_Click);
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.panel3.Controls.Add(this.pbLoading);
            this.panel3.Controls.Add(this.panel2);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Margin = new System.Windows.Forms.Padding(15);
            this.panel3.Name = "panel3";
            this.panel3.Padding = new System.Windows.Forms.Padding(15);
            this.panel3.Size = new System.Drawing.Size(1098, 682);
            this.panel3.TabIndex = 2;
            // 
            // pbLoading
            // 
            this.pbLoading.BackColor = System.Drawing.SystemColors.Window;
            this.pbLoading.Image = ((System.Drawing.Image)(resources.GetObject("pbLoading.Image")));
            this.pbLoading.InitialImage = null;
            this.pbLoading.Location = new System.Drawing.Point(-328, 205);
            this.pbLoading.Name = "pbLoading";
            this.pbLoading.Size = new System.Drawing.Size(338, 309);
            this.pbLoading.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbLoading.TabIndex = 2;
            this.pbLoading.TabStop = false;
            this.pbLoading.Visible = false;
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.tabControl1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(15, 15);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1068, 652);
            this.panel2.TabIndex = 2;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1066, 650);
            this.tabControl1.TabIndex = 3;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.lvScanResults);
            this.tabPage1.Controls.Add(this.panel1);
            this.tabPage1.Location = new System.Drawing.Point(4, 24);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1058, 622);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Offline";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // lvScanResults
            // 
            this.lvScanResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvScanResults.FullRowSelect = true;
            this.lvScanResults.GridLines = true;
            this.lvScanResults.Location = new System.Drawing.Point(3, 80);
            this.lvScanResults.MultiSelect = false;
            this.lvScanResults.Name = "lvScanResults";
            this.lvScanResults.OwnerDraw = true;
            this.lvScanResults.Size = new System.Drawing.Size(1052, 539);
            this.lvScanResults.TabIndex = 2;
            this.lvScanResults.UseCompatibleStateImageBehavior = false;
            this.lvScanResults.View = System.Windows.Forms.View.Details;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.lvOrchestrationScanResult);
            this.tabPage2.Controls.Add(this.panel4);
            this.tabPage2.Location = new System.Drawing.Point(4, 24);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1058, 622);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Orchestration";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // lvOrchestrationScanResult
            // 
            this.lvOrchestrationScanResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvOrchestrationScanResult.FullRowSelect = true;
            this.lvOrchestrationScanResult.GridLines = true;
            this.lvOrchestrationScanResult.Location = new System.Drawing.Point(3, 80);
            this.lvOrchestrationScanResult.MultiSelect = false;
            this.lvOrchestrationScanResult.Name = "lvOrchestrationScanResult";
            this.lvOrchestrationScanResult.OwnerDraw = true;
            this.lvOrchestrationScanResult.Size = new System.Drawing.Size(1052, 539);
            this.lvOrchestrationScanResult.TabIndex = 4;
            this.lvOrchestrationScanResult.UseCompatibleStateImageBehavior = false;
            this.lvOrchestrationScanResult.View = System.Windows.Forms.View.Details;
            // 
            // panel4
            // 
            this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel4.Controls.Add(this.btnUpdateSDK);
            this.panel4.Controls.Add(this.btnInstallOrchestration);
            this.panel4.Controls.Add(this.btnScanOrchestration);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(3, 3);
            this.panel4.Margin = new System.Windows.Forms.Padding(15);
            this.panel4.Name = "panel4";
            this.panel4.Padding = new System.Windows.Forms.Padding(15);
            this.panel4.Size = new System.Drawing.Size(1052, 77);
            this.panel4.TabIndex = 3;
            // 
            // btnUpdateSDK
            // 
            this.btnUpdateSDK.AutoSize = false;
            this.btnUpdateSDK.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnUpdateSDK.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnUpdateSDK.Depth = 0;
            this.btnUpdateSDK.HighEmphasis = true;
            this.btnUpdateSDK.Icon = null;
            this.btnUpdateSDK.Location = new System.Drawing.Point(925, 21);
            this.btnUpdateSDK.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnUpdateSDK.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnUpdateSDK.Name = "btnUpdateSDK";
            this.btnUpdateSDK.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnUpdateSDK.Size = new System.Drawing.Size(106, 36);
            this.btnUpdateSDK.TabIndex = 4;
            this.btnUpdateSDK.Text = "Update SDK";
            this.btnUpdateSDK.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnUpdateSDK.UseAccentColor = false;
            this.btnUpdateSDK.UseVisualStyleBackColor = true;
            this.btnUpdateSDK.Click += new System.EventHandler(this.btnUpdateSDK_Click);
            // 
            // btnInstallOrchestration
            // 
            this.btnInstallOrchestration.AutoSize = false;
            this.btnInstallOrchestration.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnInstallOrchestration.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnInstallOrchestration.Depth = 0;
            this.btnInstallOrchestration.HighEmphasis = true;
            this.btnInstallOrchestration.Icon = null;
            this.btnInstallOrchestration.Location = new System.Drawing.Point(811, 21);
            this.btnInstallOrchestration.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnInstallOrchestration.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnInstallOrchestration.Name = "btnInstallOrchestration";
            this.btnInstallOrchestration.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnInstallOrchestration.Size = new System.Drawing.Size(106, 36);
            this.btnInstallOrchestration.TabIndex = 3;
            this.btnInstallOrchestration.Text = "Install";
            this.btnInstallOrchestration.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnInstallOrchestration.UseAccentColor = false;
            this.btnInstallOrchestration.UseVisualStyleBackColor = true;
            this.btnInstallOrchestration.Click += new System.EventHandler(this.btnInstallOrchestration_Click);
            // 
            // btnScanOrchestration
            // 
            this.btnScanOrchestration.AutoSize = false;
            this.btnScanOrchestration.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnScanOrchestration.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnScanOrchestration.Depth = 0;
            this.btnScanOrchestration.HighEmphasis = true;
            this.btnScanOrchestration.Icon = null;
            this.btnScanOrchestration.Location = new System.Drawing.Point(19, 21);
            this.btnScanOrchestration.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnScanOrchestration.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnScanOrchestration.Name = "btnScanOrchestration";
            this.btnScanOrchestration.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnScanOrchestration.Size = new System.Drawing.Size(106, 36);
            this.btnScanOrchestration.TabIndex = 0;
            this.btnScanOrchestration.Text = "Scan";
            this.btnScanOrchestration.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnScanOrchestration.UseAccentColor = false;
            this.btnScanOrchestration.UseVisualStyleBackColor = true;
            this.btnScanOrchestration.Click += new System.EventHandler(this.btnScanOrchestration_Click);
            // 
            // ScannerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1098, 682);
            this.Controls.Add(this.panel3);
            this.Name = "ScannerForm";
            this.Text = "Acme Scanner";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbLoading)).EndInit();
            this.panel2.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private Panel panel1;
        private Panel panel3;
        private Panel panel2;
        private MaterialSkin.Controls.MaterialButton btnScan;
        private ScannerListView lvScanResults;
        private System.Windows.Forms.Timer timer1;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private CheckBox cbScanOSCVEs;
        private PictureBox pbLoading;
        private MaterialSkin.Controls.MaterialButton btnUpdate;
        private MaterialSkin.Controls.MaterialButton btnInstall;
        private MaterialSkin.Controls.MaterialButton btnCVEJSON;
        private ScannerListView lvOrchestrationScanResult;
        private Panel panel4;
        private MaterialSkin.Controls.MaterialButton btnUpdateSDK;
        private MaterialSkin.Controls.MaterialButton btnInstallOrchestration;
        private MaterialSkin.Controls.MaterialButton btnScanOrchestration;
    }
}