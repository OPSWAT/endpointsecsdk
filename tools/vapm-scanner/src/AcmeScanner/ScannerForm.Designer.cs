using MaterialSkin.Controls;
using System.Drawing;
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
            panel3 = new Panel();
            pbLoading = new PictureBox();
            panel2 = new Panel();
            timer1 = new Timer(components);
            tabCatalog = new TabPage();
            panel5 = new Panel();
            mbLoad = new MaterialButton();
            label1 = new Label();
            label2 = new Label();
            btnLookupCVE = new MaterialButton();
            lblTotalProducts = new Label();
            lblTotalCVEs = new Label();
            label3 = new Label();
            lblTotalInstalls = new Label();
            btnExportCSV = new MaterialButton();
            btnFreshInstall = new MaterialButton();
            btnListCatalogCVE = new MaterialButton();
            btnDomainCSV = new MaterialButton();
            panel9 = new Panel();
            searchCatalog = new TextBox();
            pictureBox1 = new PictureBox();
            lvCatalog = new ScannerListView();
            tabOrchestrate = new TabPage();
            panel4 = new Panel();
            btnScanOrchestration = new MaterialButton();
            btnInstallOrchestration = new MaterialButton();
            label10 = new Label();
            label11 = new Label();
            label12 = new Label();
            label13 = new Label();
            label14 = new Label();
            label15 = new Label();
            lvOrchestrationScanResult = new ScannerListView();
            tabOffline = new TabPage();
            panel1 = new Panel();
            btnScan = new MaterialButton();
            cbScanOSCVEs = new CheckBox();
            btnCVEJSON = new MaterialButton();
            btnInstall = new MaterialButton();
            btnUpdate = new MaterialButton();
            label4 = new Label();
            label5 = new Label();
            label6 = new Label();
            label7 = new Label();
            label8 = new Label();
            label9 = new Label();
            btnUpdateSDK = new MaterialButton();
            lvScanResults = new ScannerListView();
            tabBiosDrivers = new TabPage();
            panelBiosDrivers = new Panel();
            btnScanBiosDrivers = new Button();
            lblBiosDriversSummary = new Label();
            lvBiosDrivers = new ScannerListView();
            tbcMainView = new TabControl();
            panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pbLoading).BeginInit();
            panel2.SuspendLayout();
            tabCatalog.SuspendLayout();
            panel5.SuspendLayout();
            panel9.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            tabOrchestrate.SuspendLayout();
            panel4.SuspendLayout();
            tabOffline.SuspendLayout();
            panel1.SuspendLayout();
            tabBiosDrivers.SuspendLayout();
            panelBiosDrivers.SuspendLayout();
            tbcMainView.SuspendLayout();
            SuspendLayout();
            // 
            // panel3
            // 
            panel3.BackColor = SystemColors.ActiveCaption;
            panel3.Controls.Add(pbLoading);
            panel3.Controls.Add(panel2);
            panel3.Dock = DockStyle.Fill;
            panel3.Location = new Point(0, 0);
            panel3.Margin = new Padding(15);
            panel3.Name = "panel3";
            panel3.Padding = new Padding(15);
            panel3.Size = new Size(1098, 630);
            panel3.TabIndex = 2;
            // 
            // pbLoading
            // 
            pbLoading.BackColor = SystemColors.Window;
            pbLoading.Image = (Image)resources.GetObject("pbLoading.Image");
            pbLoading.InitialImage = null;
            pbLoading.Location = new Point(-328, 205);
            pbLoading.Name = "pbLoading";
            pbLoading.Size = new Size(338, 309);
            pbLoading.SizeMode = PictureBoxSizeMode.StretchImage;
            pbLoading.TabIndex = 2;
            pbLoading.TabStop = false;
            pbLoading.Visible = false;
            // 
            // panel2
            // 
            panel2.BorderStyle = BorderStyle.FixedSingle;
            panel2.Controls.Add(tbcMainView);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(15, 15);
            panel2.Name = "panel2";
            panel2.Size = new Size(1068, 600);
            panel2.TabIndex = 2;
            //
            // tabCatalog
            // 
            tabCatalog.Controls.Add(lvCatalog);
            tabCatalog.Controls.Add(panel5);
            tabCatalog.Location = new Point(4, 24);
            tabCatalog.Name = "tabCatalog";
            tabCatalog.Padding = new Padding(3);
            tabCatalog.Size = new Size(1058, 570);
            tabCatalog.TabIndex = 2;
            tabCatalog.Text = "Catalog";
            tabCatalog.UseVisualStyleBackColor = true;
            // 
            // panel5
            // 
            panel5.BorderStyle = BorderStyle.FixedSingle;
            panel5.Controls.Add(panel9);
            panel5.Controls.Add(btnDomainCSV);
            panel5.Controls.Add(btnListCatalogCVE);
            panel5.Controls.Add(btnFreshInstall);
            panel5.Controls.Add(btnExportCSV);
            panel5.Controls.Add(lblTotalInstalls);
            panel5.Controls.Add(label3);
            panel5.Controls.Add(lblTotalCVEs);
            panel5.Controls.Add(lblTotalProducts);
            panel5.Controls.Add(btnLookupCVE);
            panel5.Controls.Add(label2);
            panel5.Controls.Add(label1);
            panel5.Controls.Add(mbLoad);
            panel5.Dock = DockStyle.Top;
            panel5.Location = new Point(3, 3);
            panel5.Margin = new Padding(15);
            panel5.Name = "panel5";
            panel5.Padding = new Padding(15);
            panel5.Size = new Size(1052, 77);
            panel5.TabIndex = 5;
            // 
            // mbLoad
            // 
            mbLoad.AutoSize = false;
            mbLoad.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            mbLoad.Density = MaterialButton.MaterialButtonDensity.Default;
            mbLoad.Depth = 0;
            mbLoad.HighEmphasis = true;
            mbLoad.Icon = null;
            mbLoad.Location = new Point(19, 21);
            mbLoad.Margin = new Padding(4, 6, 4, 6);
            mbLoad.MouseState = MaterialSkin.MouseState.HOVER;
            mbLoad.Name = "mbLoad";
            mbLoad.NoAccentTextColor = Color.Empty;
            mbLoad.Size = new Size(124, 36);
            mbLoad.TabIndex = 0;
            mbLoad.Text = "Load Product Information";
            mbLoad.Type = MaterialButton.MaterialButtonType.Contained;
            mbLoad.UseAccentColor = false;
            mbLoad.UseVisualStyleBackColor = true;
            mbLoad.Click += MbLoad_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            label1.Location = new Point(182, 6);
            label1.Name = "label1";
            label1.Size = new Size(123, 21);
            label1.TabIndex = 2;
            label1.Text = "Total Products:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            label2.Location = new Point(182, 27);
            label2.Name = "label2";
            label2.Size = new Size(97, 21);
            label2.TabIndex = 3;
            label2.Text = "Total CVE's:";
            // 
            // btnLookupCVE
            // 
            btnLookupCVE.AutoSize = false;
            btnLookupCVE.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnLookupCVE.Density = MaterialButton.MaterialButtonDensity.Default;
            btnLookupCVE.Depth = 0;
            btnLookupCVE.HighEmphasis = true;
            btnLookupCVE.Icon = null;
            btnLookupCVE.Location = new Point(628, 19);
            btnLookupCVE.Margin = new Padding(3, 4, 3, 4);
            btnLookupCVE.MouseState = MaterialSkin.MouseState.HOVER;
            btnLookupCVE.Name = "btnLookupCVE";
            btnLookupCVE.NoAccentTextColor = Color.Empty;
            btnLookupCVE.Size = new Size(110, 36);
            btnLookupCVE.TabIndex = 4;
            btnLookupCVE.Text = "Lookup CVE";
            btnLookupCVE.Type = MaterialButton.MaterialButtonType.Contained;
            btnLookupCVE.UseAccentColor = false;
            btnLookupCVE.UseVisualStyleBackColor = true;
            btnLookupCVE.Click += BtnLookupCVE_Click;
            // 
            // lblTotalProducts
            // 
            lblTotalProducts.AutoSize = true;
            lblTotalProducts.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTotalProducts.Location = new Point(311, 6);
            lblTotalProducts.Name = "lblTotalProducts";
            lblTotalProducts.Size = new Size(19, 21);
            lblTotalProducts.TabIndex = 6;
            lblTotalProducts.Text = "0";
            // 
            // lblTotalCVEs
            // 
            lblTotalCVEs.AutoSize = true;
            lblTotalCVEs.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTotalCVEs.Location = new Point(311, 27);
            lblTotalCVEs.Name = "lblTotalCVEs";
            lblTotalCVEs.Size = new Size(19, 21);
            lblTotalCVEs.TabIndex = 7;
            lblTotalCVEs.Text = "0";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            label3.Location = new Point(182, 47);
            label3.Name = "label3";
            label3.Size = new Size(114, 21);
            label3.TabIndex = 8;
            label3.Text = "Total Install's:";
            // 
            // lblTotalInstalls
            // 
            lblTotalInstalls.AutoSize = true;
            lblTotalInstalls.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTotalInstalls.Location = new Point(311, 47);
            lblTotalInstalls.Name = "lblTotalInstalls";
            lblTotalInstalls.Size = new Size(19, 21);
            lblTotalInstalls.TabIndex = 9;
            lblTotalInstalls.Text = "0";
            // 
            // btnExportCSV
            // 
            btnExportCSV.AutoSize = false;
            btnExportCSV.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnExportCSV.Density = MaterialButton.MaterialButtonDensity.Default;
            btnExportCSV.Depth = 0;
            btnExportCSV.HighEmphasis = true;
            btnExportCSV.Icon = null;
            btnExportCSV.Location = new Point(773, 37);
            btnExportCSV.Margin = new Padding(3, 4, 3, 4);
            btnExportCSV.MouseState = MaterialSkin.MouseState.HOVER;
            btnExportCSV.Name = "btnExportCSV";
            btnExportCSV.NoAccentTextColor = Color.Empty;
            btnExportCSV.Size = new Size(110, 32);
            btnExportCSV.TabIndex = 11;
            btnExportCSV.Text = "Export CSV";
            btnExportCSV.Type = MaterialButton.MaterialButtonType.Contained;
            btnExportCSV.UseAccentColor = false;
            btnExportCSV.UseVisualStyleBackColor = true;
            btnExportCSV.Click += BtnExportCSV_Click;
            // 
            // btnFreshInstall
            // 
            btnFreshInstall.AutoSize = false;
            btnFreshInstall.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnFreshInstall.Density = MaterialButton.MaterialButtonDensity.Default;
            btnFreshInstall.Depth = 0;
            btnFreshInstall.HighEmphasis = true;
            btnFreshInstall.Icon = null;
            btnFreshInstall.Location = new Point(922, 4);
            btnFreshInstall.Margin = new Padding(3, 4, 3, 4);
            btnFreshInstall.MouseState = MaterialSkin.MouseState.HOVER;
            btnFreshInstall.Name = "btnFreshInstall";
            btnFreshInstall.NoAccentTextColor = Color.Empty;
            btnFreshInstall.Size = new Size(110, 32);
            btnFreshInstall.TabIndex = 12;
            btnFreshInstall.Text = "Fresh Install";
            btnFreshInstall.Type = MaterialButton.MaterialButtonType.Contained;
            btnFreshInstall.UseAccentColor = false;
            btnFreshInstall.UseVisualStyleBackColor = true;
            btnFreshInstall.Click += BtnFreshInstall_Click;
            // 
            // btnListCatalogCVE
            // 
            btnListCatalogCVE.AutoSize = false;
            btnListCatalogCVE.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnListCatalogCVE.Density = MaterialButton.MaterialButtonDensity.Default;
            btnListCatalogCVE.Depth = 0;
            btnListCatalogCVE.HighEmphasis = true;
            btnListCatalogCVE.Icon = null;
            btnListCatalogCVE.Location = new Point(922, 37);
            btnListCatalogCVE.Margin = new Padding(3, 4, 3, 4);
            btnListCatalogCVE.MouseState = MaterialSkin.MouseState.HOVER;
            btnListCatalogCVE.Name = "btnListCatalogCVE";
            btnListCatalogCVE.NoAccentTextColor = Color.Empty;
            btnListCatalogCVE.Size = new Size(110, 32);
            btnListCatalogCVE.TabIndex = 1;
            btnListCatalogCVE.Text = "View CVES";
            btnListCatalogCVE.Type = MaterialButton.MaterialButtonType.Contained;
            btnListCatalogCVE.UseAccentColor = false;
            btnListCatalogCVE.UseVisualStyleBackColor = true;
            btnListCatalogCVE.Click += BtnListCatalogCVE_Click;
            // 
            // btnDomainCSV
            // 
            btnDomainCSV.AutoSize = false;
            btnDomainCSV.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnDomainCSV.Density = MaterialButton.MaterialButtonDensity.Default;
            btnDomainCSV.Depth = 0;
            btnDomainCSV.HighEmphasis = true;
            btnDomainCSV.Icon = null;
            btnDomainCSV.Location = new Point(773, 4);
            btnDomainCSV.Margin = new Padding(3, 4, 3, 4);
            btnDomainCSV.MouseState = MaterialSkin.MouseState.HOVER;
            btnDomainCSV.Name = "btnDomainCSV";
            btnDomainCSV.NoAccentTextColor = Color.Empty;
            btnDomainCSV.Size = new Size(110, 32);
            btnDomainCSV.TabIndex = 13;
            btnDomainCSV.Text = "Url CSV";
            btnDomainCSV.Type = MaterialButton.MaterialButtonType.Contained;
            btnDomainCSV.UseAccentColor = false;
            btnDomainCSV.UseMnemonic = false;
            btnDomainCSV.UseVisualStyleBackColor = true;
            btnDomainCSV.Click += BtnUrlCSV_Click;
            // 
            // panel9
            // 
            panel9.Controls.Add(pictureBox1);
            panel9.Controls.Add(searchCatalog);
            panel9.Location = new Point(436, 21);
            panel9.Name = "panel9";
            panel9.Size = new Size(154, 35);
            panel9.TabIndex = 15;
            // 
            // searchCatalog
            // 
            searchCatalog.BorderStyle = BorderStyle.None;
            searchCatalog.Font = new Font("Segoe UI", 12F);
            searchCatalog.Location = new Point(25, 7);
            searchCatalog.Margin = new Padding(0);
            searchCatalog.Name = "searchCatalog";
            searchCatalog.Size = new Size(118, 22);
            searchCatalog.TabIndex = 14;
            searchCatalog.Text = "Search Products";
            searchCatalog.Click += searchCatalogClicked;
            searchCatalog.TextChanged += searchCatalog_TextChanged;
            searchCatalog.KeyDown += searchCatalogEnter;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(0, 2);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(18, 33);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 15;
            pictureBox1.TabStop = false;
            // 
            // lvCatalog
            // 
            lvCatalog.Dock = DockStyle.Fill;
            lvCatalog.FullRowSelect = true;
            lvCatalog.GridLines = true;
            lvCatalog.Location = new Point(3, 80);
            lvCatalog.MultiSelect = false;
            lvCatalog.Name = "lvCatalog";
            lvCatalog.OwnerDraw = true;
            lvCatalog.Size = new Size(1052, 487);
            lvCatalog.TabIndex = 6;
            lvCatalog.UseCompatibleStateImageBehavior = false;
            lvCatalog.View = View.Details;
            // 
            // tabOrchestrate
            // 
            tabOrchestrate.Controls.Add(lvOrchestrationScanResult);
            tabOrchestrate.Controls.Add(panel4);
            tabOrchestrate.Location = new Point(4, 24);
            tabOrchestrate.Name = "tabOrchestrate";
            tabOrchestrate.Padding = new Padding(3);
            tabOrchestrate.Size = new Size(1058, 570);
            tabOrchestrate.TabIndex = 1;
            tabOrchestrate.Text = "WU Orchestration";
            tabOrchestrate.UseVisualStyleBackColor = true;
            // 
            // panel4
            // 
            panel4.BorderStyle = BorderStyle.FixedSingle;
            panel4.Controls.Add(label15);
            panel4.Controls.Add(label14);
            panel4.Controls.Add(label13);
            panel4.Controls.Add(label12);
            panel4.Controls.Add(label11);
            panel4.Controls.Add(label10);
            panel4.Controls.Add(btnInstallOrchestration);
            panel4.Controls.Add(btnScanOrchestration);
            panel4.Dock = DockStyle.Top;
            panel4.Location = new Point(3, 3);
            panel4.Margin = new Padding(15);
            panel4.Name = "panel4";
            panel4.Padding = new Padding(15);
            panel4.Size = new Size(1052, 77);
            panel4.TabIndex = 3;
            // 
            // btnScanOrchestration
            // 
            btnScanOrchestration.AutoSize = false;
            btnScanOrchestration.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnScanOrchestration.Density = MaterialButton.MaterialButtonDensity.Default;
            btnScanOrchestration.Depth = 0;
            btnScanOrchestration.HighEmphasis = true;
            btnScanOrchestration.Icon = null;
            btnScanOrchestration.Location = new Point(19, 21);
            btnScanOrchestration.Margin = new Padding(4, 6, 4, 6);
            btnScanOrchestration.MouseState = MaterialSkin.MouseState.HOVER;
            btnScanOrchestration.Name = "btnScanOrchestration";
            btnScanOrchestration.NoAccentTextColor = Color.Empty;
            btnScanOrchestration.Size = new Size(124, 36);
            btnScanOrchestration.TabIndex = 0;
            btnScanOrchestration.Text = "Scan Windows Update";
            btnScanOrchestration.Type = MaterialButton.MaterialButtonType.Contained;
            btnScanOrchestration.UseAccentColor = false;
            btnScanOrchestration.UseVisualStyleBackColor = true;
            btnScanOrchestration.Click += BtnScanOrchestration_Click;
            // 
            // btnInstallOrchestration
            // 
            btnInstallOrchestration.AutoSize = false;
            btnInstallOrchestration.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnInstallOrchestration.Density = MaterialButton.MaterialButtonDensity.Default;
            btnInstallOrchestration.Depth = 0;
            btnInstallOrchestration.HighEmphasis = true;
            btnInstallOrchestration.Icon = null;
            btnInstallOrchestration.Location = new Point(151, 21);
            btnInstallOrchestration.Margin = new Padding(4, 6, 4, 6);
            btnInstallOrchestration.MouseState = MaterialSkin.MouseState.HOVER;
            btnInstallOrchestration.Name = "btnInstallOrchestration";
            btnInstallOrchestration.NoAccentTextColor = Color.Empty;
            btnInstallOrchestration.Size = new Size(117, 36);
            btnInstallOrchestration.TabIndex = 3;
            btnInstallOrchestration.Text = "Install WU Patch";
            btnInstallOrchestration.Type = MaterialButton.MaterialButtonType.Contained;
            btnInstallOrchestration.UseAccentColor = false;
            btnInstallOrchestration.UseVisualStyleBackColor = true;
            btnInstallOrchestration.Click += BtnInstallOrchestration_Click;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(465, 15);
            label10.Margin = new Padding(2, 0, 2, 0);
            label10.Name = "label10";
            label10.Size = new Size(72, 15);
            label10.TabIndex = 5;
            label10.Text = "SDK Version:";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(465, 30);
            label11.Margin = new Padding(2, 0, 2, 0);
            label11.Name = "label11";
            label11.Size = new Size(58, 15);
            label11.TabIndex = 6;
            label11.Text = "SDK Date:";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(465, 45);
            label12.Margin = new Padding(2, 0, 2, 0);
            label12.Name = "label12";
            label12.Size = new Size(89, 15);
            label12.TabIndex = 7;
            label12.Text = "Analog Update:";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(541, 15);
            label13.Margin = new Padding(2, 0, 2, 0);
            label13.Name = "label13";
            label13.Size = new Size(0, 15);
            label13.TabIndex = 8;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new Point(527, 30);
            label14.Margin = new Padding(2, 0, 2, 0);
            label14.Name = "label14";
            label14.Size = new Size(0, 15);
            label14.TabIndex = 9;
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Location = new Point(558, 45);
            label15.Margin = new Padding(2, 0, 2, 0);
            label15.Name = "label15";
            label15.Size = new Size(0, 15);
            label15.TabIndex = 10;
            // 
            // lvOrchestrationScanResult
            // 
            lvOrchestrationScanResult.Dock = DockStyle.Fill;
            lvOrchestrationScanResult.FullRowSelect = true;
            lvOrchestrationScanResult.GridLines = true;
            lvOrchestrationScanResult.Location = new Point(3, 80);
            lvOrchestrationScanResult.MultiSelect = false;
            lvOrchestrationScanResult.Name = "lvOrchestrationScanResult";
            lvOrchestrationScanResult.OwnerDraw = true;
            lvOrchestrationScanResult.Size = new Size(1052, 487);
            lvOrchestrationScanResult.TabIndex = 4;
            lvOrchestrationScanResult.UseCompatibleStateImageBehavior = false;
            lvOrchestrationScanResult.View = View.Details;
            // 
            // tabOffline
            // 
            tabOffline.Controls.Add(lvScanResults);
            tabOffline.Controls.Add(panel1);
            tabOffline.Location = new Point(4, 24);
            tabOffline.Name = "tabOffline";
            tabOffline.Padding = new Padding(3);
            tabOffline.Size = new Size(1058, 570);
            tabOffline.TabIndex = 0;
            tabOffline.Text = "Detection";
            tabOffline.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Controls.Add(btnUpdateSDK);
            panel1.Controls.Add(label9);
            panel1.Controls.Add(label8);
            panel1.Controls.Add(label7);
            panel1.Controls.Add(label6);
            panel1.Controls.Add(label5);
            panel1.Controls.Add(label4);
            panel1.Controls.Add(btnUpdate);
            panel1.Controls.Add(btnInstall);
            panel1.Controls.Add(btnCVEJSON);
            panel1.Controls.Add(cbScanOSCVEs);
            panel1.Controls.Add(btnScan);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(3, 3);
            panel1.Margin = new Padding(15);
            panel1.Name = "panel1";
            panel1.Padding = new Padding(15);
            panel1.Size = new Size(1052, 77);
            panel1.TabIndex = 1;
            // 
            // btnScan
            // 
            btnScan.AutoSize = false;
            btnScan.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnScan.Density = MaterialButton.MaterialButtonDensity.Default;
            btnScan.Depth = 0;
            btnScan.HighEmphasis = true;
            btnScan.Icon = null;
            btnScan.Location = new Point(19, 20);
            btnScan.Margin = new Padding(4, 6, 4, 6);
            btnScan.MouseState = MaterialSkin.MouseState.HOVER;
            btnScan.Name = "btnScan";
            btnScan.NoAccentTextColor = Color.Empty;
            btnScan.Size = new Size(105, 36);
            btnScan.TabIndex = 0;
            btnScan.Text = "Scan";
            btnScan.Type = MaterialButton.MaterialButtonType.Contained;
            btnScan.UseAccentColor = false;
            btnScan.UseVisualStyleBackColor = true;
            btnScan.Click += BtnScan_Click;
            // 
            // cbScanOSCVEs
            // 
            cbScanOSCVEs.AutoSize = true;
            cbScanOSCVEs.Font = new Font("Segoe UI", 14.25F);
            cbScanOSCVEs.Location = new Point(149, 23);
            cbScanOSCVEs.Name = "cbScanOSCVEs";
            cbScanOSCVEs.Size = new Size(147, 29);
            cbScanOSCVEs.TabIndex = 1;
            cbScanOSCVEs.Text = "Scan OS CVEs";
            cbScanOSCVEs.UseVisualStyleBackColor = true;
            // 
            // btnCVEJSON
            // 
            btnCVEJSON.AutoSize = false;
            btnCVEJSON.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnCVEJSON.Density = MaterialButton.MaterialButtonDensity.Default;
            btnCVEJSON.Depth = 0;
            btnCVEJSON.HighEmphasis = true;
            btnCVEJSON.Icon = null;
            btnCVEJSON.Location = new Point(314, 20);
            btnCVEJSON.Margin = new Padding(4, 6, 4, 6);
            btnCVEJSON.MouseState = MaterialSkin.MouseState.HOVER;
            btnCVEJSON.Name = "btnCVEJSON";
            btnCVEJSON.NoAccentTextColor = Color.Empty;
            btnCVEJSON.Size = new Size(106, 36);
            btnCVEJSON.TabIndex = 2;
            btnCVEJSON.Text = "List CVEs";
            btnCVEJSON.Type = MaterialButton.MaterialButtonType.Contained;
            btnCVEJSON.UseAccentColor = false;
            btnCVEJSON.UseVisualStyleBackColor = true;
            btnCVEJSON.Click += BtnCVEJSON_Click;
            // 
            // btnInstall
            // 
            btnInstall.AutoSize = false;
            btnInstall.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnInstall.Density = MaterialButton.MaterialButtonDensity.Default;
            btnInstall.Depth = 0;
            btnInstall.HighEmphasis = true;
            btnInstall.Icon = null;
            btnInstall.Location = new Point(427, 20);
            btnInstall.Margin = new Padding(3, 4, 3, 4);
            btnInstall.MouseState = MaterialSkin.MouseState.HOVER;
            btnInstall.Name = "btnInstall";
            btnInstall.NoAccentTextColor = Color.Empty;
            btnInstall.Size = new Size(136, 36);
            btnInstall.TabIndex = 3;
            btnInstall.Text = "Install Latest Patch";
            btnInstall.Type = MaterialButton.MaterialButtonType.Contained;
            btnInstall.UseAccentColor = false;
            btnInstall.UseVisualStyleBackColor = true;
            btnInstall.Click += BtnInstall_Click;
            // 
            // btnUpdate
            // 
            btnUpdate.AutoSize = false;
            btnUpdate.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnUpdate.Density = MaterialButton.MaterialButtonDensity.Default;
            btnUpdate.Depth = 0;
            btnUpdate.HighEmphasis = true;
            btnUpdate.Icon = null;
            btnUpdate.Location = new Point(570, 20);
            btnUpdate.Margin = new Padding(4, 6, 4, 6);
            btnUpdate.MouseState = MaterialSkin.MouseState.HOVER;
            btnUpdate.Name = "btnUpdate";
            btnUpdate.NoAccentTextColor = Color.Empty;
            btnUpdate.Size = new Size(104, 36);
            btnUpdate.TabIndex = 4;
            btnUpdate.Text = "Download Latest DB";
            btnUpdate.Type = MaterialButton.MaterialButtonType.Contained;
            btnUpdate.UseAccentColor = false;
            btnUpdate.UseVisualStyleBackColor = true;
            btnUpdate.Click += BtnUpdate_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(813, 15);
            label4.Margin = new Padding(2, 0, 2, 0);
            label4.Name = "label4";
            label4.Size = new Size(72, 15);
            label4.TabIndex = 5;
            label4.Text = "SDK Version:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(889, 15);
            label5.Margin = new Padding(2, 0, 2, 0);
            label5.Name = "label5";
            label5.Size = new Size(0, 15);
            label5.TabIndex = 6;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(813, 32);
            label6.Margin = new Padding(2, 0, 2, 0);
            label6.Name = "label6";
            label6.Size = new Size(58, 15);
            label6.TabIndex = 7;
            label6.Text = "SDK Date:";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(875, 32);
            label7.Margin = new Padding(2, 0, 2, 0);
            label7.Name = "label7";
            label7.Size = new Size(0, 15);
            label7.TabIndex = 8;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(813, 47);
            label8.Margin = new Padding(2, 0, 2, 0);
            label8.Name = "label8";
            label8.Size = new Size(89, 15);
            label8.TabIndex = 9;
            label8.Text = "Analog Update:";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(906, 47);
            label9.Margin = new Padding(2, 0, 2, 0);
            label9.Name = "label9";
            label9.Size = new Size(0, 15);
            label9.TabIndex = 10;
            // 
            // btnUpdateSDK
            // 
            btnUpdateSDK.AutoSize = false;
            btnUpdateSDK.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnUpdateSDK.Density = MaterialButton.MaterialButtonDensity.Default;
            btnUpdateSDK.Depth = 0;
            btnUpdateSDK.HighEmphasis = true;
            btnUpdateSDK.Icon = null;
            btnUpdateSDK.Location = new Point(681, 21);
            btnUpdateSDK.Margin = new Padding(3, 4, 3, 4);
            btnUpdateSDK.MouseState = MaterialSkin.MouseState.HOVER;
            btnUpdateSDK.Name = "btnUpdateSDK";
            btnUpdateSDK.NoAccentTextColor = Color.Empty;
            btnUpdateSDK.Size = new Size(116, 36);
            btnUpdateSDK.TabIndex = 11;
            btnUpdateSDK.Text = "Download Latest SDK";
            btnUpdateSDK.Type = MaterialButton.MaterialButtonType.Contained;
            btnUpdateSDK.UseAccentColor = false;
            btnUpdateSDK.UseVisualStyleBackColor = true;
            btnUpdateSDK.Click += btnUpdateSDK_Click_1;
            // 
            // lvScanResults
            // 
            lvScanResults.Dock = DockStyle.Fill;
            lvScanResults.FullRowSelect = true;
            lvScanResults.GridLines = true;
            lvScanResults.Location = new Point(3, 80);
            lvScanResults.MultiSelect = false;
            lvScanResults.Name = "lvScanResults";
            lvScanResults.OwnerDraw = true;
            lvScanResults.Size = new Size(1052, 487);
            lvScanResults.TabIndex = 2;
            lvScanResults.UseCompatibleStateImageBehavior = false;
            lvScanResults.View = View.Details;
            lvScanResults.SelectedIndexChanged += LvScanResults_SelectedIndexChanged;
            //
            // tabBiosDrivers
            //
            tabBiosDrivers.Controls.Add(lvBiosDrivers);
            tabBiosDrivers.Controls.Add(panelBiosDrivers);
            tabBiosDrivers.Location = new Point(4, 24);
            tabBiosDrivers.Name = "tabBiosDrivers";
            tabBiosDrivers.Padding = new Padding(3);
            tabBiosDrivers.Size = new Size(1058, 570);
            tabBiosDrivers.TabIndex = 3;
            tabBiosDrivers.Text = "BIOS & Drivers";
            tabBiosDrivers.UseVisualStyleBackColor = true;
            //
            // panelBiosDrivers
            //
            panelBiosDrivers.Controls.Add(btnScanBiosDrivers);
            panelBiosDrivers.Controls.Add(lblBiosDriversSummary);
            panelBiosDrivers.Dock = DockStyle.Top;
            panelBiosDrivers.Location = new Point(3, 3);
            panelBiosDrivers.Name = "panelBiosDrivers";
            panelBiosDrivers.Size = new Size(1052, 48);
            panelBiosDrivers.TabIndex = 0;
            //
            // btnScanBiosDrivers
            //
            btnScanBiosDrivers.Location = new Point(8, 10);
            btnScanBiosDrivers.Name = "btnScanBiosDrivers";
            btnScanBiosDrivers.Size = new Size(180, 28);
            btnScanBiosDrivers.TabIndex = 0;
            btnScanBiosDrivers.Text = "Scan BIOS && Drivers";
            btnScanBiosDrivers.UseVisualStyleBackColor = true;
            btnScanBiosDrivers.Click += BtnScanBiosDrivers_Click;
            //
            // lblBiosDriversSummary
            //
            lblBiosDriversSummary.AutoSize = true;
            lblBiosDriversSummary.Location = new Point(200, 16);
            lblBiosDriversSummary.Name = "lblBiosDriversSummary";
            lblBiosDriversSummary.Size = new Size(0, 15);
            lblBiosDriversSummary.TabIndex = 1;
            //
            // lvBiosDrivers
            //
            lvBiosDrivers.Dock = DockStyle.Fill;
            lvBiosDrivers.FullRowSelect = true;
            lvBiosDrivers.GridLines = true;
            lvBiosDrivers.MultiSelect = false;
            lvBiosDrivers.Name = "lvBiosDrivers";
            lvBiosDrivers.Size = new Size(1052, 519);
            lvBiosDrivers.TabIndex = 1;
            lvBiosDrivers.UseCompatibleStateImageBehavior = false;
            lvBiosDrivers.View = View.Details;
            //
            // tbcMainView
            //
            tbcMainView.Controls.Add(tabOffline);
            tbcMainView.Controls.Add(tabOrchestrate);
            tbcMainView.Controls.Add(tabCatalog);
            tbcMainView.Controls.Add(tabBiosDrivers);
            tbcMainView.Dock = DockStyle.Fill;
            tbcMainView.Location = new Point(0, 0);
            tbcMainView.Name = "tbcMainView";
            tbcMainView.SelectedIndex = 0;
            tbcMainView.Size = new Size(1066, 598);
            tbcMainView.TabIndex = 3;
            // 
            // ScannerForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1098, 630);
            Controls.Add(panel3);
            Icon = (Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            Name = "ScannerForm";
            Text = "Acme Scanner";
            panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pbLoading).EndInit();
            panel2.ResumeLayout(false);
            tabCatalog.ResumeLayout(false);
            panel5.ResumeLayout(false);
            panel5.PerformLayout();
            panel9.ResumeLayout(false);
            panel9.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            tabOrchestrate.ResumeLayout(false);
            panel4.ResumeLayout(false);
            panel4.PerformLayout();
            tabOffline.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            tabBiosDrivers.ResumeLayout(false);
            panelBiosDrivers.ResumeLayout(false);
            panelBiosDrivers.PerformLayout();
            tbcMainView.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private Panel panel1;
        private Panel panel3;
        private Panel panel2;
        private MaterialSkin.Controls.MaterialButton btnScan;
        private ScannerListView lvScanResults;
        private System.Windows.Forms.Timer timer1;
        private TabControl tbcMainView;
        private TabPage tabBiosDrivers;
        private Panel panelBiosDrivers;
        private System.Windows.Forms.Button btnScanBiosDrivers;
        private Label lblBiosDriversSummary;
        private ScannerListView lvBiosDrivers;
        private TabPage tabOffline;
        private TabPage tabOrchestrate;
        private CheckBox cbScanOSCVEs;
        private MaterialSkin.Controls.MaterialButton btnUpdate;
        private MaterialSkin.Controls.MaterialButton btnInstall;
        private MaterialSkin.Controls.MaterialButton btnCVEJSON;
        private ScannerListView lvOrchestrationScanResult;
        private Panel panel4;
        private MaterialSkin.Controls.MaterialButton btnInstallOrchestration;
        private MaterialSkin.Controls.MaterialButton btnScanOrchestration;
        private TabPage tabCatalog;
        private ScannerListView lvCatalog;
        private Panel panel5;
        private MaterialSkin.Controls.MaterialButton mbLoad;
        public MaterialSkin.Controls.MaterialButton btnListCatalogCVE;
        private MaterialSkin.Controls.MaterialButton btnLookupCVE;
        private Label label2;
        private Label label1;
        private Label lblTotalCVEs;
        private Label lblTotalProducts;
        private Label lblTotalInstalls;
        private Label label3;
        private MaterialSkin.Controls.MaterialButton btnExportCSV;
        private MaterialSkin.Controls.MaterialButton btnFreshInstall;
        private MaterialSkin.Controls.MaterialButton btnDomainCSV;
        private Label label5;
        private Label label4;
        private Label label7;
        private Label label6;
        private Label label9;
        private Label label8;
        private Label label12;
        private Label label11;
        private Label label10;
        private Label label15;
        private Label label14;
        private Label label13;
        public PictureBox pbLoading;
        private TextBox searchCatalog;
        private Panel panel9;
        private PictureBox pictureBox1;
        private MaterialButton btnUpdateSDK;
    }
}