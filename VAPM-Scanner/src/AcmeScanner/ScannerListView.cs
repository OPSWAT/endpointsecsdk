///////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////
using System.Drawing;
using System.Windows.Forms;
///  Sample Code for Acme Scanner
///  Reference Implementation for OPSWAT Endpoint SDK
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
namespace AcmeScanner
{

    public class ScannerListView : ListView
    {
#nullable disable
        private ListViewColumnSorter lvwColumnSorter;

        public ScannerListView() : base()
        {
            this.OwnerDraw = true;
            this.DrawItem += new DrawListViewItemEventHandler(DrawItemEvent);
            this.DrawColumnHeader += new DrawListViewColumnHeaderEventHandler(DrawColumnHeaderEvent);
            this.DrawSubItem += new DrawListViewSubItemEventHandler(DrawSubItemEvent);

            lvwColumnSorter = new ListViewColumnSorter();
            this.ListViewItemSorter = lvwColumnSorter;
            this.ColumnClick += lv_ColumnClick;
            this.FullRowSelect = true;
            this.GridLines = true;
            this.View = View.Details;
            this.MultiSelect = false;
        }

        private void lv_ColumnClick(object sender, ColumnClickEventArgs e)
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

    }
}
