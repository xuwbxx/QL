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
    public class FlowController : WebEncryptionController
    {
        private readonly BackFlowService _flowService;

        public FlowController(CookieService cookieService, Service.Wind.WebValidateService webValidateService, BackFlowService flowService)
            : base(cookieService, webValidateService)
        {
            _flowService = flowService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Node()
        {
            if (!await RightValidate())
            {
                return Redirect("/Cloud/CloudBase/ErrorPage?ErrorText=不能进入后台配置界面。");
            }

            var softwareList = _flowService.GetSoftwareList();
            ViewData["SoftwareList"] = softwareList;
            ViewData["PostToken"] = CreateWebToken();

            return View();
        }

        [HttpPost]
        public IActionResult NodeListQuery(CloudWindBackManageRequest request)
        {
            BaseReturn ret = new BaseReturn();
            try
            {
                var (list, totalCount, pageIndex, msg) = _flowService.NodeListQuery(request);

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
                LoggerUtils.Error(ex.ToString(), typeof(FlowController));
                ret.Success = false;
                ret.Message = "发生错误";
            }
            return Ok(ret);
        }

        [HttpPost]
        public IActionResult NodeDataQuery(CloudWindBackManageRequest request)
        {
            BaseReturn ret = new BaseReturn();

            if (request.ID == 0)
            {
                return Ok(ret);
            }

            try
            {
                var (node, msg) = _flowService.NodeDataQuery(request);

                if (!string.IsNullOrEmpty(msg))
                {
                    ret.Message = msg;
                    return Ok(ret);
                }

                ret.Data = node;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(FlowController));
                ret.Success = false;
                ret.Message = "发生错误";
            }
            return Ok(ret);
        }

        [HttpPost]
        public IActionResult DataSave(CloudWindBackManageRequest request)
        {
            BaseReturn ret = new BaseReturn();

            if (request.NodeID == 0)
            {
                ret.Message = "节点数据错误";
                return Ok(ret);
            }

            try
            {
                var msg = _flowService.DataSave(request);

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
                LoggerUtils.Error(ex.ToString(), typeof(FlowController));
                ret.Success = false;
                ret.Message = "发生错误，请联系管理员";
            }
            return Ok(ret);
        }
    }
}
