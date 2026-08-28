using DataFactory.Factory;
using DataFactory.MySql;
using Model.StructHandle;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using Tool;

namespace Service.StructHandle
{
    public class WorkService
    {

        // 依赖注入：仓储工厂（核心）+ 日志（可选，用于异常追踪）
        private readonly StructHandle_MySql_Test_UnitOfWorkFactory _structHandleDbFactory;

        public WorkService(StructHandle_MySql_Test_UnitOfWorkFactory structHandleDbFactory)
        {
            _structHandleDbFactory = structHandleDbFactory;
        }

        public async Task<List<WorkAreaResponse>> WorkAreaList(StructHandleRequest request)
        {
            List<WorkAreaResponse> list = new List<WorkAreaResponse>();
            try
            {
                using (var uow = _structHandleDbFactory.Create())
                {
                    // 获取软件仓储
                    var areaRepo = uow.GetRepository<data_work_area>();
                    var areaPointRepo = uow.GetRepository<data_work_area_point>();


                    // 先解析字符串类型的时间参数（安全解析，避免格式错误）
                    DateTime startTime;
                    bool isStartTimeValid = DateTime.TryParse(request.startTime, out startTime);

                    DateTime endTime;
                    bool isEndTimeValid = DateTime.TryParse(request.endTime, out endTime);

                    // 直接构建包含所有非空判断的表达式
                    Expression<Func<data_work_area, bool>> query = x =>
                        // ID有值时匹配ID
                        ((string.IsNullOrEmpty(request.areaNo) || x.AreaNo.Contains(request.areaNo))
                        // 部门ID大于0时匹配
                        && (string.IsNullOrEmpty(request.UserName) || x.UserName.Contains(request.UserName))
                        // 时间范围有值时匹配
                        && ((!isStartTimeValid || !isEndTimeValid) || (x.CreateTime >= startTime && x.CreateTime <= endTime))
                        && x.IsDelete == 0);

                    // 获取部门仓储
                    var queryResult = await areaRepo.FindAsync(query);

                    queryResult.OrderByDescending(a => a.CreateTime).ToList().ForEach(a =>
                    {

                        var finishCount = areaPointRepo.FindCount(b => b.AreaID == a.ID && b.Status == 2);
                        var allCount = areaPointRepo.FindCount(b => b.AreaID == a.ID);
                        double finishRate = 0;
                        if (allCount != 0)
                        {
                            // 第一步：先解决整数除法精度问题（比如 1/2 会变成 0.5 而不是 0）
                            finishRate = Math.Round((double)finishCount / allCount, 4);

                        }

                        WorkAreaResponse se = new WorkAreaResponse();
                        se.ID = a.ID;
                        se.AreaNo = a.AreaNo ?? "未知";
                        se.Square = a.WorkColumn + "×" + a.WorkRow;
                        se.CreateTime1 = a.CreateTime.ToString("yyyy-MM-dd");
                        se.CreateTime2 = a.CreateTime.ToString("HH:mm");
                        se.StartX = a.StartX.ToString();
                        se.StartY = a.StartY.ToString();
                        se.EndX = a.EndX.ToString();
                        se.EndY = a.EndY.ToString();
                        se.UserName = a.UserName ?? "";
                        se.FinishRate = (finishRate * 100).ToString("0.00") + "%"; // 结果如 85.00%
                        list.Add(se);

                    });



                    return list;
                }

            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(WorkService));
                return new List<WorkAreaResponse>();
            }
        }

        public async Task<data_work_area?> WorkAreaData(int ID)
        {
            try
            {
                using (var uow = _structHandleDbFactory.Create())
                {
                    // 获取软件仓储
                    var areaRepo = uow.GetRepository<data_work_area>();
                    // 获取部门仓储
                    var data = await areaRepo.FindByIDAsync(ID);

                    if (data == null)
                    {
                        return null;
                    }

                    return data;
                }

            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(WorkService));
                return null;
            }
        }

        public async Task<int> WorkAreaAdd(data_work_area data)
        {
            try
            {
                using (var uow = _structHandleDbFactory.Create())
                {
                    // 获取软件仓储
                    var areaRepo = uow.GetRepository<data_work_area>();
                    // 获取部门仓储
                    await areaRepo.AddAsync(data);

                    int count = await uow.SaveAsync();

                    if (count > 0)
                    {
                        return data.ID;
                    }
                    return 0;
                }

            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(WorkService));
                return 0;
            }
        }

        public async Task<bool> WorkAreaUpdate(data_work_area data)
        {
            try
            {
                if (data.ID == 0)
                {
                    return false;
                }

                using (var uow = _structHandleDbFactory.Create())
                {
                    // 获取软件仓储
                    var areaRepo = uow.GetRepository<data_work_area>();
                    // 获取部门仓储
                    var obj = await areaRepo.FindByIDAsync(data.ID);

                    if (obj == null)
                    {
                        return false;
                    }

                    //data.TopBiaoGao = request.topBiaoGao;
                    //data.BottomBiaoGao = request.bottomBiaoGao;
                    //data.SoilGuanRuLiang = request.soilGuanRuLiang;
                    //data.SoilUseTotal = request.soilUseTotal;
                    //data.SoilBiaoGao = request.soilBiaoGao;

                    obj.AreaNo = data.AreaNo;
                    obj.UserName = data.UserName;
                    obj.TopBiaoGao = data.TopBiaoGao;
                    obj.BottomBiaoGao = data.BottomBiaoGao;
                    obj.SoilGuanRuLiang = data.SoilGuanRuLiang;
                    obj.SoilUseTotal = data.SoilUseTotal;
                    obj.SoilBiaoGao = data.SoilBiaoGao;

                    obj.CreateTime = DateTime.Now;

                    await areaRepo.UpdateAsync(obj);

                    int count = await uow.SaveAsync();

                    return count > 0;
                }

            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(WorkService));
                return false;
            }
        }

        public async Task<bool> WorkAreaDelete(int ID)
        {
            try
            {
                using (var uow = _structHandleDbFactory.Create())
                {
                    // 获取软件仓储
                    var areaRepo = uow.GetRepository<data_work_area>();
                    // 获取部门仓储
                    var data = areaRepo.FindByID(ID);

                    if (data == null)
                    {
                        return false;
                    }

                    data.IsDelete = 1;

                    await areaRepo.UpdateAsync(data);

                    int count = await uow.SaveAsync();

                    return count > 0;
                }

            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(WorkService));
                return false;
            }
        }



        public async Task<WorkAreaResponse> WorkAreaPointData(StructHandleRequest request)
        {
            if (request.ID == 0)
            {
                return null;
            }

            try
            {

                WorkAreaResponse data = new WorkAreaResponse();

                using (var uow = _structHandleDbFactory.Create())
                {
                    // 获取软件仓储
                    var areaRepo = uow.GetRepository<data_work_area>();
                    var areaPointRepo = uow.GetRepository<data_work_area_point>();
                    var workPointRepo = uow.GetRepository<view_data_handle_working_point>();
                    var dataHandleRepo = uow.GetRepository<data_handle>();

                    var area = await areaRepo.FindByIDAsync(request.ID);
                    if (area == null)
                    {
                        return null;
                    }

                    data.AreaNo = area.AreaNo;
                    data.WorkRow = Convert.ToInt32(area.WorkRow);
                    data.WorkColumn = Convert.ToInt32(area.WorkColumn);
                    data.StartX = area.StartX.ToString();
                    data.StartY = area.StartY.ToString();
                    data.EndX = area.EndX.ToString();
                    data.EndY = area.EndY.ToString();
                    data.TopBiaoGao = area.TopBiaoGao.ToString();
                    data.BottomBiaoGao = area.BottomBiaoGao.ToString();
                    data.SoilGuanRuLiang = area.SoilGuanRuLiang.ToString();
                    data.SoilUseTotal = area.SoilUseTotal.ToString();
                    data.SoilBiaoGao = area.SoilBiaoGao.ToString();
                    data.FinishRate = "0%";

                    var points = await areaPointRepo.FindAsync(a => a.IsDelete == 0 && a.AreaID == request.ID);
                    var pointList = points.ToList();
                    foreach (var point in pointList)
                    {
                        data.WorkAreaPoints.Add(new WorkAreaPointResponse()
                        {
                            ID = point.ID,
                            Status = point.Status,
                            PointName = point.PointName ?? "",
                            PointX = point.PointX.ToString(),
                            PointY = point.PointY.ToString(),
                            TopBiaoGao = area.TopBiaoGao.ToString(),
                            BottomBiaoGao = area.BottomBiaoGao.ToString(),
                            SoilBiaoGao = area.SoilBiaoGao.ToString(),
                            SoilGuanRuLiang = area.SoilGuanRuLiang.ToString(),
                            SoilUseTotal = area.SoilUseTotal.ToString(),
                            SoilGuanRuLiangAct = point.SoilGuanRuLiangAct.ToString(),
                            SoilUseTotalAct = point.SoilUseTotalAct.ToString(),
                        });
                    }
                    if (pointList.Count != 0)
                    {
                        data.FinishRate = (pointList.Count(a => a.Status == 2) / pointList.Count).ToString("F2");
                    }

                    data.WorkingPoint = "";
                    workPointRepo.Find(a => a.AreaID == request.ID).ToList().ForEach(a =>
                    {
                        data.WorkingPoint += a.AreaNo + a.PointName + ",";
                    });
                    data.WorkingPoint = data.WorkingPoint.Trim(',');

                    var allWorkingMachines = await dataHandleRepo.FindAsync(a => a.WorkStatus == 1);
                    foreach (var item in allWorkingMachines)
                    {
                        data.WorkingPoints.Add(new WorkingPoint()
                        {
                            ID = item.ID,
                            PointID = item.PointID
                        });
                    }

                }

                return data;


            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(WorkService));
                return null;
            }



        }


        public async Task<bool> WorkAreaPointAdd(StructHandleRequest request, int DataID)
        {
            if (request.startX == null || request.startY == null || request.endX == null || request.startY == null)
            {
                return false;
            }
            if (request.workColumn == null || request.workColumn == 0 || request.workRow == null || request.workRow == 0)
            {
                return false;
            }
            if (DataID == 0)
            {
                return false;
            }

            try
            {
                List<data_work_area_point> list = new List<data_work_area_point>();

                // 计算步长（处理单行/单列的特殊情况，避免除以0）
                double stepX = request.workColumn == 1 ? 0 : Convert.ToDouble((request.endX - request.startX) / (request.workColumn - 1));
                double stepY = request.workRow == 1 ? 0 : Convert.ToDouble((request.endY - request.startY) / (request.workRow - 1));

                DateTime now = DateTime.Now;
                int PointCount = 1;
                // 双重循环生成所有坐标（先遍历行，再遍历列）
                for (int row = 0; row < request.workRow; row++)
                {
                    double currentY = request.startY.Value + row * stepY;
                    for (int col = 0; col < request.workColumn; col++)
                    {
                        double currentX = request.startX.Value + col * stepX;
                        list.Add(new data_work_area_point()
                        {

                            CreateTime = now,
                            IsDelete = 0,
                            AreaID = DataID,
                            PointName = PointCount.ToString("D3"),
                            PointX = currentX,
                            PointY = currentY,
                            Status = 0,
                            SoilGuanRuLiang = request.soilGuanRuLiang,
                            SoilUseTotal = request.soilUseTotal,
                            SoilBiaoGao = request.soilBiaoGao,
                        });
                        PointCount++;
                    }
                }

                using (var uow = _structHandleDbFactory.Create())
                {
                    var areaPointRepo = uow.GetRepository<data_work_area_point>();
                    await areaPointRepo.AddListAsync(list);

                    int count = await uow.SaveAsync();

                    return count > 0;

                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(WorkService));
                return false;
            }



        }


        public async Task<data_work_area_point?> WorkAreaPointData(int ID)
        {
            try
            {
                using (var uow = _structHandleDbFactory.Create())
                {

                    // 获取软件仓储
                    var areaRepo = uow.GetRepository<data_work_area>();
                    var areapointRepo = uow.GetRepository<data_work_area_point>();
                    // 获取部门仓储
                    var data = await areapointRepo.FindByIDAsync(ID);

                    if (data == null)
                    {
                        return null;
                    }

                    var area = await areaRepo.FindAsync(a => a.IsDelete == 0 && a.ID == data.AreaID);

                    var areaData = area.FirstOrDefault();

                    if (areaData == null)
                    {
                        return null;
                    }

                    data.AreaNo = areaData.AreaNo;
                    data.FinishTimeStr = data.FinishTime == null ? "" : data.FinishTime.Value.ToString("yyyy-MM-dd HH:mm");
                    data.StatusStr = "未施工";
                    data.TopBiaoGao = areaData.TopBiaoGao.ToString();
                    data.BottomBiaoGao = areaData.BottomBiaoGao.ToString();
                    if (data.Status == 1)
                    {
                        data.StatusStr = "正在施工";
                    }
                    else if (data.Status == 2)
                    {
                        data.StatusStr = "已完成";
                    }

                    return data;
                }

            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(WorkService));
                return null;
            }
        }

        public async Task<string> WorkAreaPointHandle(List<int> points, List<int> machines)
        {
            try
            {
                using (var uow = _structHandleDbFactory.Create())
                {

                    // 获取软件仓储
                    var handleRepo = uow.GetRepository<data_handle>();
                    var areapointRepo = uow.GetRepository<data_work_area_point>();

                    var handleList = handleRepo.FindAll().ToList();

                    int machineCount = 0;
                    handleList.ForEach(a =>
                    {
                        if (machines.Contains(a.ID))
                        {
                            int workCount = a.WorkCount;
                            a.WorkTime = DateTime.Now;
                            a.WorkStatus = 1;
                            a.WorkCount += 1;
                            a.PointID = points[machineCount];
                            machineCount++;
                        }
                        else
                        {
                            a.WorkStatus = 0;
                        }

                    });



                    //point
                    int pointCount = 0;
                    areapointRepo.Find(a => points.Contains(a.ID)).ToList().ForEach(a =>
                    {
                        a.Status = 1;//正在施工
                        a.MachineID = machines[pointCount];//施工设备
                        pointCount++;
                    });

                    int saveCount = await uow.SaveAsync();

                    if (saveCount > 0)
                    {
                        return "";
                    }
                    else
                    {
                        return "发生错误";
                    }

                }

            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(WorkService));
                return "发生错误";
            }
        }




        public async Task<List<data_work_area_point>> WorkAreaPointList(int AreaID)
        {
            try
            {
                using (var uow = _structHandleDbFactory.Create())
                {

                    // 获取软件仓储
                    var areapointRepo = uow.GetRepository<data_work_area_point>();
                    // 获取部门仓储
                    var data = await areapointRepo.FindAsync(a => a.IsDelete == 0 && a.AreaID == AreaID);

                    var list = data.ToList();

                    return list;
                }

            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(WorkService));
                return null;
            }
        }


        public async Task<bool> WorkAreaPointImport(int AreaID, DataTable dt)
        {
            try
            {
                using (var uow = _structHandleDbFactory.Create())
                {

                    // 获取软件仓储
                    var areapointRepo = uow.GetRepository<data_work_area_point>();
                    // 获取部门仓储
                    var data = await areapointRepo.FindAsync(a => a.IsDelete == 0 && a.AreaID == AreaID);

                    var list = data.ToList();

                    if (list.Count != dt.Rows.Count)
                    {
                        return false;
                    }

                    bool success = true;
                    foreach (DataRow dr in dt.Rows)
                    {
                        int ID = Convert.ToInt32(dr[0]);
                        var point = list.FirstOrDefault(a => a.ID == ID);
                        if (point == null)
                        {
                            success = false;
                            break;
                        }

                        double? pointX = Convert.ToDouble(dr[2]);
                        double? pointY = Convert.ToDouble(dr[3]);
                        double? SoilGuanRuLiang = Convert.ToDouble(dr[4]);
                        double? SoilUseTotal = Convert.ToDouble(dr[5]);
                        double? SoilBiaoGao = Convert.ToDouble(dr[6]);

                        point.PointX = pointX;
                        point.PointY = pointY;
                        point.SoilGuanRuLiang = SoilGuanRuLiang;
                        point.SoilUseTotal = SoilUseTotal;
                        point.SoilBiaoGao = SoilBiaoGao;

                        await areapointRepo.UpdateAsync(point);

                    }

                    success = await areapointRepo.SaveAsync() > 0;


                    if (!success)
                    {
                        return false;
                    }

                    return true;
                }

            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(WorkService));
                return false;
            }
        }



        public async Task<bool> WorkAreaPointImport2(DataTable dt, string name, string username)
        {
            try
            {
                using (var uow = _structHandleDbFactory.Create())
                {

                    // 获取软件仓储
                    var areaRepo = uow.GetRepository<data_work_area>();
                    var areaPointRepo = uow.GetRepository<data_work_area_point>();

                    await areaRepo.AddAsync(new data_work_area()
                    {
                        AreaNo = name,
                        UserName = username,
                        CreateTime = DateTime.Now,
                        IsDelete = 0
                    });
                    await uow.SaveAsync();

                    var latestAreaID = (await areaRepo.FindAllAsync()).OrderByDescending(a => a.ID).FirstOrDefault().ID;


                    List<data_work_area_point> points = new List<data_work_area_point>();
                    bool success = true;
                    //int rowCount = 0;

                    DateTime now = DateTime.Now;

                    //每米用浆量
                    double MeiMiYongJiangLiang = 1.2 * 2.8 * 0.8;
                    foreach (DataRow dr in dt.Rows)
                    {
                        //if (rowCount == 0)
                        //{
                        //    rowCount++;
                        //    continue;
                        //}
                        //    ;

                        string pointName = dr[0].ToString().Trim();

                        data_work_area_point point = new data_work_area_point();
                        point.AreaID = latestAreaID;
                        point.PointName = pointName;
                        point.PointArea = pointName.Substring(0, pointName.Length - 3);
                        point.PointX = string.IsNullOrEmpty(dr[1].ToString()) ? null : Convert.ToDouble(dr[1]);
                        point.PointY = string.IsNullOrEmpty(dr[2].ToString()) ? null : Convert.ToDouble(dr[2]);
                        //设计桩顶标高
                        point.ZhuangDing = string.IsNullOrEmpty(dr[3].ToString()) ? null : Convert.ToDouble(dr[3]);
                        //设计桩底标高
                        point.ZhuangDi = string.IsNullOrEmpty(dr[5].ToString()) ? null : Convert.ToDouble(dr[5]);
                        point.ShaMianBiaoGao = string.IsNullOrEmpty(dr[4].ToString()) ? null : Convert.ToDouble(dr[4]);
                        point.SheJiYongJiangLiang = (point.ShaMianBiaoGao - point.ZhuangDi) * MeiMiYongJiangLiang;
                        point.SheJiZhuangChang = point.ZhuangDing - point.ZhuangDi;
                        point.MeiMiYongJiangLiang = MeiMiYongJiangLiang;
                        //泥面标高=桩顶
                        point.NiJiangBiZhong = string.IsNullOrEmpty(dr[7].ToString()) ? null : Convert.ToDouble(dr[7]);
                        point.ShaCengZhuJiangTiJiBi = string.IsNullOrEmpty(dr[8].ToString()) ? null : Convert.ToDouble(dr[8]);
                        point.Remark = dr[9].ToString();
                        point.CreateTime = now;
                        point.IsDelete = 0;
                        points.Add(point);

                        //rowCount++;


                    }

                    await areaPointRepo.AddListAsync(points);

                    success = await uow.SaveAsync() > 0;


                    if (!success)
                    {
                        return false;
                    }

                    return true;
                }

            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(WorkService));
                return false;
            }
        }


        public async Task<WorkAreaResponse> WorkAreaPointData2(StructHandleRequest request)
        {
            if (request.ID == 0)
            {
                return null;
            }

            try
            {

                WorkAreaResponse data = new WorkAreaResponse();

                using (var uow = _structHandleDbFactory.Create())
                {
                    // 获取软件仓储
                    var areaRepo = uow.GetRepository<data_work_area>();
                    var areaPointRepo = uow.GetRepository<data_work_area_point>();
                    var workPointRepo = uow.GetRepository<view_data_handle_working_point>();
                    var dataHandleRepo = uow.GetRepository<data_handle>();

                    var area = await areaRepo.FindByIDAsync(request.ID);
                    if (area == null)
                    {
                        return null;
                    }

                    data.AreaNo = area.AreaNo;
                    //data.WorkRow = Convert.ToInt32(area.WorkRow);
                    //data.WorkColumn = Convert.ToInt32(area.WorkColumn);
                    //data.StartX = area.StartX.ToString();
                    //data.StartY = area.StartY.ToString();
                    //data.EndX = area.EndX.ToString();
                    //data.EndY = area.EndY.ToString();
                    //data.TopBiaoGao = area.TopBiaoGao.ToString();
                    //data.BottomBiaoGao = area.BottomBiaoGao.ToString();
                    //data.SoilGuanRuLiang = area.SoilGuanRuLiang.ToString();
                    //data.SoilUseTotal = area.SoilUseTotal.ToString();
                    //data.SoilBiaoGao = area.SoilBiaoGao.ToString();
                    data.FinishRate = "0%";

                    // 1. 先查出基础数据（不会报错）
                    var points = await areaPointRepo.FindAsync(a => a.IsDelete == 0 && a.AreaID == request.ID);
                    var pointList = points.ToList();

                    // 2. 如果 areaNo 不是 0，在内存中筛选（安全，支持任何条件）
                    if (!string.Equals(request.areaNo, "0"))
                    {
                        pointList = pointList.Where(a => a.PointArea.Equals(request.areaNo)).ToList();
                    }

                    //分组
                    pointList.GroupBy(a => a.PointArea).ToList().ForEach(a =>
                    {
                        var thisList = pointList.Where(b => b.PointArea.Equals(a.FirstOrDefault().PointArea)).ToList();
                        WorkAreaArea area = new WorkAreaArea();
                        area.AreaName = a.FirstOrDefault().PointArea;
                        thisList.ForEach(b =>
                        {
                            area.Points.Add(new WorkAreaPointResponse()
                            {
                                ID = b.ID,
                                Status = b.Status,
                                PointName = b.PointName ?? "",
                                PointX = b.PointX.ToString(),
                                PointY = b.PointY.ToString(),
                                ZhuangDing = b.ZhuangDing.ToString(),
                                ZhuangDi = b.ZhuangDi.ToString(),
                                ShaMianBiaoGao = b.ShaMianBiaoGao.ToString(),
                                SheJiYongJiangLiang = b.SheJiYongJiangLiang.ToString(),
                                SheJiZhuangChang = b.SheJiZhuangChang.ToString()
                            });
                        });
                        data.WorkAreas.Add(area);
                    });

                    //foreach (var point in pointList)
                    //{
                    //    data.WorkAreaPoints.Add(new WorkAreaPointResponse()
                    //    {
                    //        ID = point.ID,
                    //        Status = point.Status,
                    //        PointName = point.PointName ?? "",
                    //        PointX = point.PointX.ToString(),
                    //        PointY = point.PointY.ToString(),
                    //        TopBiaoGao = area.TopBiaoGao.ToString(),
                    //        BottomBiaoGao = area.BottomBiaoGao.ToString(),
                    //        SoilBiaoGao = area.SoilBiaoGao.ToString(),
                    //        SoilGuanRuLiang = area.SoilGuanRuLiang.ToString(),
                    //        SoilUseTotal = area.SoilUseTotal.ToString(),
                    //        SoilGuanRuLiangAct = point.SoilGuanRuLiangAct.ToString(),
                    //        SoilUseTotalAct = point.SoilUseTotalAct.ToString(),
                    //    });
                    //}
                    if (pointList.Count != 0)
                    {
                        data.FinishRate = (pointList.Count(a => a.Status == 2) / pointList.Count).ToString("F2");
                    }

                    data.WorkingPoint = "";
                    workPointRepo.Find(a => a.AreaID == request.ID).ToList().ForEach(a =>
                    {
                        data.WorkingPoint += a.PointName + ",";
                    });
                    data.WorkingPoint = data.WorkingPoint.Trim(',');

                    var allWorkingMachines = await dataHandleRepo.FindAsync(a => a.WorkStatus == 1);
                    foreach (var item in allWorkingMachines)
                    {
                        data.WorkingPoints.Add(new WorkingPoint()
                        {
                            ID = item.ID,
                            PointID = item.PointID
                        });
                    }

                }

                return data;


            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(WorkService));
                return null;
            }



        }


        /// <summary>
        /// 验证编码序列是否连续、分组数量一致
        /// 规则：152A01 → 前缀152 + 字母A + 序号01
        /// 同一字母序号必须连续，所有字母分组数量必须相同
        /// </summary>
        /// <param name="codeList">编码列表</param>
        /// <returns>错误信息，正常返回空字符串</returns>
        public async Task<string> ValidateCodeSequence(List<string> codeList)
        {
            // 空校验
            if (codeList == null || !codeList.Any())
                return "数据列表不能为空";

            // 用来存放分组：key=前缀+字母(152A)，value=序号列表(01,02...)
            var groupDict = new Dictionary<string, List<int>>();

            foreach (var code in codeList)
            {
                if (string.IsNullOrWhiteSpace(code) || code.Length < 4)
                    return $"编码格式错误：{code}，长度不符合规则";

                try
                {
                    // 最后三位是 字母+两位数字
                    string last3 = code.Substring(code.Length - 3);
                    char letter = last3[0];
                    string numStr = last3.Substring(1);

                    if (!char.IsLetter(letter))
                        return $"编码格式错误：{code}，倒数第三位必须是字母";

                    if (!int.TryParse(numStr, out int num) || numStr.Length != 2)
                        return $"编码格式错误：{code}，最后两位必须是01~99的数字";

                    // 分组 key = 前缀 + 字母（152A）
                    string prefix = code.Substring(0, code.Length - 3);
                    string groupKey = $"{prefix}{letter}";

                    if (!groupDict.ContainsKey(groupKey))
                        groupDict[groupKey] = new List<int>();

                    groupDict[groupKey].Add(num);
                }
                catch
                {
                    return $"编码格式异常：{code}";
                }
            }

            // 检查每个分组内部是否连续、不重复、不错序
            foreach (var group in groupDict)
            {
                var nums = group.Value.OrderBy(x => x).ToList();
                var first = nums.First();
                var last = nums.Last();
                var count = nums.Count;

                // 检查重复
                if (nums.Distinct().Count() != count)
                    return $"分组【{group.Key}】存在重复序号";

                // 检查是否从 1 开始
                if (first != 1)
                    return $"分组【{group.Key}】序号不是从01开始";

                // 检查连续
                for (int i = 0; i < count; i++)
                {
                    int expect = i + 1;
                    if (nums[i] != expect)
                        return $"分组【{group.Key}】序号不连续，缺失：{expect:D2}";
                }
            }

            // 检查所有分组数量是否相同
            var counts = groupDict.Values.Select(v => v.Count).Distinct().ToList();
            if (counts.Count > 1)
            {
                var errorInfo = new StringBuilder();
                errorInfo.Append("各字母分组数量不一致：");
                foreach (var g in groupDict)
                    errorInfo.Append($"{g.Key}({g.Value.Count})个、");

                errorInfo.Length--;
                return errorInfo.ToString();
            }

            // 全部正常
            return "";
        }
    }
}
