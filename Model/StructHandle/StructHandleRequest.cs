using Model.Base;

namespace Model.StructHandle
{
    public class StructHandleRequest : BaseRequest
    {
        public StructHandleRequest()
        {
            pointList = new List<int>();
            machineList = new List<int>();
        }

        public int ID { set; get; }

        public int AreaID { set; get; }

        public string? UserName { set; get; }

        public string? Password { set; get; }


        public string? areaNo { set; get; }
        public int? workColumn { set; get; }
        public int? workRow { set; get; }
        public double? startX { set; get; }
        public double? startY { set; get; }
        public double? endX { set; get; }
        public double? endY { set; get; }
        public double? topBiaoGao { set; get; }

        public double? bottomBiaoGao { set; get; }

        public double? soilGuanRuLiang { set; get; }

        public double? soilUseTotal { set; get; }

        public double? soilBiaoGao { set; get; }

        public string? startTime { set; get; }

        public string? endTime { set; get; }

        public List<int> pointList { set; get; }

        public List<int> machineList { set; get; }

    }
}
