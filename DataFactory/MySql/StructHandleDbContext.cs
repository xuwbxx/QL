using DataFactory.Factory;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataFactory.MySql
{
    public class StructHandleDbContext : BaseDbContext
    {
        public StructHandleDbContext(string connectionString, DatabaseType databaseType)
            : base(connectionString, databaseType) { }

        // SQL Server的实体定义（示例）
        //public DbSet<data_handle> data_handle { get; set; }
        public DbSet<manage_user> manage_user { get; set; }

        public DbSet<data_work_area> data_work_area { get; set; }

        public DbSet<data_handle> data_handle { get; set; }

        public DbSet<data_work_area_point> data_work_area_point { set; get; }

        public DbSet<view_data_handle_working_point> view_data_handle_working_point { set; get; }
    }

    //public class data_handle
    //{

    //}

    public class manage_user
    {
        public int ID { set; get; }

        public string? UserName { set; get; }

        public string? Password { set; get; }

        public DateTime CreateTime { set; get; }

        public int IsDelete { set; get; }
    }

    public class data_work_area
    {
        public int ID { set; get; }

        public string? AreaNo { set; get; }

        public int? WorkColumn { set; get; }

        public int? WorkRow { set; get; }

        public double? StartX { set; get; }

        public double? StartY { set; get; }

        public double? EndX { set; get; }

        public double? EndY { set; get; }

        public double? TopBiaoGao { set; get; }

        public double? BottomBiaoGao { set; get; }

        public double? SoilGuanRuLiang { set; get; }

        public double? SoilUseTotal { set; get; }

        public double? SoilBiaoGao { set; get; }

        public string? UserName { set; get; }

        public DateTime CreateTime { set; get; }

        public int IsDelete { set; get; }

        [NotMapped]
        public double? FinishRate { set; get; }

    }

    public class data_work_area_point
    {
        public int ID { set; get; }

        public int AreaID { set; get; }

        public string? PointName { set; get; }

        public string? PointArea { set; get; }

        public double? PointX { set; get; }

        public double? PointY { set; get; }

        public double? ZhuangDing { set; get; }

        public double? ZhuangDi { set; get; }

        public double? ShaMianBiaoGao { set; get; }

        public double? SheJiYongJiangLiang { set; get; }

        public double? SheJiZhuangChang { set; get; }


        public double? SoilGuanRuLiang { set; get; }

        public double? SoilUseTotal { set; get; }

        public double? SoilBiaoGao { set; get; }


        public DateTime CreateTime { set; get; }

        public int IsDelete { set; get; }

        public int Status { set; get; }

        public double? SoilGuanRuLiangAct { set; get; }

        public double? SoilUseTotalAct { set; get; }

        public DateTime? StartTime { set; get; }

        public DateTime? FinishTime { set; get; }

        public double? NiJiangBiZhong { set; get; }
        public double? NiJiangBiZhongAct { set; get; }
        public double? MeiMiYongJiangLiang { set; get; }

        public double? ShaCengZhuJiangTiJiBi { set; get; }

        public int? MachineID { set; get; }

        public double? LiuLiangNum1Act { set; get; }

        public double? LiuLiangNum2Act { set; get; }
        public double? YaLiNum1Act { set; get; }
        public double? YaLiNum2Act { set; get; }
        public double? DianLiuNum1Act { set; get; }
        public double? DianLiuNum2Act { set; get; }

        public double? PointXAct { set; get; }

        public double? PointYAct { set; get; }

        public double? TopBiaoGaoAct { set; get; }

        public double? BottomBiaoGaoAct { set; get; }

        public string? Remark { set; get; }

        [NotMapped]
        public string? AreaNo { set; get; }

        [NotMapped]
        public string? TopBiaoGao { set; get; }

        [NotMapped]
        public string? BottomBiaoGao { set; get; }

        [NotMapped]
        public string? FinishTimeStr { set; get; }

        [NotMapped]
        public string? StatusStr { set; get; }
    }


    public class data_handle
    {
        public int ID { set; get; }

        public DateTime WorkTime { set; get; }

        public int PointID { set; get; }

        public int WorkCount { set; get; }

        public int WorkStatus { set; get; }
    }

    public class view_data_handle_working_point
    {
        public int ID { set; get; }

        public string? PointName { set; get; }

        public string? AreaNo { set; get; }

        public int AreaID { set; get; }
    }

}
