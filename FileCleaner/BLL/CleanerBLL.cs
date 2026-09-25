using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WinformLib;

namespace FileCleaner.BLL
{
    internal class CleanerBLL
    {
        /// <summary>
        /// 清空用户Temp目录下全部内容，跳过占用/无权限的项
        /// </summary>
        public static void ClearUserTempDirectory(Form form,Action<string,decimal> funs)
        {
            string tempPath = Path.GetTempPath();
            var allFiles = Directory.EnumerateFileSystemEntries(tempPath);
            var allChunkList = allFiles.Chunk(allFiles.Count()/10).ToList();
            int index = 0;
            foreach (var allList in allChunkList)
            {
                index++;
                foreach (var item in allList)
                {
                    try
                    {
                        if (File.Exists(item))
                        {
                            File.Delete(item);
                        }
                        else
                        {
                            Directory.Delete(item, true);
                        }
                    }
                    catch (IOException)
                    {
                        // 文件被占用，跳过
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // 权限不足，跳过
                    }
                }
                int showIndex = Math.Min(index, 10);
                if(index == allChunkList.Count)
                {
                    form.UISafeInvoke(() =>
                    {
                        ClearDownloadFolder();
                        int ret = EmptyRecycleBin(form.Handle);
                    });
                }
                funs($"当前进度:{showIndex * 10}%", showIndex * 10);
            }
            
        }


        #region 清空回收站
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHEmptyRecycleBin(IntPtr hWnd, string pszRootPath, int dwFlags);

        // 标志：不确认、不显示进度窗口、不播放声音
        private const int SHERB_NOCONFIRMATION = 0x00000001;
        private const int SHERB_NOPROGRESSUI = 0x00000002;
        private const int SHERB_NOSOUND = 0x00000004;

        /// <summary>
        /// 清空回收站
        /// </summary>
        /// <param name="hwnd">父窗体句柄，传this.Handle</param>
        /// <returns>0=成功</returns>
        public static int EmptyRecycleBin(IntPtr hwnd)
        {
            int flags = SHERB_NOCONFIRMATION | SHERB_NOPROGRESSUI | SHERB_NOSOUND;
            // pszRootPath="" 代表清空全部驱动器回收站
            return SHEmptyRecycleBin(hwnd, "", flags);
        }
        #endregion

        #region 清空下载文件夹
        /// <summary>
        /// 清空当前用户下载文件夹
        /// </summary>
        public static void ClearDownloadFolder()
        {
            string downloadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            if (!Directory.Exists(downloadPath))
            {
                return;
            }
            foreach (var file in Directory.GetFiles(downloadPath))
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                    //跳过占用、权限不足文件
                }
            }
            foreach (var dir in Directory.GetDirectories(downloadPath))
            {
                try
                {
                    Directory.Delete(dir, true);
                }
                catch
                {
                    //跳过占用、权限不足目录
                }
            }
        }
        #endregion
    }
}
