using Microsoft.AspNetCore.Mvc;
using Model.Base;
using Service.Base;

namespace TechCenter.Home.Areas.Tech.Controllers
{
    public class BaseController : Controller
    {
        public CookieService _cookieService { get; }

        public BaseController(CookieService cookieService)
        {
            _cookieService = cookieService;
        }

        public SHJUserInfo CurrentUser
        {
            get
            {
                try
                {
                    return _cookieService.GetUserCookie();

                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

    }
}
