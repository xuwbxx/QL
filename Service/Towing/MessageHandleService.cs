using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataFactory.Factory;
using DataFactory.KingBase.Towing;
using Tool;

namespace Service.Towing
{
    public class MessageHandleService
    {
        private readonly Towing_KingBase_UnitOfWorkFactory _towingKingBaseUowFactory;

        public MessageHandleService(Towing_KingBase_UnitOfWorkFactory towingKingBaseUowFactory)
        {
            _towingKingBaseUowFactory = towingKingBaseUowFactory;
        }

        /// <summary>
        /// 拖航
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public async Task DataHandle_SanXia(string text)
        {
            try
            {
                string[] arrText = text.Split('|');

                //时间 $20250619152624
                string timeStr = arrText[0].Trim('$');
                if (timeStr.Length != 14)
                {
                    return;
                }
                string format = "yyyyMMddHHmmss";
                DateTime time = DateTime.ParseExact(timeStr, format, null);

                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var windRepo = uow.GetRepository<Data_Wind>();
                    int count = windRepo.Find(a => !a.IsDelete && a.CreateTime == time).Count();
                    if (count > 1)
                    {
                        return;
                    }

                    //风速风向1
                    decimal? WindSpeed = null;
                    decimal? WindDirection = null;
                    if (!arrText[1].Trim().ToLower().Contains("null"))
                    {
                        string[] windArr = arrText[1].Trim().Split(',');
                        WindSpeed = DataConvert(windArr[3], 2);
                        WindDirection = DataConvert(windArr[1], 2);
                    }

                    //风速风向2
                    decimal? WindSpeed2 = null;
                    decimal? WindDirection2 = null;
                    if (!arrText[2].Trim().ToLower().Contains("null"))
                    {
                        string[] windArr2 = arrText[2].Trim().Split(',');
                        WindSpeed2 = DataConvert(windArr2[3], 2);
                        WindDirection2 = DataConvert(windArr2[1], 2);
                    }

                    //电磁罗经1
                    decimal? AngleX = null;
                    decimal? AngleY = null;
                    decimal? AzAngle = null;
                    if (!arrText[3].Trim().ToLower().Contains("null"))
                    {
                        string[] dxlj1 = arrText[3].Trim().Split(' ');
                        string dxljRollStr = dxlj1[4] + dxlj1[5] + dxlj1[6];
                        string dxljPitchStr = dxlj1[7] + dxlj1[8] + dxlj1[9];
                        string dxljHeadingStr = dxlj1[10] + dxlj1[11] + dxlj1[12];

                        AngleX = DCLJConvert(dxljRollStr);
                        AngleY = DCLJConvert(dxljPitchStr);
                        AzAngle = DCLJConvert(dxljHeadingStr);
                    }

                    //电磁罗经2
                    decimal? AngleX2 = null;
                    decimal? AngleY2 = null;
                    decimal? AzAngle2 = null;
                    if (!arrText[4].Trim().ToLower().Contains("null"))
                    {
                        string[] dxlj2 = arrText[4].Trim().Split(' ');
                        string dxljRollStr2 = dxlj2[4] + dxlj2[5] + dxlj2[6];
                        string dxljPitchStr2 = dxlj2[7] + dxlj2[8] + dxlj2[9];
                        string dxljHeadingStr2 = dxlj2[10] + dxlj2[11] + dxlj2[12];

                        AngleX2 = DCLJConvert(dxljRollStr2);
                        AngleY2 = DCLJConvert(dxljPitchStr2);
                        AzAngle2 = DCLJConvert(dxljHeadingStr2);
                    }

                    //GPS1
                    decimal? Lat = null;
                    decimal? Lon = null;
                    decimal? Altitude = null;
                    if (!arrText[7].Trim().ToLower().Contains("null"))
                    {
                        string gpsStr1 = arrText[7].Trim();
                        string[] gps1Arr = gpsStr1.Substring(gpsStr1.IndexOf('$') + 1, gpsStr1.IndexOf('*') - gpsStr1.IndexOf('$') - 1).Split(',');
                        Lat = PositionConvert(gps1Arr[2]);
                        Lon = PositionConvert(gps1Arr[4]);
                        Altitude = DataConvert(gps1Arr[9], 2);
                    }

                    //GPS2
                    decimal? Lat2 = null;
                    decimal? Lon2 = null;
                    decimal? Altitude2 = null;
                    if (!arrText[8].Trim().ToLower().Contains("null"))
                    {
                        string gpsStr2 = arrText[8].Trim();
                        string[] gps1Arr2 = gpsStr2.Substring(gpsStr2.IndexOf('$') + 1, gpsStr2.IndexOf('*') - gpsStr2.IndexOf('$') - 1).Split(',');
                        Lat2 = PositionConvert(gps1Arr2[2]);
                        Lon2 = PositionConvert(gps1Arr2[4]);
                        Altitude2 = DataConvert(gps1Arr2[9], 2);
                    }

                    //发电机转速1
                    decimal? engine1 = null;
                    if (!arrText[9].Trim().ToLower().Contains("null"))
                    {
                        string engineStr1 = arrText[9].Trim();
                        string[] engineArr1 = engineStr1.Split(' ');
                        string engineMainStr1 = engineArr1[3] + engineArr1[4];
                        engine1 = Convert.ToInt32(engineMainStr1, 16);
                    }

                    //发电机转速2
                    decimal? engine2 = null;
                    if (!arrText[10].Trim().ToLower().Contains("null"))
                    {
                        string engineStr2 = arrText[10].Trim();
                        string[] engineArr2 = engineStr2.Split(' ');
                        string engineMainStr2 = engineArr2[3] + engineArr2[4];
                        engine2 = Convert.ToInt32(engineMainStr2, 16);
                    }

                    //电池余量1
                    string dcStr = arrText[11].Trim();
                    decimal? RemainEle = null;
                    if (!dcStr.Trim().ToLower().Contains("null"))
                    {
                        string[] dcArr = dcStr.Split(' ');
                        string dc16str = dcArr[119] + dcArr[120];
                        RemainEle = Math.Round(Convert.ToDecimal(Convert.ToInt32(dc16str, 16)) / 10, 2);
                    }

                    //电池余量2
                    string dcStr2 = arrText[12].Trim();
                    decimal? RemainEle2 = null;
                    if (!dcStr2.Trim().ToLower().Contains("null"))
                    {
                        string[] dcArr2 = dcStr2.Split(' ');
                        string dc16str2 = dcArr2[119] + dcArr2[120];
                        RemainEle2 = Math.Round(Convert.ToDecimal(Convert.ToInt32(dc16str2, 16)) / 10, 2);
                    }

                    //电压
                    string dyStr = arrText[13].Trim('*').Trim('$');
                    decimal? Voltage = null;
                    if (!dyStr.Trim().ToLower().Contains("null"))
                    {
                        Voltage = Convert.ToDecimal(dyStr);
                    }

                    //航速
                    decimal? SailSpeed = 0;

                    var dataEgyro = new Data_EGYRO();
                    var dataMru = new Data_MRU();
                    var dataSatellite = new Data_SATELLITE();
                    var dataWind = new Data_Wind();
                    var dataBattery = new Data_Battery();

                    var deviceRepo = uow.GetRepository<Data_Device>();
                    var Devices = deviceRepo.Find(a => !a.IsDelete).ToList();

                    DateTime now = DateTime.Now;

                    // 电磁罗经
                    if (AzAngle != null)
                    {
                        dataEgyro.AzAngle = AzAngle;
                        dataEgyro.AngleX = AngleX;
                        dataEgyro.AngleY = AngleY;
                        dataEgyro.CreateTime = time;
                        dataEgyro.CreateTime2 = now;
                        dataEgyro.IsDelete = false;

                        var egyroRepo = uow.GetRepository<Data_EGYRO>();
                        egyroRepo.Add(dataEgyro);

                        Devices.Where(a => a.Type.Equals("电磁罗经")).ToList().ForEach(a =>
                        {
                            a.LatestUpdateTime = time;
                            a.CreateTime = now;
                        });
                    }

                    // GPS 卫星
                    if (Lat != null && Lon != null)
                    {
                        var satRepo = uow.GetRepository<Data_SATELLITE>();
                        var latestSATELLITE = satRepo.Find(a => !a.IsDelete)
                            .OrderByDescending(a => a.CreateTime)
                            .FirstOrDefault();

                        decimal? Heading = null;
                        if (latestSATELLITE != null)
                        {
                            if (latestSATELLITE.Lon != null && latestSATELLITE.Lat != null && latestSATELLITE.CreateTime != null)
                            {
                                Heading = CalculateHeading(latestSATELLITE.Lon.Value, latestSATELLITE.Lat.Value, latestSATELLITE.CreateTime.Value, Lon.Value, Lat.Value, time);
                            }
                        }

                        dataSatellite.Lon = Lon;
                        dataSatellite.Lat = Lat;
                        dataSatellite.Altitude = Altitude;
                        dataSatellite.Heading = Heading;
                        dataSatellite.CreateTime = time;
                        dataSatellite.CreateTime2 = now;
                        dataSatellite.IsDelete = false;

                        satRepo.Add(dataSatellite);

                        Devices.Where(a => a.Type.Equals("卫星")).ToList().ForEach(a =>
                        {
                            a.LatestUpdateTime = time;
                            a.CreateTime = now;
                        });

                        //计算航速
                        var LastPositions = satRepo.Find(a => !a.IsDelete)
                            .OrderByDescending(a => a.CreateTime)
                            .Take(2)
                            .ToList();

                        if (LastPositions.Count == 2
                            && LastPositions[0].Lon != null && LastPositions[1].Lon != null
                            && LastPositions[0].Lat != null && LastPositions[1].Lat != null
                            && LastPositions[0].CreateTime != LastPositions[1].CreateTime)
                        {
                            var LonDiffMeter = (LastPositions[0].Lon - LastPositions[1].Lon) * 31544206 / 360;
                            var LatDiffMeter = (LastPositions[0].Lat - LastPositions[1].Lat) * 40030173 / 360;

                            double hourDiff = (LastPositions[0].CreateTime.Value - LastPositions[1].CreateTime.Value).TotalHours;

                            var Speed = Math.Abs(Math.Sqrt(Math.Pow(Convert.ToDouble(LonDiffMeter), 2) + Math.Pow(Convert.ToDouble(LatDiffMeter), 2)) / hourDiff);
                            var SailSpeedTemp = Math.Round(Speed / 1852, 2);
                            SailSpeed = Convert.ToDecimal(SailSpeedTemp);

                            var NewSailSpeed = new Data_SailSpeed();
                            NewSailSpeed.SailSpeed = SailSpeed;
                            NewSailSpeed.CreateTime = time;
                            NewSailSpeed.CreateTime2 = now;
                            NewSailSpeed.IsDelete = false;

                            var speedRepo = uow.GetRepository<Data_SailSpeed>();
                            speedRepo.Add(NewSailSpeed);
                        }
                    }

                    // 风速
                    if (WindSpeed != null && WindDirection != null)
                    {
                        dataWind.WindSpeed = WindSpeed;
                        dataWind.WindDirection = WindDirection;
                        dataWind.CreateTime = time;
                        dataWind.CreateTime2 = now;
                        dataWind.IsDelete = false;

                        windRepo.Add(dataWind);

                        Devices.Where(a => a.Type.Equals("风速")).ToList().ForEach(a =>
                        {
                            a.LatestUpdateTime = time;
                            a.CreateTime = now;
                        });
                    }

                    // 电池
                    if (engine1 != null && engine2 != null && Voltage != null)
                    {
                        dataBattery.RemainEle = RemainEle;
                        dataBattery.RemainEle2 = RemainEle2;
                        dataBattery.Engine1 = engine1;
                        dataBattery.Engine2 = engine2;
                        dataBattery.Voltage = Voltage;
                        dataBattery.CreateTime = time;
                        dataBattery.CreateTime2 = now;
                        dataBattery.IsDelete = false;

                        var batteryRepo = uow.GetRepository<Data_Battery>();
                        batteryRepo.Add(dataBattery);

                        Devices.Where(a => a.Type.Equals("电池")).ToList().ForEach(a =>
                        {
                            a.LatestUpdateTime = time;
                            a.CreateTime = now;
                        });
                    }

                    //保存原始报文
                    var bakRepo = uow.GetRepository<Data_Backup>();
                    var bak = new Data_Backup();
                    bak.DataString = text;
                    bak.CreateTime = now;
                    bakRepo.Add(bak);

                    await uow.SaveAsync();
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(MessageHandleService));
                throw;
            }
        }

        public async Task DataHandleMRU_SanXia(string text)
        {
            try
            {
                string[] arrText = text.Split('|');

                if (arrText.Length == 0 || arrText.Length != 31)
                {
                    LoggerUtils.Error(text, typeof(MessageHandleService));
                    return;
                }

                //20250915081748
                string timeStr = arrText[0].Trim('$');
                if (timeStr.Length != 14)
                {
                    return;
                }
                string format = "yyyyMMddHHmmss";
                DateTime time = DateTime.ParseExact(timeStr, format, null);

                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var mruRepo = uow.GetRepository<Data_MRU>();
                    int count = mruRepo.Find(a => !a.IsDelete && a.CreateTime == time).Count();
                    if (count > 1)
                    {
                        return;
                    }

                    List<Data_MRU> list = new List<Data_MRU>();
                    DateTime now = DateTime.Now;

                    for (int i = 1; i < arrText.Length; i = i + 2)
                    {
                        //$PSXN,10,010,+2.2878e-2,-4.1987e-2,+2.0082e-2,-1.2385e-3,-5.1204e-4,-3.1434e-3*21
                        string txt = arrText[i].Trim().ToString();

                        if (txt.ToLower().Contains("null"))
                        {
                            continue;
                        }

                        string[] mru1 = txt.Substring(txt.IndexOf('$') + 1, txt.IndexOf('*') - txt.IndexOf('$') - 1).Split(',');

                        decimal? DangX = DataConvert(mru1[7].Trim(), 4);
                        decimal? DangY = DataConvert(mru1[6].Trim(), 4);
                        decimal? DangZ = DataConvert(mru1[8].Trim(), 4);
                        //横摇纵摇要反一下（2025-10-28）
                        decimal? YaoY = DataConvertRadian(mru1[3].Trim(), 4);//弧度
                        decimal? YaoX = DataConvertRadian(mru1[4].Trim(), 4);//弧度
                        decimal? YaoZ = DataConvertRadian(mru1[5].Trim(), 4);//弧度

                        Data_MRU data = new Data_MRU();
                        data.YaoX = YaoX;
                        data.YaoY = YaoY;
                        data.YaoZ = YaoZ;
                        data.DangX = DangX;
                        data.DangY = DangY;
                        data.DangZ = DangZ;
                        data.CreateTime = time;
                        data.CreateTime2 = now;
                        data.IsDelete = false;
                        time = time.AddSeconds(2);

                        list.Add(data);
                    }

                    if (list.Count > 0)
                    {
                        var deviceRepo = uow.GetRepository<Data_Device>();
                        var devices = deviceRepo.Find(a => !a.IsDelete && a.Type.Equals("MRU")).ToList();
                        devices.ForEach(a =>
                        {
                            a.LatestUpdateTime = time;
                            a.CreateTime = now;
                        });

                        // 备份报文
                        var backupMruRepo = uow.GetRepository<Data_Backup_Mru>();
                        var mruJson = new Data_Backup_Mru();
                        mruJson.DataString = text;
                        mruJson.CreateTime = now;
                        backupMruRepo.Add(mruJson);

                        // 批量插入
                        mruRepo.AddList(list);

                        // 提交
                        await uow.SaveAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(MessageHandleService));
                throw;
            }
        }

        /// <summary>
        /// 批量处理 MRU 数据（使用 AddList 替代 BulkInsert）
        /// </summary>
        public async Task DataHandleMRU_SanXia_List(List<string> texts)
        {
            try
            {
                List<string> times = new List<string>();

                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var mruRepo = uow.GetRepository<Data_MRU>();
                    List<Data_MRU> list = new List<Data_MRU>();

                    foreach (var text in texts)
                    {
                        string[] arrText = text.Split('|');

                        if (arrText.Length == 0 || arrText.Length != 31)
                        {
                            continue;
                        }

                        //20250915081748
                        string timeStr = arrText[0].Trim('$');
                        if (timeStr.Length != 14)
                        {
                            continue;
                        }

                        if (times.Contains(timeStr))
                        {
                            continue;
                        }

                        times.Add(timeStr);

                        string format = "yyyyMMddHHmmss";
                        DateTime time = DateTime.ParseExact(timeStr, format, null);

                        int count = mruRepo.Find(a => !a.IsDelete && a.CreateTime == time).Count();
                        if (count > 1)
                        {
                            continue;
                        }

                        DateTime now = DateTime.Now;

                        for (int i = 1; i < arrText.Length; i = i + 2)
                        {
                            //$PSXN,10,010,+2.2878e-2,-4.1987e-2,+2.0082e-2,-1.2385e-3,-5.1204e-4,-3.1434e-3*21
                            string txt = arrText[i].Trim().ToString();

                            if (txt.ToLower().Contains("null"))
                            {
                                continue;
                            }

                            string[] mru1 = txt.Substring(txt.IndexOf('$') + 1, txt.IndexOf('*') - txt.IndexOf('$') - 1).Split(',');

                            decimal? DangX = DataConvert(mru1[7].Trim(), 4);
                            decimal? DangY = DataConvert(mru1[6].Trim(), 4);
                            decimal? DangZ = DataConvert(mru1[8].Trim(), 4);
                            decimal? YaoX = DataConvert(mru1[3].Trim(), 4);
                            decimal? YaoY = DataConvert(mru1[4].Trim(), 4);
                            decimal? YaoZ = DataConvert(mru1[5].Trim(), 4);

                            Data_MRU data = new Data_MRU();
                            data.DangX = DangX;
                            data.DangY = DangY;
                            data.DangZ = DangZ;
                            data.YaoX = YaoX;
                            data.YaoY = YaoY;
                            data.YaoZ = YaoZ;
                            data.CreateTime = time;
                            data.CreateTime2 = now;
                            data.IsDelete = false;

                            time = time.AddSeconds(2);

                            list.Add(data);
                        }
                    }

                    if (list.Count > 0)
                    {
                        mruRepo.AddList(list);
                        await uow.SaveAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(MessageHandleService));
            }
        }

        /// <summary>
        /// 批量处理文件数据（含 MRU）
        /// </summary>
        public async Task DataHandle_Batch(string filePath, string startTxt)
        {
            try
            {
                // 初始化结果集合
                List<string> wholeList = new List<string>();

                // 读取文件所有行
                string[] allLines = File.ReadAllLines(filePath);

                for (int i = 0; i < allLines.Length; i += 2)
                {
                    // 跳过空行
                    if (string.IsNullOrWhiteSpace(allLines[i]))
                        continue;

                    // 步骤1：移除行首 [] 包裹的时间部分
                    int endTimeIndex = allLines[i].IndexOf(']');
                    if (endTimeIndex == -1)
                        continue;
                    string content = allLines[i].Substring(endTimeIndex + 1).Trim();

                    wholeList.Add(content);
                }

                //筛选日期
                List<string> filterList = new List<string>();

                bool start = false;
                foreach (var item in wholeList)
                {
                    if (start)
                    {
                        filterList.Add(item);
                    }
                    if (item.StartsWith(startTxt))
                    {
                        start = true;
                    }
                }

                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var egyroRepo = uow.GetRepository<Data_EGYRO>();
                    var satRepo = uow.GetRepository<Data_SATELLITE>();
                    var windRepo = uow.GetRepository<Data_Wind>();
                    var batteryRepo = uow.GetRepository<Data_Battery>();
                    var mruRepo = uow.GetRepository<Data_MRU>();
                    var speedRepo = uow.GetRepository<Data_SailSpeed>();
                    var bakRepo = uow.GetRepository<Data_Backup>();

                    foreach (var text in filterList)
                    {
                        //开始解析报文
                        string[] arrText = text.Split('|');

                        //时间 $20250619152624
                        string timeStr = arrText[0].Trim('$');
                        if (timeStr.Length != 14)
                        {
                            return;
                        }
                        string format = "yyyyMMddHHmmss";
                        DateTime time = DateTime.ParseExact(timeStr, format, null);

                        //风速风向1
                        decimal? WindSpeed = null;
                        decimal? WindDirection = null;
                        if (!arrText[1].Trim().ToLower().Contains("null"))
                        {
                            string[] windArr = arrText[1].Trim().Split(',');
                            WindSpeed = DataConvert(windArr[3], 2);
                            WindDirection = DataConvert(windArr[1], 2);
                        }

                        //风速风向2
                        decimal? WindSpeed2 = null;
                        decimal? WindDirection2 = null;
                        if (!arrText[2].Trim().ToLower().Contains("null"))
                        {
                            string[] windArr2 = arrText[2].Trim().Split(',');
                            WindSpeed2 = DataConvert(windArr2[3], 2);
                            WindDirection2 = DataConvert(windArr2[1], 2);
                        }

                        //电磁罗经1
                        decimal? AngleX = null;
                        decimal? AngleY = null;
                        decimal? AzAngle = null;
                        if (!arrText[3].Trim().ToLower().Contains("null"))
                        {
                            string[] dxlj1 = arrText[3].Trim().Split(' ');
                            string dxljRollStr = dxlj1[4] + dxlj1[5] + dxlj1[6];
                            string dxljPitchStr = dxlj1[7] + dxlj1[8] + dxlj1[9];
                            string dxljHeadingStr = dxlj1[10] + dxlj1[11] + dxlj1[12];

                            AngleX = DCLJConvert(dxljRollStr);
                            AngleY = DCLJConvert(dxljPitchStr);
                            AzAngle = DCLJConvert(dxljHeadingStr);
                        }

                        //电磁罗经2
                        decimal? AngleX2 = null;
                        decimal? AngleY2 = null;
                        decimal? AzAngle2 = null;
                        if (!arrText[4].Trim().ToLower().Contains("null"))
                        {
                            string[] dxlj2 = arrText[4].Trim().Split(' ');
                            string dxljRollStr2 = dxlj2[4] + dxlj2[5] + dxlj2[6];
                            string dxljPitchStr2 = dxlj2[7] + dxlj2[8] + dxlj2[9];
                            string dxljHeadingStr2 = dxlj2[10] + dxlj2[11] + dxlj2[12];

                            AngleX2 = DCLJConvert(dxljRollStr2);
                            AngleY2 = DCLJConvert(dxljPitchStr2);
                            AzAngle2 = DCLJConvert(dxljHeadingStr2);
                        }

                        //MRU1
                        string mruTxt = arrText[5].Trim().ToString();

                        string[] mru1 = mruTxt.Substring(mruTxt.IndexOf('$') + 1, mruTxt.IndexOf('*') - mruTxt.IndexOf('$') - 1).Split(',');

                        decimal? DangX = DataConvert(mru1[7].Trim(), 4);
                        decimal? DangY = DataConvert(mru1[6].Trim(), 4);
                        decimal? DangZ = DataConvert(mru1[8].Trim(), 4);
                        //横摇纵摇要反一下（2025-10-28）
                        decimal? YaoY = DataConvertRadian(mru1[3].Trim(), 4);//弧度
                        decimal? YaoX = DataConvertRadian(mru1[4].Trim(), 4);//弧度
                        decimal? YaoZ = DataConvertRadian(mru1[5].Trim(), 4);//弧度

                        //GPS1
                        decimal? Lat = null;
                        decimal? Lon = null;
                        decimal? Altitude = null;
                        if (!arrText[7].Trim().ToLower().Contains("null"))
                        {
                            string gpsStr1 = arrText[7].Trim();
                            string[] gps1Arr = gpsStr1.Substring(gpsStr1.IndexOf('$') + 1, gpsStr1.IndexOf('*') - gpsStr1.IndexOf('$') - 1).Split(',');
                            Lat = PositionConvert(gps1Arr[2]);
                            Lon = PositionConvert(gps1Arr[4]);
                            Altitude = DataConvert(gps1Arr[9], 2);
                        }

                        //GPS2
                        decimal? Lat2 = null;
                        decimal? Lon2 = null;
                        decimal? Altitude2 = null;
                        if (!arrText[8].Trim().ToLower().Contains("null"))
                        {
                            string gpsStr2 = arrText[8].Trim();
                            string[] gps1Arr2 = gpsStr2.Substring(gpsStr2.IndexOf('$') + 1, gpsStr2.IndexOf('*') - gpsStr2.IndexOf('$') - 1).Split(',');
                            Lat2 = PositionConvert(gps1Arr2[2]);
                            Lon2 = PositionConvert(gps1Arr2[4]);
                            Altitude2 = DataConvert(gps1Arr2[9], 2);
                        }

                        //发电机转速1
                        decimal? engine1 = null;
                        if (!arrText[9].Trim().ToLower().Contains("null"))
                        {
                            string engineStr1 = arrText[9].Trim();
                            string[] engineArr1 = engineStr1.Split(' ');
                            string engineMainStr1 = engineArr1[3] + engineArr1[4];
                            engine1 = Convert.ToInt32(engineMainStr1, 16);
                        }

                        //发电机转速2
                        decimal? engine2 = null;
                        if (!arrText[10].Trim().ToLower().Contains("null"))
                        {
                            string engineStr2 = arrText[10].Trim();
                            string[] engineArr2 = engineStr2.Split(' ');
                            string engineMainStr2 = engineArr2[3] + engineArr2[4];
                            engine2 = Convert.ToInt32(engineMainStr2, 16);
                        }

                        //电池余量1
                        string dcStr = arrText[11].Trim();
                        decimal? RemainEle = null;
                        if (!dcStr.Trim().ToLower().Contains("null"))
                        {
                            string[] dcArr = dcStr.Split(' ');
                            string dc16str = dcArr[119] + dcArr[120];
                            RemainEle = Math.Round(Convert.ToDecimal(Convert.ToInt32(dc16str, 16)) / 10, 2);
                        }

                        //电池余量2
                        string dcStr2 = arrText[12].Trim();
                        decimal? RemainEle2 = null;
                        if (!dcStr2.Trim().ToLower().Contains("null"))
                        {
                            string[] dcArr2 = dcStr2.Split(' ');
                            string dc16str2 = dcArr2[119] + dcArr2[120];
                            RemainEle2 = Math.Round(Convert.ToDecimal(Convert.ToInt32(dc16str2, 16)) / 10, 2);
                        }

                        //电压
                        string dyStr = arrText[13].Trim('*').Trim('$');
                        decimal? Voltage = null;
                        if (!dyStr.Trim().ToLower().Contains("null"))
                        {
                            Voltage = Convert.ToDecimal(dyStr);
                        }

                        //航速
                        decimal? SailSpeed = 0;

                        DateTime now = DateTime.Now;

                        // 电磁罗经
                        if (AzAngle != null)
                        {
                            var Data_EGYRO = new Data_EGYRO();
                            Data_EGYRO.AzAngle = AzAngle;
                            Data_EGYRO.AngleX = AngleX;
                            Data_EGYRO.AngleY = AngleY;
                            Data_EGYRO.CreateTime = time;
                            Data_EGYRO.CreateTime2 = now;
                            Data_EGYRO.IsDelete = false;

                            egyroRepo.Add(Data_EGYRO);
                        }

                        // GPS 卫星
                        if (Lat != null && Lon != null)
                        {
                            var latestSATELLITE = satRepo.Find(a => !a.IsDelete)
                                .OrderByDescending(a => a.CreateTime)
                                .FirstOrDefault();

                            decimal? Heading = null;
                            if (latestSATELLITE != null)
                            {
                                if (latestSATELLITE.Lon != null && latestSATELLITE.Lat != null && latestSATELLITE.CreateTime != null)
                                {
                                    Heading = CalculateHeading(latestSATELLITE.Lon.Value, latestSATELLITE.Lat.Value, latestSATELLITE.CreateTime.Value, Lon.Value, Lat.Value, time);
                                }
                            }

                            var Data_SATELLITE = new Data_SATELLITE();
                            Data_SATELLITE.Lon = Lon;
                            Data_SATELLITE.Lat = Lat;
                            Data_SATELLITE.Altitude = Altitude;
                            Data_SATELLITE.Heading = Heading;
                            Data_SATELLITE.CreateTime = time;
                            Data_SATELLITE.CreateTime2 = now;
                            Data_SATELLITE.IsDelete = false;

                            satRepo.Add(Data_SATELLITE);

                            //计算航速
                            var LastPositions = satRepo.Find(a => !a.IsDelete)
                                .OrderByDescending(a => a.CreateTime)
                                .Take(2)
                                .ToList();

                            if (LastPositions.Count == 2
                                && LastPositions[0].Lon != null && LastPositions[1].Lon != null
                                && LastPositions[0].Lat != null && LastPositions[1].Lat != null
                                && LastPositions[0].CreateTime != LastPositions[1].CreateTime)
                            {
                                var LonDiffMeter = (LastPositions[0].Lon - LastPositions[1].Lon) * 31544206 / 360;
                                var LatDiffMeter = (LastPositions[0].Lat - LastPositions[1].Lat) * 40030173 / 360;

                                double hourDiff = (LastPositions[0].CreateTime.Value - LastPositions[1].CreateTime.Value).TotalHours;

                                var Speed = Math.Abs(Math.Sqrt(Math.Pow(Convert.ToDouble(LonDiffMeter), 2) + Math.Pow(Convert.ToDouble(LatDiffMeter), 2)) / hourDiff);
                                var SailSpeedTemp = Math.Round(Speed / 1852, 2);
                                SailSpeed = Convert.ToDecimal(SailSpeedTemp);

                                var NewSailSpeed = new Data_SailSpeed();
                                NewSailSpeed.SailSpeed = SailSpeed;
                                NewSailSpeed.CreateTime = time;
                                NewSailSpeed.CreateTime2 = now;
                                NewSailSpeed.IsDelete = false;

                                speedRepo.Add(NewSailSpeed);
                            }
                        }

                        // 风速
                        if (WindSpeed != null && WindDirection != null)
                        {
                            var Data_Wind = new Data_Wind();
                            Data_Wind.WindSpeed = WindSpeed;
                            Data_Wind.WindDirection = WindDirection;
                            Data_Wind.CreateTime = time;
                            Data_Wind.CreateTime2 = now;
                            Data_Wind.IsDelete = false;

                            windRepo.Add(Data_Wind);
                        }

                        // 电池
                        if (engine1 != null && engine2 != null && Voltage != null)
                        {
                            var Data_Battery = new Data_Battery();
                            Data_Battery.RemainEle = RemainEle;
                            Data_Battery.RemainEle2 = RemainEle2;
                            Data_Battery.Engine1 = engine1;
                            Data_Battery.Engine2 = engine2;
                            Data_Battery.Voltage = Voltage;
                            Data_Battery.CreateTime = time;
                            Data_Battery.CreateTime2 = now;
                            Data_Battery.IsDelete = false;

                            batteryRepo.Add(Data_Battery);
                        }

                        //MRU
                        if (DangX != null && DangY != null && DangZ != null && YaoX != null && YaoY != null && YaoZ != null)
                        {
                            Data_MRU data = new Data_MRU();
                            data.YaoX = YaoX;
                            data.YaoY = YaoY;
                            data.YaoZ = YaoZ;
                            data.DangX = DangX;
                            data.DangY = DangY;
                            data.DangZ = DangZ;
                            data.CreateTime = time;
                            data.CreateTime2 = now;
                            data.IsDelete = false;

                            mruRepo.Add(data);
                        }

                        //保存原始报文
                        Data_Backup bak = new Data_Backup();
                        bak.DataString = text;
                        bak.CreateTime = now;

                        bakRepo.Add(bak);

                        await uow.SaveAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #region 私有辅助方法

        private static decimal? DataConvert(string value, int dec)
        {
            if (string.IsNullOrEmpty(value) || value.ToLower().Equals("null"))
            {
                return null;
            }

            decimal ThisValue = Convert.ToDecimal(Math.Round(Convert.ToDouble(value), dec));

            return ThisValue;
        }

        private static decimal? DataConvertRadian(string value, int dec)
        {
            if (string.IsNullOrEmpty(value) || value.ToLower().Equals("null"))
            {
                return null;
            }

            decimal ThisValue = Convert.ToDecimal(Math.Round(Convert.ToDouble(value) * 180 / Math.PI, 4));

            return ThisValue;
        }

        private static decimal? PositionConvert(string value)
        {
            if (string.IsNullOrEmpty(value) || value.ToLower().Equals("null"))
            {
                return null;
            }

            string PositionValueStr = (Convert.ToDecimal(value) / 100).ToString();

            string[] PositionValueStrArr = PositionValueStr.Split('.');

            decimal hour = Convert.ToDecimal(PositionValueStrArr[0]);

            decimal minute = Convert.ToDecimal(value.Substring(hour.ToString().Length));

            return Convert.ToDecimal(Math.Round((hour + minute / 60), 7));
        }

        private static decimal? DCLJConvert(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            if (value.Substring(0, 1).Equals("0"))
            {
                return Convert.ToDecimal(value.Substring(1, 3) + "." + value.Substring(4, 2));
            }
            else if (value.Substring(0, 1).Equals("1"))
            {
                return Convert.ToDecimal("-" + value.Substring(1, 3) + "." + value.Substring(4, 2));
            }

            return null;
        }

        /// <summary>
        /// 计算航向
        /// </summary>
        private static decimal? CalculateHeading(decimal lon1, decimal lat1, DateTime time1, decimal lon2, decimal lat2, DateTime time2)
        {
            // 计算经纬度差异
            decimal lonDiff = Math.Abs(lon1 - lon2);
            decimal latDiff = Math.Abs(lat1 - lat2);

            // 当经纬度差异都小于0.00001时返回null
            if (lonDiff < 0.00001m && latDiff < 0.00001m)
            {
                return null;
            }

            // 将十进制经纬度转换为弧度
            double longitude1 = (double)lon1 * Math.PI / 180.0;
            double latitude1 = (double)lat1 * Math.PI / 180.0;
            double longitude2 = (double)lon2 * Math.PI / 180.0;
            double latitude2 = (double)lat2 * Math.PI / 180.0;

            // 计算经度差
            double deltaLongitude = longitude2 - longitude1;

            // 计算航向
            double y = Math.Sin(deltaLongitude) * Math.Cos(latitude2);
            double x = Math.Cos(latitude1) * Math.Sin(latitude2) -
                       Math.Sin(latitude1) * Math.Cos(latitude2) * Math.Cos(deltaLongitude);

            double headingRadians = Math.Atan2(y, x);

            // 将弧度转换为度
            double headingDegrees = headingRadians * 180.0 / Math.PI;

            // 将航向调整到0-360度范围
            headingDegrees = (headingDegrees + 360) % 360;
            return Convert.ToDecimal(Math.Round(headingDegrees, 2));
        }


        #endregion
    }
}
