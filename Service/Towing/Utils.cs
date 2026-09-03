using Tool;
using Model.Base;
using System.Text;

namespace Service.Towing
{
    public class Utils
    {
        private static string LoginCookieKey = AppSettingUtils.GetSetting("AppSettings:Login:CookieKey");
        private static string LoginKey = AppSettingUtils.GetSetting("AppSettings:Login:Key");
        private static string LoginIV = AppSettingUtils.GetSetting("AppSettings:Login:IV");

        public static T CreateInstance<T>()
        {
            return Activator.CreateInstance<T>();
        }

        public static void CreateCookie(string cookieName, string cookieValue, string cookieDomain)
        {
            string base64Value = Convert.ToBase64String(Encoding.UTF8.GetBytes(cookieValue));
            CookieUtils.SetCookie(cookieName, base64Value, expires: DateTime.Now.AddHours(4));
        }

        public static void CleanUpCookie<T>()
        {
            CleanUpCookie(typeof(T).Name);
        }

        public static void CleanUpCookie(string cookieName = "")
        {
            // 注：HttpContext.Session.Abandon() 在 .NET 8 中需通过 IHttpContextAccessor 获取上下文
            if (!string.IsNullOrEmpty(cookieName))
            {
                CookieUtils.DeleteCookie(cookieName);
            }
        }

        public static void SaveCookies<T>(T obj, string cookieKey = "", bool urlEncode = false) where T : class
        {
            if (string.IsNullOrEmpty(cookieKey))
            {
                cookieKey = typeof(T).Name;
            }
            CleanUpCookie(cookieKey);
            var str = JsonUtils.Serialize(obj);
            string cookieValue = CryptographyUtils.TripleDESEncrypt(
                urlEncode ? System.Net.WebUtility.UrlEncode(str) : str, LoginKey, LoginIV);
            CreateCookie(cookieKey, cookieValue, "");
        }

        public static T GetCookies<T>(string cookeyKey = "", bool urlDecode = false) where T : class
        {
            T obj = CreateInstance<T>();
            try
            {
                if (string.IsNullOrEmpty(cookeyKey))
                {
                    cookeyKey = typeof(T).Name;
                }
                string base64Value = CookieUtils.GetCookie(cookeyKey);
                if (base64Value == null)
                {
                    return null;
                }

                string @string = Encoding.UTF8.GetString(Convert.FromBase64String(base64Value));
                string text = CryptographyUtils.TripleDESDecrypt(@string, LoginKey, LoginIV);
                if (urlDecode)
                {
                    text = System.Net.WebUtility.UrlDecode(text);
                }
                if (string.IsNullOrWhiteSpace(text))
                {
                    return null;
                }

                obj = JsonUtils.Deserialize<T>(text);
            }
            catch (Exception)
            {
                return null;
            }

            return obj;
        }

        public static UserLoginInfo GetLoginInfo(bool urlEncode = false)
        {
            return GetCookies<UserLoginInfo>(urlDecode: urlEncode);
        }

        public static void SetLoginInfo(UserLoginInfo loginInfo, bool urlEncode = false)
        {
            SaveCookies(loginInfo, urlEncode: urlEncode);
        }

        /// <summary>
        /// 从列表中均匀抽取指定数量的点
        /// </summary>
        /// <param name="sourceList">原始列表</param>
        /// <param name="count">需要抽取的数量</param>
        /// <returns>均匀抽取的结果</returns>
        public static List<T> GetUniformSample<T>(List<T> sourceList, int count)
        {
            if (sourceList == null || sourceList.Count == 0)
            {
                return new List<T>();
            }

            int originalCount = sourceList.Count;
            if (originalCount <= count || count == 0)
            {
                return sourceList; // 如果列表长度小于等于所需数量，直接返回原列表
            }

            List<T> result = new List<T>();
            double step = (double)originalCount / count;

            int index = 0;
            for (int i = 0; i < count; i++)
            {
                index = (int)Math.Floor(i * step); // 计算每个点的位置
                result.Add(sourceList[index]);
            }
            // 若当前节点最后一个未添加，则应添加至结果集
            if ((index + 1) < sourceList.Count)
            {
                result.Add(sourceList.Last());
            }

            return result;
        }

        /// <summary>
        /// 使用Haversine公式计算两个经纬度之间的海里数
        /// </summary>
        public static double CalculateNauticalMiles(double lat1, double lon1, double lat2, double lon2)
        {
            const double KmToNauticalMiles = 0.539956803; // 1千米 = 0.539956803海里
            double distanceKm = CalculateKms(lat1, lon1, lat2, lon2);
            return distanceKm * KmToNauticalMiles;
        }

        public static decimal CalculateNauticalMiles(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
            => (decimal)CalculateNauticalMiles((double)lat1, (double)lon1, (double)lat2, (double)lon2);

        /// <summary>
        /// 使用Haversine公式计算两个经纬度之间的公里数
        /// </summary>
        public static double CalculateKms(double lat1, double lon1, double lat2, double lon2)
        {
            const double EarthRadiusKm = 6371.0; // 地球半径（千米）

            // 将经纬度转换为弧度
            double phi1 = DegreesToRadians(lat1);
            double phi2 = DegreesToRadians(lat2);
            double deltaPhi = DegreesToRadians(lat2 - lat1);
            double deltaLambda = DegreesToRadians(lon2 - lon1);

            // Haversine公式
            double a = Math.Pow(Math.Sin(deltaPhi / 2), 2) +
                       Math.Cos(phi1) * Math.Cos(phi2) *
                       Math.Pow(Math.Sin(deltaLambda / 2), 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double distanceKm = EarthRadiusKm * c;

            return distanceKm;
        }

        public static decimal CalculateKms(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
            => (decimal)CalculateKms((double)lat1, (double)lon1, (double)lat2, (double)lon2);

        /// <summary>
        /// 将角度转换为弧度
        /// </summary>
        public static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        /// <summary>
        /// 地图16等分方位指向，顺时针
        /// </summary>
        public static List<string> DirectionMap16 = new List<string> { "N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE", "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW" };

        public static string GetDirectionStr16(decimal degree)
        {
            // 将角度归一化到 [0, 360) 范围内
            degree = degree % 360;
            if (degree < 0) degree += 360;
            // 计算方位索引
            decimal intervalSize = 22.5m; // 每个区间的大小
            int index = (int)((degree + 11.25m) / intervalSize) % 16;
            return DirectionMap16[index];
        }

        public static string getStr(double? val, int round = 0)
            => getStr((decimal)val, round);

        public static string getStr(decimal? val, int round = 0)
        {
            if (val == null) return "-";
            else if (round > 0) return Math.Round(val.Value, round).ToString();
            else return val.Value.ToString();
        }
    }


    /// <summary>
    /// 经纬度坐标（全 decimal 类型）
    /// </summary>
    public class Location
    {
        /// <summary>
        /// 经度
        /// </summary>
        public decimal Lon { get; set; }

        /// <summary>
        /// 纬度
        /// </summary>
        public decimal Lat { get; set; }

        public Location(decimal lon, decimal lat)
        {
            Lon = lon;
            Lat = lat;
        }

        public Location() { }
    }


    public static class NavigationCalculator
    {
        private const double EarthRadiusMeters = 6371000;
        private const double MetersPerNauticalMile = 1852;


        //------------------------------------------------------
        // 【1】已航行里程
        // 逻辑：起点 → 最近点 → 当前位置
        //------------------------------------------------------
        public static decimal CalculateTraveledNauticalMiles(List<Location> plannedPath, Location currentLocation)
        {
            if (plannedPath == null || plannedPath.Count == 0 || currentLocation == null)
                return 0;

            // 找到最近点索引
            int nearestIndex = FindNearestPointIndex(plannedPath, currentLocation);

            // 取出：起点 → 最近点
            var traveledPlanned = plannedPath.Take(nearestIndex + 1).ToList();

            // 拼接路线：起点 → ... → 最近点 → 当前位置
            var traveledPath = new List<Location>(traveledPlanned);
            traveledPath.Add(currentLocation);

            // 计算总海里
            return TotalDistanceNauticalMiles(traveledPath);
        }


        /// <summary>
        /// 计算剩余航程（输入：计划路线 + 当前位置，输出：剩余海里）
        /// </summary>
        public static decimal CalculateRemainingNauticalMiles(List<Location> plannedPath, Location currentLocation)
        {
            if (plannedPath == null || plannedPath.Count == 0 || currentLocation == null)
                return 0;

            // 找到最近点索引
            int nearestIndex = FindNearestPointIndex(plannedPath, currentLocation);

            // 剩余计划点
            var remainingPlanned = plannedPath.Skip(nearestIndex).ToList();

            // 构造剩余路线：当前位置 → 最近点 → ... → 终点
            var remainingPath = new List<Location> { currentLocation };
            remainingPath.AddRange(remainingPlanned);

            // 计算总海里
            return TotalDistanceNauticalMiles(remainingPath);
        }

        /// <summary>
        /// 计算整条轨迹总海里
        /// </summary>
        public static decimal TotalDistanceNauticalMiles(List<Location> path)
        {
            if (path == null || path.Count < 2) return 0;

            decimal total = 0;
            for (int i = 0; i < path.Count - 1; i++)
            {
                total += DistanceNauticalMiles(path[i], path[i + 1]);
            }
            return total;
        }

        /// <summary>
        /// 两点之间距离（海里）
        /// </summary>
        public static decimal DistanceNauticalMiles(Location a, Location b)
        {
            double meters = HaversineDistanceMeters(
                (double)a.Lon, (double)a.Lat,
                (double)b.Lon, (double)b.Lat);

            return (decimal)(meters / MetersPerNauticalMile);
        }

        /// <summary>
        /// 找到离当前位置最近的点索引
        /// </summary>
        private static int FindNearestPointIndex(List<Location> plannedPath, Location current)
        {
            int index = 0;
            double min = double.MaxValue;

            for (int i = 0; i < plannedPath.Count; i++)
            {
                double dist = HaversineDistanceMeters(
                    (double)current.Lon, (double)current.Lat,
                    (double)plannedPath[i].Lon, (double)plannedPath[i].Lat);

                if (dist < min)
                {
                    min = dist;
                    index = i;
                }
            }
            return index;
        }

        // Haversine 公式计算（内部用 double 计算，不影响外部类型）
        private static double HaversineDistanceMeters(double lon1, double lat1, double lon2, double lat2)
        {
            double rLat1 = ToRadians(lat1);
            double rLat2 = ToRadians(lat2);
            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(rLat1) * Math.Cos(rLat2) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return EarthRadiusMeters * c;
        }

        private static double ToRadians(double angle)
        {
            return angle * Math.PI / 180;
        }


        /// <summary>
        /// 计算从点A到点B的航行方位角
        /// 规则：0=正北，90=正东，180=正南，270=正西
        /// </summary>
        public static decimal CalculateBearing(Location pointA, Location pointB)
        {
            if (pointA == null || pointB == null)
                return 0m;

            // 转 double 计算
            double lat1 = (double)pointA.Lat;
            double lon1 = (double)pointA.Lon;
            double lat2 = (double)pointB.Lat;
            double lon2 = (double)pointB.Lon;

            // 角度转弧度
            double rLat1 = ToRadians(lat1);
            double rLat2 = ToRadians(lat2);
            double rDeltaLon = ToRadians(lon2 - lon1);

            // 方位角计算公式
            double y = Math.Sin(rDeltaLon) * Math.Cos(rLat2);
            double x = Math.Cos(rLat1) * Math.Sin(rLat2) -
                       Math.Sin(rLat1) * Math.Cos(rLat2) * Math.Cos(rDeltaLon);
            double radians = Math.Atan2(y, x);

            // 弧度转角度
            double degrees = ToDegrees(radians);

            // 转为 0-360° 航海方位角
            double bearing = (degrees + 360) % 360;

            // 转 decimal 并保留0位小数
            return Math.Round((decimal)bearing, 0);
        }

        /// <summary>
        /// 弧度 转 角度
        /// </summary>
        private static double ToDegrees(double radians)
        {
            return radians * 180 / Math.PI;
        }
    }
}
