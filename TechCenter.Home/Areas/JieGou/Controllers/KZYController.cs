using Microsoft.AspNetCore.Mvc;
using Model.TechCenter.Others;
using Tool;

namespace TechCenter.Home.Areas.JieGou.Controllers
{
    [Area("JieGou")]
    public class KZYController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnv;

        public KZYController(IWebHostEnvironment webHostEnv)
        {
            _webHostEnv = webHostEnv;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Page1()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetData([FromBody] PostRequest request)
        {

            List<WindResultYear> years = new List<WindResultYear>();
            try
            {
                //风速小于8米/秒   hs（浪高）小于0.8米   周期小于5秒

                // 关键：通过 WebRootPath 获取 wwwroot 目录路径
                string wwwrootPath = _webHostEnv.WebRootPath;
                // 拼接文件路径（wwwroot/file/data.csv）
                string filePath = Path.Combine(wwwrootPath, "file", "JieGou", "data.csv");

                var data = CsvService<WindData>.ReadCsvFile(filePath);

                List<DateTime> dt = new List<DateTime>();

                data.Where(a => a.valid_time.Month == request.Month).ToList().ForEach(a =>
                {
                    bool IsDo = false;
                    if (a.wind >= request.WindSpeedMin && a.wind <= request.WindSpeedMax
                    && a.Hs >= request.WaveHeightMin && a.Hs <= request.WaveHeightMax
                    && a.Swell_period >= request.input_PeriodMin && a.Swell_period <= request.input_PeriodMax)
                    {
                        IsDo = true;
                        dt.Add(a.valid_time);
                    }


                    // 直接从DateTime提取信息
                    int year = a.valid_time.Year;
                    int month = a.valid_time.Month;
                    int day = a.valid_time.Day;
                    int hour = a.valid_time.Hour;

                    // 1. 获取或创建年份对象
                    var targetYear = years.FirstOrDefault(y => y.Year == year);
                    if (targetYear == null)
                    {
                        targetYear = new WindResultYear { Year = year };
                        years.Add(targetYear);
                    }

                    // 2. 获取或创建月份对象
                    var targetMonth = targetYear.Months.FirstOrDefault(m => m.Month == month);
                    if (targetMonth == null)
                    {
                        targetMonth = new WindResultMonth { Month = month };
                        targetYear.Months.Add(targetMonth);
                    }

                    // 3. 获取或创建日期对象
                    var targetDay = targetMonth.Days.FirstOrDefault(d => d.Day == day);
                    if (targetDay == null)
                    {
                        targetDay = new WindResultDay { Day = day };
                        targetMonth.Days.Add(targetDay);
                    }

                    // 4. 添加小时对象（去重 + 排序）
                    if (!targetDay.Hours.Any(h => h.Hour == hour))
                    {


                        targetDay.Hours.Add(new WindResultHour { Hour = hour, Do = IsDo });
                        targetDay.Hours = targetDay.Hours.OrderBy(h => h.Hour).ToList();
                    }



                    //if (a.wind <= 8 && a.Hs <= 0.8 && a.Swell_period <= 5)
                    //{

                    //}
                });

                // 层级排序（年→月→日）
                years = years.OrderBy(y => y.Year).ToList();
                foreach (var year in years)
                {
                    year.Months = year.Months.OrderBy(m => m.Month).ToList();
                    foreach (var month in year.Months)
                    {
                        month.Days = month.Days.OrderBy(d => d.Day).ToList();
                    }
                }


            }
            catch (Exception ex)
            {
                years = new List<WindResultYear>();
            }

            return Json(years);

        }





    }
}
