using Microsoft.AspNetCore.Mvc;
using Model.Base;
using Model.Tech.Cloud;
using Service.Base;
using Service.Wind;
using Tool;
using WindCloud.Areas.Base.Controllers;

namespace WindCloud.Areas.Cloud.Controllers
{
    [Area("Cloud")]
    public class TaskController : ProjectTaskBaseController
    {
        public TaskController(CookieService cookieService, ProjectService projectService, TaskService taskService)
            : base(cookieService, projectService, taskService)
        {
        }

        #region 公共类

        public async Task<IActionResult> Index()
        {
            if (CurrentUser == null)
            {
                return RedirectToAction("ErrorPage", "CloudBase", new { ErrorText = "身份信息失效" });
            }

            ViewData["PostToken"] = CreateWebToken(CurrentUser.UserCode);

            var softwares = _taskService.GetSoftwares();
            ViewData["Softwares"] = softwares;

            ViewData["ProjectID"] = 0;
            ViewData["ProjectName"] = "已分配任务列表";
            ViewData["TaskID"] = 0;
            ViewData["SoftwareID"] = 0;
            ViewData["OpenType"] = "";

            // 由项目列表进入
            var pid = HttpContext.Request.Query["ProjectID"].ToString();
            if (!string.IsNullOrEmpty(pid))
            {
                if (!int.TryParse(pid, out int projectId))
                {
                    return RedirectToAction("ErrorPage", "CloudBase", new { ErrorText = "项目数据错误" });
                }

                var thisProject = _taskService.GetProjectById(projectId);
                if (thisProject == null)
                {
                    return RedirectToAction("ErrorPage", "CloudBase", new { ErrorText = "项目数据错误" });
                }

                ViewData["ProjectID"] = projectId;
                ViewData["ProjectName"] = thisProject.ProjectName;

                return View();
            }

            // 交建通消息进入
            var cloudId = HttpContext.Request.Query["CloudID"].ToString();
            if (!string.IsNullOrEmpty(cloudId))
            {
                if (!int.TryParse(cloudId, out int taskId))
                {
                    return RedirectToAction("ErrorPage", "CloudBase", new { ErrorText = "任务数据错误" });
                }

                var thisTask = _taskService.GetTaskById(taskId);
                if (thisTask == null)
                {
                    return RedirectToAction("ErrorPage", "CloudBase", new { ErrorText = "任务数据错误" });
                }

                var isHasRight = await _taskService.HasTaskViewRight(taskId, CurrentUser.UserCode);
                if (!isHasRight)
                {
                    return RedirectToAction("ErrorPage", "CloudBase", new { ErrorText = "没有权限浏览" });
                }

                string openType = "";
                if (thisTask.FlowStatus == (int)CloudWindTaskFlowStatus.完成)
                {
                    // 打开报告弹窗
                    openType = "ReportPopup";
                }
                else
                {
                    // 打开审批弹窗
                    openType = "ApprovalPopup";
                }

                ViewData["TaskID"] = taskId;
                ViewData["SoftwareID"] = thisTask.SoftwareID;
                ViewData["OpenType"] = openType;

                return View();
            }

            return View();
        }

        /// <summary>
        /// 待审批，并且是自己的任务
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GetTaskCount(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }

            try
            {
                string msg = "";
                int count = await _taskService.GetTaskCount(CurrentUser.UserCode);

                ret.Data = count;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                LoggerUtils.Error(ex.ToString(), typeof(TaskController));
            }

            return Ok(ret);
        }

        /// <summary>
        /// 任务列表查询
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> TaskListQuery(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            try
            {
                var list = await _taskService.TaskListQuery(request);

                // 分页
                int count = list.Count;
                ret.TotalCount = count;
                ret.TotalPage = count % request.PageSize == 0 ? (count / request.PageSize) : (count / request.PageSize) + 1;

                var currentQuery = list
                    .OrderByDescending(a => a.ApplyTime)
                    .ThenByDescending(a => a.FlowStatus == (int)CloudWindTaskFlowStatus.退回)
                    .ThenByDescending(a => a.FlowStatus == (int)CloudWindTaskFlowStatus.审批中)
                    .Skip(request.GetSkipCount())
                    .Take(request.PageSize)
                    .ToList();

                // 如果不是第一页且没有数据，则查询第一页
                if (request.PageIndex != 1 && currentQuery.Count == 0)
                {
                    request.PageIndex = 1;
                    currentQuery = list
                        .OrderByDescending(a => a.FlowStatus == (int)CloudWindTaskFlowStatus.退回)
                        .ThenByDescending(a => a.FlowStatus == (int)CloudWindTaskFlowStatus.审批中)
                        .ThenByDescending(a => a.ApplyTime)
                        .Skip(request.GetSkipCount())
                        .Take(request.PageSize)
                        .ToList();
                }

                ret.Data = currentQuery;
                ret.Success = true;
                ret.PageIndex = request.PageIndex;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                LoggerUtils.Error(ex.ToString(), typeof(TaskController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 弹出页面选择项目
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ChooseProject(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            if (request.ProjectID == 0)
            {
                ret.Message = "请选择风场项目";
                return Ok(ret);
            }
            try
            {
                var list = await _taskService.ChooseProject(request);
                ret.Data = list;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                LoggerUtils.Error(ex.ToString(), typeof(TaskController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 查询流程历史
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> QueryFlowHistory(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            if (request.TaskID == 0)
            {
                ret.Message = "项目数据错误";
                return Ok(ret);
            }
            try
            {
                var list = await _taskService.QueryFlowHistory(request);
                ret.Data = list;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                LoggerUtils.Error(ex.ToString(), typeof(TaskController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 加载项目文件
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadFile(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            if (request.ProjectID == 0)
            {
                return Ok(ret);
            }
            try
            {
                var file = await _taskService.LoadFile(request);
                ret.Data = file;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                LoggerUtils.Error(ex.ToString(), typeof(TaskController));
            }
            return Ok(ret);
        }

        #endregion


        #region 插拔桩计算

        /// <summary>
        /// 任务提交（插拔桩）
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> TaskSubmit_CBZ(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (request.ProjectID == 0)
            {
                ret.Message = "需要选择关联的风场项目";
                return Ok(ret);
            }
            if (request.TaskID == 0)
            {
                ret.Message = "任务数据错误";
                return Ok(ret);
            }
            if (request.SoftwareID == 0)
            {
                ret.Message = "需要选择申请软件";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(request.TaskName))
            {
                ret.Message = "任务名不能为空";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(request.DKIDstring))
            {
                ret.Message = "没有选择地勘数据";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(request.ShipIDstring))
            {
                ret.Message = "没有选择平台船";
                return Ok(ret);
            }
            if (CurrentUser == null || string.IsNullOrEmpty(CurrentUser.UserCode))
            {
                ret.Message = "用户信息失效，请从交建通重新登录云平台";
                return Ok(ret);
            }
            try
            {
                var (url, msg) = await _taskService.TaskSubmit_CBZ(request);
                if (!string.IsNullOrEmpty(msg))
                {
                    ret.Success = false;
                    ret.Message = msg;
                }
                else
                {
                    ret.Success = true;
                    ret.Data = url;
                }
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = "发生错误";
                LoggerUtils.Error(ex.ToString(), typeof(TaskController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 任务查询（插拔桩）
        /// </summary>
        [HttpPost]
        public IActionResult TaskQuery_CBZ(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            if (request.TaskID == 0)
            {
                ret.Message = "任务数据错误";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(CurrentUser.UserCode))
            {
                ret.Message = "用户信息失效，请重新交建通登录";
                return Ok(ret);
            }
            try
            {
                CloudWindTaskFlow data = _taskService.TaskQuery_CBZ(request);
                ret.Data = data;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(TaskController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 审批提交（插拔桩）
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> TaskApprovalSubmit_CBZ(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (request.TaskID == 0)
            {
                ret.Message = "项目信息出错";
                return Ok(ret);
            }
            if (request.ApprovalType != 0 && request.ApprovalType != 2)
            {
                ret.Message = "流程错误";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(CurrentUser.UserCode))
            {
                ret.Message = "用户信息失效，请从交建通重新登录云平台";
                return Ok(ret);
            }
            try
            {
                string url = await _taskService.TaskApprovalSubmit_CBZ(request);
                ret.Success = true;
                ret.Data = url;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(TaskController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 退回上一节点（插拔桩）
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> TaskBackLastSubmit_CBZ(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (request.TaskID == 0)
            {
                ret.Message = "项目信息出错";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(CurrentUser.UserCode))
            {
                ret.Message = "用户信息失效，请从交建通重新登录云平台";
                return Ok(ret);
            }
            try
            {
                await _taskService.TaskBackLastSubmit_CBZ(request);
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(TaskController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 作废任务（插拔桩）
        /// </summary>
        [HttpPost]
        public IActionResult TaskCancelSubmit_CBZ(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (request.TaskID == 0)
            {
                ret.Message = "任务信息出错";
                return Ok(ret);
            }
            try
            {
                _taskService.CancelTask(request.TaskID);
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(TaskController));
            }
            return Ok(ret);
        }

        #endregion



        #region 可作业预报

        /// <summary>
        /// 任务提交（可作业预报）
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> TaskSubmit_KZY(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (request.ProjectID == 0)
            {
                ret.Message = "需要选择关联的风场项目";
                return Ok(ret);
            }
            if (request.SoftwareID == 0)
            {
                ret.Message = "需要选择申请软件";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(request.TaskName))
            {
                ret.Message = "任务名不能为空";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(request.KZYForecastStartTime) || string.IsNullOrEmpty(request.KZYForecastEndTime))
            {
                ret.Message = "需要输入预报时间";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(request.ShipIDstring))
            {
                ret.Message = "没有选择平台船";
                return Ok(ret);
            }
            if (CurrentUser == null || string.IsNullOrEmpty(CurrentUser.UserCode))
            {
                ret.Message = "用户信息失效，请从交建通重新登录云平台";
                return Ok(ret);
            }
            try
            {
                await _taskService.TaskSubmit_KZY(request);
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(TaskController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 任务查询（可作业预报）
        /// </summary>
        [HttpPost]
        public IActionResult TaskQuery_KZY(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            if (request.TaskID == 0)
            {
                ret.Message = "任务数据错误";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(CurrentUser.UserCode))
            {
                ret.Message = "用户信息失效，请重新交建通登录";
                return Ok(ret);
            }
            try
            {
                CloudWindTaskFlow data = _taskService.TaskQuery_KZY(request);
                ret.Data = data;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(TaskController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 审批提交（可作业预报）
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> TaskApprovalSubmit_KZY(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (request.TaskID == 0)
            {
                ret.Message = "项目信息出错";
                return Ok(ret);
            }
            if (request.ApprovalType != 0 && request.ApprovalType != 2)
            {
                ret.Message = "流程错误";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(CurrentUser.UserCode))
            {
                ret.Message = "用户信息失效，请从交建通重新登录云平台";
                return Ok(ret);
            }
            try
            {
                await _taskService.TaskApprovalSubmit_KZY(request);
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(TaskController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 作废任务（可作业预报）
        /// </summary>
        [HttpPost]
        public IActionResult TaskCancelSubmit_KZY(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (request.TaskID == 0)
            {
                ret.Message = "任务信息出错";
                return Ok(ret);
            }
            try
            {
                _taskService.CancelTask(request.TaskID);
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(TaskController));
            }
            return Ok(ret);
        }

        #endregion


        #region 桩基沉桩

        /// <summary>
        /// 项目位置图片上传（桩基沉桩）
        /// </summary>
        [HttpPost]
        public IActionResult ProjectPositionFileUpload(CloudWindRequest request, List<IFormFile> files)
        {
            BaseReturn ret = new BaseReturn();
            if (request.TaskID == 0)
            {
                ret.Message = "任务数据错误";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(request.FlowHandle))
            {
                ret.Message = "任务数据错误";
                return Ok(ret);
            }
            if (files == null || files.Count == 0)
            {
                ret.Message = "没有选择任何文件";
                return Ok(ret);
            }
            try
            {
                List<CloudProjectFile> list = _taskService.TaskPilePositionFileUpload(request, files);
                ret.Data = list;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(TaskController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 桩数据上传（桩基沉桩）
        /// </summary>
        [HttpPost]
        public IActionResult TaskPileFileUpload(CloudWindRequest request, List<IFormFile> files)
        {
            BaseReturn ret = new BaseReturn();
            if (request.FileType == 0)
            {
                ret.Message = "项目数据错误";
                return Ok(ret);
            }
            if (request.TaskID == 0)
            {
                ret.Message = "项目数据错误";
                return Ok(ret);
            }
            if (files == null || files.Count == 0)
            {
                ret.Message = "没有选择任何文件";
                return Ok(ret);
            }
            try
            {
                List<CloudProjectFile> list = _taskService.TaskPileFileUpload(request, files);
                ret.Data = list;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(TaskController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 删除项目位置图片（桩基沉桩）
        /// </summary>
        [HttpPost]
        public IActionResult DeleteProjectPositionFile(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            if (request.ID == 0)
            {
                ret.Message = "数据错误";
                return Ok(ret);
            }
            try
            {
                List<CloudProjectFile> files = _taskService.DeleteProjectPositionFile(request);
                ret.Data = files;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(TaskController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 删除桩文件（桩基沉桩）
        /// </summary>
        [HttpPost]
        public IActionResult DeletePileFile(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            if (request.ID == 0)
            {
                ret.Message = "数据错误";
                return Ok(ret);
            }
            try
            {
                List<CloudProjectFile> files = _taskService.DeletePileFile(request);
                ret.Data = files;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(TaskController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 任务提交（桩基沉桩）
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> TaskSubmit_ZJCZ(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (request.ProjectID == 0)
            {
                ret.Message = "需要选择关联的风场项目";
                return Ok(ret);
            }
            if (request.TaskID == 0)
            {
                ret.Message = "任务数据错误";
                return Ok(ret);
            }
            if (request.SoftwareID == 0)
            {
                ret.Message = "需要选择申请软件";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(request.TaskName))
            {
                ret.Message = "任务名不能为空";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(request.ZJCZProjectIntroduce))
            {
                ret.Message = "项目介绍不能为空";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(request.HammerModel))
            {
                ret.Message = "锤型号不能为空";
                return Ok(ret);
            }
            if (CurrentUser == null || string.IsNullOrEmpty(CurrentUser.UserCode))
            {
                ret.Message = "用户信息失效，请从交建通重新登录云平台";
                return Ok(ret);
            }
            try
            {
                await _taskService.TaskSubmit_ZJCZ(request);
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(TaskController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 任务查询（桩基沉桩）
        /// </summary>
        [HttpPost]
        public IActionResult TaskQuery_ZJCZ(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            if (request.TaskID == 0)
            {
                ret.Message = "任务数据错误";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(CurrentUser.UserCode))
            {
                ret.Message = "用户信息失效，请重新交建通登录";
                return Ok(ret);
            }
            try
            {
                CloudWindTaskFlow data = _taskService.TaskQuery_ZJCZ(request);
                ret.Data = data;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(TaskController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 作废任务（桩基沉桩）
        /// </summary>
        [HttpPost]
        public IActionResult TaskCancelSubmit_ZJCZ(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (request.TaskID == 0)
            {
                ret.Message = "任务信息出错";
                return Ok(ret);
            }
            try
            {
                _taskService.CancelTask(request.TaskID);
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(TaskController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 审批提交（桩基沉桩）
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> TaskApprovalSubmit_ZJCZ(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (request.TaskID == 0)
            {
                ret.Message = "项目信息出错";
                return Ok(ret);
            }
            if (request.ApprovalType != 0 && request.ApprovalType != 2)
            {
                ret.Message = "流程错误";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(CurrentUser.UserCode))
            {
                ret.Message = "用户信息失效，请从交建通重新登录云平台";
                return Ok(ret);
            }
            try
            {
                await _taskService.TaskApprovalSubmit_ZJCZ(request);
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(TaskController));
            }
            return Ok(ret);
        }

        #endregion


        #region 拖航可行性分析

        /// <summary>
        /// 任务提交（拖航可行性分析）
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> TaskSubmit_THFX(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (request.ProjectID == 0)
            {
                ret.Message = "需要选择关联的风场项目";
                return Ok(ret);
            }
            if (request.SoftwareID == 0)
            {
                ret.Message = "需要选择申请软件";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(request.TaskName))
            {
                ret.Message = "任务名不能为空";
                return Ok(ret);
            }
            if (CurrentUser == null || string.IsNullOrEmpty(CurrentUser.UserCode))
            {
                ret.Message = "用户信息失效，请从交建通重新登录云平台";
                return Ok(ret);
            }
            try
            {
                await _taskService.TaskSubmit_THFX(request);
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(TaskController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 任务查询（拖航可行性分析）
        /// </summary>
        [HttpPost]
        public IActionResult TaskQuery_THFX(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            if (request.TaskID == 0)
            {
                ret.Message = "任务数据错误";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(CurrentUser.UserCode))
            {
                ret.Message = "用户信息失效，请重新交建通登录";
                return Ok(ret);
            }
            try
            {
                CloudWindTaskFlow data = _taskService.TaskQuery_THFX(request);
                ret.Data = data;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(TaskController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 审批提交（拖航可行性分析）
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> TaskApprovalSubmit_THFX(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (request.TaskID == 0)
            {
                ret.Message = "项目信息出错";
                return Ok(ret);
            }
            if (request.ApprovalType != 0 && request.ApprovalType != 2)
            {
                ret.Message = "流程错误";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(CurrentUser.UserCode))
            {
                ret.Message = "用户信息失效，请从交建通重新登录云平台";
                return Ok(ret);
            }
            try
            {
                await _taskService.TaskApprovalSubmit_THFX(request);
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(TaskController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 作废任务（拖航可行性分析）
        /// </summary>
        [HttpPost]
        public IActionResult TaskCancelSubmit_THFX(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (request.TaskID == 0)
            {
                ret.Message = "任务信息出错";
                return Ok(ret);
            }
            try
            {
                _taskService.CancelTask(request.TaskID);
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(TaskController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 拖航航线上传
        /// </summary>
        [HttpPost]
        public IActionResult TowingPlanRouteUpload(CloudWindRequest request, IFormFile file)
        {
            BaseReturn ret = new BaseReturn();
            if (request.ProjectID == 0)
            {
                ret.Message = "项目数据错误";
                return Ok(ret);
            }
            if (file == null)
            {
                ret.Message = "没有选择任何文件";
                return Ok(ret);
            }
            try
            {
                List<TowingPlanRoute> list = _taskService.TowingPlanRouteUpload(request, file);
                ret.Data = list;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(TaskController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 拖航文件上传
        /// </summary>
        [HttpPost]
        public IActionResult TowingFileUpload(CloudWindRequest request, List<IFormFile> files)
        {
            BaseReturn ret = new BaseReturn();
            if (request.ProjectID == 0)
            {
                ret.Message = "项目数据错误";
                return Ok(ret);
            }
            if (files == null || files.Count == 0)
            {
                ret.Message = "没有选择任何文件";
                return Ok(ret);
            }
            try
            {
                List<CloudProjectFile> list = _taskService.TowingFileUpload(request, files);
                ret.Data = list;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(TaskController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 删除拖航文件
        /// </summary>
        [HttpPost]
        public IActionResult TowingFileDelete(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            if (request.ID == 0)
            {
                ret.Message = "数据错误";
                return Ok(ret);
            }
            try
            {
                List<CloudProjectFile> files = _taskService.TowingFileDelete(request);
                ret.Data = files;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(TaskController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 加载拖航航线
        /// </summary>
        [HttpPost]
        public IActionResult LoadPlanRoute(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            if (request.ProjectID == 0)
            {
                ret.Message = "数据错误";
                return Ok(ret);
            }
            try
            {
                List<TowingPlanRoute> list = _taskService.LoadPlanRoute(request);
                ret.Data = list;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(TaskController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 加载拖航文件
        /// </summary>
        [HttpPost]
        public IActionResult LoadTowingFile(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            if (request.ProjectID == 0)
            {
                ret.Message = "数据错误";
                return Ok(ret);
            }
            try
            {
                List<CloudProjectFile> files = _taskService.LoadTowingFile(request);
                ret.Data = files;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(TaskController));
            }
            return Ok(ret);
        }

        #endregion


        #region 选船分析

        /// <summary>
        /// 任务提交（选船分析）
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> TaskSubmit_XCFX(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (request.ProjectID == 0)
            {
                ret.Message = "需要选择关联的风场项目";
                return Ok(ret);
            }
            if (request.SoftwareID == 0)
            {
                ret.Message = "需要选择申请软件";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(request.TaskName))
            {
                ret.Message = "任务名不能为空";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(request.ShipIDstring))
            {
                ret.Message = "没有选择平台船";
                return Ok(ret);
            }
            if (CurrentUser == null || string.IsNullOrEmpty(CurrentUser.UserCode))
            {
                ret.Message = "用户信息失效，请从交建通重新登录云平台";
                return Ok(ret);
            }
            try
            {
                await _taskService.TaskSubmit_XCFX(request);
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(TaskController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 任务查询（选船分析）
        /// </summary>
        [HttpPost]
        public IActionResult TaskQuery_XCFX(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            if (request.TaskID == 0)
            {
                ret.Message = "任务数据错误";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(CurrentUser.UserCode))
            {
                ret.Message = "用户信息失效，请重新交建通登录";
                return Ok(ret);
            }
            try
            {
                CloudWindTaskFlow data = _taskService.TaskQuery_XCFX(request);
                ret.Data = data;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(TaskController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 审批提交（选船分析）
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> TaskApprovalSubmit_XCFX(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (request.TaskID == 0)
            {
                ret.Message = "项目信息出错";
                return Ok(ret);
            }
            if (request.ApprovalType != 0 && request.ApprovalType != 2)
            {
                ret.Message = "流程错误";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(CurrentUser.UserCode))
            {
                ret.Message = "用户信息失效，请从交建通重新登录云平台";
                return Ok(ret);
            }
            try
            {
                await _taskService.TaskApprovalSubmit_XCFX(request);
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(TaskController));
            }
            return Ok(ret);
        }

        #endregion


        #region 气象收集

        /// <summary>
        /// 任务提交（气象收集）
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> TaskSubmit_QXSJ(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (request.ProjectID == 0)
            {
                ret.Message = "需要选择关联的风场项目";
                return Ok(ret);
            }
            if (request.SoftwareID == 0)
            {
                ret.Message = "需要选择申请软件";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(request.TaskName))
            {
                ret.Message = "任务名不能为空";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(request.QXSJ_StartTime) || string.IsNullOrEmpty(request.QXSJ_EndTime))
            {
                ret.Message = "需要输入采集时间";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(request.QXSJ_Points))
            {
                ret.Message = "没有数据采集点";
                return Ok(ret);
            }
            if (CurrentUser == null || string.IsNullOrEmpty(CurrentUser.UserCode))
            {
                ret.Message = "用户信息失效，请从交建通重新登录云平台";
                return Ok(ret);
            }
            try
            {
                await _taskService.TaskSubmit_QXSJ(request);
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(TaskController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 任务查询（气象收集）
        /// </summary>
        [HttpPost]
        public IActionResult TaskQuery_QXSJ(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            if (request.TaskID == 0)
            {
                ret.Message = "任务数据错误";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(CurrentUser.UserCode))
            {
                ret.Message = "用户信息失效，请重新交建通登录";
                return Ok(ret);
            }
            try
            {
                CloudWindTaskFlow data = _taskService.TaskQuery_QXSJ(request);
                ret.Data = data;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(TaskController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 审批提交（气象收集）
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> TaskApprovalSubmit_QXSJ(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (request.TaskID == 0)
            {
                ret.Message = "项目信息出错";
                return Ok(ret);
            }
            if (request.ApprovalType != 0 && request.ApprovalType != 2)
            {
                ret.Message = "流程错误";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(CurrentUser.UserCode))
            {
                ret.Message = "用户信息失效，请从交建通重新登录云平台";
                return Ok(ret);
            }
            try
            {
                await _taskService.TaskApprovalSubmit_QXSJ(request);
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(TaskController));
            }
            return Ok(ret);
        }

        #endregion

    }
}