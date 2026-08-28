namespace Model.Tech.Environment
{
    public class UserRight
    {
        public int ID { set; get; }

        /// <summary>
        /// 1：最高级
        /// </summary>
        public int DataRight { set; get; }

        /// <summary>
        /// 1:读写  2：只读
        /// </summary>
        public int ViewRight { set; get; }

        /// <summary>
        /// 1:管理员
        /// </summary>
        public int RoleID { set; get; }
    }
}
