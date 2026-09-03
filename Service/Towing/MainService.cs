using DataFactory.Factory;
using Model.Towing;
using Tool;

namespace Service.Towing
{
    public class MainServiceResult
    {
        public bool Success { set; get; }
        public string? ErrorText { set; get; }
        public string? RedirectUrl { set; get; }
        public string? PXYKey { set; get; }
        public string? LoginInfoJson { set; get; }
        public string? FlashTimeJson { set; get; }
    }

    public class MainService
    {
        private readonly Towing_KingBase_UnitOfWorkFactory _towingKingBaseUowFactory;

        public MainService(Towing_KingBase_UnitOfWorkFactory towingKingBaseUowFactory)
        {
            _towingKingBaseUowFactory = towingKingBaseUowFactory;
        }

        /// <summary>
        /// Index 页面：需要用户已确认（IsConfirm）
        /// </summary>
        public MainServiceResult PrepareIndex(string? userCode, string? realName, string? userName)
        {
            var result = new MainServiceResult();
            if (string.IsNullOrEmpty(userCode))
            {
                result.Success = false;
                result.RedirectUrl = "/Cloud/CloudBase/ErrorPage?ErrorText=用户信息失效，请重新从云平台登录";
                return result;
            }

            using (var uow = _towingKingBaseUowFactory.Create())
            {
                var userRepo = uow.GetRepository<DataFactory.KingBase.Towing.Manage_User>();
                var user = userRepo.FindAll()
                    .FirstOrDefault(a => !a.IsDelete && a.UserName.Equals(userCode) && a.IsConfirm == true);

                if (user == null)
                {
                    result.Success = false;
                    result.RedirectUrl = "/Towing/Manage/Register";
                    return result;
                }

                RecordLoginLog(uow, userCode, realName, userName);

                result.Success = true;
                result.PXYKey = AppSettingUtils.GetSetting("PXY:PXYKey") ?? "";
                result.LoginInfoJson = BuildLoginInfoJson(userCode, realName, userName, user.Role ?? 0);
                result.FlashTimeJson = BuildFlashTimeJson(isPad: false);
            }

            return result;
        }

        /// <summary>
        /// Pad 页面：仅需用户存在
        /// </summary>
        public MainServiceResult Prepare_Pad(string? userCode, string? realName, string? userName)
        {
            var result = new MainServiceResult();
            if (string.IsNullOrEmpty(userCode))
            {
                result.Success = false;
                result.RedirectUrl = "/Cloud/CloudBase/ErrorPage?ErrorText=用户信息失效，请重新从云平台登录";
                return result;
            }

            using (var uow = _towingKingBaseUowFactory.Create())
            {
                var userRepo = uow.GetRepository<DataFactory.KingBase.Towing.Manage_User>();
                var user = userRepo.FindAll()
                    .FirstOrDefault(a => !a.IsDelete && a.UserName.Equals(userCode));

                if (user == null)
                {
                    result.Success = false;
                    result.RedirectUrl = "/Cloud/CloudBase/ErrorPage?ErrorText=您没有权限登录，请申请注册";
                    return result;
                }

                RecordLoginLog(uow, userCode, realName, userName);

                result.Success = true;
                result.PXYKey = AppSettingUtils.GetSetting("PXY:PXYKey") ?? "";
                result.LoginInfoJson = BuildLoginInfoJson(userCode, realName, userName, user.Role ?? 0);
                result.FlashTimeJson = BuildFlashTimeJson(isPad: true);
            }

            return result;
        }

        private void RecordLoginLog(IUnitOfWork uow, string? userCode, string? realName, string? userName)
        {
            var loginLogRepo = uow.GetRepository<DataFactory.KingBase.Towing.Manage_LoginLog>();
            var loginData = new DataFactory.KingBase.Towing.Manage_LoginLog();
            loginData.UserCode = userCode;
            loginData.RealName = realName;
            loginData.Depart = userName ?? "";
            loginData.Job = "";
            loginData.CreateTime = DateTime.UtcNow;
            loginData.IsDelete = false;
            loginLogRepo.Add(loginData);
            uow.Save();
        }

        private string BuildLoginInfoJson(string? userCode, string? realName, string? userName, int roleID)
        {
            UserLoginInfo userInfo = new UserLoginInfo();
            userInfo.UserCode = userCode;
            userInfo.RealName = realName;
            userInfo.UserName = userName;
            userInfo.RoleID = roleID;
            return JsonUtils.Serialize(userInfo);
        }

        private string BuildFlashTimeJson(bool isPad)
        {
            TowingMonitorFlashTime time = new TowingMonitorFlashTime();
            time.fillLeftPanel = 30;
            time.fillRightPanel = 60;
            time.offlineSeconds = 300;
            time.loadMsgList = 60;
            time.fillChatPanel = 60;
            time.loadShipSurShips = 300;
            time.fillProgressPath = isPad ? 300 : 60;
            time.fillWeatherInfo = 240 * 60;
            time.fillWeatherForecost = 240 * 60;
            if (!isPad)
            {
                time.fillTyphoon = 60 * 30;
            }
            return JsonUtils.Serialize(time);
        }
    }
}
