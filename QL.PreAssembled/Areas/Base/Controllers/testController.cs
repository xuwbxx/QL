using DataFactory.KingBase;
using Microsoft.AspNetCore.Mvc;
using Service.PreAssembled;

namespace QL.PreAssembled.Areas.Base.Controllers
{
    [Area("Base")]
    public class testController : Controller
    {
        private testTableService _testTableService { get; }

        public testController(testTableService testTableService)
        {
            _testTableService = testTableService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Test()
        {
            var list = await _testTableService.GetListAsync();


            return View();
        }

        public async Task<IActionResult> Test2()
        {
            testTable data = new testTable();
            //data.ID = 4;
            data.Name = "fff";

            await _testTableService.Add(data);


            return View();
        }

        public async Task<IActionResult> Test3()
        {
            //testTable data = new testTable();
            //data.ID = 4;
            //data.Name = "fff";

            string sql = @"select * from testTable";

            var list = await _testTableService.queryBySql(sql);


            return View();
        }

    }
}
