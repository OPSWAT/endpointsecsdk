///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VAPMAdapater.Updates;
using VAPMAdapter.Catalog.POCO;
using VAPMAdapter.OESIS.POCO;
using VAPMAdapter.Tasks;
using VAPMAdapter.Updates;

namespace AcmeScanner
{
    public partial class ScannerForm : Form
    {
        static Dictionary<string, ProductScanResult> staticScanResults = new Dictionary<string, ProductScanResult>();
        static Dictionary<string, CatalogSignature> staticSignatureCatalogResults = new Dictionary<string, CatalogSignature>();
        static Dictionary<string, OnlinePatchDetail> staticOrchestrationScanResults = new Dictionary<string, OnlinePatchDetail>();
        static List<CatalogProduct> staticProductList = null;


        private System.ComponentModel.BackgroundWorker scanWorker;
        private System.ComponentModel.BackgroundWorker updateDBWorker;
        private System.ComponentModel.BackgroundWorker installVAPMPatchWorker;
        private System.ComponentModel.BackgroundWorker installOnlinePatchWorker;
        private System.ComponentModel.BackgroundWorker loadCatalogWorker;


        public ScannerForm()
        {
            InitializeComponent();
            InitializeBackgroundWorker();
            CheckLicenseFiles();
            UpdateFilesOnStartup();
        }


        private void CheckLicenseFiles()
        {
            if(!File.Exists("license.cfg") || !File.Exists("pass_key.txt"))
            {
                ShowMessageDialog("This program requires the license.cfg and pass_key.txt to be in the running directory.  Please check and make sure this is correct.", false);
                Close();
            }
        }



        //
        // Update SDK if needed
        //
        private void UpdateFilesOnStartup()
        {
            if (!UpdateSDK.isSDKUpdated())
            {
                ShowLoading(true);
                updateDBWorker.RunWorkerAsync(false);
            }
        }

        // Set up the BackgroundWorker object by
        // attaching event handlers.
        private void InitializeBackgroundWorker()
        {
            scanWorker = new BackgroundWorker();
            scanWorker.DoWork +=
                new DoWorkEventHandler(scanWorker_DoWork);
            scanWorker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(
            scanWorker_Completed);


            installVAPMPatchWorker = new BackgroundWorker();
            installVAPMPatchWorker.DoWork +=
                new DoWorkEventHandler(installVAPMPatchWorker_DoWork);
            installVAPMPatchWorker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(
            installVAPMPatchWorker_Completed);

            installOnlinePatchWorker = new BackgroundWorker();
            installOnlinePatchWorker.DoWork +=
                new DoWorkEventHandler(installOnlinePatchWorker_DoWork);
            installOnlinePatchWorker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(
            installOnlinePatchWorker_Completed);


            updateDBWorker = new BackgroundWorker();
            updateDBWorker.DoWork +=
                new DoWorkEventHandler(updateDBWorker_DoWork);
            updateDBWorker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(
            updateDBWorker_Completed);

            loadCatalogWorker = new BackgroundWorker();
            loadCatalogWorker.DoWork +=
                new DoWorkEventHandler(loadCatalogWorker_DoWork);
            loadCatalogWorker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(
            loadCatalogWorker_Completed);


        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///  Worker Threads
        /// </summary>
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void scanWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            bool isOffline = (bool)e.Argument;

            if(isOffline)
            {
                // Scan Offline
                bool scanOSCVEs = cbScanOSCVEs.Checked;
                staticScanResults = TaskScanAll.Scan(scanOSCVEs);
            }
            else
            {
                // Scan Windows
                staticOrchestrationScanResults = TaskScanOrchestration.Scan();
            }

            e.Result = isOffline;
        }

        private void scanWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            bool isOffline = (bool)e.Result;

            if(isOffline)
            {
                UpdateScanResults();
            }
            else
            {
                UpdateOrchestrationScanResults();
            }

            ShowLoading(false);
        }


        private void loadCatalogWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            staticProductList = TaskLoadCatalog.Load();
            staticSignatureCatalogResults.Clear();

            foreach (CatalogProduct product in staticProductList)
            {
                foreach(CatalogSignature signature in product.SigList)
                {
                    staticSignatureCatalogResults.Add(signature.Id, signature);
                }
            }
        }

        private void loadCatalogWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (staticProductList != null)
            {
                UpdateCatalogResults();
            }

            ShowLoading(false);
        }


        private void installVAPMPatchWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string signatureId = (string)e.Argument;
            TaskDownloadAndInstallApplication.InstallAndDownload(signatureId);
        }

        private void installVAPMPatchWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            // Do a scan again
            lvScanResults.Items.Clear();
            scanWorker.RunWorkerAsync(true);
        }

        private void installOnlinePatchWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string kb = (string)e.Argument;
            OnlinePatchDetail patchDetail = staticOrchestrationScanResults[kb];

            TaskOnlineDownloadAndInstall.InstallAndDownload(patchDetail);
        }

        private void installOnlinePatchWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            // Do a scan again
            lvOrchestrationScanResult.Items.Clear();
            scanWorker.RunWorkerAsync(false);
        }



        private void updateDBWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            bool sdkOnly = (bool)e.Argument;

            UpdateSDK.DownloadAndInstall_OPSWAT_SDK();

            if (!sdkOnly)
            {
                UpdateDBFiles.DownloadFiles();
            }
        }

        private void updateDBWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            ShowLoading(false);
        }


        private bool ShowMessageDialog(string message, bool question)
        {
            bool result = false;


            CustomMessageDialog messageDialog = new CustomMessageDialog(message, question);
            messageDialog.StartPosition = FormStartPosition.CenterParent;
            messageDialog.ShowDialog();
            result = messageDialog.IsSuccess();

            return result;
        }

        private void EnableButtons(bool enabled)
        {
            btnInstall.Enabled = enabled;
            btnUpdate.Enabled = enabled;
            btnScan.Enabled = enabled;
            btnCVEJSON.Enabled = enabled;
            btnScanOrchestration.Enabled = enabled;
            btnInstallOrchestration.Enabled = enabled;
            btnUpdateSDK.Enabled = enabled;
        }

        private void ShowLoading(bool visible)
        {
            if (visible)
            {
                pbLoading.Location = new Point(ClientSize.Width / 2 - pbLoading.Size.Width / 2,
                                                ClientSize.Height / 2 - pbLoading.Size.Height / 2);

                pbLoading.BringToFront();
                EnableButtons(false);
                pbLoading.Visible = true;
            }
            else
            {
                pbLoading.SendToBack();
                pbLoading.Visible = false;
                EnableButtons(true);
            }
        }


        private int getSeverity(ProductScanResult scanResult)
        {
            int result = 0;

            if(scanResult.cveDetailList.Count > 0)
            {
                foreach(CVEDetail current in scanResult.cveDetailList)
                {
                    if(current.opswatSeverity > result)
                    {
                        result = current.opswatSeverity;
                    }
                }
            }

            return result;
        }


        private void UpdateCatalogResults()
        {
            List<ListViewItem> resultList = new List<ListViewItem>();

            //
            // Setup the header
            //

            lvCatalog.Columns.Clear();
            lvCatalog.Columns.Add("Application", 300);
            lvCatalog.Columns.Add("CVE Count", 80);
            lvCatalog.Columns.Add("Installable", 80);
            lvCatalog.Columns.Add("Platform", 100);
            lvCatalog.Columns.Add("", 400);
            lvCatalog.View = View.Details;
            lvCatalog.Update();

            int productCount = 0;
            int cveCount = 0;
            int installCount = 0;

            foreach (CatalogProduct product in staticProductList)
            {
                foreach(CatalogSignature signature in product.SigList)
                {
                    ListViewItem lviCurrent = new ListViewItem();
                    lviCurrent.Text = signature.Name;
                    lviCurrent.SubItems.Add(signature.CVECount.ToString());
                    lviCurrent.SubItems.Add(product.SupportsInstall ? "Yes" : "");
                    lviCurrent.SubItems.Add(signature.Platform);

                    lviCurrent.Tag = signature.Id;
                    resultList.Add(lviCurrent);

                    productCount++;
                    cveCount += signature.CVECount;
                    if(product.SupportsInstall)
                    {
                        installCount++;
                    }
                }
            }

            lblTotalCVEs.Text = cveCount.ToString();
            lblTotalProducts.Text = productCount.ToString();
            lblTotalInstalls.Text = installCount.ToString();

            lvCatalog.Items.Clear();
            lvCatalog.Items.AddRange(resultList.ToArray());
        }


        private void UpdateScanResults()
        {
            List<ListViewItem> resultList = new List<ListViewItem>();
            List<ProductScanResult> sortedList = staticScanResults.Values.OrderBy(o => o.product.name).ToList();

            //
            // Setup the header
            //
            lvScanResults.Columns.Clear();
            lvScanResults.Columns.Add("Application", 300);
            lvScanResults.Columns.Add("Ver", 100);
            lvScanResults.Columns.Add("Arch", 50);
            lvScanResults.Columns.Add("Lang", 50);
            lvScanResults.Columns.Add("Latest", 100);
            lvScanResults.Columns.Add("Severity", 100);
            lvScanResults.Columns.Add("CVE Count", 100);
            lvScanResults.Columns.Add("Patched", 100);
            lvScanResults.Columns.Add("Auto", 50);
            lvScanResults.Columns.Add("", 400);
            lvScanResults.View = View.Details;
            lvScanResults.Update();


            foreach (ProductScanResult current in sortedList)
            {
                int OPSWATseverity = getSeverity(current);
                int cveCount = current.cveDetailList.Count;

                ListViewItem lviCurrent = new ListViewItem();
                lviCurrent.Text = current.product.name;
                lviCurrent.SubItems.Add(current.product.versionDetail.version);
                lviCurrent.SubItems.Add(current.product.versionDetail.architecture);
                lviCurrent.SubItems.Add(current.product.versionDetail.language == "n/a" ? "" : current.product.versionDetail.language);
                lviCurrent.SubItems.Add(current.patchLevelDetail.latestVersion);
                lviCurrent.SubItems.Add(OPSWATseverity == 0 ? "" : OPSWATseverity.ToString());
                lviCurrent.SubItems.Add(cveCount == 0 ? "" : cveCount.ToString());
                lviCurrent.SubItems.Add(current.patchLevelDetail.isLatest ? "" : "Missing");
                lviCurrent.SubItems.Add(current.installDetail.Count == 0 ? "" : "Yes");

                lviCurrent.Tag = current.product.signatureId;

                resultList.Add(lviCurrent);
            }

            lvScanResults.Items.Clear();
            lvScanResults.Items.AddRange(resultList.ToArray());
        }


        private void UpdateOrchestrationScanResults()
        {
            List<ListViewItem> resultList = new List<ListViewItem>();
            List<OnlinePatchDetail> sortedList = staticOrchestrationScanResults.Values.OrderBy(o => o.title).ToList();

            //
            // Setup the header
            //
            lvOrchestrationScanResult.Columns.Clear();
            lvOrchestrationScanResult.Columns.Add("Title", 300);
            lvOrchestrationScanResult.Columns.Add("Severity", 100);
            lvOrchestrationScanResult.Columns.Add("Product", 100);
            lvOrchestrationScanResult.Columns.Add("KB", 100);
            lvOrchestrationScanResult.Columns.Add("Patched", 70);
            lvOrchestrationScanResult.Columns.Add("Description", 400);
            lvOrchestrationScanResult.Columns.Add("", 400);
            lvOrchestrationScanResult.View = View.Details;
            lvOrchestrationScanResult.Update();


            foreach(OnlinePatchDetail current in sortedList)
            {
                ListViewItem lviCurrent = new ListViewItem();
                lviCurrent.Text = current.title;
                lviCurrent.SubItems.Add(current.severity == "unknown" ? "" : current.severity);
                lviCurrent.SubItems.Add(current.product == "unknown" ? "" : current.product);
                lviCurrent.SubItems.Add(current.kb);
                lviCurrent.SubItems.Add(current.installed ? "" : "Missing");
                lviCurrent.SubItems.Add(current.description);
                lviCurrent.Tag = current.kb;

                resultList.Add(lviCurrent);
            }

            lvOrchestrationScanResult.Items.Clear();
            lvOrchestrationScanResult.Items.AddRange(resultList.ToArray());
        }


        private void btnScan_Click(object sender, EventArgs e)
        {
            ShowLoading(true);
            scanWorker.RunWorkerAsync(true);
        }

        private void btnCVEJSON_Click(object sender, EventArgs e)
        {
            if (lvScanResults.SelectedItems != null && lvScanResults.SelectedItems.Count > 0)
            {

                string signatureId = lvScanResults.SelectedItems[0].Tag.ToString();

                ProductScanResult scanResult = staticScanResults[signatureId];

                if(scanResult.cveDetailList.Count > 0)
                {
                    CVEListDialog cveDialog = new CVEListDialog(scanResult.product.name, scanResult.cveDetailList);
                    cveDialog.StartPosition = FormStartPosition.CenterParent;

                    cveDialog.ShowDialog();
                }
                else
                {
                    ShowMessageDialog("There are no CVE's on the selected item.", false);
                }
            }
            else
            {
                ShowMessageDialog("There is not an item selected.", false);
            }
        }


        private void btnInstall_Click(object sender, EventArgs e)
        {
            if (lvScanResults.SelectedItems.Count > 0)
            {
                string signatureId = lvScanResults.SelectedItems[0].Tag.ToString();
                ProductScanResult scanResult = staticScanResults[signatureId];

                if (scanResult != null)
                {
                    if (scanResult.installDetail.Count > 0)
                    {
                        if (!scanResult.patchLevelDetail.isLatest)
                        {
                            if (ShowMessageDialog("Are you sure you want to install \"" + scanResult.product.name + "\"", true))
                            {
                                ShowLoading(true);
                                installVAPMPatchWorker.RunWorkerAsync(signatureId);
                            }
                        }
                        else
                        {
                            ShowMessageDialog("Product is currently the latest", false);
                        }
                    }
                    else
                    {
                        ShowMessageDialog("Auto patching is not available for this product.", false);
                    }
                }
                else
                {
                    ShowMessageDialog("Product not found", false);
                }
            }
            else
            {
                ShowMessageDialog("Make sure to Scan for products.  After doing that select a product to install.", false);
            }

        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            ShowLoading(true);
            updateDBWorker.RunWorkerAsync(false);
        }

        private void btnUpdateSDK_Click(object sender, EventArgs e)
        {
            ShowLoading(true);
            updateDBWorker.RunWorkerAsync(true);
        }

        private void btnScanOrchestration_Click(object sender, EventArgs e)
        {
            ShowLoading(true);
            scanWorker.RunWorkerAsync(false); // This will not scan offline
        }

        private void btnInstallOrchestration_Click(object sender, EventArgs e)
        {
            if (lvOrchestrationScanResult.SelectedItems.Count > 0)
            {
                string kbId = lvOrchestrationScanResult.SelectedItems[0].Tag.ToString();
                OnlinePatchDetail patchDetail = staticOrchestrationScanResults[kbId];

                if (patchDetail != null)
                {
                    if (!patchDetail.installed)
                    {
                        if (ShowMessageDialog("Are you sure you want to install \"" + patchDetail.title + "\"", true))
                        {
                            ShowLoading(true);
                            installOnlinePatchWorker.RunWorkerAsync(kbId);
                        }
                    }
                    else
                    {
                        ShowMessageDialog("Patch is currently installed", false);
                    }
                }
                else
                {
                    ShowMessageDialog("Patch not found", false);
                }
            }
            else
            {
                ShowMessageDialog("Make sure to Scan for products.  After doing that select a product to install.", false);
            }


        }

        private void mbLoad_Click(object sender, EventArgs e)
        {
            ShowLoading(true);
            loadCatalogWorker.RunWorkerAsync();
        }

        private void btnListCatalogCVE_Click(object sender, EventArgs e)
        {
            if (lvCatalog.SelectedItems != null && lvCatalog.SelectedItems.Count > 0)
            {

                string signatureID = lvCatalog.SelectedItems[0].Tag.ToString();
                CatalogSignature signature = staticSignatureCatalogResults[signatureID];

                if (signature != null && signature.CVECount > 0)
                {
                    List<CVEDetail> cveDetailList = TaskGetCVEDetails.GetCveDetailList(signature.CVEList);
                    CVEListDialog cveDialog = new CVEListDialog(signature.Name, cveDetailList);
                    cveDialog.StartPosition = FormStartPosition.CenterParent;

                    cveDialog.ShowDialog();
                }
                else
                {
                    ShowMessageDialog("There are no CVE's on the selected item.", false);
                }
            }
            else
            {
                ShowMessageDialog("There is not an item selected.", false);
            }

        }

        private void btnLookupCVE_Click(object sender, EventArgs e)
        {
            string cve = tbCVE.Text;

            string cveJson = TaskLookupCVE.LookupCVE(cve);
            if(!string.IsNullOrEmpty(cveJson))
            {
                TextDialog textDialog = new TextDialog(cveJson);
                textDialog.StartPosition = FormStartPosition.CenterParent;
                textDialog.ShowDialog();
            }
            else
            {
                ShowMessageDialog("CVE Entered is not valid.  Check the value and try again.",false);
            }
        }

        private void btnExportCSV_Click(object sender, EventArgs e)
        {
            StringBuilder csvFile = new StringBuilder();

            foreach (CatalogProduct product in staticProductList)
            {
                foreach (CatalogSignature signature in product.SigList)
                {
                    csvFile.Append(signature.Name);
                    csvFile.Append(",");
                    csvFile.Append(signature.CVECount);
                    csvFile.Append(",");
                    csvFile.Append(product.SupportsInstall ? "Yes" : "");
                    csvFile.Append(",");
                    csvFile.AppendLine(signature.Platform);
                    
                }
            }

            File.WriteAllText("ProductSupport.csv", csvFile.ToString());
            MessageBox.Show("Results have been written to " + Path.Combine(Directory.GetCurrentDirectory(), "ProductSupport.csv"));
        }
    }
}