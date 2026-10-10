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

        public void CopyWind_Project()
        {
            List<DataFactory.SqlServer.CloudWind.Wind_Project> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Wind_Project>();
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
                    sb.Append("INSERT INTO \"Wind_Project\" (\"ID\",\"ProjectCode\",\"ProjectCodeIndex\",\"ProjectName\",\"CompanyID\",\"Lon\",\"Lat\",\"Status\",\"FlowStatus\",\"ProjectStartTime\",\"ProjectEndTime\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var pc = a.ProjectCode == null ? "NULL" : $"'{a.ProjectCode.Replace("'", "''")}'";
                        var pci = a.ProjectCodeIndex?.ToString() ?? "NULL";
                        var pn = a.ProjectName == null ? "NULL" : $"'{a.ProjectName.Replace("'", "''")}'";
                        var cid = a.CompanyID?.ToString() ?? "NULL";
                        var lon = a.Lon == null ? "NULL" : $"'{a.Lon.Replace("'", "''")}'";
                        var lat = a.Lat == null ? "NULL" : $"'{a.Lat.Replace("'", "''")}'";
                        var st = a.Status?.ToString() ?? "NULL";
                        var fs = a.FlowStatus?.ToString() ?? "NULL";
                        var pst = a.ProjectStartTime == null ? "NULL" : $"'{a.ProjectStartTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var pet = a.ProjectEndTime == null ? "NULL" : $"'{a.ProjectEndTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete ? "true" : "false";
                        sb.Append($"({a.ID},{pc},{pci},{pn},{cid},{lon},{lat},{st},{fs},{pst},{pet},{ct},{isd})");
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

        public void CopyLibrary_Geology()
        {
            List<DataFactory.SqlServer.CloudWind.Library_Geology> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Library_Geology>();
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
                    sb.Append("INSERT INTO \"Library_Geology\" (\"ID\",\"ProjectID\",\"Type\",\"FileName\",\"FilePath\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var pid = a.ProjectID.ToString();
                        var ty = a.Type?.ToString() ?? "NULL";
                        var fn = a.FileName == null ? "NULL" : $"'{a.FileName.Replace("'", "''")}'";
                        var fp = a.FilePath == null ? "NULL" : $"'{a.FilePath.Replace("'", "''")}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete ? "true" : "false";
                        sb.Append($"({a.ID},{pid},{ty},{fn},{fp},{ct},{isd})");
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

        public void CopyLibrary_Geology_DK()
        {
            List<DataFactory.SqlServer.CloudWind.Library_Geology_DK> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Library_Geology_DK>();
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
                    sb.Append("INSERT INTO \"Library_Geology_DK\" (\"ID\",\"ProjectID\",\"FanID\",\"DKName\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var pid = a.ProjectID?.ToString() ?? "NULL";
                        var fid = a.FanID?.ToString() ?? "NULL";
                        var dn = a.DKName == null ? "NULL" : $"'{a.DKName.Replace("'", "''")}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete ? "true" : "false";
                        sb.Append($"({a.ID},{pid},{fid},{dn},{ct},{isd})");
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

        public void CopyLibrary_Geology_Data()
        {
            DataTable dt = new DataTable();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Library_Geology_Data>();
                dt = repo.QueryDataTable("select * from Library_Geology_Data");
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
                    sb.Append("INSERT INTO \"Library_Geology_Data\" (\"ID\",\"DKID\",\"xh\",\"dcbh\",\"tcbh\",\"cdbg\",\"tclx\",\"bpskjqd\",\"stmcj\",\"yxzd\",\"bgjs\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, rows.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var dr = dt.Rows[i + j];
                        if (j > 0) sb.Append(',');
                        string S(int idx) { var v = dr[idx].ToString(); return string.IsNullOrEmpty(v) ? "NULL" : $"'{v.Replace("'", "''")}'"; }
                        var id = dr[0].ToString();
                        var dkid = dr[1].ToString();
                        var ct = string.IsNullOrEmpty(dr[11].ToString()) ? "NULL" : $"'{Convert.ToDateTime(dr[11].ToString()):yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = Convert.ToBoolean(dr[12].ToString()) ? "true" : "false";
                        sb.Append($"({id},{dkid},{S(2)},{S(3)},{S(4)},{S(5)},{S(6)},{S(7)},{S(8)},{S(9)},{S(10)},{ct},{isd})");
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
            List<DataFactory.SqlServer.CloudWind.Wind_ProjectFile> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Wind_ProjectFile>();
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
                    sb.Append("INSERT INTO \"Wind_ProjectFile\" (\"ID\",\"ProjectID\",\"FileName\",\"FilePath\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var pid = a.ProjectID.ToString();
                        var fn = a.FileName == null ? "NULL" : $"'{a.FileName.Replace("'", "''")}'";
                        var fp = a.FilePath == null ? "NULL" : $"'{a.FilePath.Replace("'", "''")}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete ? "true" : "false";
                        sb.Append($"({a.ID},{pid},{fn},{fp},{ct},{isd})");
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

        public void CopyManage_Viewer()
        {
            List<DataFactory.SqlServer.CloudWind.Manage_Viewer> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Manage_Viewer>();
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
                    sb.Append("INSERT INTO \"Manage_Viewer\" (\"ID\",\"UserName\",\"UserCode\",\"UserDepartName\",\"UserPhone\",\"UserJobName\",\"CreateTime\",\"IsDelete\") VALUES ");
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

        public void CopyLibrary_Pile_Data()
        {
            DataTable dt = new DataTable();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Library_Pile_Data>();
                dt = repo.QueryDataTable("select * from Library_Pile_Data");
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
                    sb.Append("INSERT INTO \"Library_Pile_Data\" (\"id\",\"PileID\",\"PileName\",\"mdcd\",\"zc\",\"fzzwj\",\"bh\",\"zzl\",\"zdgcT\",\"zcm\",\"zdgcB\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, rows.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var dr = dt.Rows[i + j];
                        if (j > 0) sb.Append(',');
                        string S(int idx) { var v = dr[idx].ToString(); return string.IsNullOrEmpty(v) ? "NULL" : $"'{v.Replace("'", "''")}'"; }
                        var id = dr[0].ToString();
                        var pileid = dr[1].ToString();
                        var ct = string.IsNullOrEmpty(dr[11].ToString()) ? "NULL" : $"'{Convert.ToDateTime(dr[11].ToString()):yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = Convert.ToBoolean(dr[12].ToString()) ? "true" : "false";
                        sb.Append($"({id},{pileid},{S(2)},{S(3)},{S(4)},{S(5)},{S(6)},{S(7)},{S(8)},{S(9)},{S(10)},{ct},{isd})");
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
            List<DataFactory.SqlServer.CloudWind.Wind_ProjectInfo> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Wind_ProjectInfo>();
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
                    sb.Append("INSERT INTO \"Wind_ProjectInfo\" (\"ID\",\"ProjectID\",\"WaterDepth\",\"WaterDepthMin\",\"WaterDepthMax\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var pid = a.ProjectID.ToString();
                        var wd = a.WaterDepth == null ? "NULL" : $"'{a.WaterDepth.Replace("'", "''")}'";
                        var wdm = a.WaterDepthMin == null ? "NULL" : $"'{a.WaterDepthMin.Replace("'", "''")}'";
                        var wdx = a.WaterDepthMax == null ? "NULL" : $"'{a.WaterDepthMax.Replace("'", "''")}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete == null ? "false" : (a.IsDelete.Value ? "true" : "false");
                        sb.Append($"({a.ID},{pid},{wd},{wdm},{wdx},{ct},{isd})");
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
            List<DataFactory.SqlServer.CloudWind.Wind_ProjectArea> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Wind_ProjectArea>();
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
                    sb.Append("INSERT INTO \"Wind_ProjectArea\" (\"ID\",\"ProjectID\",\"AreaLon\",\"AreaLat\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var pid = a.ProjectID.ToString();
                        var alon = a.AreaLon == null ? "NULL" : $"'{a.AreaLon.Replace("'", "''")}'";
                        var alat = a.AreaLat == null ? "NULL" : $"'{a.AreaLat.Replace("'", "''")}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete ? "true" : "false";
                        sb.Append($"({a.ID},{pid},{alon},{alat},{ct},{isd})");
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

        public void CopyWind_ProjectFan()
        {
            List<DataFactory.SqlServer.CloudWind.Wind_ProjectFan> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Wind_ProjectFan>();
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
                    sb.Append("INSERT INTO \"Wind_ProjectFan\" (\"ID\",\"ProjectID\",\"FanName\",\"Status\",\"Lon\",\"Lat\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var pid = a.ProjectID.ToString();
                        var fn = a.FanName == null ? "NULL" : $"'{a.FanName.Replace("'", "''")}'";
                        var st = a.Status?.ToString() ?? "NULL";
                        var lon = a.Lon == null ? "NULL" : $"'{a.Lon.Replace("'", "''")}'";
                        var lat = a.Lat == null ? "NULL" : $"'{a.Lat.Replace("'", "''")}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete ? "true" : "false";
                        sb.Append($"({a.ID},{pid},{fn},{st},{lon},{lat},{ct},{isd})");
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

        public void CopyWind_Project_Copyer()
        {
            List<DataFactory.SqlServer.CloudWind.Wind_Project_Copyer> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Wind_Project_Copyer>();
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
                    sb.Append("INSERT INTO \"Wind_Project_Copyer\" (\"ID\",\"ProjectID\",\"UserName\",\"UserCode\",\"UserDepart\",\"UserPhone\",\"UserJobName\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var pid = a.ProjectID.ToString();
                        var un = a.UserName == null ? "NULL" : $"'{a.UserName.Replace("'", "''")}'";
                        var uc = a.UserCode == null ? "NULL" : $"'{a.UserCode.Replace("'", "''")}'";
                        var ud = a.UserDepart == null ? "NULL" : $"'{a.UserDepart.Replace("'", "''")}'";
                        var up = a.UserPhone == null ? "NULL" : $"'{a.UserPhone.Replace("'", "''")}'";
                        var uj = a.UserJobName == null ? "NULL" : $"'{a.UserJobName.Replace("'", "''")}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete ? "true" : "false";
                        sb.Append($"({a.ID},{pid},{un},{uc},{ud},{up},{uj},{ct},{isd})");
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
            List<DataFactory.SqlServer.CloudWind.Flow_NodeManageUser> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Flow_NodeManageUser>();
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
                    sb.Append("INSERT INTO \"Flow_NodeManageUser\" (\"ID\",\"NodeID\",\"ManageName\",\"ManageUserCode\",\"ManageDepart\",\"ManagePhone\",\"ManageJobName\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var nid = a.NodeID.ToString();
                        var mn = a.ManageName == null ? "NULL" : $"'{a.ManageName.Replace("'", "''")}'";
                        var mc = a.ManageUserCode == null ? "NULL" : $"'{a.ManageUserCode.Replace("'", "''")}'";
                        var md = a.ManageDepart == null ? "NULL" : $"'{a.ManageDepart.Replace("'", "''")}'";
                        var mp = a.ManagePhone == null ? "NULL" : $"'{a.ManagePhone.Replace("'", "''")}'";
                        var mj = a.ManageJobName == null ? "NULL" : $"'{a.ManageJobName.Replace("'", "''")}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete ? "true" : "false";
                        sb.Append($"({a.ID},{nid},{mn},{mc},{md},{mp},{mj},{ct},{isd})");
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

        public void CopyFlow_Task_CommentFile()
        {
            List<DataFactory.SqlServer.CloudWind.Flow_Task_CommentFile> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Flow_Task_CommentFile>();
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
                    sb.Append("INSERT INTO \"Flow_Task_CommentFile\" (\"id\",\"TaskID\",\"FlowID\",\"UserName\",\"FileName\",\"FilePath\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var tid = a.TaskID?.ToString() ?? "NULL";
                        var fid = a.FlowID?.ToString() ?? "NULL";
                        var un = a.UserName == null ? "NULL" : $"'{a.UserName.Replace("'", "''")}'";
                        var fn = a.FileName == null ? "NULL" : $"'{a.FileName.Replace("'", "''")}'";
                        var fp = a.FilePath == null ? "NULL" : $"'{a.FilePath.Replace("'", "''")}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete == null ? "false" : (a.IsDelete.Value ? "true" : "false");
                        sb.Append($"({a.id},{tid},{fid},{un},{fn},{fp},{ct},{isd})");
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
            List<DataFactory.SqlServer.CloudWind.Library_Pile> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Library_Pile>();
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
                    sb.Append("INSERT INTO \"Library_Pile\" (\"id\",\"TaskID\",\"Type\",\"FileName\",\"FilePath\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var tid = a.TaskID?.ToString() ?? "NULL";
                        var ty = a.Type?.ToString() ?? "NULL";
                        var fn = a.FileName == null ? "NULL" : $"'{a.FileName.Replace("'", "''")}'";
                        var fp = a.FilePath == null ? "NULL" : $"'{a.FilePath.Replace("'", "''")}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete == null ? "false" : (a.IsDelete.Value ? "true" : "false");
                        sb.Append($"({a.id},{tid},{ty},{fn},{fp},{ct},{isd})");
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
            List<DataFactory.SqlServer.CloudWind.Library_Ship_KZY> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Library_Ship_KZY>();
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
                    sb.Append("INSERT INTO \"Library_Ship_KZY\" (\"ID\",\"ShipName\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var sn = a.ShipName == null ? "NULL" : $"'{a.ShipName.Replace("'", "''")}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete == null ? "false" : (a.IsDelete.Value ? "true" : "false");
                        sb.Append($"({a.ID},{sn},{ct},{isd})");
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

        public void CopyManage_CompanyRole()
        {
            List<DataFactory.SqlServer.CloudWind.Manage_CompanyRole> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Manage_CompanyRole>();
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
                    sb.Append("INSERT INTO \"Manage_CompanyRole\" (\"ID\",\"CompanyID\",\"RoleID\",\"UserName\",\"UserCode\",\"UserDepartName\",\"UserPhone\",\"UserJobName\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var cid = a.CompanyID?.ToString() ?? "NULL";
                        var rid = a.RoleID?.ToString() ?? "NULL";
                        var un = a.UserName == null ? "NULL" : $"'{a.UserName.Replace("'", "''")}'";
                        var uc = a.UserCode == null ? "NULL" : $"'{a.UserCode.Replace("'", "''")}'";
                        var ud = a.UserDepartName == null ? "NULL" : $"'{a.UserDepartName.Replace("'", "''")}'";
                        var up = a.UserPhone == null ? "NULL" : $"'{a.UserPhone.Replace("'", "''")}'";
                        var uj = a.UserJobName == null ? "NULL" : $"'{a.UserJobName.Replace("'", "''")}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete ? "true" : "false";
                        sb.Append($"({a.ID},{cid},{rid},{un},{uc},{ud},{up},{uj},{ct},{isd})");
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
            List<DataFactory.SqlServer.CloudWind.Manage_Role> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Manage_Role>();
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
                    sb.Append("INSERT INTO \"Manage_Role\" (\"id\",\"RoleName\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var rn = a.RoleName == null ? "NULL" : $"'{a.RoleName.Replace("'", "''")}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete == null ? "false" : (a.IsDelete.Value ? "true" : "false");
                        sb.Append($"({a.id},{rn},{ct},{isd})");
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

        public void CopyManage_Software()
        {
            List<DataFactory.SqlServer.CloudWind.Manage_Software> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Manage_Software>();
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
                    sb.Append("INSERT INTO \"Manage_Software\" (\"ID\",\"SoftwareName\",\"Comment\",\"DepartID\",\"FlowType\",\"SoftwareUrl\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var sn = a.SoftwareName == null ? "NULL" : $"'{a.SoftwareName.Replace("'", "''")}'";
                        var cm = a.Comment == null ? "NULL" : $"'{a.Comment.Replace("'", "''")}'";
                        var did = a.DepartID?.ToString() ?? "NULL";
                        var ft = a.FlowType?.ToString() ?? "NULL";
                        var su = a.SoftwareUrl == null ? "NULL" : $"'{a.SoftwareUrl.Replace("'", "''")}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete ? "true" : "false";
                        sb.Append($"({a.ID},{sn},{cm},{did},{ft},{su},{ct},{isd})");
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

        public void CopyWebGIS_Ship()
        {
            List<DataFactory.SqlServer.CloudWind.WebGIS_Ship> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.WebGIS_Ship>();
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
                    sb.Append("INSERT INTO \"WebGIS_Ship\" (\"id\",\"ShipName\",\"mmsi\",\"ShipType\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var sn = a.ShipName == null ? "NULL" : $"'{a.ShipName.Replace("'", "''")}'";
                        var mm = a.mmsi == null ? "NULL" : $"'{a.mmsi.Replace("'", "''")}'";
                        var st = a.ShipType == null ? "NULL" : $"'{a.ShipType.Replace("'", "''")}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete == null ? "false" : (a.IsDelete.Value ? "true" : "false");
                        sb.Append($"({a.id},{sn},{mm},{st},{ct},{isd})");
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
            List<DataFactory.SqlServer.CloudWind.Wind_TaskInfoImg_ZJCZ> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Wind_TaskInfoImg_ZJCZ>();
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
                    sb.Append("INSERT INTO \"Wind_TaskInfoImg_ZJCZ\" (\"id\",\"TaskID\",\"FileName\",\"FilePath\",\"CreateTime\",\"IsDelete\") VALUES ");
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
            List<DataFactory.SqlServer.CloudWind.Data_PXYWeather> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Data_PXYWeather>();
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
                    sb.Append("INSERT INTO \"Data_PXYWeather\" (\"id\",\"TaskID\",\"PointName\",\"LonLat\",\"bm500\",\"humidity\",\"pressure\",\"swelldir\",\"swellheight\",\"swellperiod\",\"temperature\",\"visibility\",\"waveheight\",\"winddir\",\"windspeed\",\"windlevel\",\"windwarninglevel\",\"windalarm\",\"time\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var tid = a.TaskID?.ToString() ?? "NULL";
                        var pn = a.PointName == null ? "NULL" : $"'{a.PointName.Replace("'", "''")}'";
                        var ll = a.LonLat == null ? "NULL" : $"'{a.LonLat.Replace("'", "''")}'";
                        var b5 = a.bm500 == null ? "NULL" : $"'{a.bm500.Replace("'", "''")}'";
                        var hu = a.humidity == null ? "NULL" : $"'{a.humidity.Replace("'", "''")}'";
                        var pr = a.pressure == null ? "NULL" : $"'{a.pressure.Replace("'", "''")}'";
                        var sd = a.swelldir == null ? "NULL" : $"'{a.swelldir.Replace("'", "''")}'";
                        var sh = a.swellheight == null ? "NULL" : $"'{a.swellheight.Replace("'", "''")}'";
                        var sp = a.swellperiod == null ? "NULL" : $"'{a.swellperiod.Replace("'", "''")}'";
                        var te = a.temperature == null ? "NULL" : $"'{a.temperature.Replace("'", "''")}'";
                        var vi = a.visibility == null ? "NULL" : $"'{a.visibility.Replace("'", "''")}'";
                        var wh = a.waveheight == null ? "NULL" : $"'{a.waveheight.Replace("'", "''")}'";
                        var wd = a.winddir == null ? "NULL" : $"'{a.winddir.Replace("'", "''")}'";
                        var ws = a.windspeed == null ? "NULL" : $"'{a.windspeed.Replace("'", "''")}'";
                        var wl = a.windlevel == null ? "NULL" : $"'{a.windlevel.Replace("'", "''")}'";
                        var ww = a.windwarninglevel == null ? "NULL" : $"'{a.windwarninglevel.Replace("'", "''")}'";
                        var wa = a.windalarm == null ? "NULL" : (a.windalarm.Value ? "true" : "false");
                        var tm = a.time == null ? "NULL" : $"'{a.time.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        sb.Append($"({a.id},{tid},{pn},{ll},{b5},{hu},{pr},{sd},{sh},{sp},{te},{vi},{wh},{wd},{ws},{wl},{ww},{wa},{tm})");
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

        public void CopyManage_Platform()
        {
            List<DataFactory.SqlServer.CloudWind.Manage_Platform> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Manage_Platform>();
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
                    sb.Append("INSERT INTO \"Manage_Platform\" (\"ID\",\"Platform\",\"Url\",\"DepartID\",\"Introduction\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var pl = a.Platform == null ? "NULL" : $"'{a.Platform.Replace("'", "''")}'";
                        var url = a.Url == null ? "NULL" : $"'{a.Url.Replace("'", "''")}'";
                        var did = a.DepartID?.ToString() ?? "NULL";
                        var intro = a.Introduction == null ? "NULL" : $"'{a.Introduction.Replace("'", "''")}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete ? "true" : "false";
                        sb.Append($"({a.ID},{pl},{url},{did},{intro},{ct},{isd})");
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

        public void CopyFlow_ProjectApply()
        {
            List<DataFactory.SqlServer.CloudWind.Flow_ProjectApply> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Flow_ProjectApply>();
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
                    sb.Append("INSERT INTO \"Flow_ProjectApply\" (\"ID\",\"ProjectID\",\"NodeID\",\"FlowOrder\",\"NodeUserName\",\"NodeUserCode\",\"NodeUserDepart\",\"NodeUserPhone\",\"NodeUserJobName\",\"FlowHandle\",\"ApprovalTime\",\"Comment\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var pid = a.ProjectID?.ToString() ?? "NULL";
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
                        var isd = a.IsDelete ? "true" : "false";
                        sb.Append($"({a.ID},{pid},{nid},{fo},{un},{uc},{ud},{up},{uj},{fh},{at},{cm},{ct},{isd})");
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

        public void CopyFlow_Node()
        {
            List<DataFactory.SqlServer.CloudWind.Flow_Node> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Flow_Node>();
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
                    sb.Append("INSERT INTO \"Flow_Node\" (\"ID\",\"SoftwareID\",\"NodeNo\",\"NodeApprovalType\",\"DoEdit\",\"NodeName\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var sid = a.SoftwareID.ToString();
                        var nno = a.NodeNo.ToString();
                        var nat = a.NodeApprovalType == null ? "NULL" : (a.NodeApprovalType.Value ? "true" : "false");
                        var de = a.DoEdit == null ? "NULL" : (a.DoEdit.Value ? "true" : "false");
                        var nn = a.NodeName == null ? "NULL" : $"'{a.NodeName.Replace("'", "''")}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete ? "true" : "false";
                        sb.Append($"({a.ID},{sid},{nno},{nat},{de},{nn},{ct},{isd})");
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

        public void CopyWind_TaskInfo_QXSJ()
        {
            List<DataFactory.SqlServer.CloudWind.Wind_TaskInfo_QXSJ> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Wind_TaskInfo_QXSJ>();
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
                    sb.Append("INSERT INTO \"Wind_TaskInfo_QXSJ\" (\"id\",\"TaskID\",\"CollectStartTime\",\"CollectEndTime\",\"Points\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var tid = a.TaskID?.ToString() ?? "NULL";
                        var cst = a.CollectStartTime == null ? "NULL" : $"'{a.CollectStartTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var cet = a.CollectEndTime == null ? "NULL" : $"'{a.CollectEndTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var pts = a.Points == null ? "NULL" : $"'{a.Points.Replace("'", "''")}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete ? "true" : "false";
                        sb.Append($"({a.ID},{tid},{cst},{cet},{pts},{ct},{isd})");
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

        public void CopyWebGIS_ShipAnchor()
        {
            List<DataFactory.SqlServer.CloudWind.WebGIS_ShipAnchor> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.WebGIS_ShipAnchor>();
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
                    sb.Append("INSERT INTO \"WebGIS_ShipAnchor\" (\"id\",\"AnchorName\",\"AnchorGroup\",\"Lon\",\"Lat\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var an = a.AnchorName == null ? "NULL" : $"'{a.AnchorName.Replace("'", "''")}'";
                        var ag = a.AnchorGroup == null ? "NULL" : $"'{a.AnchorGroup.Replace("'", "''")}'";
                        var lon = a.Lon == null ? "NULL" : $"'{a.Lon.Replace("'", "''")}'";
                        var lat = a.Lat == null ? "NULL" : $"'{a.Lat.Replace("'", "''")}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete == null ? "false" : (a.IsDelete.Value ? "true" : "false");
                        sb.Append($"({a.id},{an},{ag},{lon},{lat},{ct},{isd})");
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

        public void CopyLibrary_FloatShip()
        {
            List<DataFactory.SqlServer.CloudWind.Library_FloatShip> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Library_FloatShip>();
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
                    sb.Append("INSERT INTO \"Library_FloatShip\" (\"id\",\"ShipName\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var sn = a.ShipName == null ? "NULL" : $"'{a.ShipName.Replace("'", "''")}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete == null ? "false" : (a.IsDelete.Value ? "true" : "false");
                        sb.Append($"({a.id},{sn},{ct},{isd})");
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

        public void CopyWind_TaskInfo_ZJCZ()
        {
            List<DataFactory.SqlServer.CloudWind.Wind_TaskInfo_ZJCZ> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Wind_TaskInfo_ZJCZ>();
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
                    sb.Append("INSERT INTO \"Wind_TaskInfo_ZJCZ\" (\"id\",\"TaskID\",\"ProjectIntroduce\",\"ProjectPosition\",\"zmtd\",\"HammerModel\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var tid = a.TaskID?.ToString() ?? "NULL";
                        var pi = a.ProjectIntroduce == null ? "NULL" : $"'{a.ProjectIntroduce.Replace("'", "''")}'";
                        var pp = a.ProjectPosition == null ? "NULL" : $"'{a.ProjectPosition.Replace("'", "''")}'";
                        var zm = a.zmtd == null ? "NULL" : $"'{a.zmtd.Replace("'", "''")}'";
                        var hm = a.HammerModel == null ? "NULL" : $"'{a.HammerModel.Replace("'", "''")}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete == null ? "false" : (a.IsDelete.Value ? "true" : "false");
                        sb.Append($"({a.id},{tid},{pi},{pp},{zm},{hm},{ct},{isd})");
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

        public void CopyWind_TaskInfo_THFX()
        {
            List<DataFactory.SqlServer.CloudWind.Wind_TaskInfo_THFX> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Wind_TaskInfo_THFX>();
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
                    sb.Append("INSERT INTO \"Wind_TaskInfo_THFX\" (\"id\",\"TaskID\",\"Scheme\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var tid = a.TaskID?.ToString() ?? "NULL";
                        var sc = a.Scheme == null ? "NULL" : $"'{a.Scheme.Replace("'", "''")}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete == null ? "false" : (a.IsDelete.Value ? "true" : "false");
                        sb.Append($"({a.id},{tid},{sc},{ct},{isd})");
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

        public void CopyWind_TaskInfo_XCFX()
        {
            List<DataFactory.SqlServer.CloudWind.Wind_TaskInfo_XCFX> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Wind_TaskInfo_XCFX>();
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
                    sb.Append("INSERT INTO \"Wind_TaskInfo_XCFX\" (\"id\",\"TaskID\",\"ShipIDs\",\"TaskDesc\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var tid = a.TaskID?.ToString() ?? "NULL";
                        var sids = a.ShipIDs == null ? "NULL" : $"'{a.ShipIDs.Replace("'", "''")}'";
                        var td = a.TaskDesc == null ? "NULL" : $"'{a.TaskDesc.Replace("'", "''")}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete == null ? "false" : (a.IsDelete.Value ? "true" : "false");
                        sb.Append($"({a.id},{tid},{sids},{td},{ct},{isd})");
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

        public void CopyWind_Task_ProjectFile()
        {
            List<DataFactory.SqlServer.CloudWind.Wind_Task_ProjectFile> listSql;
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Wind_Task_ProjectFile>();
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
                    sb.Append("INSERT INTO \"Wind_Task_ProjectFile\" (\"id\",\"ProjectID\",\"TaskID\",\"FileType\",\"FileName\",\"FilePath\",\"CreateTime\",\"IsDelete\") VALUES ");
                    int count = Math.Min(insertBatch, listSql.Count - i);
                    for (int j = 0; j < count; j++)
                    {
                        var a = listSql[i + j];
                        if (j > 0) sb.Append(',');
                        var pid = a.ProjectID?.ToString() ?? "NULL";
                        var tid = a.TaskID?.ToString() ?? "NULL";
                        var ft = a.FileType?.ToString() ?? "NULL";
                        var fn = a.FileName == null ? "NULL" : $"'{a.FileName.Replace("'", "''")}'";
                        var fp = a.FilePath == null ? "NULL" : $"'{a.FilePath.Replace("'", "''")}'";
                        var ct = a.CreateTime == null ? "NULL" : $"'{a.CreateTime.Value:yyyy-MM-dd HH:mm:ss.fff}'";
                        var isd = a.IsDelete == null ? "false" : (a.IsDelete.Value ? "true" : "false");
                        sb.Append($"({a.id},{pid},{tid},{ft},{fn},{fp},{ct},{isd})");
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

        public void test()
        {
            try
            {
                using (var uow = _windKingBaseUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.KingBase.CloudWind.WebGIS_Ship>();

                    repo.Add(new DataFactory.KingBase.CloudWind.WebGIS_Ship
                    {
                        ShipName = "测试船舶A",
                        mmsi = "413000001",
                        ShipType = "货船",
                        CreateTime = DateTime.Now,
                        IsDelete = false
                    });
                    repo.Add(new DataFactory.KingBase.CloudWind.WebGIS_Ship
                    {
                        ShipName = "测试船舶B",
                        mmsi = "413000002",
                        ShipType = "拖轮",
                        CreateTime = DateTime.Now,
                        IsDelete = false
                    });
                    repo.Add(new DataFactory.KingBase.CloudWind.WebGIS_Ship
                    {
                        ShipName = "测试船舶C",
                        mmsi = "413000003",
                        ShipType = "锚艇",
                        CreateTime = DateTime.Now,
                        IsDelete = false
                    });

                    uow.Save();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
