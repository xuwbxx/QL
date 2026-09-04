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
                    .OrderByDescending(a => a.FlowStatus == (int)CloudWindTaskFlowStatus.退回)
                    .ThenByDescending(a => a.FlowStatus == (int)CloudWindTaskFlowStatus.审批中)
                    .ThenByDescending(a => a.ApplyTime)
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
                string url = await _taskService.TaskSubmit_CBZ(request);
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

    }
}
