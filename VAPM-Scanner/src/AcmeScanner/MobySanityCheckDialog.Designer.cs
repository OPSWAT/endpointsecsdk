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
            materialCheckbox1 = new MaterialSkin.Controls.MaterialCheckbox();
            materialCheckbox2 = new MaterialSkin.Controls.MaterialCheckbox();
            materialCheckbox3 = new MaterialSkin.Controls.MaterialCheckbox();
            SuspendLayout();
            // 
            // btnRunAllChecksMoby
            // 
            btnRunAllChecksMoby.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnRunAllChecksMoby.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnRunAllChecksMoby.Depth = 0;
            btnRunAllChecksMoby.HighEmphasis = true;
            btnRunAllChecksMoby.Icon = null;
            btnRunAllChecksMoby.Location = new System.Drawing.Point(714, 45);
            btnRunAllChecksMoby.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            btnRunAllChecksMoby.MouseState = MaterialSkin.MouseState.HOVER;
            btnRunAllChecksMoby.Name = "btnRunAllChecksMoby";
            btnRunAllChecksMoby.NoAccentTextColor = System.Drawing.Color.Empty;
            btnRunAllChecksMoby.Size = new System.Drawing.Size(195, 36);
            btnRunAllChecksMoby.TabIndex = 0;
            btnRunAllChecksMoby.Text = "Run all sanity checks";
            btnRunAllChecksMoby.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnRunAllChecksMoby.UseAccentColor = false;
            btnRunAllChecksMoby.UseVisualStyleBackColor = true;
            btnRunAllChecksMoby.Click += btnRunAllChecksMoby_Click;
            // 
            // btnRunSelectedChecksMoby
            // 
            btnRunSelectedChecksMoby.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnRunSelectedChecksMoby.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnRunSelectedChecksMoby.Depth = 0;
            btnRunSelectedChecksMoby.HighEmphasis = true;
            btnRunSelectedChecksMoby.Icon = null;
            btnRunSelectedChecksMoby.Location = new System.Drawing.Point(304, 45);
            btnRunSelectedChecksMoby.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            btnRunSelectedChecksMoby.MouseState = MaterialSkin.MouseState.HOVER;
            btnRunSelectedChecksMoby.Name = "btnRunSelectedChecksMoby";
            btnRunSelectedChecksMoby.NoAccentTextColor = System.Drawing.Color.Empty;
            btnRunSelectedChecksMoby.Size = new System.Drawing.Size(185, 36);
            btnRunSelectedChecksMoby.TabIndex = 1;
            btnRunSelectedChecksMoby.Text = "run selected checks";
            btnRunSelectedChecksMoby.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnRunSelectedChecksMoby.UseAccentColor = false;
            btnRunSelectedChecksMoby.UseVisualStyleBackColor = true;
            btnRunSelectedChecksMoby.Click += btnRunSelectedChecksMoby_Click;
            // 
            // materialCheckbox1
            // 
            materialCheckbox1.AutoSize = true;
            materialCheckbox1.Depth = 0;
            materialCheckbox1.Location = new System.Drawing.Point(23, 9);
            materialCheckbox1.Margin = new System.Windows.Forms.Padding(0);
            materialCheckbox1.MouseLocation = new System.Drawing.Point(-1, -1);
            materialCheckbox1.MouseState = MaterialSkin.MouseState.HOVER;
            materialCheckbox1.Name = "materialCheckbox1";
            materialCheckbox1.ReadOnly = false;
            materialCheckbox1.Ripple = true;
            materialCheckbox1.Size = new System.Drawing.Size(140, 37);
            materialCheckbox1.TabIndex = 2;
            materialCheckbox1.Text = "Sanity Check 1";
            materialCheckbox1.UseVisualStyleBackColor = true;
            // 
            // materialCheckbox2
            // 
            materialCheckbox2.AutoSize = true;
            materialCheckbox2.Depth = 0;
            materialCheckbox2.Location = new System.Drawing.Point(23, 44);
            materialCheckbox2.Margin = new System.Windows.Forms.Padding(0);
            materialCheckbox2.MouseLocation = new System.Drawing.Point(-1, -1);
            materialCheckbox2.MouseState = MaterialSkin.MouseState.HOVER;
            materialCheckbox2.Name = "materialCheckbox2";
            materialCheckbox2.ReadOnly = false;
            materialCheckbox2.Ripple = true;
            materialCheckbox2.Size = new System.Drawing.Size(140, 37);
            materialCheckbox2.TabIndex = 3;
            materialCheckbox2.Text = "Sanity Check 2";
            materialCheckbox2.UseVisualStyleBackColor = true;
            // 
            // materialCheckbox3
            // 
            materialCheckbox3.AutoSize = true;
            materialCheckbox3.Depth = 0;
            materialCheckbox3.Location = new System.Drawing.Point(23, 82);
            materialCheckbox3.Margin = new System.Windows.Forms.Padding(0);
            materialCheckbox3.MouseLocation = new System.Drawing.Point(-1, -1);
            materialCheckbox3.MouseState = MaterialSkin.MouseState.HOVER;
            materialCheckbox3.Name = "materialCheckbox3";
            materialCheckbox3.ReadOnly = false;
            materialCheckbox3.Ripple = true;
            materialCheckbox3.Size = new System.Drawing.Size(140, 37);
            materialCheckbox3.TabIndex = 4;
            materialCheckbox3.Text = "Sanity Check 3";
            materialCheckbox3.UseVisualStyleBackColor = true;
            // 
            // MobySanityCheckDialog
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(979, 529);
            Controls.Add(materialCheckbox3);
            Controls.Add(materialCheckbox2);
            Controls.Add(materialCheckbox1);
            Controls.Add(btnRunSelectedChecksMoby);
            Controls.Add(btnRunAllChecksMoby);
            Margin = new System.Windows.Forms.Padding(2);
            Name = "MobySanityCheckDialog";
            Text = "MobySanityCheckDialog";
            Load += MobySanityCheckDialog_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MaterialSkin.Controls.MaterialButton btnRunAllChecksMoby;
        private MaterialSkin.Controls.MaterialButton btnRunSelectedChecksMoby;
        private MaterialSkin.Controls.MaterialCheckbox materialCheckbox1;
        private MaterialSkin.Controls.MaterialCheckbox materialCheckbox2;
        private MaterialSkin.Controls.MaterialCheckbox materialCheckbox3;
    }
}