namespace AcmeScanner
{
    partial class MobySanityCheckDialog
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
            btnRunAllChecksMoby = new MaterialSkin.Controls.MaterialButton();
            btnRunSelectedChecksMoby = new MaterialSkin.Controls.MaterialButton();
            sanityChecksListView = new ScannerListView();
            SuspendLayout();
            // 
            // btnRunAllChecksMoby
            // 
            btnRunAllChecksMoby.AutoSize = false;
            btnRunAllChecksMoby.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnRunAllChecksMoby.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnRunAllChecksMoby.Depth = 0;
            btnRunAllChecksMoby.HighEmphasis = true;
            btnRunAllChecksMoby.Icon = null;
            btnRunAllChecksMoby.Location = new System.Drawing.Point(804, 68);
            btnRunAllChecksMoby.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            btnRunAllChecksMoby.MouseState = MaterialSkin.MouseState.HOVER;
            btnRunAllChecksMoby.Name = "btnRunAllChecksMoby";
            btnRunAllChecksMoby.NoAccentTextColor = System.Drawing.Color.Empty;
            btnRunAllChecksMoby.Size = new System.Drawing.Size(279, 60);
            btnRunAllChecksMoby.TabIndex = 0;
            btnRunAllChecksMoby.Text = "Run all sanity checks";
            btnRunAllChecksMoby.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnRunAllChecksMoby.UseAccentColor = false;
            btnRunAllChecksMoby.UseVisualStyleBackColor = true;
            btnRunAllChecksMoby.Click += BtnRunAllChecksMoby_Click;
            // 
            // btnRunSelectedChecksMoby
            // 
            btnRunSelectedChecksMoby.AutoSize = false;
            btnRunSelectedChecksMoby.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnRunSelectedChecksMoby.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnRunSelectedChecksMoby.Depth = 0;
            btnRunSelectedChecksMoby.HighEmphasis = true;
            btnRunSelectedChecksMoby.Icon = null;
            btnRunSelectedChecksMoby.Location = new System.Drawing.Point(312, 68);
            btnRunSelectedChecksMoby.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            btnRunSelectedChecksMoby.MouseState = MaterialSkin.MouseState.HOVER;
            btnRunSelectedChecksMoby.Name = "btnRunSelectedChecksMoby";
            btnRunSelectedChecksMoby.NoAccentTextColor = System.Drawing.Color.Empty;
            btnRunSelectedChecksMoby.Size = new System.Drawing.Size(264, 60);
            btnRunSelectedChecksMoby.TabIndex = 1;
            btnRunSelectedChecksMoby.Text = "select checks to run";
            btnRunSelectedChecksMoby.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnRunSelectedChecksMoby.UseAccentColor = false;
            btnRunSelectedChecksMoby.UseVisualStyleBackColor = true;
            btnRunSelectedChecksMoby.Click += BtnRunSelectedChecksMoby_Click;
            // 
            // sanityChecksListView
            // 
            sanityChecksListView.FullRowSelect = true;
            sanityChecksListView.GridLines = true;
            sanityChecksListView.Location = new System.Drawing.Point(9, 200);
            sanityChecksListView.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            sanityChecksListView.MultiSelect = false;
            sanityChecksListView.Name = "sanityChecksListView";
            sanityChecksListView.OwnerDraw = true;
            sanityChecksListView.Size = new System.Drawing.Size(1384, 677);
            sanityChecksListView.TabIndex = 5;
            sanityChecksListView.UseCompatibleStateImageBehavior = false;
            sanityChecksListView.View = System.Windows.Forms.View.Details;
            sanityChecksListView.SelectedIndexChanged += sanityChecksListView_SelectedIndexChanged;
            // 
            // MobySanityCheckDialog
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1399, 882);
            Controls.Add(sanityChecksListView);
            Controls.Add(btnRunSelectedChecksMoby);
            Controls.Add(btnRunAllChecksMoby);
            Name = "MobySanityCheckDialog";
            Text = "MobySanityCheckDialog";
            Load += MobySanityCheckDialog_Load;
            ResumeLayout(false);
        }

        #endregion

        private MaterialSkin.Controls.MaterialButton btnRunAllChecksMoby;
        private MaterialSkin.Controls.MaterialButton btnRunSelectedChecksMoby;
        private ScannerListView sanityChecksListView;
    }
}