using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AcmeScanner.Dialogs;
using VAPMAdapter.Tasks;

namespace AcmeScanner
{
    public partial class LookupCVEBox : Form
    {
        public LookupCVEBox()
        {
            InitializeComponent();
        }

        private bool ShowMessageDialog(IScannerMessageDialog messageDialog)
        {
            bool result = false;

            messageDialog.SetStartPosition(FormStartPosition.CenterParent);
            messageDialog.ShowDialog();
            result = messageDialog.IsSuccess();

            return result;
        }


        private bool ShowMessageDialog(string message, bool question)
        {
            CustomMessageDialog messageDialog = new CustomMessageDialog(message, question);
            return this.ShowMessageDialog(messageDialog);
        }

        private void CallLookup_Click(object sender, EventArgs e)
        {
            string cve = TB_CVE.Text;

            string cveJson = TaskLookupCVE.LookupCVE(cve);
            if (!string.IsNullOrEmpty(cveJson))
            {
                TextDialog textDialog = new TextDialog(cveJson);
                textDialog.StartPosition = FormStartPosition.CenterParent;
                textDialog.ShowDialog();
            }
            else
            {
                ShowMessageDialog("CVE Entered is not valid.  Check the value and try again.", false);
            }
        }

        private void Download_json_Click(object sender, EventArgs e)
        {
            string cve = TB_CVE.Text;
            string cveJson = TaskLookupCVE.LookupCVE(cve);
            if (!string.IsNullOrEmpty(cveJson))
            {
                // create a writer and open the file
                TextWriter tw = new StreamWriter(TB_CVE.Text + ".txt");
                // write a line of text to the file
                tw.WriteLine(cveJson);
                // close the stream
                tw.Close();
                MessageBox.Show("Saved to " + TB_CVE.Text + ".txt", "CVE", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                ShowMessageDialog("CVE Entered is not valid.  Check the value and try again.", false);
            }
        }
    }
}
