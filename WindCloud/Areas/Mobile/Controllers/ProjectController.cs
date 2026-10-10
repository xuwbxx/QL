using Microsoft.AspNetCore.Mvc;
using Model.Base;
using Model.Tech.Cloud;
using Service.Base;
using Service.Base.Filter;
using Service.Wind;
using System.Web;
using Tool;
using WindCloud.Areas.Cloud.Controllers;

namespace WindCloud.Areas.Mobile.Controllers
{
    [Area("Mobile")]
    public class ProjectController : ProjectTaskBaseController
    {
        private readonly MobileService _mobileService;

        public ProjectController(
            CookieService cookieService,
            ProjectService projectService,
            TaskService taskService,
            MobileService mobileService)
            : base(cookieService, projectService, taskService)
        {
            _mobileService = mobileService;
        }

        /// <summary>
        /// SSO单点登录跳转
        /// </summary>
        public IActionResult Redirect()
        {
            try
            {
                string paramString = HttpContext.Request.Query["paramString"];
                LoggerUtils.Info(paramString, typeof(ProjectController));
                string token = HttpUtility.UrlDecode(paramString);

                // 解密token并获取用户信息
                var (userinfo, errorMsg) = _mobileService.ProcessSSOLogin(token);
                if (userinfo == null)
                {
                    return RedirectToAction("ErrorPage", "CloudBase", new { ErrorText = errorMsg });
                }

                // 登录信息保存cookie
                _cookieService.SetUserCookie(userinfo);

                // 保存登录信息
                _mobileService.SaveLoginRecord(userinfo);
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(ProjectController));
                return RedirectToAction("ErrorPage", "CloudBase", new { ErrorText = "身份错误，请联系管理员。" });
            }
            return Redirect("/Mobile/Project/Index");
        }

        /// <summary>
        /// 手机端项目列表页
        /// </summary>
        [TypeFilter(typeof(WindCloudFilter))]
        public IActionResult Index()
        {
            string CloudID = HttpContext.Request.Query["CloudID"].ToString();
            if (!string.IsNullOrEmpty(CloudID))
            {
                // 判断权限
                int ProjectID = Convert.ToInt32(CloudID);
                var project = _mobileService.GetProjectByID(ProjectID);
                if (project == null)
                {
                    return RedirectToAction("Error", "BaseMobile", new { ErrorText = "项目数据错误" });
                }

                return Redirect("/Mobile/Project/ProjectInfo?ProjectID=" + CloudID);
            }

            ViewData["UserName"] = CurrentUser?.RealName;
            ViewData["PostToken"] = CreateWebToken(CurrentUser.UserCode);

            return View();
        }

        /// <summary>
        /// 项目列表查询
        /// </summary>
        [TypeFilter(typeof(WindCloudFilter))]
        public async Task<IActionResult> ProjectListQuery([FromForm] CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            try
            {
                var (list, msg) = await _mobileService.GetMobileProjectList(request);

                if (!string.IsNullOrEmpty(msg))
                {
                    ret.Success = false;
                    ret.Message = msg;
                    return Ok(ret);
                }

                if (!string.IsNullOrEmpty(request.ProjectName))
                {
                    list = list.Where(a => a.ProjectName.Contains(request.ProjectName)).ToList();
                }

                ret.TotalCount = list.Count;
                ret.TotalPage = list.Count % request.PageSize == 0
                    ? (list.Count / request.PageSize)
                    : (list.Count / request.PageSize) + 1;

                var currentQuery = list.OrderByDescending(a => a.ApplyTime)
                    .Skip(request.GetSkipCount())
                    .Take(request.PageSize)
                    .ToList();

                ret.Data = currentQuery;
                ret.Success = true;
                ret.PageIndex = request.PageIndex;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                LoggerUtils.Error(ex.ToString(), typeof(ProjectController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 项目详情页
        /// </summary>
        [TypeFilter(typeof(WindCloudFilter))]
        public IActionResult ProjectInfo()
        {
            var RequestID = HttpContext.Request.Query["ProjectID"].ToString();
            if (string.IsNullOrEmpty(RequestID))
            {
                return RedirectToAction("Error", "BaseMobile", new { ErrorText = "项目数据错误" });
            }

            int ProjectID = Convert.ToInt32(RequestID);
            var isHasRight = _mobileService.HasProjectViewRight(ProjectID, CurrentUser.UserCode);
            if (!isHasRight)
            {
                return RedirectToAction("Error", "BaseMobile", new { ErrorText = "没有权限浏览" });
            }

            ViewData["ProjectID"] = ProjectID;
            ViewData["UserName"] = CurrentUser?.RealName;
            ViewData["PostToken"] = CreateWebToken(CurrentUser.UserCode);
            return View();
        }

        /// <summary>
        /// 项目详情数据查询
        /// </summary>
        [TypeFilter(typeof(WindCloudFilter))]
        public async Task<IActionResult> ProjectInfoQuery([FromForm] CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            if (request.ProjectID == 0)
            {
                ret.Message = "项目数据错误";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(CurrentUser?.UserCode))
            {
                ret.Message = "用户信息失效，请重新交建通登录";
                return Ok(ret);
            }
            try
            {
                var (data, msg) = await _mobileService.GetMobileProjectInfo(request);

                if (string.IsNullOrEmpty(msg))
                {
                    ret.Data = data;
                    ret.Success = true;
                }
                else
                {
                    ret.Data = data;
                    ret.Message = msg;
                    ret.Success = false;
                }
            }
            catch (Exception ex)
            {
                ret.Success = false;
                LoggerUtils.Error(ex.ToString(), typeof(ProjectController));
            }
            return Ok(ret);
        }
    }
}
