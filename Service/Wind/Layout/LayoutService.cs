using Service.Base;

namespace Service.Wind.Layout
{
    public interface IBoardHeaderService
    {
        (string Name, string FirstName, string DepartName) GetBoardInfo();
    }

    public class BoardHeaderService : IBoardHeaderService
    {
        private readonly CookieService _cookieService;
        public BoardHeaderService(CookieService cookieService)
        {
            _cookieService = cookieService;
        }
        public (string Name, string FirstName, string DepartName) GetBoardInfo()
        {
            var user = _cookieService.GetUserCookie();
            if (user == null)
            {
                return ("", "无", "");
            }
            var name = user.RealName ?? "";
            var first = string.IsNullOrEmpty(name) ? "无" : name.Substring(0, 1);
            var depart = user.DepartName ?? "";
            return (name, first, depart);
        }
    }

}
