///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using VAPMAdapter.Catalog;

namespace AcmeScanner
{

    public class ScannerListView : ListView
    {
        private ListViewColumnSorter lvwColumnSorter;
        private CVEDetailsManager cveDetailsManager;
        public ScannerListView() : base()
        {
            this.OwnerDraw = true;
            this.DrawItem += new DrawListViewItemEventHandler(DrawItemEvent);
            this.DrawColumnHeader += new DrawListViewColumnHeaderEventHandler(DrawColumnHeaderEvent);
            this.DrawSubItem += new DrawListViewSubItemEventHandler(DrawSubItemEvent);

            lvwColumnSorter = new ListViewColumnSorter();
            this.ListViewItemSorter = lvwColumnSorter;
            this.ColumnClick += Lv_ColumnClick;
            this.FullRowSelect = true;
            this.GridLines = true;
            this.View = View.Details;
            this.MultiSelect = false;
            this.MouseClick += ScannerListView_MouseClick;
            this.MouseDoubleClick += ScannerListView_MouseDoubleClick;
           
        }

        

        private void Lv_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == lvwColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (lvwColumnSorter.Order == SortOrder.Ascending)
                {
                    lvwColumnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    lvwColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            this.Sort();

        }


        // Draws column headers.
        private void DrawColumnHeaderEvent(object sender,
            DrawListViewColumnHeaderEventArgs e)
        {
            using (StringFormat sf = new StringFormat())
            {
                // Store the column text alignment, letting it default
                // to Left if it has not been set to Center or Right.
                switch (e.Header.TextAlign)
                {
                    case HorizontalAlignment.Center:
                        sf.Alignment = StringAlignment.Center;
                        break;
                    case HorizontalAlignment.Right:
                        sf.Alignment = StringAlignment.Far;
                        break;
                }

                sf.LineAlignment = StringAlignment.Center;

                // Draw the background for an unselected item.
                using (SolidBrush brush =
                    new SolidBrush(System.Drawing.Color.LightBlue))
                {
                    e.Graphics.FillRectangle(brush, e.Bounds);
                }


                // Draw the header text.
                using (Font headerFont =
                            new Font("Helvetica", 10, FontStyle.Bold))
                {
                    e.Graphics.DrawString(e.Header.Text, headerFont,
                        Brushes.Black, e.Bounds, sf);
                }
            }
            return;
        }


        // Draws the backgrounds for entire ListView items.
        private void DrawItemEvent(object sender,
            DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        // Draws the backgrounds for entire ListView items.
        private void DrawSubItemEvent(object sender,
            DrawListViewSubItemEventArgs e)
        {
            e.DrawDefault = true;
        }
               

        private void CatalogClickHandler(object sender, MouseEventArgs e)
        {
            string sigId = this.SelectedItems[0].SubItems[2].Text;
            string txt = ScannerForm.ProductInfoForSignatureId(sigId);
            TextDialog textDialog = new TextDialog(txt);
            textDialog.StartPosition = FormStartPosition.CenterParent;
            textDialog.ShowDialog();
        }

        private void CveClickHandler(object sender, MouseEventArgs e)
        {
            CVEDetailsManager cveDetailsManager = new CVEDetailsManager();
            if (cveDetailsManager == null)
            {
                MessageBox.Show("CVE Details Manager is not initialized.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (this.SelectedItems.Count > 0)
            {
                string cveId = this.SelectedItems[0].SubItems[0].Text;
                string cveContent = cveDetailsManager.GetCveJsonContentById(cveId);
                TextDialog textDialog = new TextDialog(cveContent);
                textDialog.StartPosition = FormStartPosition.CenterParent;
                textDialog.ShowDialog();
            }
        }

        private void ScannerListView_MouseClick(object sender, MouseEventArgs e)
        {
            if (this.SelectedItems.Count > 0 && this.Columns[0].Text == "Application" && this.Columns[1].Text == "Installed")
            {
                ScannerForm formObject = (ScannerForm)((ScannerListView)sender).Tag;
                formObject.EnableButtons(true);
            }
        }
        private void ShowRowAsJson(object sender, MouseEventArgs e)
        {
            var selectedItem = this.SelectedItems[0];
            JObject kbObject = new JObject();

            for (int i = 0; i < selectedItem.SubItems.Count; i++)
            {                
                string key = this.Columns.Count > i ? this.Columns[i].Text : "Column" + i;
                kbObject[key] = selectedItem.SubItems[i].Text;
            }

            string kbObjectString = kbObject.ToString();
            TextDialog textDialog = new TextDialog(kbObjectString);
            textDialog.StartPosition = FormStartPosition.CenterParent;
            textDialog.ShowDialog();
        }

        private void MobyClickHandler(object sender, EventArgs e)
        {
            ScannerForm formObject = (ScannerForm)((ScannerListView)sender).Tag;
            formObject.BtnViewJson_Click(sender,e);
        }

        private void ScannerListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {

            if (this.SelectedItems.Count > 0 && this.Columns[0].Text=="Application"&& this.Columns[1].Text=="Installed")
            {
                CatalogClickHandler(sender, e);
            }            
           
            else if (this.Columns[0].Text == "CVE ID")
            {
                CveClickHandler(sender, e);
            }
            else if (this.SelectedItems.Count > 0 && this.Columns[0].Text == "Name")
            {
                MobyClickHandler(sender, e);
            }
            else if (this.SelectedItems.Count > 0 && this.Columns[0].Text != "Application Name")
            {
                ShowRowAsJson(sender, e);
            }
            
        }
    }
}
