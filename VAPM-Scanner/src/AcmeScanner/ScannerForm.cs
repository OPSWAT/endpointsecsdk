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
            if (!File.Exists("license.cfg") || !File.Exists("pass_key.txt"))
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

            if (isOffline)
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

            if (isOffline)
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
                foreach (CatalogSignature signature in product.SigList)
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
            InstallCommand installCommand = (InstallCommand)e.Argument;
            ProductInstallResult installResult = TaskDownloadAndInstallApplication.InstallAndDownload(installCommand.signatureId,
                                                                                                      installCommand.freshInstall,
                                                                                                      installCommand.backgroundInstall,
                                                                                                      installCommand.validateInstall,
                                                                                                      installCommand.forceClose,
                                                                                                      installCommand.usePatchId);
            e.Result = installResult;
        }

        private void installVAPMPatchWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if(e.Result != null && e.Result is ProductInstallResult)
            {
                ProductInstallResult productInstallResult = (ProductInstallResult)e.Result;

                if (productInstallResult.success)
                {
                    if (productInstallResult.installResult != null && productInstallResult.installResult.code > 0)
                    {
                        if(productInstallResult.installResult.require_restart > 0)
                        {
                            ShowMessageDialog("Application installed, but requires restart to be fully patched.", false);
                        }
                        else
                        {
                            ShowMessageDialog("Successfully installed latest application with result: " + productInstallResult.installResult.code, false);
                        }
                    }
                    else
                    {
                        ShowMessageDialog("Successfully installed latest application.", false);
                    }
                }
                else
                {
                    if(productInstallResult.errorResult != null)
                    {
                        ShowMessageDialog("An error occured during install: \n\n" + productInstallResult.errorResult.description, false);
                    }
                    else
                    {
                        ShowMessageDialog("An error occured during install: \n" + productInstallResult.message, false);
                    }
                }
            }
            else
            {
                ShowMessageDialog("Unexpected result occurred installing the product",false);
            }



            // Do a scan again
            //lvScanResults.Items.Clear();
            //scanWorker.RunWorkerAsync(true);
            ShowLoading(false);
        }

        private void installOnlinePatchWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string kb = (string)e.Argument;
            OnlinePatchDetail patchDetail = staticOrchestrationScanResults[kb];

            TaskOrchestrateDownloadAndInstall.InstallAndDownload(patchDetail);
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

            if (scanResult.cveDetailList.Count > 0)
            {
                foreach (CVEDetail current in scanResult.cveDetailList)
                {
                    if (current.opswatSeverity > result)
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
            lvCatalog.Columns.Add("SigId", 80);
            lvCatalog.Columns.Add("CVE Count", 80);
            lvCatalog.Columns.Add("Installable", 80);
            lvCatalog.Columns.Add("Platform", 100);
            lvCatalog.Columns.Add("Fresh Install", 100);
            lvCatalog.Columns.Add("Package Count", 100);
            lvCatalog.Columns.Add("Install Version", 100);
            lvCatalog.Columns.Add("Background", 80);
            lvCatalog.Columns.Add("Validate", 80);



            lvCatalog.Columns.Add("", 400);
            lvCatalog.View = View.Details;
            lvCatalog.Update();

            int productCount = 0;
            int cveCount = 0;
            int installCount = 0;

            foreach (CatalogProduct product in staticProductList)
            {
                foreach (CatalogSignature signature in product.SigList)
                {
                    bool supportsPatch = product.SupportsInstall;
                    if (supportsPatch && signature.PatchAssociations.Count == 0)
                    {
                        supportsPatch = false;
                    }

                    bool freshInstall = signature.FreshInstall;
                    if (freshInstall && signature.PatchAssociations.Count == 0)
                    {
                        freshInstall = false;
                    }


                    ListViewItem lviCurrent = new ListViewItem();
                    lviCurrent.Text = signature.Name;
                    lviCurrent.SubItems.Add(signature.Id);
                    lviCurrent.SubItems.Add(signature.CVECount.ToString());
                    lviCurrent.SubItems.Add(supportsPatch ? "Yes" : "");
                    lviCurrent.SubItems.Add(signature.Platform);
                    lviCurrent.SubItems.Add(freshInstall ? "Yes" : "");


                    if (signature.PatchAssociations != null)
                    {
                        lviCurrent.SubItems.Add(signature.PatchAssociations.Count.ToString());
                    }
                    else
                    {
                        lviCurrent.SubItems.Add("");
                    }

                    if(supportsPatch)
                    {
                        lviCurrent.SubItems.Add(signature.PatchAssociations[0].PatchAggregation.LatestVersion);
                    }
                    else
                    {
                        lviCurrent.SubItems.Add("");
                    }

                    //
                    // Add the Background and Validate flags
                    //
                    lviCurrent.SubItems.Add(signature.BackgroundInstallSupport ? "Yes" : "");
                    lviCurrent.SubItems.Add(signature.ValidateInstallSupport ? "Yes" : "");



                    lviCurrent.Tag = signature.Id;
                    resultList.Add(lviCurrent);

                    productCount++;
                    cveCount += signature.CVECount;
                    if (supportsPatch)
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
            lvScanResults.Columns.Add("SigId", 50);
            lvScanResults.Columns.Add("PatchId", 50);
            lvScanResults.Columns.Add("Url", 50);

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
                lviCurrent.SubItems.Add(current.product.signatureId);

                if (current.installDetail.Count > 0)
                {
                    lviCurrent.SubItems.Add(current.installDetail[0].patch_id);
                    lviCurrent.SubItems.Add(current.installDetail[0].url);
                }

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


            foreach (OnlinePatchDetail current in sortedList)
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

                if (scanResult.cveDetailList.Count > 0)
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
                            InstallPatchMessageDialog installConfirmation = new InstallPatchMessageDialog("Are you sure you want to install \"" + scanResult.product.name + "\"", true);
                            if (ShowMessageDialog(installConfirmation))
                            {
                                ShowLoading(true);
                                
                                InstallCommand installCommand = new InstallCommand( signatureId, 
                                                                                    false, 
                                                                                    installConfirmation.IsBackgroundInstall(), 
                                                                                    installConfirmation.IsValidateInstaller(),
                                                                                    installConfirmation.IsForceClose(),
                                                                                    installConfirmation.UsePatchId());

                                installVAPMPatchWorker.RunWorkerAsync(installCommand);
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

        private void btnFreshInstall_Click(object sender, EventArgs e)
        {
            if (staticSignatureCatalogResults != null && staticSignatureCatalogResults.Count > 0)
            {
                if (lvCatalog.SelectedItems.Count > 0)
                {

                    string signatureId = lvCatalog.SelectedItems[0].Tag.ToString();
                    CatalogSignature sig = staticSignatureCatalogResults[signatureId];

                    if (sig.FreshInstall && sig.PatchAssociations.Count > 0)
                    {
                        InstallPatchMessageDialog installConfirmation = new InstallPatchMessageDialog("Are you sure you want to install \"" + sig.Name + "\"", true);
                        if (ShowMessageDialog(installConfirmation))
                        {
                            ShowLoading(true);
                            InstallCommand installCommand = new InstallCommand(signatureId,
                                                                                true,
                                                                                installConfirmation.IsBackgroundInstall(),
                                                                                installConfirmation.IsValidateInstaller(),
                                                                                installConfirmation.IsForceClose(),
                                                                                installConfirmation.UsePatchId());

                            installVAPMPatchWorker.RunWorkerAsync(installCommand);
                        }
                    }
                    else
                    {
                        ShowMessageDialog("Select an Application with the Fresh Install flag.", false);
                    }
                }
                else
                {
                    ShowMessageDialog("Select an Application to install.", false);
                }
            }
            else
            {
                ShowMessageDialog("Make sure to Scan for products.  After doing that select a product to install.", false);
            }
        }

        private void ForceWrite(StringBuilder sb, string filename)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            File.WriteAllText(filename, sb.ToString());
        }

        private void btnUrlCSV_Click(object sender, EventArgs e)
        {
            if(staticSignatureCatalogResults == null || staticSignatureCatalogResults.Count == 0)
            {
                ShowMessageDialog("Load the Catalog first to generate the URLs", false);
                return;
            }

            StringBuilder urlOutput = new StringBuilder();
            urlOutput.AppendLine("Application,URL,Architecture,Language");

            StringBuilder domainOutput = new StringBuilder();

            HashSet<string> domains = new HashSet<string>();
            foreach(CatalogSignature signature in staticSignatureCatalogResults.Values)
            {
                foreach(CatalogPatchAssociation association in signature.PatchAssociations)
                {
                    if(association.PatchAggregation != null && association.PatchAggregation.DownloadDetailsList != null)
                    {
                        foreach(CatalogDownloadDetails details in association.PatchAggregation.DownloadDetailsList)
                        {
                            urlOutput.Append(signature.Name);
                            urlOutput.Append(",");

                            urlOutput.Append(details.Link);
                            urlOutput.Append(",");

                            if (details.Architecture != null)
                                urlOutput.Append(details.Architecture);

                            urlOutput.Append(",");

                            if (details.Language != null)
                                urlOutput.Append(details.Language);

                            urlOutput.AppendLine();

                            Uri myUri = new Uri(details.Link);
                            string host = myUri.Host;

                            if (!domains.Contains(host))
                            {
                                domains.Add(host);
                                domainOutput.AppendLine(host);
                            }
                        }
                    }
                }
            }

            ForceWrite(urlOutput, "urls.csv");
            ForceWrite(domainOutput, "domains.csv");

            ShowMessageDialog("The files \"urls.csv\" and \"domains.csv\" have been created in the working directory.", false);
        }
    }
}