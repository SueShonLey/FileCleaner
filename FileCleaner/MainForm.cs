using FileCleaner.BLL;
using System.Collections.Generic;
using System.Diagnostics;
using WinformLib;

namespace FileCleaner
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            //常规设置
            this.SetCommon(new FormSettings
            {
                isExitAsk = false,
            });
        }
        List<string> DiskList =     new List<string>();
        private void Form1_Load(object sender, EventArgs e)
        {
            //工具条
            DiskList = DriveInfo.GetDrives().Select(d => d.Name.TrimEnd('\\')).ToList();
            this.SetStatusStripCommon("初始化完成");
        }

        /// <summary>
        /// 打开资源监视器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            Process.Start("resmon.exe");
            this.SetStatusStripTextAndRate("已申请打开资源监视器，请稍后！");
            progressBar1.SetCommon(100);
        }

        /// <summary>
        /// 文件管理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            string tips = "选择盘符:";
            var dict = this.SetCustomizeForms(new CustomizeFormsExtentions.CustomizeFormInput
            {
                FormTitle = "请选择",
                inputs = new List<CustomizeFormsExtentions.CustomizeValueInput>
                {

                    new CustomizeFormsExtentions.CustomizeValueInput
                    {
                        Label = tips,
                        Value = DiskList,
                        FormControlType = CustomizeFormsExtentions.FormControlType.DropDown
                    }
                },
                funsForm = (a) =>
                {
                    a.TopMost = true;
                }
            });
            if (dict.Count != 0)
            {
                var disk = dict[tips];
                this.ShowOnlyOne(new FileManageForm(disk));
                this.SetStatusStripTextAndRate("已打开文件管理！");
                progressBar1.SetCommon(100);
            }
        }

        /// <summary>
        /// 清空用户Temp目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            try
            {
                //C盘清理
                this.TaskRun(() =>
                {
                    CleanerBLL.ClearUserTempDirectory(this,(txt, rate) => this.UISafeInvoke(() =>
                    {
                        this.SetStatusStripTextAndRate(txt, null);
                        progressBar1.SetCommon(Math.Min(Convert.ToInt32(rate),100));
                    }));
                    this.UISafeInvoke(() =>
                    {
                        button1.Enabled = true;
                    });
                });
            }
            catch (Exception ex)
            {
                this.PopUpTips("出错了："+ex.ToString());
                button1.Enabled = true;
            }
        }
    }
}
