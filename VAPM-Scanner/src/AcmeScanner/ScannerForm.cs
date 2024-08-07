///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VAPMAdapater.Updates;
using VAPMAdapter.Catalog.POCO;
using VAPMAdapter.OESIS.POCO;
using VAPMAdapter.Tasks;
using VAPMAdapter.Updates;
using VAPMAdapter.Moby.POCO;
using VAPMAdapter.Moby;
using Newtonsoft.Json;
using System.Security.Cryptography.Xml;
using System.Globalization;
using System.Net.Http.Json;
using Newtonsoft.Json.Linq;
using VAPMAdapter.Catalog;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


namespace AcmeScanner
{
    public partial class ScannerForm : Form
    {
        static Dictionary<string, ProductScanResult> staticScanResults = new Dictionary<string, ProductScanResult>();
        static Dictionary<string, CatalogSignature> staticSignatureCatalogResults = new Dictionary<string, CatalogSignature>();
        static Dictionary<string, OnlinePatchDetail> staticOrchestrationScanResults = new Dictionary<string, OnlinePatchDetail>();
        private CVEDetailsManager cveDetailsManager = new CVEDetailsManager();
        static List<CatalogProduct> staticProductList = null;
        static List<MobyProduct> staticMobyProductList = null;
        static List<PatchStatus> staticPatchStatusList = null;
        static bool isCatalogUpdated = false;
        static List<string> sigIds;

        private System.ComponentModel.BackgroundWorker scanWorker;
        private System.ComponentModel.BackgroundWorker updateDBWorker;
        private System.ComponentModel.BackgroundWorker updateMobyWorker;
        private System.ComponentModel.BackgroundWorker installVAPMPatchWorker;
        private System.ComponentModel.BackgroundWorker installOnlinePatchWorker;
        private System.ComponentModel.BackgroundWorker loadCatalogWorker;
        private System.ComponentModel.BackgroundWorker loadMobyWorker;
        private System.ComponentModel.BackgroundWorker loadStatusWorker;
        private System.ComponentModel.BackgroundWorker loadVulnerabilitiesWorker;

        //first method called by the main class
        public ScannerForm()
        {
            //initializes UI componets
            InitializeComponent();
            //is used to perform async operations
            InitializeBackgroundWorker();
            CheckLicenseFiles();
            UpdateFilesOnStartup();
            FillSDKlabels();
            FillMobyLabels();
            SetTitleWithFileVersion();

        }

        //
        private void SetTitleWithFileVersion()
        {
            string exePath = Assembly.GetExecutingAssembly().Location;
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(exePath);
            string fileVersion = fileVersionInfo.FileVersion;
            this.Text = $"AcmeScanner - Version {fileVersion}";
        }

        private void FillMobyLabels()
        {
            mobyTimestampData.Text = UpdateMobyFile.GetMobyTimestamp();
        }

        private void FillSDKlabels()
        {
            EnableButtons(true);
            // Check if libwavmodapi.dll exists
            if (UpdateSDK.DoesSDKExist())
            {
                FileInfo vmodInfo = new FileInfo("libwavmodapi.dll");
                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(vmodInfo.FullName);
                string productVersion = versionInfo.ProductVersion;
                label5.Text = productVersion;
                label13.Text = productVersion;
                label7.Text = UpdateSDK.GetLatestSDKReleaseDate();
                label14.Text = label7.Text;
                if (!UpdateSDK.IsSDKUpdated())
                {
                    label6.ForeColor = System.Drawing.Color.Red;
                    label7.ForeColor = System.Drawing.Color.Red;
                    label11.ForeColor = System.Drawing.Color.Red;
                    label14.ForeColor = System.Drawing.Color.Red;
                }
            }
            else
            {
                btnUpdateSDK.Text = "Download SDK";
            }
            // Check if patch.dat exists
            if (UpdateDBFiles.DoesDBExist())
            {
                FileInfo dbFileInfo = new FileInfo("patch.dat");
                DateTime lastModifiedDB = dbFileInfo.LastWriteTime.Date;
                label9.Text = lastModifiedDB.ToString("MMMM dd, yyyy");
                label15.Text = label9.Text;

                if (!UpdateDBFiles.IsDBUpdated())
                {
                    label9.ForeColor = System.Drawing.Color.Red;
                    label8.ForeColor = System.Drawing.Color.Red;
                    label12.ForeColor = System.Drawing.Color.Red;
                    label15.ForeColor = System.Drawing.Color.Red;
                }
            }
            else
            {
                btnUpdate.Text = "Download DB";
            }
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
            if (!UpdateSDK.IsSDKUpdated())
            {
                btnUpdateSDK.UseAccentColor = true;

            }

            if (!UpdateDBFiles.IsDBUpdated())
            {
                btnUpdate.UseAccentColor = true;
            }
        }

        // Set up the BackgroundWorker object by
        // attaching event handlers.
        private void InitializeBackgroundWorker()
        {
            scanWorker = new BackgroundWorker();
            scanWorker.DoWork +=
            new DoWorkEventHandler(ScanWorker_DoWork);
            scanWorker.RunWorkerCompleted +=
            new RunWorkerCompletedEventHandler(
            ScanWorker_Completed);

            installVAPMPatchWorker = new BackgroundWorker();
            installVAPMPatchWorker.DoWork +=
                new DoWorkEventHandler(InstallVAPMPatchWorker_DoWork);
            installVAPMPatchWorker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(
            InstallVAPMPatchWorker_Completed);

            installOnlinePatchWorker = new BackgroundWorker();
            installOnlinePatchWorker.DoWork +=
                new DoWorkEventHandler(InstallOnlinePatchWorker_DoWork);
            installOnlinePatchWorker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(
            InstallOnlinePatchWorker_Completed);


            updateDBWorker = new BackgroundWorker();
            updateDBWorker.DoWork +=
                new DoWorkEventHandler(UpdateDBWorker_DoWork);
            updateDBWorker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(
            UpdateDBWorker_Completed);

            updateMobyWorker = new BackgroundWorker();
            updateMobyWorker.DoWork +=
                new DoWorkEventHandler(UpdateMobyWorker_DoWork);
            updateMobyWorker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(UpdateMobyWorker_Completed);


            loadCatalogWorker = new BackgroundWorker();
            loadCatalogWorker.DoWork +=
                new DoWorkEventHandler(LoadCatalogWorker_DoWork);
            loadCatalogWorker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(
            LoadCatalogWorker_Completed);

            loadStatusWorker = new BackgroundWorker();
            loadStatusWorker.DoWork +=
                new DoWorkEventHandler(LoadStatusWorker_DoWork);
            loadStatusWorker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(
            LoadStatusWorker_Completed);

            loadMobyWorker = new BackgroundWorker();
            loadMobyWorker.DoWork +=
                new DoWorkEventHandler(LoadMobyWorker_DoWork);
            loadMobyWorker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(LoadMobyWorker_Completed);

            loadVulnerabilitiesWorker = new BackgroundWorker();
            loadVulnerabilitiesWorker.DoWork +=
                new DoWorkEventHandler(loadVulnerabilitiesWorker_DoWork);
            loadVulnerabilitiesWorker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(loadVulnerabilitiesWorker_Completed);
        }

        private List<string> GetScanResults()
        {
            List<String> results = new List<string>();
            foreach (string item in staticScanResults.Keys)
            {
                results.Add(item);
            }
            return results;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///  Worker Threads
        /// </summary>
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void ScanWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            bool isOffline = (bool)e.Argument;

            if (isOffline)
            {
                // Scan Offline
                bool scanOSCVEs = cbScanOSCVEs.Checked;
                staticScanResults = TaskScanAll.Scan(scanOSCVEs);
                sigIds = GetScanResults();
            }
            else
            {
                // Scan Windows
                staticOrchestrationScanResults = TaskScanOrchestration.Scan();
            }

            e.Result = isOffline;
        }

        private void ScanWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
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

        private static bool IsJsonCatalogChanged()
        {
            string basePath = "catalog\\analog\\server\\";

            string productsPath = Path.Combine(basePath, "products.json");
            string cvesPath = Path.Combine(basePath, "cves.json");
            string patchAggregationPath = Path.Combine(basePath, "patch_aggregation.json");
            string patchAssociationsPath = Path.Combine(basePath, "patch_associations.json");
            string vulnAssociationsPath = Path.Combine(basePath, "vuln_associations.json");
            string binaryFilePath = Path.Combine("", "catalog.bin");

            DateTime productsLastModified = File.Exists(productsPath) ? new FileInfo(productsPath).LastWriteTime : DateTime.MinValue;
            DateTime cvesLastModified = File.Exists(cvesPath) ? new FileInfo(cvesPath).LastWriteTime : DateTime.MinValue;
            DateTime patchAggregationLastModified = File.Exists(patchAggregationPath) ? new FileInfo(patchAggregationPath).LastWriteTime : DateTime.MinValue;
            DateTime patchAssociationsLastModified = File.Exists(patchAssociationsPath) ? new FileInfo(patchAssociationsPath).LastWriteTime : DateTime.MinValue;
            DateTime vulnAssociationsLastModified = File.Exists(vulnAssociationsPath) ? new FileInfo(vulnAssociationsPath).LastWriteTime : DateTime.MinValue;
            DateTime binaryFileLastModified = File.Exists(binaryFilePath) ? new FileInfo(binaryFilePath).LastWriteTime : DateTime.MinValue;

            if (productsLastModified > binaryFileLastModified ||
            cvesLastModified > binaryFileLastModified ||
            patchAggregationLastModified > binaryFileLastModified ||
            patchAssociationsLastModified > binaryFileLastModified ||
            vulnAssociationsLastModified > binaryFileLastModified)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        private void LoadCatalogWorker_DoWork(object sender, DoWorkEventArgs e)
        {

            if (CatalogCache.CachedCatalog != null && !IsJsonCatalogChanged())
            {
                staticProductList = CatalogCache.CachedCatalog;
            }
            else
            {
                staticProductList = TaskLoadCatalog.Load();
            }
            staticSignatureCatalogResults.Clear();
            foreach (CatalogProduct product in staticProductList)
            {
                foreach (CatalogSignature signature in product.SigList)
                {
                    staticSignatureCatalogResults.Add(signature.Id, signature);
                }
            }


        }

        private void LoadCatalogWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (staticProductList != null)
            {
                UpdateCatalogResults();
            }
            CatalogCache.CachedCatalog = staticProductList;
            ShowLoading(false);
            searchCatalog.Enabled = true;
            UpdateScanResults();
        }



        private void LoadStatusWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            staticPatchStatusList = TaskLoadStatus.Load();
        }

        private void LoadStatusWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdatePatchStatusResults();
            ShowLoading(false);
        }

        private void LoadMobyWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            staticMobyProductList = TaskLoadMoby.Load();

            mobyCounts = TaskLoadMobyCounts.LoadCounts();
        }

        private void LoadMobyWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateMobyScanResults();
            ShowLoading(false);
        }

        private void InstallVAPMPatchWorker_DoWork(object sender, DoWorkEventArgs e)
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

        private void InstallVAPMPatchWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null && e.Result is ProductInstallResult)
            {
                ProductInstallResult productInstallResult = (ProductInstallResult)e.Result;

                if (productInstallResult.success)
                {
                    if (productInstallResult.installResult != null && productInstallResult.installResult.code > 0)
                    {
                        if (productInstallResult.installResult.require_restart > 0)
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
                    if (productInstallResult.errorResult != null)
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
                ShowMessageDialog("Unexpected result occurred installing the product", false);
            }

            ShowLoading(false);
        }

        private void InstallOnlinePatchWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string kb = (string)e.Argument;
            OnlinePatchDetail patchDetail = staticOrchestrationScanResults[kb];

            TaskOrchestrateDownloadAndInstall.InstallAndDownload(patchDetail);
        }

        private void InstallOnlinePatchWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            // Do a scan again
            lvOrchestrationScanResult.Items.Clear();
            scanWorker.RunWorkerAsync(false);
        }



        private void UpdateDBWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            bool sdkOnly = (bool)e.Argument;

            if (!sdkOnly)
            {
                UpdateDBFiles.DownloadFiles();
                btnUpdate.UseAccentColor = false;
                label8.ForeColor = System.Drawing.Color.Black;
                label9.ForeColor = System.Drawing.Color.Black;
                label12.ForeColor = System.Drawing.Color.Black;
                label15.ForeColor = System.Drawing.Color.Black;
            }

            else
            {
                UpdateSDK.DownloadAndInstall_OPSWAT_SDK();
                btnUpdateSDK.UseAccentColor = false;
                label6.ForeColor = System.Drawing.Color.Black;
                label7.ForeColor = System.Drawing.Color.Black;
                label11.ForeColor = System.Drawing.Color.Black;
                label14.ForeColor = System.Drawing.Color.Black;
            }
        }

        private void UpdateDBWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            ShowLoading(false);
            FillSDKlabels();//change sdk labels to current version
        }

        private void UpdateMobyWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            UpdateMobyFile.DownloadMoby();

        }

        private void UpdateMobyWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            btnUpdateMoby.UseAccentColor = false;
            btnUpdateMoby.Text = "Update Moby";
            ShowLoading(false);
        }
        private void loadVulnerabilitiesWorker_DoWork(Object sender, DoWorkEventArgs e)
        {
            e.Result = LoadVulnerabilities();
        }

        private void loadVulnerabilitiesWorker_Completed(Object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                var resultList = (List<ListViewItem>)e.Result;
                lvVulnerabilities.BeginInvoke(new Action(() =>
                {
                    lvVulnerabilities.Columns.Clear();
                    lvVulnerabilities.Columns.Add("Application Name", 200);
                    lvVulnerabilities.Columns.Add("Platform", 200);
                    lvVulnerabilities.Columns.Add("SigID", 200);
                    lvVulnerabilities.Columns.Add("Info", 300);
                    lvVulnerabilities.View = View.Details;
                    lvVulnerabilities.Items.AddRange(resultList.ToArray());
                    lvVulnerabilities.Update();

                    if (!VulnerabilitiesTab.Controls.Contains(lvVulnerabilities))
                    {
                        VulnerabilitiesTab.Controls.Add(lvVulnerabilities);
                    }
                    ShowLoading(false);
                }));
            }
            else
            {
                ShowLoading(false);
                MessageBox.Show("An error occurred: " + e.Error.Message);
            }
        }

        private static bool ShowMessageDialog(IScannerMessageDialog messageDialog)
        {
            bool result = false;

            messageDialog.SetStartPosition(FormStartPosition.CenterParent);
            messageDialog.ShowDialog();
            result = messageDialog.IsSuccess();

            return result;
        }


        private static bool ShowMessageDialog(string message, bool question)
        {
            CustomMessageDialog messageDialog = new CustomMessageDialog(message, question);
            return ShowMessageDialog(messageDialog);
        }

        public void EnableButtons(bool enabled)
        {
            bool SDKdownload = UpdateSDK.DoesSDKExist();
            bool DBdownload = UpdateDBFiles.DoesDBExist();
            bool MobyDownload = UpdateMobyFile.DoesMobyExist();

            //these buttons are still being loaded in and enabeled somewhere else, need to find out where
            if (!SDKdownload || !DBdownload)
            {
                btnInstall.Enabled = false;
                btnScan.Enabled = false;
                btnCVEJSON.Enabled = false;
                btnScanOrchestration.Enabled = false;
                btnInstallOrchestration.Enabled = false;
                btnInstall.Enabled = false;
                mbLoad.Enabled = false;
                btnListCatalogCVE.Enabled = false;
                btnLookupCVE.Enabled = false;
                btnExportCSV.Enabled = false;
                btnFreshInstall.Enabled = false;
                btnDomainCSV.Enabled = false;
            }
            else
            {
                btnInstall.Enabled = enabled;
                btnScan.Enabled = enabled;
                btnCVEJSON.Enabled = enabled;
                btnScanOrchestration.Enabled = enabled;
                btnInstallOrchestration.Enabled = enabled;
                btnInstall.Enabled = enabled;
                mbLoad.Enabled = enabled;
                btnListCatalogCVE.Enabled = enabled;
                btnLookupCVE.Enabled = enabled;
                btnExportCSV.Enabled = enabled;
                btnFreshInstall.Enabled = enabled;
                btnDomainCSV.Enabled = enabled;
                btnRefreshStatus.Enabled = enabled;

            }
            if (!MobyDownload)
            {
                btnUpdateMoby.UseAccentColor = true;
                btnUpdateMoby.Text = "Download Moby";
                btnLoadMoby.Enabled = false;
                btnMobyViewTotals.Enabled = false;
                btnRunChecksMoby.Enabled = false;
                btnViewMobySubsets.Enabled = false;
            }
            else
            {
                btnLoadMoby.Enabled = enabled;
                btnMobyViewTotals.Enabled = enabled;
                btnRunChecksMoby.Enabled = enabled;
                btnViewMobySubsets.Enabled = enabled;
            }
            if (staticProductList == null)
            {
                btnDomainCSV.Enabled = false;
                btnFreshInstall.Enabled = false;
                btnListCatalogCVE.Enabled = false;
                btnExportCSV.Enabled = false;
            }
            if (lvCatalog != null && lvCatalog.SelectedItems.Count == 0)
            {
                btnListCatalogCVE.Enabled = false;
                btnFreshInstall.Enabled = false;
            }
            btnUpdate.Enabled = enabled;
            btnUpdateSDK.Enabled = enabled;
            btnUpdateMoby.Enabled = enabled;
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


        private int GetSeverity(ProductScanResult scanResult)
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


        private async void UpdateCatalogResults()
        {
            // Initialize a thread-safe collection to hold ListViewItem objects
            ConcurrentBag<ListViewItem> concurrentResultList = new ConcurrentBag<ListViewItem>();

            // Setup the header
            lvCatalog.Columns.Clear();
            lvCatalog.Columns.Add("Application", 300);
            lvCatalog.Columns.Add("Installed", 80);
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
            lvCatalog.Tag = this;
            lvCatalog.View = View.Details;
            lvCatalog.Update();

            // Initialize counters
            int productCount = 0;
            int cveCount = 0;
            int installCount = 0;

            // Ensure sigIds is populated
            if (sigIds == null || sigIds.Count == 0)
            {
                staticScanResults = TaskScanAll.Scan(false);
                sigIds = GetScanResults();
            }

            await Task.Run(() =>
            {
                // Parallel processing for staticProductList
                Parallel.ForEach(staticProductList, product =>
                {
                    bool productSupportsInstall = product.SupportsInstall;
                    foreach (var signature in product.SigList)
                    {
                        bool supportsPatch = productSupportsInstall && signature.PatchAssociations.Count > 0;
                        bool freshInstall = signature.FreshInstall && signature.PatchAssociations.Count > 0;

                        // Create and populate ListViewItem
                        ListViewItem lviCurrent = new ListViewItem
                        {
                            Text = signature.Name,
                            Tag = signature.Id
                        };
                        lviCurrent.SubItems.Add(sigIds.Contains(signature.Id) ? "Yes" : "No");
                        lviCurrent.SubItems.Add(signature.Id);
                        lviCurrent.SubItems.Add(signature.CVECount.ToString());
                        lviCurrent.SubItems.Add(supportsPatch ? "Yes" : "");
                        lviCurrent.SubItems.Add(signature.Platform);
                        lviCurrent.SubItems.Add(freshInstall ? "Yes" : "");
                        lviCurrent.SubItems.Add(signature.PatchAssociations?.Count.ToString() ?? "");
                        lviCurrent.SubItems.Add(supportsPatch ? signature.PatchAssociations[0].PatchAggregation.LatestVersion : "");
                        lviCurrent.SubItems.Add(signature.BackgroundInstallSupport ? "Yes" : "");
                        lviCurrent.SubItems.Add(signature.ValidateInstallSupport ? "Yes" : "");

                        // Add ListViewItem to the thread-safe collection
                        concurrentResultList.Add(lviCurrent);

                        // Increment counters in a thread-safe manner
                        Interlocked.Increment(ref productCount);
                        Interlocked.Add(ref cveCount, signature.CVECount);
                        if (supportsPatch)
                        {
                            Interlocked.Increment(ref installCount);
                        }
                    }
                });
            });

            // Update UI with counters on the main thread
            Invoke(new Action(() =>
            {
                lblTotalCVEs.Text = cveCount.ToString();
                lblTotalProducts.Text = productCount.ToString();
                lblTotalInstalls.Text = installCount.ToString();

                // Update ListView control with items
                lvCatalog.BeginUpdate();
                lvCatalog.Items.Clear();
                lvCatalog.Items.AddRange(concurrentResultList.ToArray());
                lvCatalog.EndUpdate();
                isCatalogUpdated = true;
                UpdateScanResults();
            }));
        }




        private void UpdatePatchStatusResults()
        {
            List<ListViewItem> resultList = new List<ListViewItem>();

            //
            // Setup the header
            //

            lvStatus.Columns.Clear();
            lvStatus.Columns.Add("Status", 80);
            lvStatus.Columns.Add("Product", 150);
            lvStatus.Columns.Add("Signature", 150);
            lvStatus.Columns.Add("Platform", 80);
            lvStatus.Columns.Add("ProductId", 80);
            lvStatus.Columns.Add("SignatureId", 80);
            lvStatus.Columns.Add("LastGood", 80);
            lvStatus.Columns.Add("LastTested", 80);

            lvStatus.Columns.Add("", 400);
            lvStatus.View = View.Details;
            lvStatus.Update();

            foreach (PatchStatus patch in staticPatchStatusList)
            {
                ListViewItem lviCurrent = new ListViewItem();
                lviCurrent.Text = patch.status;
                lviCurrent.SubItems.Add(patch.productName);
                lviCurrent.SubItems.Add(patch.signatureName);
                lviCurrent.SubItems.Add(patch.platform);
                lviCurrent.SubItems.Add(patch.productId.ToString());
                lviCurrent.SubItems.Add(patch.signatureId.ToString());
                lviCurrent.SubItems.Add(patch.lastKnownGood);
                lviCurrent.SubItems.Add(patch.lastTested);
                resultList.Add(lviCurrent);
            }

            lvStatus.Items.Clear();
            lvStatus.Items.AddRange(resultList.ToArray());
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
                int OPSWATseverity = GetSeverity(current);
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

        private void UpdateMobyScanResults()
        {
            List<ListViewItem> resultList = new List<ListViewItem>();

            // Setup the header

            scannerListView1.Columns.Clear();
            scannerListView1.Columns.Add("Name", 200);
            scannerListView1.Columns.Add("Signature ID", 100);
            scannerListView1.Columns.Add("OS Type", 100);
            scannerListView1.Columns.Add("Supports Auto Patching", 200);
            scannerListView1.Columns.Add("Validation Supported", 200);
            scannerListView1.Columns.Add("Supports App Remover", 200);
            scannerListView1.Columns.Add("Supports Background Patching", 220);
            scannerListView1.View = View.Details;
            scannerListView1.Update();

            foreach (MobyProduct product in staticMobyProductList)
            {
                foreach (MobySignature signature in product.sigList)
                {
                    ListViewItem lviCurrent = new ListViewItem();
                    lviCurrent.Text = signature.Name;
                    lviCurrent.SubItems.Add(signature.Id);
                    lviCurrent.SubItems.Add(product.osType);
                    lviCurrent.SubItems.Add(signature.supportAutoPatching.ToString());
                    lviCurrent.SubItems.Add(signature.validationSupported.ToString());
                    lviCurrent.SubItems.Add(signature.supportAppRemover.ToString());
                    lviCurrent.SubItems.Add(signature.backgroundPatchingSupported.ToString());

                    // Set Tag to store signature Id
                    lviCurrent.Tag = product.Id;

                    // Add ListViewItem to the resultList
                    resultList.Add(lviCurrent);
                }
            }



            scannerListView1.Items.Clear();
            scannerListView1.Items.AddRange(resultList.ToArray());
        }

        private ListViewItem CreateCountListViewItem(string name, MobyPlatformCounts counts)
        {
            ListViewItem lviCounts = new ListViewItem();
            lviCounts.Text = name;
            lviCounts.SubItems.Add(counts.Total.ToString());
            lviCounts.SubItems.Add(counts.Windows.ToString());
            lviCounts.SubItems.Add(counts.Mac.ToString());
            lviCounts.SubItems.Add(counts.Linux.ToString());
            return lviCounts;
        }

        private void BtnLoadMoby_Click(object sender, EventArgs e)
        {
            ShowLoading(true);
            loadMobyWorker.RunWorkerAsync();
        }

        private void BtnScan_Click(object sender, EventArgs e)
        {
            ShowLoading(true);
            scanWorker.RunWorkerAsync(true);
        }


        private string GetMobyTotalCountsJson()
        {
            if (mobyCounts == null)
            {
                mobyCounts = TaskLoadMobyCounts.LoadCounts();
            }

            var totalCounts = new
            {
                TotalProducts = mobyCounts.TotalProductsCount,
                TotalSignatures = mobyCounts.TotalSignaturesCount,
                CveDetection = mobyCounts.CveDetection,
                SupportAutoPatching = mobyCounts.SupportAutoPatching,
                BackgroundPatching = mobyCounts.BackgroundPatching,
                FreshInstallable = mobyCounts.FreshInstallable,
                ValidationSupported = mobyCounts.ValidationSupported,
                AppRemover = mobyCounts.AppRemover
            };

            return JsonConvert.SerializeObject(totalCounts, Formatting.Indented);
        }


        private void BtnMobyViewTotals_Click(object sender, EventArgs e)
        {
            string totalCountsJson = GetMobyTotalCountsJson();
            TextDialog textDialog = new TextDialog(totalCountsJson);
            textDialog.StartPosition = FormStartPosition.CenterParent;
            textDialog.ShowDialog();
        }

        private void BtnCVEJSON_Click(object sender, EventArgs e)
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


        private void BtnInstall_Click(object sender, EventArgs e)
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

                                InstallCommand installCommand = new InstallCommand(signatureId,
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

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            ShowLoading(true);
            updateDBWorker.RunWorkerAsync(false);
        }

        private void BtnUpdateSDK_Click(object sender, EventArgs e)
        {
            ShowLoading(true);
            updateDBWorker.RunWorkerAsync(true);
        }

        private void BtnScanOrchestration_Click(object sender, EventArgs e)
        {
            ShowLoading(true);
            scanWorker.RunWorkerAsync(false); // This will not scan offline
        }

        private void BtnInstallOrchestration_Click(object sender, EventArgs e)
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

        private void MbLoad_Click(object sender, EventArgs e)
        {
            ShowLoading(true);
            loadCatalogWorker.RunWorkerAsync();
        }

        private void BtnListCatalogCVE_Click(object sender, EventArgs e)
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

        private void BtnLookupCVE_Click(object sender, EventArgs e)
        {
            LookupCVEBox cb = new LookupCVEBox();
            cb.ShowDialog();
        }

        private void BtnExportCSV_Click(object sender, EventArgs e)
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

        private void BtnFreshInstall_Click(object sender, EventArgs e)
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

        private static void ForceWrite(StringBuilder sb, string filename)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            File.WriteAllText(filename, sb.ToString());
        }

        private void BtnUrlCSV_Click(object sender, EventArgs e)
        {
            if (staticSignatureCatalogResults == null || staticSignatureCatalogResults.Count == 0)
            {
                ShowMessageDialog("Load the Catalog first to generate the URLs", false);
                return;
            }

            StringBuilder urlOutput = new StringBuilder();
            urlOutput.AppendLine("Application,URL,Architecture,Language");

            StringBuilder domainOutput = new StringBuilder();

            HashSet<string> domains = new HashSet<string>();
            foreach (CatalogSignature signature in staticSignatureCatalogResults.Values)
            {
                foreach (CatalogPatchAssociation association in signature.PatchAssociations)
                {
                    if (association.PatchAggregation != null && association.PatchAggregation.DownloadDetailsList != null)
                    {
                        foreach (CatalogDownloadDetails details in association.PatchAggregation.DownloadDetailsList)
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

        private void BtnRefreshStatus_Click(object sender, EventArgs e)
        {
            ShowLoading(true);
            loadStatusWorker.RunWorkerAsync();
        }

        private void LvScanResults_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        public void BtnOrchestrationView_Click()
        {
            if (lvOrchestrationScanResult.SelectedItems.Count > 0)
            {

                StringBuilder kbIdBuilder = new StringBuilder();
                kbIdBuilder.AppendLine("Title:\t\t" + lvOrchestrationScanResult.SelectedItems[0].SubItems[0].Text);
                kbIdBuilder.AppendLine("Severity:\t" + lvOrchestrationScanResult.SelectedItems[0].SubItems[1].Text);
                kbIdBuilder.AppendLine("Product:\t" + lvOrchestrationScanResult.SelectedItems[0].SubItems[2].Text);
                kbIdBuilder.AppendLine("KB:\t\t" + lvOrchestrationScanResult.SelectedItems[0].SubItems[3].Text);
                kbIdBuilder.AppendLine("Patched:\t" + lvOrchestrationScanResult.SelectedItems[0].SubItems[4].Text);
                kbIdBuilder.AppendLine("Description:\t" + lvOrchestrationScanResult.SelectedItems[0].SubItems[5].Text);

                string view_full = kbIdBuilder.ToString();
                TextDialog textDialog = new TextDialog(view_full);
                textDialog.StartPosition = FormStartPosition.CenterParent;
                textDialog.ShowDialog();
            }
            else
            {
                ShowMessageDialog("Select an item to view!!", false);
            }

        }




        private void BtnViewJson_Click(object sender, EventArgs e)
        {

            if (scannerListView1.SelectedItems.Count > 0)
            {
                string sigID = scannerListView1.SelectedItems[0].SubItems[1].Text;
                string pID = scannerListView1.SelectedItems[0].Tag.ToString();
                MobyProduct selectedProduct = staticMobyProductList.FirstOrDefault(product => product.Id == pID);
                MobySignature selectedSignature = selectedProduct.sigList.FirstOrDefault(signature => signature.Id == sigID);
                string json = JsonConvert.SerializeObject(selectedSignature, Formatting.Indented);




                ViewMobyJsonDialog textDialog = new ViewMobyJsonDialog(json);
                textDialog.StartPosition = FormStartPosition.CenterParent;
                textDialog.ShowDialog();
            }
            else
            {
                ShowMessageDialog("Select an item to view JSON!!", false);
            }
        }

        private void BtnUpdateMoby_Click(object sender, EventArgs e)
        {
            ShowLoading(true);
            updateMobyWorker.RunWorkerAsync(true);
        }

        private void BtnRunChecksMoby_Click(object sender, EventArgs e)
        {
            MobySanityCheckDialog sanityCheckDialog = new MobySanityCheckDialog();
            sanityCheckDialog.StartPosition = FormStartPosition.CenterParent;
            sanityCheckDialog.ShowDialog();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("called button funciton");

        }

        public static string ProductInfoForSignatureId(string sigId)
        {
            int i = 0;
            int foundId = 0;
            bool found = false;
            foreach (CatalogProduct prod in staticProductList)
            {
                foundId = 0;
                foreach (CatalogSignature sig in prod.SigList)
                {
                    if (sig.Id == sigId)
                    {
                        found = true;
                        break;
                    }
                    foundId += 1;
                }
                if (found) { break; }
                i += 1;
            }

            CatalogProduct finalProduct = staticProductList[i];
            CatalogSignature finalSignature = finalProduct.SigList[foundId];
            var productWithSingleSignature = new
            {
                finalProduct.Name,
                finalProduct.Vendor,
                finalProduct.Id,
                finalProduct.SupportsInstall,
                finalProduct.OsType,
                finalProduct.CveCount,
                SelectedSignature = finalSignature
            };

            string json = JsonConvert.SerializeObject(productWithSingleSignature, Formatting.Indented);
            return json;

        }

        //need to rework this panel
        private void LoadMobySubsets()
        {
            //need to hide the other panels when this panel opens up, so you can only see this one, then reopen them when this panel is closed
            panel1.Visible = false;
            panel2.Visible = false;
            panel3.Visible = false;
            panel4.Visible = false;
            panel5.Visible = false;
            panel6.Visible = false;
            panel7.Visible = false;


            // Get the dictionary of JSON file names and timestamps
            Dictionary<string, string> mobyFileTimestamps = DownloadMobySubsets.GetMobyFileTimestamps();

            // Clear previous controls from the ListView
            System.Windows.Forms.ListView listView = mobySubsetsPanel.Controls.OfType<System.Windows.Forms.ListView>().FirstOrDefault();

            if (listView != null)
            {
                listView.Items.Clear();
                listView.Columns.Clear();

                listView.Columns.Add("JSON File Name", 200);
                listView.Columns.Add("Timestamp (GMT 0 Time)", 200);

                // Add items to the ListView
                foreach (var entry in mobyFileTimestamps)
                {
                    ListViewItem listViewItem = new ListViewItem(entry.Key);
                    listViewItem.SubItems.Add(entry.Value);
                    listView.Items.Add(listViewItem);
                }

                // Show the panel
                mobySubsetsPanel.Visible = true;
            }
        }

        private async void BtnViewMobySubsets_Click(object sender, EventArgs e)
        {
            await DownloadMobySubsets.DownloadMobyFilesAsync();

            LoadMobySubsets();
        }

        private void BtnMobysubsetClose_Click(object sender, EventArgs e)
        {
            mobySubsetsPanel.Visible = false;  // Hide the panel
            panel1.Visible = true;
            panel2.Visible = true;
            panel3.Visible = true;
            panel4.Visible = true;
            panel5.Visible = true;
            panel6.Visible = true;
            panel7.Visible = true;

        }

        private void ListView_ItemActivateMobySubsetTable(object sender, EventArgs e)
        {
            System.Windows.Forms.ListView listView = sender as System.Windows.Forms.ListView;

            if (listView != null && listView.SelectedItems.Count > 0)
            {
                string fileName = listView.SelectedItems[0].Text;
                ShowJsonContentMobySubset(fileName);
            }
        }

        // Method to show JSON content in a text dialog
        private void ShowJsonContentMobySubset(string filename)
        {
            string jsonContent = DownloadMobySubsets.GetJsonContent(filename);
            ViewMobyJsonDialog textDialog = new ViewMobyJsonDialog(jsonContent);
            textDialog.StartPosition = FormStartPosition.CenterParent;
            textDialog.ShowDialog();
        }

        private List<ListViewItem> LoadVulnerabilities()
        {
            int cveCount = 0;
            string JsonFilePath = Directory.GetCurrentDirectory();
            string JsonName = "T1Applications.json";
            string FilePath = Path.Combine(JsonFilePath, JsonName);

            List<ListViewItem> resultList = new List<ListViewItem>();
            Dictionary<string, string> productDictionary = new Dictionary<string, string>();

            // Read and parse the JSON file
            var jsonContent = File.ReadAllText(FilePath);
            var productList = JArray.Parse(jsonContent);

            foreach (var product in productList)
            {
                string productName = product["name"]?.ToString();
                string productID = product["productID"]?.ToString();

                if (string.IsNullOrEmpty(productName) || string.IsNullOrEmpty(productID))
                    continue;

                // Populate the dictionary with productID as the key and productName as the value
                productDictionary[productID] = productName;
            }

            // Pass the dictionary to MapPatchData to get vulnerabilities
            var vulnerabilities = TaskGetProductVulnerabilities.MapPatchData(productDictionary);

            foreach (var kvp in vulnerabilities)
            {
                string productID = kvp.Key;
                string productVulJson = kvp.Value;
                string productName = productDictionary[productID];

                string platform = "Windows"; // Default platform
                if (productName.Contains("macOS", StringComparison.OrdinalIgnoreCase))
                {
                    platform = "Mac";
                }
                else if (productName.Contains("Linux", StringComparison.OrdinalIgnoreCase))
                {
                    platform = "Linux";
                }

                if (productVulJson == "Install app to see data")
                {
                    ListViewItem item = new ListViewItem(productName);
                    item.SubItems.Add(platform); // Add platform information
                    item.SubItems.Add(productID);
                    item.SubItems.Add(productVulJson); // Use the error message directly

                    // Optionally store an empty JSON or a message in the Tag property
                    item.Tag = productVulJson;

                    resultList.Add(item);
                }
                else
                {
                    try
                    {
                        // Try to parse the JSON
                        JObject cveJson = JObject.Parse(productVulJson);
                        cveCount += cveJson["result"]["cves"].Count();

                        ListViewItem item = new ListViewItem(productName);
                        item.SubItems.Add(platform);
                        item.SubItems.Add(productID);
                        item.SubItems.Add("Double click to view CVEs and resolutions");

                        // Store the JSON content in the Tag property for easy access later
                        item.Tag = productVulJson;

                        resultList.Add(item);
                    }
                    catch (JsonReaderException ex)
                    {
                        // Handle JSON parsing error
                        Console.WriteLine($"Error parsing JSON for productID {productID}: {ex.Message}");
                        // Optionally add an item with an error message
                        ListViewItem item = new ListViewItem(productName);
                        item.SubItems.Add(platform); // Add platform information
                        item.SubItems.Add(productID);
                        item.SubItems.Add("Error parsing product vulnerabilities");

                        // Optionally store the error message or invalid JSON in the Tag property
                        item.Tag = productVulJson;

                        resultList.Add(item);
                    }
                }
            }

            if (InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate
                {
                    label17.Text = cveCount.ToString();
                }));
            }

            return resultList;
        }


        private void LvVulnerabilities_DoubleClick(object sender, EventArgs e)
        {
            if (lvVulnerabilities.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = lvVulnerabilities.SelectedItems[0];
                string jsonContent = selectedItem.Tag as string;

                if (!string.IsNullOrEmpty(jsonContent))
                {
                    try
                    {
                        // Parse and format the JSON string
                        var parsedJson = JToken.Parse(jsonContent);
                        string formattedJson = parsedJson.ToString(Formatting.Indented);

                        // Pass the formatted JSON string to the dialog
                        ViewMobyJsonDialog textDialog = new ViewMobyJsonDialog(formattedJson);
                        textDialog.StartPosition = FormStartPosition.CenterParent;
                        textDialog.ShowDialog();
                    }
                    catch
                    {
                        // Handle JSON parsing error
                        MessageBox.Show($"Error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnLoadCVEs_Click(object sender, EventArgs e)
        {
            ShowLoading(true);
            loadVulnerabilitiesWorker.RunWorkerAsync();
        }
        private void searchCatalogClicked(object sender, EventArgs e)
        {
            if (staticProductList == null)
            {
                searchCatalog.Enabled = false;
                return;
            }
            if (searchCatalog.Text == "Search Products")
            {
                searchCatalog.Text = "";
            }

        }

        private void searchCatalog_TextChanged(object sender, EventArgs e)
        {

            if (!isCatalogUpdated)
            {
                UpdateCatalogResults();
            }

        }
        private List<CatalogSignature> searchResult(string signatureName)
        {
            List<CatalogSignature> result = new List<CatalogSignature>();

            foreach (CatalogProduct prod in staticProductList)
            {

                foreach (CatalogSignature sig in prod.SigList)
                {
                    if (sig.Name.ToLower().Contains(signatureName.ToLower()))
                    {
                        result.Add(sig);

                    }

                }
            }
            return result;
        }

        private void UpdateSearchCatalogResults(List<CatalogSignature> resultList)
        {
            List<ListViewItem> resultListCatalog = new List<ListViewItem>();
            int intIndex = 0;
            bool found = false;
            while (intIndex < lvCatalog.Items.Count)
            {

                string line = lvCatalog.Items[intIndex].SubItems[2].Text;
                foreach (CatalogSignature sig in resultList)
                {
                    if (line == sig.Id)
                    {
                        found = true;
                        break;
                    }
                }
                if (found)
                {
                    resultListCatalog.Add(lvCatalog.Items[intIndex]);
                }
                intIndex++;
                found = false;

            }
            lvCatalog.BeginUpdate();
            lvCatalog.Items.Clear();
            lvCatalog.Items.AddRange(resultListCatalog.ToArray());
            lvCatalog.EndUpdate();
            isCatalogUpdated = false;
        }

        private void searchCatalogEnter(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                List<CatalogSignature> resultList = searchResult(searchCatalog.Text);
                UpdateSearchCatalogResults(resultList);

            }
        }
    }
}