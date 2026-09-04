using Business.Towing;
using DataFactory.Factory;
using DataFactory.KingBase.Towing;
using Service.Towing.Tool;
using Tool;

namespace Service.Towing
{
    public class HikBaseResponseData<T>
    {
        public int total { set; get; }
        public List<T> list { set; get; } = new List<T>();
        public int pageNo { set; get; }
        public int pageSize { set; get; }
    }

    public class HikBaseResponse<T>
    {
        public string code { set; get; }
        public string msg { set; get; }
        public T data { set; get; }

        public void SetOK(string message = "")
        {
            msg = string.IsNullOrEmpty(message) ? "success" : $"success:{message}";
            code = "200";
        }

        public void SetError(Exception ex)
        {
            msg = $"[ERROR]{ex.Message}";
            code = "500";
        }

        public void SetError(string message)
        {
            msg = $"[ERROR]{message}";
            code = "500";
        }
    }

    public class HikPicInfoItem
    {
        public string captureId { set; get; }
        public string cameraIndexCode { set; get; }
        public string cameraName { set; get; }
        public string cameraType { set; get; }
        public string captureUrl { set; get; }
        public string thumbnailUrl { set; get; }
        public string captureTime { set; get; }
    }

    public class HikPicInfo : HikBaseResponse<HikBaseResponseData<HikPicInfoItem>>
    {
    }

    public class HikInfoService
    {
        private readonly Towing_KingBase_UnitOfWorkFactory _towingKingBaseUowFactory;
        private readonly HikHttpUtilLib _hikHttpUtilLib;

        public HikInfoService(Towing_KingBase_UnitOfWorkFactory towingKingBaseUowFactory, HikHttpUtilLib hikHttpUtilLib)
        {
            _towingKingBaseUowFactory = towingKingBaseUowFactory;
            _hikHttpUtilLib = hikHttpUtilLib;
        }

        public async Task<HikPicInfo> GetPics(DateTime? startTime = null, DateTime? endTime = null, int pageNo = 1, int pageSize = 20, string netProtocol = "http")
        {
            var action = "/api/video/v1/pictureInfos";
            startTime = startTime ?? DateTime.UtcNow.AddDays(-1);
            endTime = endTime ?? DateTime.UtcNow;

            var req = new
            {
                pageNo = pageNo,
                pageSize = pageSize,
                cameraIndexCodes = _hikHttpUtilLib.HikIdxCode.Split(',').ToList(),
                startTime = startTime.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                endTime = endTime.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                netProtocol = netProtocol,
                sort = 0,
                compressRatio = 50
            };
            return await _hikHttpUtilLib.CallApiAsync<HikPicInfo>(action, req);
        }

        public ResponseBase<Data_HikLocalPic> SaveLocalPic(DateTime snapTime, string localPath)
        {
            var res = new ResponseBase<Data_HikLocalPic>();
            try
            {
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var projRepo = uow.GetRepository<DataFactory.KingBase.Towing.Manage_Project>();
                    var proj = projRepo.Find(a => !a.IsDelete).FirstOrDefault();

                    var repo = uow.GetRepository<DataFactory.KingBase.Towing.Data_HikLocalPic>();
                    var itm = new DataFactory.KingBase.Towing.Data_HikLocalPic();
                    itm.CreateTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                    itm.IsDelete = false;
                    itm.ProjID = proj == null ? 0 : proj.id;
                    itm.SnapPicUrl = localPath;
                    itm.SnapTime = DateTime.SpecifyKind(snapTime, DateTimeKind.Unspecified);

                    repo.Add(itm);
                    uow.Save();
                }
                res.SetOK();
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(HikInfoService));
                res.SetError(ex);
            }
            return res;
        }

        public HikBaseResponse<HikBaseResponseData<Data_HikLocalPic>> GetLocalPics(DateTime? startTime = null, DateTime? endTime = null, int pageNo = 1, int pageSize = 12)
        {
            var res = new HikBaseResponse<HikBaseResponseData<Data_HikLocalPic>>();
            try
            {
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    startTime = DateTime.SpecifyKind(startTime ?? DateTime.UtcNow.AddDays(-1), DateTimeKind.Unspecified);
                    endTime = DateTime.SpecifyKind(endTime ?? DateTime.UtcNow, DateTimeKind.Unspecified);

                    var repo = uow.GetRepository<DataFactory.KingBase.Towing.Data_HikLocalPic>();
                    var query = repo.Find(a => !a.IsDelete && a.SnapTime >= startTime && a.SnapTime <= endTime)
                        .OrderByDescending(a => a.SnapTime);

                    res.data = new HikBaseResponseData<Data_HikLocalPic>();
                    res.data.total = query.Count();
                    res.data.list = query.Skip((pageNo - 1) * pageSize).Take(pageSize).ToList();
                    res.data.pageNo = pageNo;
                    res.data.pageSize = pageSize;
                }
                res.SetOK();
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(HikInfoService));
                res.SetError(ex);
            }
            return res;
        }
    }
}
