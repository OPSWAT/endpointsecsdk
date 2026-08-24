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
using System.Drawing;
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
using Newtonsoft.Json;
using System.Globalization;
using AcmeScanner.Dialogs;
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
        static List<DriverFirmwareStatus> staticDriverResults = new List<DriverFirmwareStatus>();
        static List<CatalogProduct> staticProductList = null;
        static bool isCatalogUpdated = false;

        private System.ComponentModel.BackgroundWorker scanWorker;
        private System.ComponentModel.BackgroundWorker updateDBWorker;
        private System.ComponentModel.BackgroundWorker installVAPMPatchWorker;
        private System.ComponentModel.BackgroundWorker installOnlinePatchWorker;
        private System.ComponentModel.BackgroundWorker loadCatalogWorker;

        //first method called by the main class
        public ScannerForm(string[] args)
        {
            //initializes UI componets
            InitializeComponent();
            // The BIOS & Drivers list view routes its double-click back to this form via its
            // Tag (same pattern as lvCatalog); the designer defines the control, we set the owner.
            lvBiosDrivers.Tag = this;
            //is used to perform async operations
            InitializeBackgroundWorker();
            if (CheckLicenseFiles())
            {
                UpdateFilesOnStartup();
                FillSDKlabels();
                SetTitleWithFileVersion();
                SetTabs(args);
            }
        }



        private void SetTabs(string[] args)
        {
            tbcMainView.TabPages.Clear();
            tbcMainView.TabPages.Add(tabOffline);
            tbcMainView.TabPages.Add(tabOrchestrate);
            tbcMainView.TabPages.Add(tabCatalog);
            tbcMainView.TabPages.Add(tabBiosDrivers);
        }

        private void SetTitleWithFileVersion()
        {
            string exePath = Assembly.GetExecutingAssembly().Location;
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(exePath);
            string fileVersion = fileVersionInfo.FileVersion;
            this.Text = $"AcmeScanner - Version {fileVersion}";
        }


        //This function fills in the SDK labels present on the Offline and Patches tab.
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

            // BIOS & Drivers tab: show the driver/firmware DB version (modified time), size, and
            // whether the catalog is staging or production. Refreshes on startup and after updates.
            lblBiosDriversSummary.Text = GetDriverFirmwareDbStatus();
        }

        //Check if license files are present in bin folder; does not allow to run program if not
        private bool CheckLicenseFiles()
        {
            bool result = false;
            if (!File.Exists("license.cfg") || !File.Exists("pass_key.txt"))
            {
                ShowMessageDialog("This program requires the license.cfg and pass_key.txt to be in the running directory.  Please check and make sure this is correct.", false);
                Close();
            }
            else
            {
                result = true;
            }

            return result;
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

            loadCatalogWorker = new BackgroundWorker();
            loadCatalogWorker.DoWork +=
                new DoWorkEventHandler(LoadCatalogWorker_DoWork);
            loadCatalogWorker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(
            LoadCatalogWorker_Completed);
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
            string binaryFilePath = Path.Combine("", "catalog.bin");

            DateTime productsLastModified = File.Exists(productsPath) ? new FileInfo(productsPath).LastWriteTime : DateTime.MinValue;
            DateTime binaryFileLastModified = File.Exists(binaryFilePath) ? new FileInfo(binaryFilePath).LastWriteTime : DateTime.MinValue;

            if (productsLastModified < binaryFileLastModified)
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

            try
            {
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
            catch (Exception ex)
            {
                MessageBox.Show("There was an error downloading the files: " + ex.Message);
            }
        }

        private void UpdateDBWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            ShowLoading(false);
            FillSDKlabels();//change sdk labels to current version
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

        //Function which disables buttons on certain conditions, mostly when other background worker is being run.
        public void EnableButtons(bool enabled)
        {
            bool SDKdownload = UpdateSDK.DoesSDKExist();
            bool DBdownload = UpdateDBFiles.DoesDBExist();

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


        private void BtnScanBiosDrivers_Click(object sender, EventArgs e)
        {
            // Driver/firmware detection is Windows-only and can take a while (loads the DB and
            // collects the hardware inventory), so run it off the UI thread.
            ShowLoading(true);
            btnScanBiosDrivers.Enabled = false;

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, ev) => { ev.Result = TaskScanDriverFirmware.ScanWithInventory(); };
            worker.RunWorkerCompleted += (s, ev) =>
            {
                if (ev.Error != null)
                {
                    MessageBox.Show(
                        "BIOS/Driver scan failed:\n\n" + ev.Error.Message,
                        "BIOS & Drivers",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    staticDriverResults = (List<DriverFirmwareStatus>)ev.Result;
                    UpdateDriverResults();
                }
                btnScanBiosDrivers.Enabled = true;
                ShowLoading(false);
            };
            worker.RunWorkerAsync();
        }


        private void UpdateDriverResults()
        {
            List<ListViewItem> resultList = new List<ListViewItem>();

            // Everything that needs updating goes to the top - on a typical machine this is a
            // handful of rows among a couple of hundred devices, and burying them defeats the
            // point of the view. BIOS/firmware outranks drivers within each group.
            List<DriverFirmwareStatus> sortedList = staticDriverResults
                .OrderBy(p => p.IsMissing ? 0 : 1)
                .ThenBy(p => p.component == "BIOS" ? 0 : 1)
                .ThenBy(p => p.component)
                .ThenBy(p => p.title)
                .ToList();

            lvBiosDrivers.Columns.Clear();
            lvBiosDrivers.Columns.Add("Status", 80);
            lvBiosDrivers.Columns.Add("Component", 80);
            lvBiosDrivers.Columns.Add("Title", 320);
            lvBiosDrivers.Columns.Add("Category", 130);
            lvBiosDrivers.Columns.Add("Severity", 80);
            lvBiosDrivers.Columns.Add("Current", 110);
            lvBiosDrivers.Columns.Add("Target", 110);
            lvBiosDrivers.Columns.Add("Reboot", 130);
            lvBiosDrivers.Columns.Add("Vendor", 120);
            lvBiosDrivers.Columns.Add("Download", 400);
            lvBiosDrivers.View = View.Details;
            lvBiosDrivers.Update();

            int missingCount = 0;

            foreach (DriverFirmwareStatus current in sortedList)
            {
                ListViewItem lvi = new ListViewItem();
                lvi.Text = current.status;                    // column 0
                lvi.SubItems.Add(current.component);
                lvi.SubItems.Add(current.title);
                lvi.SubItems.Add(current.category);
                lvi.SubItems.Add(current.severity);
                lvi.SubItems.Add(current.currentVersion);
                lvi.SubItems.Add(current.targetVersion);
                lvi.SubItems.Add(current.rebootLabel);
                lvi.SubItems.Add(current.vendor);
                lvi.SubItems.Add(current.downloadUrl);
                lvi.Tag = current.patchId;

                if (current.IsMissing)
                {
                    // Colour rather than an icon: it survives sorting and copy/paste of the row.
                    lvi.ForeColor = Color.Firebrick;
                    lvi.Font = new Font(lvBiosDrivers.Font, FontStyle.Bold);
                    missingCount++;
                }

                resultList.Add(lvi);
            }

            lvBiosDrivers.Items.Clear();
            lvBiosDrivers.Items.AddRange(resultList.ToArray());

            lblBiosDriversSummary.Text = sortedList.Count + " device(s) found, " +
                                         missingCount + " needing an update     |     " +
                                         GetDriverFirmwareDbStatus();
        }

        // Version/time of the driver/firmware (BIOS) DB plus the catalog channel it came from.
        // The .dat carries no embedded version string, so its modified time is the version signal;
        // the file size distinguishes the broad staging DB from the smaller production one.
        private string GetDriverFirmwareDbStatus()
        {
            string channel = UpdateDBFiles.GetCatalogChannelDescription();
            string dbName = UpdateDBFiles.DriverFirmwareDbFileName;
            string path = Path.Combine(Directory.GetCurrentDirectory(), dbName);

            if (!File.Exists(path))
            {
                return dbName + ": not present - run Update DB     |     Catalog: " + channel;
            }

            FileInfo fi = new FileInfo(path);
            double mb = fi.Length / (1024.0 * 1024.0);
            return dbName + ":  modified " + fi.LastWriteTime.ToString("yyyy-MM-dd HH:mm") +
                   "  (" + mb.ToString("0.0") + " MB)     |     Catalog: " + channel;
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

                        lviCurrent.SubItems.Add("No");
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
            lvScanResults.Columns.Add("Arch", 40);
            lvScanResults.Columns.Add("Lang", 50);
            lvScanResults.Columns.Add("Latest", 100);
            lvScanResults.Columns.Add("Severity", 40);
            lvScanResults.Columns.Add("CVE Count", 40);
            lvScanResults.Columns.Add("Patched", 70);
            lvScanResults.Columns.Add("Auto", 50);
            lvScanResults.Columns.Add("SigId", 50);
            lvScanResults.Columns.Add("PatchId", 60);
            lvScanResults.Columns.Add("Url", 100);

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

        private void BtnScan_Click(object sender, EventArgs e)
        {
            ShowLoading(true);
            scanWorker.RunWorkerAsync(true);
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

        //Removes placeholder text of "Search Products" when search bar is clicked
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

        //Function called whenever text changed in search bar and updates catalog if it hasn't been updated yet.
        private void searchCatalog_TextChanged(object sender, EventArgs e)
        {

            if (!isCatalogUpdated)
            {
                UpdateCatalogResults();
            }

        }

        //Function returns list of CatalogSignature objects containing searched name in their name property
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

        //Function takes in list of CatalogSignature objects to display on the catalog tab
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

        //This function is called whenever enter key is pressed in the search box on the catalog tab. Fetches list of CatalogSignature objects to show from 
        //searchResult function and then displays them using UpdateSearchCatalogResults function
        private void searchCatalogEnter(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                List<CatalogSignature> resultList = searchResult(searchCatalog.Text);
                UpdateSearchCatalogResults(resultList);

            }
        }

        private void btnUpdateSDK_Click_1(object sender, EventArgs e)
        {
            ShowLoading(true);
            updateDBWorker.RunWorkerAsync(true);
        }
    }
}