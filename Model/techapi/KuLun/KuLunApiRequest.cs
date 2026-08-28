using Model.Base;

namespace Model.TechApi.KuLun
{

    public class WindCloudApiKuLunRequest : BaseApiRequest
    {
        public WindCloudApiKuLunRequest()
        {
            DataRequest = new KuLunRequest();
        }

        public KuLunRequest DataRequest { set; get; }
    }


    public class KuLunRequest
    {
        public KuLunRequest()
        {
            records = new List<KuLunRecord>();
        }

        /// <summary>
        /// 项目ID
        /// </summary>
        public int pid { set; get; }

        public List<KuLunRecord> records { set; get; }

    }



    public class KuLunRecord
    {
        public KuLunRecord()
        {
            datas = new List<KuLunRecordData>();
        }

        /// <summary>
        /// | type | String | 是 | 监测类型,类型不可以更改，如：地下水位、周边建筑物沉降、深层水平位移、支撑轴力 |
        /// </summary>
        public string type { set; get; }

        /// <summary>
        /// | datas | Array | 是 | 该类型下的监测数据列表  
        /// </summary>
        public List<KuLunRecordData> datas { set; get; }

    }

    public class KuLunRecordData
    {
        public KuLunRecordData()
        {
            multiData = new List<KuLunRecordMultiData>();
        }

        //必要字段
        /// <summary>
        /// | pointNumber | String | 是 | 测点编号（必须在系统中已存在） |
        /// </summary>
        public string pointNumber { set; get; }

        /// <summary>
        /// | time | String | 是 | 监测时间，格式：yyyy-MM-dd HH:mm:ss |
        /// </summary>
        public string time { set; get; }


        //扩展字段
        /// <summary>
        /// | value | Double | 否 | 监测值 单组数据时使用 |
        /// </summary>
        public double value { set; get; }

        /// <summary>
        /// | number | String | 否 | 编号    |
        /// </summary>
        public string number { set; get; }

        /// <summary>
        /// | groupNo | String | 否 | 组号    |
        /// </summary>
        public string groupNo { set; get; }

        /// <summary>
        /// | depth | Double | 否 | 深度    |
        /// </summary>
        public double depth { set; get; }

        public List<KuLunRecordMultiData> multiData { set; get; }
    }


    public class KuLunRecordMultiData
    {
        /// <summary>
        /// 类型
        /// </summary>
        public string type { set; get; }

        /// <summary>
        /// 数值
        /// </summary>
        public double value { set; get; }

        public double depth { set; get; }
    }
}
