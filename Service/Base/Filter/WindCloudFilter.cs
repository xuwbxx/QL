using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Web;

namespace Service.Base.Filter
{
    public class WindCloudFilter : ActionFilterAttribute
    {

        private CookieService _cookieService { get; }

        public WindCloudFilter(CookieService cookieService)
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

                    string errorText = "身份信息失效，请重新登录。";
                    string encodedErrorText = HttpUtility.UrlEncode(errorText);


                    context.Result = new RedirectResult("/Cloud/CloudBase/ErrorPage");
                    return;
                }
                return;
            }
            catch (Exception)
            {
                string errorText = "身份信息失效，请重新登录,从“交建通工作台”App“技术中心信息平台”进入";
                string encodedErrorText = HttpUtility.UrlEncode(errorText);
                context.Result = new RedirectResult("/Base/System/Login");
                return;
            }

        }

    }
}
