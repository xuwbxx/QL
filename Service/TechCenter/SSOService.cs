using DataFactory.Factory;
using DataFactory.KingBase;
using Model.Base;
using Model.TechCenter.JJT;
using Tool;

namespace Service.TechCenter
{
    public class SSOService
    {
        // 依赖注入：仓储工厂（核心）+ 日志（可选，用于异常追踪）
        private readonly TechCenter_KingBase_UnitOfWorkFactory _techCenterUowFactory;

        public SSOService(TechCenter_KingBase_UnitOfWorkFactory techCenterUowFactory)
        {
            _techCenterUowFactory = techCenterUowFactory;
        }

        /// <summary>
        /// 创建单点登录token
        /// 平台ID 时间 用户Code 
        /// </summary>
        public async Task<string> CreateSSOToken(SHJUserInfo user)
        {

            if (string.IsNullOrEmpty(user.UserCode) || user.SoftwareID == 0)
            {
                return "";
            }

            try
            {
                string GUID = Guid.NewGuid().ToString();

                var tokenData = new EncryptData<SHJUserInfo>()
                {
                    AGuid = GUID,
                    Data = user,
                    Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    ZGuid = GUID
                };

                string EncryptKey = AppSettingUtils.GetSetting("AppSettings:TripleEncrypt:Key");
                string EncryptIV = AppSettingUtils.GetSetting("AppSettings:TripleEncrypt:IV");

                // 使用TripleDES加密JSON数据
                string encryptedToken = CryptographyUtils.TripleDESEncrypt(JsonUtils.Serialize(tokenData), EncryptKey, EncryptIV);

                TechCenter_DataLoginResult data = new TechCenter_DataLoginResult();
                data.CreateTime = DateTime.Now;
                data.IsDelete = false;
                data.Token = encryptedToken;
                data.UserCode = user.UserCode;
                data.GUID = GUID;
                data.UserName = user.RealName;
                data.SoftwareID = user.SoftwareID;
                data.Result = false;

                //保存token
                using (var uow = _techCenterUowFactory.Create())
                {
                    var repo = uow.GetRepository<TechCenter_DataLoginResult>();
                    //新增需要使用sql语句
                    string ThisSql = SqlUtils.ToInsertSql(data, "Data_LoginResult");
                    int count = await repo.ExecuteSqlAsync(ThisSql);

                    if (count == 0)
                    {
                        return "";
                    }
                }

                return encryptedToken;
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(SSOService));
                return "";
            }


        }

        public async Task<ServiceReturn<SHJUserInfo>> DecryptSSOToken(string SSOToken)
        {

            ServiceReturn<SHJUserInfo> ret = new ServiceReturn<SHJUserInfo>();

            if (string.IsNullOrEmpty(SSOToken))
            {
                ret.Success = false;
                ret.Message = "Token是空值";
                return ret;
            }

            try
            {
                string EncryptKey = AppSettingUtils.GetSetting("AppSettings:TripleEncrypt:Key");
                string EncryptIV = AppSettingUtils.GetSetting("AppSettings:TripleEncrypt:IV");

                //设计通用token。方便测试
                string TestToken = "dilnamXTCvEQROpANmrOe9QxSBrE8e9gBCQ4f1N6vKSDxn/PoQPmybiXTegbaihjI0Rrzr751devOg72IvHVDs2V1aMMnTK/JNuoVayErAsqx+b/vMfcsezKGJA+xz+Vulf029TyrL/IJgjp1Zf086N+6Nhf/XlH/EMg5GILtRMJNiUknYPLWRtPMuOsm7eLem7s3dXVRvLkZYXieMfUDp1nHHBZ0SQVvBciXviSal1kbsuWzhiSifZkz90X12Ep/JfMHVuH0/6Vko6+i4GEt3Hcw1QhJxszQd4lJItUiC58DNBBLM7Z0ECiZiw1db1Y6zUwxH0eEmEFipTemXlHw1u/CiL/qve3BRxO8vOCgVP6Odc92sVQIeEaG+IovTEkGDH5pRSp9CZxcIzS8yK48CdiFRKGQl1QCnh6KPr80sgqsAlTWE6WYdmHjv5X0zrdPYWbYI8lDP9LzaytOaMEXUbKyOxUHoLPC349k2L/JO04VJgORCWaj82QC9g9uJS3N9t+SI7VZzc0PFEh4ubHX63sdWaFPr1CjLRrQYTFN9EwC1nbGNHvXOfO5wUp8Bpm";
                if (SSOToken.Equals(TestToken))
                {
                    string decryptedToken = CryptographyUtils.TripleDESDecrypt(SSOToken, EncryptKey, EncryptIV);

                    EncryptData<SHJUserInfo> model = JsonUtils.Deserialize<EncryptData<SHJUserInfo>>(decryptedToken);

                    ret.Data = model.Data;
                    ret.Success = true;
                    return ret;

                }
                else
                {
                    string decryptedToken = CryptographyUtils.TripleDESDecrypt(SSOToken, EncryptKey, EncryptIV);

                    EncryptData<SHJUserInfo> model = JsonUtils.Deserialize<EncryptData<SHJUserInfo>>(decryptedToken);


                    if (model == null || model.Data == null || string.IsNullOrEmpty(model.AGuid) || string.IsNullOrEmpty(model.Data.UserCode))
                    {
                        ret.Success = false;
                        ret.Message = "发生错误";
                        return ret;
                    }

                    //时间超过1小时
                    DateTime time = Convert.ToDateTime(model.Time);
                    if (time.AddHours(1) <= DateTime.Now)
                    {
                        ret.Success = false;
                        ret.Message = "token超时1小时无效了";
                        return ret;
                    }

                    //更新数据库
                    using (var uow = _techCenterUowFactory.Create())
                    {
                        var repo = uow.GetRepository<TechCenter_DataLoginResult>();

                        var data = await repo.FindFirstAsync(a => a.GUID.Equals(model.AGuid));
                        if (data == null)
                        {
                            ret.Success = false;
                            ret.Message = "发生错误";
                            return ret;
                        }
                        data.Result = true;
                        int count = await repo.SaveAsync();

                    }


                    //返回用户数据

                    ret.Data = model.Data;
                    ret.Success = true;
                    return ret;
                }



            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = "发生错误";
                LoggerUtils.Error(ex.ToString(), typeof(SSOService));
                return ret;
            }

        }

    }

}
