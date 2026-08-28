using Microsoft.AspNetCore.Mvc;
using Model.Base;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TechCenter.Home.Areas.Tech.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpPost]
        public string GetData([FromBody] BaseApiRequest request)
        {
            return request.Requester;
        }

    }
}
