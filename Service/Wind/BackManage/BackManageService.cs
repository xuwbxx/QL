using DataFactory.Factory;
using DataFactory.KingBase.CloudWind;

namespace Service.Wind.BackManage
{
    public class BackManageService
    {
        private readonly CloudWind_KingBase_UnitOfWorkFactory _techCenterUowFactory;

        public BackManageService(CloudWind_KingBase_UnitOfWorkFactory techCenterUowFactory)
        {
            _techCenterUowFactory = techCenterUowFactory;
        }

        /// <summary>
        /// 判断当前用户是否是超级管理员或项目经理
        /// </summary>
        public bool IsAdminOrProjectDirector(string userCode)
        {
            using (var uow = _techCenterUowFactory.Create())
            {
                var adminRepo = uow.GetRepository<Manage_Admin>();
                var contacterRepo = uow.GetRepository<Wind_ProjectContacter>();

                var admins = adminRepo.Find(a => !a.IsDelete).ToList();
                var projectDirectors = contacterRepo.Find(a => !a.IsDelete).ToList();

                if (admins.Exists(a => a.UserCode.Equals(userCode)) || projectDirectors.Exists(a => a.DirectorCode.Equals(userCode)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
