using System.Windows.Forms;

namespace OPSWATPosture
{
    partial class MainForm
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
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("");
            this.GeolocationTab = new System.Windows.Forms.TabControl();
            this.tabPolicyCheck = new System.Windows.Forms.TabPage();
            this.lvPolicy = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.gbEncryption = new System.Windows.Forms.GroupBox();
            this.cbEncrytionDriveEncrypted = new System.Windows.Forms.CheckBox();
            this.comboEncryptionProduct = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.gbFirewall = new System.Windows.Forms.GroupBox();
            this.cbFirewallEnforced = new System.Windows.Forms.CheckBox();
            this.comboFirewallProduct = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.gbAntimalware = new System.Windows.Forms.GroupBox();
            this.dtAntimalwareScanDate = new System.Windows.Forms.DateTimePicker();
            this.label11 = new System.Windows.Forms.Label();
            this.cbValidateAntimalware = new System.Windows.Forms.CheckBox();
            this.dtDefinitionDate = new System.Windows.Forms.DateTimePicker();
            this.label4 = new System.Windows.Forms.Label();
            this.comboAntimalwareProduct = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbEncryptionEnabled = new System.Windows.Forms.CheckBox();
            this.cbFirewallEnabled = new System.Windows.Forms.CheckBox();
            this.pbStatusIcon = new System.Windows.Forms.PictureBox();
            this.cbAntimalwareEnabled = new System.Windows.Forms.CheckBox();
            this.btnCheckPolicy = new System.Windows.Forms.Button();
            this.tabScore = new System.Windows.Forms.TabPage();
            this.lvSecurityScore = new System.Windows.Forms.ListView();
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label10 = new System.Windows.Forms.Label();
            this.lblCurrentSecurityScore = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.lblConfiguredSecurityScore = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.tbSecurityScore = new System.Windows.Forms.TrackBar();
            this.btnGetSecurityScore = new System.Windows.Forms.Button();
            this.pbScoreImage = new System.Windows.Forms.PictureBox();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.linkOrigin = new System.Windows.Forms.LinkLabel();
            this.label19 = new System.Windows.Forms.Label();
            this.linkGeolocation = new System.Windows.Forms.LinkLabel();
            this.lblMiles = new System.Windows.Forms.Label();
            this.lblLatitude = new System.Windows.Forms.Label();
            this.lblLongitude = new System.Windows.Forms.Label();
            this.lblCountry = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.btnGetLocation = new System.Windows.Forms.Button();
            this.pbGeoFenceResult = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.rbAllowedCountries = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.tbLatitude = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbLongitude = new System.Windows.Forms.TextBox();
            this.tbMiles = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cbAllowedCountries = new System.Windows.Forms.CheckedListBox();
            this.rbDistanceInMiles = new System.Windows.Forms.RadioButton();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.label21 = new System.Windows.Forms.Label();
            this.cblBrowsers = new System.Windows.Forms.CheckedListBox();
            this.label20 = new System.Windows.Forms.Label();
            this.clbBlockedPlugins = new System.Windows.Forms.CheckedListBox();
            this.lvPlugins = new OPSWATPosture.PostureListView();
            this.btnCheckPlugins = new System.Windows.Forms.Button();
            this.pbPluginStatus = new System.Windows.Forms.PictureBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pbLoader = new System.Windows.Forms.PictureBox();
            this.GeolocationTab.SuspendLayout();
            this.tabPolicyCheck.SuspendLayout();
            this.gbEncryption.SuspendLayout();
            this.gbFirewall.SuspendLayout();
            this.gbAntimalware.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbStatusIcon)).BeginInit();
            this.tabScore.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbSecurityScore)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbScoreImage)).BeginInit();
            this.tabPage1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbGeoFenceResult)).BeginInit();
            this.panel1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbPluginStatus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbLoader)).BeginInit();
            this.SuspendLayout();
            // 
            // GeolocationTab
            // 
            this.GeolocationTab.Controls.Add(this.tabPolicyCheck);
            this.GeolocationTab.Controls.Add(this.tabScore);
            this.GeolocationTab.Controls.Add(this.tabPage1);
            this.GeolocationTab.Controls.Add(this.tabPage2);
            this.GeolocationTab.Location = new System.Drawing.Point(12, 83);
            this.GeolocationTab.Name = "GeolocationTab";
            this.GeolocationTab.SelectedIndex = 0;
            this.GeolocationTab.Size = new System.Drawing.Size(810, 407);
            this.GeolocationTab.TabIndex = 4;
            // 
            // tabPolicyCheck
            // 
            this.tabPolicyCheck.Controls.Add(this.pbLoader);
            this.tabPolicyCheck.Controls.Add(this.lvPolicy);
            this.tabPolicyCheck.Controls.Add(this.gbEncryption);
            this.tabPolicyCheck.Controls.Add(this.gbFirewall);
            this.tabPolicyCheck.Controls.Add(this.gbAntimalware);
            this.tabPolicyCheck.Controls.Add(this.cbEncryptionEnabled);
            this.tabPolicyCheck.Controls.Add(this.cbFirewallEnabled);
            this.tabPolicyCheck.Controls.Add(this.pbStatusIcon);
            this.tabPolicyCheck.Controls.Add(this.cbAntimalwareEnabled);
            this.tabPolicyCheck.Controls.Add(this.btnCheckPolicy);
            this.tabPolicyCheck.Location = new System.Drawing.Point(4, 22);
            this.tabPolicyCheck.Name = "tabPolicyCheck";
            this.tabPolicyCheck.Padding = new System.Windows.Forms.Padding(3);
            this.tabPolicyCheck.Size = new System.Drawing.Size(802, 381);
            this.tabPolicyCheck.TabIndex = 0;
            this.tabPolicyCheck.Text = "Policy Check";
            this.tabPolicyCheck.UseVisualStyleBackColor = true;
            // 
            // lvPolicy
            // 
            this.lvPolicy.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.lvPolicy.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lvPolicy.HideSelection = false;
            this.lvPolicy.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1});
            this.lvPolicy.Location = new System.Drawing.Point(14, 243);
            this.lvPolicy.Name = "lvPolicy";
            this.lvPolicy.Size = new System.Drawing.Size(774, 119);
            this.lvPolicy.TabIndex = 7;
            this.lvPolicy.UseCompatibleStateImageBehavior = false;
            this.lvPolicy.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "";
            this.columnHeader1.Width = 600;
            // 
            // gbEncryption
            // 
            this.gbEncryption.Controls.Add(this.cbEncrytionDriveEncrypted);
            this.gbEncryption.Controls.Add(this.comboEncryptionProduct);
            this.gbEncryption.Controls.Add(this.label3);
            this.gbEncryption.Enabled = false;
            this.gbEncryption.Location = new System.Drawing.Point(568, 52);
            this.gbEncryption.Name = "gbEncryption";
            this.gbEncryption.Size = new System.Drawing.Size(220, 173);
            this.gbEncryption.TabIndex = 6;
            this.gbEncryption.TabStop = false;
            this.gbEncryption.Text = "Encryption";
            // 
            // cbEncrytionDriveEncrypted
            // 
            this.cbEncrytionDriveEncrypted.AutoSize = true;
            this.cbEncrytionDriveEncrypted.Location = new System.Drawing.Point(9, 19);
            this.cbEncrytionDriveEncrypted.Name = "cbEncrytionDriveEncrypted";
            this.cbEncrytionDriveEncrypted.Size = new System.Drawing.Size(112, 17);
            this.cbEncrytionDriveEncrypted.TabIndex = 5;
            this.cbEncrytionDriveEncrypted.Text = "Drive is Encrypted";
            this.cbEncrytionDriveEncrypted.UseVisualStyleBackColor = true;
            // 
            // comboEncryptionProduct
            // 
            this.comboEncryptionProduct.FormattingEnabled = true;
            this.comboEncryptionProduct.Location = new System.Drawing.Point(55, 42);
            this.comboEncryptionProduct.Name = "comboEncryptionProduct";
            this.comboEncryptionProduct.Size = new System.Drawing.Size(157, 21);
            this.comboEncryptionProduct.TabIndex = 4;
            this.comboEncryptionProduct.Text = "Any";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 45);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Product";
            // 
            // gbFirewall
            // 
            this.gbFirewall.Controls.Add(this.cbFirewallEnforced);
            this.gbFirewall.Controls.Add(this.comboFirewallProduct);
            this.gbFirewall.Controls.Add(this.label2);
            this.gbFirewall.Enabled = false;
            this.gbFirewall.Location = new System.Drawing.Point(342, 52);
            this.gbFirewall.Name = "gbFirewall";
            this.gbFirewall.Size = new System.Drawing.Size(220, 173);
            this.gbFirewall.TabIndex = 5;
            this.gbFirewall.TabStop = false;
            this.gbFirewall.Text = "Firewall";
            // 
            // cbFirewallEnforced
            // 
            this.cbFirewallEnforced.AutoSize = true;
            this.cbFirewallEnforced.Location = new System.Drawing.Point(6, 19);
            this.cbFirewallEnforced.Name = "cbFirewallEnforced";
            this.cbFirewallEnforced.Size = new System.Drawing.Size(107, 17);
            this.cbFirewallEnforced.TabIndex = 4;
            this.cbFirewallEnforced.Text = "Firewall Enforced";
            this.cbFirewallEnforced.UseVisualStyleBackColor = true;
            // 
            // comboFirewallProduct
            // 
            this.comboFirewallProduct.FormattingEnabled = true;
            this.comboFirewallProduct.Location = new System.Drawing.Point(53, 42);
            this.comboFirewallProduct.Name = "comboFirewallProduct";
            this.comboFirewallProduct.Size = new System.Drawing.Size(157, 21);
            this.comboFirewallProduct.TabIndex = 3;
            this.comboFirewallProduct.Text = "Any";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Product";
            // 
            // gbAntimalware
            // 
            this.gbAntimalware.Controls.Add(this.dtAntimalwareScanDate);
            this.gbAntimalware.Controls.Add(this.label11);
            this.gbAntimalware.Controls.Add(this.cbValidateAntimalware);
            this.gbAntimalware.Controls.Add(this.dtDefinitionDate);
            this.gbAntimalware.Controls.Add(this.label4);
            this.gbAntimalware.Controls.Add(this.comboAntimalwareProduct);
            this.gbAntimalware.Controls.Add(this.label1);
            this.gbAntimalware.Enabled = false;
            this.gbAntimalware.Location = new System.Drawing.Point(116, 52);
            this.gbAntimalware.Name = "gbAntimalware";
            this.gbAntimalware.Size = new System.Drawing.Size(220, 173);
            this.gbAntimalware.TabIndex = 4;
            this.gbAntimalware.TabStop = false;
            this.gbAntimalware.Text = "Antimalware";
            // 
            // dtAntimalwareScanDate
            // 
            this.dtAntimalwareScanDate.Location = new System.Drawing.Point(9, 147);
            this.dtAntimalwareScanDate.Name = "dtAntimalwareScanDate";
            this.dtAntimalwareScanDate.Size = new System.Drawing.Size(200, 20);
            this.dtAntimalwareScanDate.TabIndex = 8;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 127);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(81, 13);
            this.label11.TabIndex = 7;
            this.label11.Text = "Last Scan Date";
            // 
            // cbValidateAntimalware
            // 
            this.cbValidateAntimalware.AutoSize = true;
            this.cbValidateAntimalware.Location = new System.Drawing.Point(9, 19);
            this.cbValidateAntimalware.Name = "cbValidateAntimalware";
            this.cbValidateAntimalware.Size = new System.Drawing.Size(124, 17);
            this.cbValidateAntimalware.TabIndex = 5;
            this.cbValidateAntimalware.Text = "Validate No Malware";
            this.cbValidateAntimalware.UseVisualStyleBackColor = true;
            // 
            // dtDefinitionDate
            // 
            this.dtDefinitionDate.Location = new System.Drawing.Point(9, 91);
            this.dtDefinitionDate.Name = "dtDefinitionDate";
            this.dtDefinitionDate.Size = new System.Drawing.Size(200, 20);
            this.dtDefinitionDate.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 71);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Definition Date";
            // 
            // comboAntimalwareProduct
            // 
            this.comboAntimalwareProduct.FormattingEnabled = true;
            this.comboAntimalwareProduct.Location = new System.Drawing.Point(52, 42);
            this.comboAntimalwareProduct.Name = "comboAntimalwareProduct";
            this.comboAntimalwareProduct.Size = new System.Drawing.Size(157, 21);
            this.comboAntimalwareProduct.TabIndex = 2;
            this.comboAntimalwareProduct.Text = "Any";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 45);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Product";
            // 
            // cbEncryptionEnabled
            // 
            this.cbEncryptionEnabled.AutoSize = true;
            this.cbEncryptionEnabled.Location = new System.Drawing.Point(577, 29);
            this.cbEncryptionEnabled.Name = "cbEncryptionEnabled";
            this.cbEncryptionEnabled.Size = new System.Drawing.Size(65, 17);
            this.cbEncryptionEnabled.TabIndex = 1;
            this.cbEncryptionEnabled.Text = "Enabled";
            this.cbEncryptionEnabled.UseVisualStyleBackColor = true;
            this.cbEncryptionEnabled.CheckedChanged += new System.EventHandler(this.cbEncryptionEnabled_CheckedChanged);
            // 
            // cbFirewallEnabled
            // 
            this.cbFirewallEnabled.AutoSize = true;
            this.cbFirewallEnabled.Location = new System.Drawing.Point(348, 29);
            this.cbFirewallEnabled.Name = "cbFirewallEnabled";
            this.cbFirewallEnabled.Size = new System.Drawing.Size(65, 17);
            this.cbFirewallEnabled.TabIndex = 1;
            this.cbFirewallEnabled.Text = "Enabled";
            this.cbFirewallEnabled.UseVisualStyleBackColor = true;
            this.cbFirewallEnabled.CheckedChanged += new System.EventHandler(this.cbSystemVulnerabiltiesEnabled_CheckedChanged);
            // 
            // pbStatusIcon
            // 
            this.pbStatusIcon.Image = global::OPSWATPosture.Properties.Resources.RedLight;
            this.pbStatusIcon.InitialImage = ((System.Drawing.Image)(resources.GetObject("pbStatusIcon.InitialImage")));
            this.pbStatusIcon.Location = new System.Drawing.Point(14, 25);
            this.pbStatusIcon.Name = "pbStatusIcon";
            this.pbStatusIcon.Size = new System.Drawing.Size(89, 89);
            this.pbStatusIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbStatusIcon.TabIndex = 3;
            this.pbStatusIcon.TabStop = false;
            // 
            // cbAntimalwareEnabled
            // 
            this.cbAntimalwareEnabled.AutoSize = true;
            this.cbAntimalwareEnabled.Location = new System.Drawing.Point(125, 29);
            this.cbAntimalwareEnabled.Name = "cbAntimalwareEnabled";
            this.cbAntimalwareEnabled.Size = new System.Drawing.Size(65, 17);
            this.cbAntimalwareEnabled.TabIndex = 0;
            this.cbAntimalwareEnabled.Text = "Enabled";
            this.cbAntimalwareEnabled.UseVisualStyleBackColor = true;
            this.cbAntimalwareEnabled.CheckedChanged += new System.EventHandler(this.cbAntimalwareEnabled_CheckedChanged);
            // 
            // btnCheckPolicy
            // 
            this.btnCheckPolicy.Location = new System.Drawing.Point(14, 124);
            this.btnCheckPolicy.Name = "btnCheckPolicy";
            this.btnCheckPolicy.Size = new System.Drawing.Size(96, 23);
            this.btnCheckPolicy.TabIndex = 0;
            this.btnCheckPolicy.Text = "Check Policy";
            this.btnCheckPolicy.UseVisualStyleBackColor = true;
            this.btnCheckPolicy.Click += new System.EventHandler(this.btnCheckPolicyClick);
            // 
            // tabScore
            // 
            this.tabScore.Controls.Add(this.lvSecurityScore);
            this.tabScore.Controls.Add(this.label10);
            this.tabScore.Controls.Add(this.lblCurrentSecurityScore);
            this.tabScore.Controls.Add(this.groupBox4);
            this.tabScore.Controls.Add(this.btnGetSecurityScore);
            this.tabScore.Controls.Add(this.pbScoreImage);
            this.tabScore.Location = new System.Drawing.Point(4, 22);
            this.tabScore.Name = "tabScore";
            this.tabScore.Padding = new System.Windows.Forms.Padding(3);
            this.tabScore.Size = new System.Drawing.Size(802, 381);
            this.tabScore.TabIndex = 1;
            this.tabScore.Text = "Security Score";
            this.tabScore.UseVisualStyleBackColor = true;
            // 
            // lvSecurityScore
            // 
            this.lvSecurityScore.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2});
            this.lvSecurityScore.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lvSecurityScore.HideSelection = false;
            this.lvSecurityScore.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem2});
            this.lvSecurityScore.Location = new System.Drawing.Point(14, 178);
            this.lvSecurityScore.Name = "lvSecurityScore";
            this.lvSecurityScore.Size = new System.Drawing.Size(774, 182);
            this.lvSecurityScore.TabIndex = 14;
            this.lvSecurityScore.UseCompatibleStateImageBehavior = false;
            this.lvSecurityScore.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "";
            this.columnHeader2.Width = 600;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(169, 36);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(82, 13);
            this.label10.TabIndex = 13;
            this.label10.Text = "Detected Score";
            // 
            // lblCurrentSecurityScore
            // 
            this.lblCurrentSecurityScore.AutoSize = true;
            this.lblCurrentSecurityScore.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCurrentSecurityScore.Location = new System.Drawing.Point(174, 51);
            this.lblCurrentSecurityScore.Name = "lblCurrentSecurityScore";
            this.lblCurrentSecurityScore.Size = new System.Drawing.Size(68, 73);
            this.lblCurrentSecurityScore.TabIndex = 12;
            this.lblCurrentSecurityScore.Text = "3";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.lblConfiguredSecurityScore);
            this.groupBox4.Controls.Add(this.label7);
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Controls.Add(this.tbSecurityScore);
            this.groupBox4.Location = new System.Drawing.Point(319, 15);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(443, 144);
            this.groupBox4.TabIndex = 10;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Expected Score";
            // 
            // lblConfiguredSecurityScore
            // 
            this.lblConfiguredSecurityScore.AutoSize = true;
            this.lblConfiguredSecurityScore.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblConfiguredSecurityScore.Location = new System.Drawing.Point(347, 36);
            this.lblConfiguredSecurityScore.Name = "lblConfiguredSecurityScore";
            this.lblConfiguredSecurityScore.Size = new System.Drawing.Size(68, 73);
            this.lblConfiguredSecurityScore.TabIndex = 11;
            this.lblConfiguredSecurityScore.Text = "5";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 34);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(175, 13);
            this.label7.TabIndex = 7;
            this.label7.Text = "Specified Allowed Score for Access";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(-154, 26);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(82, 13);
            this.label6.TabIndex = 9;
            this.label6.Text = "Detected Score";
            // 
            // tbSecurityScore
            // 
            this.tbSecurityScore.Location = new System.Drawing.Point(9, 64);
            this.tbSecurityScore.Name = "tbSecurityScore";
            this.tbSecurityScore.Size = new System.Drawing.Size(310, 45);
            this.tbSecurityScore.TabIndex = 6;
            this.tbSecurityScore.Value = 5;
            this.tbSecurityScore.Scroll += new System.EventHandler(this.tbSecurityScore_Scroll);
            // 
            // btnGetSecurityScore
            // 
            this.btnGetSecurityScore.Location = new System.Drawing.Point(14, 124);
            this.btnGetSecurityScore.Name = "btnGetSecurityScore";
            this.btnGetSecurityScore.Size = new System.Drawing.Size(96, 23);
            this.btnGetSecurityScore.TabIndex = 5;
            this.btnGetSecurityScore.Text = "Get Score";
            this.btnGetSecurityScore.UseVisualStyleBackColor = true;
            this.btnGetSecurityScore.Click += new System.EventHandler(this.btnGetSecurityScore_Click);
            // 
            // pbScoreImage
            // 
            this.pbScoreImage.Image = global::OPSWATPosture.Properties.Resources.RedLight;
            this.pbScoreImage.InitialImage = ((System.Drawing.Image)(resources.GetObject("pbScoreImage.InitialImage")));
            this.pbScoreImage.Location = new System.Drawing.Point(14, 25);
            this.pbScoreImage.Name = "pbScoreImage";
            this.pbScoreImage.Size = new System.Drawing.Size(89, 89);
            this.pbScoreImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbScoreImage.TabIndex = 4;
            this.pbScoreImage.TabStop = false;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.groupBox3);
            this.tabPage1.Controls.Add(this.btnGetLocation);
            this.tabPage1.Controls.Add(this.pbGeoFenceResult);
            this.tabPage1.Controls.Add(this.panel1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(802, 381);
            this.tabPage1.TabIndex = 2;
            this.tabPage1.Text = "Geo Fencing";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.linkOrigin);
            this.groupBox3.Controls.Add(this.label19);
            this.groupBox3.Controls.Add(this.linkGeolocation);
            this.groupBox3.Controls.Add(this.lblMiles);
            this.groupBox3.Controls.Add(this.lblLatitude);
            this.groupBox3.Controls.Add(this.lblLongitude);
            this.groupBox3.Controls.Add(this.lblCountry);
            this.groupBox3.Controls.Add(this.label18);
            this.groupBox3.Controls.Add(this.label17);
            this.groupBox3.Controls.Add(this.label16);
            this.groupBox3.Controls.Add(this.label15);
            this.groupBox3.Controls.Add(this.label14);
            this.groupBox3.Location = new System.Drawing.Point(167, 162);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(604, 196);
            this.groupBox3.TabIndex = 15;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Results";
            // 
            // linkOrigin
            // 
            this.linkOrigin.AutoSize = true;
            this.linkOrigin.Location = new System.Drawing.Point(124, 161);
            this.linkOrigin.Name = "linkOrigin";
            this.linkOrigin.Size = new System.Drawing.Size(16, 13);
            this.linkOrigin.TabIndex = 11;
            this.linkOrigin.TabStop = true;
            this.linkOrigin.Text = "---";
            this.linkOrigin.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkOrigin_LinkClicked);
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(15, 161);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(60, 13);
            this.label19.TabIndex = 10;
            this.label19.Text = "Origin Link:";
            // 
            // linkGeolocation
            // 
            this.linkGeolocation.AutoSize = true;
            this.linkGeolocation.Location = new System.Drawing.Point(124, 133);
            this.linkGeolocation.Name = "linkGeolocation";
            this.linkGeolocation.Size = new System.Drawing.Size(16, 13);
            this.linkGeolocation.TabIndex = 9;
            this.linkGeolocation.TabStop = true;
            this.linkGeolocation.Text = "---";
            this.linkGeolocation.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkGeolocation_LinkClicked);
            // 
            // lblMiles
            // 
            this.lblMiles.AutoSize = true;
            this.lblMiles.Location = new System.Drawing.Point(124, 108);
            this.lblMiles.Name = "lblMiles";
            this.lblMiles.Size = new System.Drawing.Size(16, 13);
            this.lblMiles.TabIndex = 8;
            this.lblMiles.Text = "---";
            // 
            // lblLatitude
            // 
            this.lblLatitude.AutoSize = true;
            this.lblLatitude.Location = new System.Drawing.Point(124, 63);
            this.lblLatitude.Name = "lblLatitude";
            this.lblLatitude.Size = new System.Drawing.Size(16, 13);
            this.lblLatitude.TabIndex = 7;
            this.lblLatitude.Text = "---";
            // 
            // lblLongitude
            // 
            this.lblLongitude.AutoSize = true;
            this.lblLongitude.Location = new System.Drawing.Point(124, 86);
            this.lblLongitude.Name = "lblLongitude";
            this.lblLongitude.Size = new System.Drawing.Size(16, 13);
            this.lblLongitude.TabIndex = 6;
            this.lblLongitude.Text = "---";
            // 
            // lblCountry
            // 
            this.lblCountry.AutoSize = true;
            this.lblCountry.Location = new System.Drawing.Point(124, 41);
            this.lblCountry.Name = "lblCountry";
            this.lblCountry.Size = new System.Drawing.Size(16, 13);
            this.lblCountry.TabIndex = 5;
            this.lblCountry.Text = "---";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(15, 133);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(90, 13);
            this.label18.TabIndex = 4;
            this.label18.Text = "Geolocation Link:";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(15, 108);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(88, 13);
            this.label17.TabIndex = 3;
            this.label17.Text = "Miles from Policy:";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(15, 63);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(48, 13);
            this.label16.TabIndex = 2;
            this.label16.Text = "Latitude:";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(15, 86);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(57, 13);
            this.label15.TabIndex = 1;
            this.label15.Text = "Longitude:";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(15, 41);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(46, 13);
            this.label14.TabIndex = 0;
            this.label14.Text = "Country:";
            // 
            // btnGetLocation
            // 
            this.btnGetLocation.Location = new System.Drawing.Point(14, 124);
            this.btnGetLocation.Name = "btnGetLocation";
            this.btnGetLocation.Size = new System.Drawing.Size(96, 23);
            this.btnGetLocation.TabIndex = 7;
            this.btnGetLocation.Text = "Get Location";
            this.btnGetLocation.UseVisualStyleBackColor = true;
            this.btnGetLocation.Click += new System.EventHandler(this.btnGetLocation_Click);
            // 
            // pbGeoFenceResult
            // 
            this.pbGeoFenceResult.Image = global::OPSWATPosture.Properties.Resources.RedLight;
            this.pbGeoFenceResult.InitialImage = ((System.Drawing.Image)(resources.GetObject("pbGeoFenceResult.InitialImage")));
            this.pbGeoFenceResult.Location = new System.Drawing.Point(14, 25);
            this.pbGeoFenceResult.Name = "pbGeoFenceResult";
            this.pbGeoFenceResult.Size = new System.Drawing.Size(89, 89);
            this.pbGeoFenceResult.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbGeoFenceResult.TabIndex = 6;
            this.pbGeoFenceResult.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.rbAllowedCountries);
            this.panel1.Controls.Add(this.groupBox2);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.rbDistanceInMiles);
            this.panel1.Location = new System.Drawing.Point(167, 6);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(604, 150);
            this.panel1.TabIndex = 16;
            // 
            // rbAllowedCountries
            // 
            this.rbAllowedCountries.AutoSize = true;
            this.rbAllowedCountries.Checked = true;
            this.rbAllowedCountries.Location = new System.Drawing.Point(52, 11);
            this.rbAllowedCountries.Name = "rbAllowedCountries";
            this.rbAllowedCountries.Size = new System.Drawing.Size(64, 17);
            this.rbAllowedCountries.TabIndex = 7;
            this.rbAllowedCountries.TabStop = true;
            this.rbAllowedCountries.Text = "Enabled";
            this.rbAllowedCountries.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.tbLatitude);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.tbLongitude);
            this.groupBox2.Controls.Add(this.tbMiles);
            this.groupBox2.Location = new System.Drawing.Point(312, 34);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(222, 100);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Distance in Miles";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(11, 51);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(45, 13);
            this.label9.TabIndex = 18;
            this.label9.Text = "Latitude";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(11, 77);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(54, 13);
            this.label8.TabIndex = 17;
            this.label8.Text = "Longitude";
            // 
            // tbLatitude
            // 
            this.tbLatitude.Location = new System.Drawing.Point(94, 48);
            this.tbLatitude.Name = "tbLatitude";
            this.tbLatitude.Size = new System.Drawing.Size(100, 20);
            this.tbLatitude.TabIndex = 16;
            this.tbLatitude.Text = "37.764890";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 25);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(31, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Miles";
            // 
            // tbLongitude
            // 
            this.tbLongitude.Location = new System.Drawing.Point(94, 74);
            this.tbLongitude.Name = "tbLongitude";
            this.tbLongitude.Size = new System.Drawing.Size(100, 20);
            this.tbLongitude.TabIndex = 10;
            this.tbLongitude.Text = "-122.403870";
            // 
            // tbMiles
            // 
            this.tbMiles.Location = new System.Drawing.Point(94, 22);
            this.tbMiles.Name = "tbMiles";
            this.tbMiles.Size = new System.Drawing.Size(100, 20);
            this.tbMiles.TabIndex = 11;
            this.tbMiles.Text = "500";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cbAllowedCountries);
            this.groupBox1.Location = new System.Drawing.Point(52, 34);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(159, 100);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Allowed Countries";
            // 
            // cbAllowedCountries
            // 
            this.cbAllowedCountries.FormattingEnabled = true;
            this.cbAllowedCountries.Items.AddRange(new object[] {
            "United States",
            "Romania",
            "Vietnam",
            "Israel",
            "Germany",
            "United Kingdom",
            "Canada"});
            this.cbAllowedCountries.Location = new System.Drawing.Point(6, 19);
            this.cbAllowedCountries.Name = "cbAllowedCountries";
            this.cbAllowedCountries.Size = new System.Drawing.Size(120, 64);
            this.cbAllowedCountries.TabIndex = 7;
            // 
            // rbDistanceInMiles
            // 
            this.rbDistanceInMiles.AutoSize = true;
            this.rbDistanceInMiles.Location = new System.Drawing.Point(312, 11);
            this.rbDistanceInMiles.Name = "rbDistanceInMiles";
            this.rbDistanceInMiles.Size = new System.Drawing.Size(64, 17);
            this.rbDistanceInMiles.TabIndex = 15;
            this.rbDistanceInMiles.Text = "Enabled";
            this.rbDistanceInMiles.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.label21);
            this.tabPage2.Controls.Add(this.cblBrowsers);
            this.tabPage2.Controls.Add(this.label20);
            this.tabPage2.Controls.Add(this.clbBlockedPlugins);
            this.tabPage2.Controls.Add(this.lvPlugins);
            this.tabPage2.Controls.Add(this.btnCheckPlugins);
            this.tabPage2.Controls.Add(this.pbPluginStatus);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(802, 381);
            this.tabPage2.TabIndex = 3;
            this.tabPage2.Text = "Browser Plugins";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label21.Location = new System.Drawing.Point(474, 25);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(152, 20);
            this.label21.TabIndex = 15;
            this.label21.Text = "Blocked Browsers";
            // 
            // cblBrowsers
            // 
            this.cblBrowsers.FormattingEnabled = true;
            this.cblBrowsers.Items.AddRange(new object[] {
            "Microsoft Edge",
            "Mozilla Firefox (x64)",
            "Internet Explorer (x64)",
            "Opera"});
            this.cblBrowsers.Location = new System.Drawing.Point(478, 49);
            this.cblBrowsers.Name = "cblBrowsers";
            this.cblBrowsers.Size = new System.Drawing.Size(310, 94);
            this.cblBrowsers.TabIndex = 14;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label20.Location = new System.Drawing.Point(136, 25);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(136, 20);
            this.label20.TabIndex = 13;
            this.label20.Text = "Blocked Plugins";
            // 
            // clbBlockedPlugins
            // 
            this.clbBlockedPlugins.FormattingEnabled = true;
            this.clbBlockedPlugins.Items.AddRange(new object[] {
            "Google Docs Offline",
            "Firefox Alpenglow",
            "Opera Wallet",
            "Opera Ad Blocker",
            "Cashback Assistant",
            "Microsoft OneDrive for Business Browser Helper"});
            this.clbBlockedPlugins.Location = new System.Drawing.Point(140, 49);
            this.clbBlockedPlugins.Name = "clbBlockedPlugins";
            this.clbBlockedPlugins.Size = new System.Drawing.Size(310, 94);
            this.clbBlockedPlugins.TabIndex = 12;
            // 
            // lvPlugins
            // 
            this.lvPlugins.FullRowSelect = true;
            this.lvPlugins.GridLines = true;
            this.lvPlugins.HideSelection = false;
            this.lvPlugins.Location = new System.Drawing.Point(14, 149);
            this.lvPlugins.MultiSelect = false;
            this.lvPlugins.Name = "lvPlugins";
            this.lvPlugins.OwnerDraw = true;
            this.lvPlugins.Size = new System.Drawing.Size(774, 214);
            this.lvPlugins.TabIndex = 11;
            this.lvPlugins.UseCompatibleStateImageBehavior = false;
            this.lvPlugins.View = System.Windows.Forms.View.Details;
            // 
            // btnCheckPlugins
            // 
            this.btnCheckPlugins.Location = new System.Drawing.Point(14, 120);
            this.btnCheckPlugins.Name = "btnCheckPlugins";
            this.btnCheckPlugins.Size = new System.Drawing.Size(96, 23);
            this.btnCheckPlugins.TabIndex = 9;
            this.btnCheckPlugins.Text = "Check Plugins";
            this.btnCheckPlugins.UseVisualStyleBackColor = true;
            this.btnCheckPlugins.Click += new System.EventHandler(this.btnCheckPlugins_Click);
            // 
            // pbPluginStatus
            // 
            this.pbPluginStatus.Image = global::OPSWATPosture.Properties.Resources.RedLight;
            this.pbPluginStatus.InitialImage = ((System.Drawing.Image)(resources.GetObject("pbPluginStatus.InitialImage")));
            this.pbPluginStatus.Location = new System.Drawing.Point(14, 25);
            this.pbPluginStatus.Name = "pbPluginStatus";
            this.pbPluginStatus.Size = new System.Drawing.Size(89, 89);
            this.pbPluginStatus.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbPluginStatus.TabIndex = 8;
            this.pbPluginStatus.TabStop = false;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(12, 9);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(219, 37);
            this.label12.TabIndex = 5;
            this.label12.Text = "Acme Product";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(605, 48);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(213, 13);
            this.label13.TabIndex = 6;
            this.label13.Text = "Powered by OPSWAT Endpoint SDK";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(-4, -3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(840, 80);
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // pbLoader
            // 
            this.pbLoader.Image = global::OPSWATPosture.Properties.Resources.progressbar;
            this.pbLoader.Location = new System.Drawing.Point(14, 29);
            this.pbLoader.Name = "pbLoader";
            this.pbLoader.Size = new System.Drawing.Size(89, 80);
            this.pbLoader.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbLoader.TabIndex = 5;
            this.pbLoader.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(834, 503);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.GeolocationTab);
            this.Controls.Add(this.pictureBox1);
            this.Name = "MainForm";
            this.Text = "Acme Posture Check Sample";
            this.GeolocationTab.ResumeLayout(false);
            this.tabPolicyCheck.ResumeLayout(false);
            this.tabPolicyCheck.PerformLayout();
            this.gbEncryption.ResumeLayout(false);
            this.gbEncryption.PerformLayout();
            this.gbFirewall.ResumeLayout(false);
            this.gbFirewall.PerformLayout();
            this.gbAntimalware.ResumeLayout(false);
            this.gbAntimalware.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbStatusIcon)).EndInit();
            this.tabScore.ResumeLayout(false);
            this.tabScore.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbSecurityScore)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbScoreImage)).EndInit();
            this.tabPage1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbGeoFenceResult)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbPluginStatus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbLoader)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TabControl GeolocationTab;
        private System.Windows.Forms.TabPage tabPolicyCheck;
        private System.Windows.Forms.Button btnCheckPolicy;
        private System.Windows.Forms.TabPage tabScore;
        private System.Windows.Forms.PictureBox pbStatusIcon;
        private System.Windows.Forms.GroupBox gbEncryption;
        private System.Windows.Forms.GroupBox gbFirewall;
        private System.Windows.Forms.GroupBox gbAntimalware;
        private System.Windows.Forms.CheckBox cbAntimalwareEnabled;
        private System.Windows.Forms.CheckBox cbEncryptionEnabled;
        private System.Windows.Forms.CheckBox cbFirewallEnabled;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboFirewallProduct;
        private System.Windows.Forms.ComboBox comboAntimalwareProduct;
        private System.Windows.Forms.ComboBox comboEncryptionProduct;
        private System.Windows.Forms.DateTimePicker dtDefinitionDate;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox cbValidateAntimalware;
        private System.Windows.Forms.CheckBox cbFirewallEnforced;
        private System.Windows.Forms.CheckBox cbEncrytionDriveEncrypted;
        private System.Windows.Forms.Button btnGetSecurityScore;
        private System.Windows.Forms.PictureBox pbScoreImage;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TrackBar tbSecurityScore;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label lblConfiguredSecurityScore;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lblCurrentSecurityScore;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.DateTimePicker dtAntimalwareScanDate;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.ListView lvPolicy;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ListView lvSecurityScore;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.CheckedListBox cbAllowedCountries;
        private System.Windows.Forms.RadioButton rbAllowedCountries;
        private System.Windows.Forms.Button btnGetLocation;
        private System.Windows.Forms.PictureBox pbGeoFenceResult;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbMiles;
        private System.Windows.Forms.TextBox tbLongitude;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox tbLatitude;
        private System.Windows.Forms.RadioButton rbDistanceInMiles;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.LinkLabel linkGeolocation;
        private System.Windows.Forms.Label lblMiles;
        private System.Windows.Forms.Label lblLatitude;
        private System.Windows.Forms.Label lblLongitude;
        private System.Windows.Forms.Label lblCountry;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.LinkLabel linkOrigin;
        private System.Windows.Forms.Label label19;
        private PictureBox pbLoader;
        private TabPage tabPage2;
        private Button btnCheckPlugins;
        private PictureBox pbPluginStatus;
        private PostureListView lvPlugins;
        private Label label20;
        private CheckedListBox clbBlockedPlugins;
        private Label label21;
        private CheckedListBox cblBrowsers;
    }
}

