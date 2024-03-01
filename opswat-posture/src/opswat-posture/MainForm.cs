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
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.ComponentModel;
using OPSWAT_Adapter.Tasks;
using VAPMAdapater.Updates;
using OPSWAT_Adapter.POCO;

namespace OPSWATPosture
{
    public partial class MainForm : Form
    {

        private BackgroundWorker validatePolicyWorker;
        private BackgroundWorker getSecurityScoreWorker;
        private BackgroundWorker geoLocationWorker;
        private BackgroundWorker updateSDKWorker;
        private BackgroundWorker checkPluginsWorker;


        private TaskValidatePolicy      taskValidatePolicy;
        private TaskSecurityScore       taskSecurityScore;
        private List<BrowserPlugins>    browserPluginList = null;



        //
        // Update SDK if needed
        //
        private void UpdateFilesOnStartup()
        {
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

            

            UpdateFilesOnStartup();
        }



        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///  Worker Threads - GetPolicy
        /// </summary>
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void validatePolicyWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            bool valid = taskValidatePolicy.ValidatePolicy();
            e.Result = valid;
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
            taskSecurityScore = new TaskSecurityScore();
            int securityScore = taskSecurityScore.GetSecurityScore();

            e.Result = securityScore;
        }

        private void getSecurityScoreWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
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

            TaskGeoLocation geolocationValidator = new TaskGeoLocation();
            geolocationValidator.GetGeolocation();

            GeoLocationInfo info = geolocationValidator.GetGeoLocationInfo();
            e.Result = info;
        }

        private void geoLocationWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            GeoLocationInfo info = (GeoLocationInfo)e.Result;

            if(info.longitude != null)
            {
                lblLatitude.Text = info.latitude;
                lblLongitude.Text = info.longitude;
                lblCountry.Text = info.countryName;

                double policyLatitude = double.Parse(tbLatitude.Text);
                double policyLongitude = double.Parse(tbLongitude.Text);

                TaskGeoLocation geolocationValidator = new TaskGeoLocation();
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
            UpdateSDK.DownloadAndInstall_OPSWAT_SDK();
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
            checkPlugins();
        }

        private void checkPlugins_Worker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            updatePluginUI();
            pbLoader.SendToBack();
            btnCheckPlugins.Enabled = true;

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
