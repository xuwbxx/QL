using Microsoft.AspNetCore.Mvc;
using Model.Base;
using Model.Tech.Cloud.BackManage;
using Service.Base;
using Service.Wind;
using Service.Wind.BackManage;
using Service.Base.Filter;
using Tool;
using WindCloud.Areas.Base.Controllers;

namespace WindCloud.Areas.BackManage.Controllers
{
    [Area("BackManage")]
    public class CopyInformController : WebEncryptionController
    {
        private readonly BackCopyInformService _copyInformService;

        public CopyInformController(CookieService cookieService, Service.Wind.WebValidateService webValidateService, ProjectService projectService, BackCopyInformService copyInformService)
            : base(cookieService, projectService, webValidateService)
        {
            _copyInformService = copyInformService;
        }

        [TypeFilter(typeof(WindCloudFilter))]
        public async Task<IActionResult> Index()
        {
            if (!await RightValidate())
            {
                return Redirect("/Cloud/CloudBase/ErrorPage?ErrorText=不能进入后台配置界面。");
            }

            var softwareList = _copyInformService.GetSoftwareList();
            ViewData["SoftwareList"] = softwareList;
            ViewData["PostToken"] = CreateWebToken();
            return View();
        }

        [HttpPost]
        public IActionResult ListQuery(CloudWindBackManageRequest request)
        {
            BaseReturn ret = new BaseReturn();
            try
            {
                var (list, totalCount, pageIndex, msg) = _copyInformService.ListQuery(request);

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
                LoggerUtils.Error(ex.ToString(), typeof(CopyInformController));
                ret.Success = false;
                ret.Message = "发生错误";
            }
            return Ok(ret);
        }

        [HttpPost]
        public IActionResult DataSave(CloudWindBackManageRequest request)
        {
            BaseReturn ret = new BaseReturn();

            if (request.NodeManagers == null || request.NodeManagers.Count == 0)
            {
                ret.Message = "没有任何用户信息";
                return Ok(ret);
            }

            if (request.SoftwareID == 0)
            {
                ret.Message = "没有选择任何软件";
                return Ok(ret);
            }

            try
            {
                var msg = _copyInformService.DataSave(request);

                if (!string.IsNullOrEmpty(msg))
                {
                    ret.Success = false;
                    ret.Message = msg;
                    return Ok(ret);
                }

                ret.Success = true;
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(CopyInformController));
                ret.Success = false;
                ret.Message = "发生错误，请联系管理员";
            }
            return Ok(ret);
        }

        [HttpPost]
        public IActionResult DataDelete(CloudWindBackManageRequest request)
        {
            BaseReturn ret = new BaseReturn();

            if (request.ID == 0)
            {
                ret.Message = "数据错误";
                return Ok(ret);
            }

            try
            {
                var msg = _copyInformService.DataDelete(request);

                if (!string.IsNullOrEmpty(msg))
                {
                    ret.Success = false;
                    ret.Message = msg;
                    return Ok(ret);
                }

                ret.Success = true;
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(CopyInformController));
                ret.Success = false;
                ret.Message = "发生错误，请联系管理员";
            }
            return Ok(ret);
        }
    }
}
