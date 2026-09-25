using FileCleaner.BLL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinformLib;
using static FileCleaner.BLL.ShowFileBLL;
using static WinformLib.DataGridViewExtentions;

namespace FileCleaner
{
    public partial class FileManageForm : Form
    {
        private string _disk = string.Empty;//当前的文件夹路径
        private string _initdisk = string.Empty;//初始化的文件夹路径
        public FileManageForm(string disk)
        {
            InitializeComponent();
            _disk = disk + "\\";
        }

        private async void FileManageForm_Load(object sender, EventArgs e)
        {
            label2.Text = "【提示】正在扫描,请稍后...";
            label2.ForeColor = Color.Blue;
            await Scan(_disk);
            _initdisk = _disk;
            button1.Enabled = false;
        }

        private async Task Scan(string currentUrl)
        {
            button1.Enabled = false;
            label2.Text = "【提示】正在扫描,请稍后...";
            label2.ForeColor = Color.Blue;
            _disk = currentUrl;//当前url
            linkLabel2.Enabled = false;
            List<ShowFileBLL.EasyFileDtoList> list = new List<ShowFileBLL.EasyFileDtoList>();
            await this.TaskRunWithUIAsync(() =>
            {
                list = ShowFileBLL.GetDirectoryItems(currentUrl)
                                   .Where(x => x.size > 1024)
                                   .OrderByDescending(x => x.size)
                                   .ToList();
            }, () =>
            {
                if (!list.Any())
                {
                    label2.Text = $"【扫描结束】该磁盘下没有大于1GB空间的文件/文件夹。";
                    label2.ForeColor = Color.Red;
                    dataGridView1.Rows.Clear();
                    button1.Enabled = true;
                    linkLabel2.Enabled = true;
                    return;
                }
                dataGridView1.SetCommonWithCell(new DataGridViewExtentions.DataDisplayEntityCell<EasyFileDtoList>
                {
                    DataList = list,
                    ButtonList = new List<(string ButtonName, string TitileName, int Width)>
                    {
                        ("扫描","操作",60),
                        ("打开","操作",60),
                    },
                    HeadtextList = new List<(System.Linq.Expressions.Expression<Func<EasyFileDtoList, object>> Feild, string TitileName, int Width)>
                    {
                        (x=>x.fileName,"文件名称",230),
                        (x=>x.sizeStr,"大小",100),
                        (x=>x.UpdateTime,"修改时间",180),
                    },
                    RowAction = (dto, row) =>
                    {
                        if (!dto.isFile)
                        {
                            row.DefaultCellStyle = new DataGridViewCellStyle
                            {
                                ForeColor = Color.Blue,
                            };
                        }
                        else
                        {
                            row.Cells["扫描"] = new DataGridViewTextBoxCell();
                            row.Cells["打开"] = new DataGridViewTextBoxCell();
                            row.ReadOnly = true;
                        }
                    }
                });
                label2.Text = $"【扫描结束】{currentUrl}";
                label2.ForeColor = Color.Blue;
                button1.Enabled = true;
                linkLabel2.Enabled = true;
            });
        }

        private async void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var scan = dataGridView1.GetCommonByButton<EasyFileDtoList>("扫描", e);
            var open = dataGridView1.GetCommonByButton<EasyFileDtoList>("打开", e);
            if (scan != null)
            {
                if (_disk.Count(c => c == '\\') != 1)
                {
                    await Scan(scan.path);//渲染当前界面
                }
                else
                {
                    //打开新界面
                    var form = new FileManageForm(scan.path);
                    form.Icon = this.Icon;
                    form.StartPosition = this.StartPosition;
                    form.FormBorderStyle = this.FormBorderStyle;
                    form.MaximizeBox = false;
                    form.Show();
                }
            }
            else if (open != null)
            {
                new LinkLabel().OpenLink(open.path, LinkLabel1Extensions.EnumLinkType.Folder);
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.OpenLink(_disk, LinkLabel1Extensions.EnumLinkType.Folder);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.PopUpTips($"【当前路径】{_disk}");
        }

        /// <summary>
        /// 上一页
        /// </summary>
        private async void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            string lastUrl = GetLastUrl();
            try
            {
                await Scan(lastUrl);
            }
            catch (Exception ex)
            {
                this.PopUpTips(ex.ToString());
            }
            finally
            {
                button1.Enabled = _initdisk != lastUrl;//上一个地址不等于初始化地址
            }

        }

        /// <summary>
        /// 获取上一页的地址
        /// </summary>
        /// <returns></returns>
        private string GetLastUrl()
        {
            return _disk.Replace(_disk.Trim(('\\')).Split('\\').LastOrDefault(), "").Trim('\\') + '\\';
        }

        private async void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel2.Enabled = false;
            var btn1Status = button1.Enabled;
            await Scan(_disk);
            linkLabel2.LinkVisited = true;
            button1.Enabled = btn1Status;
            linkLabel2.Enabled = true;
        }
    }
}
