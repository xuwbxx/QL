using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Web;

namespace Service.Base
{
    public class TcFilter : ActionFilterAttribute
    {
        private CookieService _cookieService { get; }

        public TcFilter(CookieService cookieService)
        {
            _cookieService = cookieService;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // 在 Action 执行之前执行的逻辑
            // 可以在这里进行权限验证、日志记录等操作
            //System.Console.WriteLine("Before action execution");

            try
            {
                var UserInfo = _cookieService.GetUserCookie();
                if (UserInfo == null)
                {

                    string errorText = "身份信息失效，请重新登录,从“交建通工作台”App“技术中心信息平台”进入";
                    string encodedErrorText = HttpUtility.UrlEncode(errorText);


                    context.Result = new RedirectResult("/Tech/Home/Error?ErrorText=" + encodedErrorText);
                    return;
                }
                return;
            }
            catch (Exception)
            {
                string errorText = "身份信息失效，请重新登录,从“交建通工作台”App“技术中心信息平台”进入";
                string encodedErrorText = HttpUtility.UrlEncode(errorText);
                context.Result = new RedirectResult("/Tech/Home/Error?ErrorText=" + encodedErrorText);
                return;
            }

        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            // 在 Action 执行之后执行的逻辑
            // 可以在这里进行响应处理、日志记录等操作
            //if (context.Exception != null)
            //{
            //    // 处理异常
            //    context.Result = new StatusCodeResult(500);
            //    context.ExceptionHandled = true;
            //}
            //System.Console.WriteLine("After action execution");
        }

    }
}
