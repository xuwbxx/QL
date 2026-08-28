using DataFactory.Factory;
using DataFactory.KingBase.CloudWind;
using Microsoft.AspNetCore.Mvc;
using Service.Base;

namespace WindCloud.ViewComponents
{
    public class MenuViewComponent : ViewComponent
    {
        private readonly CloudWind_KingBase_UnitOfWorkFactory _uowFactory;
        private readonly CookieService _cookieService;

        public MenuViewComponent(CloudWind_KingBase_UnitOfWorkFactory uowFactory, CookieService cookieService)
        {
            _uowFactory = uowFactory;
            _cookieService = cookieService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int roleID = 0;

            var user = _cookieService.GetUserCookie();
            if (user != null && !string.IsNullOrEmpty(user.UserCode))
            {
                using (var uow = _uowFactory.Create())
                {
                    var adminRepo = uow.GetRepository<Manage_Admin>();
                    var contacterRepo = uow.GetRepository<Wind_ProjectContacter>();

                    var isAdmin = adminRepo.Find(a => !a.IsDelete && a.UserCode == user.UserCode).Any();
                    if (isAdmin)
                    {
                        roleID = 1;
                    }
                    else
                    {
                        var isDirector = contacterRepo.Find(a => !a.IsDelete && a.DirectorCode == user.UserCode).Any();
                        if (isDirector)
                        {
                            roleID = 2;
                        }
                    }
                }
            }

            return View(roleID);
        }
    }
}
