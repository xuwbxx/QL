using Microsoft.AspNetCore.Mvc;
using Model.Base;
using Model.Tech.Cloud;
using Service.Base;
using Service.Base.Filter;
using Service.Wind;
using Tool;
using WindCloud.Areas.Cloud.Controllers;

namespace WindCloud.Areas.Mobile.Controllers
{
    [Area("Mobile")]
    public class TaskController : ProjectTaskBaseController
    {
        private readonly MobileService _mobileService;

        public TaskController(
            CookieService cookieService,
            ProjectService projectService,
            TaskService taskService,
            MobileService mobileService)
            : base(cookieService, projectService, taskService)
        {
            _mobileService = mobileService;
        }

        /// <summary>
        /// 手机端任务列表页
        /// </summary>
        [TypeFilter(typeof(WindCloudFilter))]
        public IActionResult Index()
        {
            // 交建通进入
            string CloudID = HttpContext.Request.Query["CloudID"].ToString();
            if (!string.IsNullOrEmpty(CloudID))
            {
                var TaskID = Convert.ToInt32(CloudID);
                var ThisTask = _mobileService.GetTaskByID(TaskID);
                if (ThisTask == null)
                {
                    return RedirectToAction("Error", "BaseMobile", new { ErrorText = "任务数据错误" });
                }

                if (ThisTask.FlowStatus == (int)CloudWindTaskFlowStatus.完成)
                {
                    return Redirect("/Mobile/Task/ReportAction?TaskID=" + TaskID);
                }
                else
                {
                    if (ThisTask.SoftwareID == (int)CloudWindSoftware.自升式平台桩腿插拔计算)
                    {
                        return Redirect("/Mobile/Task/CBZTask?TaskID=" + CloudID);
                    }
                    else if (ThisTask.SoftwareID == (int)CloudWindSoftware.起重船基础施工可作业性预报)
                    {
                        return Redirect("/Mobile/Task/KZYTask?TaskID=" + CloudID);
                    }
                }
            }

            ViewData["ProjectName"] = "任务列表";
            string ProjectID = HttpContext.Request.Query["ProjectID"].ToString();
            if (!string.IsNullOrEmpty(ProjectID))
            {
                ViewData["ProjectID"] = ProjectID;

                int PID = Convert.ToInt32(ProjectID);
                var Project = _mobileService.GetProjectByID(PID);
                if (Project == null)
                {
                    return RedirectToAction("Error", "BaseMobile", new { ErrorText = "任务数据错误" });
                }
                ViewData["ProjectName"] = Project.ProjectName != null && Project.ProjectName.Length >= 16
                    ? Project.ProjectName.Substring(0, 16)
                    : Project.ProjectName;
            }
            else
            {
                ViewData["ProjectID"] = 0;
            }

            ViewData["ProjectID"] = ProjectID ?? "";
            ViewData["UserName"] = CurrentUser?.RealName;
            ViewData["PostToken"] = CreateWebToken(CurrentUser.UserCode);

            return View();
        }

        /// <summary>
        /// 任务列表查询
        /// </summary>
        [TypeFilter(typeof(WindCloudFilter))]
        public async Task<IActionResult> TaskListQuery([FromForm] CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            try
            {
                var list = await _mobileService.GetMobileTaskList(request);

                // 只显示最近100个
                ret.Data = list
                    .OrderByDescending(a => a.FlowStatus == (int)CloudWindTaskFlowStatus.退回)
                    .ThenByDescending(a => a.FlowStatus == (int)CloudWindTaskFlowStatus.审批中)
                    .ThenByDescending(a => a.ApplyTime)
                    .Skip(request.GetSkipCount())
                    .Take(request.PageSize)
                    .ToList();
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
        /// 插拔桩任务详情页
        /// </summary>
        [TypeFilter(typeof(WindCloudFilter))]
        public async Task<IActionResult> CBZTask()
        {
            var ParamID = HttpContext.Request.Query["TaskID"].ToString();
            if (string.IsNullOrEmpty(ParamID))
            {
                return RedirectToAction("Error", "BaseMobile", new { ErrorText = "任务数据错误" });
            }

            int TaskID = Convert.ToInt32(ParamID);

            var Task = _mobileService.GetTaskByID(TaskID);
            if (Task == null)
            {
                return RedirectToAction("Error", "BaseMobile", new { ErrorText = "任务数据错误" });
            }

            var isHasRight = await _mobileService.HasTaskViewRight(TaskID, CurrentUser.UserCode);
            if (!isHasRight)
            {
                return RedirectToAction("Error", "BaseMobile", new { ErrorText = "没有浏览权限" });
            }

            ViewData["TaskID"] = TaskID;
            ViewData["ProjectID"] = Task.ProjectID;
            ViewData["UserName"] = CurrentUser?.RealName;
            ViewData["PostToken"] = CreateWebToken(CurrentUser.UserCode);

            return View();
        }

        /// <summary>
        /// 插拔桩任务数据查询
        /// </summary>
        [TypeFilter(typeof(WindCloudFilter))]
        public IActionResult TaskQuery_CBZ([FromForm] CloudWindRequest request)
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
            if (string.IsNullOrEmpty(CurrentUser?.UserCode))
            {
                ret.Message = "用户信息失效，请重新交建通登录";
                return Ok(ret);
            }
            try
            {
                var data = _mobileService.GetCBZTaskInfo(request);
                ret.Data = data;
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
        /// 可作业性预报任务详情页
        /// </summary>
        [TypeFilter(typeof(WindCloudFilter))]
        public async Task<IActionResult> KZYTask()
        {
            var ParamID = HttpContext.Request.Query["TaskID"].ToString();
            if (string.IsNullOrEmpty(ParamID))
            {
                return RedirectToAction("Error", "BaseMobile", new { ErrorText = "任务数据错误" });
            }

            int TaskID = Convert.ToInt32(ParamID);

            var Task = _mobileService.GetTaskByID(TaskID);
            if (Task == null)
            {
                return RedirectToAction("Error", "BaseMobile", new { ErrorText = "任务数据错误" });
            }

            var isHasRight = await _mobileService.HasTaskViewRight(TaskID, CurrentUser.UserCode);
            if (!isHasRight)
            {
                return RedirectToAction("Error", "BaseMobile", new { ErrorText = "没有浏览权限" });
            }

            var ShipList = _mobileService.GetKZYShipList();

            ViewData["TaskID"] = TaskID;
            ViewData["ProjectID"] = Task.ProjectID;
            ViewData["UserName"] = CurrentUser?.RealName;
            ViewData["PostToken"] = CreateWebToken(CurrentUser.UserCode);

            return View(ShipList);
        }

        /// <summary>
        /// 可作业性预报任务数据查询
        /// </summary>
        [TypeFilter(typeof(WindCloudFilter))]
        public IActionResult TaskQuery_KZY([FromForm] CloudWindRequest request)
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
            if (string.IsNullOrEmpty(CurrentUser?.UserCode))
            {
                ret.Message = "用户信息失效，请重新交建通登录";
                return Ok(ret);
            }
            try
            {
                var data = _mobileService.GetKZYTASKInfoWithFiles(request);
                ret.Data = data;
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
        /// 报告查看页
        /// </summary>
        [TypeFilter(typeof(WindCloudFilter))]
        public async Task<IActionResult> ReportAction()
        {
            var QueryID = HttpContext.Request.Query["TaskID"].ToString();
            if (string.IsNullOrEmpty(QueryID))
            {
                return RedirectToAction("Error", "BaseMobile", new { ErrorText = "任务数据错误" });
            }
            int TaskID = Convert.ToInt32(QueryID);
            var Task = _mobileService.GetTaskByID(TaskID);
            if (Task == null)
            {
                return RedirectToAction("Error", "BaseMobile", new { ErrorText = "任务数据错误" });
            }
            var Project = _mobileService.GetProjectByID(Task.ProjectID);
            if (Project == null)
            {
                return RedirectToAction("Error", "BaseMobile", new { ErrorText = "任务数据错误" });
            }

            var isHasRight = await _mobileService.HasTaskViewRight(TaskID, CurrentUser.UserCode);
            if (!isHasRight)
            {
                return RedirectToAction("Error", "BaseMobile", new { ErrorText = "没有浏览权限" });
            }

            ViewData["SoftwareName"] = Enum.GetName(typeof(CloudWindSoftware), Task.SoftwareID);
            ViewData["ProjectName"] = Project.ProjectName;
            ViewData["UserName"] = CurrentUser?.RealName;
            ViewData["TaskID"] = Task.ID;
            ViewData["TaskName"] = Task.TaskName;
            ViewData["TaskCode"] = Task.TaskCode;
            ViewData["TaskTime"] = Task.CreateTime.HasValue
                ? Task.CreateTime.Value.ToString("yyyy-MM-dd HH:mm")
                : "";

            ViewData["PostToken"] = CreateWebToken(CurrentUser.UserCode);

            return View();
        }
    }
}
