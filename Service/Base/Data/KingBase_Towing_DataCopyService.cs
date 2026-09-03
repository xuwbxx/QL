using DataFactory.Factory;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Base.Data
{

    public class KingBase_Towing_DataCopyService
    {
        private readonly Towing_KingBase_UnitOfWorkFactory _towingKingBaseUowFactory;

        private readonly Towing_Sql_UnitOfWorkFactory _towingSqlUowFactory;


        public KingBase_Towing_DataCopyService(Towing_KingBase_UnitOfWorkFactory towingKingBaseUowFactory, Towing_Sql_UnitOfWorkFactory towingSqlUowFactory)
        {
            _towingKingBaseUowFactory = towingKingBaseUowFactory;
            _towingSqlUowFactory = towingSqlUowFactory;
        }

        public void CopyData_Backup()
        {
            int batchSize = 10000;
            int skip = 0;
            bool hasMore = true;

            while (hasMore)
            {
                List<DataFactory.SqlServer.Towing.Data_Backup> listSql;
                using (var uow = _towingSqlUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.SqlServer.Towing.Data_Backup>();
                    listSql = repo.FindAll().OrderBy(x => x.id).Skip(skip).Take(batchSize).ToList();
                }

                if (listSql.Count == 0)
                {
                    hasMore = false;
                    break;
                }

                List<DataFactory.KingBase.Towing.Data_Backup> listKing = new List<DataFactory.KingBase.Towing.Data_Backup>();
                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.Towing.Data_Backup()
                    {
                        id = a.id,
                        DataString = a.DataString == null ? null : a.DataString.Replace("\0", ""),
                        CreateTime = a.CreateTime == null ? null : DateTime.SpecifyKind(a.CreateTime.Value, DateTimeKind.Unspecified)
                    });
                });

                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.KingBase.Towing.Data_Backup>();
                    repo.AddList(listKing);
                    repo.Save();
                }

                skip += batchSize;

                if (listSql.Count < batchSize)
                {
                    hasMore = false;
                }
            }
        }

        public void CopyData_Backup_Mru()
        {
            int batchSize = 10000;
            int skip = 0;
            bool hasMore = true;

            while (hasMore)
            {
                List<DataFactory.SqlServer.Towing.Data_Backup_Mru> listSql;
                using (var uow = _towingSqlUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.SqlServer.Towing.Data_Backup_Mru>();
                    listSql = repo.FindAll().OrderBy(x => x.id).Skip(skip).Take(batchSize).ToList();
                }

                if (listSql.Count == 0)
                {
                    hasMore = false;
                    break;
                }

                List<DataFactory.KingBase.Towing.Data_Backup_Mru> listKing = new List<DataFactory.KingBase.Towing.Data_Backup_Mru>();
                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.Towing.Data_Backup_Mru()
                    {
                        id = a.id,
                        DataString = a.DataString == null ? null : a.DataString.Replace("\0", ""),
                        CreateTime = a.CreateTime == null ? null : DateTime.SpecifyKind(a.CreateTime.Value, DateTimeKind.Unspecified)
                    });
                });

                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.KingBase.Towing.Data_Backup_Mru>();
                    repo.AddList(listKing);
                    repo.Save();
                }

                skip += batchSize;

                if (listSql.Count < batchSize)
                {
                    hasMore = false;
                }
            }
        }

        public void CopyData_Backup_Weather()
        {
            List<DataFactory.SqlServer.Towing.Data_Backup_Weather> listSql;
            using (var uow = _towingSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.Towing.Data_Backup_Weather>();
                listSql = repo.FindAll().ToList();
            }

            List<DataFactory.KingBase.Towing.Data_Backup_Weather> listKing = new List<DataFactory.KingBase.Towing.Data_Backup_Weather>();
            listSql.ForEach(a =>
            {
                listKing.Add(new DataFactory.KingBase.Towing.Data_Backup_Weather()
                {
                    id = a.id,
                    bm500 = a.bm500,
                    humidity = a.humidity,
                    pressure = a.pressure,
                    swelldir = a.swelldir,
                    swellheight = a.swellheight,
                    swellperiod = a.swellperiod,
                    temperature = a.temperature,
                    visibility = a.visibility,
                    waveheight = a.waveheight,
                    winddir = a.winddir,
                    winddirStr = a.winddirStr,
                    windspeed = a.windspeed,
                    windlevel = a.windlevel,
                    windlevelStr = a.windlevelStr,
                    windwarninglevel = a.windwarninglevel,
                    windalarm = a.windalarm,
                    time = a.time,
                    timeStr = a.timeStr,
                    oceandir = a.oceandir,
                    oceandirStr = a.oceandirStr,
                    oceanspeed = a.oceanspeed,
                    CreateTime = a.CreateTime == null ? null : DateTime.SpecifyKind(a.CreateTime.Value, DateTimeKind.Unspecified)
                });
            });

            using (var uow = _towingKingBaseUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.KingBase.Towing.Data_Backup_Weather>();
                if (listKing.Count > 0)
                {
                    repo.AddList(listKing);
                    repo.Save();
                }
            }
        }

        public void CopyData_Battery()
        {
            int batchSize = 10000;
            int skip = 0;
            bool hasMore = true;

            while (hasMore)
            {
                List<DataFactory.SqlServer.Towing.Data_Battery> listSql;
                using (var uow = _towingSqlUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.SqlServer.Towing.Data_Battery>();
                    listSql = repo.FindAll().OrderBy(x => x.id).Skip(skip).Take(batchSize).ToList();
                }

                if (listSql.Count == 0)
                {
                    hasMore = false;
                    break;
                }

                List<DataFactory.KingBase.Towing.Data_Battery> listKing = new List<DataFactory.KingBase.Towing.Data_Battery>();
                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.Towing.Data_Battery()
                    {
                        id = a.id,
                        ProjectID = a.ProjectID,
                        RemainEle = a.RemainEle,
                        RemainEle2 = a.RemainEle2,
                        Engine1 = a.Engine1,
                        Engine2 = a.Engine2,
                        Voltage = a.Voltage,
                        CreateTime = a.CreateTime == null ? null : DateTime.SpecifyKind(a.CreateTime.Value, DateTimeKind.Unspecified),
                        CreateTime2 = a.CreateTime2 == null ? null : DateTime.SpecifyKind(a.CreateTime2.Value, DateTimeKind.Unspecified),
                        IsDelete = a.IsDelete
                    });
                });

                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.KingBase.Towing.Data_Battery>();
                    repo.AddList(listKing);
                    repo.Save();
                }

                skip += batchSize;

                if (listSql.Count < batchSize)
                {
                    hasMore = false;
                }
            }
        }

        public void CopyData_Device()
        {
            List<DataFactory.SqlServer.Towing.Data_Device> listSql;
            using (var uow = _towingSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.Towing.Data_Device>();
                listSql = repo.FindAll().ToList();
            }

            List<DataFactory.KingBase.Towing.Data_Device> listKing = new List<DataFactory.KingBase.Towing.Data_Device>();
            listSql.ForEach(a =>
            {
                listKing.Add(new DataFactory.KingBase.Towing.Data_Device()
                {
                    id = a.id,
                    ProjectID = a.ProjectID,
                    Type = a.Type,
                    DeviceName = a.DeviceName,
                    LatestUpdateTime = a.LatestUpdateTime == null ? null : DateTime.SpecifyKind(a.LatestUpdateTime.Value, DateTimeKind.Unspecified),
                    CreateTime = a.CreateTime == null ? null : DateTime.SpecifyKind(a.CreateTime.Value, DateTimeKind.Unspecified),
                    IsDelete = a.IsDelete
                });
            });

            using (var uow = _towingKingBaseUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.KingBase.Towing.Data_Device>();
                if (listKing.Count > 0)
                {
                    repo.AddList(listKing);
                    repo.Save();
                }
            }
        }

        public void CopyData_EGYRO()
        {
            int batchSize = 10000;
            int skip = 0;
            bool hasMore = true;

            while (hasMore)
            {
                List<DataFactory.SqlServer.Towing.Data_EGYRO> listSql;
                using (var uow = _towingSqlUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.SqlServer.Towing.Data_EGYRO>();
                    listSql = repo.FindAll().OrderBy(x => x.id).Skip(skip).Take(batchSize).ToList();
                }

                if (listSql.Count == 0)
                {
                    hasMore = false;
                    break;
                }

                List<DataFactory.KingBase.Towing.Data_EGYRO> listKing = new List<DataFactory.KingBase.Towing.Data_EGYRO>();
                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.Towing.Data_EGYRO()
                    {
                        id = a.id,
                        DeviceID = a.DeviceID,
                        AngleX = a.AngleX,
                        AngleY = a.AngleY,
                        AzAngle = a.AzAngle,
                        CreateTime = a.CreateTime == null ? null : DateTime.SpecifyKind(a.CreateTime.Value, DateTimeKind.Unspecified),
                        CreateTime2 = a.CreateTime2 == null ? null : DateTime.SpecifyKind(a.CreateTime2.Value, DateTimeKind.Unspecified),
                        IsDelete = a.IsDelete
                    });
                });

                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.KingBase.Towing.Data_EGYRO>();
                    repo.AddList(listKing);
                    repo.Save();
                }

                skip += batchSize;

                if (listSql.Count < batchSize)
                {
                    hasMore = false;
                }
            }
        }

        public void CopyData_Forecast()
        {
            int batchSize = 10000;
            int skip = 0;
            bool hasMore = true;

            while (hasMore)
            {
                List<DataFactory.SqlServer.Towing.Data_Forecast> listSql;
                using (var uow = _towingSqlUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.SqlServer.Towing.Data_Forecast>();
                    listSql = repo.FindAll().OrderBy(x => x.id).Skip(skip).Take(batchSize).ToList();
                }

                if (listSql.Count == 0)
                {
                    hasMore = false;
                    break;
                }

                List<DataFactory.KingBase.Towing.Data_Forecast> listKing = new List<DataFactory.KingBase.Towing.Data_Forecast>();
                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.Towing.Data_Forecast()
                    {
                        id = a.id,
                        No = a.No,
                        ForecastTime = a.ForecastTime == null ? null : DateTime.SpecifyKind(a.ForecastTime.Value, DateTimeKind.Unspecified),
                        Lat = a.Lat,
                        Lon = a.Lon,
                        MaxYaoX = a.MaxYaoX,
                        MaxYaoY = a.MaxYaoY,
                        MaxDangZ = a.MaxDangZ,
                        TotalAlarm = a.TotalAlarm,
                        WindAlarm = a.WindAlarm,
                        WaveAlarm = a.WaveAlarm,
                        ShakeAlarm = a.ShakeAlarm,
                        CreateTime = a.CreateTime == null ? null : DateTime.SpecifyKind(a.CreateTime.Value, DateTimeKind.Unspecified),
                        IsDelete = a.IsDelete
                    });
                });

                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.KingBase.Towing.Data_Forecast>();
                    repo.AddList(listKing);
                    repo.Save();
                }

                skip += batchSize;

                if (listSql.Count < batchSize)
                {
                    hasMore = false;
                }
            }
        }

        public void CopyData_HikLocalPic()
        {
            int batchSize = 10000;
            int skip = 0;
            bool hasMore = true;

            while (hasMore)
            {
                List<DataFactory.SqlServer.Towing.Data_HikLocalPic> listSql;
                using (var uow = _towingSqlUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.SqlServer.Towing.Data_HikLocalPic>();
                    listSql = repo.FindAll().OrderBy(x => x.id).Skip(skip).Take(batchSize).ToList();
                }

                if (listSql.Count == 0)
                {
                    hasMore = false;
                    break;
                }

                List<DataFactory.KingBase.Towing.Data_HikLocalPic> listKing = new List<DataFactory.KingBase.Towing.Data_HikLocalPic>();
                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.Towing.Data_HikLocalPic()
                    {
                        id = a.id,
                        ProjID = a.ProjID,
                        SnapTime = a.SnapTime == null ? null : DateTime.SpecifyKind(a.SnapTime.Value, DateTimeKind.Unspecified),
                        SnapPicUrl = a.SnapPicUrl,
                        CreateTime = a.CreateTime == null ? null : DateTime.SpecifyKind(a.CreateTime.Value, DateTimeKind.Unspecified),
                        IsDelete = a.IsDelete
                    });
                });

                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.KingBase.Towing.Data_HikLocalPic>();
                    repo.AddList(listKing);
                    repo.Save();
                }

                skip += batchSize;

                if (listSql.Count < batchSize)
                {
                    hasMore = false;
                }
            }
        }

        public void CopyData_MRU()
        {
            int batchSize = 50000;   // 每次从SQL Server读取的行数
            int insertBatch = 5000;  // 每条INSERT语句包含的行数
            int lastId = 0;
            bool hasMore = true;

            var kingConn = _towingKingBaseUowFactory.GetConnection();
            var wasClosed = kingConn.State == ConnectionState.Closed;
            if (wasClosed) kingConn.Open();

            try
            {
                while (hasMore)
                {
                    // 使用keyset分页替代Skip/Take，避免O(n²)扫描
                    List<DataFactory.SqlServer.Towing.Data_MRU> listSql;
                    using (var uow = _towingSqlUowFactory.Create())
                    {
                        var repo = uow.GetRepository<DataFactory.SqlServer.Towing.Data_MRU>();
                        listSql = repo.FindAll()
                            .Where(x => x.id > lastId)
                            .OrderBy(x => x.id)
                            .Take(batchSize)
                            .ToList();
                    }

                    if (listSql.Count == 0)
                    {
                        hasMore = false;
                        break;
                    }

                    // 分批构建批量INSERT语句
                    for (int i = 0; i < listSql.Count; i += insertBatch)
                    {
                        var sb = new StringBuilder();
                        sb.Append("INSERT INTO \"Data_MRU\" (id,\"DeviceID\",\"DangX\",\"DangY\",\"DangZ\",\"YaoX\",\"YaoY\",\"YaoZ\",\"CreateTime\",\"CreateTime2\",\"IsDelete\") VALUES ");

                        int count = Math.Min(insertBatch, listSql.Count - i);
                        for (int j = 0; j < count; j++)
                        {
                            var a = listSql[i + j];
                            if (j > 0) sb.Append(',');
                            var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                            var ct2 = a.CreateTime2 == null ? "NULL" : $"'{a.CreateTime2.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                            sb.Append($"({a.id},{a.DeviceID?.ToString() ?? "NULL"},{a.DangX?.ToString() ?? "NULL"},{a.DangY?.ToString() ?? "NULL"},{a.DangZ?.ToString() ?? "NULL"},{a.YaoX?.ToString() ?? "NULL"},{a.YaoY?.ToString() ?? "NULL"},{a.YaoZ?.ToString() ?? "NULL"},{ct},{ct2},{(a.IsDelete ? "true" : "false")})");
                        }

                        using var cmd = kingConn.CreateCommand();
                        cmd.CommandText = sb.ToString();
                        using var tx = kingConn.BeginTransaction();
                        cmd.Transaction = tx;
                        cmd.ExecuteNonQuery();
                        tx.Commit();
                    }

                    lastId = listSql[listSql.Count - 1].id;
                    if (listSql.Count < batchSize)
                    {
                        hasMore = false;
                    }
                }
            }
            finally
            {
                if (wasClosed) kingConn.Close();
            }
        }

        public void CopyData_SailSpeed()
        {
            int batchSize = 10000;
            int skip = 0;
            bool hasMore = true;

            while (hasMore)
            {
                List<DataFactory.SqlServer.Towing.Data_SailSpeed> listSql;
                using (var uow = _towingSqlUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.SqlServer.Towing.Data_SailSpeed>();
                    listSql = repo.FindAll().OrderBy(x => x.id).Skip(skip).Take(batchSize).ToList();
                }

                if (listSql.Count == 0)
                {
                    hasMore = false;
                    break;
                }

                List<DataFactory.KingBase.Towing.Data_SailSpeed> listKing = new List<DataFactory.KingBase.Towing.Data_SailSpeed>();
                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.Towing.Data_SailSpeed()
                    {
                        id = a.id,
                        ProjectID = a.ProjectID,
                        SailSpeed = a.SailSpeed,
                        CreateTime = a.CreateTime == null ? null : DateTime.SpecifyKind(a.CreateTime.Value, DateTimeKind.Unspecified),
                        CreateTime2 = a.CreateTime2 == null ? null : DateTime.SpecifyKind(a.CreateTime2.Value, DateTimeKind.Unspecified),
                        IsDelete = a.IsDelete
                    });
                });

                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.KingBase.Towing.Data_SailSpeed>();
                    repo.AddList(listKing);
                    repo.Save();
                }

                skip += batchSize;

                if (listSql.Count < batchSize)
                {
                    hasMore = false;
                }
            }
        }

        public void CopyData_SATELLITE()
        {
            int batchSize = 10000;
            int skip = 0;
            bool hasMore = true;

            while (hasMore)
            {
                List<DataFactory.SqlServer.Towing.Data_SATELLITE> listSql;
                using (var uow = _towingSqlUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.SqlServer.Towing.Data_SATELLITE>();
                    listSql = repo.FindAll().OrderBy(x => x.id).Skip(skip).Take(batchSize).ToList();
                }

                if (listSql.Count == 0)
                {
                    hasMore = false;
                    break;
                }

                List<DataFactory.KingBase.Towing.Data_SATELLITE> listKing = new List<DataFactory.KingBase.Towing.Data_SATELLITE>();
                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.Towing.Data_SATELLITE()
                    {
                        id = a.id,
                        DeviceID = a.DeviceID,
                        Lon = a.Lon,
                        LonDirection = a.LonDirection,
                        Lat = a.Lat,
                        LatDirection = a.LatDirection,
                        Altitude = a.Altitude,
                        Heading = a.Heading,
                        CreateTime = a.CreateTime == null ? null : DateTime.SpecifyKind(a.CreateTime.Value, DateTimeKind.Unspecified),
                        CreateTime2 = a.CreateTime2 == null ? null : DateTime.SpecifyKind(a.CreateTime2.Value, DateTimeKind.Unspecified),
                        IsDelete = a.IsDelete
                    });
                });

                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.KingBase.Towing.Data_SATELLITE>();
                    repo.AddList(listKing);
                    repo.Save();
                }

                skip += batchSize;

                if (listSql.Count < batchSize)
                {
                    hasMore = false;
                }
            }
        }

        public void CopyData_Wind()
        {
            int batchSize = 10000;
            int skip = 0;
            bool hasMore = true;

            while (hasMore)
            {
                List<DataFactory.SqlServer.Towing.Data_Wind> listSql;
                using (var uow = _towingSqlUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.SqlServer.Towing.Data_Wind>();
                    listSql = repo.FindAll().OrderBy(x => x.id).Skip(skip).Take(batchSize).ToList();
                }

                if (listSql.Count == 0)
                {
                    hasMore = false;
                    break;
                }

                List<DataFactory.KingBase.Towing.Data_Wind> listKing = new List<DataFactory.KingBase.Towing.Data_Wind>();
                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.Towing.Data_Wind()
                    {
                        id = a.id,
                        DeviceID = a.DeviceID,
                        WindSpeed = a.WindSpeed,
                        WindDirection = a.WindDirection,
                        CreateTime = a.CreateTime == null ? null : DateTime.SpecifyKind(a.CreateTime.Value, DateTimeKind.Unspecified),
                        CreateTime2 = a.CreateTime2 == null ? null : DateTime.SpecifyKind(a.CreateTime2.Value, DateTimeKind.Unspecified),
                        IsDelete = a.IsDelete
                    });
                });

                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.KingBase.Towing.Data_Wind>();
                    repo.AddList(listKing);
                    repo.Save();
                }

                skip += batchSize;

                if (listSql.Count < batchSize)
                {
                    hasMore = false;
                }
            }
        }

        public void CopyData_Manage_Area()
        {
            int batchSize = 10000;
            int skip = 0;
            bool hasMore = true;

            while (hasMore)
            {
                List<DataFactory.SqlServer.Towing.Manage_Area> listSql;
                using (var uow = _towingSqlUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.SqlServer.Towing.Manage_Area>();
                    listSql = repo.FindAll().OrderBy(x => x.ID).Skip(skip).Take(batchSize).ToList();
                }

                if (listSql.Count == 0)
                {
                    hasMore = false;
                    break;
                }

                List<DataFactory.KingBase.Towing.Manage_Area> listKing = new List<DataFactory.KingBase.Towing.Manage_Area>();
                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.Towing.Manage_Area()
                    {
                        id = a.ID,
                        ProjectID = a.ProjectID,
                        Lon = a.Lon,
                        Lat = a.Lat,
                        CreateTime = a.CreateTime == null ? null : DateTime.SpecifyKind(a.CreateTime.Value, DateTimeKind.Unspecified),
                        IsDelete = a.IsDelete
                    });
                });

                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.KingBase.Towing.Manage_Area>();
                    repo.AddList(listKing);
                    repo.Save();
                }

                skip += batchSize;

                if (listSql.Count < batchSize)
                {
                    hasMore = false;
                }
            }
        }

        public void CopyData_Manage_Basic()
        {
            int batchSize = 10000;
            int skip = 0;
            bool hasMore = true;

            while (hasMore)
            {
                List<DataFactory.SqlServer.Towing.Manage_Basic> listSql;
                using (var uow = _towingSqlUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.SqlServer.Towing.Manage_Basic>();
                    listSql = repo.FindAll().OrderBy(x => x.id).Skip(skip).Take(batchSize).ToList();
                }

                if (listSql.Count == 0)
                {
                    hasMore = false;
                    break;
                }

                List<DataFactory.KingBase.Towing.Manage_Basic> listKing = new List<DataFactory.KingBase.Towing.Manage_Basic>();
                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.Towing.Manage_Basic()
                    {
                        id = a.id,
                        ProjectID = a.ProjectID,
                        ProjectStartTime = a.ProjectStartTime == null ? null : DateTime.SpecifyKind(a.ProjectStartTime.Value, DateTimeKind.Unspecified),
                        StartLon = a.StartLon,
                        StartLat = a.StartLat,
                        EndLon = a.EndLon,
                        EndLat = a.EndLat,
                        CreateTime = a.CreateTime == null ? null : DateTime.SpecifyKind(a.CreateTime.Value, DateTimeKind.Unspecified),
                        IsDelete = a.IsDelete
                    });
                });

                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.KingBase.Towing.Manage_Basic>();
                    repo.AddList(listKing);
                    repo.Save();
                }

                skip += batchSize;

                if (listSql.Count < batchSize)
                {
                    hasMore = false;
                }
            }
        }

        public void CopyData_Manage_PlanRoute()
        {
            int batchSize = 10000;
            int skip = 0;
            bool hasMore = true;

            while (hasMore)
            {
                List<DataFactory.SqlServer.Towing.Manage_PlanRoute> listSql;
                using (var uow = _towingSqlUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.SqlServer.Towing.Manage_PlanRoute>();
                    listSql = repo.FindAll().OrderBy(x => x.id).Skip(skip).Take(batchSize).ToList();
                }

                if (listSql.Count == 0)
                {
                    hasMore = false;
                    break;
                }

                List<DataFactory.KingBase.Towing.Manage_PlanRoute> listKing = new List<DataFactory.KingBase.Towing.Manage_PlanRoute>();
                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.Towing.Manage_PlanRoute()
                    {
                        id = a.id,
                        ProjectID = a.ProjectID,
                        Lon = a.Lon,
                        Lat = a.Lat,
                        RouteTime = a.RouteTime == null ? null : DateTime.SpecifyKind(a.RouteTime.Value, DateTimeKind.Unspecified),
                        CreateTime = a.CreateTime == null ? null : DateTime.SpecifyKind(a.CreateTime.Value, DateTimeKind.Unspecified),
                        IsDelete = a.IsDelete
                    });
                });

                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.KingBase.Towing.Manage_PlanRoute>();
                    repo.AddList(listKing);
                    repo.Save();
                }

                skip += batchSize;

                if (listSql.Count < batchSize)
                {
                    hasMore = false;
                }
            }
        }

        public void CopyData_Manage_ShipAnchorage()
        {
            int batchSize = 10000;
            int skip = 0;
            bool hasMore = true;

            while (hasMore)
            {
                List<DataFactory.SqlServer.Towing.Manage_ShipAnchorage> listSql;
                using (var uow = _towingSqlUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.SqlServer.Towing.Manage_ShipAnchorage>();
                    listSql = repo.FindAll().OrderBy(x => x.id).Skip(skip).Take(batchSize).ToList();
                }

                if (listSql.Count == 0)
                {
                    hasMore = false;
                    break;
                }

                List<DataFactory.KingBase.Towing.Manage_ShipAnchorage> listKing = new List<DataFactory.KingBase.Towing.Manage_ShipAnchorage>();
                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.Towing.Manage_ShipAnchorage()
                    {
                        id = a.id,
                        Type = a.Type,
                        Group = a.Group,
                        Name = a.Name,
                        Lon = a.Lon,
                        Lat = a.Lat,
                        Radius = a.Radius,
                        CreateTime = a.CreateTime == null ? null : DateTime.SpecifyKind(a.CreateTime.Value, DateTimeKind.Unspecified),
                        IsDelete = a.IsDelete
                    });
                });

                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.KingBase.Towing.Manage_ShipAnchorage>();
                    repo.AddList(listKing);
                    repo.Save();
                }

                skip += batchSize;

                if (listSql.Count < batchSize)
                {
                    hasMore = false;
                }
            }
        }

        public void CopyData_Manage_User()
        {
            List<DataFactory.SqlServer.Towing.Manage_User> listSql;
            using (var uow = _towingSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.Towing.Manage_User>();
                listSql = repo.FindAll().ToList();
            }

            List<DataFactory.KingBase.Towing.Manage_User> listKing = new List<DataFactory.KingBase.Towing.Manage_User>();
            listSql.ForEach(a =>
            {
                listKing.Add(new DataFactory.KingBase.Towing.Manage_User()
                {
                    id = a.id,
                    UserName = a.UserName,
                    RealName = a.RealName,
                    Depart = a.Depart,
                    IsConfirm = a.IsConfirm,
                    Role = a.Role,
                    CreateTime = a.CreateTime == null ? null : DateTime.SpecifyKind(a.CreateTime.Value, DateTimeKind.Unspecified),
                    IsDelete = a.IsDelete
                });
            });

            using (var uow = _towingKingBaseUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.KingBase.Towing.Manage_User>();
                if (listKing.Count > 0)
                {
                    repo.AddList(listKing);
                    repo.Save();
                }
            }
        }

        public void CopyData_Message_Body()
        {
            int batchSize = 10000;
            int skip = 0;
            bool hasMore = true;

            while (hasMore)
            {
                List<DataFactory.SqlServer.Towing.Message_Body> listSql;
                using (var uow = _towingSqlUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.SqlServer.Towing.Message_Body>();
                    listSql = repo.FindAll().OrderBy(x => x.id).Skip(skip).Take(batchSize).ToList();
                }

                if (listSql.Count == 0)
                {
                    hasMore = false;
                    break;
                }

                List<DataFactory.KingBase.Towing.Message_Body> listKing = new List<DataFactory.KingBase.Towing.Message_Body>();
                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.Towing.Message_Body()
                    {
                        id = a.id,
                        MsgType = a.MsgType,
                        MsgLevel = a.MsgLevel,
                        MsgContent = a.MsgContent,
                        Sender = a.Sender,
                        CreateTime = a.CreateTime == null ? null : DateTime.SpecifyKind(a.CreateTime.Value, DateTimeKind.Unspecified),
                        IsDelete = a.IsDelete
                    });
                });

                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.KingBase.Towing.Message_Body>();
                    repo.AddList(listKing);
                    repo.Save();
                }

                skip += batchSize;

                if (listSql.Count < batchSize)
                {
                    hasMore = false;
                }
            }
        }

        

    }

    

}
