using Microsoft.AspNetCore.Http;
using System.Text;

namespace Tool
{
    public class FileUtils
    {
        static FileUtils()
        {
            // 如果需要日志记录，可以在这里初始化日志记录器
            // _logger = ...;
        }

        // 文件操作
        #region 文件操作

        /// <summary>
        /// 移动文件
        /// </summary>
        public static (bool success, string message) MoveFile(string sourcePath, string destinationPath, bool overwrite = false)
        {
            try
            {
                if (!File.Exists(sourcePath))
                {
                    return (false, $"源文件不存在: {sourcePath}");
                }

                if (File.Exists(destinationPath) && !overwrite)
                {
                    return (false, $"目标文件已存在: {destinationPath}");
                }

                // 确保目标目录存在
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

                File.Move(sourcePath, destinationPath, overwrite);
                return (true, $"文件已成功移动: {sourcePath} -> {destinationPath}");
            }
            catch (Exception ex)
            {

                LoggerUtils.Error(ex.ToString(), typeof(FileUtils));

                return (false, $"移动文件时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 复制文件
        /// </summary>
        public static (bool success, string message) CopyFile(string sourcePath, string destinationPath, bool overwrite = false)
        {
            try
            {
                if (!File.Exists(sourcePath))
                {
                    return (false, $"源文件不存在: {sourcePath}");
                }

                if (File.Exists(destinationPath) && !overwrite)
                {
                    return (false, $"目标文件已存在: {destinationPath}");
                }

                // 确保目标目录存在
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

                File.Copy(sourcePath, destinationPath, overwrite);
                return (true, $"文件已成功复制: {sourcePath} -> {destinationPath}");
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(FileUtils));
                return (false, $"复制文件时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        public static (bool success, string message) DeleteFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return (false, $"文件不存在: {filePath}");
                }

                File.Delete(filePath);
                return (true, $"文件已成功删除: {filePath}");
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(FileUtils));
                return (false, $"删除文件时出错: {ex.Message}");
            }
        }

        #endregion

        // 文件夹操作
        #region 文件夹操作

        /// <summary>
        /// 创建文件夹
        /// </summary>
        public static (bool success, string message) CreateDirectory(string directoryPath)
        {
            try
            {
                if (Directory.Exists(directoryPath))
                {
                    return (false, $"文件夹已存在: {directoryPath}");
                }

                Directory.CreateDirectory(directoryPath);
                return (true, $"文件夹已成功创建: {directoryPath}");
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(FileUtils));
                return (false, $"创建文件夹时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 移动文件夹
        /// </summary>
        public static (bool success, string message) MoveDirectory(string sourcePath, string destinationPath)
        {
            try
            {
                if (!Directory.Exists(sourcePath))
                {
                    return (false, $"源文件夹不存在: {sourcePath}");
                }

                if (Directory.Exists(destinationPath))
                {
                    return (false, $"目标文件夹已存在: {destinationPath}");
                }

                // 确保目标目录的父目录存在
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

                Directory.Move(sourcePath, destinationPath);
                return (true, $"文件夹已成功移动: {sourcePath} -> {destinationPath}");
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(FileUtils));
                return (false, $"移动文件夹时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 复制文件夹（递归）
        /// </summary>
        public static (bool success, string message) CopyDirectory(string sourcePath, string destinationPath, bool overwrite = false)
        {
            try
            {
                if (!Directory.Exists(sourcePath))
                {
                    return (false, $"源文件夹不存在: {sourcePath}");
                }

                if (Directory.Exists(destinationPath) && !overwrite)
                {
                    return (false, $"目标文件夹已存在: {destinationPath}");
                }

                // 递归复制文件夹
                CopyDirectoryRecursive(sourcePath, destinationPath, overwrite);
                return (true, $"文件夹已成功复制: {sourcePath} -> {destinationPath}");
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(FileUtils));
                return (false, $"复制文件夹时出错: {ex.Message}");
            }
        }

        // 递归复制文件夹的辅助方法
        private static void CopyDirectoryRecursive(string sourceDir, string destDir, bool overwrite)
        {
            // 创建目标目录
            Directory.CreateDirectory(destDir);

            // 复制文件
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, overwrite);
            }

            // 递归复制子目录
            foreach (var subDir in Directory.GetDirectories(sourceDir))
            {
                var destSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
                CopyDirectoryRecursive(subDir, destSubDir, overwrite);
            }
        }

        /// <summary>
        /// 删除文件夹（递归）
        /// </summary>
        public static (bool success, string message) DeleteDirectory(string directoryPath, bool recursive = true)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    return (false, $"文件夹不存在: {directoryPath}");
                }

                Directory.Delete(directoryPath, recursive);
                return (true, $"文件夹已成功删除: {directoryPath}");
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(FileUtils));
                return (false, $"删除文件夹时出错: {ex.Message}");
            }
        }


        public static void SaveFile(IFormFile file, string fileName)
        {
            var dir = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using (var stream = new FileStream(fileName, FileMode.Create))
            {
                file.CopyToAsync(stream);
            }
        }

        public static byte[] GetFileData(string fileUrl)
        {
            FileStream fileStream = new FileStream(fileUrl, FileMode.Open, FileAccess.Read);
            try
            {
                byte[] array = new byte[fileStream.Length];
                fileStream.Read(array, 0, (int)fileStream.Length);
                return array;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                fileStream?.Close();
            }
        }

        public static byte[] AuthGetFileData(string fileUrl)
        {
            using FileStream fileStream = new FileStream(fileUrl, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            byte[] array = new byte[fileStream.Length];
            using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
            {
                binaryWriter.Write(array);
                binaryWriter.Close();
            }
            return array;
        }

        public static byte[] File2Bytes(string path)
        {
            if (!File.Exists(path))
            {
                return new byte[0];
            }

            FileInfo fileInfo = new FileInfo(path);
            byte[] array = new byte[fileInfo.Length];
            FileStream fileStream = fileInfo.OpenRead();
            fileStream.Read(array, 0, Convert.ToInt32(fileStream.Length));
            fileStream.Close();
            return array;
        }

        public static string GetFileSize(long filesize)
        {
            try
            {
                if (filesize < 0)
                {
                    return "0";
                }

                if (filesize >= 1073741824)
                {
                    return $"{(double)filesize / 1073741824.0:0.00} GB";
                }

                if (filesize >= 1048576)
                {
                    return $"{(double)filesize / 1048576.0:0.00} MB";
                }

                if (filesize >= 1024)
                {
                    return $"{(double)filesize / 1024.0:0.00} KB";
                }

                return $"{filesize:0.00} bytes";
            }
            catch (Exception)
            {
                return "未知";
            }
        }

        public static string GetFileExtension(string filename)
        {
            try
            {
                if (string.IsNullOrEmpty(filename))
                {
                    return "未知";
                }

                return filename.Substring(filename.LastIndexOf('.'));
            }
            catch (Exception)
            {
                return "未知";
            }
        }

        public static void CreateFileFolder(string path)
        {
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static string CreateFileStream(string FilePath)
        {
            if (string.IsNullOrEmpty(FilePath))
            {
                return "";
            }

            string result = "";
            using (FileStream fileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                byte[] array = new byte[fileStream.Length];
                fileStream.Seek(0L, SeekOrigin.Begin);
                fileStream.Read(array, 0, array.Length);
                result = Convert.ToBase64String(array);
            }

            return result;
        }

        public static void CreateFolderFile(string content, string FolderPath, string FilePath)
        {
            if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(FilePath) || string.IsNullOrEmpty(FolderPath))
            {
                return;
            }

            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }

            using FileStream fileStream = new FileStream(FilePath, FileMode.Create);
            byte[] array = Convert.FromBase64String(content);
            fileStream.Seek(0L, SeekOrigin.Begin);
            fileStream.Write(array, 0, array.Length);
        }

        public static void CreateFile(string content, string FilePath)
        {
            if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(FilePath))
            {
                return;
            }

            using FileStream fileStream = new FileStream(FilePath, FileMode.Create);
            byte[] array = Convert.FromBase64String(content);
            fileStream.Seek(0L, SeekOrigin.Begin);
            fileStream.Write(array, 0, array.Length);
        }

        public static void MoveFile(string FileSource, string SavePath, string FileName)
        {
            if (Directory.Exists(SavePath))
            {
                string path = SavePath + "\\" + FileName;
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                File.Move(FileSource, SavePath + "\\" + FileName);
            }
        }

        public static void CopyFile(string FileSource, string SavePath, string FileName)
        {
            if (Directory.Exists(SavePath))
            {
                string path = SavePath + "\\" + FileName;
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                File.Copy(FileSource, SavePath + "\\" + FileName);
            }
        }

        public static void DeleteFolder(string FolderPath)
        {
            if (Directory.Exists(FolderPath))
            {
                Directory.Delete(FolderPath, recursive: true);
            }
        }

        // DeleteFile 已有返回元组版本，此处不再重复添加

        public static string ReadText(string FilePath)
        {
            string result = "";
            using (StreamReader streamReader = new StreamReader(FilePath, Encoding.UTF8))
            {
                result = streamReader.ReadToEnd();
            }

            return result;
        }

        public static void MoveFolder(string sourcePath, string destPath)
        {
            if (Directory.Exists(sourcePath))
            {
                if (!Directory.Exists(destPath))
                {
                    try
                    {
                        Directory.CreateDirectory(destPath);
                    }
                    catch (Exception ex)
                    {
                        LoggerUtils.Error(ex.ToString(), typeof(FileUtils));
                    }
                }

                List<string> list = new List<string>(Directory.GetFiles(sourcePath));
                list.ForEach(delegate (string c)
                {
                    string text = Path.Combine(destPath, Path.GetFileName(c));
                    if (File.Exists(text))
                    {
                        File.Delete(text);
                    }

                    File.Move(c, text);
                });
                List<string> list2 = new List<string>(Directory.GetDirectories(sourcePath));
                list2.ForEach(delegate (string c)
                {
                    string destPath2 = Path.Combine(destPath, Path.GetFileName(c));
                    MoveFolder(c, destPath2);
                });
            }
            else
            {
                LoggerUtils.Error("源目录不存在！", typeof(FileUtils));
            }
        }

        public static string RenameDirectory(string sourcePath, string newName)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                return "文件路径名字是空的";
            }

            if (string.IsNullOrWhiteSpace(newName))
            {
                return "新文件夹名字是空的";
            }

            if (!Directory.Exists(sourcePath))
            {
                return "文件夹路径不存在";
            }

            char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
            if (newName.IndexOfAny(invalidFileNameChars) >= 0)
            {
                string text = string.Join(", ", invalidFileNameChars.Select((char c) => $"[{c}]"));
                return "包含非法字符串";
            }

            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(sourcePath);
                string parentFullName = directoryInfo.Parent.FullName;
                string text2 = Path.Combine(parentFullName, newName);
                if (Directory.Exists(text2))
                {
                    return "目标文件夹已存在";
                }

                Directory.Move(sourcePath, text2);
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(FileUtils));
                return "发生了错误";
            }

            return "";
        }

        #endregion
    }
}
