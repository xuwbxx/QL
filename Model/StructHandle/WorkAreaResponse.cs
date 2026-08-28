namespace Model.StructHandle
{
    public class WorkAreaResponse
    {
        public WorkAreaResponse()
        {
            WorkAreaPoints = new List<WorkAreaPointResponse>();
            WorkAreas = new List<WorkAreaArea>();
            WorkingPoints = new List<WorkingPoint>();
        }

        public int ID { set; get; }

        public string? AreaNo { set; get; }

        public int WorkColumn { set; get; }

        public int WorkRow { set; get; }

        public string? StartX { set; get; }
        public string? StartY { set; get; }
        public string? EndX { set; get; }
        public string? EndY { set; get; }

        public string? Square { set; get; }

        public string? UserName { set; get; }

        public string? CreateTime1 { set; get; }
        public string? CreateTime2 { set; get; }

        public string? FinishRate { set; get; }

        public string? SoilGuanRuLiang { set; get; }

        public string? SoilBiaoGao { set; get; }

        public string? SoilUseTotal { set; get; }

        public string? TopBiaoGao { set; get; }

        public string? BottomBiaoGao { set; get; }

        public string? WorkingPoint { set; get; }

        public List<WorkingPoint> WorkingPoints { set; get; }

        public List<WorkAreaPointResponse> WorkAreaPoints { set; get; }

        public List<WorkAreaArea> WorkAreas { set; get; }
    }

    public class WorkingPoint
    {
        public int ID { set; get; }

        public int PointID { set; get; }
    }

    public class WorkAreaArea
    {
        public WorkAreaArea()
        {
            Points = new List<WorkAreaPointResponse>();
        }

        public string? AreaName { set; get; }

        public List<WorkAreaPointResponse> Points { set; get; }

    }

    public class WorkAreaPointResponse
    {
        public int ID { set; get; }

        public int AreaID { set; get; }

        public int Status { set; get; }

        public string? PointName { set; get; }

        public string? PointX { set; get; }

        public string? PointY { set; get; }

        public string? FinishTime { set; get; }

        public string? SoilGuanRuLiang { set; get; }

        public string? SoilUseTotal { set; get; }


        public string? SoilBiaoGao { set; get; }


        public string? TopBiaoGao { set; get; }

        public string? BottomBiaoGao { set; get; }


        public string? SoilGuanRuLiangAct { set; get; }

        public string? SoilUseTotalAct { set; get; }



        public string? ZhuangDing { set; get; }

        public string? ZhuangDi { set; get; }

        public string? ShaMianBiaoGao { set; get; }

        public string? SheJiYongJiangLiang { set; get; }

        public string? SheJiZhuangChang { set; get; }
    }


}
