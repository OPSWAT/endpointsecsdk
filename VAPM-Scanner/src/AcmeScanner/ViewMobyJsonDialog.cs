using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json.Linq;

namespace AcmeScanner
{
    public partial class ViewMobyJsonDialog : Form
    {

        public ViewMobyJsonDialog(string json)
        {
            InitializeComponent();
            TB_MOBY_JSON.Text = json;
        }

        private void BtnCloseViewJsonDialog_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BtnDownloadMobyJson_Click(object sender, EventArgs e)
        {
            string jsonContent = TB_MOBY_JSON.Text;
            JObject jsonObject = JObject.Parse(jsonContent);
            string defaultFileName = jsonObject["Name"]?.ToString();
            defaultFileName = $"{defaultFileName}.json";

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                saveFileDialog.DefaultExt = "json";
                saveFileDialog.AddExtension = true;
                saveFileDialog.FileName = defaultFileName;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveFileDialog.FileName;

                    try
                    {
                        File.WriteAllText(filePath, jsonContent);
                        MessageBox.Show("JSON file has been saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred while saving the file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }


    }
}
