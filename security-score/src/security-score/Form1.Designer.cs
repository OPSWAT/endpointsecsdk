namespace security_score
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea3 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend3 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.scoreChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.btnCalculateScore = new System.Windows.Forms.Button();
            this.lblScore = new System.Windows.Forms.Label();
            this.pbLoader = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.scoreChart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbLoader)).BeginInit();
            this.SuspendLayout();
            // 
            // scoreChart
            // 
            chartArea3.Name = "ChartArea1";
            this.scoreChart.ChartAreas.Add(chartArea3);
            legend3.Name = "Legend1";
            this.scoreChart.Legends.Add(legend3);
            this.scoreChart.Location = new System.Drawing.Point(41, 32);
            this.scoreChart.Name = "scoreChart";
            series3.ChartArea = "ChartArea1";
            series3.Legend = "Legend1";
            series3.Name = "Series1";
            this.scoreChart.Series.Add(series3);
            this.scoreChart.Size = new System.Drawing.Size(300, 300);
            this.scoreChart.TabIndex = 0;
            this.scoreChart.Text = "chart1";
            // 
            // btnCalculateScore
            // 
            this.btnCalculateScore.Location = new System.Drawing.Point(41, 348);
            this.btnCalculateScore.Name = "btnCalculateScore";
            this.btnCalculateScore.Size = new System.Drawing.Size(75, 23);
            this.btnCalculateScore.TabIndex = 1;
            this.btnCalculateScore.Text = "Calculate Score";
            this.btnCalculateScore.UseVisualStyleBackColor = true;
            this.btnCalculateScore.Click += new System.EventHandler(this.btnCalculateScore_Click);
            // 
            // lblScore
            // 
            this.lblScore.AutoSize = true;
            this.lblScore.BackColor = System.Drawing.Color.White;
            this.lblScore.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblScore.Location = new System.Drawing.Point(165, 120);
            this.lblScore.Name = "lblScore";
            this.lblScore.Size = new System.Drawing.Size(0, 55);
            this.lblScore.TabIndex = 3;
            // 
            // pbLoader
            // 
            this.pbLoader.Image = global::security_score.Properties.Resources.giphy;
            this.pbLoader.Location = new System.Drawing.Point(41, 32);
            this.pbLoader.Name = "pbLoader";
            this.pbLoader.Size = new System.Drawing.Size(300, 300);
            this.pbLoader.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbLoader.TabIndex = 2;
            this.pbLoader.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(386, 398);
            this.Controls.Add(this.lblScore);
            this.Controls.Add(this.pbLoader);
            this.Controls.Add(this.btnCalculateScore);
            this.Controls.Add(this.scoreChart);
            this.Name = "Form1";
            this.Text = "Acme Security Score";
            ((System.ComponentModel.ISupportInitialize)(this.scoreChart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbLoader)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart scoreChart;
        private System.Windows.Forms.Button btnCalculateScore;
        private System.Windows.Forms.Label lblScore;
        private System.Windows.Forms.PictureBox pbLoader;
    }
}

