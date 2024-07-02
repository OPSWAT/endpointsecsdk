using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VAPMAdapter.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Text.Json.Nodes;

namespace AcmeScanner
{
    public partial class MobySanityCheckDialog : Form
    {
        private System.ComponentModel.BackgroundWorker runAllChecks;
        private System.ComponentModel.BackgroundWorker runSelectedChecks;
        private JObject jsonContentAutoPatchingCheck;
        private void InitializeBackgroundWorker()
        {
            runAllChecks = new BackgroundWorker();
            runAllChecks.DoWork +=
            new DoWorkEventHandler(runAllChecks_DoWork);
            runAllChecks.RunWorkerCompleted +=
            new RunWorkerCompletedEventHandler(
            runAllChecks_Completed);

            runSelectedChecks = new BackgroundWorker();
            runSelectedChecks.DoWork +=
            new DoWorkEventHandler(runSelectedChecks_DoWork);
            runSelectedChecks.RunWorkerCompleted +=
            new RunWorkerCompletedEventHandler(
            runSelectedChecks_Completed);
        }
        private void runAllChecks_DoWork(object sender, DoWorkEventArgs e)
        {
            jsonContentAutoPatchingCheck = TaskRunPythonScripts.Execute("auto_patching_check");
            UpdateCheckResults();
            
        }

        private void runAllChecks_Completed(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        private void runSelectedChecks_DoWork(object sender, DoWorkEventArgs e)
        {

        }

        private void runSelectedChecks_Completed(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        private void UpdateCheckResults()
        {
            this.Invoke((MethodInvoker)delegate
            {
                List<ListViewItem> resultList = new List<ListViewItem>();
                sanityChecksListView.Columns.Clear();
                sanityChecksListView.Columns.Add("Name", 200);
                sanityChecksListView.Columns.Add("Signature ID", 100);
                sanityChecksListView.Columns.Add("Platform", 200);
                if (!(jsonContentAutoPatchingCheck == null)) { sanityChecksListView.Columns.Add("auto_patching_check Result", 200); }
                
                sanityChecksListView.View = View.Details;
                sanityChecksListView.Update();
                foreach (var product in jsonContentAutoPatchingCheck)
                {
                    string productName = product.Key;
                    var platforms = (JObject)product.Value;

                    foreach (var platform in platforms)
                    {
                        string platformName = platform.Key;
                        var signatures = (JObject)platform.Value;

                        foreach (var signature in signatures)
                        {
                            string signatureId = signature.Key;
                            bool autoPatchingCheckResult = (bool)signature.Value;

                            // Create a new ListViewItem
                            ListViewItem item = new ListViewItem(productName);
                            item.SubItems.Add(signatureId);
                            item.SubItems.Add(platformName);
                            if (!autoPatchingCheckResult) { item.SubItems.Add("Fail"); }
                            else { item.SubItems.Add(""); }

                            // Add the item to the resultList
                            resultList.Add(item);
                        }
                    }
                }

                // Add the items to the ListView
                sanityChecksListView.Items.AddRange(resultList.ToArray());
            });
        }

        public MobySanityCheckDialog()
        {
            InitializeComponent();
            InitializeBackgroundWorker();
        }

        private void MobySanityCheckDialog_Load(object sender, EventArgs e)
        {

        }

        private void btnRunAllChecksMoby_Click(object sender, EventArgs e)
        {
            runAllChecks.RunWorkerAsync();
        }

        private void btnRunSelectedChecksMoby_Click(object sender, EventArgs e)
        {
            runSelectedChecks.RunWorkerAsync();
        }
    }
}
