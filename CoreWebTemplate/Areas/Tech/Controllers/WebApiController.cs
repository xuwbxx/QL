using Microsoft.AspNetCore.Mvc;
using Model.Base;

namespace CoreWebTemplate.Areas.Tech.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class WebApiController : ControllerBase
    {

        [HttpPost]
        public string Test(BaseApiRequest request)
        {
            return "Success";
        }

    }
}
