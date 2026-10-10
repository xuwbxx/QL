using Business.Towing;
using DataFactory.Factory;
using Tool;

namespace Service.Towing
{
    
    public class ProjectPlanRouteInfo
    {
        public int DataID { set; get; }
        public int ProjectID { set; get; }
        public decimal Lon { set; get; }
        public decimal Lat { set; get; }
        public DateTime? RouteTime { set; get; }

        public string TimeStr { set; get; }

        public decimal YaoX { set; get; }

        public decimal YaoY { set; get; }

        public decimal DangZ { set; get; }

        /// <summary>
        /// 总报警
        /// </summary>
        public bool TotalAlarm { set; get; }

        public bool WindAlarm { set; get; }

        public string WindAlarmContent
        {
            get
            {
                return WindAlarm ? "风力大于8级" : string.Empty;
            }
        }

        public bool WaveAlarm { set; get; }

        public string WaveAlarmContent
        {
            get
            {
                return WaveAlarm ? "义波高可能超过4米" : string.Empty;
            }
        }

        public bool ShakeAlarm { set; get; }

        public string ShakeAlarmContent
        {
            get
            {
                return ShakeAlarm ? "浮体摇晃可能超过6°" : string.Empty;
            }
        }
    }

    public class ProjectPlanRouteInfoService
    {
        private readonly Towing_KingBase_UnitOfWorkFactory _towingKingBaseUowFactory;

        public ProjectPlanRouteInfoService(Towing_KingBase_UnitOfWorkFactory towingKingBaseUowFactory)
        {
            _towingKingBaseUowFactory = towingKingBaseUowFactory;
        }

        public ResponseBase<ProjectPlanRouteInfo> GetProjectList(int projID = 0)
        {
            var res = new ResponseBase<ProjectPlanRouteInfo>();
            try
            {
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    if (projID == 0)
                    {
                        var projRepo = uow.GetRepository<DataFactory.KingBase.Towing.Manage_Project>();
                        var proj = projRepo.Find(a => true).FirstOrDefault();
                        if (proj != null)
                        {
                            projID = proj.id;
                        }
                    }

                    var planRouteRepo = uow.GetRepository<DataFactory.KingBase.Towing.Manage_PlanRoute>();
                    var list = planRouteRepo.Find(a => a.ProjectID == projID && !a.IsDelete)
                        .OrderBy(a => a.RouteTime)
                        .ToList();

                    var basicRepo = uow.GetRepository<DataFactory.KingBase.Towing.Manage_Basic>();
                    var prjInfo = basicRepo.Find(a => a.ProjectID == projID && !a.IsDelete).FirstOrDefault();

                    // 设置开始点
                    if (prjInfo != null)
                    {
                        var item = new ProjectPlanRouteInfo()
                        {
                            ProjectID = prjInfo.ProjectID ?? 0,
                            Lon = prjInfo.StartLon ?? 0,
                            Lat = prjInfo.StartLat ?? 0,
                            RouteTime = prjInfo.ProjectStartTime ?? DateTime.MinValue
                        };
                        res.List.Add(item);
                    }

                    if (list != null && list.Count > 0)
                    {
                        foreach (var data in list)
                        {
                            var item = new ProjectPlanRouteInfo();
                            item.ProjectID = data.ProjectID ?? 0;
                            item.Lat = data.Lat ?? 0;
                            item.Lon = data.Lon ?? 0;
                            item.RouteTime = data.RouteTime ?? DateTime.Now;
                            res.List.Add(item);
                        }
                    }
                    else
                    {
                        res.SetError("尚未有项目规划路径。");
                    }

                    // 设置结束点
                    if (prjInfo != null)
                    {
                        var item = new ProjectPlanRouteInfo()
                        {
                            ProjectID = prjInfo.ProjectID ?? 0,
                            Lon = prjInfo.EndLon ?? 0,
                            Lat = prjInfo.EndLat ?? 0,
                            RouteTime = null
                        };
                        res.List.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(ProjectPlanRouteInfoService));
                res.SetError(ex);
            }
            return res;
        }

        public ResponseBase<ProjectPlanRouteInfo> SaveProjectList(List<ProjectPlanRouteInfo> routes, int projID = 0)
        {
            var res = new ResponseBase<ProjectPlanRouteInfo>();
            try
            {
                if (routes.Count == 0)
                {
                    res.SetError("无路径需保存");
                    return res;
                }

                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    if (projID == 0)
                    {
                        projID = routes.Max(a => a.ProjectID);
                    }

                    if (projID == 0)
                    {
                        var projRepo = uow.GetRepository<DataFactory.KingBase.Towing.Manage_Project>();
                        var proj = projRepo.Find(a => !a.IsDelete).FirstOrDefault();
                        if (proj != null)
                        {
                            projID = proj.id;
                        }
                    }

                    // set old routes to delete
                    var planRouteRepo = uow.GetRepository<DataFactory.KingBase.Towing.Manage_PlanRoute>();
                    var toDelList = planRouteRepo.Find(a => !a.IsDelete && a.ProjectID == projID).ToList();
                    foreach (var itm in toDelList)
                    {
                        itm.IsDelete = true;
                    }
                    uow.Save();

                    // insert new routes
                    foreach (var itm in routes)
                    {
                        var data = new DataFactory.KingBase.Towing.Manage_PlanRoute();
                        data.ProjectID = projID;
                        data.Lat = itm.Lat;
                        data.Lon = itm.Lon;
                        data.RouteTime = itm.RouteTime;
                        data.CreateTime = DateTime.Now;
                        data.IsDelete = false;
                        planRouteRepo.Add(data);
                    }
                    uow.Save();

                    res = GetProjectList(projID);
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(ProjectPlanRouteInfoService));
                res.SetError(ex);
            }
            return res;
        }

        public ResponseBase<ProjectPlanRouteInfo> GetForecastList(int projID = 0)
        {
            var res = new ResponseBase<ProjectPlanRouteInfo>();
            try
            {
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var forecastRepo = uow.GetRepository<DataFactory.KingBase.Towing.Data_Forecast>();
                    var latestData = forecastRepo.Find(a => !a.IsDelete)
                        .OrderByDescending(a => a.No)
                        .FirstOrDefault();

                    if (latestData != null)
                    {
                        forecastRepo.Find(a => !a.IsDelete && a.No == latestData.No)
                            .OrderBy(a => a.ForecastTime)
                            .ToList()
                            .ForEach(a =>
                            {
                                res.List.Add(new ProjectPlanRouteInfo()
                                {
                                    DataID = a.id,
                                    Lon = a.Lon ?? 0,
                                    Lat = a.Lat ?? 0,
                                    RouteTime = a.ForecastTime ?? DateTime.Now,
                                    TimeStr = (a.ForecastTime ?? DateTime.Now).ToString("MMdd-HH"),
                                    YaoX = a.MaxYaoX ?? 0,
                                    YaoY = a.MaxYaoY ?? 0,
                                    DangZ = a.MaxDangZ ?? 0,
                                    TotalAlarm = AlarmConvert(a.TotalAlarm),
                                    WindAlarm = AlarmConvert(a.WindAlarm),
                                    WaveAlarm = AlarmConvert(a.WaveAlarm),
                                    ShakeAlarm = AlarmConvert(a.ShakeAlarm)
                                });
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(ProjectPlanRouteInfoService));
                res.SetError(ex);
            }
            return res;
        }

        private static bool AlarmConvert(int? value)
        {
            if (value == null)
            {
                return false;
            }
            return value == 0 ? true : false;
        }
    }
}
