///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using VAPMAdapter.OESIS.POCO;

namespace AcmeScanner
{


    internal class CVEListDialog : Form
    {
        #nullable disable

        private Panel panel1;
        private MaterialSkin.Controls.MaterialButton btnDetails;
        private ScannerListView lvCVEList;
        private MaterialSkin.Controls.MaterialLabel lblApplication;
        private MaterialSkin.Controls.MaterialButton btnClose;

        private List<CVEDetail> CVEDetailList;

        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.lvCVEList = new AcmeScanner.ScannerListView();
            this.btnClose = new MaterialSkin.Controls.MaterialButton();
            this.btnDetails = new MaterialSkin.Controls.MaterialButton();
            this.lblApplication = new MaterialSkin.Controls.MaterialLabel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.lvCVEList);
            this.panel1.Location = new System.Drawing.Point(12, 41);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(10);
            this.panel1.Size = new System.Drawing.Size(1032, 526);
            this.panel1.TabIndex = 0;
            // 
            // lvCVEList
            // 
            this.lvCVEList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvCVEList.FullRowSelect = true;
            this.lvCVEList.GridLines = true;
            this.lvCVEList.Location = new System.Drawing.Point(10, 10);
            this.lvCVEList.MultiSelect = false;
            this.lvCVEList.Name = "lvCVEList";
            this.lvCVEList.OwnerDraw = true;
            this.lvCVEList.Size = new System.Drawing.Size(1008, 502);
            this.lvCVEList.TabIndex = 0;
            this.lvCVEList.UseCompatibleStateImageBehavior = false;
            this.lvCVEList.View = System.Windows.Forms.View.Details;
            // 
            // btnClose
            // 
            this.btnClose.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnClose.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnClose.Depth = 0;
            this.btnClose.HighEmphasis = true;
            this.btnClose.Icon = null;
            this.btnClose.Location = new System.Drawing.Point(966, 576);
            this.btnClose.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnClose.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnClose.Name = "btnClose";
            this.btnClose.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnClose.Size = new System.Drawing.Size(66, 36);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "Close";
            this.btnClose.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnClose.UseAccentColor = false;
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnDetails
            // 
            this.btnDetails.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnDetails.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnDetails.Depth = 0;
            this.btnDetails.HighEmphasis = true;
            this.btnDetails.Icon = null;
            this.btnDetails.Location = new System.Drawing.Point(847, 576);
            this.btnDetails.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnDetails.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnDetails.Name = "btnDetails";
            this.btnDetails.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnDetails.Size = new System.Drawing.Size(98, 36);
            this.btnDetails.TabIndex = 2;
            this.btnDetails.Text = "View JSON";
            this.btnDetails.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnDetails.UseAccentColor = false;
            this.btnDetails.UseVisualStyleBackColor = true;
            this.btnDetails.Click += new System.EventHandler(this.btnDetails_Click);
            // 
            // lblApplication
            // 
            this.lblApplication.AutoSize = true;
            this.lblApplication.Depth = 0;
            this.lblApplication.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblApplication.Location = new System.Drawing.Point(24, 9);
            this.lblApplication.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblApplication.Name = "lblApplication";
            this.lblApplication.Size = new System.Drawing.Size(81, 19);
            this.lblApplication.TabIndex = 3;
            this.lblApplication.Text = "Application";
            // 
            // CVEListDialog
            // 
            this.ClientSize = new System.Drawing.Size(1056, 629);
            this.Controls.Add(this.lblApplication);
            this.Controls.Add(this.btnDetails);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.panel1);
            this.Name = "CVEListDialog";
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        public CVEListDialog(string productName, List<CVEDetail> cveDetailList)
        {
            InitializeComponent();
            this.CVEDetailList = cveDetailList;

            lblApplication.Text = "Application:  " + productName;

            populateList();
        }

        private void populateList()
        {
            lvCVEList.Columns.Add("CVE", 150);
            lvCVEList.Columns.Add("Severity", 100);
            lvCVEList.Columns.Add("Description", 900);

            foreach(CVEDetail current in CVEDetailList)
            {
                ListViewItem lviCurrent = new ListViewItem();
                lviCurrent.Text = current.cveId;
                lviCurrent.SubItems.Add(current.opswatSeverity.ToString());
                lviCurrent.SubItems.Add(current.description);
                
                lvCVEList.Items.Add(lviCurrent);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnDetails_Click(object sender, EventArgs e)
        {
            StringBuilder cveJson = new StringBuilder();

            foreach(CVEDetail current in CVEDetailList)
            {
                cveJson.Append(current.rawData);
            }
            
            TextDialog textDialog = new TextDialog(cveJson.ToString());
            textDialog.StartPosition = FormStartPosition.CenterParent;

            textDialog.ShowDialog();

        }
    }
}
