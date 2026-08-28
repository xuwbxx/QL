using DataFactory.MySql;
using Microsoft.AspNetCore.Mvc;
using Model.Base;
using Model.StructHandle;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Service.Base;
using Service.StructHandle;
using System.Data;
using Tool;

namespace TongShiBaseStructHandleSys.Areas.Work.Controllers
{
    [Area("Work")]
    public class ImproveController : Controller
    {

        private WorkService _workService { get; }

        private CookieService _cookieService { get; }

        public ImproveController(WorkService workService, CookieService cookieService)
        {
            _cookieService = cookieService;
            _workService = workService;
        }



        [TypeFilter(typeof(StructHandleFilter))]
        public IActionResult Index()
        {
            var UserInfo = _cookieService.GetUserCookie();

            ViewData["userName"] = UserInfo.UserName;

            return View();
        }

        [HttpPost]
        public async Task<BaseReturn> WorkAreaList([FromBody] StructHandleRequest request)
        {
            BaseReturn ret = new BaseReturn();

            var data = await _workService.WorkAreaList(request);



            ret.Data = data;
            ret.Success = true;

            return ret;

        }

        [HttpPost]
        public async Task<IActionResult> WorkAreaData([FromBody] StructHandleRequest request)
        {
            BaseReturn ret = new BaseReturn();

            if (request.ID == 0)
            {
                return Json(new { success = false, message = "查询数据发生错误" });
            }


            var data = await _workService.WorkAreaData(request.ID);

            if (data == null)
            {
                ret.Success = false;
            }
            else
            {
                ret.Success = true;
                ret.Data = data;
            }

            return Json(ret);
        }

        [HttpPost]
        public async Task<IActionResult> WorkAreaSave([FromBody] StructHandleRequest request)
        {
            if (string.IsNullOrEmpty(request.areaNo))
            {
                return Json(new { success = false, message = "编号名称不能是空" });
            }


            if (request.workColumn == null || request.workColumn == 0 || request.workRow == null || request.workRow == 0)
            {
                return Json(new { success = false, message = "矩阵数据不可以是空值或者0" });
            }

            if (request.startX == null || request.startY == null || request.endX == null || request.endY == null)
            {
                return Json(new { success = false, message = "坐标数据不能是空" });
            }

            var UserInfo = _cookieService.GetUserCookie();

            data_work_area data = new data_work_area();
            data.ID = request.ID;
            data.AreaNo = request.areaNo ?? "";
            data.WorkColumn = request.workColumn;
            data.WorkRow = request.workRow;
            data.StartX = request.startX;
            data.StartY = request.startY;
            data.EndX = request.endX;
            data.EndY = request.endY;

            data.TopBiaoGao = request.topBiaoGao;
            data.BottomBiaoGao = request.bottomBiaoGao;
            data.SoilGuanRuLiang = request.soilGuanRuLiang;
            data.SoilUseTotal = request.soilUseTotal;
            data.SoilBiaoGao = request.soilBiaoGao;

            data.CreateTime = DateTime.Now;
            data.IsDelete = 0;

            data.UserName = UserInfo.UserName;

            var IsSuccess = false;

            if (request.ID == 0)
            {
                int dataID = await _workService.WorkAreaAdd(data);

                //生成矩阵工作点
                if (dataID > 0)
                {
                    await _workService.WorkAreaPointAdd(request, dataID);
                }
                IsSuccess = true;
            }
            else
            {
                IsSuccess = await _workService.WorkAreaUpdate(data);
            }


            if (IsSuccess)
            {
                return Json(new { success = true, message = "保存成功" });
            }
            else
            {
                return Json(new { success = false, message = "保存失败" });
            }


        }

        [HttpPost]
        public async Task<IActionResult> WorkAreaDelete([FromBody] StructHandleRequest request)
        {
            if (request.ID == 0)
            {
                return Json(new { success = false, message = "删除数据发生错误" });
            }


            var IsSuccess = await _workService.WorkAreaDelete(request.ID);

            if (IsSuccess)
            {
                return Json(new { success = true, message = "删除成功" });
            }
            else
            {
                return Json(new { success = false, message = "删除失败" });
            }


        }



        [TypeFilter(typeof(StructHandleFilter))]
        public IActionResult Work()
        {
            string paramString = HttpContext.Request.Query["areaID"];
            if (string.IsNullOrEmpty(paramString))
            {
                return null;
            }

            int areaID = Convert.ToInt32(paramString);

            return View(areaID);
        }

        [HttpPost]
        public async Task<BaseReturn> WorkAreaPointInfo([FromBody] StructHandleRequest request)
        {
            BaseReturn ret = new BaseReturn();

            WorkAreaResponse area = new WorkAreaResponse();

            var data = await _workService.WorkAreaPointData(request);

            ret.Data = data;
            ret.Success = true;

            return ret;

        }


        [HttpPost]
        public async Task<IActionResult> WorkAreaPointData([FromBody] StructHandleRequest request)
        {
            BaseReturn ret = new BaseReturn();

            if (request.ID == 0)
            {
                return Json(new { success = false, message = "查询数据发生错误" });
            }


            var data = await _workService.WorkAreaPointData(request.ID);

            if (data == null)
            {
                ret.Success = false;
            }
            else
            {
                ret.Success = true;
                ret.Data = data;
            }

            return Json(ret);
        }


        [HttpPost]
        public async Task<IActionResult> WorkAreaPointHandle([FromBody] StructHandleRequest request)
        {
            BaseReturn ret = new BaseReturn();

            if (request.machineList.Count == 0 || request.pointList.Count == 0 || request.machineList.Count != request.pointList.Count)
            {
                ret.Message = "没有选择工作点，或者选择的数量不匹配";
                return Json(ret);
            }

            var msg = await _workService.WorkAreaPointHandle(request.pointList, request.machineList);

            if (string.IsNullOrEmpty(msg))
            {
                ret.Success = true;
            }
            return Json(ret);

        }

        [HttpGet]
        public async Task<IActionResult> WorkAreaPointExport([FromQuery] int AreaID)
        {
            BaseReturn ret = new BaseReturn();

            List<data_work_area_point> list = new List<data_work_area_point>();

            if (AreaID == 0)
            {
                ret.Success = false;
                return null;
            }

            DataSet ds = new DataSet();

            list = await _workService.WorkAreaPointList(AreaID);

            DataTable dt = new DataTable();
            dt.TableName = "工作区域设计数据";
            dt.Columns.Add("编号ID", typeof(String));
            dt.Columns.Add("工作点名称", typeof(String));
            dt.Columns.Add("坐标X", typeof(String));
            dt.Columns.Add("坐标Y", typeof(String));
            dt.Columns.Add("泥贯入量", typeof(String));
            dt.Columns.Add("用泥总量", typeof(String));
            dt.Columns.Add("泥面标高", typeof(String));

            list.ForEach(a =>
            {
                DataRow dr = dt.NewRow();
                dr[0] = a.ID.ToString();
                dr[1] = a.PointName ?? "";
                dr[2] = a.PointX == null ? "" : a.PointX.ToString();
                dr[3] = a.PointY == null ? "" : a.PointY.ToString();
                dr[4] = a.SoilGuanRuLiang == null ? "" : a.SoilGuanRuLiang.ToString();
                dr[5] = a.SoilUseTotal == null ? "" : a.SoilUseTotal.ToString();
                dr[6] = a.SoilBiaoGao == null ? "" : a.SoilBiaoGao.ToString();
                dt.Rows.Add(dr);
            });

            ds.Tables.Add(dt);

            var bytes = ExcelUtils.SetToBytes(ds, true);

            // 7. 校验字节流（防止ExcelUtils返回空）
            if (bytes == null || bytes.Length == 0)
            {
                ret.Success = false;
                ret.Message = "Excel文件生成失败";
                return Json(ret);
            }

            // 8. 构造文件名（简化拼接，避免特殊字符问题）
            string fileName = $"工作区域数据({DateTime.Now:yyyyMMddHHmmss}).xlsx";
            // 关键修正：ContentType适配xlsx格式
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            // 9. 返回文件流（核心：正确的ContentType + 文件名）
            return File(bytes, contentType, fileName);

        }


        [HttpPost]
        public async Task<IActionResult> WorkAreaPointImport(IFormFile file, [FromForm] int AreaID)
        {
            BaseReturn ret = new BaseReturn();
            if (AreaID == 0)
            {
                ret.Success = false;
                ret.Message = "数据错误";
                return Json(ret);
            }
            if (file == null)
            {
                ret.Success = false;
                ret.Message = "没有选择文件";
                return Json(ret);
            }
            try
            {
                // 2. 校验文件格式
                var fileName = file.FileName;
                var fileExt = Path.GetExtension(fileName).ToLower();
                if (fileExt != ".xlsx")
                {
                    ret.Success = false;
                    ret.Message = "仅支持导入Excel格式文件（.xlsx）！";
                    return Json(ret);
                }

                DataSet ds = ExcelUtils.ConvertExcelFileToDataSet(file);
                System.Data.DataTable dt = ds.Tables[0];

                bool success = await _workService.WorkAreaPointImport(AreaID, dt);

                if (!success)
                {
                    ret.Success = false;
                    ret.Message = "请注意文件格式，不能删除任意一行；注意数值格式。";
                    return Json(ret);
                }
                else
                {
                    ret.Success = true;
                    ret.Message = "导入成功";
                    return Json(ret);
                }

            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = $"导入失败：{ex.Message}";
                // 日志记录（可选）
                // _logger.LogError(ex, "工作区域数据导入异常");
            }

            return Json(ret);
        }


        [TypeFilter(typeof(StructHandleFilter))]
        public IActionResult Index2()
        {
            var UserInfo = _cookieService.GetUserCookie();

            ViewData["userName"] = UserInfo.UserName;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> WorkAreaPointImport2(IFormFile file)
        {
            BaseReturn ret = new BaseReturn();
            //if (AreaID == 0)
            //{
            //    ret.Success = false;
            //    ret.Message = "数据错误";
            //    return Json(ret);
            //}
            if (file == null)
            {
                ret.Success = false;
                ret.Message = "没有选择文件";
                return Json(ret);
            }
            try
            {
                // 2. 校验文件格式
                var fileName = file.FileName;
                var fileExt = Path.GetExtension(fileName).ToLower();
                if (fileExt != ".xlsx")
                {
                    ret.Success = false;
                    ret.Message = "仅支持导入xlsx格式文件";
                    return Json(ret);
                }

                string name = fileName.Substring(0, fileName.LastIndexOf('.'));

                DataSet ds = ExcelUtils.ConvertExcelFileToDataSet(file);
                DataTable dt = ds.Tables[0];

                if (dt.Rows.Count <= 1)
                {
                    ret.Success = false;
                    ret.Message = "文件里没有数据";
                    return Json(ret);
                }

                List<string> nameList = new List<string>();
                //int count = 0;
                foreach (DataRow dr in dt.Rows)
                {
                    //if (count == 0)
                    //{
                    //    count++;
                    //    continue;
                    //}

                    nameList.Add(dr[0].ToString().Trim());
                    //count++;
                }
                string res = await _workService.ValidateCodeSequence(nameList);
                if (!string.IsNullOrEmpty(res))
                {
                    ret.Success = false;
                    ret.Message = res;
                    return Json(ret);
                }

                var UserInfo = _cookieService.GetUserCookie();

                bool success = await _workService.WorkAreaPointImport2(dt, name, UserInfo.UserName);

                if (!success)
                {
                    ret.Success = false;
                    ret.Message = "请注意文件格式，不能删除任意一行；注意数值格式。";
                    return Json(ret);
                }
                else
                {
                    ret.Success = true;
                    ret.Message = "导入成功";
                    return Json(ret);
                }

            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = $"导入失败：{ex.Message}";
                // 日志记录（可选）
                // _logger.LogError(ex, "工作区域数据导入异常");
            }

            return Json(ret);
        }


        [HttpGet]
        public async Task<IActionResult> WorkAreaPointExport2([FromQuery] int AreaID)
        {
            BaseReturn ret = new BaseReturn();
            List<data_work_area_point> list = new List<data_work_area_point>();

            // 1. 参数校验
            if (AreaID == 0)
            {
                ret.Success = false;
                ret.Message = "区域ID不能为空";
                return Json(ret);
            }

            // 2. 获取数据（和你原逻辑完全一致）
            list = await _workService.WorkAreaPointList(AreaID);
            if (list == null || list.Count == 0)
            {
                ret.Success = false;
                ret.Message = "该区域暂无数据可导出";
                return Json(ret);
            }

            // 3. 获取区域信息（补全你原代码里的 area.AreaNo，避免报错）
            var area = await _workService.WorkAreaData(AreaID);
            if (area == null)
            {
                ret.Success = false;
                ret.Message = "区域信息不存在";
                return Json(ret);
            }

            // 4. EPPlus 4.5 兼容Excel导出（核心逻辑，边框已完全修复）
            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("工作区域设计数据");

                // 4.1 大标题
                ws.Cells["A1:Q1"].Merge = true;
                ws.Cells["A1"].Value = "双轮铣改良处理地基综合施工记录表";
                ws.Cells["A1"].Style.Font.Size = 16;
                ws.Cells["A1"].Style.Font.Bold = true;
                ws.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // 4.2 项目信息行
                ws.Cells["A3:D3"].Merge = true;
                ws.Cells["A3"].Value = "工程部位：" + area.AreaNo;
                ws.Cells["E3:G3"].Merge = true;
                ws.Cells["E3"].Value = "施工船舶：砂桩3号";
                ws.Cells["I3:K3"].Merge = true;
                ws.Cells["I3"].Value = "单次处理面积：2.8m*1.2m=3.36㎡";
                ws.Cells["M3:Q3"].Merge = true;
                ws.Cells["M3"].Value = "砂层每延米深度理论浆用量：2.7m³";

                // 项目信息行边框
                using (var infoRange = ws.Cells["A3:Q3"])
                {
                    infoRange.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    infoRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    infoRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                // 4.3 表头行
                int headerRow = 4;
                int headerEndRow = headerRow + 1;

                ws.Cells[$"A{headerRow}:A{headerEndRow}"].Merge = true; ws.Cells[$"A{headerRow}"].Value = "序号";
                ws.Cells[$"B{headerRow}:B{headerEndRow}"].Merge = true; ws.Cells[$"B{headerRow}"].Value = "桩号";
                ws.Cells[$"C{headerRow}:C{headerEndRow}"].Merge = true; ws.Cells[$"C{headerRow}"].Value = "施工日期";

                ws.Cells[$"D{headerRow}:H{headerRow}"].Merge = true; ws.Cells[$"D{headerRow}"].Value = "设计参数";
                ws.Cells[$"D{headerEndRow}"].Value = "泥面标高(m)";
                ws.Cells[$"E{headerEndRow}"].Value = "处理底标高(m)";
                ws.Cells[$"F{headerEndRow}"].Value = "砂面标高(m)";
                ws.Cells[$"G{headerEndRow}"].Value = "泥浆比重";
                ws.Cells[$"H{headerEndRow}"].Value = "砂层注浆体积比";

                ws.Cells[$"I{headerRow}:P{headerRow}"].Merge = true; ws.Cells[$"I{headerRow}"].Value = "实际施工数据";
                ws.Cells[$"I{headerEndRow}"].Value = "设备编号（双轮铣）";
                ws.Cells[$"J{headerEndRow}"].Value = "泥面标高(m)";
                ws.Cells[$"K{headerEndRow}"].Value = "处理底标高(m)";
                ws.Cells[$"L{headerEndRow}"].Value = "砂面标高(m)";
                ws.Cells[$"M{headerEndRow}"].Value = "泥浆比重（g/cm³）";
                ws.Cells[$"N{headerEndRow}"].Value = "泥浆用量（m3）";
                ws.Cells[$"O{headerEndRow}"].Value = "开始时间";
                ws.Cells[$"P{headerEndRow}"].Value = "结束时间";

                ws.Cells[$"Q{headerRow}:Q{headerEndRow}"].Merge = true; ws.Cells[$"Q{headerRow}"].Value = "备注";

                // 表头边框（4.5兼容写法）
                using (var headRange = ws.Cells[$"A{headerRow}:Q{headerEndRow}"])
                {
                    headRange.Style.Font.Bold = true;
                    headRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    headRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    headRange.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                // 表头每个单元格单独加内部边框
                for (int col = 1; col <= 17; col++)
                {
                    for (int r = headerRow; r <= headerEndRow; r++)
                    {
                        var cell = ws.Cells[r, col];
                        cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    }
                }

                // 4.4 数据填充+边框
                int dataStartRow = headerEndRow + 1;
                int row = dataStartRow;
                int count = 1;

                foreach (var a in list)
                {
                    // 赋值（和你原逻辑完全一致）
                    ws.Cells[row, 1].Value = count.ToString();
                    ws.Cells[row, 2].Value = a.PointName?.ToString() ?? "";
                    ws.Cells[row, 3].Value = a.FinishTime == null ? "" : a.FinishTime.Value.ToString("yyyyMMdd");
                    ws.Cells[row, 4].Value = a.ZhuangDing?.ToString() ?? "";
                    ws.Cells[row, 5].Value = a.ZhuangDi?.ToString() ?? "";
                    ws.Cells[row, 6].Value = a.ShaMianBiaoGao?.ToString() ?? "";
                    ws.Cells[row, 7].Value = a.NiJiangBiZhong?.ToString() ?? "";
                    ws.Cells[row, 8].Value = a.ShaCengZhuJiangTiJiBi?.ToString() ?? "";
                    ws.Cells[row, 9].Value = a.MachineID?.ToString() ?? "";
                    ws.Cells[row, 10].Value = a.ZhuangDing?.ToString() ?? "";
                    ws.Cells[row, 11].Value = a.BottomBiaoGaoAct?.ToString() ?? "";
                    ws.Cells[row, 12].Value = a.ShaMianBiaoGao?.ToString() ?? "";
                    ws.Cells[row, 13].Value = a.NiJiangBiZhongAct?.ToString() ?? "";
                    ws.Cells[row, 14].Value = a.SoilUseTotalAct?.ToString() ?? "";
                    ws.Cells[row, 15].Value = a.StartTime == null ? "" : a.StartTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                    ws.Cells[row, 16].Value = a.FinishTime == null ? "" : a.FinishTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                    ws.Cells[row, 17].Value = a.Remark ?? "";

                    // 数据行每个单元格加完整边框（4.5兼容）
                    for (int col = 1; col <= 17; col++)
                    {
                        var cell = ws.Cells[row, col];
                        cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    }

                    row++;
                    count++;
                }

                // 4.5 列宽优化
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                ws.Column(17).Width = 15; // 备注列最小宽度

                // 4.6 输出文件
                var bytes = package.GetAsByteArray();
                string fileName = $"工作区域数据({DateTime.Now:yyyyMMddHHmmss}).xlsx";
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(bytes, contentType, fileName);
            }
        }



        [TypeFilter(typeof(StructHandleFilter))]
        public IActionResult Work2()
        {
            string paramString = HttpContext.Request.Query["areaID"];
            if (string.IsNullOrEmpty(paramString))
            {
                return null;
            }

            int areaID = Convert.ToInt32(paramString);

            return View(areaID);
        }



        [HttpPost]
        public async Task<BaseReturn> WorkAreaPointInfo2([FromBody] StructHandleRequest request)
        {
            BaseReturn ret = new BaseReturn();

            WorkAreaResponse area = new WorkAreaResponse();

            var data = await _workService.WorkAreaPointData2(request);

            ret.Data = data;
            ret.Success = true;

            return ret;

        }
    }
}
