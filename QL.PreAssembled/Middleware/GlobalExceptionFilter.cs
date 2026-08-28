using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;
using System.Net;
using Tool;

namespace QL.PreAssembled.Middleware
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var exception = context.Exception;

            // ============ 核心修正 ============
            // 1. 获取原始报错的方法名和类名（从堆栈追踪中提取）
            string sourceMethod = "Unknown";
            string sourceClass = "Unknown";

            // 尝试从堆栈中提取真正的源头
            var stackTrace = new StackTrace(exception, true);
            var frames = stackTrace.GetFrames();
            if (frames != null && frames.Length > 0)
            {
                var firstFrame = frames.FirstOrDefault(f => f.GetMethod() != null);
                if (firstFrame != null)
                {
                    var method = firstFrame.GetMethod();
                    sourceMethod = method.Name;
                    sourceClass = method.DeclaringType?.Name ?? "Unknown";
                }
            }

            // 2. 构建一个精确描述“哪里报错了”的消息
            // 格式：[源头类名.源头方法名] 错误消息 + 完整的堆栈详情（方便细查）
            string detailedLogMessage = $"[{sourceClass}.{sourceMethod}] 异常触发: {exception.Message}";

            // 3. 调用你的日志工具（将真正出错的源头名字作为消息前缀传入）
            // 注意：callerType 依然传 GlobalExceptionFilter 本身，因为这确实是捕获它的位置
            // 但我们在消息体里把真正的源头剖析出来了
            LoggerUtils.Error(
                logMessage: detailedLogMessage,
                callerType: typeof(GlobalExceptionFilter),
                exception: exception // 传入完整的 exception，你的工具会打印异常详情
            );
            // ==================================

            // 构建前端返回的 JSON
            var result = new
            {
                code = "500",
                success = false,
                message = exception.Message
            };

            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.ExceptionHandled = true;
            context.Result = new JsonResult(result);
        }
    }
}
