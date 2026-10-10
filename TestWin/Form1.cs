using Model.Base;
using Tool;

namespace TestWin
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {

                // 测试1: 无参数
                //var ret0 = HttpUtils.PostSync("http://localhost:5005/CCSHJTechApi/CloudCenter/ApiTest2", "");

                // 测试2: [FromBody] string
                //var ret1 = HttpUtils.PostSync("http://localhost:5005/CCSHJTechApi/CloudCenter/FromBodyTestString", "hello");

                // 测试3: [FromBody] TechCloudApiRequest
                //TechCloudApiRequest request = new TechCloudApiRequest();
                //request.Token = "123";
                //var ret2 = HttpUtils.PostSync("http://localhost:5005/CCSHJTechApi/CloudCenter/FromBodyTestObject", request);

                // 测试4: [FromBody] 本地 SimpleTestDto
                //var ret3 = HttpUtils.PostSync("http://localhost:5005/CCSHJTechApi/CloudCenter/FromBodyTestLocal", new { Name = "test" });

                string postUrl = @"http://localhost:5005/CCSHJTechApi/CloudCenter/SSOLoginChecking";

                TechCloudApiRequest request = new TechCloudApiRequest();
                request.Token = @"eExyMFpyVHd4Mlc2dXlQcUtYUjI5OVVZQWtoYUZaU3VXdW1IeEJDOXRZWHVGRXRaaXJzbWNnPT0=";


                var ret = HttpUtils.PostSync(postUrl, request);

            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
