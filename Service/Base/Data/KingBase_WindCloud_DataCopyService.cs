using DataFactory.Factory;
using DataFactory.KingBase.CloudWind;
using System.Data;
using System.Text;

namespace Service.Base.Data
{
    public class KingBase_WindCloud_DataCopyService
    {

        private readonly CloudWind_KingBase_UnitOfWorkFactory _windKingBaseUowFactory;

        private readonly CloudWind_Sql_UnitOfWorkFactory _windSqlUowFactory;


        public KingBase_WindCloud_DataCopyService(CloudWind_KingBase_UnitOfWorkFactory windCenterUowFactory, CloudWind_Sql_UnitOfWorkFactory windSqlUowFactory)
        {
            _windKingBaseUowFactory = windCenterUowFactory;
            _windSqlUowFactory = windSqlUowFactory;
        }

        public void DataSave()
        {
            List<DataFactory.SqlServer.CloudWind.Wind_ProjectRole> listSql = new List<DataFactory.SqlServer.CloudWind.Wind_ProjectRole>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Wind_ProjectRole>();

                listSql = repo.FindAll().ToList();


            }

            List<DataFactory.KingBase.CloudWind.Wind_ProjectRole> listKing = new List<Wind_ProjectRole>();
            using (var uow = _windKingBaseUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.KingBase.CloudWind.Wind_ProjectRole>();

                listSql.ForEach(a =>
                {
                    listKing.Add(new Wind_ProjectRole()
                    {
                        ProjectID = a.ProjectID,
                        RoleID = a.RoleID,
                        UserName = a.UserName,
                        UserCode = a.UserCode,
                        UserDepartName = a.UserDepartName,
                        UserPhone = a.UserPhone,
                        UserJobName = a.UserJobName,
                        CreateTime = a.CreateTime == null ? null : DateTime.SpecifyKind(a.CreateTime.Value, DateTimeKind.Utc),
                        IsDelete = a.IsDelete
                    });
                });

                if (listKing.Count > 0)
                {
                    repo.AddList(listKing);
                    repo.Save();
                }


            }



        }

        public void CopyLibrary_Geology()
        {
            List<DataFactory.SqlServer.CloudWind.Library_Geology> listSql = new List<DataFactory.SqlServer.CloudWind.Library_Geology>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Library_Geology>();
                listSql = repo.FindAll().ToList();
            }

            List<DataFactory.KingBase.CloudWind.Library_Geology> listKing = new List<DataFactory.KingBase.CloudWind.Library_Geology>();
            using (var uow = _windKingBaseUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.KingBase.CloudWind.Library_Geology>();

                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.CloudWind.Library_Geology()
                    {
                        ProjectID = a.ProjectID,
                        Type = a.Type,
                        FileName = a.FileName,
                        FilePath = a.FilePath,
                        CreateTime = a.CreateTime == null ? null : DateTime.SpecifyKind(a.CreateTime.Value, DateTimeKind.Utc),
                        IsDelete = a.IsDelete
                    });
                });

                if (listKing.Count > 0)
                {
                    repo.AddList(listKing);
                    repo.Save();
                }
            }
        }

        public void CopyLibrary_Geology_DK()
        {
            List<DataFactory.SqlServer.CloudWind.Library_Geology_DK> listSql = new List<DataFactory.SqlServer.CloudWind.Library_Geology_DK>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Library_Geology_DK>();
                listSql = repo.FindAll().ToList();
            }

            List<DataFactory.KingBase.CloudWind.Library_Geology_DK> listKing = new List<DataFactory.KingBase.CloudWind.Library_Geology_DK>();
            using (var uow = _windKingBaseUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.KingBase.CloudWind.Library_Geology_DK>();

                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.CloudWind.Library_Geology_DK()
                    {
                        ProjectID = a.ProjectID,
                        FanID = a.FanID,
                        DKName = a.DKName,
                        CreateTime = a.CreateTime == null ? null : DateTime.SpecifyKind(a.CreateTime.Value, DateTimeKind.Utc),
                        IsDelete = a.IsDelete
                    });
                });

                if (listKing.Count > 0)
                {
                    repo.AddList(listKing);
                    repo.Save();
                }
            }
        }

        public void CopyLibrary_Geology_Data()
        {
            DataTable dt = new DataTable();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Library_Geology_Data>();
                dt = repo.QueryDataTable("select * from Library_Geology_Data");
            }

            List<DataFactory.KingBase.CloudWind.Library_Geology_Data> listKing = new List<DataFactory.KingBase.CloudWind.Library_Geology_Data>();
            using (var uow = _windKingBaseUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.KingBase.CloudWind.Library_Geology_Data>();

                foreach (DataRow dr in dt.Rows)
                {
                    listKing.Add(new DataFactory.KingBase.CloudWind.Library_Geology_Data()
                    {
                        DKID = Convert.ToInt32(dr[1].ToString()),
                        xh = dr[2].ToString(),
                        dcbh = dr[3].ToString(),
                        tcbh = dr[4].ToString(),
                        cdbg = dr[5].ToString(),
                        tclx = dr[6].ToString(),
                        bpskjqd = dr[7].ToString(),
                        stmcj = dr[8].ToString(),
                        yxzd = dr[9].ToString(),
                        bgjs = dr[10].ToString(),
                        CreateTime = string.IsNullOrEmpty(dr[11].ToString()) ? null : DateTime.SpecifyKind(Convert.ToDateTime(dr[11].ToString()), DateTimeKind.Utc),
                        IsDelete = Convert.ToBoolean(dr[12].ToString())
                    });
                }


                if (listKing.Count > 0)
                {
                    repo.AddList(listKing);
                    repo.Save();
                }
            }
        }

        public void CopyManage_Company()
        {
            List<DataFactory.SqlServer.CloudWind.Manage_Company> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Manage_Company>();
                listSql = repo.FindAll().ToList();
            }
            if (listSql.Count == 0) return;

            var kingConn = _windKingBaseUowFactory.GetConnection();
            var wasClosed = kingConn.State == ConnectionState.Closed;
            if (wasClosed) kingConn.Open();
            try
            {
                int insertBatch = 500;
                for (int i = 0; i < listSql.Count; i += insertBatch)
                {
                    var sb = new StringBuilder();
                    sb.Append("INSERT INTO \"Manage_Company\" (\"ID\",\"Company\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var co = a.Company == null ? "NULL" : $"'{a.Company.Replace("'", "''")}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete ? "true" : "false";
                        sb.Append($"({a.ID},{co},{ct},{isd})");
                    }
                    using var cmd = kingConn.CreateCommand();
                    cmd.CommandText = sb.ToString();
                    using var tx = kingConn.BeginTransaction();
                    cmd.Transaction = tx;
                    cmd.ExecuteNonQuery();
                    tx.Commit();
                }
            }
            finally { if (wasClosed) kingConn.Close(); }
        }

        public void CopyWind_ProjectFile()
        {
            List<DataFactory.SqlServer.CloudWind.Wind_ProjectFile> listSql = new List<DataFactory.SqlServer.CloudWind.Wind_ProjectFile>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Wind_ProjectFile>();
                listSql = repo.FindAll().ToList();
            }

            List<DataFactory.KingBase.CloudWind.Wind_ProjectFile> listKing = new List<DataFactory.KingBase.CloudWind.Wind_ProjectFile>();
            using (var uow = _windKingBaseUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.KingBase.CloudWind.Wind_ProjectFile>();

                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.CloudWind.Wind_ProjectFile()
                    {
                        ProjectID = a.ProjectID,
                        FileName = a.FileName,
                        FilePath = a.FilePath,
                        CreateTime = a.CreateTime == null ? null : DateTime.SpecifyKind(a.CreateTime.Value, DateTimeKind.Utc),
                        IsDelete = a.IsDelete
                    });
                });

                if (listKing.Count > 0)
                {
                    repo.AddList(listKing);
                    repo.Save();
                }
            }
        }

        public void CopyManage_Viewer()
        {
            List<DataFactory.SqlServer.CloudWind.Manage_Viewer> listSql = new List<DataFactory.SqlServer.CloudWind.Manage_Viewer>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Manage_Viewer>();
                listSql = repo.FindAll().ToList();
            }

            List<DataFactory.KingBase.CloudWind.Manage_Viewer> listKing = new List<DataFactory.KingBase.CloudWind.Manage_Viewer>();
            using (var uow = _windKingBaseUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.KingBase.CloudWind.Manage_Viewer>();

                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.CloudWind.Manage_Viewer()
                    {
                        UserName = a.UserName,
                        UserCode = a.UserCode,
                        UserDepartName = a.UserDepartName,
                        UserPhone = a.UserPhone,
                        UserJobName = a.UserJobName,
                        CreateTime = a.CreateTime == null ? null : DateTime.SpecifyKind(a.CreateTime.Value, DateTimeKind.Utc),
                        IsDelete = a.IsDelete
                    });
                });

                if (listKing.Count > 0)
                {
                    repo.AddList(listKing);
                    repo.Save();
                }
            }
        }

        public void CopyLibrary_Pile_Data()
        {
            DataTable dt = new DataTable();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Library_Pile_Data>();
                dt = repo.QueryDataTable("select * from Library_Pile_Data");
            }

            List<DataFactory.KingBase.CloudWind.Library_Pile_Data> listKing = new List<DataFactory.KingBase.CloudWind.Library_Pile_Data>();
            using (var uow = _windKingBaseUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.KingBase.CloudWind.Library_Pile_Data>();

                foreach (DataRow dr in dt.Rows)
                {
                    listKing.Add(new DataFactory.KingBase.CloudWind.Library_Pile_Data()
                    {
                        PileID = Convert.ToInt32(dr[1].ToString()),
                        PileName = dr[2].ToString(),
                        mdcd = dr[3].ToString(),
                        zc = dr[4].ToString(),
                        fzzwj = dr[5].ToString(),
                        bh = dr[6].ToString(),
                        zzl = dr[7].ToString(),
                        zdgcT = dr[8].ToString(),
                        zcm = dr[9].ToString(),
                        zdgcB = dr[10].ToString(),
                        CreateTime = string.IsNullOrEmpty(dr[11].ToString()) ? null : DateTime.SpecifyKind(Convert.ToDateTime(dr[11].ToString()), DateTimeKind.Unspecified),
                        IsDelete = Convert.ToBoolean(dr[12].ToString())
                    });
                }


                if (listKing.Count > 0)
                {
                    repo.AddList(listKing);
                    repo.Save();
                }
            }
        }

        public void CopyLibrary_Ship_Data()
        {
            DataTable dt = new DataTable();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Library_Ship_Data>();
                dt = repo.QueryDataTable("select * from Library_Ship_Data");
            }
            if (dt.Rows.Count == 0) return;

            var kingConn = _windKingBaseUowFactory.GetConnection();
            var wasClosed = kingConn.State == ConnectionState.Closed;
            if (wasClosed) kingConn.Open();
            try
            {
                int insertBatch = 500;
                var rows = dt.Rows.Cast<DataRow>().ToList();
                for (int i = 0; i < rows.Count; i += insertBatch)
                {
                    var sb = new StringBuilder();
                    sb.Append("INSERT INTO \"Library_Ship_Data\" (\"ID\",\"ShipID\",\"ztzj\",\"ztzj_unit\",\"ztzc\",\"ztzc_unit\",\"zxcdL\",\"zxcdL_unit\",\"zxkdB\",\"zxkdB_unit\",\"zxgdH\",\"zxgdH_unit\",\"zxmjA\",\"zxmjA_unit\",\"zxzdjmzc\",\"zxzdjmzc_unit\",\"zxtjV\",\"zxtjV_unit\",\"ztzxzzW\",\"ztzxzzW_unit\",\"ztyyl\",\"ztyyl_unit\",\"jsyyhz\",\"jsyyhz_unit\",\"bzl\",\"bzl_unit\",\"ddby\",\"ddby_unit\",\"yxztcd_cddxd\",\"yxztcd_cddxd_unit\",\"qx_cddsm\",\"qx_cddsm_unit\",\"ztyxcd\",\"ztyxcd_unit\",\"ztjmj\",\"ztjmj_unit\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, rows.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var dr = dt.Rows[i + j];
                        if (j > 0) sb.Append(',');
                        string S(int idx) { var v = dr[idx].ToString(); return string.IsNullOrEmpty(v) ? "NULL" : $"'{v.Replace("'", "''")}'"; }
                        var id = dr[0].ToString();
                        var shipid = dr[1].ToString();
                        var ct = string.IsNullOrEmpty(dr[36].ToString()) ? "NULL" : $"'{Convert.ToDateTime(dr[36].ToString()):yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = Convert.ToBoolean(dr[37].ToString()) ? "true" : "false";
                        sb.Append($"({id},{shipid},{S(2)},{S(3)},{S(4)},{S(5)},{S(6)},{S(7)},{S(8)},{S(9)},{S(10)},{S(11)},{S(12)},{S(13)},{S(14)},{S(15)},{S(16)},{S(17)},{S(18)},{S(19)},{S(20)},{S(21)},{S(22)},{S(23)},{S(24)},{S(25)},{S(26)},{S(27)},{S(28)},{S(29)},{S(30)},{S(31)},{S(32)},{S(33)},{S(34)},{S(35)},{ct},{isd})");
                    }
                    using var cmd = kingConn.CreateCommand();
                    cmd.CommandText = sb.ToString();
                    using var tx = kingConn.BeginTransaction();
                    cmd.Transaction = tx;
                    cmd.ExecuteNonQuery();
                    tx.Commit();
                }
            }
            finally { if (wasClosed) kingConn.Close(); }
        }

        public void CopyWind_ProjectInfo()
        {
            List<DataFactory.SqlServer.CloudWind.Wind_ProjectInfo> listSql = new List<DataFactory.SqlServer.CloudWind.Wind_ProjectInfo>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Wind_ProjectInfo>();
                listSql = repo.FindAll().ToList();
            }

            List<DataFactory.KingBase.CloudWind.Wind_ProjectInfo> listKing = new List<DataFactory.KingBase.CloudWind.Wind_ProjectInfo>();
            using (var uow = _windKingBaseUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.KingBase.CloudWind.Wind_ProjectInfo>();

                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.CloudWind.Wind_ProjectInfo()
                    {
                        ProjectID = a.ProjectID,
                        WaterDepth = a.WaterDepth,
                        WaterDepthMin = a.WaterDepthMin,
                        WaterDepthMax = a.WaterDepthMax,
                        CreateTime = a.CreateTime == null ? null : DateTime.SpecifyKind(a.CreateTime.Value, DateTimeKind.Utc),
                        IsDelete = a.IsDelete
                    });
                });

                if (listKing.Count > 0)
                {
                    repo.AddList(listKing);
                    repo.Save();
                }
            }
        }

        public void CopyWind_ProjectRole()
        {
            List<DataFactory.SqlServer.CloudWind.Wind_ProjectRole> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Wind_ProjectRole>();
                listSql = repo.FindAll().ToList();
            }
            if (listSql.Count == 0) return;

            var kingConn = _windKingBaseUowFactory.GetConnection();
            var wasClosed = kingConn.State == ConnectionState.Closed;
            if (wasClosed) kingConn.Open();
            try
            {
                int insertBatch = 500;
                for (int i = 0; i < listSql.Count; i += insertBatch)
                {
                    var sb = new StringBuilder();
                    sb.Append("INSERT INTO \"Wind_ProjectRole\" (\"ID\",\"ProjectID\",\"RoleID\",\"UserName\",\"UserCode\",\"UserDepartName\",\"UserPhone\",\"UserJobName\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var pid = a.ProjectID?.ToString() ?? "NULL";
                        var rid = a.RoleID?.ToString() ?? "NULL";
                        var un = a.UserName == null ? "NULL" : $"'{a.UserName.Replace("'", "''")}'";
                        var uc = a.UserCode == null ? "NULL" : $"'{a.UserCode.Replace("'", "''")}'";
                        var ud = a.UserDepartName == null ? "NULL" : $"'{a.UserDepartName.Replace("'", "''")}'";
                        var up = a.UserPhone == null ? "NULL" : $"'{a.UserPhone.Replace("'", "''")}'";
                        var uj = a.UserJobName == null ? "NULL" : $"'{a.UserJobName.Replace("'", "''")}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete ? "true" : "false";
                        sb.Append($"({a.ID},{pid},{rid},{un},{uc},{ud},{up},{uj},{ct},{isd})");
                    }
                    using var cmd = kingConn.CreateCommand();
                    cmd.CommandText = sb.ToString();
                    using var tx = kingConn.BeginTransaction();
                    cmd.Transaction = tx;
                    cmd.ExecuteNonQuery();
                    tx.Commit();
                }
            }
            finally { if (wasClosed) kingConn.Close(); }
        }

        public void CopyWind_ProjectArea()
        {
            List<DataFactory.SqlServer.CloudWind.Wind_ProjectArea> listSql = new List<DataFactory.SqlServer.CloudWind.Wind_ProjectArea>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Wind_ProjectArea>();
                listSql = repo.FindAll().ToList();
            }

            List<DataFactory.KingBase.CloudWind.Wind_ProjectArea> listKing = new List<DataFactory.KingBase.CloudWind.Wind_ProjectArea>();
            using (var uow = _windKingBaseUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.KingBase.CloudWind.Wind_ProjectArea>();

                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.CloudWind.Wind_ProjectArea()
                    {
                        ProjectID = a.ProjectID,
                        AreaLon = a.AreaLon,
                        AreaLat = a.AreaLat,
                        CreateTime = a.CreateTime == null ? null : DateTime.SpecifyKind(a.CreateTime.Value, DateTimeKind.Utc),
                        IsDelete = a.IsDelete
                    });
                });

                if (listKing.Count > 0)
                {
                    repo.AddList(listKing);
                    repo.Save();
                }
            }
        }

        public void CopyWind_ProjectFan()
        {
            List<DataFactory.SqlServer.CloudWind.Wind_ProjectFan> listSql = new List<DataFactory.SqlServer.CloudWind.Wind_ProjectFan>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Wind_ProjectFan>();
                listSql = repo.FindAll().ToList();
            }

            List<DataFactory.KingBase.CloudWind.Wind_ProjectFan> listKing = new List<DataFactory.KingBase.CloudWind.Wind_ProjectFan>();
            using (var uow = _windKingBaseUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.KingBase.CloudWind.Wind_ProjectFan>();

                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.CloudWind.Wind_ProjectFan()
                    {
                        ProjectID = a.ProjectID,
                        FanName = a.FanName,
                        Status = a.Status,
                        Lon = a.Lon,
                        Lat = a.Lat,
                        CreateTime = a.CreateTime == null ? null : DateTime.SpecifyKind(a.CreateTime.Value, DateTimeKind.Utc),
                        IsDelete = a.IsDelete
                    });
                });

                if (listKing.Count > 0)
                {
                    repo.AddList(listKing);
                    repo.Save();
                }
            }
        }

        public void CopyWind_Project_Copyer()
        {
            List<DataFactory.SqlServer.CloudWind.Wind_Project_Copyer> listSql = new List<DataFactory.SqlServer.CloudWind.Wind_Project_Copyer>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Wind_Project_Copyer>();
                listSql = repo.FindAll().ToList();
            }

            List<DataFactory.KingBase.CloudWind.Wind_Project_Copyer> listKing = new List<DataFactory.KingBase.CloudWind.Wind_Project_Copyer>();
            using (var uow = _windKingBaseUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.KingBase.CloudWind.Wind_Project_Copyer>();

                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.CloudWind.Wind_Project_Copyer()
                    {
                        ProjectID = a.ProjectID,
                        UserName = a.UserName,
                        UserCode = a.UserCode,
                        UserDepart = a.UserDepart,
                        UserPhone = a.UserPhone,
                        UserJobName = a.UserJobName,
                        CreateTime = a.CreateTime == null ? null : DateTime.SpecifyKind(a.CreateTime.Value, DateTimeKind.Utc),
                        IsDelete = a.IsDelete
                    });
                });

                if (listKing.Count > 0)
                {
                    repo.AddList(listKing);
                    repo.Save();
                }
            }
        }

        public void CopyWind_Task()
        {
            List<DataFactory.SqlServer.CloudWind.Wind_Task> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Wind_Task>();
                listSql = repo.FindAll().ToList();
            }
            if (listSql.Count == 0) return;

            var kingConn = _windKingBaseUowFactory.GetConnection();
            var wasClosed = kingConn.State == ConnectionState.Closed;
            if (wasClosed) kingConn.Open();
            try
            {
                int insertBatch = 500;
                for (int i = 0; i < listSql.Count; i += insertBatch)
                {
                    var sb = new StringBuilder();
                    sb.Append("INSERT INTO \"Wind_Task\" (\"ID\",\"ProjectID\",\"TaskCode\",\"TaskName\",\"FlowStatus\",\"SoftwareID\",\"Applyer\",\"ApplyerCode\",\"ApplyerDepart\",\"ApplyerPhone\",\"ApplyerJobName\",\"DeliverTime\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var pid = a.ProjectID?.ToString() ?? "NULL";
                        var tc = a.TaskCode == null ? "NULL" : $"'{a.TaskCode.Replace("'", "''")}'";
                        var tn = a.TaskName == null ? "NULL" : $"'{a.TaskName.Replace("'", "''")}'";
                        var fs = a.FlowStatus?.ToString() ?? "NULL";
                        var sid = a.SoftwareID?.ToString() ?? "NULL";
                        var ap = a.Applyer == null ? "NULL" : $"'{a.Applyer.Replace("'", "''")}'";
                        var ac = a.ApplyerCode == null ? "NULL" : $"'{a.ApplyerCode.Replace("'", "''")}'";
                        var ad = a.ApplyerDepart == null ? "NULL" : $"'{a.ApplyerDepart.Replace("'", "''")}'";
                        var aph = a.ApplyerPhone == null ? "NULL" : $"'{a.ApplyerPhone.Replace("'", "''")}'";
                        var aj = a.ApplyerJobName == null ? "NULL" : $"'{a.ApplyerJobName.Replace("'", "''")}'";
                        var dt = a.DeliverTime == null ? "NULL" : $"'{a.DeliverTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete ? "true" : "false";
                        sb.Append($"({a.ID},{pid},{tc},{tn},{fs},{sid},{ap},{ac},{ad},{aph},{aj},{dt},{ct},{isd})");
                    }
                    using var cmd = kingConn.CreateCommand();
                    cmd.CommandText = sb.ToString();
                    using var tx = kingConn.BeginTransaction();
                    cmd.Transaction = tx;
                    cmd.ExecuteNonQuery();
                    tx.Commit();
                }
            }
            finally { if (wasClosed) kingConn.Close(); }
        }

        public void CopyWind_TaskFileDeliver()
        {
            List<DataFactory.SqlServer.CloudWind.Wind_TaskFileDeliver> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Wind_TaskFileDeliver>();
                listSql = repo.FindAll().ToList();
            }
            if (listSql.Count == 0) return;

            var kingConn = _windKingBaseUowFactory.GetConnection();
            var wasClosed = kingConn.State == ConnectionState.Closed;
            if (wasClosed) kingConn.Open();
            try
            {
                int insertBatch = 500;
                for (int i = 0; i < listSql.Count; i += insertBatch)
                {
                    var sb = new StringBuilder();
                    sb.Append("INSERT INTO \"Wind_TaskFileDeliver\" (\"ID\",\"TaskID\",\"DeliverName\",\"DeliverCode\",\"DeliverDepart\",\"DeliverPhone\",\"DeliverJobName\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var tid = a.TaskID?.ToString() ?? "NULL";
                        var dn = a.DeliverName == null ? "NULL" : $"'{a.DeliverName.Replace("'", "''")}'";
                        var dc = a.DeliverCode == null ? "NULL" : $"'{a.DeliverCode.Replace("'", "''")}'";
                        var dd = a.DeliverDepart == null ? "NULL" : $"'{a.DeliverDepart.Replace("'", "''")}'";
                        var dp = a.DeliverPhone == null ? "NULL" : $"'{a.DeliverPhone.Replace("'", "''")}'";
                        var dj = a.DeliverJobName == null ? "NULL" : $"'{a.DeliverJobName.Replace("'", "''")}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete ? "true" : "false";
                        sb.Append($"({a.ID},{tid},{dn},{dc},{dd},{dp},{dj},{ct},{isd})");
                    }
                    using var cmd = kingConn.CreateCommand();
                    cmd.CommandText = sb.ToString();
                    using var tx = kingConn.BeginTransaction();
                    cmd.Transaction = tx;
                    cmd.ExecuteNonQuery();
                    tx.Commit();
                }
            }
            finally { if (wasClosed) kingConn.Close(); }
        }

        public void CopyFlow_NodeManageUser()
        {
            List<DataFactory.SqlServer.CloudWind.Flow_NodeManageUser> listSql = new List<DataFactory.SqlServer.CloudWind.Flow_NodeManageUser>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Flow_NodeManageUser>();
                listSql = repo.FindAll().ToList();
            }

            List<DataFactory.KingBase.CloudWind.Flow_NodeManageUser> listKing = new List<DataFactory.KingBase.CloudWind.Flow_NodeManageUser>();
            using (var uow = _windKingBaseUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.KingBase.CloudWind.Flow_NodeManageUser>();

                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.CloudWind.Flow_NodeManageUser()
                    {
                        NodeID = a.NodeID,
                        ManageName = a.ManageName,
                        ManageUserCode = a.ManageUserCode,
                        ManageDepart = a.ManageDepart,
                        ManagePhone = a.ManagePhone,
                        ManageJobName = a.ManageJobName,
                        CreateTime = a.CreateTime == null ? null : DateTime.SpecifyKind(a.CreateTime.Value, DateTimeKind.Unspecified),
                        IsDelete = a.IsDelete
                    });
                });

                if (listKing.Count > 0)
                {
                    repo.AddList(listKing);
                    repo.Save();
                }
            }
        }

        public void CopyFlow_Task_CommentFile()
        {
            List<DataFactory.SqlServer.CloudWind.Flow_Task_CommentFile> listSql = new List<DataFactory.SqlServer.CloudWind.Flow_Task_CommentFile>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Flow_Task_CommentFile>();
                listSql = repo.FindAll().ToList();
            }

            List<DataFactory.KingBase.CloudWind.Flow_Task_CommentFile> listKing = new List<DataFactory.KingBase.CloudWind.Flow_Task_CommentFile>();
            using (var uow = _windKingBaseUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.KingBase.CloudWind.Flow_Task_CommentFile>();

                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.CloudWind.Flow_Task_CommentFile()
                    {
                        id = a.id,
                        TaskID = a.TaskID,
                        FlowID = a.FlowID,
                        UserName = a.UserName,
                        FileName = a.FileName,
                        FilePath = a.FilePath,
                        CreateTime = a.CreateTime == null ? null : DateTime.SpecifyKind(a.CreateTime.Value, DateTimeKind.Unspecified),
                        IsDelete = a.IsDelete
                    });
                });

                if (listKing.Count > 0)
                {
                    repo.AddList(listKing);
                    repo.Save();
                }
            }
        }

        public void CopyFlow_Task_DKFile()
        {
            List<DataFactory.SqlServer.CloudWind.Flow_Task_DKFile> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Flow_Task_DKFile>();
                listSql = repo.FindAll().ToList();
            }
            if (listSql.Count == 0) return;

            var kingConn = _windKingBaseUowFactory.GetConnection();
            var wasClosed = kingConn.State == ConnectionState.Closed;
            if (wasClosed) kingConn.Open();
            try
            {
                int insertBatch = 500;
                for (int i = 0; i < listSql.Count; i += insertBatch)
                {
                    var sb = new StringBuilder();
                    sb.Append("INSERT INTO \"Flow_Task_DKFile\" (\"id\",\"TaskID\",\"FileName\",\"FilePath\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var tid = a.TaskID?.ToString() ?? "NULL";
                        var fn = a.FileName == null ? "NULL" : $"'{a.FileName.Replace("'", "''")}'";
                        var fp = a.FilePath == null ? "NULL" : $"'{a.FilePath.Replace("'", "''")}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete == null ? "false" : (a.IsDelete.Value ? "true" : "false");
                        sb.Append($"({a.id},{tid},{fn},{fp},{ct},{isd})");
                    }
                    using var cmd = kingConn.CreateCommand();
                    cmd.CommandText = sb.ToString();
                    using var tx = kingConn.BeginTransaction();
                    cmd.Transaction = tx;
                    cmd.ExecuteNonQuery();
                    tx.Commit();
                }
            }
            finally { if (wasClosed) kingConn.Close(); }
        }

        public void CopyFlow_Task_ShipFile()
        {
            List<DataFactory.SqlServer.CloudWind.Flow_Task_ShipFile> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Flow_Task_ShipFile>();
                listSql = repo.FindAll().ToList();
            }
            if (listSql.Count == 0) return;

            var kingConn = _windKingBaseUowFactory.GetConnection();
            var wasClosed = kingConn.State == ConnectionState.Closed;
            if (wasClosed) kingConn.Open();
            try
            {
                int insertBatch = 500;
                for (int i = 0; i < listSql.Count; i += insertBatch)
                {
                    var sb = new StringBuilder();
                    sb.Append("INSERT INTO \"Flow_Task_ShipFile\" (\"id\",\"TaskID\",\"FileName\",\"FilePath\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var tid = a.TaskID?.ToString() ?? "NULL";
                        var fn = a.FileName == null ? "NULL" : $"'{a.FileName.Replace("'", "''")}'";
                        var fp = a.FilePath == null ? "NULL" : $"'{a.FilePath.Replace("'", "''")}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete == null ? "false" : (a.IsDelete.Value ? "true" : "false");
                        sb.Append($"({a.id},{tid},{fn},{fp},{ct},{isd})");
                    }
                    using var cmd = kingConn.CreateCommand();
                    cmd.CommandText = sb.ToString();
                    using var tx = kingConn.BeginTransaction();
                    cmd.Transaction = tx;
                    cmd.ExecuteNonQuery();
                    tx.Commit();
                }
            }
            finally { if (wasClosed) kingConn.Close(); }
        }

        public void CopyLibrary_Pile()
        {
            List<DataFactory.SqlServer.CloudWind.Library_Pile> listSql = new List<DataFactory.SqlServer.CloudWind.Library_Pile>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Library_Pile>();
                listSql = repo.FindAll().ToList();
            }

            List<DataFactory.KingBase.CloudWind.Library_Pile> listKing = new List<DataFactory.KingBase.CloudWind.Library_Pile>();
            using (var uow = _windKingBaseUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.KingBase.CloudWind.Library_Pile>();

                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.CloudWind.Library_Pile()
                    {
                        id = a.id,
                        TaskID = a.TaskID,
                        Type = a.Type,
                        FileName = a.FileName,
                        FilePath = a.FilePath,
                        CreateTime = a.CreateTime == null ? null : DateTime.SpecifyKind(a.CreateTime.Value, DateTimeKind.Unspecified),
                        IsDelete = a.IsDelete
                    });
                });

                if (listKing.Count > 0)
                {
                    repo.AddList(listKing);
                    repo.Save();
                }
            }
        }

        public void CopyLibrary_Ship()
        {
            List<DataFactory.SqlServer.CloudWind.Library_Ship> listSql = new List<DataFactory.SqlServer.CloudWind.Library_Ship>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Library_Ship>();
                listSql = repo.FindAll().ToList();
            }

            if (listSql.Count == 0) return;

            var kingConn = _windKingBaseUowFactory.GetConnection();
            var wasClosed = kingConn.State == ConnectionState.Closed;
            if (wasClosed) kingConn.Open();

            try
            {
                int insertBatch = 500;
                for (int i = 0; i < listSql.Count; i += insertBatch)
                {
                    var sb = new StringBuilder();
                    sb.Append("INSERT INTO \"Library_Ship\" (\"ID\",\"TaskID\",\"ShipName\",\"FileName\",\"FilePath\",\"IsConfirm\",\"CreateTime\",\"IsDelete\") VALUES ");

                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var tid = a.TaskID?.ToString() ?? "NULL";
                        var sn = a.ShipName == null ? "NULL" : $"'{a.ShipName.Replace("'", "''")}'";
                        var fn = a.FileName == null ? "NULL" : $"'{a.FileName.Replace("'", "''")}'";
                        var fp = a.FilePath == null ? "NULL" : $"'{a.FilePath.Replace("'", "''")}'";
                        var ic = a.IsConfirm == null ? "NULL" : (a.IsConfirm.Value ? "true" : "false");
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete == null ? "false" : (a.IsDelete.Value ? "true" : "false");
                        sb.Append($"({a.ID},{tid},{sn},{fn},{fp},{ic},{ct},{isd})");
                    }

                    using var cmd = kingConn.CreateCommand();
                    cmd.CommandText = sb.ToString();
                    using var tx = kingConn.BeginTransaction();
                    cmd.Transaction = tx;
                    cmd.ExecuteNonQuery();
                    tx.Commit();
                }
            }
            finally
            {
                if (wasClosed) kingConn.Close();
            }
        }

        public void CopyLibrary_Ship_KZY()
        {
            List<DataFactory.SqlServer.CloudWind.Library_Ship_KZY> listSql = new List<DataFactory.SqlServer.CloudWind.Library_Ship_KZY>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Library_Ship_KZY>();
                listSql = repo.FindAll().ToList();
            }

            List<DataFactory.KingBase.CloudWind.Library_Ship_KZY> listKing = new List<DataFactory.KingBase.CloudWind.Library_Ship_KZY>();
            using (var uow = _windKingBaseUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.KingBase.CloudWind.Library_Ship_KZY>();

                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.CloudWind.Library_Ship_KZY()
                    {
                        ID = a.ID,
                        ShipName = a.ShipName,
                        CreateTime = a.CreateTime == null ? null : DateTime.SpecifyKind(a.CreateTime.Value, DateTimeKind.Unspecified),
                        IsDelete = a.IsDelete
                    });
                });

                if (listKing.Count > 0)
                {
                    repo.AddList(listKing);
                    repo.Save();
                }
            }
        }

        public void CopyManage_CompanyRole()
        {
            List<DataFactory.SqlServer.CloudWind.Manage_CompanyRole> listSql = new List<DataFactory.SqlServer.CloudWind.Manage_CompanyRole>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Manage_CompanyRole>();
                listSql = repo.FindAll().ToList();
            }

            List<DataFactory.KingBase.CloudWind.Manage_CompanyRole> listKing = new List<DataFactory.KingBase.CloudWind.Manage_CompanyRole>();
            using (var uow = _windKingBaseUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.KingBase.CloudWind.Manage_CompanyRole>();

                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.CloudWind.Manage_CompanyRole()
                    {
                        ID = a.ID,
                        CompanyID = a.CompanyID,
                        RoleID = a.RoleID,
                        UserName = a.UserName,
                        UserCode = a.UserCode,
                        UserDepartName = a.UserDepartName,
                        UserPhone = a.UserPhone,
                        UserJobName = a.UserJobName,
                        CreateTime = a.CreateTime == null ? null : DateTime.SpecifyKind(a.CreateTime.Value, DateTimeKind.Unspecified),
                        IsDelete = a.IsDelete
                    });
                });

                if (listKing.Count > 0)
                {
                    repo.AddList(listKing);
                    repo.Save();
                }
            }
        }

        public void CopyManage_Copyer()
        {
            List<DataFactory.SqlServer.CloudWind.Manage_Copyer> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Manage_Copyer>();
                listSql = repo.FindAll().ToList();
            }
            if (listSql.Count == 0) return;

            var kingConn = _windKingBaseUowFactory.GetConnection();
            var wasClosed = kingConn.State == ConnectionState.Closed;
            if (wasClosed) kingConn.Open();
            try
            {
                int insertBatch = 500;
                for (int i = 0; i < listSql.Count; i += insertBatch)
                {
                    var sb = new StringBuilder();
                    sb.Append("INSERT INTO \"Manage_Copyer\" (\"ID\",\"SoftwareID\",\"UserName\",\"UserCode\",\"UserDepart\",\"UserPhone\",\"UserJobName\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var sid = a.SoftwareID?.ToString() ?? "NULL";
                        var un = a.UserName == null ? "NULL" : $"'{a.UserName.Replace("'", "''")}'";
                        var uc = a.UserCode == null ? "NULL" : $"'{a.UserCode.Replace("'", "''")}'";
                        var ud = a.UserDepart == null ? "NULL" : $"'{a.UserDepart.Replace("'", "''")}'";
                        var up = a.UserPhone == null ? "NULL" : $"'{a.UserPhone.Replace("'", "''")}'";
                        var uj = a.UserJobName == null ? "NULL" : $"'{a.UserJobName.Replace("'", "''")}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete ? "true" : "false";
                        sb.Append($"({a.ID},{sid},{un},{uc},{ud},{up},{uj},{ct},{isd})");
                    }
                    using var cmd = kingConn.CreateCommand();
                    cmd.CommandText = sb.ToString();
                    using var tx = kingConn.BeginTransaction();
                    cmd.Transaction = tx;
                    cmd.ExecuteNonQuery();
                    tx.Commit();
                }
            }
            finally { if (wasClosed) kingConn.Close(); }
        }

        public void CopyManage_Role()
        {
            List<DataFactory.SqlServer.CloudWind.Manage_Role> listSql = new List<DataFactory.SqlServer.CloudWind.Manage_Role>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Manage_Role>();
                listSql = repo.FindAll().ToList();
            }

            List<DataFactory.KingBase.CloudWind.Manage_Role> listKing = new List<DataFactory.KingBase.CloudWind.Manage_Role>();
            using (var uow = _windKingBaseUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.KingBase.CloudWind.Manage_Role>();

                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.CloudWind.Manage_Role()
                    {
                        id = a.id,
                        RoleName = a.RoleName,
                        CreateTime = a.CreateTime == null ? null : DateTime.SpecifyKind(a.CreateTime.Value, DateTimeKind.Unspecified),
                        IsDelete = a.IsDelete
                    });
                });

                if (listKing.Count > 0)
                {
                    repo.AddList(listKing);
                    repo.Save();
                }
            }
        }

        public void CopyManage_Software()
        {
            List<DataFactory.SqlServer.CloudWind.Manage_Software> listSql = new List<DataFactory.SqlServer.CloudWind.Manage_Software>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Manage_Software>();
                listSql = repo.FindAll().ToList();
            }

            List<DataFactory.KingBase.CloudWind.Manage_Software> listKing = new List<DataFactory.KingBase.CloudWind.Manage_Software>();
            using (var uow = _windKingBaseUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.KingBase.CloudWind.Manage_Software>();

                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.CloudWind.Manage_Software()
                    {
                        ID = a.ID,
                        SoftwareName = a.SoftwareName,
                        Comment = a.Comment,
                        DepartID = a.DepartID,
                        FlowType = a.FlowType,
                        SoftwareUrl = a.SoftwareUrl,
                        CreateTime = a.CreateTime == null ? null : DateTime.SpecifyKind(a.CreateTime.Value, DateTimeKind.Unspecified),
                        IsDelete = a.IsDelete
                    });
                });

                if (listKing.Count > 0)
                {
                    repo.AddList(listKing);
                    repo.Save();
                }
            }
        }

        public void CopyWebGIS_Ship()
        {
            List<DataFactory.SqlServer.CloudWind.WebGIS_Ship> listSql = new List<DataFactory.SqlServer.CloudWind.WebGIS_Ship>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.WebGIS_Ship>();
                listSql = repo.FindAll().ToList();
            }

            List<DataFactory.KingBase.CloudWind.WebGIS_Ship> listKing = new List<DataFactory.KingBase.CloudWind.WebGIS_Ship>();
            using (var uow = _windKingBaseUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.KingBase.CloudWind.WebGIS_Ship>();

                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.CloudWind.WebGIS_Ship()
                    {
                        id = a.id,
                        ShipName = a.ShipName,
                        mmsi = a.mmsi,
                        ShipType = a.ShipType,
                        CreateTime = a.CreateTime == null ? null : DateTime.SpecifyKind(a.CreateTime.Value, DateTimeKind.Unspecified),
                        IsDelete = a.IsDelete
                    });
                });

                if (listKing.Count > 0)
                {
                    repo.AddList(listKing);
                    repo.Save();
                }
            }
        }

        public void CopyWind_TaskFile()
        {
            List<DataFactory.SqlServer.CloudWind.Wind_TaskFile> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Wind_TaskFile>();
                listSql = repo.FindAll().ToList();
            }
            if (listSql.Count == 0) return;

            var kingConn = _windKingBaseUowFactory.GetConnection();
            var wasClosed = kingConn.State == ConnectionState.Closed;
            if (wasClosed) kingConn.Open();
            try
            {
                int insertBatch = 500;
                for (int i = 0; i < listSql.Count; i += insertBatch)
                {
                    var sb = new StringBuilder();
                    sb.Append("INSERT INTO \"Wind_TaskFile\" (\"id\",\"TaskID\",\"TypeID\",\"FileName\",\"FilePath\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var tid = a.TaskID?.ToString() ?? "NULL";
                        var tyid = a.TypeID?.ToString() ?? "NULL";
                        var fn = a.FileName == null ? "NULL" : $"'{a.FileName.Replace("'", "''")}'";
                        var fp = a.FilePath == null ? "NULL" : $"'{a.FilePath.Replace("'", "''")}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete == null ? "false" : (a.IsDelete.Value ? "true" : "false");
                        sb.Append($"({a.id},{tid},{tyid},{fn},{fp},{ct},{isd})");
                    }
                    using var cmd = kingConn.CreateCommand();
                    cmd.CommandText = sb.ToString();
                    using var tx = kingConn.BeginTransaction();
                    cmd.Transaction = tx;
                    cmd.ExecuteNonQuery();
                    tx.Commit();
                }
            }
            finally { if (wasClosed) kingConn.Close(); }
        }

        public void CopyWind_TaskInfoImg_ZJCZ()
        {
            List<DataFactory.SqlServer.CloudWind.Wind_TaskInfoImg_ZJCZ> listSql = new List<DataFactory.SqlServer.CloudWind.Wind_TaskInfoImg_ZJCZ>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Wind_TaskInfoImg_ZJCZ>();
                listSql = repo.FindAll().ToList();
            }

            List<DataFactory.KingBase.CloudWind.Wind_TaskInfoImg_ZJCZ> listKing = new List<DataFactory.KingBase.CloudWind.Wind_TaskInfoImg_ZJCZ>();
            using (var uow = _windKingBaseUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.KingBase.CloudWind.Wind_TaskInfoImg_ZJCZ>();

                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.CloudWind.Wind_TaskInfoImg_ZJCZ()
                    {
                        id = a.id,
                        TaskID = a.TaskID,
                        FileName = a.FileName,
                        FilePath = a.FilePath,
                        CreateTime = a.CreateTime == null ? null : DateTime.SpecifyKind(a.CreateTime.Value, DateTimeKind.Unspecified),
                        IsDelete = a.IsDelete
                    });
                });

                if (listKing.Count > 0)
                {
                    repo.AddList(listKing);
                    repo.Save();
                }
            }
        }

        public void CopyWind_TaskReport()
        {
            List<DataFactory.SqlServer.CloudWind.Wind_TaskReport> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Wind_TaskReport>();
                listSql = repo.FindAll().ToList();
            }
            if (listSql.Count == 0) return;

            var kingConn = _windKingBaseUowFactory.GetConnection();
            var wasClosed = kingConn.State == ConnectionState.Closed;
            if (wasClosed) kingConn.Open();
            try
            {
                int insertBatch = 500;
                for (int i = 0; i < listSql.Count; i += insertBatch)
                {
                    var sb = new StringBuilder();
                    sb.Append("INSERT INTO \"Wind_TaskReport\" (\"id\",\"TaskID\",\"Type\",\"ReportName\",\"ReportPath\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var tid = a.TaskID?.ToString() ?? "NULL";
                        var ty = a.Type?.ToString() ?? "NULL";
                        var rn = a.ReportName == null ? "NULL" : $"'{a.ReportName.Replace("'", "''")}'";
                        var rp = a.ReportPath == null ? "NULL" : $"'{a.ReportPath.Replace("'", "''")}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete == null ? "false" : (a.IsDelete.Value ? "true" : "false");
                        sb.Append($"({a.id},{tid},{ty},{rn},{rp},{ct},{isd})");
                    }
                    using var cmd = kingConn.CreateCommand();
                    cmd.CommandText = sb.ToString();
                    using var tx = kingConn.BeginTransaction();
                    cmd.Transaction = tx;
                    cmd.ExecuteNonQuery();
                    tx.Commit();
                }
            }
            finally { if (wasClosed) kingConn.Close(); }
        }

        public void CopyWind_TaskInfo_KZY()
        {
            List<DataFactory.SqlServer.CloudWind.Wind_TaskInfo_KZY> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Wind_TaskInfo_KZY>();
                listSql = repo.FindAll().ToList();
            }
            if (listSql.Count == 0) return;

            var kingConn = _windKingBaseUowFactory.GetConnection();
            var wasClosed = kingConn.State == ConnectionState.Closed;
            if (wasClosed) kingConn.Open();
            try
            {
                int insertBatch = 500;
                for (int i = 0; i < listSql.Count; i += insertBatch)
                {
                    var sb = new StringBuilder();
                    sb.Append("INSERT INTO \"Wind_TaskInfo_KZY\" (\"id\",\"TaskID\",\"ShipIDs\",\"TechType\",\"ForecastStartTime\",\"ForecastEndTime\",\"YFYNo\",\"Balance\",\"Lon\",\"Lat\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var tid = a.TaskID?.ToString() ?? "NULL";
                        var sids = a.ShipIDs == null ? "NULL" : $"'{a.ShipIDs.Replace("'", "''")}'";
                        var tt = a.TechType?.ToString() ?? "NULL";
                        var fst = a.ForecastStartTime == null ? "NULL" : $"'{a.ForecastStartTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var fet = a.ForecastEndTime == null ? "NULL" : $"'{a.ForecastEndTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var yfy = a.YFYNo == null ? "NULL" : $"'{a.YFYNo.Replace("'", "''")}'";
                        var bal = a.Balance == null ? "NULL" : $"'{a.Balance.Replace("'", "''")}'";
                        var lon = a.Lon == null ? "NULL" : $"'{a.Lon.Replace("'", "''")}'";
                        var lat = a.Lat == null ? "NULL" : $"'{a.Lat.Replace("'", "''")}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete == null ? "false" : (a.IsDelete.Value ? "true" : "false");
                        sb.Append($"({a.id},{tid},{sids},{tt},{fst},{fet},{yfy},{bal},{lon},{lat},{ct},{isd})");
                    }
                    using var cmd = kingConn.CreateCommand();
                    cmd.CommandText = sb.ToString();
                    using var tx = kingConn.BeginTransaction();
                    cmd.Transaction = tx;
                    cmd.ExecuteNonQuery();
                    tx.Commit();
                }
            }
            finally { if (wasClosed) kingConn.Close(); }
        }

        public void CopyManage_Admin()
        {
            List<DataFactory.SqlServer.CloudWind.Manage_Admin> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Manage_Admin>();
                listSql = repo.FindAll().ToList();
            }
            if (listSql.Count == 0) return;

            var kingConn = _windKingBaseUowFactory.GetConnection();
            var wasClosed = kingConn.State == ConnectionState.Closed;
            if (wasClosed) kingConn.Open();
            try
            {
                int insertBatch = 500;
                for (int i = 0; i < listSql.Count; i += insertBatch)
                {
                    var sb = new StringBuilder();
                    sb.Append("INSERT INTO \"Manage_Admin\" (\"ID\",\"UserName\",\"UserCode\",\"UserDepartName\",\"UserPhone\",\"UserJobName\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var un = a.UserName == null ? "NULL" : $"'{a.UserName.Replace("'", "''")}'";
                        var uc = a.UserCode == null ? "NULL" : $"'{a.UserCode.Replace("'", "''")}'";
                        var ud = a.UserDepartName == null ? "NULL" : $"'{a.UserDepartName.Replace("'", "''")}'";
                        var up = a.UserPhone == null ? "NULL" : $"'{a.UserPhone.Replace("'", "''")}'";
                        var uj = a.UserJobName == null ? "NULL" : $"'{a.UserJobName.Replace("'", "''")}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete ? "true" : "false";
                        sb.Append($"({a.ID},{un},{uc},{ud},{up},{uj},{ct},{isd})");
                    }
                    using var cmd = kingConn.CreateCommand();
                    cmd.CommandText = sb.ToString();
                    using var tx = kingConn.BeginTransaction();
                    cmd.Transaction = tx;
                    cmd.ExecuteNonQuery();
                    tx.Commit();
                }
            }
            finally { if (wasClosed) kingConn.Close(); }
        }

        public void CopyWind_ProjectContacter()
        {
            List<DataFactory.SqlServer.CloudWind.Wind_ProjectContacter> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Wind_ProjectContacter>();
                listSql = repo.FindAll().ToList();
            }
            if (listSql.Count == 0) return;

            var kingConn = _windKingBaseUowFactory.GetConnection();
            var wasClosed = kingConn.State == ConnectionState.Closed;
            if (wasClosed) kingConn.Open();
            try
            {
                int insertBatch = 500;
                for (int i = 0; i < listSql.Count; i += insertBatch)
                {
                    var sb = new StringBuilder();
                    sb.Append("INSERT INTO \"Wind_ProjectContacter\" (\"ID\",\"ProjectID\",\"Applyer\",\"ApplyerCode\",\"ApplyerDepart\",\"ApplyerPhone\",\"ApplyerJobName\",\"Director\",\"DirectorCode\",\"DirectorDepart\",\"DirectorPhone\",\"DirectorJobName\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var pid = a.ProjectID.ToString();
                        var ap = a.Applyer == null ? "NULL" : $"'{a.Applyer.Replace("'", "''")}'";
                        var ac = a.ApplyerCode == null ? "NULL" : $"'{a.ApplyerCode.Replace("'", "''")}'";
                        var ad = a.ApplyerDepart == null ? "NULL" : $"'{a.ApplyerDepart.Replace("'", "''")}'";
                        var aph = a.ApplyerPhone == null ? "NULL" : $"'{a.ApplyerPhone.Replace("'", "''")}'";
                        var aj = a.ApplyerJobName == null ? "NULL" : $"'{a.ApplyerJobName.Replace("'", "''")}'";
                        var di = a.Director == null ? "NULL" : $"'{a.Director.Replace("'", "''")}'";
                        var dc = a.DirectorCode == null ? "NULL" : $"'{a.DirectorCode.Replace("'", "''")}'";
                        var dd = a.DirectorDepart == null ? "NULL" : $"'{a.DirectorDepart.Replace("'", "''")}'";
                        var dph = a.DirectorPhone == null ? "NULL" : $"'{a.DirectorPhone.Replace("'", "''")}'";
                        var dj = a.DirectorJobName == null ? "NULL" : $"'{a.DirectorJobName.Replace("'", "''")}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete ? "true" : "false";
                        sb.Append($"({a.ID},{pid},{ap},{ac},{ad},{aph},{aj},{di},{dc},{dd},{dph},{dj},{ct},{isd})");
                    }
                    using var cmd = kingConn.CreateCommand();
                    cmd.CommandText = sb.ToString();
                    using var tx = kingConn.BeginTransaction();
                    cmd.Transaction = tx;
                    cmd.ExecuteNonQuery();
                    tx.Commit();
                }
            }
            finally { if (wasClosed) kingConn.Close(); }
        }

        public void CopyWind_Task_JJTResult()
        {
            List<DataFactory.SqlServer.CloudWind.Wind_Task_JJTResult> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Wind_Task_JJTResult>();
                listSql = repo.FindAll().ToList();
            }
            if (listSql.Count == 0) return;

            var kingConn = _windKingBaseUowFactory.GetConnection();
            var wasClosed = kingConn.State == ConnectionState.Closed;
            if (wasClosed) kingConn.Open();
            try
            {
                int insertBatch = 500;
                for (int i = 0; i < listSql.Count; i += insertBatch)
                {
                    var sb = new StringBuilder();
                    sb.Append("INSERT INTO \"Wind_Task_JJTResult\" (\"id\",\"Type\",\"UserCode\",\"DirectUrl\",\"Result\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var ty = a.Type == null ? "NULL" : $"'{a.Type.Replace("'", "''")}'";
                        var uc = a.UserCode == null ? "NULL" : $"'{a.UserCode.Replace("'", "''")}'";
                        var du = a.DirectUrl == null ? "NULL" : $"'{a.DirectUrl.Replace("'", "''")}'";
                        var res = a.Result == null ? "NULL" : (a.Result.Value ? "true" : "false");
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete == null ? "false" : (a.IsDelete.Value ? "true" : "false");
                        sb.Append($"({a.id},{ty},{uc},{du},{res},{ct},{isd})");
                    }
                    using var cmd = kingConn.CreateCommand();
                    cmd.CommandText = sb.ToString();
                    using var tx = kingConn.BeginTransaction();
                    cmd.Transaction = tx;
                    cmd.ExecuteNonQuery();
                    tx.Commit();
                }
            }
            finally { if (wasClosed) kingConn.Close(); }
        }

        public void CopyFlow_TaskApply()
        {
            List<DataFactory.SqlServer.CloudWind.Flow_TaskApply> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Flow_TaskApply>();
                listSql = repo.FindAll().ToList();
            }
            if (listSql.Count == 0) return;

            var kingConn = _windKingBaseUowFactory.GetConnection();
            var wasClosed = kingConn.State == ConnectionState.Closed;
            if (wasClosed) kingConn.Open();
            try
            {
                int insertBatch = 500;
                for (int i = 0; i < listSql.Count; i += insertBatch)
                {
                    var sb = new StringBuilder();
                    sb.Append("INSERT INTO \"Flow_TaskApply\" (\"id\",\"TaskID\",\"NodeID\",\"FlowOrder\",\"NodeUserName\",\"NodeUserCode\",\"NodeUserDepart\",\"NodeUserPhone\",\"NodeUserJobName\",\"FlowHandle\",\"ApprovalTime\",\"Comment\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var tid = a.TaskID?.ToString() ?? "NULL";
                        var nid = a.NodeID?.ToString() ?? "NULL";
                        var fo = a.FlowOrder?.ToString() ?? "NULL";
                        var un = a.NodeUserName == null ? "NULL" : $"'{a.NodeUserName.Replace("'", "''")}'";
                        var uc = a.NodeUserCode == null ? "NULL" : $"'{a.NodeUserCode.Replace("'", "''")}'";
                        var ud = a.NodeUserDepart == null ? "NULL" : $"'{a.NodeUserDepart.Replace("'", "''")}'";
                        var up = a.NodeUserPhone == null ? "NULL" : $"'{a.NodeUserPhone.Replace("'", "''")}'";
                        var uj = a.NodeUserJobName == null ? "NULL" : $"'{a.NodeUserJobName.Replace("'", "''")}'";
                        var fh = a.FlowHandle?.ToString() ?? "NULL";
                        var at = a.ApprovalTime == null ? "NULL" : $"'{a.ApprovalTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var cm = a.Comment == null ? "NULL" : $"'{a.Comment.Replace("'", "''")}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete == null ? "false" : (a.IsDelete.Value ? "true" : "false");
                        sb.Append($"({a.id},{tid},{nid},{fo},{un},{uc},{ud},{up},{uj},{fh},{at},{cm},{ct},{isd})");
                    }
                    using var cmd = kingConn.CreateCommand();
                    cmd.CommandText = sb.ToString();
                    using var tx = kingConn.BeginTransaction();
                    cmd.Transaction = tx;
                    cmd.ExecuteNonQuery();
                    tx.Commit();
                }
            }
            finally { if (wasClosed) kingConn.Close(); }
        }

        public void CopyData_PXYWeather()
        {
            List<DataFactory.SqlServer.CloudWind.Data_PXYWeather> listSql = new List<DataFactory.SqlServer.CloudWind.Data_PXYWeather>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Data_PXYWeather>();
                listSql = repo.FindAll().ToList();
            }

            List<DataFactory.KingBase.CloudWind.Data_PXYWeather> listKing = new List<DataFactory.KingBase.CloudWind.Data_PXYWeather>();
            using (var uow = _windKingBaseUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.KingBase.CloudWind.Data_PXYWeather>();

                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.CloudWind.Data_PXYWeather()
                    {
                        id = a.id,
                        TaskID = a.TaskID,
                        PointName = a.PointName,
                        LonLat = a.LonLat,
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
                        windspeed = a.windspeed,
                        windlevel = a.windlevel,
                        windwarninglevel = a.windwarninglevel,
                        windalarm = a.windalarm,
                        time = a.time == null ? null : DateTime.SpecifyKind(a.time.Value, DateTimeKind.Unspecified)
                    });
                });

                if (listKing.Count > 0)
                {
                    repo.AddList(listKing);
                    repo.Save();
                }
            }
        }

        public void CopyData_Typhoon()
        {
            int batchSize = 50000;
            int insertBatch = 5000;
            int lastId = 0;
            bool hasMore = true;

            var kingConn = _windKingBaseUowFactory.GetConnection();
            var wasClosed = kingConn.State == ConnectionState.Closed;
            if (wasClosed) kingConn.Open();

            try
            {
                while (hasMore)
                {
                    List<DataFactory.SqlServer.CloudWind.Data_Typhoon> listSql;
                    using (var uow = _windSqlUowFactory.Create())
                    {
                        var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Data_Typhoon>();
                        listSql = repo.Find(x => x.id > lastId)
                            .OrderBy(x => x.id)
                            .Take(batchSize)
                            .ToList();
                    }

                    if (listSql.Count == 0)
                    {
                        hasMore = false;
                        break;
                    }

                    for (int i = 0; i < listSql.Count; i += insertBatch)
                    {
                        var sb = new StringBuilder();
                        sb.Append("INSERT INTO \"Data_Typhoon\" (id,\"chnCode\",\"chnName\",\"currentyear\",\"xuHao\",\"time\",\"dateTime\",\"forecast\",\"fhour\",\"lat\",\"lon\",\"grade\",\"mspeed\",\"pressure\",\"kspeed\",\"direction\",\"radius7\",\"radius10\",\"CreateTime\",\"IsDelete\") VALUES ");

                        int count = Math.Min(insertBatch, listSql.Count - i);
                        for (int j = 0; j < count; j++)
                        {
                            var a = listSql[i + j];
                            if (j > 0) sb.Append(',');
                            var cc = a.chnCode == null ? "NULL" : $"'{a.chnCode.Replace("'", "''")}'";
                            var cn = a.chnName == null ? "NULL" : $"'{a.chnName.Replace("'", "''")}'";
                            var cy = a.currentyear?.ToString() ?? "NULL";
                            var xh = a.xuHao == null ? "NULL" : $"'{a.xuHao.Replace("'", "''")}'";
                            var tm = a.time == null ? "NULL" : $"'{a.time.Replace("'", "''")}'";
                            var dt = a.dateTime == null ? "NULL" : $"'{a.dateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                            var fc = a.forecast == null ? "NULL" : $"'{a.forecast.Replace("'", "''")}'";
                            var fh = a.fhour?.ToString() ?? "NULL";
                            var la = a.lat?.ToString() ?? "NULL";
                            var lo = a.lon?.ToString() ?? "NULL";
                            var gr = a.grade?.ToString() ?? "NULL";
                            var ms = a.mspeed?.ToString() ?? "NULL";
                            var pr = a.pressure?.ToString() ?? "NULL";
                            var ks = a.kspeed?.ToString() ?? "NULL";
                            var dr = a.direction == null ? "NULL" : $"'{a.direction.Replace("'", "''")}'";
                            var r7 = a.radius7?.ToString() ?? "NULL";
                            var r10 = a.radius10?.ToString() ?? "NULL";
                            var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                            var isd = a.IsDelete ? "true" : "false";
                            sb.Append($"({a.id},{cc},{cn},{cy},{xh},{tm},{dt},{fc},{fh},{la},{lo},{gr},{ms},{pr},{ks},{dr},{r7},{r10},{ct},{isd})");
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

    }
}
