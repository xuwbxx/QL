using Model.TechCenter.SHJ4AUser;
using Tool;

namespace Service.TechCenter
{

    public class TC_SHJ4AService
    {
        private static string sysType = AppSettingUtils.GetSetting("DigitalCenter:SHJUser4A:TC_sysType");
        private static string verifykey = AppSettingUtils.GetSetting("DigitalCenter:SHJUser4A:TC_verifykey");

        /// <summary>
        /// 通过三航局用户编号，获取用户信息
        /// </summary>
        /// <param name="User4ACode"></param>
        /// <returns></returns>
        public static async Task<SHJUserResponse<List<SHJUserData>>> GetShjUserInfo(string User4ACode)
        {
            SHJUserResponse<List<SHJUserData>> ret = new SHJUserResponse<List<SHJUserData>>();
            try
            {
                string PostUrl = @"http://10.6.54.10:9098/In_Emp/QueryEmpBy4ACode";
                SHJUserRequest request = new SHJUserRequest();
                request.User4ACode = User4ACode;
                request.OID = User4ACode;
                request.sysType = sysType;
                request.verifykey = verifykey;
                var JsonRet = await HttpUtils.PostAsync(PostUrl, request);
                ret = JsonUtils.Deserialize<SHJUserResponse<List<SHJUserData>>>(JsonRet);
            }
            catch (Exception ex)
            {
                // 记录业务级别的Error日志
                LoggerUtils.Error(ex.ToString(), typeof(CloudWind_SHJ4AService));
                throw;
            }
            return ret;
        }


        /// <summary>
        /// 通过关键字查询三航局用户(模糊搜索)
        /// </summary>
        /// <param name="Name"></param>
        public static async Task<SHJUserResponse<List<SHJUserData>>> GetShjUserInfoByName(string Name)
        {
            SHJUserResponse<List<SHJUserData>> ret = new SHJUserResponse<List<SHJUserData>>();
            try
            {
                //
                string PostUrl = @"http://10.6.54.10:9098/In_Emp/QueryEmpByName";
                SHJUserRequest request = new SHJUserRequest();
                request.UserName = Name;
                request.sysType = sysType;
                request.verifykey = verifykey;

                var JsonRet = await HttpUtils.PostAsync(PostUrl, request);
                ret = JsonUtils.Deserialize<SHJUserResponse<List<SHJUserData>>>(JsonRet);
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(CloudWind_SHJ4AService));
                throw;
            }
            return ret;
        }

    }


    public class CloudWind_SHJ4AService
    {
        private static string sysType = AppSettingUtils.GetSetting("DigitalCenter:SHJUser4A:WindCloud_sysType");
        private static string verifykey = AppSettingUtils.GetSetting("DigitalCenter:SHJUser4A:WindCloud_verifykey");

        /// <summary>
        /// 通过三航局用户编号，获取用户信息
        /// </summary>
        /// <param name="User4ACode"></param>
        /// <returns></returns>
        public static async Task<SHJUserResponse<List<SHJUserData>>> GetShjUserInfo(string User4ACode)
        {
            SHJUserResponse<List<SHJUserData>> ret = new SHJUserResponse<List<SHJUserData>>();
            try
            {
                string PostUrl = @"http://10.6.54.10:9098/In_Emp/QueryEmpBy4ACode";
                SHJUserRequest request = new SHJUserRequest();
                request.User4ACode = User4ACode;
                request.OID = User4ACode;
                request.sysType = sysType;
                request.verifykey = verifykey;
                var JsonRet = await HttpUtils.PostAsync(PostUrl, request);
                ret = JsonUtils.Deserialize<SHJUserResponse<List<SHJUserData>>>(JsonRet);
            }
            catch (Exception ex)
            {
                // 记录业务级别的Error日志
                LoggerUtils.Error(ex.ToString(), typeof(CloudWind_SHJ4AService));
                throw;
            }
            return ret;
        }


        /// <summary>
        /// 通过关键字查询三航局用户(模糊搜索)
        /// </summary>
        /// <param name="Name"></param>
        public static async Task<SHJUserResponse<List<SHJUserData>>> GetShjUserInfoByName(string Name)
        {
            SHJUserResponse<List<SHJUserData>> ret = new SHJUserResponse<List<SHJUserData>>();
            try
            {
                //
                string PostUrl = @"http://10.6.54.10:9098/In_Emp/QueryEmpByName";
                SHJUserRequest request = new SHJUserRequest();
                request.UserName = Name;
                request.sysType = sysType;
                request.verifykey = verifykey;

                var JsonRet = await HttpUtils.PostAsync(PostUrl, request);
                ret = JsonUtils.Deserialize<SHJUserResponse<List<SHJUserData>>>(JsonRet);
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(CloudWind_SHJ4AService));
                throw;
            }
            return ret;
        }

    }
}
