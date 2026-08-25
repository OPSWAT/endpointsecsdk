///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for OPSWAT Posture
///  Reference Implementation using OPSWAT Endpoint SDK Compliance module for demoing the 
///  Compliance capability
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using ComplianceAdapater.Log;
using ComplianceAdapater.OESIS;
using ComplianceAdapater.Policy;
using OPSWAT_Adapter.POCO;
using OPSWAT_Adapter.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using VAPMAdapater.Updates;

namespace OPSWATPosture
{
    public partial class MainForm : Form
    {

        private BackgroundWorker validatePolicyWorker;
        private BackgroundWorker getSecurityScoreWorker;
        private BackgroundWorker geoLocationWorker;
        private BackgroundWorker updateSDKWorker;
        private BackgroundWorker checkPluginsWorker;
        private BackgroundWorker getComplianceReportWorker;
        private BackgroundWorker getCategoriesWorker;

        // Serializes access to the process-global OESIS engine. All the OESIS worker threads
        // (policy, score, geolocation, plugins, compliance report, categories) each run
        // InitializeFramework -> Invoke -> TearDown; without this, two overlapping runs could tear
        // the engine down mid-invoke on another thread and crash.
        private static readonly object oesisLock = new object();


        private TaskValidatePolicy      taskValidatePolicy;
        private TaskSecurityScore       taskSecurityScore;
        private List<BrowserPlugins>    browserPluginList = null;

        TaskGeoLocation geolocationValidator = new TaskGeoLocation();



        //
        // Walks up from startDir looking for the 'sdkroot' marker file that identifies the repo
        // root (next to the eval-license directory). Returns null if not found.
        //
        private static string FindRepoRoot(string startDir)
        {
            DirectoryInfo dir = new DirectoryInfo(startDir);
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir.FullName, "sdkroot")))
                {
                    return dir.FullName;
                }
                dir = dir.Parent;
            }
            return null;
        }


        //
        // Ensures license.cfg and pass_key.txt exist in runDir. If they are missing, copies them
        // (and download_token.txt if present) from the repo's eval-license directory. If they can't
        // be found there either, throws a clear "License not found" error.
        //
        private static void EnsureLicenseFiles(string runDir)
        {
            string cfg = Path.Combine(runDir, "license.cfg");
            string key = Path.Combine(runDir, "pass_key.txt");

            // Locate <repo-root>/eval-license (repo root is marked by an 'sdkroot' file); may be null.
            string repoRoot = FindRepoRoot(runDir) ?? FindRepoRoot(Environment.CurrentDirectory);
            string evalDir = (repoRoot != null) ? Path.Combine(repoRoot, "eval-license") : null;

            // Provision the license itself if it isn't already in the running directory.
            if (!File.Exists(cfg) || !File.Exists(key))
            {
                bool copied = false;
                if (evalDir != null)
                {
                    string evalCfg = Path.Combine(evalDir, "license.cfg");
                    string evalKey = Path.Combine(evalDir, "pass_key.txt");
                    if (File.Exists(evalCfg) && File.Exists(evalKey))
                    {
                        File.Copy(evalCfg, cfg, true);
                        File.Copy(evalKey, key, true);
                        copied = true;
                    }
                }

                if (!copied)
                {
                    throw new Exception(
                        "License not found. Please include license.cfg and pass_key.txt in the running directory: " + runDir);
                }
            }

            // Always make sure the download token is present too (used by the SDK/DB update flow) -
            // even when the license files were already in place - copying it from eval-license if
            // it's missing and available.
            string token = Path.Combine(runDir, "download_token.txt");
            if (!File.Exists(token) && evalDir != null)
            {
                string evalToken = Path.Combine(evalDir, "download_token.txt");
                if (File.Exists(evalToken))
                {
                    File.Copy(evalToken, token, true);
                }
            }
        }



        //
        // Update SDK if needed
        //
        private void UpdateFilesOnStartup()
        {
            // The SDK reads license.cfg / pass_key.txt from the executable's directory, so provision
            // them there (with a fallback to the repo's eval-license directory). If no license can be
            // found, show a clear message and exit cleanly rather than crashing during construction.
            string appDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            try
            {
                EnsureLicenseFiles(appDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "License", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
                return;
            }

            GeolocationTab.Enabled = false;
            if (!UpdateSDK.isSDKUpdated())
            {
                pbLoader.BringToFront();
                pbLoader.Visible = true;
                updateSDKWorker.RunWorkerAsync(true);
            }
            else
            {
                pbLoader.BringToFront();
                pbLoader.Visible = false;
                GeolocationTab.Enabled = true;
                LoadLists();
            }
        }

        private void LoadLists()
        {
            List<ProductInfo> firewalList = SupportChart.LoadProductList(OESISCategory.FIREWALL);
            foreach(ProductInfo productInfo in firewalList)
            {
                comboFirewallProduct.Items.Add(productInfo);
            }

            List<ProductInfo> encryptionList = SupportChart.LoadProductList(OESISCategory.DISK_ENCRYPTION);
            foreach (ProductInfo productInfo in encryptionList)
            {
                comboEncryptionProduct.Items.Add(productInfo);
            }

            List<ProductInfo> antimalwareList = SupportChart.LoadProductList(OESISCategory.ANTIMALWARE);
            foreach (ProductInfo productInfo in antimalwareList)
            {
                comboAntimalwareProduct.Items.Add(productInfo);
            }

            dtAntimalwareScanDate.Value = DateTime.Now.AddDays(-1);
            dtDefinitionDate.Value = DateTime.Now.AddDays(-1);
            lblConfiguredSecurityScore.Text = tbSecurityScore.Value.ToString();

            // 
            // Setup default checks for GeoFencing
            //
            cbAllowedCountries.SetItemChecked(0, true);
            cbAllowedCountries.SetItemChecked(1, true);

        }


        public MainForm()
        {
            InitializeComponent();

            // Initialize the Worker Threads
            validatePolicyWorker = new BackgroundWorker();
            validatePolicyWorker.DoWork +=
                new DoWorkEventHandler(validatePolicyWorker_DoWork);
            validatePolicyWorker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(
            validatePolicyWorker_Completed);


            getSecurityScoreWorker = new BackgroundWorker();
            getSecurityScoreWorker.DoWork +=
                new DoWorkEventHandler(getSecurityScoreWorker_DoWork);
            getSecurityScoreWorker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(
            getSecurityScoreWorker_Completed);


            geoLocationWorker = new BackgroundWorker();
            geoLocationWorker.DoWork +=
                new DoWorkEventHandler(geoLocationWorker_DoWork);
            geoLocationWorker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(
            geoLocationWorker_Completed);


            updateSDKWorker = new BackgroundWorker();
            updateSDKWorker.DoWork +=
                new DoWorkEventHandler(updateSDK_Worker_DoWork);
            updateSDKWorker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(
            updateSDK_Worker_Completed);


            checkPluginsWorker = new BackgroundWorker();
            checkPluginsWorker.DoWork +=
                new DoWorkEventHandler(checkPlugins_Worker_DoWork);
            checkPluginsWorker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(
            checkPlugins_Worker_Completed);


            getComplianceReportWorker = new BackgroundWorker();
            getComplianceReportWorker.DoWork +=
                new DoWorkEventHandler(getComplianceReportWorker_DoWork);
            getComplianceReportWorker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(
            getComplianceReportWorker_Completed);


            getCategoriesWorker = new BackgroundWorker();
            getCategoriesWorker.DoWork +=
                new DoWorkEventHandler(getCategoriesWorker_DoWork);
            getCategoriesWorker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(
            getCategoriesWorker_Completed);



            UpdateFilesOnStartup();
        }



        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///  Worker Threads - GetPolicy
        /// </summary>
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void validatePolicyWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                bool valid;
                lock (oesisLock)
                {
                    valid = taskValidatePolicy.ValidatePolicy();
                }
                e.Result = valid;
            }
            catch(Exception exception)
            {
                MessageBox.Show(exception.Message);
                e.Result = false;
            }
        }

        private void validatePolicyWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            PrintLogEntries(lvPolicy, taskValidatePolicy.GetLogger());

            if ((bool)e.Result == true)
            {
                pbStatusIcon.Image = Properties.Resources.GreenLight;
            }
            else
            {
                pbStatusIcon.Image = Properties.Resources.RedLight;
            }

            Cursor.Current = Cursors.Default;
            btnCheckPolicy.Enabled = true;

        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///  Worker Threads - GetSecurity Score
        /// </summary>
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void getSecurityScoreWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                taskSecurityScore = new TaskSecurityScore();
                int securityScore;
                lock (oesisLock)
                {
                    securityScore = taskSecurityScore.GetSecurityScore();
                }

                e.Result = securityScore;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                e.Result = null;
            }

        }

        private void getSecurityScoreWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                lvSecurityScore.Items.Clear();
                PrintLogEntries(lvSecurityScore, taskSecurityScore.GetLogger());

                int securityScore = (int)e.Result;

                lblCurrentSecurityScore.Text = securityScore.ToString();
                if (tbSecurityScore.Value <= securityScore)
                {
                    pbScoreImage.Image = Properties.Resources.GreenLight;
                }
                else
                {
                    pbScoreImage.Image = Properties.Resources.RedLight;
                }
            }
            else
            {
                pbScoreImage.Image = Properties.Resources.RedLight;
            }

            // Always re-enable the button and reset the cursor, so a failure doesn't strand the tab.
            Cursor.Current = Cursors.Default;
            btnGetSecurityScore.Enabled = true;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///  Worker Threads - GeoLocation
        /// </summary>
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void geoLocationWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                lock (oesisLock)
                {
                    geolocationValidator.GetGeolocation();
                }

                GeoLocationInfo info = geolocationValidator.GetGeoLocationInfo();
                e.Result = info;
            }
            catch(Exception exception)
            {
                MessageBox.Show(exception.Message);
                e.Result = null;
            }
        }

        private void geoLocationWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {

            if (e.Result != null)
            {
                GeoLocationInfo info = (GeoLocationInfo)e.Result;

                if (info.longitude != null)
                {
                    lblLatitude.Text = info.latitude;
                    lblLongitude.Text = info.longitude;
                    lblCountry.Text = info.countryName;

                    double policyLatitude = double.Parse(tbLatitude.Text);
                    double policyLongitude = double.Parse(tbLongitude.Text);

                    double actualMiles = geolocationValidator.CalculateMiles(policyLatitude, policyLongitude);
                    lblMiles.Text = actualMiles.ToString();

                    bool deviceIsValid = false;
                    if (rbDistanceInMiles.Checked)
                    {
                        double policyMiles = double.Parse(tbMiles.Text);

                        if (policyMiles > actualMiles)
                        {
                            deviceIsValid = true;
                        }
                    }
                    else if (rbAllowedCountries.Checked)
                    {
                        for (int i = 0; i < cbAllowedCountries.CheckedItems.Count; i++)
                        {
                            if ((string)cbAllowedCountries.CheckedItems[i] == info.countryName)
                            {
                                deviceIsValid = true;
                                break;
                            }
                        }
                    }

                    if (deviceIsValid)
                    {
                        pbGeoFenceResult.Image = Properties.Resources.GreenLight;
                    }
                    else
                    {
                        pbGeoFenceResult.Image = Properties.Resources.RedLight;
                    }

                    linkGeolocation.Text = new Uri("https://www.google.com/maps/search/?api=1&map_action=map&query=" + info.latitude + "%2C" + info.longitude).ToString();
                    linkOrigin.Text = new Uri("https://www.google.com/maps/search/?api=1&map_action=map&query=" + policyLatitude + "%2C" + policyLongitude).ToString();
                }
                else
                {
                    MessageBox.Show("Unable to retrieve location.  Check license and make sure location services is enabled.");
                    pbGeoFenceResult.Image = Properties.Resources.RedLight;
                }
            }
            else
            {
                pbGeoFenceResult.Image = Properties.Resources.RedLight;
            }

            Cursor.Current = Cursors.Default;
            btnGetLocation.Enabled = true;
        }


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///  Worker Threads - Update SDK
        /// </summary>
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void updateSDK_Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                UpdateSDK.DownloadAndInstall_OPSWAT_SDK();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Failed to download SDK.  " + ex.Message);
            }
        }

        private void updateSDK_Worker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            GeolocationTab.Enabled = true;
            pbLoader.SendToBack();
            pbLoader.Visible = false;


            LoadLists();
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///  Worker Threads - Checkin Plugins
        /// </summary>
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void checkPlugins_Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                lock (oesisLock)
                {
                    checkPlugins();
                }
                e.Result = true;
            }
            catch(Exception exception)
            {
                MessageBox.Show(exception.Message);
                e.Result = null;
            }
        }

        private void checkPlugins_Worker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                updatePluginUI();
                pbLoader.SendToBack();
                btnCheckPlugins.Enabled = true;
            }

            pbLoader.Visible = false;
        }


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///  Buttons and other logic
        /// </summary>
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void cbAntimalwareEnabled_CheckedChanged(object sender, EventArgs e)
        {
            if(cbAntimalwareEnabled.Checked)
            {
                gbAntimalware.Enabled = true;
            }
            else
            {
                gbAntimalware.Enabled = false;
            }
        }

        private void cbSystemVulnerabiltiesEnabled_CheckedChanged(object sender, EventArgs e)
        {
            if (cbFirewallEnabled.Checked)
            {
                gbFirewall.Enabled = true;
            }
            else
            {
                gbFirewall.Enabled = false;
            }
        }

        private void cbEncryptionEnabled_CheckedChanged(object sender, EventArgs e)
        {
            if(cbEncryptionEnabled.Checked)
            {
                gbEncryption.Enabled = true;
            }
            else
            {
                gbEncryption.Enabled = false;
            }
        }

        private bool checkFirewall()
        {
            bool result = true;
            return result;
        }

        private bool checkEncryption()
        {
            bool result = true;

            if (cbEncryptionEnabled.Checked)
            {

            }


            return result;
        }

        private SecurityPolicy FillInSecurityPolicy()
        {
            SecurityPolicy securityPolicy = new SecurityPolicy();

            FirewallPolicy firewallPolicy = new FirewallPolicy();
            firewallPolicy.enabled = cbFirewallEnabled.Checked;
            firewallPolicy.isProtected = cbFirewallEnforced.Checked;
            if(comboFirewallProduct.SelectedItem != null && comboFirewallProduct.SelectedItem.GetType().Equals(typeof(ProductInfo)))
            {
                ProductInfo productInfo = (ProductInfo)comboFirewallProduct.SelectedItem;
                firewallPolicy.expectedProduct = productInfo;
            }


            EncryptionPolicy encryptionPolicy = new EncryptionPolicy();
            encryptionPolicy.enabled = cbEncryptionEnabled.Checked;
            encryptionPolicy.isEncrypted = cbEncrytionDriveEncrypted.Checked;
            if (comboEncryptionProduct.SelectedItem != null && comboEncryptionProduct.SelectedItem.GetType().Equals(typeof(ProductInfo)))
            {
                ProductInfo productInfo = (ProductInfo)comboEncryptionProduct.SelectedItem;
                encryptionPolicy.expectedProduct = productInfo;
            }


            AntimalwarePolicy antimalwarePolicy = new AntimalwarePolicy();
            antimalwarePolicy.enabled = cbAntimalwareEnabled.Checked;
            antimalwarePolicy.isProtected = cbValidateAntimalware.Checked;
            antimalwarePolicy.defintionDate = dtDefinitionDate.Value;
            antimalwarePolicy.scanDate = dtAntimalwareScanDate.Value;
            if (comboAntimalwareProduct.SelectedItem != null && comboAntimalwareProduct.SelectedItem.GetType().Equals(typeof(ProductInfo)))
            {
                ProductInfo productInfo = (ProductInfo)comboAntimalwareProduct.SelectedItem;
                antimalwarePolicy.expectedProduct = productInfo;
            }


            securityPolicy.antimalwarePolicy = antimalwarePolicy;
            securityPolicy.firewallPolicy = firewallPolicy;
            securityPolicy.encryptionPolicy = encryptionPolicy;

            return securityPolicy;
        }

        private void PrintLogEntries(ListView lv, Logger log)
        {
            List<LogEntry> entryList = log.GetLogEntryList();

            foreach(LogEntry entry in entryList)
            {
                ListViewItem item = new ListViewItem();
                item.Text = entry.message;

                if (entry.success)
                {
                    item.ForeColor = System.Drawing.Color.Green;
                }
                else
                {
                    item.ForeColor = System.Drawing.Color.Red;
                }
                item.SubItems.Add("test");

                lv.Items.Add(item);
            }
        }


        private void btnCheckPolicyClick(object sender, EventArgs e)
        {
            lvPolicy.Items.Clear();
            Cursor.Current = Cursors.WaitCursor;
            btnCheckPolicy.Enabled = false;
            pbStatusIcon.Image = Properties.Resources.progressbar;

            SecurityPolicy secPolicy = FillInSecurityPolicy();
            taskValidatePolicy = new TaskValidatePolicy(secPolicy);

            // Do a scan again
            validatePolicyWorker.RunWorkerAsync(true);
        }

        private void tbSecurityScore_Scroll(object sender, EventArgs e)
        {
            lblConfiguredSecurityScore.Text = tbSecurityScore.Value.ToString();
        }

        private void btnGetSecurityScore_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            btnGetSecurityScore.Enabled = false;
            lvSecurityScore.Items.Clear();
            pbScoreImage.Image = Properties.Resources.progressbar;

            // Do a scan again
            getSecurityScoreWorker.RunWorkerAsync(true);
        }

        private void btnGetComplianceReport_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            btnGetComplianceReport.Enabled = false;
            txtComplianceReport.Text = "Getting compliance report...";

            getComplianceReportWorker.RunWorkerAsync(true);
        }

        private void getComplianceReportWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            lock (oesisLock)
            {
                e.Result = TaskComplianceReport.GetReportJson();
            }
        }

        private void getComplianceReportWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                txtComplianceReport.Text = "Error getting compliance report:" +
                    Environment.NewLine + Environment.NewLine + e.Error.Message;
            }
            else
            {
                // The engine pretty-prints with '\n'; normalize to Windows line endings so the
                // multiline TextBox renders the JSON correctly.
                string json = (string)e.Result;
                txtComplianceReport.Text = json.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
            }

            Cursor.Current = Cursors.Default;
            btnGetComplianceReport.Enabled = true;
        }

        private void btnGetCategories_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            btnGetCategories.Enabled = false;

            getCategoriesWorker.RunWorkerAsync(true);
        }

        private void getCategoriesWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            lock (oesisLock)
            {
                e.Result = TaskCategories.GetCategories();
            }
        }

        private void getCategoriesWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show("Error getting categories:" +
                    Environment.NewLine + Environment.NewLine + e.Error.Message);
            }
            else
            {
                List<ProductCategory> categories = (List<ProductCategory>)e.Result;

                lvCategories.BeginUpdate();
                lvCategories.Items.Clear();
                lvCategories.Columns.Clear();
                lvCategories.Columns.Add("Application", 380);
                lvCategories.Columns.Add("Signature ID", 120);
                lvCategories.Columns.Add("Category", 260);

                // One row per (application, category); products with multiple categories repeat.
                // Build the items first and AddRange once so the active column sorter runs a single
                // sort rather than re-sorting on every insert.
                List<ListViewItem> items = new List<ListViewItem>();
                foreach (ProductCategory pc in categories)
                {
                    ListViewItem item = new ListViewItem(pc.application);
                    item.SubItems.Add(pc.signatureId.ToString());
                    item.SubItems.Add(pc.category);
                    items.Add(item);
                }
                lvCategories.Items.AddRange(items.ToArray());
                lvCategories.EndUpdate();
            }

            Cursor.Current = Cursors.Default;
            btnGetCategories.Enabled = true;
        }

        private void btnGetLocation_Click(object sender, EventArgs e)
        {
            btnGetLocation.Enabled = false;
            Cursor.Current = Cursors.WaitCursor;
            pbGeoFenceResult.Image = Properties.Resources.progressbar;

            geoLocationWorker.RunWorkerAsync(true);
        }

        private void linkGeolocation_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(linkGeolocation.Text);
        }

        private void linkOrigin_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(linkOrigin.Text);
        }

        private bool isPluginBlocked(PluginDetail pluginDetail)
        {
            bool result = false;

            foreach(string blockedItem in clbBlockedPlugins.CheckedItems)
            {
                if(blockedItem == pluginDetail.name)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        private bool isBrowswerBlocked(BrowserPlugins browserPlugin)
        {
            bool result = false;

            foreach (string blockedItem in cblBrowsers.CheckedItems)
            {
                if (blockedItem == browserPlugin.browserName)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }


        private void checkPlugins()
        {
            TaskGetPlugins task = new TaskGetPlugins();
            browserPluginList = task.GetPlugins();
        }

        private void updatePluginUI()
        {
            bool isAllowed = true;

            //
            // Setup the header
            //
            lvPlugins.Items.Clear();
            lvPlugins.Columns.Clear();
            lvPlugins.Columns.Add("", 20);
            lvPlugins.Columns.Add("Plugin", 250);
            lvPlugins.Columns.Add("Type", 75);
            lvPlugins.Columns.Add("Browser", 125);
            lvPlugins.Columns.Add("Description", 400);
            lvPlugins.View = View.Details;
            lvPlugins.Update();

            if (browserPluginList != null)
            {
                foreach (BrowserPlugins current in browserPluginList)
                {
                    bool browserBlocked = false;
                    if (isBrowswerBlocked(current))
                    {
                        isAllowed = false;
                        browserBlocked = true;
                    }


                    foreach (PluginDetail currentDetail in current.pluginList)
                    {
                        bool isBlocked = isPluginBlocked(currentDetail);
                        if (isBlocked)
                        {
                            isAllowed = false;
                        }

                        ListViewItem lviCurrent = new ListViewItem();
                        lviCurrent.Text = isBlocked || browserBlocked ? "*" : "";
                        lviCurrent.SubItems.Add(currentDetail.name);
                        lviCurrent.SubItems.Add(currentDetail.type);
                        lviCurrent.SubItems.Add(current.browserName);
                        lviCurrent.SubItems.Add(currentDetail.description);
                        lviCurrent.Tag = currentDetail.id;
                        lvPlugins.Items.Add(lviCurrent);
                    }
                }
                lvPlugins.Update();
            }

            if (isAllowed == true)
            {
                pbPluginStatus.Image = Properties.Resources.GreenLight;
            }
            else
            {
                pbPluginStatus.Image = Properties.Resources.RedLight;
            }

            Cursor.Current = Cursors.Default;
        }

        private void btnCheckPlugins_Click(object sender, EventArgs e)
        {
            pbLoader.BringToFront();
            pbLoader.Visible = true;
            lvPlugins.Clear();
            btnCheckPlugins.Enabled = false;

            checkPluginsWorker.RunWorkerAsync(true);
        }
    }
}
