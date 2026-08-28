using DataFactory.Factory;
using DataFactory.KingBase.CloudWind;
using Model.Base;

namespace Service.Wind
{
    public class LoginService
    {
        private readonly CloudWind_KingBase_UnitOfWorkFactory _windUowFactory;

        public LoginService(CloudWind_KingBase_UnitOfWorkFactory windCenterUowFactory)
        {
            _windUowFactory = windCenterUowFactory;
        }

        public void JJTLoginRecord(SHJUserInfo user)
        {
            using (var repo = _windUowFactory.Create())
            {
                var loginRecordRepo = repo.GetRepository<Base_LoginRecord>();

                Base_LoginRecord se = new Base_LoginRecord();
                se.UserName = user.UserName;
                se.UserCode = user.UserCode;
                se.Name = user.RealName;
                se.LoginResult = true;
                se.LoginTime = DateTime.Now;
                se.CreateTime = DateTime.Now;
                se.IsDelete = false;
                loginRecordRepo.Add(se);
                loginRecordRepo.Save();
            }
            return;
        }

    }
}
