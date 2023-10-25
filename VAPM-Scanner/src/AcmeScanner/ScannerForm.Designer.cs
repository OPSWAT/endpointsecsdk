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
            this.tabCatalog = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.lvScanResults = new AcmeScanner.ScannerListView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.lvOrchestrationScanResult = new AcmeScanner.ScannerListView();
            this.panel4 = new System.Windows.Forms.Panel();
            this.btnUpdateSDK = new MaterialSkin.Controls.MaterialButton();
            this.btnInstallOrchestration = new MaterialSkin.Controls.MaterialButton();
            this.btnScanOrchestration = new MaterialSkin.Controls.MaterialButton();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.lvCatalog = new AcmeScanner.ScannerListView();
            this.panel5 = new System.Windows.Forms.Panel();
            this.btnExportCSV = new MaterialSkin.Controls.MaterialButton();
            this.lblTotalInstalls = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblTotalCVEs = new System.Windows.Forms.Label();
            this.lblTotalProducts = new System.Windows.Forms.Label();
            this.btnLookupCVE = new MaterialSkin.Controls.MaterialButton();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnListCatalogCVE = new MaterialSkin.Controls.MaterialButton();
            this.mbLoad = new MaterialSkin.Controls.MaterialButton();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbLoading)).BeginInit();
            this.panel2.SuspendLayout();
            this.tabCatalog.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.panel4.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.panel5.SuspendLayout();
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
            this.btnCVEJSON.Text = "List CVEs";
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
            this.panel2.Controls.Add(this.tabCatalog);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(15, 15);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1068, 652);
            this.panel2.TabIndex = 2;
            // 
            // tabCatalog
            // 
            this.tabCatalog.Controls.Add(this.tabPage1);
            this.tabCatalog.Controls.Add(this.tabPage2);
            this.tabCatalog.Controls.Add(this.tabPage3);
            this.tabCatalog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabCatalog.Location = new System.Drawing.Point(0, 0);
            this.tabCatalog.Name = "tabCatalog";
            this.tabCatalog.SelectedIndex = 0;
            this.tabCatalog.Size = new System.Drawing.Size(1066, 650);
            this.tabCatalog.TabIndex = 3;
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
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.lvCatalog);
            this.tabPage3.Controls.Add(this.panel5);
            this.tabPage3.Location = new System.Drawing.Point(4, 24);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(1058, 622);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Catalog";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // lvCatalog
            // 
            this.lvCatalog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvCatalog.FullRowSelect = true;
            this.lvCatalog.GridLines = true;
            this.lvCatalog.Location = new System.Drawing.Point(3, 80);
            this.lvCatalog.MultiSelect = false;
            this.lvCatalog.Name = "lvCatalog";
            this.lvCatalog.OwnerDraw = true;
            this.lvCatalog.Size = new System.Drawing.Size(1052, 539);
            this.lvCatalog.TabIndex = 6;
            this.lvCatalog.UseCompatibleStateImageBehavior = false;
            this.lvCatalog.View = System.Windows.Forms.View.Details;
            // 
            // panel5
            // 
            this.panel5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel5.Controls.Add(this.btnExportCSV);
            this.panel5.Controls.Add(this.lblTotalInstalls);
            this.panel5.Controls.Add(this.label3);
            this.panel5.Controls.Add(this.lblTotalCVEs);
            this.panel5.Controls.Add(this.lblTotalProducts);
            this.panel5.Controls.Add(this.btnLookupCVE);
            this.panel5.Controls.Add(this.label2);
            this.panel5.Controls.Add(this.label1);
            this.panel5.Controls.Add(this.btnListCatalogCVE);
            this.panel5.Controls.Add(this.mbLoad);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel5.Location = new System.Drawing.Point(3, 3);
            this.panel5.Margin = new System.Windows.Forms.Padding(15);
            this.panel5.Name = "panel5";
            this.panel5.Padding = new System.Windows.Forms.Padding(15);
            this.panel5.Size = new System.Drawing.Size(1052, 77);
            this.panel5.TabIndex = 5;
            // 
            // btnExportCSV
            // 
            this.btnExportCSV.AutoSize = false;
            this.btnExportCSV.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnExportCSV.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnExportCSV.Depth = 0;
            this.btnExportCSV.HighEmphasis = true;
            this.btnExportCSV.Icon = null;
            this.btnExportCSV.Location = new System.Drawing.Point(921, 21);
            this.btnExportCSV.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnExportCSV.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnExportCSV.Name = "btnExportCSV";
            this.btnExportCSV.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnExportCSV.Size = new System.Drawing.Size(110, 36);
            this.btnExportCSV.TabIndex = 11;
            this.btnExportCSV.Text = "Export CSV";
            this.btnExportCSV.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnExportCSV.UseAccentColor = false;
            this.btnExportCSV.UseVisualStyleBackColor = true;
            this.btnExportCSV.Click += new System.EventHandler(this.btnExportCSV_Click);
            // 
            // lblTotalInstalls
            // 
            this.lblTotalInstalls.AutoSize = true;
            this.lblTotalInstalls.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTotalInstalls.Location = new System.Drawing.Point(311, 47);
            this.lblTotalInstalls.Name = "lblTotalInstalls";
            this.lblTotalInstalls.Size = new System.Drawing.Size(19, 21);
            this.lblTotalInstalls.TabIndex = 9;
            this.lblTotalInstalls.Text = "0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label3.Location = new System.Drawing.Point(182, 47);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(114, 21);
            this.label3.TabIndex = 8;
            this.label3.Text = "Total Install\'s:";
            // 
            // lblTotalCVEs
            // 
            this.lblTotalCVEs.AutoSize = true;
            this.lblTotalCVEs.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTotalCVEs.Location = new System.Drawing.Point(311, 27);
            this.lblTotalCVEs.Name = "lblTotalCVEs";
            this.lblTotalCVEs.Size = new System.Drawing.Size(19, 21);
            this.lblTotalCVEs.TabIndex = 7;
            this.lblTotalCVEs.Text = "0";
            // 
            // lblTotalProducts
            // 
            this.lblTotalProducts.AutoSize = true;
            this.lblTotalProducts.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTotalProducts.Location = new System.Drawing.Point(311, 6);
            this.lblTotalProducts.Name = "lblTotalProducts";
            this.lblTotalProducts.Size = new System.Drawing.Size(19, 21);
            this.lblTotalProducts.TabIndex = 6;
            this.lblTotalProducts.Text = "0";
            // 
            // btnLookupCVE
            // 
            this.btnLookupCVE.AutoSize = false;
            this.btnLookupCVE.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnLookupCVE.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnLookupCVE.Depth = 0;
            this.btnLookupCVE.HighEmphasis = true;
            this.btnLookupCVE.Icon = null;
            this.btnLookupCVE.Location = new System.Drawing.Point(539, 21);
            this.btnLookupCVE.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnLookupCVE.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnLookupCVE.Name = "btnLookupCVE";
            this.btnLookupCVE.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnLookupCVE.Size = new System.Drawing.Size(124, 36);
            this.btnLookupCVE.TabIndex = 4;
            this.btnLookupCVE.Text = "Lookup CVE";
            this.btnLookupCVE.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnLookupCVE.UseAccentColor = false;
            this.btnLookupCVE.UseVisualStyleBackColor = true;
            this.btnLookupCVE.Click += new System.EventHandler(this.btnLookupCVE_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label2.Location = new System.Drawing.Point(182, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(97, 21);
            this.label2.TabIndex = 3;
            this.label2.Text = "Total CVE\'s:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label1.Location = new System.Drawing.Point(182, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(123, 21);
            this.label1.TabIndex = 2;
            this.label1.Text = "Total Products:";
            // 
            // btnListCatalogCVE
            // 
            this.btnListCatalogCVE.AutoSize = false;
            this.btnListCatalogCVE.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnListCatalogCVE.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnListCatalogCVE.Depth = 0;
            this.btnListCatalogCVE.HighEmphasis = true;
            this.btnListCatalogCVE.Icon = null;
            this.btnListCatalogCVE.Location = new System.Drawing.Point(803, 21);
            this.btnListCatalogCVE.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnListCatalogCVE.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnListCatalogCVE.Name = "btnListCatalogCVE";
            this.btnListCatalogCVE.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnListCatalogCVE.Size = new System.Drawing.Size(110, 36);
            this.btnListCatalogCVE.TabIndex = 1;
            this.btnListCatalogCVE.Text = "Product CVES";
            this.btnListCatalogCVE.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnListCatalogCVE.UseAccentColor = false;
            this.btnListCatalogCVE.UseVisualStyleBackColor = true;
            this.btnListCatalogCVE.Click += new System.EventHandler(this.btnListCatalogCVE_Click);
            // 
            // mbLoad
            // 
            this.mbLoad.AutoSize = false;
            this.mbLoad.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.mbLoad.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.mbLoad.Depth = 0;
            this.mbLoad.HighEmphasis = true;
            this.mbLoad.Icon = null;
            this.mbLoad.Location = new System.Drawing.Point(19, 21);
            this.mbLoad.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.mbLoad.MouseState = MaterialSkin.MouseState.HOVER;
            this.mbLoad.Name = "mbLoad";
            this.mbLoad.NoAccentTextColor = System.Drawing.Color.Empty;
            this.mbLoad.Size = new System.Drawing.Size(124, 36);
            this.mbLoad.TabIndex = 0;
            this.mbLoad.Text = "Load Latest";
            this.mbLoad.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.mbLoad.UseAccentColor = false;
            this.mbLoad.UseVisualStyleBackColor = true;
            this.mbLoad.Click += new System.EventHandler(this.mbLoad_Click);
            // 
            // ScannerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1098, 682);
            this.Controls.Add(this.panel3);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ScannerForm";
            this.Text = "Acme Scanner";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbLoading)).EndInit();
            this.panel2.ResumeLayout(false);
            this.tabCatalog.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private Panel panel1;
        private Panel panel3;
        private Panel panel2;
        private MaterialSkin.Controls.MaterialButton btnScan;
        private ScannerListView lvScanResults;
        private System.Windows.Forms.Timer timer1;
        private TabControl tabCatalog;
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
        private TabPage tabPage3;
        private ScannerListView lvCatalog;
        private Panel panel5;
        private MaterialSkin.Controls.MaterialButton mbLoad;
        private MaterialSkin.Controls.MaterialButton btnListCatalogCVE;
        private MaterialSkin.Controls.MaterialButton btnLookupCVE;
        private Label label2;
        private Label label1;
        private Label lblTotalCVEs;
        private Label lblTotalProducts;
        private MaterialSkin.Controls.MaterialTextBox2 tbCVE;
        private Label lblTotalInstalls;
        private Label label3;
        private MaterialSkin.Controls.MaterialButton btnExportCSV;
    }
}