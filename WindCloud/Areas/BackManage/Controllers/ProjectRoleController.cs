using Microsoft.AspNetCore.Mvc;
using Model.Base;
using Model.Tech.Cloud.BackManage;
using Service.Base;
using Service.Wind;
using Service.Wind.BackManage;
using Tool;
using WindCloud.Areas.Base.Controllers;

namespace WindCloud.Areas.BackManage.Controllers
{
    [Area("BackManage")]
    public class ProjectRoleController : WebEncryptionController
    {
        private readonly BackProjectRoleService _backProjectRoleService;

        public ProjectRoleController(CookieService cookieService, Service.Wind.WebValidateService webValidateService, ProjectService projectService, BackProjectRoleService backProjectRoleService)
            : base(cookieService, projectService, webValidateService)
        {
            _backProjectRoleService = backProjectRoleService;
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
        public async Task<IActionResult> ListQuery(CloudWindBackManageRequest request)
        {
            BaseReturn ret = new BaseReturn();

            try
            {
                var admins = await GetAdminUserCode();
                var (list, totalCount, pageIndex, msg) = _backProjectRoleService.ListQuery(request, admins, CurrentUser.UserCode);

                if (!string.IsNullOrEmpty(msg))
                {
                    ret.Message = msg;
                    return Ok(ret);
                }

                ret.Data = list;
                ret.Success = true;
                ret.TotalCount = totalCount;
                ret.PageIndex = pageIndex;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                LoggerUtils.Error(ex.ToString(), typeof(ProjectRoleController));
            }

            return Ok(ret);
        }

        [HttpPost]
        public IActionResult DataQuery(CloudWindBackManageRequest request)
        {
            BaseReturn ret = new BaseReturn();

            if (request.ProjectID == 0)
            {
                ret.Message = "项目数据错误";
                return Ok(ret);
            }

            try
            {
                var (data, msg) = _backProjectRoleService.DataQuery(request);

                if (!string.IsNullOrEmpty(msg))
                {
                    ret.Message = msg;
                    return Ok(ret);
                }

                ret.Data = data;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                LoggerUtils.Error(ex.ToString(), typeof(ProjectRoleController));
            }

            return Ok(ret);
        }

        [HttpPost]
        public IActionResult DataSave(CloudWindBackManageRequest request)
        {
            BaseReturn ret = new BaseReturn();

            if (request.ProjectID == 0)
            {
                ret.Message = "项目数据错误";
                return Ok(ret);
            }

            try
            {
                string result = _backProjectRoleService.DataSave(request);

                if (!string.IsNullOrEmpty(result))
                {
                    ret.Message = result;
                    return Ok(ret);
                }

                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = "发生错误，请联系管理员";
                LoggerUtils.Error(ex.ToString(), typeof(ProjectRoleController));
            }

            return Ok(ret);
        }
    }
}
