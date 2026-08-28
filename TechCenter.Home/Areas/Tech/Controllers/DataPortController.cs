using Microsoft.AspNetCore.Mvc;
using Model.Base;
using Service.TechCenter;
using Tool;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TechCenter.Home.Areas.Tech.Controllers
{
    [Route("CCSHJ/[controller]/[action]")]
    [ApiController]
    public class DataPortController : ControllerBase
    {

        private SSOService _sSOService { get; }
        public DataPortController(SSOService sSOService)
        {
            _sSOService = sSOService;
        }


        [HttpPost]
        public string Test(BaseApiRequest request)
        {
            return "Success";
        }

        [HttpGet]
        public string Test2()
        {
            return "Success";
        }

        [HttpPost]
        public async Task<BaseApiReturn<SSOUserInfo>> VerifyToken([FromBody] BaseApiRequest request)
        {

            BaseApiReturn<SSOUserInfo> ret = new BaseApiReturn<SSOUserInfo>();
            if (request == null || string.IsNullOrEmpty(request.Token))
            {
                ret.Success = false;
                ret.Message = "Token是空";
                return ret;
            }

            try
            {

                var ServiceData = await _sSOService.DecryptSSOToken(request.Token);
                if (!ServiceData.Success)
                {
                    ret.Success = false;
                    ret.Message = ServiceData.Message;
                }
                else
                {
                    ret.Success = true;
                    SSOUserInfo user = new SSOUserInfo();
                    user.UserCode = ServiceData.Data.UserCode;
                    user.UserName = ServiceData.Data.RealName;
                    user.Depart = ServiceData.Data.Depart;
                    user.Phone = ServiceData.Data.Mobile;
                    ret.Data = user;
                }

            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(DataPortController));
            }

            return ret;
        }



    }
}
