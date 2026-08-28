using DataFactory.Factory;
using DataFactory.KingBase.CloudWind;
using System.Data;

namespace Service.Base.Data
{
    public class DataCopyService
    {

        private readonly CloudWind_KingBase_UnitOfWorkFactory _windKingBaseUowFactory;

        private readonly CloudWind_Sql_UnitOfWorkFactory _windSqlUowFactory;


        public DataCopyService(CloudWind_KingBase_UnitOfWorkFactory windCenterUowFactory, CloudWind_Sql_UnitOfWorkFactory windSqlUowFactory)
        {
            _windKingBaseUowFactory = windCenterUowFactory;
            _windSqlUowFactory = windSqlUowFactory;
        }

        public void DataSave()
        {
            List<DataFactory.SqlServer.Wind_ProjectRole> listSql = new List<DataFactory.SqlServer.Wind_ProjectRole>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.Wind_ProjectRole>();

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
            List<DataFactory.SqlServer.Library_Geology> listSql = new List<DataFactory.SqlServer.Library_Geology>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.Library_Geology>();
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
            List<DataFactory.SqlServer.Library_Geology_DK> listSql = new List<DataFactory.SqlServer.Library_Geology_DK>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.Library_Geology_DK>();
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
                var repo = uow.GetRepository<DataFactory.SqlServer.Library_Geology_Data>();
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
            List<DataFactory.SqlServer.Manage_Company> listSql = new List<DataFactory.SqlServer.Manage_Company>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.Manage_Company>();
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
            List<DataFactory.SqlServer.Wind_ProjectFile> listSql = new List<DataFactory.SqlServer.Wind_ProjectFile>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.Wind_ProjectFile>();
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
            List<DataFactory.SqlServer.Manage_Viewer> listSql = new List<DataFactory.SqlServer.Manage_Viewer>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.Manage_Viewer>();
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

        public void CopyWind_ProjectInfo()
        {
            List<DataFactory.SqlServer.Wind_ProjectInfo> listSql = new List<DataFactory.SqlServer.Wind_ProjectInfo>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.Wind_ProjectInfo>();
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
            List<DataFactory.SqlServer.Wind_ProjectRole> listSql = new List<DataFactory.SqlServer.Wind_ProjectRole>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.Wind_ProjectRole>();
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
            List<DataFactory.SqlServer.Wind_ProjectArea> listSql = new List<DataFactory.SqlServer.Wind_ProjectArea>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.Wind_ProjectArea>();
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
            List<DataFactory.SqlServer.Wind_ProjectFan> listSql = new List<DataFactory.SqlServer.Wind_ProjectFan>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.Wind_ProjectFan>();
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
            List<DataFactory.SqlServer.Wind_Project_Copyer> listSql = new List<DataFactory.SqlServer.Wind_Project_Copyer>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.Wind_Project_Copyer>();
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
            List<DataFactory.SqlServer.Wind_Task> listSql = new List<DataFactory.SqlServer.Wind_Task>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.Wind_Task>();
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
            List<DataFactory.SqlServer.Wind_TaskFileDeliver> listSql = new List<DataFactory.SqlServer.Wind_TaskFileDeliver>();
            using (var uow = _windSqlUowFactory.Create())
            {
                var repo = uow.GetRepository<DataFactory.SqlServer.Wind_TaskFileDeliver>();
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


    }
}
