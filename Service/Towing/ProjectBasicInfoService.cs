using Business.Towing;
using DataFactory.Factory;
using DataFactory.KingBase.Towing;
using Tool;

namespace Service.Towing
{
    public class ProjectBasicInfo
    {
        public int ProjectID { set; get; }
        public string ProjectName { set; get; }
        public DateTime? ProjectStartTime { set; get; }
        public string MMSI { set; get; }
        public string ShipName { set; get; }
        public decimal? StartLon { set; get; }
        public decimal? StartLat { set; get; }
        public decimal? EndLon { set; get; }
        public decimal? EndLat { set; get; }
    }

    public class ProjectBasicInfoService
    {
        private readonly Towing_KingBase_UnitOfWorkFactory _towingKingBaseUowFactory;

        public ProjectBasicInfoService(Towing_KingBase_UnitOfWorkFactory towingKingBaseUowFactory)
        {
            _towingKingBaseUowFactory = towingKingBaseUowFactory;
        }

        public ResponseBase<ProjectBasicInfo> GetInfo()
        {
            var res = new ResponseBase<ProjectBasicInfo>();
            try
            {
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var projRepo = uow.GetRepository<Manage_Project>();
                    var basicRepo = uow.GetRepository<Manage_Basic>();
                    var shipRepo = uow.GetRepository<Manage_Ship>();

                    // 取最新的一条记录
                    var query = from p in projRepo.Find(a => !a.IsDelete)
                                from b in basicRepo.Find(a => a.ProjectID == p.id && !a.IsDelete)
                                select new
                                {
                                    p.ProjectName,
                                    b.ProjectID,
                                    b.ProjectStartTime,
                                    b.StartLat,
                                    b.StartLon,
                                    b.EndLat,
                                    b.EndLon,
                                    b.CreateTime
                                };
                    var data = query.FirstOrDefault();
                    res.Data = new ProjectBasicInfo();

                    if (data != null)
                    {
                        res.Data.ProjectName = data.ProjectName;
                        res.Data.ProjectID = data.ProjectID ?? 0;
                        res.Data.ProjectStartTime = data.ProjectStartTime;
                        res.Data.StartLon = data.StartLon ?? 0;
                        res.Data.StartLat = data.StartLat ?? 0;
                        res.Data.EndLon = data.EndLon ?? 0;
                        res.Data.EndLat = data.EndLat ?? 0;
                    }
                    else
                    {
                        res.SetError("尚未有项目信息。");
                    }

                    var ship = shipRepo.Find(a => !a.IsDelete).FirstOrDefault();
                    if (ship != null)
                    {
                        res.Data.ShipName = ship.ShipName;
                        res.Data.MMSI = ship.mmsi;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(ProjectBasicInfoService));
                res.SetError(ex);
            }
            return res;
        }

        public ResponseBase<ProjectBasicInfo> SaveInfo(ProjectBasicInfo info)
        {
            var res = new ResponseBase<ProjectBasicInfo>();
            try
            {
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var projRepo = uow.GetRepository<Manage_Project>();
                    var basicRepo = uow.GetRepository<Manage_Basic>();
                    var shipRepo = uow.GetRepository<Manage_Ship>();

                    // 取最新的一条记录
                    var proj = projRepo.Find(a => a.id == info.ProjectID).FirstOrDefault();
                    if (proj == null)
                    {
                        proj = new Manage_Project();
                        proj.IsDelete = false;
                        proj.CreateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                        projRepo.Add(proj);
                    }
                    proj.ProjectName = info.ProjectName;
                    uow.Save();

                    info.ProjectID = proj.id;

                    var projBasic = basicRepo.Find(a => a.ProjectID == proj.id && !a.IsDelete).FirstOrDefault();
                    if (projBasic == null)
                    {
                        projBasic = new Manage_Basic();
                        projBasic.IsDelete = false;
                        projBasic.CreateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                        basicRepo.Add(projBasic);
                    }
                    projBasic.ProjectStartTime = info.ProjectStartTime;
                    projBasic.StartLat = info.StartLat;
                    projBasic.StartLon = info.StartLon;
                    projBasic.EndLat = info.EndLat;
                    projBasic.EndLon = info.EndLon;
                    uow.Save();

                    var ship = shipRepo.Find(a => !a.IsDelete).FirstOrDefault();
                    if (ship == null)
                    {
                        ship = new Manage_Ship();
                        ship.IsDelete = false;
                        ship.CreateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                        shipRepo.Add(ship);
                    }
                    ship.mmsi = info.MMSI;
                    ship.ShipName = info.ShipName;
                    uow.Save();

                    res.Data = info;
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(ProjectBasicInfoService));
                res.SetError(ex);
            }
            return res;
        }
    }
}
