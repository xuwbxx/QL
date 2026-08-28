using DataFactory.Factory;
using DataFactory.KingBase;
using Model.TechCenter;
using Tool;

namespace Service.TechCenter
{
    public class TC_DepartService
    {

        // 依赖注入：仓储工厂（核心）+ 日志（可选，用于异常追踪）
        private readonly TechCenter_KingBase_UnitOfWorkFactory _techCenterUowFactory;

        public TC_DepartService(TechCenter_KingBase_UnitOfWorkFactory techCenterUowFactory)
        {
            _techCenterUowFactory = techCenterUowFactory;
        }

        public List<TC_DepartInfo> GetDepartInfo()
        {
            List<TC_DepartInfo> list = new List<TC_DepartInfo>();
            try
            {

                using (var uow = _techCenterUowFactory.Create())
                {
                    // 获取软件仓储
                    var softwareRepo = uow.GetRepository<TechCenter_Manage_Software>();
                    // 获取部门仓储
                    var departRepo = uow.GetRepository<TechCenter_Manage_Depart>();

                    // 查询所有未删除的软件
                    var softwares = softwareRepo.Find(a => !a.IsDelete).ToList();

                    // 查询所有未删除的部门，并关联软件
                    departRepo.Find(a => !a.IsDelete).ToList().ForEach(a =>
                    {
                        TC_DepartInfo depart = new TC_DepartInfo();
                        depart.DepartID = a.ID;
                        depart.DepartName = a.Name;
                        softwares.Where(b => b.DepartID == a.ID).ToList().ForEach(b =>
                        {
                            depart.Softwares.Add(new TC_DepartSoftware()
                            {
                                SoftwareID = b.ID,
                                SoftwareName = b.Name,
                                Img = b.Img ?? "",
                                UseTime = b.UseTime ?? "",
                                Info = b.Info ?? "",
                                Manager = b.Manager ?? ""
                            });
                        });
                        list.Add(depart);
                    });

                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(DataLoginService));
                list = new List<TC_DepartInfo>();
            }
            return list;
        }


    }
}
