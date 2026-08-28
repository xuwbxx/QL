namespace Model.Tech.Cloud
{
    public class CloudWindLibraryShip
    {
        public int ID { set; get; }

        public string ShipName { set; get; }

        /// <summary>
        /// 桩腿直径
        /// </summary>
        public string ZTZJ { set; get; }

        /// <summary>
        /// 桩腿截面积
        /// </summary>
        public string ZTJMJ { set; get; }

        /// <summary>
        /// 桩腿周长
        /// </summary>
        public string ZTZC { set; get; }

        /// <summary>
        /// 桩靴长度
        /// </summary>
        public string ZXCD { set; get; }

        /// <summary>
        /// 桩靴宽度
        /// </summary>
        public string ZXKD { set; get; }

        /// <summary>
        /// 桩靴高度
        /// </summary>
        public string ZXGD { set; get; }

        /// <summary>
        /// 桩靴面积
        /// </summary>
        public string ZXMJ { set; get; }

        /// <summary>
        /// 桩靴最大截面周长
        /// </summary>
        public string ZXZDJMZC { set; get; }

        /// <summary>
        /// 桩靴体积
        /// </summary>
        public string ZXTJ { set; get; }

        /// <summary>
        /// 桩腿、桩靴自重
        /// </summary>
        public string ZTZXZZ { set; get; }

        /// <summary>
        /// 桩腿预压力
        /// </summary>
        public string ZTYYL { set; get; }

        /// <summary>
        /// 计算预压荷载
        /// </summary>
        public string JSYYHZ { set; get; }

        /// <summary>
        /// 拔桩力
        /// </summary>
        public string BZL { set; get; }

        /// <summary>
        /// 对地比压
        /// </summary>
        public string DDBY { set; get; }

        /// <summary>
        /// 有效桩腿长度(船底到靴底)
        /// </summary>
        public string YXZTCD { set; get; }

        /// <summary>
        /// 气隙(船底到水面)
        /// </summary>
        public string QX { set; get; }

        /// <summary>
        /// 桩腿有效长度
        /// </summary>
        public string ZTYXCD { set; get; }


        public bool IsConfirm { set; get; }
    }
}
