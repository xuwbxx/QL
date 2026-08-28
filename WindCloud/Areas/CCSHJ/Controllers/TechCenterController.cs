using Microsoft.AspNetCore.Mvc;
using Model.Base;
using Model.Tech.TechCenter;
using Service.Base;
using Service.Wind;
using Tool;
using WindCloud.Areas.Base.Controllers;

namespace WindCloud.Areas.CCSHJ.Controllers
{
    [Area("CCSHJ")]
    public class TechCenterController : WebEncryptionController
    {
        public TechCenterService _techCenterService;

        public TechCenterController(CookieService cookieService, WebValidateService webValidateService, TechCenterService techCenterService) : base(cookieService, webValidateService)
        {
            _techCenterService = techCenterService;
        }

        /// <summary>
        /// 技术中心平台集成页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Main()
        {
            ViewData["PostToken"] = CreateWebToken();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetUrl([FromForm] TechCenterRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Json(ret);
            }
            if (request.PlatID == 0)
            {
                ret.Message = "发生错误";
                return Ok(ret);
            }
            try
            {
                var (PlatUrl, msg) = await _techCenterService.GetUrl(request.PlatID);
                if (!string.IsNullOrEmpty(msg))
                {
                    ret.Message = msg;
                    return Ok(ret);
                }

                ret.Data = PlatUrl;
                ret.Success = true;

            }
            catch (Exception ex)
            {
                ret.Success = false;
                //记录日志
                LoggerUtils.Error(ex.ToString(), typeof(TechCenterController));
            }
            return Ok(ret);
        }
    }
}
