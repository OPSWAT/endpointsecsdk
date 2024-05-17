using OPSWAT_Adapter.TaskPOCO;
using OPSWAT_Adapter.Tasks;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using VAPMAdapater.Updates;

namespace security_score
{
    public partial class Form1 : Form
    {
        private BackgroundWorker updateSDKWorker;
        private BackgroundWorker calculateScoreWorker;
        private SecurityScoreResult securityScoreResult = null;


        public Form1()
        {
            InitializeComponent();

            updateSDKWorker = new BackgroundWorker();
            updateSDKWorker.DoWork +=
                new DoWorkEventHandler(updateSDK_Worker_DoWork);
            updateSDKWorker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(
            updateSDK_Worker_Completed);

            calculateScoreWorker = new BackgroundWorker();
            calculateScoreWorker.DoWork +=
                new DoWorkEventHandler(calculateScore_Worker_DoWork);
            calculateScoreWorker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(
            calculateScore_Worker_Completed);

            UpdateFilesOnStartup();
            UpdateScoreChart();
        }

        private void UpdateScoreChart()
        {



            //reset your chart series and legends
            scoreChart.Series.Clear();
            scoreChart.Legends.Clear();

            //Add a new Legend(if needed) and do some formating
            scoreChart.Legends.Add("MyLegend");
            scoreChart.Legends[0].LegendStyle = LegendStyle.Table;
            scoreChart.Legends[0].Docking = Docking.Bottom;
            scoreChart.Legends[0].Alignment = StringAlignment.Center;
            scoreChart.Legends[0].Title = "Security Score";
            scoreChart.Legends[0].BorderColor = Color.Black;

            //Add a new chart-series
            string seriesname = "Security Score";
            scoreChart.Series.Add(seriesname);
            //set the chart-type to "Pie"
            scoreChart.Series[seriesname].ChartType = SeriesChartType.Doughnut;


            if(securityScoreResult != null)
            {
                foreach(SecurityScoreModule module in securityScoreResult.moduleList)
                {
                    DataPoint point = new DataPoint();
                    point.Name = module.name;

                    if(module.score > 0)
                    {
                        point.SetValueXY(module.name, 2);
                        point.Color = Color.Green;
                    }
                    else
                    {
                        point.SetValueXY(module.name, 2);
                        point.Color = Color.Red;
                    }

                    scoreChart.Series[seriesname].Points.Add(point);
                }

                lblScore.Text = securityScoreResult.score.ToString();
            }
            else
            {
                DataPoint point = new DataPoint();
                point.Name = "Unknown";
                point.SetValueXY("Unknown",1);
                point.Color = Color.Yellow;
                scoreChart.Series[seriesname].Points.Add(point);
            }
        }


        private void StartWaitStatus()
        {
            pbLoader.BringToFront();
            pbLoader.Visible = true;
            btnCalculateScore.Enabled = false;
        }

        private void StopWaitStatus()
        {
            pbLoader.SendToBack();
            pbLoader.Visible = false;
            btnCalculateScore.Enabled = true;
        }


        //
        // Update SDK if needed
        //
        private void UpdateFilesOnStartup()
        {
            if (!UpdateSDK.isSDKUpdated())
            {
                StartWaitStatus();
                updateSDKWorker.RunWorkerAsync(true);
            }
            else
            {
                StopWaitStatus();
            }
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
            StopWaitStatus();
        }


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///  Worker Threads - Update SDK
        /// </summary>
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void calculateScore_Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            TaskGetAdvancedSecurityScore scoreTask = new TaskGetAdvancedSecurityScore();
            securityScoreResult = scoreTask.GetAdvancedSecurityScore();
        }

        private void calculateScore_Worker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateScoreChart();
            StopWaitStatus();
        }


        private void btnCalculateScore_Click(object sender, System.EventArgs e)
        {
            StartWaitStatus();
            calculateScoreWorker.RunWorkerAsync();
        }
    }
}
