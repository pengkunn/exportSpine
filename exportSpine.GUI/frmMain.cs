using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using exportSpine.Core;

namespace exportSpine.GUI
{
    public partial class frmMain : Form
    {
        dynamic _userData = null;

        public frmMain()
        {
            InitializeComponent();
        }

        private void btnSourceFolder_Click(object sender, EventArgs e)
        {
            var ret = folderBrowserDialog1.ShowDialog();
            if (ret == DialogResult.OK)
            {
                txtSourceFolder.Text = folderBrowserDialog1.SelectedPath;

                var sourceFolder = txtSourceFolder.Text;
                var spineFiles = Directory.GetFiles(sourceFolder, "*.spine", SearchOption.AllDirectories);


                List<bindData> list = new List<bindData>();
                //var arr = new ArrayList();
                foreach (var spineFileName in spineFiles)
                {              
                    var directoryFullName = Path.GetDirectoryName(spineFileName);
                    var directoryName = directoryFullName.Remove(0, directoryFullName.LastIndexOf('\\') + 1);

                    string settingFileFullName = Path.Combine(directoryFullName, "spinesetting.json");
                    string strContent = File.ReadAllText(settingFileFullName);

                    exportSetting setting = new exportSetting();
                    setting.parse(strContent);
                    //setting.scale[0] = 0.5f;
                    //string sd = setting.convertToString();
                    //File.WriteAllText(settingFileFullName, sd);

                    bindData spineFileInfo = new bindData()
                    { 
                        name = directoryName, 
                        spineFileName = spineFileName,
                        scale1 = setting.scale[0].ToString(),
                        scale2 = setting.scale.Count() > 1 ? setting.scale[1].ToString() : "",
                        scale3 = setting.scale.Count() > 2 ? setting.scale[2].ToString() : "",
                        scaleSuffix1 = setting.scaleSuffix.Count() > 0 ? setting.scaleSuffix[0] : "",
                        scaleSuffix2 = setting.scaleSuffix.Count() > 1 ? setting.scaleSuffix[1] : "",
                        scaleSuffix3 = setting.scaleSuffix.Count() > 2 ? setting.scaleSuffix[2] : "",
                        setting = setting
                    };
                    list.Add(spineFileInfo);
                }


                dataGridView1.DataSource = list;
                dataGridView1.ReadOnly = false;

                _userData.lastSourceFolder = folderBrowserDialog1.SelectedPath;
            }

        }

        private void btnExportFolder_Click(object sender, EventArgs e)
        {
            var ret = folderBrowserDialog2.ShowDialog();
            if (ret == DialogResult.OK)
            {
                txtExportFolder.Text = folderBrowserDialog2.SelectedPath;
                _userData.lastOutputFolder = folderBrowserDialog2.SelectedPath;
            }
                
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            var outputFolder = txtExportFolder.Text;

            foreach (DataGridViewRow row in dataGridView1.SelectedRows)
            {
                bindData s = (bindData)row.DataBoundItem;
                labStatus.Text = "开始导出:" + s.name;

                IList<float> listScale = new List<float>();
                if (s.scale1 != "") 
                {
                    listScale.Add(float.Parse(s.scale1));
                }

                if (s.scale2 != "")
                {
                    listScale.Add(float.Parse(s.scale2));
                }

                if (s.scale3 != "")
                {
                    listScale.Add(float.Parse(s.scale3));
                }
                s.setting.scale = listScale.ToArray<float>();

                IList<string> listScaleSuffix = new List<string>();
                if (s.scaleSuffix1 != "")
                {
                    listScaleSuffix.Add(s.scaleSuffix1);
                }

                if (s.scaleSuffix2 != "")
                {
                    listScaleSuffix.Add(s.scaleSuffix2);
                }

                if (s.scaleSuffix3 != "")
                {
                    listScaleSuffix.Add(s.scaleSuffix3);
                }
                s.setting.scaleSuffix = listScaleSuffix.ToArray<string>();


                string strSetting = s.setting.convertToString();
                var directoryFullName = Path.GetDirectoryName(s.spineFileName);
                string settingFileFullName = Path.Combine(directoryFullName, "spinesetting.json");
                File.WriteAllText(settingFileFullName, strSetting);

                var errorInfo = spineExporter.export(s.spineFileName, "spinesetting.json", outputFolder);
                if (errorInfo != "")
                {
                    MessageBox.Show("错误信息：" + errorInfo);
                    return;
                }
                labStatus.Text = "结束导出:" + s.name;
            }

            MessageBox.Show("导出完成");
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            string strUserData = @"{'lastSourceFolder': '','lastOutputFolder': ''}";
            if (!File.Exists("userData.json")) 
            {
                File.AppendAllText("userData.json", strUserData);
            }
            else
            {
                strUserData = File.ReadAllText("userData.json");    
            }
            
            _userData = JsonConvert.DeserializeObject(strUserData);

            txtSourceFolder.Text = _userData.lastSourceFolder;
            txtExportFolder.Text = _userData.lastOutputFolder;


            folderBrowserDialog1.SelectedPath = txtSourceFolder.Text;
            folderBrowserDialog2.SelectedPath = txtExportFolder.Text;
        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_userData != null)
            {
                string strUserData = JsonConvert.SerializeObject(_userData);
                File.WriteAllText("userData.json", strUserData);
            }
        }


        

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            bindData ddd = (bindData)dataGridView1.Rows[e.RowIndex].DataBoundItem;
        }
    }


    public class bindData
    {
        public string name { get; set; }
        public string spineFileName { get; set; }
        public string scale1 { get; set; }
        public string scale2 { get; set; }
        public string scale3 { get; set; }
        public string scaleSuffix1 { get; set; }
        public string scaleSuffix2 { get; set; }
        public string scaleSuffix3 { get; set; }
        public exportSetting setting { get; set; }
    }
}
