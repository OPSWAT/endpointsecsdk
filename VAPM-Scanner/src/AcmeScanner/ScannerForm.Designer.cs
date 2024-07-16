using System.Drawing;
using System.Windows.Forms;
using VAPMAdapter.Moby.POCO;

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
            label9 = new Label();
            label8 = new Label();
            label7 = new Label();
            label6 = new Label();
            label5 = new Label();
            label4 = new Label();
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
            label15 = new Label();
            label14 = new Label();
            label13 = new Label();
            label12 = new Label();
            label11 = new Label();
            label10 = new Label();
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
            tabPage5 = new TabPage();
            scannerListView1 = new ScannerListView();
            panel7 = new Panel();
            btnRunChecksMoby = new MaterialSkin.Controls.MaterialButton();
            btnUpdateMoby = new MaterialSkin.Controls.MaterialButton();
            btnMobyViewTotals = new MaterialSkin.Controls.MaterialButton();
            btnViewJson = new MaterialSkin.Controls.MaterialButton();
            btnLoadMoby = new MaterialSkin.Controls.MaterialButton();
            timer1 = new Timer(components);
            btnViewMobySubsets = new MaterialSkin.Controls.MaterialButton();
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
            tabPage5.SuspendLayout();
            panel7.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BorderStyle = BorderStyle.FixedSingle;
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
            panel1.Location = new System.Drawing.Point(4, 5);
            panel1.Margin = new Padding(21, 25, 21, 25);
            panel1.Name = "panel1";
            panel1.Padding = new Padding(21, 25, 21, 25);
            panel1.Size = new System.Drawing.Size(1509, 127);
            panel1.TabIndex = 1;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(941, 75);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(0, 25);
            label9.TabIndex = 10;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(811, 75);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(136, 25);
            label8.TabIndex = 9;
            label8.Text = "Analog Update:";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(941, 52);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(0, 25);
            label7.TabIndex = 8;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(811, 50);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(91, 25);
            label6.TabIndex = 7;
            label6.Text = "SDK Date:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(941, 27);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(0, 25);
            label5.TabIndex = 6;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(811, 25);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(112, 25);
            label4.TabIndex = 5;
            label4.Text = "SDK Version:";
            // 
            // btnUpdate
            // 
            btnUpdate.AutoSize = false;
            btnUpdate.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnUpdate.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnUpdate.Depth = 0;
            btnUpdate.HighEmphasis = true;
            btnUpdate.Icon = null;
            btnUpdate.Location = new System.Drawing.Point(1321, 33);
            btnUpdate.Margin = new Padding(6, 10, 6, 10);
            btnUpdate.MouseState = MaterialSkin.MouseState.HOVER;
            btnUpdate.Name = "btnUpdate";
            btnUpdate.NoAccentTextColor = System.Drawing.Color.Empty;
            btnUpdate.Size = new System.Drawing.Size(151, 60);
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
            btnInstall.Location = new System.Drawing.Point(1159, 33);
            btnInstall.Margin = new Padding(6, 10, 6, 10);
            btnInstall.MouseState = MaterialSkin.MouseState.HOVER;
            btnInstall.Name = "btnInstall";
            btnInstall.NoAccentTextColor = System.Drawing.Color.Empty;
            btnInstall.Size = new System.Drawing.Size(151, 60);
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
            btnCVEJSON.Location = new System.Drawing.Point(637, 35);
            btnCVEJSON.Margin = new Padding(6, 10, 6, 10);
            btnCVEJSON.MouseState = MaterialSkin.MouseState.HOVER;
            btnCVEJSON.Name = "btnCVEJSON";
            btnCVEJSON.NoAccentTextColor = System.Drawing.Color.Empty;
            btnCVEJSON.Size = new System.Drawing.Size(151, 60);
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
            cbScanOSCVEs.Location = new System.Drawing.Point(213, 38);
            cbScanOSCVEs.Margin = new Padding(4, 5, 4, 5);
            cbScanOSCVEs.Name = "cbScanOSCVEs";
            cbScanOSCVEs.Size = new System.Drawing.Size(218, 44);
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
            btnScan.Location = new System.Drawing.Point(27, 35);
            btnScan.Margin = new Padding(6, 10, 6, 10);
            btnScan.MouseState = MaterialSkin.MouseState.HOVER;
            btnScan.Name = "btnScan";
            btnScan.NoAccentTextColor = System.Drawing.Color.Empty;
            btnScan.Size = new System.Drawing.Size(151, 60);
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
            panel3.Margin = new Padding(21, 25, 21, 25);
            panel3.Name = "panel3";
            panel3.Padding = new Padding(21, 25, 21, 25);
            panel3.Size = new System.Drawing.Size(1569, 1050);
            panel3.TabIndex = 2;
            // 
            // pbLoading
            // 
            pbLoading.BackColor = System.Drawing.SystemColors.Window;
            pbLoading.Image = (System.Drawing.Image)resources.GetObject("pbLoading.Image");
            pbLoading.InitialImage = null;
            pbLoading.Location = new System.Drawing.Point(-469, 342);
            pbLoading.Margin = new Padding(4, 5, 4, 5);
            pbLoading.Name = "pbLoading";
            pbLoading.Size = new System.Drawing.Size(483, 515);
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
            panel2.Location = new System.Drawing.Point(21, 25);
            panel2.Margin = new Padding(4, 5, 4, 5);
            panel2.Name = "panel2";
            panel2.Size = new System.Drawing.Size(1527, 1000);
            panel2.TabIndex = 2;
            // 
            // tabCatalog
            // 
            tabCatalog.Controls.Add(tabPage1);
            tabCatalog.Controls.Add(tabPage2);
            tabCatalog.Controls.Add(tabPage3);
            tabCatalog.Controls.Add(tabPage4);
            tabCatalog.Controls.Add(tabPage5);
            tabCatalog.Dock = DockStyle.Fill;
            tabCatalog.Location = new System.Drawing.Point(0, 0);
            tabCatalog.Margin = new Padding(4, 5, 4, 5);
            tabCatalog.Name = "tabCatalog";
            tabCatalog.SelectedIndex = 0;
            tabCatalog.Size = new System.Drawing.Size(1525, 998);
            tabCatalog.TabIndex = 3;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(lvScanResults);
            tabPage1.Controls.Add(panel1);
            tabPage1.Location = new System.Drawing.Point(4, 34);
            tabPage1.Margin = new Padding(4, 5, 4, 5);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(4, 5, 4, 5);
            tabPage1.Size = new System.Drawing.Size(1517, 960);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Offline";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // lvScanResults
            // 
            lvScanResults.Dock = DockStyle.Fill;
            lvScanResults.FullRowSelect = true;
            lvScanResults.GridLines = true;
            lvScanResults.Location = new System.Drawing.Point(4, 132);
            lvScanResults.Margin = new Padding(4, 5, 4, 5);
            lvScanResults.MultiSelect = false;
            lvScanResults.Name = "lvScanResults";
            lvScanResults.OwnerDraw = true;
            lvScanResults.Size = new System.Drawing.Size(1509, 823);
            lvScanResults.TabIndex = 2;
            lvScanResults.UseCompatibleStateImageBehavior = false;
            lvScanResults.View = View.Details;
            lvScanResults.SelectedIndexChanged += lvScanResults_SelectedIndexChanged;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(lvOrchestrationScanResult);
            tabPage2.Controls.Add(panel4);
            tabPage2.Location = new System.Drawing.Point(4, 34);
            tabPage2.Margin = new Padding(4, 5, 4, 5);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(4, 5, 4, 5);
            tabPage2.Size = new System.Drawing.Size(1517, 960);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Orchestration";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // lvOrchestrationScanResult
            // 
            lvOrchestrationScanResult.Dock = DockStyle.Fill;
            lvOrchestrationScanResult.FullRowSelect = true;
            lvOrchestrationScanResult.GridLines = true;
            lvOrchestrationScanResult.Location = new System.Drawing.Point(4, 132);
            lvOrchestrationScanResult.Margin = new Padding(4, 5, 4, 5);
            lvOrchestrationScanResult.MultiSelect = false;
            lvOrchestrationScanResult.Name = "lvOrchestrationScanResult";
            lvOrchestrationScanResult.OwnerDraw = true;
            lvOrchestrationScanResult.Size = new System.Drawing.Size(1509, 823);
            lvOrchestrationScanResult.TabIndex = 4;
            lvOrchestrationScanResult.UseCompatibleStateImageBehavior = false;
            lvOrchestrationScanResult.View = View.Details;
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
            panel4.Controls.Add(btnUpdateSDK);
            panel4.Controls.Add(btnInstallOrchestration);
            panel4.Controls.Add(btnScanOrchestration);
            panel4.Dock = DockStyle.Top;
            panel4.Location = new System.Drawing.Point(4, 5);
            panel4.Margin = new Padding(21, 25, 21, 25);
            panel4.Name = "panel4";
            panel4.Padding = new Padding(21, 25, 21, 25);
            panel4.Size = new System.Drawing.Size(1509, 127);
            panel4.TabIndex = 3;
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Location = new System.Drawing.Point(941, 75);
            label15.Name = "label15";
            label15.Size = new System.Drawing.Size(0, 25);
            label15.TabIndex = 10;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new System.Drawing.Point(941, 52);
            label14.Name = "label14";
            label14.Size = new System.Drawing.Size(0, 25);
            label14.TabIndex = 9;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new System.Drawing.Point(941, 27);
            label13.Name = "label13";
            label13.Size = new System.Drawing.Size(0, 25);
            label13.TabIndex = 8;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new System.Drawing.Point(811, 75);
            label12.Name = "label12";
            label12.Size = new System.Drawing.Size(136, 25);
            label12.TabIndex = 7;
            label12.Text = "Analog Update:";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new System.Drawing.Point(811, 50);
            label11.Name = "label11";
            label11.Size = new System.Drawing.Size(91, 25);
            label11.TabIndex = 6;
            label11.Text = "SDK Date:";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new System.Drawing.Point(811, 25);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(112, 25);
            label10.TabIndex = 5;
            label10.Text = "SDK Version:";
            // 
            // btnUpdateSDK
            // 
            btnUpdateSDK.AutoSize = false;
            btnUpdateSDK.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnUpdateSDK.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnUpdateSDK.Depth = 0;
            btnUpdateSDK.HighEmphasis = true;
            btnUpdateSDK.Icon = null;
            btnUpdateSDK.Location = new System.Drawing.Point(1321, 35);
            btnUpdateSDK.Margin = new Padding(6, 10, 6, 10);
            btnUpdateSDK.MouseState = MaterialSkin.MouseState.HOVER;
            btnUpdateSDK.Name = "btnUpdateSDK";
            btnUpdateSDK.NoAccentTextColor = System.Drawing.Color.Empty;
            btnUpdateSDK.Size = new System.Drawing.Size(151, 60);
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
            btnInstallOrchestration.Location = new System.Drawing.Point(1159, 35);
            btnInstallOrchestration.Margin = new Padding(6, 10, 6, 10);
            btnInstallOrchestration.MouseState = MaterialSkin.MouseState.HOVER;
            btnInstallOrchestration.Name = "btnInstallOrchestration";
            btnInstallOrchestration.NoAccentTextColor = System.Drawing.Color.Empty;
            btnInstallOrchestration.Size = new System.Drawing.Size(151, 60);
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
            btnScanOrchestration.Location = new System.Drawing.Point(27, 35);
            btnScanOrchestration.Margin = new Padding(6, 10, 6, 10);
            btnScanOrchestration.MouseState = MaterialSkin.MouseState.HOVER;
            btnScanOrchestration.Name = "btnScanOrchestration";
            btnScanOrchestration.NoAccentTextColor = System.Drawing.Color.Empty;
            btnScanOrchestration.Size = new System.Drawing.Size(151, 60);
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
            tabPage3.Location = new System.Drawing.Point(4, 34);
            tabPage3.Margin = new Padding(4, 5, 4, 5);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new Padding(4, 5, 4, 5);
            tabPage3.Size = new System.Drawing.Size(1517, 960);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Catalog";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // lvCatalog
            // 
            lvCatalog.Dock = DockStyle.Fill;
            lvCatalog.FullRowSelect = true;
            lvCatalog.GridLines = true;
            lvCatalog.Location = new System.Drawing.Point(4, 132);
            lvCatalog.Margin = new Padding(4, 5, 4, 5);
            lvCatalog.MultiSelect = false;
            lvCatalog.Name = "lvCatalog";
            lvCatalog.OwnerDraw = true;
            lvCatalog.Size = new System.Drawing.Size(1509, 823);
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
            panel5.Location = new System.Drawing.Point(4, 5);
            panel5.Margin = new Padding(21, 25, 21, 25);
            panel5.Name = "panel5";
            panel5.Padding = new Padding(21, 25, 21, 25);
            panel5.Size = new System.Drawing.Size(1509, 127);
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
            btnDomainCSV.Location = new System.Drawing.Point(654, 35);
            btnDomainCSV.Margin = new Padding(6, 10, 6, 10);
            btnDomainCSV.MouseState = MaterialSkin.MouseState.HOVER;
            btnDomainCSV.Name = "btnDomainCSV";
            btnDomainCSV.NoAccentTextColor = System.Drawing.Color.Empty;
            btnDomainCSV.Size = new System.Drawing.Size(157, 60);
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
            btnFreshInstall.Location = new System.Drawing.Point(991, 35);
            btnFreshInstall.Margin = new Padding(6, 10, 6, 10);
            btnFreshInstall.MouseState = MaterialSkin.MouseState.HOVER;
            btnFreshInstall.Name = "btnFreshInstall";
            btnFreshInstall.NoAccentTextColor = System.Drawing.Color.Empty;
            btnFreshInstall.Size = new System.Drawing.Size(157, 60);
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
            btnExportCSV.Location = new System.Drawing.Point(1329, 35);
            btnExportCSV.Margin = new Padding(6, 10, 6, 10);
            btnExportCSV.MouseState = MaterialSkin.MouseState.HOVER;
            btnExportCSV.Name = "btnExportCSV";
            btnExportCSV.NoAccentTextColor = System.Drawing.Color.Empty;
            btnExportCSV.Size = new System.Drawing.Size(157, 60);
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
            lblTotalInstalls.Location = new System.Drawing.Point(444, 78);
            lblTotalInstalls.Margin = new Padding(4, 0, 4, 0);
            lblTotalInstalls.Name = "lblTotalInstalls";
            lblTotalInstalls.Size = new System.Drawing.Size(28, 32);
            lblTotalInstalls.TabIndex = 9;
            lblTotalInstalls.Text = "0";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            label3.Location = new System.Drawing.Point(260, 78);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(171, 32);
            label3.TabIndex = 8;
            label3.Text = "Total Install's:";
            // 
            // lblTotalCVEs
            // 
            lblTotalCVEs.AutoSize = true;
            lblTotalCVEs.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblTotalCVEs.Location = new System.Drawing.Point(444, 45);
            lblTotalCVEs.Margin = new Padding(4, 0, 4, 0);
            lblTotalCVEs.Name = "lblTotalCVEs";
            lblTotalCVEs.Size = new System.Drawing.Size(28, 32);
            lblTotalCVEs.TabIndex = 7;
            lblTotalCVEs.Text = "0";
            // 
            // lblTotalProducts
            // 
            lblTotalProducts.AutoSize = true;
            lblTotalProducts.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblTotalProducts.Location = new System.Drawing.Point(444, 10);
            lblTotalProducts.Margin = new Padding(4, 0, 4, 0);
            lblTotalProducts.Name = "lblTotalProducts";
            lblTotalProducts.Size = new System.Drawing.Size(28, 32);
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
            btnLookupCVE.Location = new System.Drawing.Point(823, 35);
            btnLookupCVE.Margin = new Padding(6, 10, 6, 10);
            btnLookupCVE.MouseState = MaterialSkin.MouseState.HOVER;
            btnLookupCVE.Name = "btnLookupCVE";
            btnLookupCVE.NoAccentTextColor = System.Drawing.Color.Empty;
            btnLookupCVE.Size = new System.Drawing.Size(157, 60);
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
            label2.Location = new System.Drawing.Point(260, 45);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(145, 32);
            label2.TabIndex = 3;
            label2.Text = "Total CVE's:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            label1.Location = new System.Drawing.Point(260, 10);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(186, 32);
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
            btnListCatalogCVE.Location = new System.Drawing.Point(1160, 35);
            btnListCatalogCVE.Margin = new Padding(6, 10, 6, 10);
            btnListCatalogCVE.MouseState = MaterialSkin.MouseState.HOVER;
            btnListCatalogCVE.Name = "btnListCatalogCVE";
            btnListCatalogCVE.NoAccentTextColor = System.Drawing.Color.Empty;
            btnListCatalogCVE.Size = new System.Drawing.Size(157, 60);
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
            mbLoad.Location = new System.Drawing.Point(27, 35);
            mbLoad.Margin = new Padding(6, 10, 6, 10);
            mbLoad.MouseState = MaterialSkin.MouseState.HOVER;
            mbLoad.Name = "mbLoad";
            mbLoad.NoAccentTextColor = System.Drawing.Color.Empty;
            mbLoad.Size = new System.Drawing.Size(177, 60);
            mbLoad.TabIndex = 0;
            mbLoad.Text = "Load Products";
            mbLoad.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            mbLoad.UseAccentColor = false;
            mbLoad.UseVisualStyleBackColor = true;
            mbLoad.Click += mbLoad_Click;
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(lvStatus);
            tabPage4.Controls.Add(panel6);
            tabPage4.Location = new System.Drawing.Point(4, 34);
            tabPage4.Margin = new Padding(4, 5, 4, 5);
            tabPage4.Name = "tabPage4";
            tabPage4.Padding = new Padding(4, 5, 4, 5);
            tabPage4.Size = new System.Drawing.Size(1517, 960);
            tabPage4.TabIndex = 3;
            tabPage4.Text = "Status";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // lvStatus
            // 
            lvStatus.Dock = DockStyle.Fill;
            lvStatus.FullRowSelect = true;
            lvStatus.GridLines = true;
            lvStatus.Location = new System.Drawing.Point(4, 132);
            lvStatus.Margin = new Padding(4, 5, 4, 5);
            lvStatus.MultiSelect = false;
            lvStatus.Name = "lvStatus";
            lvStatus.OwnerDraw = true;
            lvStatus.Size = new System.Drawing.Size(1509, 823);
            lvStatus.TabIndex = 6;
            lvStatus.UseCompatibleStateImageBehavior = false;
            lvStatus.View = View.Details;
            // 
            // panel6
            // 
            panel6.BorderStyle = BorderStyle.FixedSingle;
            panel6.Controls.Add(btnRefreshStatus);
            panel6.Dock = DockStyle.Top;
            panel6.Location = new System.Drawing.Point(4, 5);
            panel6.Margin = new Padding(21, 25, 21, 25);
            panel6.Name = "panel6";
            panel6.Padding = new Padding(21, 25, 21, 25);
            panel6.Size = new System.Drawing.Size(1509, 127);
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
            btnRefreshStatus.Location = new System.Drawing.Point(27, 35);
            btnRefreshStatus.Margin = new Padding(6, 10, 6, 10);
            btnRefreshStatus.MouseState = MaterialSkin.MouseState.HOVER;
            btnRefreshStatus.Name = "btnRefreshStatus";
            btnRefreshStatus.NoAccentTextColor = System.Drawing.Color.Empty;
            btnRefreshStatus.Size = new System.Drawing.Size(200, 70);
            btnRefreshStatus.TabIndex = 0;
            btnRefreshStatus.Text = "Update Patch Status";
            btnRefreshStatus.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnRefreshStatus.UseAccentColor = false;
            btnRefreshStatus.UseVisualStyleBackColor = true;
            btnRefreshStatus.Click += btnRefreshStatus_Click;
            // 
            // tabPage5
            // 
            tabPage5.Controls.Add(scannerListView1);
            tabPage5.Controls.Add(panel7);
            tabPage5.Location = new System.Drawing.Point(4, 34);
            tabPage5.Margin = new Padding(4, 5, 4, 5);
            tabPage5.Name = "tabPage5";
            tabPage5.Padding = new Padding(4, 5, 4, 5);
            tabPage5.Size = new System.Drawing.Size(1517, 960);
            tabPage5.TabIndex = 4;
            tabPage5.Text = "Moby";
            tabPage5.UseVisualStyleBackColor = true;
            // 
            // scannerListView1
            // 
            scannerListView1.Dock = DockStyle.Fill;
            scannerListView1.FullRowSelect = true;
            scannerListView1.GridLines = true;
            scannerListView1.Location = new System.Drawing.Point(4, 93);
            scannerListView1.MultiSelect = false;
            scannerListView1.Name = "scannerListView1";
            scannerListView1.OwnerDraw = true;
            scannerListView1.Size = new System.Drawing.Size(1509, 862);
            scannerListView1.TabIndex = 1;
            scannerListView1.UseCompatibleStateImageBehavior = false;
            scannerListView1.View = View.Details;
            // 
            // panel7
            // 
            panel7.Controls.Add(btnViewMobySubsets);
            panel7.Controls.Add(btnRunChecksMoby);
            panel7.Controls.Add(btnUpdateMoby);
            panel7.Controls.Add(btnMobyViewTotals);
            panel7.Controls.Add(btnViewJson);
            panel7.Controls.Add(btnLoadMoby);
            panel7.Dock = DockStyle.Top;
            panel7.Location = new System.Drawing.Point(4, 5);
            panel7.Name = "panel7";
            panel7.Size = new System.Drawing.Size(1509, 88);
            panel7.TabIndex = 0;
            // 
            // btnRunChecksMoby
            // 
            btnRunChecksMoby.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnRunChecksMoby.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnRunChecksMoby.Depth = 0;
            btnRunChecksMoby.HighEmphasis = true;
            btnRunChecksMoby.Icon = null;
            btnRunChecksMoby.Location = new System.Drawing.Point(726, 35);
            btnRunChecksMoby.Margin = new Padding(4, 7, 4, 7);
            btnRunChecksMoby.MouseState = MaterialSkin.MouseState.HOVER;
            btnRunChecksMoby.Name = "btnRunChecksMoby";
            btnRunChecksMoby.NoAccentTextColor = System.Drawing.Color.Empty;
            btnRunChecksMoby.Size = new System.Drawing.Size(111, 36);
            btnRunChecksMoby.TabIndex = 5;
            btnRunChecksMoby.Text = "Run checks";
            btnRunChecksMoby.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnRunChecksMoby.UseAccentColor = false;
            btnRunChecksMoby.UseVisualStyleBackColor = true;
            btnRunChecksMoby.Click += btnRunChecksMoby_Click;
            // 
            // btnUpdateMoby
            // 
            btnUpdateMoby.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnUpdateMoby.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnUpdateMoby.Depth = 0;
            btnUpdateMoby.HighEmphasis = true;
            btnUpdateMoby.Icon = null;
            btnUpdateMoby.Location = new System.Drawing.Point(1206, 35);
            btnUpdateMoby.Margin = new Padding(4, 7, 4, 7);
            btnUpdateMoby.MouseState = MaterialSkin.MouseState.HOVER;
            btnUpdateMoby.Name = "btnUpdateMoby";
            btnUpdateMoby.NoAccentTextColor = System.Drawing.Color.Empty;
            btnUpdateMoby.Size = new System.Drawing.Size(122, 36);
            btnUpdateMoby.TabIndex = 4;
            btnUpdateMoby.Text = "update moby";
            btnUpdateMoby.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnUpdateMoby.UseAccentColor = false;
            btnUpdateMoby.UseVisualStyleBackColor = true;
            btnUpdateMoby.Click += btnUpdateMoby_Click;
            // 
            // btnMobyViewTotals
            // 
            btnMobyViewTotals.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnMobyViewTotals.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnMobyViewTotals.Depth = 0;
            btnMobyViewTotals.HighEmphasis = true;
            btnMobyViewTotals.Icon = null;
            btnMobyViewTotals.Location = new System.Drawing.Point(937, 35);
            btnMobyViewTotals.Margin = new Padding(4, 7, 4, 7);
            btnMobyViewTotals.MouseState = MaterialSkin.MouseState.HOVER;
            btnMobyViewTotals.Name = "btnMobyViewTotals";
            btnMobyViewTotals.NoAccentTextColor = System.Drawing.Color.Empty;
            btnMobyViewTotals.Size = new System.Drawing.Size(167, 36);
            btnMobyViewTotals.TabIndex = 3;
            btnMobyViewTotals.Text = "view total counts";
            btnMobyViewTotals.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnMobyViewTotals.UseAccentColor = false;
            btnMobyViewTotals.UseVisualStyleBackColor = true;
            btnMobyViewTotals.Click += btnMobyViewTotals_Click;
            // 
            // btnViewJson
            // 
            btnViewJson.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnViewJson.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnViewJson.Depth = 0;
            btnViewJson.HighEmphasis = true;
            btnViewJson.Icon = null;
            btnViewJson.Location = new System.Drawing.Point(216, 35);
            btnViewJson.Margin = new Padding(4, 7, 4, 7);
            btnViewJson.MouseState = MaterialSkin.MouseState.HOVER;
            btnViewJson.Name = "btnViewJson";
            btnViewJson.NoAccentTextColor = System.Drawing.Color.Empty;
            btnViewJson.Size = new System.Drawing.Size(98, 36);
            btnViewJson.TabIndex = 2;
            btnViewJson.Text = "View JSON";
            btnViewJson.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnViewJson.UseAccentColor = false;
            btnViewJson.UseVisualStyleBackColor = true;
            btnViewJson.Click += btnViewJson_Click;
            // 
            // btnLoadMoby
            // 
            btnLoadMoby.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnLoadMoby.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnLoadMoby.Depth = 0;
            btnLoadMoby.HighEmphasis = true;
            btnLoadMoby.Icon = null;
            btnLoadMoby.Location = new System.Drawing.Point(27, 35);
            btnLoadMoby.Margin = new Padding(4, 7, 4, 7);
            btnLoadMoby.MouseState = MaterialSkin.MouseState.HOVER;
            btnLoadMoby.Name = "btnLoadMoby";
            btnLoadMoby.NoAccentTextColor = System.Drawing.Color.Empty;
            btnLoadMoby.Size = new System.Drawing.Size(104, 36);
            btnLoadMoby.TabIndex = 0;
            btnLoadMoby.Text = "Load Moby";
            btnLoadMoby.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnLoadMoby.UseAccentColor = false;
            btnLoadMoby.UseVisualStyleBackColor = true;
            btnLoadMoby.Click += btnLoadMoby_Click;
            // 
            // btnViewMobySubsets
            // 
            btnViewMobySubsets.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnViewMobySubsets.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnViewMobySubsets.Depth = 0;
            btnViewMobySubsets.HighEmphasis = true;
            btnViewMobySubsets.Icon = null;
            btnViewMobySubsets.Location = new System.Drawing.Point(553, 35);
            btnViewMobySubsets.Margin = new Padding(4, 6, 4, 6);
            btnViewMobySubsets.MouseState = MaterialSkin.MouseState.HOVER;
            btnViewMobySubsets.Name = "btnViewMobySubsets";
            btnViewMobySubsets.NoAccentTextColor = System.Drawing.Color.Empty;
            btnViewMobySubsets.Size = new System.Drawing.Size(123, 36);
            btnViewMobySubsets.TabIndex = 6;
            btnViewMobySubsets.Text = "View Subsets";
            btnViewMobySubsets.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnViewMobySubsets.UseAccentColor = false;
            btnViewMobySubsets.UseVisualStyleBackColor = true;
            btnViewMobySubsets.Click += btnViewMobySubsets_Click;
            // 
            // ScannerForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1569, 1050);
            Controls.Add(panel3);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 5, 4, 5);
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
            panel4.PerformLayout();
            tabPage3.ResumeLayout(false);
            panel5.ResumeLayout(false);
            panel5.PerformLayout();
            tabPage4.ResumeLayout(false);
            panel6.ResumeLayout(false);
            tabPage5.ResumeLayout(false);
            panel7.ResumeLayout(false);
            panel7.PerformLayout();
            ResumeLayout(false);

            //mobysubsets
            mobySubsetsPanel = new Panel
            {
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Fill,
                Padding = new Padding(21, 25, 21, 25),
                Size = new System.Drawing.Size(1509, 300), // Adjust size as needed
                Visible = false  // Initially hidden
            };

            // Close Button
            Button btnClose = new Button
            {
                Text = "Close",
                Size = new Size(75, 30),  // Set a specific size for the button
                Dock = DockStyle.Bottom,  // Keep it at the top of the panel
                Margin = new Padding(5)
            };
            btnClose.Click += BtnMobysubsetClose_Click;  // Event handler for closing the panel
            mobySubsetsPanel.Controls.Add(btnClose);
            

            // You can add a ListView or any other controls here
            ListView listView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Margin = new Padding(30, 30, 30, 30)
            };

            // Add columns to the ListView
            listView.Columns.Add("JSON File Name", 200);
            listView.Columns.Add("Timestamp (GMT 0 Time)", 200);
            mobySubsetsPanel.Controls.Add(listView);

            // Add the panel to the form's controls
            this.Controls.Add(mobySubsetsPanel);

        }

        #endregion
        private Panel mobySubsetsPanel;
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
        //moby
        private TabPage tabPage5;
        private ScannerListView lvStatus;
        private Panel panel6;
        private MaterialSkin.Controls.MaterialButton btnRefreshStatus;
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
        private Panel panel7;
        private ScannerListView scannerListView1;
        private MaterialSkin.Controls.MaterialButton btnLoadMoby;
        private MobyTotalCounts mobyCounts;
        private MaterialSkin.Controls.MaterialButton btnViewJson;
        private MaterialSkin.Controls.MaterialButton btnMobyViewTotals;
        private MaterialSkin.Controls.MaterialButton btnUpdateMoby;
        private MaterialSkin.Controls.MaterialButton btnRunChecksMoby;
        public PictureBox pbLoading;
        private MaterialSkin.Controls.MaterialButton btnViewMobySubsets;
    }
}