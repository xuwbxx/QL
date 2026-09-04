using Business.Towing;
using DataFactory.Factory;
using DataFactory.KingBase.Towing;
using Tool;

namespace Service.Towing
{
    public class AnchorageInfoPoint
    {
        public decimal Lat { set; get; }
        public decimal Lon { set; get; }
        public List<decimal> LatLon => new List<decimal>() { Lat, Lon };
    }

    public class AnchorageInfo
    {
        /// <summary>
        /// 类型：Position：范围，Circle：圆
        /// </summary>
        public string Type { set; get; }
        public string Name { get; set; }
        public string Group { set; get; }
        /// <summary>
        /// 纬度
        /// </summary>
        public decimal Lat { set; get; }
        /// <summary>
        /// 经度
        /// </summary>
        public decimal Lon { set; get; }
        public List<decimal> LatLon => new List<decimal>() { Lat, Lon };
        public List<List<decimal>> LatLons => Points.Select(a => a.LatLon).ToList();
        /// <summary>
        /// 幅员（m）
        /// </summary>
        public decimal Radius { set; get; }
        public List<AnchorageInfoPoint> Points { set; get; }
        public AnchorageInfo()
        {
            Points = new List<AnchorageInfoPoint>();
        }
    }

    public class AnchorageInfoService
    {
        private readonly Towing_KingBase_UnitOfWorkFactory _towingKingBaseUowFactory;

        public AnchorageInfoService(Towing_KingBase_UnitOfWorkFactory towingKingBaseUowFactory)
        {
            _towingKingBaseUowFactory = towingKingBaseUowFactory;
        }

        public ResponseBase<AnchorageInfo> GetList()
        {
            var res = new ResponseBase<AnchorageInfo>();
            try
            {
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var repo = uow.GetRepository<Manage_ShipAnchorage>();
                    var list = repo.Find(a => !a.IsDelete)
                        .OrderBy(a => a.Group)
                        .ToList();

                    // 处理Circle
                    foreach (var data in list.Where(a => string.IsNullOrEmpty(a.Group)))
                    {
                        var item = new AnchorageInfo();
                        item.Type = data.Type;
                        item.Name = data.Name;
                        item.Lon = data.Lon ?? 0;
                        item.Lat = data.Lat ?? 0;
                        item.Radius = data.Radius ?? 10m; // 默认给10米
                        res.List.Add(item);
                    }

                    var groupList = list.Where(a => !string.IsNullOrEmpty(a.Group))
                        .Select(a => a.Group).ToList()
                        .Distinct();

                    foreach (var group in groupList)
                    {
                        var points = list.Where(a => a.Group == group)
                            .OrderBy(a => a.id).ToList();
                        var item = new AnchorageInfo();
                        var firstP = points.FirstOrDefault();
                        item.Type = firstP.Type;
                        item.Name = firstP.Name;
                        item.Group = firstP.Group;
                        item.Points = new List<AnchorageInfoPoint>();

                        foreach (var point in points)
                        {
                            item.Points.Add(new AnchorageInfoPoint() { Lat = point.Lat ?? 0, Lon = point.Lon ?? 0 });
                        }
                        res.List.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(AnchorageInfoService));
                res.SetError(ex);
            }
            return res;
        }
    }
}
