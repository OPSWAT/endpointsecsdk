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

namespace AcmeScanner.Dialogs
{


    internal class CVEListDialog : Form
    {
        private Panel panel1;
        private MaterialSkin.Controls.MaterialButton btnDetails;
        private ScannerListView lvCVEList;
        private MaterialSkin.Controls.MaterialLabel lblApplication;
        private MaterialSkin.Controls.MaterialButton btnClose;

        private List<CVEDetail> CVEDetailList;

        private void InitializeComponent()
        {
            panel1 = new Panel();
            lvCVEList = new ScannerListView();
            btnClose = new MaterialSkin.Controls.MaterialButton();
            btnDetails = new MaterialSkin.Controls.MaterialButton();
            lblApplication = new MaterialSkin.Controls.MaterialLabel();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BorderStyle = BorderStyle.Fixed3D;
            panel1.Controls.Add(lvCVEList);
            panel1.Location = new System.Drawing.Point(12, 41);
            panel1.Name = "panel1";
            panel1.Padding = new Padding(10);
            panel1.Size = new System.Drawing.Size(1032, 526);
            panel1.TabIndex = 0;
            // 
            // lvCVEList
            // 
            lvCVEList.Dock = DockStyle.Fill;
            lvCVEList.FullRowSelect = true;
            lvCVEList.GridLines = true;
            lvCVEList.Location = new System.Drawing.Point(10, 10);
            lvCVEList.MultiSelect = false;
            lvCVEList.Name = "lvCVEList";
            lvCVEList.OwnerDraw = true;
            lvCVEList.Size = new System.Drawing.Size(1008, 502);
            lvCVEList.TabIndex = 0;
            lvCVEList.UseCompatibleStateImageBehavior = false;
            lvCVEList.View = View.Details;
            // 
            // btnClose
            // 
            btnClose.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnClose.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnClose.Depth = 0;
            btnClose.HighEmphasis = true;
            btnClose.Icon = null;
            btnClose.Location = new System.Drawing.Point(966, 576);
            btnClose.Margin = new Padding(4, 6, 4, 6);
            btnClose.MouseState = MaterialSkin.MouseState.HOVER;
            btnClose.Name = "btnClose";
            btnClose.NoAccentTextColor = System.Drawing.Color.Empty;
            btnClose.Size = new System.Drawing.Size(66, 36);
            btnClose.TabIndex = 1;
            btnClose.Text = "Close";
            btnClose.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnClose.UseAccentColor = false;
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += new EventHandler(btnClose_Click);
            // 
            // btnDetails
            // 
            btnDetails.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnDetails.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnDetails.Depth = 0;
            btnDetails.HighEmphasis = true;
            btnDetails.Icon = null;
            btnDetails.Location = new System.Drawing.Point(847, 576);
            btnDetails.Margin = new Padding(4, 6, 4, 6);
            btnDetails.MouseState = MaterialSkin.MouseState.HOVER;
            btnDetails.Name = "btnDetails";
            btnDetails.NoAccentTextColor = System.Drawing.Color.Empty;
            btnDetails.Size = new System.Drawing.Size(98, 36);
            btnDetails.TabIndex = 2;
            btnDetails.Text = "View JSON";
            btnDetails.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnDetails.UseAccentColor = false;
            btnDetails.UseVisualStyleBackColor = true;
            btnDetails.Click += new EventHandler(btnDetails_Click);
            // 
            // lblApplication
            // 
            lblApplication.AutoSize = true;
            lblApplication.Depth = 0;
            lblApplication.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            lblApplication.Location = new System.Drawing.Point(24, 9);
            lblApplication.MouseState = MaterialSkin.MouseState.HOVER;
            lblApplication.Name = "lblApplication";
            lblApplication.Size = new System.Drawing.Size(81, 19);
            lblApplication.TabIndex = 3;
            lblApplication.Text = "Application";
            // 
            // CVEListDialog
            // 
            ClientSize = new System.Drawing.Size(1056, 629);
            Controls.Add(lblApplication);
            Controls.Add(btnDetails);
            Controls.Add(btnClose);
            Controls.Add(panel1);
            Name = "CVEListDialog";
            panel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }

        public CVEListDialog(string productName, List<CVEDetail> cveDetailList)
        {
            InitializeComponent();
            CVEDetailList = cveDetailList;

            lblApplication.Text = "Application:  " + productName;

            populateList();
        }

        private void populateList()
        {
            lvCVEList.Columns.Add("CVE", 150);
            lvCVEList.Columns.Add("Severity", 100);
            lvCVEList.Columns.Add("Description", 900);

            foreach (CVEDetail current in CVEDetailList)
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
            Close();
        }

        private void btnDetails_Click(object sender, EventArgs e)
        {
            StringBuilder cveJson = new StringBuilder();

            foreach (CVEDetail current in CVEDetailList)
            {
                cveJson.Append(current.rawData);
            }

            TextDialog textDialog = new TextDialog(cveJson.ToString());
            textDialog.StartPosition = FormStartPosition.CenterParent;

            textDialog.ShowDialog();

        }
    }
}
