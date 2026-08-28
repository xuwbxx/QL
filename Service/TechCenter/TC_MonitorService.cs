using DataFactory.Factory;
using DataFactory.KingBase;
using Model.TechCenter.Monitor;

namespace Service.TechCenter
{
    public class TC_MonitorService
    {
        // 依赖注入：仓储工厂（核心）+ 日志（可选，用于异常追踪）
        private readonly SJKJC_KingBase_UnitOfWorkFactory _techCenterUowFactory;

        public TC_MonitorService(SJKJC_KingBase_UnitOfWorkFactory techCenterUowFactory)
        {
            _techCenterUowFactory = techCenterUowFactory;
        }



        public ProjectInfoResponse GetMonitorProject(MonitorRequest request)
        {
            ProjectInfoResponse data = new ProjectInfoResponse();

            Random r = new Random();

            //填海
            data.thProject.ProjectCount = r.Next(10, 30);
            data.thProject.ProjectFinishedCount = data.thProject.ProjectCount - r.Next(0, data.thProject.ProjectCount);
            data.thProject.ProjectUnfinishedCount = data.thProject.ProjectCount - data.thProject.ProjectFinishedCount;
            data.thProject.NormalProjectCount = data.thProject.ProjectUnfinishedCount - r.Next(0, data.thProject.ProjectUnfinishedCount);
            data.thProject.AbnormalProjectCount = data.thProject.ProjectUnfinishedCount - data.thProject.NormalProjectCount;

            data.thProject.ProjectEnum = ProjectTypeEnum.填海工程;
            data.thProject.ProjectName = ProjectTypeEnum.填海工程.ToString();

            for (int i = 0; i < r.Next(10, 20); i++)
            {
                data.thProject.Projects.Add(new ProjectAlarm()
                {
                    ProjectName = ProjectTypeEnum.填海工程.ToString() + r.Next(0, 100).ToString() + "号项目",
                    AlarmCount = r.Next(0, 999)
                });
            }



            //基坑
            data.jkProject.ProjectCount = r.Next(10, 30);
            data.jkProject.ProjectFinishedCount = data.jkProject.ProjectCount - r.Next(0, data.jkProject.ProjectCount);
            data.jkProject.ProjectUnfinishedCount = data.jkProject.ProjectCount - data.jkProject.ProjectFinishedCount;
            data.jkProject.NormalProjectCount = data.jkProject.ProjectUnfinishedCount - r.Next(0, data.jkProject.ProjectUnfinishedCount);
            data.jkProject.AbnormalProjectCount = data.jkProject.ProjectUnfinishedCount - data.jkProject.NormalProjectCount;

            data.jkProject.ProjectEnum = ProjectTypeEnum.基坑工程;
            data.jkProject.ProjectName = ProjectTypeEnum.基坑工程.ToString();

            for (int i = 0; i < r.Next(10, 20); i++)
            {
                data.jkProject.Projects.Add(new ProjectAlarm()
                {
                    ProjectName = ProjectTypeEnum.基坑工程.ToString() + r.Next(0, 100).ToString() + "号项目",
                    AlarmCount = r.Next(0, 999)
                });
            }

            //环境
            data.hjProject.ProjectCount = r.Next(10, 30);
            data.hjProject.ProjectFinishedCount = data.hjProject.ProjectCount - r.Next(0, data.hjProject.ProjectCount);
            data.hjProject.ProjectUnfinishedCount = data.hjProject.ProjectCount - data.hjProject.ProjectFinishedCount;
            data.hjProject.NormalProjectCount = data.hjProject.ProjectUnfinishedCount - r.Next(0, data.hjProject.ProjectUnfinishedCount);
            data.hjProject.AbnormalProjectCount = data.hjProject.ProjectUnfinishedCount - data.hjProject.NormalProjectCount;

            data.hjProject.ProjectEnum = ProjectTypeEnum.环境工程;
            data.hjProject.ProjectName = ProjectTypeEnum.环境工程.ToString();

            for (int i = 0; i < r.Next(10, 20); i++)
            {
                data.hjProject.Projects.Add(new ProjectAlarm()
                {
                    ProjectName = ProjectTypeEnum.环境工程.ToString() + r.Next(0, 100).ToString() + "号项目",
                    AlarmCount = r.Next(0, 999)
                });
            }

            //桥梁
            data.qlProject.ProjectCount = r.Next(10, 30);
            data.qlProject.ProjectFinishedCount = data.qlProject.ProjectCount - r.Next(0, data.qlProject.ProjectCount);
            data.qlProject.ProjectUnfinishedCount = data.qlProject.ProjectCount - data.qlProject.ProjectFinishedCount;
            data.qlProject.NormalProjectCount = data.qlProject.ProjectUnfinishedCount - r.Next(0, data.qlProject.ProjectUnfinishedCount);
            data.qlProject.AbnormalProjectCount = data.qlProject.ProjectUnfinishedCount - data.qlProject.NormalProjectCount;

            data.qlProject.ProjectEnum = ProjectTypeEnum.桥梁工程;
            data.qlProject.ProjectName = ProjectTypeEnum.桥梁工程.ToString();

            for (int i = 0; i < r.Next(10, 20); i++)
            {
                data.qlProject.Projects.Add(new ProjectAlarm()
                {
                    ProjectName = ProjectTypeEnum.桥梁工程.ToString() + r.Next(0, 100).ToString() + "号项目",
                    AlarmCount = r.Next(0, 999)
                });
            }

            //年度监控正常项目数量（最近五年）
            for (int i = 0; i < 5; i++)
            {
                int year = DateTime.Now.Year - 5 + i;

                data.thProject.YearNormalProjectCountChart.Add(new YearNormalProjectChart()
                {
                    Year = year,
                    ProjectCount = r.Next(0, 20)
                });

                data.jkProject.YearNormalProjectCountChart.Add(new YearNormalProjectChart()
                {
                    Year = year,
                    ProjectCount = r.Next(0, 20)
                });

                data.hjProject.YearNormalProjectCountChart.Add(new YearNormalProjectChart()
                {
                    Year = year,
                    ProjectCount = r.Next(0, 20)
                });

                data.qlProject.YearNormalProjectCountChart.Add(new YearNormalProjectChart()
                {
                    Year = year,
                    ProjectCount = r.Next(0, 20)
                });
            }


            //项目测点一览
            for (int i = 0; i < 20; i++)
            {

                Array values = Enum.GetValues(typeof(ProjectTypeEnum));

                ProjectTypeEnum randomType = (ProjectTypeEnum)values.GetValue(r.Next(values.Length));

                data.NormalProjects.Add(new NormalProject()
                {
                    ProjectID = r.Next(0, 100),

                    ProjectEnum = randomType,
                    ProjectName = randomType.ToString() + r.Next(0, 100).ToString() + "项目"
                });
            }

            //监控异常项目数量



            //监控异常项目列表
            for (int i = 0; i < 40; i++)
            {
                Array values = Enum.GetValues(typeof(ProjectTypeEnum));

                ProjectTypeEnum randomType = (ProjectTypeEnum)values.GetValue(r.Next(values.Length));

                var RandomTime = DateTime.Now.AddSeconds(-r.Next(0, 9999999));

                data.AbnormalProjectPoints.Add(new AbnormalProjectPoint()
                {

                    ProjectEnum = randomType,

                    ProjectID = r.Next(0, 100),

                    ProjectName = randomType.ToString() + r.Next(0, 100).ToString() + "项目",

                    ProjectPoint = randomType.ToString() + "-第" + r.Next(0, 1000) + "号监测点",

                    CreateTime = RandomTime,

                    Time = RandomTime.ToString("yyyy-MM-dd HH:mm:ss")

                });
            }

            data.AbnormalProjectPoints = data.AbnormalProjectPoints.OrderByDescending(a => a.CreateTime).ToList();


            return data;
        }



        public async Task<ProjectInfoResponse> GetMonitorProject_KuLun(int pid)
        {
            ProjectInfoResponse data = new ProjectInfoResponse();

            Random r = new Random();

            //填海
            data.thProject.ProjectCount = r.Next(10, 30);
            data.thProject.ProjectFinishedCount = data.thProject.ProjectCount - r.Next(0, data.thProject.ProjectCount);
            data.thProject.ProjectUnfinishedCount = data.thProject.ProjectCount - data.thProject.ProjectFinishedCount;
            data.thProject.NormalProjectCount = data.thProject.ProjectUnfinishedCount - r.Next(0, data.thProject.ProjectUnfinishedCount);
            data.thProject.AbnormalProjectCount = data.thProject.ProjectUnfinishedCount - data.thProject.NormalProjectCount;

            data.thProject.ProjectEnum = ProjectTypeEnum.填海工程;
            data.thProject.ProjectName = ProjectTypeEnum.填海工程.ToString();

            //for (int i = 0; i < r.Next(10, 20); i++)
            //{
            //    data.thProject.Projects.Add(new ProjectAlarm()
            //    {
            //        ProjectName = ProjectTypeEnum.填海工程.ToString() + r.Next(0, 100).ToString() + "号项目",
            //        AlarmCount = r.Next(0, 999)
            //    });
            //}



            //基坑
            data.jkProject.ProjectCount = r.Next(10, 30);
            data.jkProject.ProjectFinishedCount = data.jkProject.ProjectCount - r.Next(0, data.jkProject.ProjectCount);
            data.jkProject.ProjectUnfinishedCount = data.jkProject.ProjectCount - data.jkProject.ProjectFinishedCount;
            data.jkProject.NormalProjectCount = data.jkProject.ProjectUnfinishedCount - r.Next(0, data.jkProject.ProjectUnfinishedCount);
            data.jkProject.AbnormalProjectCount = data.jkProject.ProjectUnfinishedCount - data.jkProject.NormalProjectCount;

            data.jkProject.ProjectEnum = ProjectTypeEnum.基坑工程;
            data.jkProject.ProjectName = ProjectTypeEnum.基坑工程.ToString();

            //for (int i = 0; i < r.Next(10, 20); i++)
            //{
            //    data.jkProject.Projects.Add(new ProjectAlarm()
            //    {
            //        ProjectName = ProjectTypeEnum.基坑工程.ToString() + r.Next(0, 100).ToString() + "号项目",
            //        AlarmCount = r.Next(0, 999)
            //    });
            //}

            //环境
            data.hjProject.ProjectCount = r.Next(10, 30);
            data.hjProject.ProjectFinishedCount = data.hjProject.ProjectCount - r.Next(0, data.hjProject.ProjectCount);
            data.hjProject.ProjectUnfinishedCount = data.hjProject.ProjectCount - data.hjProject.ProjectFinishedCount;
            data.hjProject.NormalProjectCount = data.hjProject.ProjectUnfinishedCount - r.Next(0, data.hjProject.ProjectUnfinishedCount);
            data.hjProject.AbnormalProjectCount = data.hjProject.ProjectUnfinishedCount - data.hjProject.NormalProjectCount;

            data.hjProject.ProjectEnum = ProjectTypeEnum.环境工程;
            data.hjProject.ProjectName = ProjectTypeEnum.环境工程.ToString();

            //for (int i = 0; i < r.Next(10, 20); i++)
            //{
            //    data.hjProject.Projects.Add(new ProjectAlarm()
            //    {
            //        ProjectName = ProjectTypeEnum.环境工程.ToString() + r.Next(0, 100).ToString() + "号项目",
            //        AlarmCount = r.Next(0, 999)
            //    });
            //}

            //桥梁
            data.qlProject.ProjectCount = r.Next(10, 30);
            data.qlProject.ProjectFinishedCount = data.qlProject.ProjectCount - r.Next(0, data.qlProject.ProjectCount);
            data.qlProject.ProjectUnfinishedCount = data.qlProject.ProjectCount - data.qlProject.ProjectFinishedCount;
            data.qlProject.NormalProjectCount = data.qlProject.ProjectUnfinishedCount - r.Next(0, data.qlProject.ProjectUnfinishedCount);
            data.qlProject.AbnormalProjectCount = data.qlProject.ProjectUnfinishedCount - data.qlProject.NormalProjectCount;

            data.qlProject.ProjectEnum = ProjectTypeEnum.桥梁工程;
            data.qlProject.ProjectName = ProjectTypeEnum.桥梁工程.ToString();

            //for (int i = 0; i < r.Next(10, 20); i++)
            //{
            //    data.qlProject.Projects.Add(new ProjectAlarm()
            //    {
            //        ProjectName = ProjectTypeEnum.桥梁工程.ToString() + r.Next(0, 100).ToString() + "号项目",
            //        AlarmCount = r.Next(0, 999)
            //    });
            //}

            //年度监控正常项目数量（最近五年）
            //for (int i = 0; i < 5; i++)
            //{
            //    int year = DateTime.Now.Year - 5 + i;

            //    data.thProject.YearNormalProjectCountChart.Add(new YearNormalProjectChart()
            //    {
            //        Year = year,
            //        ProjectCount = r.Next(0, 20)
            //    });

            //    data.jkProject.YearNormalProjectCountChart.Add(new YearNormalProjectChart()
            //    {
            //        Year = year,
            //        ProjectCount = r.Next(0, 20)
            //    });

            //    data.hjProject.YearNormalProjectCountChart.Add(new YearNormalProjectChart()
            //    {
            //        Year = year,
            //        ProjectCount = r.Next(0, 20)
            //    });

            //    data.qlProject.YearNormalProjectCountChart.Add(new YearNormalProjectChart()
            //    {
            //        Year = year,
            //        ProjectCount = r.Next(0, 20)
            //    });
            //}


            //监控正常项目列表
            for (int i = 0; i < 20; i++)
            {

                Array values = Enum.GetValues(typeof(ProjectTypeEnum));

                ProjectTypeEnum randomType = (ProjectTypeEnum)values.GetValue(r.Next(values.Length));

                data.NormalProjects.Add(new NormalProject()
                {
                    ProjectID = r.Next(0, 100),

                    ProjectEnum = randomType,
                    ProjectName = randomType.ToString() + r.Next(0, 100).ToString() + "项目"
                });
            }

            //监控异常项目数量



            //监控异常项目列表
            //for (int i = 0; i < 40; i++)
            //{
            //    Array values = Enum.GetValues(typeof(ProjectTypeEnum));

            //    ProjectTypeEnum randomType = (ProjectTypeEnum)values.GetValue(r.Next(values.Length));

            //    var RandomTime = DateTime.Now.AddSeconds(-r.Next(0, 9999999));

            //    data.AbnormalProjectPoints.Add(new AbnormalProjectPoint()
            //    {

            //        ProjectEnum = randomType,

            //        ProjectID = r.Next(0, 100),

            //        ProjectName = randomType.ToString() + r.Next(0, 100).ToString() + "项目",

            //        ProjectPoint = randomType.ToString() + "-第" + r.Next(0, 1000) + "号监测点",

            //        CreateTime = RandomTime,

            //        Time = RandomTime.ToString("yyyy-MM-dd HH:mm:ss")

            //    });
            //}


            using (var uow = _techCenterUowFactory.Create())
            {
                var projectRepo = uow.GetRepository<ZJSHJ_Project>();

                var projects = (await projectRepo.FindAsync(a => a.status == 1)).ToList();

                var project = projects.FirstOrDefault(a => a.id == pid);

                data.ProjectName = project.project_name ?? "未知";

                // 获取软件仓储
                var pointRepo = uow.GetRepository<ZJSHJ_Monitor_Point>();

                var points = (await pointRepo.FindAsync(a => a.pid == pid)).ToList();

                var thPoints = points.Where(a => a.monitoringType.Equals("地表竖向位移")).ToList();
                var jkPoints = points.Where(a => a.monitoringType.Equals("支撑轴力")).ToList();
                var hjPoints = points.Where(a => a.monitoringType.Equals("深层水平位移")).ToList();
                var qlPoints = points.Where(a => a.monitoringType.Equals("地下水位")).ToList();

                //监测点分类部署情况
                data.thProject.ProjectFinishedCount = thPoints.Count;
                data.jkProject.ProjectFinishedCount = jkPoints.Count;
                data.hjProject.ProjectFinishedCount = hjPoints.Count;
                data.qlProject.ProjectFinishedCount = qlPoints.Count;

                for (int i = 0; i < 3; i++)
                {
                    int year = DateTime.Now.Year - 2 + i;

                    DateTime startTime = new DateTime(year, 1, 1, 0, 0, 0);
                    DateTime endTime = new DateTime(year + 1, 1, 1, 0, 0, 0);

                    data.thProject.YearNormalProjectCountChart.Add(new YearNormalProjectChart()
                    {
                        Year = year,
                        ProjectCount = thPoints.Count(a => a.add_time >= startTime && a.add_time < endTime)
                    });

                    data.jkProject.YearNormalProjectCountChart.Add(new YearNormalProjectChart()
                    {
                        Year = year,
                        ProjectCount = jkPoints.Count(a => a.add_time >= startTime && a.add_time < endTime)
                    });

                    data.hjProject.YearNormalProjectCountChart.Add(new YearNormalProjectChart()
                    {
                        Year = year,
                        ProjectCount = hjPoints.Count(a => a.add_time >= startTime && a.add_time < endTime)
                    });

                    data.qlProject.YearNormalProjectCountChart.Add(new YearNormalProjectChart()
                    {
                        Year = year,
                        ProjectCount = qlPoints.Count(a => a.add_time >= startTime && a.add_time < endTime)
                    });
                }

                thPoints.OrderByDescending(a => a.add_time).ToList().ForEach(a =>
                {
                    data.thProject.Projects.Add(new ProjectAlarm()
                    {
                        ProjectName = a.pointNumber ?? "",
                        AlarmCount = 0
                    });
                });

                jkPoints.OrderByDescending(a => a.add_time).ToList().ForEach(a =>
                {
                    data.jkProject.Projects.Add(new ProjectAlarm()
                    {
                        ProjectName = a.pointNumber ?? "",
                        AlarmCount = 0
                    });
                });

                hjPoints.OrderByDescending(a => a.add_time).ToList().ForEach(a =>
                {
                    data.hjProject.Projects.Add(new ProjectAlarm()
                    {
                        ProjectName = a.pointNumber ?? "",
                        AlarmCount = 0
                    });
                });

                qlPoints.OrderByDescending(a => a.add_time).ToList().ForEach(a =>
                {
                    data.qlProject.Projects.Add(new ProjectAlarm()
                    {
                        ProjectName = a.pointNumber ?? "",
                        AlarmCount = 0
                    });
                });





                //foreach (var item in projects.Where(a => a.projectType.Equals("基坑工程")).OrderByDescending(a => a.add_time).ToList())
                //{
                //    data.jkProject.Projects.Add(new ProjectAlarm()
                //    {
                //        ProjectName = item.project_name ?? "",
                //        AlarmCount = 0
                //    });
                //}


                //foreach (var item in projects.Where(a => a.projectType.Equals("填海工程")).OrderByDescending(a => a.add_time).ToList())
                //{
                //    data.thProject.Projects.Add(new ProjectAlarm()
                //    {
                //        ProjectName = item.project_name ?? "",
                //        AlarmCount = 0
                //    });
                //}



                foreach (var point in points)
                {
                    data.NormalProjects.Add(new NormalProject()
                    {
                        CreateTime = point.add_time,
                        PointName = point.pointNumber ?? "",
                        Type = point.monitoringType ?? ""
                    });
                }

                data.NormalProjects = data.NormalProjects.OrderByDescending(a => a.CreateTime).ToList();



                //报警测点
                foreach (var point in points)
                {
                    data.AbnormalProjectPoints.Add(new AbnormalProjectPoint()
                    {
                        ProjectPoint = point.pointNumber ?? "",
                        Time = point.add_time.ToString("yyyy-MM-dd HH:mm:ss"),
                        CreateTime = point.add_time
                    });
                }
                data.AbnormalProjectPoints = data.AbnormalProjectPoints.OrderByDescending(a => a.CreateTime).ToList();




            }




            return data;
        }

    }
}
