using Easy.EasyPlot;
using EasyFiniteElement.EasyStructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySpudcanCalculateClass
{
    public class SpudcanDB
    {
        private DataSet MydataSet;
        public EasyStructureKit StructureKit;
        public double MonitorScale = 1.0;
        private string ABoatBB = "1020"; // 多船模式下单船更新的版本号
        private string BB = "10004"; // 单船或多船模式下多船更新的版本号
        private static readonly string[] SelectTabNames = new string[]
        {
            "LS_Common",
            "LS_LegType",
            "LS_SpudcanType",
            "LS_SoilType",
            "LS_ExcelDrillingName",
            "LS_TempSoilDrilling",
            "LS_SoilDrillingParameter",
            "LS_Boat",
            "LS_StructureData",
            "LS_DeepType",
            "LS_TempDeepType1",
            "LS_TempDeepType2",
            "LS_ComputingModelType_Qv",
            "LS_ComputingModelType_Qb"
        };

        public SpudcanDB(EasyStructureKit structureKit, bool createTable, bool boats = true)
        {
            this.MydataSet = structureKit.StructureData.GetData();
            this.StructureKit = structureKit;

            if (createTable)
            {
                //CreateDatabase(this.StructureKit, boats);
            }
        }

        // 这里需要添加 CreateDatabase 方法的实现
        //public void OpenFile(string FileName)
        //{
        //    StructureKit.OpenFile(FileName);
        //    MydataSet = StructureKit.StructureData.GetData();
        //    UpdateData(MydataSet.Tables.Contains("LS_Boat"));
        //}

        public static string[] GetNotResultTabNames()
        {
            return SelectTabNames.Concat(new string[] { "LS_CalculationParameter", "LS_StructureData" }).ToArray();
        }
    }
}
