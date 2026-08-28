using DataFactory.Factory;
using DataFactory.KingBase.CloudWind;

namespace Service.Wind
{
    public class WebValidateService
    {
        // 依赖注入：仓储工厂（核心）+ 日志（可选，用于异常追踪）
        private readonly CloudWind_KingBase_UnitOfWorkFactory _windUowFactory;

        public WebValidateService(CloudWind_KingBase_UnitOfWorkFactory windCenterUowFactory)
        {
            _windUowFactory = windCenterUowFactory;
        }

        public async Task<bool> RightValidate(string UserCode)
        {
            using (var repo = _windUowFactory.Create())
            {
                var manageAdminRepo = repo.GetRepository<Manage_Admin>();

                int adminCount = await manageAdminRepo.FindCountAsync(a => !a.IsDelete && a.UserCode == UserCode);
                if (adminCount > 0)
                {
                    return true;
                }

                var contacterRepo = repo.GetRepository<Wind_ProjectContacter>();
                int contacterCount = await contacterRepo.FindCountAsync(a => !a.IsDelete && a.ApplyerCode == UserCode);
                if (contacterCount > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<List<string>> GetAdminUserCode()
        {
            List<string> admins = new List<string>();

            using (var repo = _windUowFactory.Create())
            {
                var manageAdminRepo = repo.GetRepository<Manage_Admin>();

                manageAdminRepo.Find(a => !a.IsDelete).ToList().ForEach(a =>
                {
                    admins.Add(a.UserCode);
                });


            }
            return admins;
        }

    }
}
