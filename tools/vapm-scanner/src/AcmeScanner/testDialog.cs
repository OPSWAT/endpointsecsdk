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
    }
}
