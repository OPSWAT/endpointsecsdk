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
        private JObject jsonContentAppRemoverCheck;
        private Dictionary<string, sanityCheckSignature> hashmap;
        private List<string> checkedBoxes;
        private List<(MaterialSkin.Controls.MaterialCheckbox, string)> checkboxScriptPairs;

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
            List<(MaterialSkin.Controls.MaterialCheckbox, string)> checkboxScriptPairs = new List<(MaterialSkin.Controls.MaterialCheckbox, string)>
            {
                (materialCheckbox1, "auto_patching_check"),
                (materialCheckbox2, "app_remover_check")
            };
            checkedBoxes = new List<string>();
            hashmap= new Dictionary<string, sanityCheckSignature>();
            foreach (var (checkbox, script) in checkboxScriptPairs)
            {
                
                checkedBoxes.Add(checkbox.Text);
                JObject jsonContent = TaskRunPythonScripts.Execute(script);
                foreach (var product in jsonContent)
                {
                    string productName = product.Key;
                    var platforms = (JObject)product.Value;

                    foreach (var platform in platforms)
                    {
                        string platformName = platform.Key;
                        var signatures = (JObject)platform.Value;

                        foreach (var signature in signatures)
                        {
                            if (hashmap.ContainsKey(signature.Key))
                            {
                                hashmap[signature.Key].scripts.Add(script);
                            }
                            else
                            {
                                hashmap[signature.Key] = new sanityCheckSignature();
                                hashmap[signature.Key].sigId = signature.Key;
                                hashmap[signature.Key].name = product.Key;
                                hashmap[signature.Key].platform = platformName;
                                hashmap[signature.Key].scripts = new List<string>();
                                hashmap[signature.Key].scripts.Add(script);
                            }
                        }

                    }
                }
                
            }
            UpdateCheckResults();
            
        }

        private void runAllChecks_Completed(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        private void runSelectedChecks_DoWork(object sender, DoWorkEventArgs e)
        {
            List<(MaterialSkin.Controls.MaterialCheckbox, string)> checkboxScriptPairs = new List<(MaterialSkin.Controls.MaterialCheckbox, string)>
            {
                (materialCheckbox1, "auto_patching_check"),
                (materialCheckbox2, "app_remover_check")
            };
            checkedBoxes = new List<string>();
            hashmap = new Dictionary<string, sanityCheckSignature>();
            

            foreach (var (checkbox, script) in checkboxScriptPairs)
            {
                if (checkbox.Checked)
                {
                    checkedBoxes.Add(checkbox.Text);
                    JObject jsonContent = TaskRunPythonScripts.Execute(script);
                    foreach (var product in jsonContent)
                    {
                        string productName = product.Key;
                        var platforms = (JObject)product.Value;

                        foreach (var platform in platforms)
                        {
                            string platformName = platform.Key;
                            var signatures = (JObject)platform.Value;

                            foreach (var signature in signatures)
                            {
                                if (hashmap.ContainsKey(signature.Key))
                                {
                                    hashmap[signature.Key].scripts.Add(script);
                                }
                                else
                                {
                                    hashmap[signature.Key] = new sanityCheckSignature();
                                    hashmap[signature.Key].sigId = signature.Key;
                                    hashmap[signature.Key].name = product.Key;
                                    hashmap[signature.Key].platform = platformName;
                                    hashmap[signature.Key].scripts = new List<string>();
                                    hashmap[signature.Key].scripts.Add(script);
                                }
                            }

                        }
                    }
                }
            }
            UpdateCheckResults();
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
                //sanityChecksListView.Items.Clear();
                sanityChecksListView.Columns.Add("Signature ID", 100);
                sanityChecksListView.Columns.Add("Name", 200);
                
                sanityChecksListView.Columns.Add("Platform", 200);
                foreach (string checkedBox in checkedBoxes)
                {
                    sanityChecksListView.Columns.Add(checkedBox, 200);
                }
                
                sanityChecksListView.View = View.Details;
                sanityChecksListView.Update();


                foreach (sanityCheckSignature sigObj in hashmap.Values)
                {
                    ListViewItem item = new ListViewItem(sigObj.sigId);
                    item.SubItems.Add(sigObj.name);
                    item.SubItems.Add(sigObj.platform);
                    for (int i = 3; i < sanityChecksListView.Columns.Count; i++)
                    {
                        string columnName = sanityChecksListView.Columns[i].Text;
                        if (sigObj.scripts.Contains(columnName))
                        {
                            item.SubItems.Add("fail");
                        }
                        else
                        {
                            item.SubItems.Add("");
                        }
                        
                    }
                    resultList.Add(item);
                }






                
                sanityChecksListView.Items.Clear();
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
