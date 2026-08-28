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

                BaseApiRequest request = new BaseApiRequest();
                request.Token = "dilnamXTCvEz2R8NP+y7T480iwJRBdTsy0ONGt6yvizRazusFKdrAohsH6nzvwqPmLtKhXzK09yTE2Cfy6+wGsYMucm+ISAtDg5BI/2y/GvvEPLbQqcg9k9PdssmoIw0THKwqB6sduVddehQEFzN0rCyv5W93DT64bR4MEYD2mhzNL+ZrS/KnC4znUTW06pNs9HItpz+5Dubst3V0sjoWRwpBMub/OAcn+U+sVOES7pCw/s0uh4aksUDW9VAIYwGaQHWrSQBC08kozUVtgEwOu+doBLgitSRmr41iFHgKg+WbyZWk135yZhGMQTJc4VEaT8Ha1cnFaUel5PfS0Y7wCsCFMUAWBDGv64220SuDLUBd8VQJz7NivkoPzRlGLQAygpJz3VmWyXOr50fWfOGHQofMJ14E2V/mS17YDQolS19xHUei9wMObIXC3E38vdih4Dck00Ibp77gXmAn5Kn3b3VEWecgS1voA2vdiRNdGHj9tvxusGSL9e0irQjE6k63FBSqscZYtN5IhPIMf3YRQ==";
                string PostUrl = "http://10.6.74.208:8061/CCSHJ/DataPort/VerifyToken";

                var ret1 = await HttpUtils.PostAsync(PostUrl, request);

                var ret = JsonUtils.Deserialize<BaseApiReturn<SSOUserInfo>>(ret1);


            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
