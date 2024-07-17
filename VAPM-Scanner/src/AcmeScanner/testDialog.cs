using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace AcmeScanner
{
    public partial class testDialog : Form
    {
        List<string> selectedScripts = new List<string>();
        public testDialog()
        {
            InitializeComponent();
        }

        private void testDialog_Load(object sender, EventArgs e)
        {

        }

        private void BtnDone_Click(object sender, EventArgs e)
        {
            selectedScripts.Clear();
            foreach (Control control in this.Controls)
            {
                if (control is CheckBox && ((CheckBox)control).Checked)
                {
                    selectedScripts.Add(((CheckBox)control).Text);
                }
            }
            this.Close();
        }

        public List<String> AddCheckboxes()
        {
            List<string> scripts = MobySanityCheckDialog.GetAllScripts();
            CheckBox box;
            int i = 1;
            foreach (string script in scripts)
            {
                box = new CheckBox();
                box.Tag = script;
                box.Text = script;
                box.Font = new Font(box.Font.FontFamily, 14);
                box.Size = new System.Drawing.Size(250, 50);
                box.Location = new Point(10, i * 50); //vertical
                                                      //box.Location = new Point(i * 50, 10); //horizontal
                this.Controls.Add(box);
                i += 1;
               
            }
            Button btnDone = new Button();
            btnDone.Text = "Done";
            btnDone.Size = new System.Drawing.Size(100, 30);
            btnDone.Location = new Point(10,50+ i * 50);
            btnDone.Click += BtnDone_Click;
            this.Controls.Add(btnDone);
            
            return selectedScripts;
            
        }
    }
}
