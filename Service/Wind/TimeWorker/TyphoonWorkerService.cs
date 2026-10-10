using DataFactory.Factory;
using DataFactory.KingBase.CloudWind;
using Model.Tech.Cloud.Webgis;
using Tool;

namespace Service.Wind.TimeWorker
{
    public class TyphoonWorkerService
    {
        private static readonly string ShiPXYKey = AppSettingUtils.GetSetting("AppSettings:PXY:PXYKeyExt") ?? "";

        private readonly CloudWind_KingBase_UnitOfWorkFactory _cloudWindKingBaseUowFactory;

        public TyphoonWorkerService(CloudWind_KingBase_UnitOfWorkFactory cloudWindKingBaseUowFactory)
        {
            _cloudWindKingBaseUowFactory = cloudWindKingBaseUowFactory;
        }

        /// <summary>
        /// 获取台风列表
        /// </summary>
        public async Task<(List<PXYTyphoon> list, string msg)> GetTyphoonList()
        {
            List<PXYTyphoon> list = new List<PXYTyphoon>();
            string msg = "";

            try
            {
                var PostUrl = @"http://api.shipxy.com/apicall/GetTyphoonList?k=" + ShiPXYKey;
                string JsonStr = await HttpUtils.GetAsync(PostUrl);
                var result = JsonUtils.Deserialize<PXYRet<List<PXYTyphoon>>>(JsonStr);

                if (result.status == 0 && result.data != null && result.data.Count > 0)
                {
                    list = result.data;
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TyphoonWorkerService));
                list = new List<PXYTyphoon>();
                msg = "发生错误";
            }

            return (list, msg);
        }

        /// <summary>
        /// 获取单个台风详情
        /// </summary>
        public async Task<(List<PXYTyphoonData> list, string msg)> GetSingleTyphoon(string xuHao)
        {
            List<PXYTyphoonData> list = new List<PXYTyphoonData>();
            string msg = "";

            try
            {
                if (string.IsNullOrEmpty(xuHao))
                {
                    return (list, msg);
                }

                var PostUrl = @"http://api.shipxy.com/apicall/GetSingleTyphoon?k=" + ShiPXYKey + "&tid=" + xuHao;
                string JsonStr = await HttpUtils.GetAsync(PostUrl);
                var result = JsonUtils.Deserialize<PXYRet<List<PXYTyphoonData>>>(JsonStr);

                if (result.status == 0 && result.data != null && result.data.Count > 0)
                {
                    list = result.data;
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(TyphoonWorkerService));
                list = new List<PXYTyphoonData>();
                msg = "发生错误";
            }

            return (list, msg);
        }

        /// <summary>
        /// 保存台风数据到数据库
        /// </summary>
        public async Task SaveTyphoonData()
        {
            var (list, msg) = await GetTyphoonList();

            if (!string.IsNullOrEmpty(msg))
            {
                LoggerUtils.Error(msg, typeof(TyphoonWorkerService));
            }

            List<Data_Typhoon> typhoons = new List<Data_Typhoon>();

            foreach (var item in list)
            {
                if (!string.IsNullOrEmpty(item.dataMark))
                {
                    var (data, msg2) = await GetSingleTyphoon(item.xuHao);
                    if (!string.IsNullOrEmpty(msg2))
                    {
                        LoggerUtils.Error($"获取台风详情失败，序号: {item.xuHao}, 错误: {msg2}", typeof(TyphoonWorkerService));
                        continue;
                    }

                    foreach (var item2 in data)
                    {
                        Data_Typhoon se = new Data_Typhoon();
                        se.chnCode = item.chnCode ?? "";
                        se.chnName = item.chnName ?? "";
                        se.currentyear = string.IsNullOrEmpty(item.currentyear) ? null : int.Parse(item.currentyear);
                        se.xuHao = item.xuHao ?? "";
                        se.time = item2.time ?? "";
                        se.dateTime = item2.dateTime;
                        se.forecast = item2.forecast ?? "";
                        se.fhour = string.IsNullOrEmpty(item2.fhour) ? null : Convert.ToDecimal(item2.fhour);
                        se.lat = string.IsNullOrEmpty(item2.lat) ? null : Convert.ToDecimal(item2.lat);
                        se.lon = string.IsNullOrEmpty(item2.lon) ? null : Convert.ToDecimal(item2.lon);
                        se.grade = string.IsNullOrEmpty(item2.grade) ? null : Convert.ToDecimal(item2.grade);
                        se.mspeed = string.IsNullOrEmpty(item2.mspeed) ? null : Convert.ToDecimal(item2.mspeed);
                        se.pressure = string.IsNullOrEmpty(item2.pressure) ? null : Convert.ToDecimal(item2.pressure);
                        se.kspeed = string.IsNullOrEmpty(item2.kspeed) ? null : Convert.ToDecimal(item2.kspeed);
                        se.radius7 = string.IsNullOrEmpty(item2.radius7) ? null : Convert.ToDecimal(item2.radius7);
                        se.radius10 = string.IsNullOrEmpty(item2.radius10) ? null : Convert.ToDecimal(item2.radius10);
                        se.direction = item2.direction ?? "";
                        se.CreateTime = DateTime.Now;
                        se.IsDelete = false;

                        typhoons.Add(se);
                    }
                }
            }

            if (typhoons.Count > 0)
            {
                using (var uow = _cloudWindKingBaseUowFactory.Create())
                {
                    var repo = uow.GetRepository<Data_Typhoon>();
                    repo.AddList(typhoons);
                    uow.Save();
                    LoggerUtils.Info($"台风数据保存成功，共 {typhoons.Count} 条", typeof(TyphoonWorkerService));
                }
                
            }
        }


    }
}
