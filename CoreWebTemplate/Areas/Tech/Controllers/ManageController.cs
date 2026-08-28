using Microsoft.AspNetCore.Mvc;
using Service.Test;
using Service.Wind;

namespace CoreWebTemplate.Areas.Tech.Controllers
{
    [Area("Tech")]
    public class ManageController : Controller
    {

        // 注入业务服务
        private readonly UserService _userService;
        private readonly ManageService _manageService;

        public ManageController(UserService userService, ManageService manageService)
        {
            _userService = userService;
            _manageService = manageService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Data()
        {
            return View();
        }

        public async Task<IActionResult> GetData()
        {
            // 直接await调用异步方法
            var testDataList = await _userService.GetDataAsync();

            // 处理结果（例如传递到视图）
            if (testDataList == null)
            {
                // 出错时返回错误视图
                return View("Error");
            }

            // 将数据传递到视图
            return View(testDataList);
        }

        public async Task<IActionResult> GetData2()
        {
            // 直接await调用异步方法
            var testDataList = await _manageService.GetCloudWindUserByIdAsync(2);

            // 处理结果（例如传递到视图）
            if (testDataList == null)
            {
                // 出错时返回错误视图
                return View("Error");
            }

            // 将数据传递到视图
            return View(testDataList);
        }
    }
}
