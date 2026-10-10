using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model.Base;
using Service.TechCenter;
using Tool;

namespace WindCloud.Areas.TechApi.Controllers
{
    [Route("CCSHJTechApi/[controller]/[action]")]
    [ApiController]
    public class TechCenterController : BaseApiController
    {
        private readonly PlatService _platService;

        public TechCenterController(PlatService platService)
        {
            _platService = platService;
        }

        /// <summary>
        /// 平台Token验证
        /// </summary>
        [HttpPost]
        public BaseReturn PlatTokenChecking(TechCloudApiRequest request)
        {
            var ret = new BaseReturn();

            if (string.IsNullOrEmpty(request.Token))
            {
                ret.Message = "token是空";
                ret.Success = false;
                return ret;
            }

            try
            {
                var info = _platService.CheckToken(request.Token);
                if (info == null || string.IsNullOrEmpty(info.UserCode))
                {
                    ret.Message = "授权出现错误";
                    ret.Success = false;
                    return ret;
                }

                ret.Data = info;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Message = "发生错误";
                ret.Success = false;
                LoggerUtils.Error(ex.ToString(), typeof(TechCenterController));
            }

            return ret;
        }
    }
}
