using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tool.Towing
{
    public static class FileHelper
    {
        public static bool IsWindowsRuntime()
        {
            // 获取当前操作系统的版本信息
            OperatingSystem os = Environment.OSVersion;

            // 判断操作系统是否为 Windows
            bool isWindows = os.Platform == PlatformID.Win32NT ||
                             os.Platform == PlatformID.Win32S ||
                             os.Platform == PlatformID.Win32Windows;
            return isWindows;
        }
        public static bool IsLinuxRuntime()
        {
            return !IsWindowsRuntime();
        }
        public static string ServerRoot { get; private set; }
        public static string WWWRoot { get; private set; }
        public static string GetRuntimePath(params string[] path)
        {
            if (path.Length == 0) return "";
            var pathStr = path.Length == 1 ? path[0] : PathCombine(path);
            pathStr = pathStr ?? "";
            if (IsWindowsRuntime())
            {
                pathStr = pathStr.Replace("/", "\\");
            }
            if (IsLinuxRuntime())
            {
                pathStr = pathStr.Replace("\\", "/");
                if (pathStr.StartsWith("/") == false) pathStr = "/" + pathStr;
            }
            return pathStr;
        }
        public static void SetServerRoot(string serverRoot, string wwwfolder = "wwwroot")
        {
            serverRoot = (serverRoot ?? "").Replace("/", "\\");
            wwwfolder = (wwwfolder ?? "").Replace("/", "\\");
            serverRoot = ((serverRoot ?? "") + "\\").Replace("\\\\", "\\");
            wwwfolder = ((wwwfolder ?? "") + "\\").Replace("\\\\", "\\");
            if (serverRoot.EndsWith(wwwfolder))
            {
                ServerRoot = ServerRoot.Substring(0, ServerRoot.Length - wwwfolder.Length);
            }
            else
            {
                ServerRoot = serverRoot;
            }
            WWWRoot = PathCombine(serverRoot, wwwfolder);
        }
        //public static string FileRoot
        //    => GetRuntimePath(ConfigMgmt.GetRoot().GetValue<string>("fileRoot"));
        //public static string GetFolder(string category)
        //{
        //    var root = FileRoot;
        //    var timeSuffix = DateTime.Now.ToString("yyyyMMdd");
        //    root = GetValidPathString(GetServerFolder(root, string.IsNullOrWhiteSpace(category) ? "Others" : category,
        //        timeSuffix));
        //    lock (new { root = root })
        //    {
        //        if (Directory.Exists(root) == false)
        //        {
        //            Directory.CreateDirectory(root);
        //        }
        //    }
        //    return GetRuntimePath(root);
        //}
        public static string PathCombine(params string[] paths)
        {
            var path = new List<string>();
            foreach (var p in paths)
            {
                path.AddRange((p ?? "").Replace("/", "\\").Split('\\').Where(a => string.IsNullOrWhiteSpace(a) == false));
            }
            var parr = path.ToArray();
            var pstr = string.Join("\\", path.ToArray());
            if (paths.Length > 0)
            {
                if (paths[0].StartsWith("\\"))
                {
                    pstr = "\\" + pstr;
                }
            }
            return GetRuntimePath(pstr);
        }
        public static string FileExtName(string fileName)
        {
            return Path.GetExtension(fileName);
        }
        public static bool Exits(string fileName)
        {
            return File.Exists(GetRuntimePath(fileName));
        }
        public static string GetServerFolder(params string[] paths)
        {
            var p = new List<string>();
            p.Add(FileHelper.ServerRoot);
            p.AddRange(paths);
            return PathCombine(p.ToArray());
        }
        public static Dictionary<string, string> FileContentTypes { set; get; }

        public static string GetFileContentType(string fileName)
        {
            var exts = fileName.Split('.').LastOrDefault();
            var contentType = "application/octet-stream";
            if (exts == null) { exts = string.Empty; }
            if (FileContentTypes != null && FileContentTypes.ContainsKey(exts.ToLower()))
            {
                contentType = FileContentTypes[exts];
            }
            return contentType;
        }

        public static string AppendStampToFileName(string fileName, string stamp = "")
        {
            if (string.IsNullOrEmpty(stamp)) stamp = DateTime.Now.ToString("yyyyMMdd.HHmmss");
            if (string.IsNullOrWhiteSpace(stamp) == false) stamp = stamp.Insert(0, ".");
            return string.Concat(Path.GetFileNameWithoutExtension(fileName), stamp, Path.GetExtension(fileName));
        }

        public static string ReplaceInvalidPathCharacter(string sourcePath, char replaceChar = '_')
        {
            var invalidCharacters = new List<char>() { '&', '+', ' ', '#', '%', '￥' };
            invalidCharacters = invalidCharacters.Union(Path.GetInvalidPathChars()).ToList();
            if (invalidCharacters.Contains(replaceChar)) replaceChar = '_';
            foreach (var cr in invalidCharacters)
            {
                sourcePath = sourcePath.Replace(cr, replaceChar);
            }

            return sourcePath;
        }
        public static string GetValidPathString(string str)
        {
            string regexSearch = new string(Path.GetInvalidPathChars());// + new string(Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return GetRuntimePath(r.Replace(str, string.Empty));
        }
        public static string GetLeafFileName(string fileName)
        {
            var f = new FileInfo(fileName);
            return f.Name;
        }

        /// <summary>
        /// Stream to byte[]
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static byte[] StreamToBytes(Stream stream)
        {
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            // 设置当前流的位置为流的开始
            stream.Seek(0, SeekOrigin.Begin);
            return bytes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static byte[] ToBytes(this Stream self)
            => StreamToBytes(self);

        /// <summary>
        /// byte[] to Stream
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static Stream BytesToStream(byte[] bytes)
        {
            Stream stream = new MemoryStream(bytes);
            return stream;
        }
        public static Stream ReadFileToStream(string filePath)
        {
            var s = default(Stream);
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open))
                {
                    s = fs;
                    s.ToBytes();
                    return fs;
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(FileHelper));
                throw ex;
            }
            return s;
        }

        public static string StreamToBase64(Stream s)
        {
            string base64 = "";
            try
            {
                if (s.Length > int.MaxValue)
                {
                    throw new Exception("当前流超出了2G大小。");
                }
                s.Seek(0, SeekOrigin.Begin);
                byte[] b = new byte[s.Length];
                int length = Convert.ToInt32(s.Length);
                s.Read(b, 0, length);
                base64 = Convert.ToBase64String(b);
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(FileHelper));
                throw ex;
            }
            return base64;
        }
        public static string Save(string fileFullName, string str)
        {
            fileFullName = fileFullName ?? "";
            if (fileFullName.ToLower().IndexOf(FileHelper.ServerRoot.ToLower()) == -1)
            {
                fileFullName = FileHelper.PathCombine(FileHelper.ServerRoot, fileFullName);
            }
            var fileInfo = new FileInfo(fileFullName);
            if (fileInfo.Directory.Exists == false) fileInfo.Directory.Create();
            System.IO.File.WriteAllText(fileFullName, str);
            return fileFullName;
        }
        public static string Save(string fileFullName, byte[] bytes)
        {
            fileFullName = fileFullName ?? "";
            if (fileFullName.ToLower().IndexOf(FileHelper.ServerRoot.ToLower()) == -1)
            {
                fileFullName = FileHelper.PathCombine(FileHelper.ServerRoot, fileFullName);
            }
            var fileInfo = new FileInfo(fileFullName);
            if (fileInfo.Directory.Exists == false) fileInfo.Directory.Create();
            System.IO.File.WriteAllBytes(fileFullName, bytes);
            return fileFullName;
        }
        //public static string Save(string fileFullName, Bitmap img)
        //{
        //    fileFullName = FileHelper.PathCombine(FileHelper.ServerRoot, fileFullName);
        //    var fileInfo = new FileInfo(fileFullName);
        //    if (fileInfo.Directory.Exists == false) fileInfo.Directory.Create();
        //    var ms = new MemoryStream();
        //    try
        //    {
        //        var nb = new Bitmap(img);
        //        nb.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
        //        System.IO.File.WriteAllBytes(fileFullName, ms.ToBytes());
        //    }
        //    catch (Exception ex)
        //    {
        //        var e32 = Marshal.GetLastWin32Error();

        //    }
        //    finally
        //    {
        //        ms.Dispose();
        //    }
        //    return fileFullName;
        //}
    }
}
