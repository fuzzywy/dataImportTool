using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    class MyFile
    {
        public FileStream CreateFileStream(string folderPath, string fileName)
        {
            try
            {
                lock (this)
                {
                    // 检查文件夹
                    //string folderPath = @"C:\Logs";
                    if (false == Directory.Exists(folderPath)) {
                        //创建文件夹
                        Directory.CreateDirectory(folderPath);
                    }
                    if (Directory.Exists(folderPath)) {
                        //存在/成功创建 文件夹
                        folderPath = folderPath + @"\";
                    } else {
                        //无则当前路径创建文件
                        folderPath = folderPath + @"-";
                    }
                    //写入日志 
                    string filePath = folderPath+ fileName;
                    FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                    return fs;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
            
        }

        public List<FileInfo> GetFileNamesFromDir(string dir)
        {
            DirectoryInfo folder = new DirectoryInfo(dir);

            List<FileInfo> fileList = new List<FileInfo>();
            foreach (FileInfo file in folder.GetFiles("*.csv"))
            {
                fileList.Add(file);
            }
            return fileList;
        }
    }
}
