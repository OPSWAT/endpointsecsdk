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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScannerForm));
            panel1 = new Panel();
            btnUpdate = new MaterialSkin.Controls.MaterialButton();
            btnInstall = new MaterialSkin.Controls.MaterialButton();
            btnCVEJSON = new MaterialSkin.Controls.MaterialButton();
            cbScanOSCVEs = new CheckBox();
            btnScan = new MaterialSkin.Controls.MaterialButton();
            panel3 = new Panel();
            pbLoading = new PictureBox();
            panel2 = new Panel();
            tabCatalog = new TabControl();
            tabPage1 = new TabPage();
            lvScanResults = new ScannerListView();
            tabPage2 = new TabPage();
            lvOrchestrationScanResult = new ScannerListView();
            panel4 = new Panel();
            btnUpdateSDK = new MaterialSkin.Controls.MaterialButton();
            btnInstallOrchestration = new MaterialSkin.Controls.MaterialButton();
            btnScanOrchestration = new MaterialSkin.Controls.MaterialButton();
            tabPage3 = new TabPage();
            lvCatalog = new ScannerListView();
            panel5 = new Panel();
            btnDomainCSV = new MaterialSkin.Controls.MaterialButton();
            btnFreshInstall = new MaterialSkin.Controls.MaterialButton();
            btnExportCSV = new MaterialSkin.Controls.MaterialButton();
            lblTotalInstalls = new Label();
            label3 = new Label();
            lblTotalCVEs = new Label();
            lblTotalProducts = new Label();
            btnLookupCVE = new MaterialSkin.Controls.MaterialButton();
            label2 = new Label();
            label1 = new Label();
            btnListCatalogCVE = new MaterialSkin.Controls.MaterialButton();
            mbLoad = new MaterialSkin.Controls.MaterialButton();
            tabPage4 = new TabPage();
            lvStatus = new ScannerListView();
            panel6 = new Panel();
            btnRefreshStatus = new MaterialSkin.Controls.MaterialButton();
            timer1 = new Timer(components);
            panel1.SuspendLayout();
            panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pbLoading).BeginInit();
            panel2.SuspendLayout();
            tabCatalog.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            panel4.SuspendLayout();
            tabPage3.SuspendLayout();
            panel5.SuspendLayout();
            tabPage4.SuspendLayout();
            panel6.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Controls.Add(btnUpdate);
            panel1.Controls.Add(btnInstall);
            panel1.Controls.Add(btnCVEJSON);
            panel1.Controls.Add(cbScanOSCVEs);
            panel1.Controls.Add(btnScan);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new System.Drawing.Point(3, 3);
            panel1.Margin = new Padding(15);
            panel1.Name = "panel1";
            panel1.Padding = new Padding(15);
            panel1.Size = new System.Drawing.Size(1052, 77);
            panel1.TabIndex = 1;
            // 
            // btnUpdate
            // 
            btnUpdate.AutoSize = false;
            btnUpdate.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnUpdate.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnUpdate.Depth = 0;
            btnUpdate.HighEmphasis = true;
            btnUpdate.Icon = null;
            btnUpdate.Location = new System.Drawing.Point(925, 21);
            btnUpdate.Margin = new Padding(4, 6, 4, 6);
            btnUpdate.MouseState = MaterialSkin.MouseState.HOVER;
            btnUpdate.Name = "btnUpdate";
            btnUpdate.NoAccentTextColor = System.Drawing.Color.Empty;
            btnUpdate.Size = new System.Drawing.Size(106, 36);
            btnUpdate.TabIndex = 4;
            btnUpdate.Text = "Update DB";
            btnUpdate.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnUpdate.UseAccentColor = false;
            btnUpdate.UseVisualStyleBackColor = true;
            btnUpdate.Click += btnUpdate_Click;
            // 
            // btnInstall
            // 
            btnInstall.AutoSize = false;
            btnInstall.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnInstall.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnInstall.Depth = 0;
            btnInstall.HighEmphasis = true;
            btnInstall.Icon = null;
            btnInstall.Location = new System.Drawing.Point(811, 21);
            btnInstall.Margin = new Padding(4, 6, 4, 6);
            btnInstall.MouseState = MaterialSkin.MouseState.HOVER;
            btnInstall.Name = "btnInstall";
            btnInstall.NoAccentTextColor = System.Drawing.Color.Empty;
            btnInstall.Size = new System.Drawing.Size(106, 36);
            btnInstall.TabIndex = 3;
            btnInstall.Text = "Install";
            btnInstall.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnInstall.UseAccentColor = false;
            btnInstall.UseVisualStyleBackColor = true;
            btnInstall.Click += btnInstall_Click;
            // 
            // btnCVEJSON
            // 
            btnCVEJSON.AutoSize = false;
            btnCVEJSON.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnCVEJSON.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnCVEJSON.Depth = 0;
            btnCVEJSON.HighEmphasis = true;
            btnCVEJSON.Icon = null;
            btnCVEJSON.Location = new System.Drawing.Point(446, 21);
            btnCVEJSON.Margin = new Padding(4, 6, 4, 6);
            btnCVEJSON.MouseState = MaterialSkin.MouseState.HOVER;
            btnCVEJSON.Name = "btnCVEJSON";
            btnCVEJSON.NoAccentTextColor = System.Drawing.Color.Empty;
            btnCVEJSON.Size = new System.Drawing.Size(106, 36);
            btnCVEJSON.TabIndex = 2;
            btnCVEJSON.Text = "List CVEs";
            btnCVEJSON.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnCVEJSON.UseAccentColor = false;
            btnCVEJSON.UseVisualStyleBackColor = true;
            btnCVEJSON.Click += btnCVEJSON_Click;
            // 
            // cbScanOSCVEs
            // 
            cbScanOSCVEs.AutoSize = true;
            cbScanOSCVEs.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            cbScanOSCVEs.Location = new System.Drawing.Point(149, 23);
            cbScanOSCVEs.Name = "cbScanOSCVEs";
            cbScanOSCVEs.Size = new System.Drawing.Size(147, 29);
            cbScanOSCVEs.TabIndex = 1;
            cbScanOSCVEs.Text = "Scan OS CVEs";
            cbScanOSCVEs.UseVisualStyleBackColor = true;
            // 
            // btnScan
            // 
            btnScan.AutoSize = false;
            btnScan.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnScan.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnScan.Depth = 0;
            btnScan.HighEmphasis = true;
            btnScan.Icon = null;
            btnScan.Location = new System.Drawing.Point(19, 21);
            btnScan.Margin = new Padding(4, 6, 4, 6);
            btnScan.MouseState = MaterialSkin.MouseState.HOVER;
            btnScan.Name = "btnScan";
            btnScan.NoAccentTextColor = System.Drawing.Color.Empty;
            btnScan.Size = new System.Drawing.Size(106, 36);
            btnScan.TabIndex = 0;
            btnScan.Text = "Scan";
            btnScan.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnScan.UseAccentColor = false;
            btnScan.UseVisualStyleBackColor = true;
            btnScan.Click += btnScan_Click;
            // 
            // panel3
            // 
            panel3.BackColor = System.Drawing.SystemColors.ActiveCaption;
            panel3.Controls.Add(pbLoading);
            panel3.Controls.Add(panel2);
            panel3.Dock = DockStyle.Fill;
            panel3.Location = new System.Drawing.Point(0, 0);
            panel3.Margin = new Padding(15);
            panel3.Name = "panel3";
            panel3.Padding = new Padding(15);
            panel3.Size = new System.Drawing.Size(1098, 682);
            panel3.TabIndex = 2;
            // 
            // pbLoading
            // 
            pbLoading.BackColor = System.Drawing.SystemColors.Window;
            pbLoading.Image = (System.Drawing.Image)resources.GetObject("pbLoading.Image");
            pbLoading.InitialImage = null;
            pbLoading.Location = new System.Drawing.Point(-328, 205);
            pbLoading.Name = "pbLoading";
            pbLoading.Size = new System.Drawing.Size(338, 309);
            pbLoading.SizeMode = PictureBoxSizeMode.StretchImage;
            pbLoading.TabIndex = 2;
            pbLoading.TabStop = false;
            pbLoading.Visible = false;
            // 
            // panel2
            // 
            panel2.BorderStyle = BorderStyle.FixedSingle;
            panel2.Controls.Add(tabCatalog);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new System.Drawing.Point(15, 15);
            panel2.Name = "panel2";
            panel2.Size = new System.Drawing.Size(1068, 652);
            panel2.TabIndex = 2;
            // 
            // tabCatalog
            // 
            tabCatalog.Controls.Add(tabPage1);
            tabCatalog.Controls.Add(tabPage2);
            tabCatalog.Controls.Add(tabPage3);
            tabCatalog.Controls.Add(tabPage4);
            tabCatalog.Dock = DockStyle.Fill;
            tabCatalog.Location = new System.Drawing.Point(0, 0);
            tabCatalog.Name = "tabCatalog";
            tabCatalog.SelectedIndex = 0;
            tabCatalog.Size = new System.Drawing.Size(1066, 650);
            tabCatalog.TabIndex = 3;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(lvScanResults);
            tabPage1.Controls.Add(panel1);
            tabPage1.Location = new System.Drawing.Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new System.Drawing.Size(1058, 622);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Offline";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // lvScanResults
            // 
            lvScanResults.Dock = DockStyle.Fill;
            lvScanResults.FullRowSelect = true;
            lvScanResults.GridLines = true;
            lvScanResults.Location = new System.Drawing.Point(3, 80);
            lvScanResults.MultiSelect = false;
            lvScanResults.Name = "lvScanResults";
            lvScanResults.OwnerDraw = true;
            lvScanResults.Size = new System.Drawing.Size(1052, 539);
            lvScanResults.TabIndex = 2;
            lvScanResults.UseCompatibleStateImageBehavior = false;
            lvScanResults.View = View.Details;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(lvOrchestrationScanResult);
            tabPage2.Controls.Add(panel4);
            tabPage2.Location = new System.Drawing.Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new System.Drawing.Size(1058, 622);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Orchestration";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // lvOrchestrationScanResult
            // 
            lvOrchestrationScanResult.Dock = DockStyle.Fill;
            lvOrchestrationScanResult.FullRowSelect = true;
            lvOrchestrationScanResult.GridLines = true;
            lvOrchestrationScanResult.Location = new System.Drawing.Point(3, 80);
            lvOrchestrationScanResult.MultiSelect = false;
            lvOrchestrationScanResult.Name = "lvOrchestrationScanResult";
            lvOrchestrationScanResult.OwnerDraw = true;
            lvOrchestrationScanResult.Size = new System.Drawing.Size(1052, 539);
            lvOrchestrationScanResult.TabIndex = 4;
            lvOrchestrationScanResult.UseCompatibleStateImageBehavior = false;
            lvOrchestrationScanResult.View = View.Details;
            // 
            // panel4
            // 
            panel4.BorderStyle = BorderStyle.FixedSingle;
            panel4.Controls.Add(btnUpdateSDK);
            panel4.Controls.Add(btnInstallOrchestration);
            panel4.Controls.Add(btnScanOrchestration);
            panel4.Dock = DockStyle.Top;
            panel4.Location = new System.Drawing.Point(3, 3);
            panel4.Margin = new Padding(15);
            panel4.Name = "panel4";
            panel4.Padding = new Padding(15);
            panel4.Size = new System.Drawing.Size(1052, 77);
            panel4.TabIndex = 3;
            // 
            // btnUpdateSDK
            // 
            btnUpdateSDK.AutoSize = false;
            btnUpdateSDK.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnUpdateSDK.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnUpdateSDK.Depth = 0;
            btnUpdateSDK.HighEmphasis = true;
            btnUpdateSDK.Icon = null;
            btnUpdateSDK.Location = new System.Drawing.Point(925, 21);
            btnUpdateSDK.Margin = new Padding(4, 6, 4, 6);
            btnUpdateSDK.MouseState = MaterialSkin.MouseState.HOVER;
            btnUpdateSDK.Name = "btnUpdateSDK";
            btnUpdateSDK.NoAccentTextColor = System.Drawing.Color.Empty;
            btnUpdateSDK.Size = new System.Drawing.Size(106, 36);
            btnUpdateSDK.TabIndex = 4;
            btnUpdateSDK.Text = "Update SDK";
            btnUpdateSDK.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnUpdateSDK.UseAccentColor = false;
            btnUpdateSDK.UseVisualStyleBackColor = true;
            btnUpdateSDK.Click += btnUpdateSDK_Click;
            // 
            // btnInstallOrchestration
            // 
            btnInstallOrchestration.AutoSize = false;
            btnInstallOrchestration.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnInstallOrchestration.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnInstallOrchestration.Depth = 0;
            btnInstallOrchestration.HighEmphasis = true;
            btnInstallOrchestration.Icon = null;
            btnInstallOrchestration.Location = new System.Drawing.Point(811, 21);
            btnInstallOrchestration.Margin = new Padding(4, 6, 4, 6);
            btnInstallOrchestration.MouseState = MaterialSkin.MouseState.HOVER;
            btnInstallOrchestration.Name = "btnInstallOrchestration";
            btnInstallOrchestration.NoAccentTextColor = System.Drawing.Color.Empty;
            btnInstallOrchestration.Size = new System.Drawing.Size(106, 36);
            btnInstallOrchestration.TabIndex = 3;
            btnInstallOrchestration.Text = "Install";
            btnInstallOrchestration.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnInstallOrchestration.UseAccentColor = false;
            btnInstallOrchestration.UseVisualStyleBackColor = true;
            btnInstallOrchestration.Click += btnInstallOrchestration_Click;
            // 
            // btnScanOrchestration
            // 
            btnScanOrchestration.AutoSize = false;
            btnScanOrchestration.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnScanOrchestration.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnScanOrchestration.Depth = 0;
            btnScanOrchestration.HighEmphasis = true;
            btnScanOrchestration.Icon = null;
            btnScanOrchestration.Location = new System.Drawing.Point(19, 21);
            btnScanOrchestration.Margin = new Padding(4, 6, 4, 6);
            btnScanOrchestration.MouseState = MaterialSkin.MouseState.HOVER;
            btnScanOrchestration.Name = "btnScanOrchestration";
            btnScanOrchestration.NoAccentTextColor = System.Drawing.Color.Empty;
            btnScanOrchestration.Size = new System.Drawing.Size(106, 36);
            btnScanOrchestration.TabIndex = 0;
            btnScanOrchestration.Text = "Scan";
            btnScanOrchestration.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnScanOrchestration.UseAccentColor = false;
            btnScanOrchestration.UseVisualStyleBackColor = true;
            btnScanOrchestration.Click += btnScanOrchestration_Click;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(lvCatalog);
            tabPage3.Controls.Add(panel5);
            tabPage3.Location = new System.Drawing.Point(4, 24);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new Padding(3);
            tabPage3.Size = new System.Drawing.Size(1058, 622);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Catalog";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // lvCatalog
            // 
            lvCatalog.Dock = DockStyle.Fill;
            lvCatalog.FullRowSelect = true;
            lvCatalog.GridLines = true;
            lvCatalog.Location = new System.Drawing.Point(3, 80);
            lvCatalog.MultiSelect = false;
            lvCatalog.Name = "lvCatalog";
            lvCatalog.OwnerDraw = true;
            lvCatalog.Size = new System.Drawing.Size(1052, 539);
            lvCatalog.TabIndex = 6;
            lvCatalog.UseCompatibleStateImageBehavior = false;
            lvCatalog.View = View.Details;
            // 
            // panel5
            // 
            panel5.BorderStyle = BorderStyle.FixedSingle;
            panel5.Controls.Add(btnDomainCSV);
            panel5.Controls.Add(btnFreshInstall);
            panel5.Controls.Add(btnExportCSV);
            panel5.Controls.Add(lblTotalInstalls);
            panel5.Controls.Add(label3);
            panel5.Controls.Add(lblTotalCVEs);
            panel5.Controls.Add(lblTotalProducts);
            panel5.Controls.Add(btnLookupCVE);
            panel5.Controls.Add(label2);
            panel5.Controls.Add(label1);
            panel5.Controls.Add(btnListCatalogCVE);
            panel5.Controls.Add(mbLoad);
            panel5.Dock = DockStyle.Top;
            panel5.Location = new System.Drawing.Point(3, 3);
            panel5.Margin = new Padding(15);
            panel5.Name = "panel5";
            panel5.Padding = new Padding(15);
            panel5.Size = new System.Drawing.Size(1052, 77);
            panel5.TabIndex = 5;
            // 
            // btnDomainCSV
            // 
            btnDomainCSV.AutoSize = false;
            btnDomainCSV.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnDomainCSV.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnDomainCSV.Depth = 0;
            btnDomainCSV.HighEmphasis = true;
            btnDomainCSV.Icon = null;
            btnDomainCSV.Location = new System.Drawing.Point(458, 21);
            btnDomainCSV.Margin = new Padding(4, 6, 4, 6);
            btnDomainCSV.MouseState = MaterialSkin.MouseState.HOVER;
            btnDomainCSV.Name = "btnDomainCSV";
            btnDomainCSV.NoAccentTextColor = System.Drawing.Color.Empty;
            btnDomainCSV.Size = new System.Drawing.Size(110, 36);
            btnDomainCSV.TabIndex = 13;
            btnDomainCSV.Text = "Url CSV";
            btnDomainCSV.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnDomainCSV.UseAccentColor = false;
            btnDomainCSV.UseMnemonic = false;
            btnDomainCSV.UseVisualStyleBackColor = true;
            btnDomainCSV.Click += btnUrlCSV_Click;
            // 
            // btnFreshInstall
            // 
            btnFreshInstall.AutoSize = false;
            btnFreshInstall.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnFreshInstall.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnFreshInstall.Depth = 0;
            btnFreshInstall.HighEmphasis = true;
            btnFreshInstall.Icon = null;
            btnFreshInstall.Location = new System.Drawing.Point(694, 21);
            btnFreshInstall.Margin = new Padding(4, 6, 4, 6);
            btnFreshInstall.MouseState = MaterialSkin.MouseState.HOVER;
            btnFreshInstall.Name = "btnFreshInstall";
            btnFreshInstall.NoAccentTextColor = System.Drawing.Color.Empty;
            btnFreshInstall.Size = new System.Drawing.Size(110, 36);
            btnFreshInstall.TabIndex = 12;
            btnFreshInstall.Text = "Fresh Install";
            btnFreshInstall.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnFreshInstall.UseAccentColor = false;
            btnFreshInstall.UseVisualStyleBackColor = true;
            btnFreshInstall.Click += btnFreshInstall_Click;
            // 
            // btnExportCSV
            // 
            btnExportCSV.AutoSize = false;
            btnExportCSV.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnExportCSV.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnExportCSV.Depth = 0;
            btnExportCSV.HighEmphasis = true;
            btnExportCSV.Icon = null;
            btnExportCSV.Location = new System.Drawing.Point(930, 21);
            btnExportCSV.Margin = new Padding(4, 6, 4, 6);
            btnExportCSV.MouseState = MaterialSkin.MouseState.HOVER;
            btnExportCSV.Name = "btnExportCSV";
            btnExportCSV.NoAccentTextColor = System.Drawing.Color.Empty;
            btnExportCSV.Size = new System.Drawing.Size(110, 36);
            btnExportCSV.TabIndex = 11;
            btnExportCSV.Text = "Export CSV";
            btnExportCSV.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnExportCSV.UseAccentColor = false;
            btnExportCSV.UseVisualStyleBackColor = true;
            btnExportCSV.Click += btnExportCSV_Click;
            // 
            // lblTotalInstalls
            // 
            lblTotalInstalls.AutoSize = true;
            lblTotalInstalls.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblTotalInstalls.Location = new System.Drawing.Point(311, 47);
            lblTotalInstalls.Name = "lblTotalInstalls";
            lblTotalInstalls.Size = new System.Drawing.Size(19, 21);
            lblTotalInstalls.TabIndex = 9;
            lblTotalInstalls.Text = "0";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            label3.Location = new System.Drawing.Point(182, 47);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(114, 21);
            label3.TabIndex = 8;
            label3.Text = "Total Install's:";
            // 
            // lblTotalCVEs
            // 
            lblTotalCVEs.AutoSize = true;
            lblTotalCVEs.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblTotalCVEs.Location = new System.Drawing.Point(311, 27);
            lblTotalCVEs.Name = "lblTotalCVEs";
            lblTotalCVEs.Size = new System.Drawing.Size(19, 21);
            lblTotalCVEs.TabIndex = 7;
            lblTotalCVEs.Text = "0";
            // 
            // lblTotalProducts
            // 
            lblTotalProducts.AutoSize = true;
            lblTotalProducts.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblTotalProducts.Location = new System.Drawing.Point(311, 6);
            lblTotalProducts.Name = "lblTotalProducts";
            lblTotalProducts.Size = new System.Drawing.Size(19, 21);
            lblTotalProducts.TabIndex = 6;
            lblTotalProducts.Text = "0";
            // 
            // btnLookupCVE
            // 
            btnLookupCVE.AutoSize = false;
            btnLookupCVE.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnLookupCVE.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnLookupCVE.Depth = 0;
            btnLookupCVE.HighEmphasis = true;
            btnLookupCVE.Icon = null;
            btnLookupCVE.Location = new System.Drawing.Point(576, 21);
            btnLookupCVE.Margin = new Padding(4, 6, 4, 6);
            btnLookupCVE.MouseState = MaterialSkin.MouseState.HOVER;
            btnLookupCVE.Name = "btnLookupCVE";
            btnLookupCVE.NoAccentTextColor = System.Drawing.Color.Empty;
            btnLookupCVE.Size = new System.Drawing.Size(110, 36);
            btnLookupCVE.TabIndex = 4;
            btnLookupCVE.Text = "Lookup CVE";
            btnLookupCVE.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnLookupCVE.UseAccentColor = false;
            btnLookupCVE.UseVisualStyleBackColor = true;
            btnLookupCVE.Click += btnLookupCVE_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            label2.Location = new System.Drawing.Point(182, 27);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(97, 21);
            label2.TabIndex = 3;
            label2.Text = "Total CVE's:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            label1.Location = new System.Drawing.Point(182, 6);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(123, 21);
            label1.TabIndex = 2;
            label1.Text = "Total Products:";
            // 
            // btnListCatalogCVE
            // 
            btnListCatalogCVE.AutoSize = false;
            btnListCatalogCVE.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnListCatalogCVE.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnListCatalogCVE.Depth = 0;
            btnListCatalogCVE.HighEmphasis = true;
            btnListCatalogCVE.Icon = null;
            btnListCatalogCVE.Location = new System.Drawing.Point(812, 21);
            btnListCatalogCVE.Margin = new Padding(4, 6, 4, 6);
            btnListCatalogCVE.MouseState = MaterialSkin.MouseState.HOVER;
            btnListCatalogCVE.Name = "btnListCatalogCVE";
            btnListCatalogCVE.NoAccentTextColor = System.Drawing.Color.Empty;
            btnListCatalogCVE.Size = new System.Drawing.Size(110, 36);
            btnListCatalogCVE.TabIndex = 1;
            btnListCatalogCVE.Text = "Product CVES";
            btnListCatalogCVE.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnListCatalogCVE.UseAccentColor = false;
            btnListCatalogCVE.UseVisualStyleBackColor = true;
            btnListCatalogCVE.Click += btnListCatalogCVE_Click;
            // 
            // mbLoad
            // 
            mbLoad.AutoSize = false;
            mbLoad.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            mbLoad.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            mbLoad.Depth = 0;
            mbLoad.HighEmphasis = true;
            mbLoad.Icon = null;
            mbLoad.Location = new System.Drawing.Point(19, 21);
            mbLoad.Margin = new Padding(4, 6, 4, 6);
            mbLoad.MouseState = MaterialSkin.MouseState.HOVER;
            mbLoad.Name = "mbLoad";
            mbLoad.NoAccentTextColor = System.Drawing.Color.Empty;
            mbLoad.Size = new System.Drawing.Size(124, 36);
            mbLoad.TabIndex = 0;
            mbLoad.Text = "Load Latest";
            mbLoad.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            mbLoad.UseAccentColor = false;
            mbLoad.UseVisualStyleBackColor = true;
            mbLoad.Click += mbLoad_Click;
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(lvStatus);
            tabPage4.Controls.Add(panel6);
            tabPage4.Location = new System.Drawing.Point(4, 24);
            tabPage4.Name = "tabPage4";
            tabPage4.Padding = new Padding(3);
            tabPage4.Size = new System.Drawing.Size(1058, 622);
            tabPage4.TabIndex = 3;
            tabPage4.Text = "Status";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // lvStatus
            // 
            lvStatus.Dock = DockStyle.Fill;
            lvStatus.FullRowSelect = true;
            lvStatus.GridLines = true;
            lvStatus.Location = new System.Drawing.Point(3, 80);
            lvStatus.MultiSelect = false;
            lvStatus.Name = "lvStatus";
            lvStatus.OwnerDraw = true;
            lvStatus.Size = new System.Drawing.Size(1052, 539);
            lvStatus.TabIndex = 6;
            lvStatus.UseCompatibleStateImageBehavior = false;
            lvStatus.View = View.Details;
            // 
            // panel6
            // 
            panel6.BorderStyle = BorderStyle.FixedSingle;
            panel6.Controls.Add(btnRefreshStatus);
            panel6.Dock = DockStyle.Top;
            panel6.Location = new System.Drawing.Point(3, 3);
            panel6.Margin = new Padding(15);
            panel6.Name = "panel6";
            panel6.Padding = new Padding(15);
            panel6.Size = new System.Drawing.Size(1052, 77);
            panel6.TabIndex = 5;
            // 
            // btnRefreshStatus
            // 
            btnRefreshStatus.AutoSize = false;
            btnRefreshStatus.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnRefreshStatus.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnRefreshStatus.Depth = 0;
            btnRefreshStatus.HighEmphasis = true;
            btnRefreshStatus.Icon = null;
            btnRefreshStatus.Location = new System.Drawing.Point(19, 21);
            btnRefreshStatus.Margin = new Padding(4, 6, 4, 6);
            btnRefreshStatus.MouseState = MaterialSkin.MouseState.HOVER;
            btnRefreshStatus.Name = "btnRefreshStatus";
            btnRefreshStatus.NoAccentTextColor = System.Drawing.Color.Empty;
            btnRefreshStatus.Size = new System.Drawing.Size(106, 36);
            btnRefreshStatus.TabIndex = 0;
            btnRefreshStatus.Text = "Refresh";
            btnRefreshStatus.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnRefreshStatus.UseAccentColor = false;
            btnRefreshStatus.UseVisualStyleBackColor = true;
            btnRefreshStatus.Click += btnRefreshStatus_Click;
            // 
            // ScannerForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1098, 682);
            Controls.Add(panel3);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "ScannerForm";
            Text = "Acme Scanner";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pbLoading).EndInit();
            panel2.ResumeLayout(false);
            tabCatalog.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            panel4.ResumeLayout(false);
            tabPage3.ResumeLayout(false);
            panel5.ResumeLayout(false);
            panel5.PerformLayout();
            tabPage4.ResumeLayout(false);
            panel6.ResumeLayout(false);
            ResumeLayout(false);

            tbCVE = new MaterialSkin.Controls.MaterialTextBox2();
            Controls.Add(this.tbCVE);
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
        private MaterialSkin.Controls.MaterialButton btnFreshInstall;
        private MaterialSkin.Controls.MaterialButton btnDomainCSV;
        private TabPage tabPage4;
        private ScannerListView lvStatus;
        private Panel panel6;
        private MaterialSkin.Controls.MaterialButton btnRefreshStatus;
    }
}