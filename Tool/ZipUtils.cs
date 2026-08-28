using ICSharpCode.SharpZipLib.Checksum;
using ICSharpCode.SharpZipLib.Zip;
using System.IO.Compression;

namespace Tool
{
    public static class ZipUtils
    {
        /// <summary>
        /// 压缩文件夹
        /// </summary>
        /// <param name="DirectoryPath">文件夹路径</param>
        /// <param name="FileSavePath">生成文件路径(d:\111\111.zip)</param>
        public static void CompressDirectory(string DirectoryPath, string FileSavePath)
        {
            //创建压缩文件
            FileStream pCompressFile = new FileStream(FileSavePath, FileMode.Create);
            using (ZipOutputStream zipoutputstream = new ZipOutputStream(pCompressFile))
            {
                Crc32 crc = new Crc32();
                Dictionary<string, DateTime> fileList = GetAllFies(DirectoryPath);
                foreach (KeyValuePair<string, DateTime> item in fileList)
                {
                    FileStream fs = new FileStream(item.Key.ToString(), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    ZipEntry entry = new ZipEntry(item.Key.Substring(DirectoryPath.Length));
                    entry.DateTime = item.Value;
                    entry.Size = fs.Length;
                    fs.Close();
                    crc.Reset();
                    crc.Update(buffer);
                    entry.Crc = crc.Value;
                    zipoutputstream.PutNextEntry(entry);
                    zipoutputstream.Write(buffer, 0, buffer.Length);
                }
            }
        }

        /// <summary>
        /// 压缩文件夹
        /// </summary>
        /// <param name="dirPath">要压缩的文件夹路径</param>
        /// <param name="deleteDir">是否删除源文件</param>
        public static void CompressDirectoryExtra(string dirPath, bool deleteDir)
        {
            //压缩文件路径
            string pCompressPath = dirPath + ".zip";
            //创建压缩文件
            FileStream pCompressFile = new FileStream(pCompressPath, FileMode.Create);
            using (ZipOutputStream zipoutputstream = new ZipOutputStream(pCompressFile))
            {
                Crc32 crc = new Crc32();
                Dictionary<string, DateTime> fileList = GetAllFies(dirPath);
                foreach (KeyValuePair<string, DateTime> item in fileList)
                {
                    FileStream fs = new FileStream(item.Key.ToString(), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    ZipEntry entry = new ZipEntry(item.Key.Substring(dirPath.Length));
                    entry.DateTime = item.Value;
                    entry.Size = fs.Length;
                    fs.Close();
                    crc.Reset();
                    crc.Update(buffer);
                    entry.Crc = crc.Value;
                    zipoutputstream.PutNextEntry(entry);
                    zipoutputstream.Write(buffer, 0, buffer.Length);
                }
            }
            if (deleteDir)
            {
                Directory.Delete(dirPath, true);
            }
        }
        /// <summary>
        /// 获取所有文件  
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        /// <exception cref="System.IO.FileNotFoundException"></exception>
        private static Dictionary<string, DateTime> GetAllFies(string dir)
        {
            Dictionary<string, DateTime> FilesList = new Dictionary<string, DateTime>();
            DirectoryInfo fileDire = new DirectoryInfo(dir);
            if (!fileDire.Exists)
            {
                throw new System.IO.FileNotFoundException("目录:" + fileDire.FullName + "没有找到!");
            }
            GetAllDirFiles(fileDire, FilesList);
            GetAllDirsFiles(fileDire.GetDirectories(), FilesList);
            return FilesList;
        }
        /// <summary>
        /// 获取一个文件夹下的所有文件夹里的文件   
        /// </summary>
        /// <param name="dirs">文件夹路径</param>
        /// <param name="filesList"></param>
        private static void GetAllDirsFiles(DirectoryInfo[] dirs, Dictionary<string, DateTime> filesList)
        {
            foreach (DirectoryInfo dir in dirs)
            {
                foreach (FileInfo file in dir.GetFiles("."))
                {
                    filesList.Add(file.FullName, file.LastWriteTime);
                }
                GetAllDirsFiles(dir.GetDirectories(), filesList);
            }
        }
        /// <summary>
        /// 获取一个文件夹下的文件
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="filesList"></param>
        private static void GetAllDirFiles(DirectoryInfo dir, Dictionary<string, DateTime> filesList)
        {
            foreach (FileInfo file in dir.GetFiles())
            {
                filesList.Add(file.FullName, file.LastWriteTime);
            }
        }


        /// <summary>
        /// 多文件压缩
        /// </summary>
        /// <param name="zipPath"></param>
        /// <param name="filesToZip"></param>
        public static void CreateZip(string zipPath, List<FileResponse> filesToZip)
        {
            if (File.Exists(zipPath))
            {
                return;
            }

            using (FileStream zipFile = new FileStream(zipPath, FileMode.Create))
            {
                using (ZipArchive zipArchive = new ZipArchive(zipFile, ZipArchiveMode.Create))
                {
                    foreach (var file in filesToZip)
                    {
                        if (File.Exists(file.FilePath))
                        {
                            ZipArchiveEntry entry = zipArchive.CreateEntryFromFile(file.FilePath, file.FileName);
                            // 你可以在这里添加额外的代码来处理entry，比如设置压缩选项等
                        }
                    }
                }
            }
        }
    }

    public class FileResponse
    {
        public int ID { set; get; }

        public int Type { set; get; }

        public string TypeName { set; get; }

        public string FileName { set; get; }

        public string FilePath { set; get; }

        public int AssID { set; get; }

        public string CreateUser { set; get; }

        public string CreateTime { set; get; }

        public bool CanEdit { set; get; }

        public bool CanDelete { set; get; }

        public bool CanRead { set; get; }

        public bool CanDownload { set; get; }
    }

}
