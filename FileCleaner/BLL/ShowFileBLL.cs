using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCleaner.BLL
{
    public class ShowFileBLL
    {
        /// <summary>
        /// 获取文件夹下的文件或文件夹信息
        /// </summary>
        /// <param name="folderPath">目标文件夹路径</param>
        /// <returns>一级目录下文件与文件夹实体集合</returns>
        /// <exception cref="DirectoryNotFoundException">文件夹不存在抛出异常</exception>
        public static List<EasyFileDtoList> GetDirectoryItems(string folderPath)
        {
            if (!Directory.Exists(folderPath)) throw new DirectoryNotFoundException("指定文件夹不存在");
            var result = new List<EasyFileDtoList>();
            foreach (var item in Directory.EnumerateFileSystemEntries(folderPath))
            {
                try
                {
                    if (File.Exists(item))
                    {
                        var fi = new FileInfo(item);
                        if ((fi.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden) continue;
                        decimal mb = Math.Round((decimal)fi.Length / 1024 / 1024, 3);
                        result.Add(new EasyFileDtoList
                        {
                            fileName = fi.Name,
                            path = fi.FullName,
                            isFile = true,
                            size = mb,
                            UpdateTime = fi.LastWriteTime
                        });
                    }
                    else
                    {
                        var di = new DirectoryInfo(item);
                        if ((di.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden || (di.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                            continue;
                        long dirSize = 0;
                        try
                        {
                            foreach (var f in di.EnumerateFiles("*", SearchOption.AllDirectories))
                            {
                                try
                                {
                                    dirSize += f.Length;
                                }
                                catch (UnauthorizedAccessException) { }
                            }
                        }
                        catch (UnauthorizedAccessException) { }
                        decimal mb = Math.Round((decimal)dirSize / 1024 / 1024, 3);
                        result.Add(new EasyFileDtoList
                        {
                            fileName = di.Name,
                            path = di.FullName,
                            isFile = false,
                            size = mb,
                            UpdateTime = di.LastWriteTime
                        });
                    }
                }
                catch (UnauthorizedAccessException) { }
            }
            return result;
        }

        /// <summary>
        /// 文件/目录信息实体
        /// </summary>
        public class EasyFileDtoList
        {
            /// <summary>
            /// 文件或文件夹名称
            /// </summary>
            public string fileName { get; set; }
            /// <summary>
            /// 文件或文件夹完整绝对路径
            /// </summary>
            public string path { get; set; }
            /// <summary>
            /// 是否为文件，false代表文件夹
            /// </summary>
            public bool isFile { get; set; }
            /// <summary>
            /// 大小，单位MB，保留3位小数
            /// </summary>
            public decimal size { get; set; }
            /// <summary>
            /// 最后修改时间
            /// </summary>
            public DateTime UpdateTime { get; set; }

            public string sizeStr => $"{Math.Round(size/1024,2)}GB";
        }
    }
}
