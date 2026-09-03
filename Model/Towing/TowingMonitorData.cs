using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Towing
{
    public class TowingMonitorData
    {
        public string Time { set; get; }

        public string Message { set; get; }

        public string Code { set; get; }
    }

    public class TowingMonitorFlashTime
    {
        /// <summary>
        /// 左侧面板
        /// </summary>
        public decimal fillLeftPanel { set; get; }

        /// <summary>
        /// 右侧面板
        /// </summary>
        public decimal fillRightPanel { set; get; }

        /// <summary>
        /// 离线时间
        /// </summary>
        public decimal offlineSeconds { set; get; }

        /// <summary>
        /// 聊天记录
        /// </summary>
        public decimal fillChatPanel { set; get; }

        /// <summary>
        /// 气象
        /// </summary>
        public decimal fillWeatherForecost { set; get; }

        /// <summary>
        /// 实时路径
        /// </summary>
        public decimal fillProgressPath { set; get; }

        /// <summary>
        /// 天气信息
        /// </summary>
        public decimal fillWeatherInfo { set; get; }

        /// <summary>
        /// 聊天
        /// </summary>
        public decimal loadMsgList { set; get; }

        /// <summary>
        /// 周边船舶
        /// </summary>
        public decimal loadShipSurShips { set; get; }

        /// <summary>
        /// 台风预报
        /// </summary>
        public decimal fillTyphoon { set; get; }

    }

}
