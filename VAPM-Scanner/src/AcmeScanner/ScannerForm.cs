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


namespace AcmeScanner
{
    public partial class ScannerForm : Form
    {
        static Dictionary<string, ProductScanResult> staticScanResults = new Dictionary<string, ProductScanResult>();
        static Dictionary<string, CatalogSignature> staticSignatureCatalogResults = new Dictionary<string, CatalogSignature>();
        static Dictionary<string, OnlinePatchDetail> staticOrchestrationScanResults = new Dictionary<string, OnlinePatchDetail>();
        static List<CatalogProduct> staticProductList = null;
        static List<MobyProduct> staticMobyProductList = null;
        static List<PatchStatus> staticPatchStatusList = null;
        static List<string> sigIds;

        private System.ComponentModel.BackgroundWorker scanWorker;
        private System.ComponentModel.BackgroundWorker updateDBWorker;
        private System.ComponentModel.BackgroundWorker updateMobyWorker;
        private System.ComponentModel.BackgroundWorker installVAPMPatchWorker;
        private System.ComponentModel.BackgroundWorker installOnlinePatchWorker;
        private System.ComponentModel.BackgroundWorker loadCatalogWorker;
        private System.ComponentModel.BackgroundWorker loadMobyWorker;
        private System.ComponentModel.BackgroundWorker loadStatusWorker;

        //first method called by the main class
        public ScannerForm()
        {
            //initializes UI componets
            InitializeComponent();
            //is used to perform async operations
            InitializeBackgroundWorker();
            CheckLicenseFiles();
            UpdateFilesOnStartup();
            fillSDKlabels();
        }

        private void fillSDKlabels()
        {
            EnableButtons(true);
            // Check if libwavmodapi.dll exists
            if (UpdateSDK.doesSDKExist())
            {
                FileInfo vmodInfo = new FileInfo("libwavmodapi.dll");
                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(vmodInfo.FullName);
                string productVersion = versionInfo.ProductVersion;
                label5.Text = productVersion;
                label13.Text = productVersion;
                DateTime lastModified = vmodInfo.LastWriteTime.Date;
                label7.Text = lastModified.ToString("MMMM dd, yyyy");
                label14.Text = label7.Text;
                if (!UpdateSDK.isSDKUpdated())
                {
                    label6.ForeColor = System.Drawing.Color.Red;
                    label7.ForeColor = System.Drawing.Color.Red;
                    label11.ForeColor = System.Drawing.Color.Red;
                    label14.ForeColor = System.Drawing.Color.Red;
                }
            }
            // Check if patch.dat exists
            if (UpdateDBFiles.doesDBExist())
            {
                FileInfo dbFileInfo = new FileInfo("patch.dat");
                DateTime lastModifiedDB = dbFileInfo.LastWriteTime.Date;
                label9.Text = lastModifiedDB.ToString("MMMM dd, yyyy");
                label15.Text = label9.Text;

                if (!UpdateDBFiles.isDBUpdated())
                {
                    label9.ForeColor = System.Drawing.Color.Red;
                    label8.ForeColor = System.Drawing.Color.Red;
                    label12.ForeColor = System.Drawing.Color.Red;
                    label15.ForeColor = System.Drawing.Color.Red;
                }
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
            if (!UpdateSDK.isSDKUpdated())
            {
                btnUpdateSDK.UseAccentColor = true;

            }

            if (!UpdateDBFiles.isDBUpdated())
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

            updateMobyWorker = new BackgroundWorker();
            updateMobyWorker.DoWork +=
                new DoWorkEventHandler(updateMobyWorker_DoWork);
            updateMobyWorker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(updateMobyWorker_Completed);


            loadCatalogWorker = new BackgroundWorker();
            loadCatalogWorker.DoWork +=
                new DoWorkEventHandler(loadCatalogWorker_DoWork);
            loadCatalogWorker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(
            loadCatalogWorker_Completed);

            loadStatusWorker = new BackgroundWorker();
            loadStatusWorker.DoWork +=
                new DoWorkEventHandler(loadStatusWorker_DoWork);
            loadStatusWorker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(
            loadStatusWorker_Completed);

            loadMobyWorker = new BackgroundWorker();
            loadMobyWorker.DoWork +=
                new DoWorkEventHandler(loadMobyWorker_DoWork);
            loadMobyWorker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(loadMobyWorker_Completed);
        }

        private List<string> getScanResults()
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
        private void scanWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            bool isOffline = (bool)e.Argument;

            if (isOffline)
            {
                // Scan Offline
                bool scanOSCVEs = cbScanOSCVEs.Checked;
                staticScanResults = TaskScanAll.Scan(scanOSCVEs);
                sigIds = getScanResults();
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

        private bool isJsonCatalogChanged()
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


        private void loadCatalogWorker_DoWork(object sender, DoWorkEventArgs e)
        {

            if (CatalogCache.CachedCatalog != null && !isJsonCatalogChanged())
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

        private void loadCatalogWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (staticProductList != null)
            {
                UpdateCatalogResults();
            }
            CatalogCache.CachedCatalog = staticProductList;
            if (CatalogCache.CachedCatalog == null) { Debug.WriteLine("after loading cataog also null"); }
            ShowLoading(false);
            UpdateScanResults();
        }



        private void loadStatusWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            staticPatchStatusList = TaskLoadStatus.Load();
        }

        private void loadStatusWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdatePatchStatusResults();
            ShowLoading(false);
        }

        private void loadMobyWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            staticMobyProductList = TaskLoadMoby.Load();

            mobyCounts = TaskLoadMobyCounts.LoadCounts();
        }

        private void loadMobyWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateMobyScanResults();
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

        private void updateDBWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            ShowLoading(false);
            fillSDKlabels();//change sdk labels to current version
        }

        private void updateMobyWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            UpdateMobyFile.DownloadMoby();
        }

        private void updateMobyWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
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
            bool SDKdownload = UpdateSDK.doesSDKExist();
            bool DBdownload = UpdateDBFiles.doesDBExist();
            bool MobyDownload = UpdateMobyFile.doesMobyExist();

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
                btnRefreshStatus.Enabled = false;
                btnOrchestrationView.Enabled = false;
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
                btnOrchestrationView.Enabled = enabled;
            }
            if (!MobyDownload)
            {
                btnLoadMoby.Enabled = false;
                btnViewJson.Enabled = false;
                btnMobyViewTotals.Enabled = false;
            }
            else
            {
                btnLoadMoby.Enabled = enabled;
                btnViewJson.Enabled = enabled;
                btnMobyViewTotals.Enabled = enabled;
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
                sigIds = getScanResults();
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

        private void btnLoadMoby_Click(object sender, EventArgs e)
        {
            ShowLoading(true);
            loadMobyWorker.RunWorkerAsync();
        }

        private void btnScan_Click(object sender, EventArgs e)
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


        private void btnMobyViewTotals_Click(object sender, EventArgs e)
        {
            string totalCountsJson = GetMobyTotalCountsJson();
            TextDialog textDialog = new TextDialog(totalCountsJson);
            textDialog.StartPosition = FormStartPosition.CenterParent;
            textDialog.ShowDialog();
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
            LookupCVEBox cb = new LookupCVEBox();
            cb.ShowDialog();
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

        private void btnRefreshStatus_Click(object sender, EventArgs e)
        {
            ShowLoading(true);
            loadStatusWorker.RunWorkerAsync();
        }

        private void lvScanResults_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnOrchestrationView_Click(object sender, EventArgs e)
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




        private void btnViewJson_Click(object sender, EventArgs e)
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

        private void scannerListView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }


        private void btnUpdateMoby_Click(object sender, EventArgs e)
        {
            ShowLoading(true);
            updateMobyWorker.RunWorkerAsync(true);
        }

        private void btnRunChecksMoby_Click(object sender, EventArgs e)
        {
            MobySanityCheckDialog sanityCheckDialog = new MobySanityCheckDialog();
            sanityCheckDialog.StartPosition = FormStartPosition.CenterParent;
            sanityCheckDialog.ShowDialog();
        }
    }
}