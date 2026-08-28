using Microsoft.AspNetCore.Mvc;
using Model.Base;
using Model.Tech.Cloud.BackManage;
using Service.Base;
using Service.Wind.BackManage;
using Tool;
using WindCloud.Areas.Base.Controllers;

namespace WindCloud.Areas.BackManage.Controllers
{
    [Area("BackManage")]
    public class ProjectController : WebEncryptionController
    {
        private readonly BackProjectService _projectService;

        public ProjectController(CookieService cookieService, Service.Wind.WebValidateService webValidateService, BackProjectService projectService)
            : base(cookieService, webValidateService)
        {
            _projectService = projectService;
        }

        public async Task<IActionResult> Index()
        {
            if (!await RightValidate())
            {
                return Redirect("/Cloud/CloudBase/ErrorPage?ErrorText=不能进入后台配置界面。");
            }

            ViewData["PostToken"] = CreateWebToken();
            return View();
        }

        [HttpPost]
        public IActionResult ProjectListQuery(CloudWindBackManageRequest request)
        {
            BaseReturn ret = new BaseReturn();
            try
            {
                var (list, totalCount, pageIndex, msg) = _projectService.ProjectListQuery(request);

                if (!string.IsNullOrEmpty(msg))
                {
                    ret.Message = msg;
                    return Ok(ret);
                }

                ret.Data = list;
                ret.TotalCount = totalCount;
                ret.TotalPage = totalCount % request.PageSize == 0 ? (totalCount / request.PageSize) : (totalCount / request.PageSize) + 1;
                ret.Success = true;
                ret.PageIndex = pageIndex;
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(ProjectController));
                ret.Success = false;
                ret.Message = "发生错误";
            }
            return Ok(ret);
        }

        [HttpPost]
        public IActionResult ProjectDataQuery(CloudWindBackManageRequest request)
        {
            BaseReturn ret = new BaseReturn();

            if (request.ProjectID == 0)
            {
                return Ok(ret);
            }

            try
            {
                var (project, msg) = _projectService.ProjectDataQuery(request);

                if (!string.IsNullOrEmpty(msg))
                {
                    ret.Message = msg;
                    return Ok(ret);
                }

                ret.Data = project;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(ProjectController));
                ret.Success = false;
                ret.Message = "发生错误";
            }
            return Ok(ret);
        }


        public IActionResult ProjectFileExport(CloudWindBackManageRequest request)
        {
            if (request.ProjectID == 0)
            {
                return NotFound();
            }

            try
            {
                var (filePath, fileName, msg) = _projectService.ProjectFileExport(request);

                if (!string.IsNullOrEmpty(msg))
                {
                    return NotFound();
                }

                return PhysicalFile(filePath, "application/x-zip-compressed", fileName);
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(ProjectController));
                return NotFound();
            }
        }
    }
}
