using DataFactory.Factory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Model.Base;
using Service.Base.Data;
using Service.Struct;
using Service.TechCenter;
using System.Data;
using System.Web;
using Tool;

namespace TestUnit
{
    [TestClass]
    public sealed class Test1
    {
        private MultiDbRepositoryFactory factory;

        private ServiceProvider serviceProvider;

        [TestInitialize]
        public void Initialize()
        {
            // 1. 读取配置文件
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            factory = new MultiDbRepositoryFactory(configuration);



            // 2.注册服务
            var services = new ServiceCollection();

            // 2. 调用封装好的服务注册方法（核心：一行代码搞定所有配置）
            ServiceInject.ConfigureServices(services);

            // 3. 构建服务提供器
            serviceProvider = services.BuildServiceProvider();

        }

        [TestMethod]
        public async Task QueryShjUser()
        {
            try
            {
                //金震天 330382199103267313
                //姚人臣 310103198609224092
                //时蓓玲 310110196911073226
                //陆骁尤 310115199607240910  L*Y_0910_081915
                //钱沛   320684199501240033
                //徐文彬 310113198206151418
                //徐锁林 4695
                //靖翔宇 3039
                //张曦 Z*_5538
                //孟若轶 0439
                //刘翰波 1711
                //苗艳遂 3319
                //孙洪春 2413
                //郭峯良 0913 2016160608
                //赵哲辉 32070319890421051X
                //崔灿 321323199307120435
                //刘孟源 211102199203012044
                //马振江 410928197804073912
                //刘璐 310110198305153778
                //王衔 340503199107310611
                //王杰 
                // 汪泽"612525199404060159"
                //雷丹421081198910013411

                //邢峰21078119981011121X

                //李业勋 411322198407014913 2013134353
                //尹海卿 33021119620821001X 1983000962
                //徐立新 320106196601240858 1988001759
                //曹金宝 320106197011191210 2005102565
                //黎亚舟
                //郭志鹏 222426198410193533 2007139795
                //潘晓炜 320822198001174831 2002003322
                //黄延琦 510132197908166616 2005102598
                //黄校松 330226197602110059 1998002114
                //夏显文 320102196709200019 1990001093
                //张继彪 411528198105065853 2011149218
                //李森   410901199107285513 2020002621
                //张曦   210106197911025538 2007139012
                //王煦   211203199412280020 2020018209
                //宇世杰 340111199508146515 2022017912
                //王世峰 321102196605060550 1990200115
                //马骏   510213197201060531 2016123493
                //王晓冉 412901197308301519 2016157082
                //尹建兵 320622197206144518 1994001842
                //姚人臣 310103198609224092 2009064495
                //汪来发 342501198403298615 2009065240

                string name = "毕宇";

                var users = await TC_SHJ4AService.GetShjUserInfoByName(name);
                //var users = TC_SHJ4AService.GetShjUserInfo("2004003455");
            }
            catch (Exception ex)
            {

                throw;
            }
        }



        [TestMethod]
        public async Task DataPortTest()
        {
            try
            {


                BaseApiRequest request = new BaseApiRequest();
                var Token = "dilnamXTCvEQROpANmrOe9QxSBrE8e9gBCQ4f1N6vKSDxn/PoQPmybiXTegbaihjI0Rrzr751devOg72IvHVDs2V1aMMnTK/JNuoVayErAsqx+b/vMfcsezKGJA+xz+Vulf029TyrL/IJgjp1Zf086N+6Nhf/XlH/EMg5GILtRMJNiUknYPLWRtPMuOsm7eLem7s3dXVRvLkZYXieMfUDp1nHHBZ0SQVvBciXviSal1kbsuWzhiSifZkz90X12Ep/JfMHVuH0/6Vko6+i4GEt3Hcw1QhJxszQd4lJItUiC58DNBBLM7Z0ECiZiw1db1Y6zUwxH0eEmEFipTemXlHw1u/CiL/qve3BRxO8vOCgVP6Odc92sVQIeEaG+IovTEkGDH5pRSp9CZxcIzS8yK48CdiFRKGQl1QCnh6KPr80sgqsAlTWE6WYdmHjv5X0zrdPYWbYI8lDP9LzaytOaMEXUbKyOxUHoLPC349k2L/JO04VJgORCWaj82QC9g9uJS3N9t+SI7VZzc0PFEh4ubHX63sdWaFPr1CjLRrQYTFN9EwC1nbGNHvXOfO5wUp8Bpm";
                //request.Token = "dilnamXTCvEHHEOQQzVbXJbJed7NFF8RKXvrzTyKlQBMD4T4ZWla7HXyWIWNLyPH33WzMX855NZ7l+27rGH27uVgX/0RmrPBuM747WRPQ+KSJLgUM40ukH/z8Ku+FfXWOy7YNIhpye/IzSf91TbQTfnvbTiqaKhMrUQ5m9CBP6f4/BmHa/R865zzcoSmZnSTaJyoQpaiRpgyO1tGvFr9EptD2mkwqq8LCaKpvt3ZAe43VM6obGjPQUvWDsowvl9onAs9/V221ySQ2xyfB0U6ifp++rkFR8XETwKvZcXWqO+qkjtNii9lqYghzs+vzPar5EeReiayRCM3knLkkwN5ns9wMmUPrxqSgI8DMhTWiuXdQPZb+E0o26kh0oC4xo6YEGVQ4AkYObd4VGA1ZAybQfFbmJ0lY/Mfg7xwr31wZTAaTrWPBFH0AxDkQGjjS9rr4+ZxGXtvHcsLvvaTIQExspexFw7GBUpn87VaFRboxo0Bt1lE6MStnacn2N2A74lwzXThn7Kx8vLCIcfwnn243evkjYtOJxbZBNq23ei/vFNXiZSAfmzMCPchW2JZtlFB";
                string PostUrl = "http://10.6.48.20:8088/CCSHJ/DataPort/VerifyToken";
                //string PostUrl = "http://10.6.74.208:5003/CCSHJ/DataPort/VerifyToken";


                request.Token = Token;

                //var TokenPlus = System.Web.HttpUtility.UrlDecode(Token);

                var ret1 = await HttpUtils.PostAsync(PostUrl, request);

                var ret = JsonUtils.Deserialize<BaseApiReturn<SSOUserInfo>>(ret1);



            }
            catch (Exception ex)
            {

                throw;
            }
        }

        [TestMethod]
        public async Task TestMethod1()
        {
            try
            {
                string EncryptKey = AppSettingUtils.GetSetting("AppSettings:TripleEncrypt:Key");
                string EncryptIV = AppSettingUtils.GetSetting("AppSettings:TripleEncrypt:IV");

                string code = Guid.NewGuid() + "_" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "_" + "2018001515" + "_" + Guid.NewGuid();

                string token = CryptographyUtils.TripleDESEncrypt(code, EncryptKey, EncryptIV);

                string urlToken = HttpUtility.UrlEncode(token);

                return;



                //using (var windRepo = factory.GetRepository<TechCenter_DataLoginResult>("KingBase_TechCenterDBConnection"))
                //{
                //    var data = await windRepo.FindFirstAsync(a => a.GUID.Equals("ebe973e6-198f-46de-95b3-dcf97b1e0963"));

                //    //var list = windRepo.QueryBySql("select * from Data_LoginResult");

                //    data.IsDelete = true;

                //    await windRepo.SaveAsync();

                //}

                //var ret = await TC_SHJ4AService.GetShjUserInfo("2018001515");

                //var rey = await TC_SHJ4AService.GetShjUserInfoByName("程伟");

                //ShjJJTMessageTCRequest request = new ShjJJTMessageTCRequest();

                //request.title = "消息测试";
                //request.content = "发送测试消息";
                //request.url = "http://www.baidu.com";
                //request.userlist = ["2018001515"];

                //var ret = await TC_JJTService.SendJJTMessage(request);

                string paramString = "0s8mSdCR456ppXRUAYA4wfX2t1XM4gSK8udi2rookY0vPJOM0Zkx7xRflDYvijbc";

                SingleSinOnModel result = new SingleSinOnModel(); ;

                string msg = TC_JJTService.AESSingleSignOn(paramString, out result);




            }
            catch (Exception ex)
            {

                throw;
            }


        }

        [TestMethod]
        public async Task TestMethod2()
        {
            try
            {


                SHJUserInfo user = new SHJUserInfo();
                user.UserCode = "2018001515";
                user.RealName = "程伟";
                user.UserName = "CHENGWEI";
                user.Depart = "软件开发中心";
                user.Job = "软件技术管理";
                user.Mobile = "13918863121";
                user.SoftwareID = 4;

                var service = serviceProvider.GetRequiredService<SSOService>();
                var ret = await service.CreateSSOToken(user);

                string token = "dilnamXTCvEQROpANmrOe9QxSBrE8e9gBCQ4f1N6vKSDxn/PoQPmybiXTegbaihjI0Rrzr751devOg72IvHVDs2V1aMMnTK/JNuoVayErAsqx+b/vMfcsezKGJA+xz+Vulf029TyrL/IJgjp1Zf086N+6Nhf/XlH/EMg5GILtRMJNiUknYPLWRtPMuOsm7eLem7s3dXVRvLkZYXieMfUDp1nHHBZ0SQVvBciXviSal1kbsuWzhiSifZkz90X12Ep/JfMHVuH0/6Vko6+i4GEt3Hcw1QhJxszQd4lJItUiC58DNBBLM7Z0ECiZiw1db1Y6zUwxH0eEmEFipTemXlHw1u/CiL/qve3BRxO8vOCgVP6Odc92sVQIeEaG+IovTEkGDH5pRSp9CZxcIzS8yK48CdiFRKGQl1QCnh6KPr80sgqsAlTWE6WYdmHjv5X0zrdPYWbYI8lDP9LzaytOaMEXUbKyOxUHoLPC349k2L/JO04VJgORCWaj82QC9g9uJS3N9t+SI7VZzc0PFEh4ubHX63sdWaFPr1CjLRrQYTFN9EwC1nbGNHvXOfO5wUp8Bpm";

                //var ret2 = await service.DecryptSSOToken(token);

                //string str = SqlUtils.ToInsertSql(user, "MyUser");


            }
            catch (Exception ex)
            {
                //LoggerUtils.Error(ex.ToString());
            }

        }

        [TestMethod]
        public void TestMethod3()
        {
            try
            {
                //var ret = PdfUtils.ExtractPdfTextByPath(@"D:\TestData.pdf");

                string folderPath = @"C:\Users\johny\Desktop\日常工作\Data";

                FDYF_Service fd = new FDYF_Service();
                var ret = fd.WeatherPdfToExcel(folderPath);

                DataSet ds = new DataSet();

                ds.Tables.Add(ret);

                ExcelUtils.WriteExcel(ds, @"D:\test.xlsx");
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        [TestMethod]
        public void DataCopy()
        {
            try
            {
                //ReadME.Test();
                //SpudcanCaculate sc = new SpudcanCaculate();

                var service = serviceProvider.GetRequiredService<DataCopyService>();
                service.CopyWind_TaskFileDeliver();


            }
            catch (Exception ex)
            {

                throw;
            }
        }

    }
}
