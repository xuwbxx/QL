using Microsoft.Extensions.Logging;

namespace Tool
{
    public class LoggerUtils
    {
        // 使用字典存储不同日志级别的锁对象，提高并发性能
        private static readonly Dictionary<LogLevel, object> _logLocks = new Dictionary<LogLevel, object>
    {
        { LogLevel.Information, new object() },
        { LogLevel.Error, new object() },
        { LogLevel.Warning, new object() },
        { LogLevel.Debug, new object() },
        { LogLevel.Critical, new object() }
    };

        /// <summary>
        /// 通用日志记录方法（需要显式传入调用方类型）
        /// </summary>
        public static void Log(
            LogLevel logLevel,
            string logMessage,
            Type callerType,
            Exception exception = null,
            [System.Runtime.CompilerServices.CallerMemberName] string callerMemberName = "")
        {
            // 使用调用方传入的类型，不再通过堆栈获取
            Type categoryType = callerType ?? typeof(object);

            // 简化日志目录：只使用Log根目录
            string logDirectory = "Log";

            // 确保日志目录存在
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // 构建日志文件名：级别_日期.log
            var logType = GetLogType(logLevel);
            var logFilePath = Path.Combine(logDirectory, $"{logType}_{DateTime.Now:yyyyMMdd}.log");

            // 获取当前日志级别的专用锁对象
            object lockObj = _logLocks.ContainsKey(logLevel) ? _logLocks[logLevel] : _logLocks[LogLevel.Information];

            // 使用专用锁确保多线程环境下文件写入安全
            lock (lockObj)
            {
                using (var writer = File.AppendText(logFilePath))
                {
                    // 获取命名空间和类名
                    string namespaceName = categoryType.Namespace ?? "UnknownNamespace";
                    string className = categoryType.Name;

                    // 日志格式：[时间] [命名空间.类名.方法名] 消息
                    writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{namespaceName}.{className}.{callerMemberName}] {logMessage}");

                    if (exception != null)
                    {
                        writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{namespaceName}.{className}.{callerMemberName}] [Exception] {exception}");
                        writer.WriteLine(new string('-', 80)); // 分隔线
                    }
                }
            }
        }

        /// <summary>
        /// 快捷方法：记录信息日志（需要传入调用方类型）
        /// </summary>
        public static void Info(
            string logMessage,
            Type callerType,
            [System.Runtime.CompilerServices.CallerMemberName] string callerMemberName = "")
        {
            Log(LogLevel.Information, logMessage, callerType, null, callerMemberName);
        }

        /// <summary>
        /// 快捷方法：记录错误日志（需要传入调用方类型）
        /// </summary>
        public static void Error(
            string logMessage,
            Type callerType,
            Exception exception = null,
            [System.Runtime.CompilerServices.CallerMemberName] string callerMemberName = "")
        {
            Log(LogLevel.Error, logMessage, callerType, exception, callerMemberName);
        }

        private static string GetLogType(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Information:
                    return "Info";
                case LogLevel.Error:
                    return "Error";
                case LogLevel.Warning:
                    return "Warning";
                case LogLevel.Debug:
                    return "Debug";
                case LogLevel.Critical:
                    return "Critical";
                default:
                    return "Other";
            }
        }
    }
}
