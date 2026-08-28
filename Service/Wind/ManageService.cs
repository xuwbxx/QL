using DataFactory.Factory;
using DataFactory.KingBase.CloudWind;
using Tool;

namespace Service.Wind
{
    public class ManageService
    {

        // 依赖注入：仓储工厂（核心）+ 日志（可选，用于异常追踪）
        private readonly CloudWind_KingBase_UnitOfWorkFactory _windUowFactory;

        public ManageService(CloudWind_KingBase_UnitOfWorkFactory windCenterUowFactory)
        {
            _windUowFactory = windCenterUowFactory;
        }

        #region 示例1：操作CloudWind数据库的Users表（KingBase）
        /// <summary>
        /// 根据ID获取CloudWind数据库的用户
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>用户实体</returns>
        public async Task<List<WindDbUsers>?> GetCloudWindUserByIdAsync(int userId)
        {
            try
            {
                using (var repo = _windUowFactory.Create())
                {
                    var userRepo = repo.GetRepository<WindDbUsers>();

                    var data = await userRepo.FindAllAsync();

                    return data.ToList();
                }

            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(ManageService));
                return null;
            }
        }

        #endregion


    }
}
