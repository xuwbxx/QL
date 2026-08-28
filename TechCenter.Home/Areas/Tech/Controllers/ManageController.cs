using Microsoft.AspNetCore.Mvc;
using Service.Base;
using Service.Test;

namespace TechCenter.Home.Areas.Tech.Controllers
{
    [Area("Tech")]
    public class ManageController : BaseController
    {
        private UserService _userService { get; }
        public ManageController(CookieService cookieService, UserService userService) : base(cookieService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {



            return View();
        }
    }
}
