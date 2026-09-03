using Business.Towing;
using DataFactory.Factory;
using Tool;

namespace Service.Towing
{
    public class DeviceStatusInfo
    {
        public string DeviceType { set; get; }
        public string DeviceName { set; get; }
        public DateTime LatestUpdateTime { set; get; }

        /// <summary>
        /// 剩余电量
        /// </summary>
        public decimal? RemainEle { set; get; }

        public decimal? RemainEle2 { set; get; }
        public string RemainEleStr => Utils.getStr(RemainEle, 2);
        public string RemainEleStr2 => Utils.getStr(RemainEle2, 2);

        /// <summary>
        /// 1 工作  2 停止
        /// </summary>
        public int? Engine1Status { set; get; }
        public string Engine1StatusStr { set; get; }
        public int? Engine2Status { set; get; }
        public string Engine2StatusStr { set; get; }

        /// <summary>
        /// 1 正常  2 需充电  3 停止工作
        /// </summary>
        public int? EleStatus { set; get; }
        public string EleStatusStr { set; get; }
    }

    public class ScanModel
    {
        public int Status { set; get; }
        public string StatusStr { set; get; }
    }

    public class DeviceStatusInfoService
    {
        private readonly Towing_KingBase_UnitOfWorkFactory _towingKingBaseUowFactory;

        public DeviceStatusInfoService(Towing_KingBase_UnitOfWorkFactory towingKingBaseUowFactory)
        {
            _towingKingBaseUowFactory = towingKingBaseUowFactory;
        }

        /// <summary>
        /// 取最新的都有效，是否在线由
        /// </summary>
        public ResponseBase<DeviceStatusInfo> GetList()
        {
            var res = new ResponseBase<DeviceStatusInfo>();
            try
            {
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var deviceRepo = uow.GetRepository<DataFactory.KingBase.Towing.Data_Device>();
                    var list = deviceRepo.FindAll()
                        .Where(a => a.IsDelete != true)
                        .OrderBy(a => a.Type).ThenBy(a => a.DeviceName)
                        .ToList();

                    res.List = new List<DeviceStatusInfo>();

                    var batteryRepo = uow.GetRepository<DataFactory.KingBase.Towing.Data_Battery>();
                    var power = batteryRepo.FindAll()
                        .OrderByDescending(a => a.CreateTime)
                        .FirstOrDefault(a => a.IsDelete == false);

                    if (power != null)
                    {
                        res.Data.RemainEle = power.RemainEle ?? 0m;
                    }

                    if (list != null && list.Count > 0)
                    {
                        foreach (var data in list)
                        {
                            var item = new DeviceStatusInfo();
                            item.DeviceType = data.Type;
                            item.DeviceName = data.DeviceName;
                            item.LatestUpdateTime = data.LatestUpdateTime ?? DateTime.Now;
                            res.List.Add(item);
                        }
                    }
                    else
                    {
                        res.SetError("尚未有设备上传信息记录。");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(DeviceStatusInfoService));
                res.SetError(ex);
            }
            return res;
        }

        /// <summary>
        /// 电池电量取近10分钟的才有效
        /// </summary>
        public ResponseBase<DeviceStatusInfo> GetListV2()
        {
            var res = new ResponseBase<DeviceStatusInfo>();
            try
            {
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var deviceRepo = uow.GetRepository<DataFactory.KingBase.Towing.Data_Device>();
                    var list = deviceRepo.FindAll()
                        .Where(a => a.IsDelete != true)
                        .OrderBy(a => a.Type).ThenBy(a => a.DeviceName)
                        .ToList();

                    res.List = new List<DeviceStatusInfo>();

                    var batteryRepo = uow.GetRepository<DataFactory.KingBase.Towing.Data_Battery>();
                    var power = batteryRepo.FindAll()
                        .OrderBy(a => a.CreateTime)
                        .FirstOrDefault(a => a.IsDelete == false
                            && a.CreateTime > LimitValues.DeviceOfflineTime);

                    if (power != null)
                    {
                        res.Data.RemainEle = power.RemainEle ?? 0m;
                        res.Data.RemainEle2 = power.RemainEle2 ?? 0m;

                        var EleModel = EleConvert(power.Voltage);
                        res.Data.EleStatus = EleModel == null ? (int?)null : EleModel.Status;
                        res.Data.EleStatusStr = EleModel == null ? "" : EleModel.StatusStr;

                        var EngineModel1 = EngineConvert(power.Engine1);
                        res.Data.Engine1Status = EngineModel1 == null ? (int?)null : EngineModel1.Status;
                        res.Data.Engine1StatusStr = EngineModel1 == null ? "" : EngineModel1.StatusStr;

                        var EngineModel2 = EngineConvert(power.Engine2);
                        res.Data.Engine2Status = EngineModel2 == null ? (int?)null : EngineModel2.Status;
                        res.Data.Engine2StatusStr = EngineModel2 == null ? "" : EngineModel2.StatusStr;
                    }

                    if (list != null && list.Count > 0)
                    {
                        foreach (var data in list)
                        {
                            var item = new DeviceStatusInfo();
                            item.DeviceType = data.Type;
                            item.DeviceName = data.DeviceName;
                            item.LatestUpdateTime = data.LatestUpdateTime ?? DateTime.Now;
                            res.List.Add(item);
                        }
                    }
                    else
                    {
                        res.SetError("尚未有设备上传信息记录。");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(DeviceStatusInfoService));
                res.SetError(ex);
            }
            return res;
        }

        /// <summary>
        /// 电压大于47.5 正常  45-47.5 需充电  小于45 停止工作
        /// </summary>
        private static ScanModel? EleConvert(decimal? value)
        {
            if (value == null)
                return null;

            ScanModel model = new ScanModel();

            if (value >= 47.5M)
            {
                model.Status = 1;
                model.StatusStr = "正常";
            }
            else if (value <= 45)
            {
                model.Status = 3;
                model.StatusStr = "停止工作";
            }
            else
            {
                model.Status = 2;
                model.StatusStr = "需充电";
            }
            return model;
        }

        /// <summary>
        /// 大于2500 工作  小于2500 停止
        /// </summary>
        private static ScanModel? EngineConvert(decimal? value)
        {
            if (value == null)
                return null;

            ScanModel model = new ScanModel();

            if (value >= 2500M)
            {
                model.Status = 1;
                model.StatusStr = "工作";
            }
            else
            {
                model.Status = 2;
                model.StatusStr = "待机";
            }
            return model;
        }
    }
}
