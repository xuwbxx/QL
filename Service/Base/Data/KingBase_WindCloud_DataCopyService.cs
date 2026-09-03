using DataFactory.Factory;
using DataFactory.KingBase.CloudWind;
using System.Data;

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
            List<DataFactory.SqlServer.CloudWind.Manage_Company> listSql = new List<DataFactory.SqlServer.CloudWind.Manage_Company>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Manage_Company>();
                listSql = repo.FindAll().ToList();
            }

            List<DataFactory.KingBase.CloudWind.Manage_Company> listKing = new List<DataFactory.KingBase.CloudWind.Manage_Company>();
            using (var uow = _windKingBaseUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.KingBase.CloudWind.Manage_Company>();

                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.CloudWind.Manage_Company()
                    {
                        Company = a.Company,
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

            List<DataFactory.KingBase.CloudWind.Library_Ship_Data> listKing = new List<DataFactory.KingBase.CloudWind.Library_Ship_Data>();
            using (var uow = _windKingBaseUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.KingBase.CloudWind.Library_Ship_Data>();

                foreach (DataRow dr in dt.Rows)
                {
                    listKing.Add(new DataFactory.KingBase.CloudWind.Library_Ship_Data()
                    {
                        ShipID = Convert.ToInt32(dr[1].ToString()),
                        ztzj = dr[2].ToString(),
                        ztzj_unit = dr[3].ToString(),
                        ztzc = dr[4].ToString(),
                        ztzc_unit = dr[5].ToString(),
                        zxcdL = dr[6].ToString(),
                        zxcdL_unit = dr[7].ToString(),
                        zxkdB = dr[8].ToString(),
                        zxkdB_unit = dr[9].ToString(),
                        zxgdH = dr[10].ToString(),
                        zxgdH_unit = dr[11].ToString(),
                        zxmjA = dr[12].ToString(),
                        zxmjA_unit = dr[13].ToString(),
                        zxzdjmzc = dr[14].ToString(),
                        zxzdjmzc_unit = dr[15].ToString(),
                        zxtjV = dr[16].ToString(),
                        zxtjV_unit = dr[17].ToString(),
                        ztzxzzW = dr[18].ToString(),
                        ztzxzzW_unit = dr[19].ToString(),
                        ztyyl = dr[20].ToString(),
                        ztyyl_unit = dr[21].ToString(),
                        jsyyhz = dr[22].ToString(),
                        jsyyhz_unit = dr[23].ToString(),
                        bzl = dr[24].ToString(),
                        bzl_unit = dr[25].ToString(),
                        ddby = dr[26].ToString(),
                        ddby_unit = dr[27].ToString(),
                        yxztcd_cddxd = dr[28].ToString(),
                        yxztcd_cddxd_unit = dr[29].ToString(),
                        qx_cddsm = dr[30].ToString(),
                        qx_cddsm_unit = dr[31].ToString(),
                        ztyxcd = dr[32].ToString(),
                        ztyxcd_unit = dr[33].ToString(),
                        ztjmj = dr[34].ToString(),
                        ztjmj_unit = dr[35].ToString(),
                        CreateTime = string.IsNullOrEmpty(dr[36].ToString()) ? null : DateTime.SpecifyKind(Convert.ToDateTime(dr[36].ToString()), DateTimeKind.Unspecified),
                        IsDelete = Convert.ToBoolean(dr[37].ToString())
                    });
                }


                if (listKing.Count > 0)
                {
                    repo.AddList(listKing);
                    repo.Save();
                }
            }
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
            List<DataFactory.SqlServer.CloudWind.Wind_ProjectRole> listSql = new List<DataFactory.SqlServer.CloudWind.Wind_ProjectRole>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Wind_ProjectRole>();
                listSql = repo.FindAll().ToList();
            }

            List<DataFactory.KingBase.CloudWind.Wind_ProjectRole> listKing = new List<DataFactory.KingBase.CloudWind.Wind_ProjectRole>();
            using (var uow = _windKingBaseUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.KingBase.CloudWind.Wind_ProjectRole>();

                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.CloudWind.Wind_ProjectRole()
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
            List<DataFactory.SqlServer.CloudWind.Wind_Task> listSql = new List<DataFactory.SqlServer.CloudWind.Wind_Task>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Wind_Task>();
                listSql = repo.FindAll().ToList();
            }

            List<DataFactory.KingBase.CloudWind.Wind_Task> listKing = new List<DataFactory.KingBase.CloudWind.Wind_Task>();
            using (var uow = _windKingBaseUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.KingBase.CloudWind.Wind_Task>();

                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.CloudWind.Wind_Task()
                    {
                        ProjectID = a.ProjectID,
                        TaskCode = a.TaskCode,
                        TaskName = a.TaskName,
                        FlowStatus = a.FlowStatus,
                        SoftwareID = a.SoftwareID,
                        Applyer = a.Applyer,
                        ApplyerCode = a.ApplyerCode,
                        ApplyerDepart = a.ApplyerDepart,
                        ApplyerPhone = a.ApplyerPhone,
                        ApplyerJobName = a.ApplyerJobName,
                        DeliverTime = a.DeliverTime == null ? null : DateTime.SpecifyKind(a.DeliverTime.Value, DateTimeKind.Utc),
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

        public void CopyWind_TaskFileDeliver()
        {
            List<DataFactory.SqlServer.CloudWind.Wind_TaskFileDeliver> listSql = new List<DataFactory.SqlServer.CloudWind.Wind_TaskFileDeliver>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Wind_TaskFileDeliver>();
                listSql = repo.FindAll().ToList();
            }

            List<DataFactory.KingBase.CloudWind.Wind_TaskFileDeliver> listKing = new List<DataFactory.KingBase.CloudWind.Wind_TaskFileDeliver>();
            using (var uow = _windKingBaseUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.KingBase.CloudWind.Wind_TaskFileDeliver>();

                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.CloudWind.Wind_TaskFileDeliver()
                    {
                        TaskID = a.TaskID,
                        DeliverName = a.DeliverName,
                        DeliverCode = a.DeliverCode,
                        DeliverDepart = a.DeliverDepart,
                        DeliverPhone = a.DeliverPhone,
                        DeliverJobName = a.DeliverJobName,
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
            List<DataFactory.SqlServer.CloudWind.Flow_Task_DKFile> listSql = new List<DataFactory.SqlServer.CloudWind.Flow_Task_DKFile>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Flow_Task_DKFile>();
                listSql = repo.FindAll().ToList();
            }

            List<DataFactory.KingBase.CloudWind.Flow_Task_DKFile> listKing = new List<DataFactory.KingBase.CloudWind.Flow_Task_DKFile>();
            using (var uow = _windKingBaseUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.KingBase.CloudWind.Flow_Task_DKFile>();

                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.CloudWind.Flow_Task_DKFile()
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

        public void CopyFlow_Task_ShipFile()
        {
            List<DataFactory.SqlServer.CloudWind.Flow_Task_ShipFile> listSql = new List<DataFactory.SqlServer.CloudWind.Flow_Task_ShipFile>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Flow_Task_ShipFile>();
                listSql = repo.FindAll().ToList();
            }

            List<DataFactory.KingBase.CloudWind.Flow_Task_ShipFile> listKing = new List<DataFactory.KingBase.CloudWind.Flow_Task_ShipFile>();
            using (var uow = _windKingBaseUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.KingBase.CloudWind.Flow_Task_ShipFile>();

                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.CloudWind.Flow_Task_ShipFile()
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

            List<DataFactory.KingBase.CloudWind.Library_Ship> listKing = new List<DataFactory.KingBase.CloudWind.Library_Ship>();
            using (var uow = _windKingBaseUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.KingBase.CloudWind.Library_Ship>();

                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.CloudWind.Library_Ship()
                    {
                        ID = a.ID,
                        TaskID = a.TaskID,
                        ShipName = a.ShipName,
                        FileName = a.FileName,
                        FilePath = a.FilePath,
                        IsConfirm = a.IsConfirm,
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
            List<DataFactory.SqlServer.CloudWind.Manage_Copyer> listSql = new List<DataFactory.SqlServer.CloudWind.Manage_Copyer>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Manage_Copyer>();
                listSql = repo.FindAll().ToList();
            }

            List<DataFactory.KingBase.CloudWind.Manage_Copyer> listKing = new List<DataFactory.KingBase.CloudWind.Manage_Copyer>();
            using (var uow = _windKingBaseUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.KingBase.CloudWind.Manage_Copyer>();

                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.CloudWind.Manage_Copyer()
                    {
                        ID = a.ID,
                        SoftwareID = a.SoftwareID,
                        UserName = a.UserName,
                        UserCode = a.UserCode,
                        UserDepart = a.UserDepart,
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
            List<DataFactory.SqlServer.CloudWind.Wind_TaskFile> listSql = new List<DataFactory.SqlServer.CloudWind.Wind_TaskFile>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Wind_TaskFile>();
                listSql = repo.FindAll().ToList();
            }

            List<DataFactory.KingBase.CloudWind.Wind_TaskFile> listKing = new List<DataFactory.KingBase.CloudWind.Wind_TaskFile>();
            using (var uow = _windKingBaseUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.KingBase.CloudWind.Wind_TaskFile>();

                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.CloudWind.Wind_TaskFile()
                    {
                        id = a.id,
                        TaskID = a.TaskID,
                        TypeID = a.TypeID,
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
            List<DataFactory.SqlServer.CloudWind.Wind_TaskReport> listSql = new List<DataFactory.SqlServer.CloudWind.Wind_TaskReport>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Wind_TaskReport>();
                listSql = repo.FindAll().ToList();
            }

            List<DataFactory.KingBase.CloudWind.Wind_TaskReport> listKing = new List<DataFactory.KingBase.CloudWind.Wind_TaskReport>();
            using (var uow = _windKingBaseUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.KingBase.CloudWind.Wind_TaskReport>();

                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.CloudWind.Wind_TaskReport()
                    {
                        id = a.id,
                        TaskID = a.TaskID,
                        Type = a.Type,
                        ReportName = a.ReportName,
                        ReportPath = a.ReportPath,
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

        public void CopyWind_TaskInfo_KZY()
        {
            List<DataFactory.SqlServer.CloudWind.Wind_TaskInfo_KZY> listSql = new List<DataFactory.SqlServer.CloudWind.Wind_TaskInfo_KZY>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.CloudWind.Wind_TaskInfo_KZY>();
                listSql = repo.FindAll().ToList();
            }

            List<DataFactory.KingBase.CloudWind.Wind_TaskInfo_KZY> listKing = new List<DataFactory.KingBase.CloudWind.Wind_TaskInfo_KZY>();
            using (var uow = _windKingBaseUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.KingBase.CloudWind.Wind_TaskInfo_KZY>();

                listSql.ForEach(a =>
                {
                    listKing.Add(new DataFactory.KingBase.CloudWind.Wind_TaskInfo_KZY()
                    {
                        id = a.id,
                        TaskID = a.TaskID,
                        ShipIDs = a.ShipIDs,
                        TechType = a.TechType,
                        ForecastStartTime = a.ForecastStartTime == null ? null : DateTime.SpecifyKind(a.ForecastStartTime.Value, DateTimeKind.Unspecified),
                        ForecastEndTime = a.ForecastEndTime == null ? null : DateTime.SpecifyKind(a.ForecastEndTime.Value, DateTimeKind.Unspecified),
                        YFYNo = a.YFYNo,
                        Balance = a.Balance,
                        Lon = a.Lon,
                        Lat = a.Lat,
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

        

    }
}
