using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tool;

namespace Business.Towing
{
    /// <summary>
    /// 警告基准值
    /// </summary>
    public class LimitValues
    {
        /// <summary>
        /// 默认拉取消息条数
        /// </summary>
        public static int MsgSize { set; get; } = 20;

        /// <summary>
        /// ECharts图表默认获取数据条数
        /// </summary>
        //public static int EChartDataSize { set; get; } = 300;
        public static int EChartDataSize = int.Parse(AppSettingUtils.GetSetting("THScan:EChartDataSize") ?? "0");

        /// <summary>
        /// 默认取卫星定位点的数量
        /// </summary>
        //public static int SatelltePointSize { set; get; } = 20;
        public static int SatelltePointSize = int.Parse(AppSettingUtils.GetSetting("THScan:SatelltePointSize") ?? "0");

        /// <summary>
        /// 默认设备失联时间，超出该时间则设置为【离线】
        /// </summary>
        //public static decimal DeviceOfflineSeconds { set; get; } = 5 * 60 * 1000;
        public static int DeviceOfflineSeconds = int.Parse(AppSettingUtils.GetSetting("THScan:DeviceOfflineSeconds") ?? "0");

        public static DateTime DeviceOfflineTime
        {
            get
            {
                //double offlineSeconds = (double)(0 - LimitValues.DeviceOfflineSeconds);
                //return DateTime.Now.AddSeconds(offlineSeconds);
                return DateTime.Now.AddDays(-129);
            }
        }

        /// <summary>
        /// 最大方位角
        /// </summary>
        public static decimal MaxAzAngle { set; get; }

        /// <summary>
        /// 最大横倾
        /// </summary>
        public static decimal MaxAngleX { set; get; }

        /// <summary>
        /// 最大纵倾
        /// </summary>
        public static decimal MaxAngleY { set; get; }

        public static decimal MaxDangX { set; get; }
        public static decimal MaxDangY { set; get; }
        public static decimal MaxDangZ { set; get; }
        public static decimal MaxYaoX { set; get; }
        public static decimal MaxYaoY { set; get; }
        public static decimal MaxYaoZ { set; get; }

        /// <summary>
        /// 最大风速
        /// </summary>
        public static decimal MaxWindSpeed { set; get; }
    }
}
