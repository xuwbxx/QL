using DataFactory.Factory;
using DataFactory.KingBase.Towing;
using Microsoft.AspNetCore.Http;
using Model.Towing;
using System.Data;
using System.IO.Compression;
using Tool;

namespace Service.Towing
{
    public class MainServiceResult
    {
        public bool Success { set; get; }
        public string? ErrorText { set; get; }
        public string? RedirectUrl { set; get; }
        public string? PXYKey { set; get; }
        public string? LoginInfoJson { set; get; }
        public string? FlashTimeJson { set; get; }
    }

    public class MainService
    {
        private readonly Towing_KingBase_UnitOfWorkFactory _towingKingBaseUowFactory;

        public MainService(Towing_KingBase_UnitOfWorkFactory towingKingBaseUowFactory)
        {
            _towingKingBaseUowFactory = towingKingBaseUowFactory;
        }

        /// <summary>
        /// Index 页面：需要用户已确认（IsConfirm）
        /// </summary>
        public MainServiceResult PrepareIndex(string? userCode, string? realName, string? userName)
        {
            var result = new MainServiceResult();
            if (string.IsNullOrEmpty(userCode))
            {
                result.Success = false;
                result.RedirectUrl = "/Cloud/CloudBase/ErrorPage?ErrorText=用户信息失效，请重新从云平台登录";
                return result;
            }

            using (var uow = _towingKingBaseUowFactory.Create())
            {
                var userRepo = uow.GetRepository<DataFactory.KingBase.Towing.Manage_User>();
                var user = userRepo.Find(a => !a.IsDelete && a.UserName.Equals(userCode) && a.IsConfirm == true)
                    .FirstOrDefault();

                if (user == null)
                {
                    result.Success = false;
                    result.RedirectUrl = "/Towing/Manage/Register";
                    return result;
                }

                RecordLoginLog(uow, userCode, realName, userName);

                result.Success = true;
                result.PXYKey = AppSettingUtils.GetSetting("PXY:PXYKey") ?? "";
                result.LoginInfoJson = BuildLoginInfoJson(userCode, realName, userName, user.Role ?? 0);
                result.FlashTimeJson = BuildFlashTimeJson(isPad: false);
            }

            return result;
        }

        /// <summary>
        /// Pad 页面：仅需用户存在
        /// </summary>
        public MainServiceResult Prepare_Pad(string? userCode, string? realName, string? userName)
        {
            var result = new MainServiceResult();
            if (string.IsNullOrEmpty(userCode))
            {
                result.Success = false;
                result.RedirectUrl = "/Cloud/CloudBase/ErrorPage?ErrorText=用户信息失效，请重新从云平台登录";
                return result;
            }

            using (var uow = _towingKingBaseUowFactory.Create())
            {
                var userRepo = uow.GetRepository<DataFactory.KingBase.Towing.Manage_User>();
                var user = userRepo.Find(a => !a.IsDelete && a.UserName.Equals(userCode))
                    .FirstOrDefault();

                if (user == null)
                {
                    result.Success = false;
                    result.RedirectUrl = "/Cloud/CloudBase/ErrorPage?ErrorText=您没有权限登录，请申请注册";
                    return result;
                }

                RecordLoginLog(uow, userCode, realName, userName);

                result.Success = true;
                result.PXYKey = AppSettingUtils.GetSetting("PXY:PXYKey") ?? "";
                result.LoginInfoJson = BuildLoginInfoJson(userCode, realName, userName, user.Role ?? 0);
                result.FlashTimeJson = BuildFlashTimeJson(isPad: true);
            }

            return result;
        }

        private void RecordLoginLog(IUnitOfWork uow, string? userCode, string? realName, string? userName)
        {
            var loginLogRepo = uow.GetRepository<DataFactory.KingBase.Towing.Manage_LoginLog>();
            var loginData = new DataFactory.KingBase.Towing.Manage_LoginLog();
            loginData.UserCode = userCode;
            loginData.RealName = realName;
            loginData.Depart = userName ?? "";
            loginData.Job = "";
            loginData.CreateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
            loginData.IsDelete = false;
            loginLogRepo.Add(loginData);
            uow.Save();
        }

        private string BuildLoginInfoJson(string? userCode, string? realName, string? userName, int roleID)
        {
            UserLoginInfo userInfo = new UserLoginInfo();
            userInfo.UserCode = userCode;
            userInfo.RealName = realName;
            userInfo.UserName = userName;
            userInfo.RoleID = roleID;
            return JsonUtils.Serialize(userInfo);
        }

        private string BuildFlashTimeJson(bool isPad)
        {
            TowingMonitorFlashTime time = new TowingMonitorFlashTime();
            time.fillLeftPanel = 30;
            time.fillRightPanel = 60;
            time.offlineSeconds = 300;
            time.loadMsgList = 60;
            time.fillChatPanel = 60;
            time.loadShipSurShips = 300;
            time.fillProgressPath = isPad ? 300 : 60;
            time.fillWeatherInfo = 240 * 60;
            time.fillWeatherForecost = 240 * 60;
            if (!isPad)
            {
                time.fillTyphoon = 60 * 30;
            }
            return JsonUtils.Serialize(time);
        }

        /// <summary>
        /// 检查用户是否为管理员（Role == 1）
        /// </summary>
        public bool IsAdmin(string userCode)
        {
            if (string.IsNullOrEmpty(userCode)) return false;
            using var uow = _towingKingBaseUowFactory.Create();
            var repo = uow.GetRepository<Manage_User>();
            var user = repo.Find(a => a.UserName == userCode && !a.IsDelete && a.Role == 1).FirstOrDefault();
            return user != null;
        }

        /// <summary>
        /// 生成测试数据
        /// </summary>
        public void DataGenerate()
        {
            try
            {
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var r = new Random();

                    // Data_MRU
                    var mruRepo = uow.GetRepository<Data_MRU>();
                    mruRepo.Add(new Data_MRU
                    {
                        DangX = Math.Round(Convert.ToDecimal(r.Next(-100000, 100000)) / 10000, 4),
                        DangY = Math.Round(Convert.ToDecimal(r.Next(-100000, 100000)) / 10000, 4),
                        DangZ = Math.Round(Convert.ToDecimal(r.Next(-100000, 100000)) / 10000, 4),
                        YaoX = Math.Round(Convert.ToDecimal(r.Next(-1020, 1020)) / 100, 2),
                        YaoY = Math.Round(Convert.ToDecimal(r.Next(-1020, 1020)) / 100, 2),
                        YaoZ = Math.Round(Convert.ToDecimal(r.Next(-1020, 1020)) / 100, 2),
                        CreateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
                        IsDelete = false
                    });

                    // Data_EGYRO
                    var egyroRepo = uow.GetRepository<Data_EGYRO>();
                    egyroRepo.Add(new Data_EGYRO
                    {
                        AzAngle = Math.Round(Convert.ToDecimal(r.Next(0, 3600)) / 200, 2),
                        CreateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
                        IsDelete = false
                    });

                    // Data_SATELLITE
                    var satRepo = uow.GetRepository<Data_SATELLITE>();
                    var lastData = satRepo.Find(a => !a.IsDelete).OrderByDescending(a => a.id).FirstOrDefault();
                    satRepo.Add(new Data_SATELLITE
                    {
                        Lon = (lastData?.Lon ?? 0) + Math.Round(Convert.ToDecimal(r.Next(-1000, 1000)) / 100000, 5),
                        Lat = (lastData?.Lat ?? 0) + Math.Round(Convert.ToDecimal(r.Next(-1000, 1000)) / 100000, 5),
                        CreateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
                        IsDelete = false
                    });

                    // Data_Wind
                    var windRepo = uow.GetRepository<Data_Wind>();
                    windRepo.Add(new Data_Wind
                    {
                        WindDirection = Math.Round(Convert.ToDecimal(r.Next(0, 3600)) / 200, 2),
                        WindSpeed = Math.Round(Convert.ToDecimal(r.Next(0, 3000)) / 100, 2),
                        CreateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
                        IsDelete = false
                    });

                    // Data_Battery
                    var batteryRepo = uow.GetRepository<Data_Battery>();
                    batteryRepo.Add(new Data_Battery
                    {
                        RemainEle = Math.Round(Convert.ToDecimal(r.Next(0, 9000)) / 100, 2),
                        RemainEle2 = Math.Round(Convert.ToDecimal(r.Next(0, 9000)) / 100, 2),
                        CreateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
                        IsDelete = false
                    });

                    // 计算航速
                    var sailRepo = uow.GetRepository<Data_SailSpeed>();
                    var lastPositions = satRepo.Find(a => !a.IsDelete)
                        .OrderByDescending(a => a.CreateTime)
                        .Take(2)
                        .ToList();

                    if (lastPositions.Count == 2
                        && lastPositions[0].Lon != null && lastPositions[1].Lon != null
                        && lastPositions[0].Lat != null && lastPositions[1].Lat != null
                        && lastPositions[0].CreateTime != lastPositions[1].CreateTime)
                    {
                        var lonDiffMeter = (lastPositions[0].Lon - lastPositions[1].Lon) * 31544206 / 360;
                        var latDiffMeter = (lastPositions[0].Lat - lastPositions[1].Lat) * 40030173 / 360;
                        double hourDiff = (lastPositions[0].CreateTime.Value - lastPositions[1].CreateTime.Value).TotalHours;
                        var speed = Math.Abs(Math.Sqrt(Math.Pow(Convert.ToDouble(lonDiffMeter), 2) + Math.Pow(Convert.ToDouble(latDiffMeter), 2)) / hourDiff);
                        var sailSpeed = Math.Round(speed / 1852, 2);

                        sailRepo.Add(new Data_SailSpeed
                        {
                            SailSpeed = Convert.ToDecimal(sailSpeed),
                            CreateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
                            IsDelete = false
                        });
                    }

                    // 更新设备时间
                    var deviceRepo = uow.GetRepository<Data_Device>();
                    var devices = deviceRepo.Find(a => !a.IsDelete).ToList();
                    foreach (var item in devices)
                    {
                        item.LatestUpdateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                    }

                    uow.Save();
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(MainService));
            }
        }

        /// <summary>
        /// 数据导出，返回 Excel 字节数组和文件名
        /// </summary>
        public (byte[] bytes, string fileName)? DataExport(string startTime, string endTime)
        {
            try
            {
                var start = Convert.ToDateTime(startTime);
                var end = Convert.ToDateTime(endTime).AddDays(1);

                using var uow = _towingKingBaseUowFactory.Create();

                var satList = uow.GetRepository<Data_SATELLITE>().Find(a => !a.IsDelete && a.CreateTime >= start && a.CreateTime < end)
                    .OrderBy(a => a.CreateTime).ToList();
                var egyroList = uow.GetRepository<Data_EGYRO>().Find(a => !a.IsDelete && a.CreateTime >= start && a.CreateTime < end)
                    .OrderBy(a => a.CreateTime).ToList();
                var mruList = uow.GetRepository<Data_MRU>().Find(a => !a.IsDelete && a.CreateTime >= start && a.CreateTime < end)
                    .OrderBy(a => a.CreateTime).ToList();
                var windList = uow.GetRepository<Data_Wind>().Find(a => !a.IsDelete && a.CreateTime >= start && a.CreateTime < end)
                    .OrderBy(a => a.CreateTime).ToList();

                var ds = new DataSet();

                // 1. 卫星定位数据
                var dtSat = new DataTable("卫星定位数据");
                dtSat.Columns.Add("经度", typeof(string));
                dtSat.Columns.Add("纬度", typeof(string));
                dtSat.Columns.Add("海拔", typeof(string));
                dtSat.Columns.Add("航向", typeof(string));
                dtSat.Columns.Add("时间", typeof(string));
                foreach (var a in satList)
                {
                    var dr = dtSat.NewRow();
                    dr[0] = a.Lon?.ToString() ?? "";
                    dr[1] = a.Lat?.ToString() ?? "";
                    dr[2] = a.Altitude?.ToString() ?? "";
                    dr[3] = a.Heading?.ToString() ?? "";
                    dr[4] = a.CreateTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
                    dtSat.Rows.Add(dr);
                }
                ds.Tables.Add(dtSat);

                // 2. 电磁罗经数据
                var dtEgyro = new DataTable("电磁罗经数据");
                dtEgyro.Columns.Add("横倾角", typeof(string));
                dtEgyro.Columns.Add("纵倾角", typeof(string));
                dtEgyro.Columns.Add("前倾角", typeof(string));
                dtEgyro.Columns.Add("时间", typeof(string));
                foreach (var a in egyroList)
                {
                    var dr = dtEgyro.NewRow();
                    dr[0] = a.AngleX?.ToString() ?? "";
                    dr[1] = a.AngleY?.ToString() ?? "";
                    dr[2] = NavigationalStatusService.AZangleConvert(a.AzAngle ?? 0).ToString();
                    dr[3] = a.CreateTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
                    dtEgyro.Rows.Add(dr);
                }
                ds.Tables.Add(dtEgyro);

                // 3. MRU数据
                var dtMru = new DataTable("MRU数据");
                dtMru.Columns.Add("横摇", typeof(string));
                dtMru.Columns.Add("纵摇", typeof(string));
                dtMru.Columns.Add("艏摇", typeof(string));
                dtMru.Columns.Add("横荡", typeof(string));
                dtMru.Columns.Add("纵荡", typeof(string));
                dtMru.Columns.Add("垂荡", typeof(string));
                dtMru.Columns.Add("时间", typeof(string));
                foreach (var a in mruList)
                {
                    var dr = dtMru.NewRow();
                    dr[0] = a.YaoX?.ToString() ?? "";
                    dr[1] = MRUStatusInfoService.MruYaoConvert(a.YaoY ?? 0).ToString();
                    dr[2] = a.YaoZ?.ToString() ?? "";
                    dr[3] = a.DangX?.ToString() ?? "";
                    dr[4] = a.DangY?.ToString() ?? "";
                    dr[5] = a.DangZ?.ToString() ?? "";
                    dr[6] = a.CreateTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
                    dtMru.Rows.Add(dr);
                }
                ds.Tables.Add(dtMru);

                // 4. 风速风向数据
                var dtWind = new DataTable("风速风向数据");
                dtWind.Columns.Add("风速", typeof(string));
                dtWind.Columns.Add("风向", typeof(string));
                dtWind.Columns.Add("时间", typeof(string));
                foreach (var a in windList)
                {
                    var dr = dtWind.NewRow();
                    dr[0] = a.WindSpeed?.ToString() ?? "";
                    dr[1] = a.WindDirection?.ToString() ?? "";
                    dr[2] = a.CreateTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
                    dtWind.Rows.Add(dr);
                }
                ds.Tables.Add(dtWind);

                var bytes = ExcelUtils.SetToBytes(ds, true);
                var fileName = $"三峡阳江16M瓦-拖航监控数据（{DateTime.UtcNow:yyyyMMddHHmmss}）.xlsx";
                return (bytes, fileName);
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(MainService));
                return null;
            }
        }

        /// <summary>
        /// 图片导出，返回 ZIP 字节数组和文件名
        /// </summary>
        public (byte[] bytes, string fileName)? PicExport(string startTime, string endTime, string webRootPath)
        {
            try
            {
                var start = Convert.ToDateTime(startTime);
                var end = Convert.ToDateTime(endTime);

                using var uow = _towingKingBaseUowFactory.Create();
                var pics = uow.GetRepository<Data_HikLocalPic>().Find(a => !a.IsDelete && a.CreateTime >= start && a.CreateTime <= end)
                    .ToList();

                var zipFileName = $"图片-{DateTime.UtcNow:yyyyMMddHHmmss}.zip";

                using var ms = new MemoryStream();
                using (var zipArchive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    foreach (var image in pics)
                    {
                        var filepath = Path.Combine(webRootPath, image.SnapPicUrl?.TrimStart('/') ?? "");
                        if (System.IO.File.Exists(filepath))
                        {
                            zipArchive.CreateEntryFromFile(filepath, Path.GetFileName(filepath));
                        }
                    }
                }

                return (ms.ToArray(), zipFileName);
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(MainService));
                return null;
            }
        }
        /// <summary>
        /// 保存拖航报告文件并写入数据库
        /// </summary>
        public void SaveTowingReportFiles(List<IFormFile> files, string fileFolderPath)
        {
            Directory.CreateDirectory(fileFolderPath);

            List<Data_File> list = new List<Data_File>();
            foreach (var item in files)
            {
                Data_File file = new Data_File();
                file.FileName = Path.GetFileName(item.FileName);
                file.FilePath = Path.Combine(fileFolderPath, Guid.NewGuid() + Path.GetExtension(item.FileName));
                file.Type = 1;
                file.CreateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                file.IsDelete = false;
                list.Add(file);

                using (var fs = new FileStream(file.FilePath, FileMode.Create))
                {
                    item.CopyTo(fs);
                }
            }

            using (var uow = _towingKingBaseUowFactory.Create())
            {
                var repo = uow.GetRepository<Data_File>();
                repo.AddList(list);
                uow.Save();
            }
        }

        /// <summary>
        /// 获取项目区域列表（未删除）
        /// </summary>
        public List<Manage_Area> GetProjectAreas()
        {
            using var uow = _towingKingBaseUowFactory.Create();
            var repo = uow.GetRepository<Manage_Area>();
            return repo.Find(a => !a.IsDelete)
                .OrderBy(a => a.id)
                .ToList();
        }
    }
}
