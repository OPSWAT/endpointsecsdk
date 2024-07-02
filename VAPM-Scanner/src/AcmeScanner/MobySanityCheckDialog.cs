using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VAPMAdapter.Tasks;

namespace AcmeScanner
{
    public partial class MobySanityCheckDialog : Form
    {
        private System.ComponentModel.BackgroundWorker runAllChecks;
        private System.ComponentModel.BackgroundWorker runSelectedChecks;
        private string jsonContent;
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
            jsonContent = TaskRunPythonScripts.Execute("auto_patching_check");
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
