using Easy;
using Easy.EasyTool;
using Easy.Structure;
using EasyFiniteElement.EasyStructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySpudcanCalculateClass
{
    public class SoilParameter
    {
        public string Name { get; set; }
        public int SoilID { get; set; }
        public SoilType SoilType { get; set; }
        public EsValue2s SuCurve { get; set; }
        public double Su0 { get; set; }
        public double DSu { get; set; }
        public double Phi { get; set; }
        public double Weight { get; set; }
        public double TopLevel { get; set; }
        public double BottomLevel { get; set; }
        public int SuInputType { get; set; }
        public double OnLegWeightReduceCoeff { get; set; }
        public double OnLegStrenthengReduceCoeff { get; set; }
        public double OnLegEReduceCoeff { get; set; }
        public double OnLegMuReduceCoeff { get; set; }


        public SoilParameter GetCopy()
        {
            SoilParameter ASoilParameter = new SoilParameter();
            ASoilParameter.SoilID = SoilID;
            ASoilParameter.SoilType = SoilType;
            ASoilParameter.SuCurve = SuCurve;
            ASoilParameter.Su0 = Su0;           // 修正：去掉多余的 = 
            ASoilParameter.DSu = DSu;
            ASoilParameter.Phi = Phi;
            ASoilParameter.Weight = Weight;
            ASoilParameter.TopLevel = TopLevel;
            ASoilParameter.BottomLevel = BottomLevel;
            ASoilParameter.SuInputType = SuInputType;

            ASoilParameter.OnLegWeightReduceCoeff = OnLegWeightReduceCoeff;
            ASoilParameter.OnLegStrenthengReduceCoeff = OnLegStrenthengReduceCoeff;
            ASoilParameter.OnLegEReduceCoeff = OnLegEReduceCoeff;
            ASoilParameter.OnLegMuReduceCoeff = OnLegMuReduceCoeff;
            ASoilParameter.Name = Name;

            return ASoilParameter;
        }

        public SoilParameter()
        {
            SoilType = SoilType.Clay;
            SuCurve = new EsValue2s();
            Phi = 0;
            TopLevel = 0;
            BottomLevel = 0;
            SuInputType = 1;
            Name = "";
        }

        public double GetSu(double Level)
        {
            if (SuInputType == 1)
            {
                return Su0 + DSu * (TopLevel - Level);
            }
            else
            {
                return SuCurve.GetValue(Level);
            }
        }

        public double GetSu0()
        {
            if (SuInputType == 1)
            {
                return Su0;
            }
            else
            {
                if (SuCurve.Values.Count > 0)
                {
                    return SuCurve.Values[0].V2;
                }
                else
                {
                    return 0;
                }
            }
        }

        public double GetDSu()
        {
            if (SuInputType == 1)
            {
                return DSu;
            }
            else
            {
                if (SuCurve.Values.Count > 1)
                {
                    return (SuCurve.Values[SuCurve.Values.Count - 1].V2 - SuCurve.Values[0].V2)
                           / (SuCurve.Values[SuCurve.Values.Count - 1].V1 - SuCurve.Values[0].V1);
                }
                else
                {
                    return 0;
                }
            }
        }

    }

    public enum SoilType
    {
        Clay = 0,
        Sand = 1,
        Both = 2
    }

    public class LegParameter
    {
        public int Type { get; set; }
        public double Circumference { get; set; }
        public double Diameter { get; set; }
        public double Area { get; set; }
        //public double Weight { get; set; }
        //public double Volume0 { get; set; }
        //public double Volume { get; set; }
        //public double TopLevel { get; set; }

        public LegParameter()  // 请将 ConstructorName 替换为实际的类名
        {
            Circumference = 0;
            Diameter = 0;
            Area = 0;
            //Weight = 0;
            //Volume = 0;
            //Volume0 = 0;
            //TopLevel = 0;
        }
    }

    public class SpudcanParameter
    {
        public int Type;
        public int ShapeType; // 0 为圆形，1为方形
        public double Circumference;
        public double Diameter;
        public double Weight;
        public double Area;
        public double L;
        public double B;
        public double Volume;
        public double Ht;
        public double B1;
        public double H1;
        public double H2;
        public double H3;
        public double H4;
        public double L1;
        public double L2;

        public SpudcanParameter()
        {
            Circumference = 0;
            Diameter = 0;
            Weight = 0;
            Area = 0;
            L = 0;
            Volume = 0;
            Ht = 0;
        }

        public double GetSpudcanB()
        {
            return ShapeType == 0 ? Diameter : Math.Min(L, B);
        }

        public double GetVd()
        {
            // VD--桩靴与土体接触部分的最大承载截面以下的桩靴体积
            double Vd;
            double A, ADown, VDown;

            switch (ShapeType)
            {
                case 0:
                    A = Math.PI * Math.Pow(B / 2, 2);
                    ADown = Math.PI * Math.Pow(L2 / 2, 2);
                    break;
                case 1:
                    A = B * L;
                    ADown = A * Math.Pow(L2 / L, 2);
                    break;
                default:
                    A = 0;
                    ADown = 0;
                    break;
            }

            if (ADown == 0)
            {
                VDown = 0;
            }
            else
            {
                VDown = H2 * (ADown + A + Math.Pow(ADown * A, 0.5)) / 3;
            }

            Vd = VDown;
            return Vd;
        }
    }


    public class CalculateParameter
    {
        public double DestinationLevel;
        public int NCalculatePoint;
        public int CalculationMethod;
        public double MeshSize;
        public int DPType;
        public bool KeepHistory;
        public double DCoeff;
        public bool IsBackFlow;
        public bool AutoGetHc;
        public double ftop;
        public double fbase;
        public double fleg;
        public double NBreakout;
        public double alpha;
        public double Hc;
        public double Hc2;
        public double CaculateL;
        public double CaculateH;
        public double fb; // 冲桩减阻系数fb
        public bool IsEquivalentToCircleSpudcan; // 是否等效为圆形桩靴，针对砂土
        public double UnderWaterPhiSubtractValue; // 砂土内摩擦角降低度数
        public double PressForce; // 计算预压荷载(t)，为桩腿预压力与桩腿、桩靴自重之和，同LS_Boat中的SumW

        public CalculateParameter()
        {
            UnderWaterPhiSubtractValue = 5;
            IsEquivalentToCircleSpudcan = true;  // 修正：原来是 1，改为 true
            DestinationLevel = 0;
            NCalculatePoint = 0;
            CalculationMethod = 0;
            MeshSize = 0;
            DPType = 0;
            KeepHistory = true;
            DCoeff = 0;
            IsBackFlow = false;
            AutoGetHc = false;
            ftop = 1;
            fbase = 1;
            fleg = 0;
            NBreakout = 1;
            alpha = 1;
            Hc = 0;
            Hc2 = 0;
            CaculateL = 40;
            CaculateH = 20;
            fb = 1;
            PressForce = 4500;
        }
    }

    public class LegShape
    {
        public int MaterialID { get; set; }
        public double B1 { get; set; }
        public double B2 { get; set; }
        public double B3 { get; set; }
        public double H1 { get; set; }
        public double H2 { get; set; }
        public double H3 { get; set; }
        public double H4 { get; set; }

        public LegShape()
        {
            MaterialID = 1000;
            B1 = 0;
            B2 = 0;
            B3 = 0;
            H1 = 0;
            H2 = 0;
            H3 = 0;
            H4 = 0;
        }

        public EsTLArea GetArea(double TopLevel, double BottomLevel)
        {
            // 一半区域（轴对称模型）
            EsTLArea soilArea = new EsTLArea();
            soilArea.Propertys.Add(new EsTLProperty(1, MaterialID));
            soilArea.Type = 1; // 表示腿区域

            // 添加顶点（从底部到顶部，逆时针方向）
            // 底部中心点
            soilArea.Points.Add(new EsTLPoint2D(0, 0, BottomLevel));

            // 桩靴底部（如果 B3 > 0）
            if (B3 > 0)
            {
                soilArea.Points.Add(new EsTLPoint2D(0, B3 * 0.5, BottomLevel + H1));
            }

            // 桩靴下部
            soilArea.Points.Add(new EsTLPoint2D(0, B3 * 0.5, BottomLevel + H1 + H3));

            // 桩靴底部最宽处
            soilArea.Points.Add(new EsTLPoint2D(0, B2 * 0.5, BottomLevel + H1 + H3 + H4));

            // 桩腿顶部
            soilArea.Points.Add(new EsTLPoint2D(0, B2 * 0.5, TopLevel));

            // 顶部中心点
            soilArea.Points.Add(new EsTLPoint2D(0, 0, TopLevel));

            return soilArea;
        }
    }

    public class AreaEdge : EsTLAreaEdge
    {
        public int ID { get; set; }
        public int N { get; set; }

        public AreaEdge(int id, EsTLEdge edge, int orientation)
        {
            ID = id;
            Edge = edge;
            Orientation = orientation;
            N = 0;
        }

        public AreaEdge()
        {
            ID = 0;
            Edge = new EsTLEdge();
            Orientation = 0;
            N = 0;
        }
    }


    public class SpudcanCaculate
    {
        public DataSet MyDataSet;
        public EasyStructureKit StructureKit;
        private List<string> WarningMessageList;
        private List<string> ErrorMessageList;

        public SpudcanCaculate(EasyStructureKit structureKit)  // 请替换为实际的类名
        {
            MyDataSet = structureKit.StructureData.GetData();
            StructureKit = structureKit;
            WarningMessageList = new List<string>();
            ErrorMessageList = new List<string>();
            EsMessageReporter.ReportMessage += ReportMessage;  // 事件订阅
        }

        //接收并处理消息报告，将警告和错误信息分别存入对应列表
        private void ReportMessage(string Message, EsMessageType MessageType)
        {
            if (MessageType == EsMessageType.Warning)
            {
                if (!WarningMessageList.Contains(Message))
                {
                    WarningMessageList.Add(Message);
                }
            }
            if (MessageType == EsMessageType.Error)
            {
                if (!ErrorMessageList.Contains(Message))
                {
                    ErrorMessageList.Add(Message);
                }
            }
        }

        //获取所有警告和错误信息，合并为字符串返回
        public string GetWarningAndErrorMessage()
        {
            string message = "";

            if (WarningMessageList.Count > 0)
            {
                message += "警告信息：";
                foreach (string wm in WarningMessageList)
                {
                    message += wm;
                }
            }

            if (ErrorMessageList.Count > 0)
            {
                message += "错误信息：";
                foreach (string em in ErrorMessageList)
                {
                    message += em;
                }
            }

            if (message != "")
            {
                message = message.Replace("\r\n", ";" + "\r");
                message = message.Remove(message.Length - 2, 2); // 移除最后的 \r 和 ;
                message += "。";
            }

            return message;
        }

        //将计算结果写入到船只数据集
        public void WriteResult(int BoatID, EasyStructureKit AStructureKit)
        {
            DataSet BoatsDataSet = AStructureKit.GetData();
            DataSet ABoatDataSet = this.MyDataSet;
            string[] NotResultTabNames = SpudcanDB.GetNotResultTabNames();

            foreach (DataTable ATable in BoatsDataSet.Tables)
            {
                if (!NotResultTabNames.Contains(ATable.TableName) && ATable.TableName.Contains("LS_"))
                {
                    // 删除该 BoatID 的现有数据
                    DataRow[] rowsToDelete = ATable.Select("BoatID=" + BoatID);
                    foreach (DataRow row in rowsToDelete)
                    {
                        ATable.Rows.Remove(row);
                    }

                    // 获取源表
                    DataTable TheTable = ABoatDataSet.Tables[ATable.TableName];

                    // 复制数据
                    foreach (DataRow Trow in TheTable.Rows)
                    {
                        DataRow NewRow = ATable.NewRow();
                        NewRow["BoatID"] = BoatID;

                        for (int i = 0; i < ATable.Columns.Count; i++)
                        {
                            for (int j = 0; j < TheTable.Columns.Count; j++)
                            {
                                if (TheTable.Columns[j].ColumnName == ATable.Columns[i].ColumnName)
                                {
                                    NewRow[i] = Trow[j];
                                    break;
                                }
                            }
                        }

                        ATable.Rows.Add(NewRow);
                    }
                }
            }
        }

        //计算并更新钻孔的计算高程列表
        public void ComputeLevels(ref string ErrorString, bool boats = true)
        {
            // 获取计算参数
            CalculateParameter calcParam = GetCaculateParameter();

            // 判断是否使用单一钻孔
            bool selectSingleDrilling = Convert.ToBoolean(MyDataSet.Tables["LS_Common"].Rows[0]["UseSingleDrilling"]);
            List<int> drillingIDs = new List<int>();

            if (selectSingleDrilling)
            {
                foreach (DataRow row in MyDataSet.Tables["LS_LegSoilLayer"].Rows)
                {
                    int drillingID = Convert.ToInt32(row["DrillingID"]);
                    if (!drillingIDs.Contains(drillingID))
                    {
                        drillingIDs.Add(drillingID);
                    }
                }
            }
            else
            {
                foreach (DataRow row in MyDataSet.Tables["LS_SoilDrilling"].Rows)
                {
                    int drillingID = Convert.ToInt32(row["ID"]);
                    if (!drillingIDs.Contains(drillingID))
                    {
                        drillingIDs.Add(drillingID);
                    }
                }
            }

            // 清除旧高程（排除当前钻孔ID）
            string filterString = "";
            for (int i = 0; i < drillingIDs.Count; i++)
            {
                filterString += "DrillingID<>" + drillingIDs[i] + (i == drillingIDs.Count - 1 ? "" : " and ");
            }

            DataRow[] rowsToDelete = MyDataSet.Tables["LS_CalculationLevels"].Select(filterString, "Level DESC");
            foreach (DataRow row in rowsToDelete)
            {
                MyDataSet.Tables["LS_CalculationLevels"].Rows.Remove(row);
            }

            // 遍历每个钻孔
            foreach (int drillingID in drillingIDs)
            {
                List<SoilParameter> soils = GetSoils(
                    calcParam.UnderWaterPhiSubtractValue,
                    drillingID,
                    selectSingleDrilling,
                    ref ErrorString,
                    boats
                );

                if (!string.IsNullOrEmpty(ErrorString))
                {
                    EsMessageReporter.ReportMessageFunction(ErrorString, EsMessageType.Error);
                    return;
                }

                // 计算高程列表
                double maxLevel = Math.Max(calcParam.DestinationLevel, soils[soils.Count - 1].TopLevel - 10);
                List<double> computeLevels = GetComputeLevels(
                    calcParam.NCalculatePoint,
                    soils,
                    soils[0].TopLevel,
                    maxLevel
                );

                // 检查当前计算模式是否一致
                bool selectCurrentComMode = true;
                List<double> oldComputeLevels = new List<double>();
                DataRow[] existingRows = MyDataSet.Tables["LS_CalculationLevels"].Select("DrillingID=" + drillingID, "Level DESC");

                if (computeLevels.Count != existingRows.Length)
                {
                    selectCurrentComMode = false;
                }
                else
                {
                    for (int i = 0; i < existingRows.Length; i++)
                    {
                        DataRow row = existingRows[i];
                        double level = Convert.ToDouble(row["Level"]);
                        oldComputeLevels.Add(level);

                        if (Math.Abs(level - Math.Round(computeLevels[i], 2)) > 0.0001)
                        {
                            selectCurrentComMode = false;
                            break;
                        }
                    }
                }

                // 如果模式不一致，重新写入高程
                if (!selectCurrentComMode)
                {
                    // 删除旧数据
                    DataRow[] rowsToDeleteForDrilling = MyDataSet.Tables["LS_CalculationLevels"].Select("DrillingID=" + drillingID, "Level DESC");
                    foreach (DataRow row in rowsToDeleteForDrilling)
                    {
                        MyDataSet.Tables["LS_CalculationLevels"].Rows.Remove(row);
                    }

                    // 写入新数据
                    for (int i = 0; i < computeLevels.Count; i++)
                    {
                        DataRow newRow = MyDataSet.Tables["LS_CalculationLevels"].NewRow();
                        newRow["DrillingID"] = drillingID;
                        newRow["LevelID"] = i + 1;
                        newRow["Level"] = Math.Round(computeLevels[i], 2);
                        newRow["SelectMode_Qv"] = 0;
                        newRow["SelectMode_Qb"] = 0;
                        MyDataSet.Tables["LS_CalculationLevels"].Rows.Add(newRow);
                    }
                }
            }

            MyDataSet.AcceptChanges();
        }

        //从数据库读取计算参数
        public CalculateParameter GetCaculateParameter()
        {
            CalculateParameter calcParam = new CalculateParameter();
            DataRow row = MyDataSet.Tables["LS_CalculationParameter"].Rows[0];

            calcParam.DestinationLevel = Convert.ToDouble(row["DestinationLevel"]);
            calcParam.NCalculatePoint = Convert.ToInt32(row["NCalculatePoint"]);
            calcParam.CalculationMethod = Convert.ToInt32(row["CalculationMethod"]);
            calcParam.MeshSize = Convert.ToDouble(row["MeshSize"]);
            calcParam.DPType = Convert.ToInt32(row["DPType"]);
            calcParam.DCoeff = Convert.ToDouble(row["DCoeff"]);
            calcParam.KeepHistory = Convert.ToBoolean(row["KeepHistory"]);
            calcParam.IsBackFlow = Convert.ToBoolean(row["IsBackFlow"]);
            calcParam.AutoGetHc = Convert.ToBoolean(row["AutoGetHc"]);
            calcParam.Hc = Convert.ToDouble(row["Hc"]);
            calcParam.Hc2 = Convert.ToDouble(row["Hc2"]);
            calcParam.fbase = Convert.ToDouble(row["fbase"]);
            calcParam.ftop = Convert.ToDouble(row["ftop"]);
            calcParam.fleg = Convert.ToDouble(row["fleg"]);
            calcParam.NBreakout = Convert.ToDouble(row["NBreakout"]);
            calcParam.alpha = Convert.ToDouble(row["SoilCoarseCoeff"]);
            calcParam.fb = Convert.ToDouble(row["fb"]);
            calcParam.PressForce = Convert.ToDouble(row["PressForce"]);
            calcParam.IsEquivalentToCircleSpudcan = Convert.ToBoolean(row["IsEquivalentToCircleSpudcan"]);
            calcParam.UnderWaterPhiSubtractValue = Convert.ToDouble(row["UnderWaterPhiSubtractValue"]);

            return calcParam;
        }

        //根据钻孔ID获取土层参数列表（支持单/多钻孔模式）
        public List<SoilParameter> GetSoils(double UnderWaterPhiSubtractValue, int DrillingID, bool SelectSingleDrilling, ref string ErrorString, bool Boats = false)
        {
            List<SoilParameter> soils = new List<SoilParameter>();

            // 获取 SuInputType
            int suInputType = Convert.ToInt32(MyDataSet.Tables["LS_Common"].Rows[0]["SuInputType"]);

            // 获取土层行
            DataRow[] rows;
            if (SelectSingleDrilling)
            {
                rows = MyDataSet.Tables["LS_LegSoilLayer"].Select();
            }
            else
            {
                rows = MyDataSet.Tables["LS_SoilDrilling"].Select("ID=" + DrillingID, "ID");
            }

            Dictionary<double, int> soilLevelIDDic = new Dictionary<double, int>();
            string tempErrorString = "";
            string soilFilterString = Boats ? " And DrillingID=" + DrillingID : "";
            string drillingName = "";

            if (SelectSingleDrilling)
            {
                drillingName = rows[0]["DrillingName"].ToString();
                foreach (DataRow row in rows)
                {
                    double topLevel = Convert.ToDouble(row["TopLevel"]);
                    int soilID = Convert.ToInt32(row["SoilID"]);
                    soilLevelIDDic.Add(topLevel, soilID);
                }
            }
            else
            {
                drillingName = rows[0]["Name"].ToString();
                string[] soilLayers = rows[0]["SoilLayers"].ToString().Split(';');

                foreach (string layer in soilLayers)
                {
                    string[] parts = layer.Split(',');
                    string soilName = parts[0];
                    double topLevel = Convert.ToDouble(parts[1]);

                    DataRow[] foundRows = MyDataSet.Tables["LS_Soil"].Select("Name='" + soilName + "'" + soilFilterString);

                    if (foundRows.Length == 0)
                    {
                        tempErrorString += "\"" + drillingName + "\"钻孔下的土层\"" + soilName + "\"在土层参数中未找到！" + Environment.NewLine;
                    }
                    else
                    {
                        int soilID = Convert.ToInt32(foundRows[0]["ID"]);
                        if (soilLevelIDDic.ContainsKey(topLevel))
                        {
                            tempErrorString += "\"" + drillingName + "\"钻孔下的土层\"" + soilName + "\"的标高重复！" + Environment.NewLine;
                        }
                        else
                        {
                            soilLevelIDDic.Add(topLevel, soilID);
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(tempErrorString))
            {
                ErrorString += tempErrorString;
                return soils;
            }

            int index = 0;
            foreach (double level in soilLevelIDDic.Keys)
            {
                index++;
                DataRow soilRow = MyDataSet.Tables["LS_Soil"].Select("ID=" + soilLevelIDDic[level] + soilFilterString)[0];

                SoilParameter soil = new SoilParameter();
                soil.Name = soilRow["Name"].ToString();
                soil.SoilID = soilLevelIDDic[level];
                soil.TopLevel = level;
                soil.SoilType = (SoilType)Convert.ToInt32(soilRow["Type"]);
                soil.SuCurve.SetString(soilRow["Su"].ToString());
                soil.SuInputType = suInputType;
                soil.Su0 = Convert.ToDouble(soilRow["Su0"]);
                soil.DSu = Convert.ToDouble(soilRow["DSu"]);
                soil.SuCurve.Reverse();
                soil.Weight = Convert.ToDouble(soilRow["UnderWaterWeight"]);

                // 计算内摩擦角
                if ((SoilType)Convert.ToInt32(soilRow["Type"]) == SoilType.Sand)
                {
                    double underWaterPhi = Convert.ToDouble(soilRow["UnderWaterPhi"]);
                    soil.Phi = (underWaterPhi - UnderWaterPhiSubtractValue) / 180.0 * Math.PI;
                }
                else
                {
                    double underWaterPhi = Convert.ToDouble(soilRow["UnderWaterPhi"]);
                    soil.Phi = underWaterPhi / 180.0 * Math.PI;
                }

                soil.BottomLevel = -10000;
                if (index > 1)
                {
                    soils[soils.Count - 1].BottomLevel = soil.TopLevel;
                }

                soils.Add(soil);

                // 验证砂土内摩擦角
                if (soil.SoilType == SoilType.Sand && (soil.Phi < 0 || soil.Phi >= 0.5 * Math.PI))
                {
                    ErrorString += "\"" + drillingName + "\"钻孔下的土层\"" + soilRow["Name"].ToString() + "\"的砂土内摩擦角未在范围内[0°,90°)" + Environment.NewLine;
                }
            }

            return soils;
        }

        //获取所有土层参数列表（单船模式）
        public List<SoilParameter> GetSoils(double UnderWaterPhiSubtractValue)
        {
            List<SoilParameter> soils = new List<SoilParameter>();
            int index = 0;

            DataRow[] rows = MyDataSet.Tables["LS_LegSoilLayer"].Select();
            int suInputType = Convert.ToInt32(MyDataSet.Tables["LS_Common"].Rows[0]["SuInputType"]);

            foreach (DataRow row in rows)
            {
                index++;

                DataRow soilRow = MyDataSet.Tables["LS_Soil"].Select("ID=" + row["SoilID"])[0];
                SoilParameter soil = new SoilParameter();

                soil.SoilID = Convert.ToInt32(row["SoilID"]);
                soil.SoilType = (SoilType)Convert.ToInt32(soilRow["Type"]);
                soil.SuCurve.SetString(soilRow["Su"].ToString());
                soil.SuInputType = suInputType;
                soil.Su0 = Convert.ToDouble(soilRow["Su0"]);
                soil.DSu = Convert.ToDouble(soilRow["DSu"]);
                soil.SuCurve.Reverse();
                soil.Weight = Convert.ToDouble(soilRow["UnderWaterWeight"]);

                // 计算内摩擦角
                if ((SoilType)Convert.ToInt32(soilRow["Type"]) == SoilType.Sand)
                {
                    soil.Phi = (Convert.ToDouble(soilRow["UnderWaterPhi"]) - UnderWaterPhiSubtractValue) / 180.0 * Math.PI;
                }
                else
                {
                    soil.Phi = Convert.ToDouble(soilRow["UnderWaterPhi"]) / 180.0 * Math.PI;
                }

                soil.TopLevel = Convert.ToDouble(row["TopLevel"]);
                soil.BottomLevel = -10000;

                // 读取折减系数
                soil.OnLegWeightReduceCoeff = Convert.ToDouble(soilRow["OnLegWeightReduceCoeff"]);
                soil.OnLegStrenthengReduceCoeff = Convert.ToDouble(soilRow["OnLegStrenthengReduceCoeff"]);
                soil.OnLegEReduceCoeff = Convert.ToDouble(soilRow["OnLegEReduceCoeff"]);
                soil.OnLegMuReduceCoeff = Convert.ToDouble(soilRow["OnLegMuReduceCoeff"]);

                // 设置上一个土层的底部标高
                if (index > 1)
                {
                    soils[soils.Count - 1].BottomLevel = soil.TopLevel;
                }

                soils.Add(soil);

                // 验证砂土内摩擦角
                if (soil.SoilType == SoilType.Sand && (soil.Phi < 0 || soil.Phi >= 0.5 * Math.PI))
                {
                    EsMessageReporter.ReportMessageFunction(
                        $"土层\"{soilRow["Name"]}\"的砂土内摩擦角未在范围内[0°,90°)",
                        EsMessageType.Warning
                    );
                }
            }

            return soils;
        }

        //根据土层和计算点数量生成计算高程列表（含加密逻辑）
        public List<double> GetComputeLevels(int NCalculatePoint, List<SoilParameter> Soils, double TopLevel, double BottomLevel, bool AddTopLevel = true)
        {
            List<double> computeLevels = new List<double>();
            List<double> tempComputeLevels = new List<double>();

            double dLevel0 = (TopLevel - BottomLevel) / (NCalculatePoint - 1);
            double currentTopLevel;

            if (AddTopLevel)
            {
                tempComputeLevels.Add(TopLevel);
            }

            for (int j = 0; j < Soils.Count; j++)
            {
                SoilParameter soil = Soils[j];

                if (soil.BottomLevel < TopLevel)
                {
                    currentTopLevel = Math.Min(TopLevel, soil.TopLevel);
                    int n = (int)Math.Max((currentTopLevel - Math.Max(BottomLevel, soil.BottomLevel)) / dLevel0, 1);
                    double dLevel = (currentTopLevel - Math.Max(BottomLevel, soil.BottomLevel)) / n;

                    if (dLevel > 0)
                    {
                        for (int i = 1; i <= n; i++)
                        {
                            double level = Math.Round(currentTopLevel - i * dLevel, 2);
                            tempComputeLevels.Add(level);
                        }
                    }

                    // 计算点加密（针对砂土或混合土的第一层，厚度大于1m时按1m加密）
                    if (j == 0 && (soil.SoilType == SoilType.Sand || soil.SoilType == SoilType.Both)
                        && soil.TopLevel - soil.BottomLevel > 1)
                    {
                        n = (int)Math.Floor(currentTopLevel - Math.Max(BottomLevel, soil.BottomLevel));
                        dLevel = 1;

                        if (n > 0)
                        {
                            for (int i = 1; i <= n; i++)
                            {
                                double level = Math.Round(currentTopLevel - i * dLevel, 2);
                                if (!tempComputeLevels.Contains(level))
                                {
                                    tempComputeLevels.Add(level);
                                }
                            }
                        }
                    }
                }
            }

            // 排序并反转（从大到小）
            tempComputeLevels.Sort();
            for (int i = tempComputeLevels.Count - 1; i >= 0; i--)
            {
                computeLevels.Add(tempComputeLevels[i]);
            }

            return computeLevels;
        }

        //计算指定深度范围内土层的加权平均参数
        public SoilParameter GetAverageSoilValue(List<SoilParameter> soils, double fromLevel, double toLevel)
        {
            SoilParameter sumSoil = new SoilParameter();
            double sumH = 0;

            foreach (SoilParameter aSoil in soils)
            {
                if (aSoil.BottomLevel < toLevel && aSoil.TopLevel > fromLevel)
                {
                    double h = Math.Min(aSoil.TopLevel, toLevel) - Math.Max(aSoil.BottomLevel, fromLevel);
                    sumH += h;

                    sumSoil.Phi += h * aSoil.Phi;
                    sumSoil.Weight += h * aSoil.Weight;

                    // 计算该层顶部和底部的 Su 平均值
                    double suTop = aSoil.GetSu(Math.Min(aSoil.TopLevel, toLevel));
                    double suBottom = aSoil.GetSu(Math.Max(aSoil.BottomLevel, fromLevel));
                    sumSoil.Su0 += h * (suTop + suBottom) / 2.0;
                }
            }

            if (Math.Abs(sumH) > 1e-9)  // 避免浮点数比较问题
            {
                sumSoil.Phi = sumSoil.Phi / sumH;
                sumSoil.Weight = sumSoil.Weight / sumH;
                sumSoil.Su0 = sumSoil.Su0 / sumH;
            }

            return sumSoil;
        }

        //计算指定深度范围内土层的加权平均参数（支持类型筛选和输出总厚度）
        public SoilParameter GetAverageSoilValue(List<SoilParameter> soils, double fromLevel, double toLevel, ref double sumH, int selectSoilType = 99)
        {
            SoilParameter sumSoil = new SoilParameter();
            sumH = 0;

            foreach (SoilParameter aSoil in soils)
            {
                if (aSoil.BottomLevel < toLevel && aSoil.TopLevel > fromLevel)
                {
                    // 筛选土层类型
                    if (selectSoilType == 99 ||
                        aSoil.SoilType == SoilType.Both ||
                        aSoil.SoilType == (SoilType)selectSoilType)
                    {
                        double h = Math.Min(aSoil.TopLevel, toLevel) - Math.Max(aSoil.BottomLevel, fromLevel);
                        sumH += h;

                        sumSoil.Phi += h * aSoil.Phi;
                        sumSoil.Weight += h * aSoil.Weight;

                        double suTop = aSoil.GetSu(Math.Min(aSoil.TopLevel, toLevel));
                        double suBottom = aSoil.GetSu(Math.Max(aSoil.BottomLevel, fromLevel));
                        sumSoil.Su0 += h * (suTop + suBottom) / 2.0;
                    }
                }
            }

            if (Math.Abs(sumH) > 1e-9)
            {
                sumSoil.Phi /= sumH;
                sumSoil.Weight /= sumH;
                sumSoil.Su0 /= sumH;
            }

            return sumSoil;
        }

        //计算极限洞深Hc
        public void GetHc(int drillingID, SpudcanParameter spudcanParameter, CalculateParameter calculateParameter, List<SoilParameter> soils)
        {
            double hc = 0;
            double spudcanB = spudcanParameter.GetSpudcanB();

            if (calculateParameter.AutoGetHc)
            {
                if (soils.Count == 1)
                {
                    hc = GetHc_SingleLayer(spudcanB, soils[0]);
                }
                else if (soils.Count > 1)
                {
                    hc = GetHc_MultiLayer(spudcanB, soils);
                }
                // 如果 soils.Count == 0，hc 保持为 0

                calculateParameter.Hc = hc;
            }

            DataRow newRow = MyDataSet.Tables["LS_Holl"].NewRow();
            newRow["DrillingID"] = drillingID;
            newRow["Hc"] = Math.Round(calculateParameter.Hc, 2);
            MyDataSet.Tables["LS_Holl"].Rows.Add(newRow);
        }

        //计算单层土的极限洞深
        public double GetHc_SingleLayer(double B, SoilParameter Soil)
        {
            double S = Math.Pow(Soil.GetSu0() / (Soil.Weight * B), 1 - Soil.GetDSu() / Soil.Weight);
            return B * (Math.Pow(S, 0.55) - 0.25 * S);
        }

        //迭代计算多层土的极限洞深
        public double GetHc_MultiLayer(double B, List<SoilParameter> Soils) // 迭代计算
        {
            // s(i,0)-距离顶面高度，s(i,1)-不排水强度，s(i,2)-重度
            // 计算深度H以上加权值
            double Hc0 = GetHc_SingleLayer(B, Soils[0]);
            double Hc1 = 0;
            double TopLevel = Soils[0].TopLevel;

            while (Math.Abs(Hc1 - Hc0) / Hc0 > 0.001)
            {
                SoilParameter Soil = GetAverageSoilValue(Soils, TopLevel - Hc0, TopLevel);
                double S = Soils[0].GetSu0() / (Soil.Weight * B);
                Hc1 = B * (Math.Pow(S, 0.55) - 0.25 * S);

                // 交换 Hc0 和 Hc1
                double temp = Hc0;
                Hc0 = Hc1;
                Hc1 = temp;
            }

            return Hc0;
        }

        //从数据库读取桩腿参数
        public LegParameter GetLegParameter()
        {
            LegParameter legParameter = new LegParameter();
            DataRow row = MyDataSet.Tables["LS_Leg"].Rows[0];

            legParameter.Circumference = Convert.ToDouble(row["Circumference"]);
            legParameter.Diameter = Convert.ToDouble(row["Diameter"]);
            legParameter.Area = Convert.ToDouble(row["Area"]);
            //legParameter.Volume = Convert.ToDouble(row["Volume"]);
            //legParameter.Volume0 = Convert.ToDouble(row["Volume0"]);
            //legParameter.TopLevel = Convert.ToDouble(row["TopLevel"]);
            //legParameter.Weight = Convert.ToDouble(row["Weight"]) - Convert.ToDouble(row["Volume"]) * WaterWeight;

            return legParameter;
        }

        //从数据库读取桩靴参数并解析参数字符串
        public SpudcanParameter GetSpudcanParameter()
        {
            SpudcanParameter spudcanParameter = new SpudcanParameter();
            DataRow row = MyDataSet.Tables["LS_Spudcan"].Rows[0];

            spudcanParameter.Area = Convert.ToDouble(row["Area"]);
            spudcanParameter.Circumference = Convert.ToDouble(row["Circumference"]);
            spudcanParameter.Diameter = Convert.ToDouble(row["Diameter"]);
            spudcanParameter.Volume = Convert.ToDouble(row["Volume"]);

            // 计算重量（kN），水密度1000kg/m3
            double volume = Convert.ToDouble(row["Volume"]);
            bool isSealed = Convert.ToBoolean(MyDataSet.Tables["LS_CalculationParameter"].Rows[0]["IsSealed"]);
            spudcanParameter.Weight = Convert.ToDouble(row["Weight"]) - 9.8 * (isSealed ? volume * 1 : 0);

            spudcanParameter.L = Convert.ToDouble(row["L"]);
            spudcanParameter.B = Convert.ToDouble(row["B"]);
            spudcanParameter.B1 = Convert.ToDouble(row["B"]);

            // 解析参数字符串
            string parametersString = row["Parameter"].ToString();
            string[] parameters = string.IsNullOrEmpty(parametersString)
                ? Array.Empty<string>()
                : parametersString.Split(',');

            double h1 = Convert.ToDouble(GetParameter(parameters, "H1"));
            double h2 = Convert.ToDouble(GetParameter(parameters, "H2"));
            double h3 = Convert.ToDouble(GetParameter(parameters, "H3"));

            spudcanParameter.Ht = h2;
            spudcanParameter.H1 = 0;
            spudcanParameter.H2 = h1;
            spudcanParameter.H3 = h2;
            spudcanParameter.H4 = h3;
            spudcanParameter.ShapeType = Convert.ToInt32(row["ShapeType"]);

            return spudcanParameter;
        }

        //从参数字符串数组中解析指定名称的参数值
        public static string GetParameter(string[] parameters, string parameterName)
        {
            if (parameters == null || parameters.Length == 0)
            {
                return "0";
            }

            foreach (string parameter in parameters)
            {
                if (parameter.Contains("="))
                {
                    string[] parts = parameter.Split('=');
                    if (parts.Length == 2 && parts[0].Trim() == parameterName)
                    {
                        return parts[1].Trim();
                    }
                }
            }

            return "0";
        }

        //判断标高以下B/2范围内是否存在3种及以上土层
        public bool GetIsDownSoilTypeExtra(double Level, double SpudcanB, List<SoilParameter> Soils)
        {
            List<int> typeList = new List<int>();

            for (int i = 0; i < Soils.Count; i++)
            {
                if (Soils[i].BottomLevel < Level && Soils[i].TopLevel > Level - 0.5 * SpudcanB)
                {
                    if (!typeList.Contains((int)Soils[i].SoilType))
                    {
                        typeList.Add((int)Soils[i].SoilType);
                    }
                }
            }

            return typeList.Count > 2;
        }

        //查找当前标高以下下一个砂土层的底部标高
        public double GetDownSandLayersNextLevel(double Level, List<SoilParameter> Soils, ref bool MergeSandSoil)
        {
            double sandNextLevel = 1e10; // 10^10

            for (int i = 0; i < Soils.Count; i++)
            {
                if (Soils[i].TopLevel == Level && Soils[i].SoilType == SoilType.Clay)
                {
                    return sandNextLevel;
                }

                if (Soils[i].BottomLevel < Level)
                {
                    if (Soils[i].SoilType == SoilType.Clay)
                    {
                        return sandNextLevel;
                    }
                    else
                    {
                        sandNextLevel = Soils[i].BottomLevel;
                        MergeSandSoil = true;
                    }
                }
            }

            return sandNextLevel;
        }

        //判断指定标高以上的土层是否均为同一类型
        public bool GetIsSameUpSoilType(double Level, List<SoilParameter> Soils)
        {
            List<int> typeList = new List<int>();

            for (int i = 0; i < Soils.Count; i++)
            {
                if (Soils[i].TopLevel > Level)
                {
                    if (!typeList.Contains((int)Soils[i].SoilType))
                    {
                        typeList.Add((int)Soils[i].SoilType);
                    }
                }
            }

            return typeList.Count <= 1;
        }

        //根据标高查找对应的土层
        public SoilParameter GetSoil(double Level, List<SoilParameter> Soils)
        {
            if (Soils[0].TopLevel < Level)
            {
                return Soils[0];
            }

            int index = Soils.Count - 1;

            for (int i = 0; i < Soils.Count; i++)
            {
                if (Soils[i].TopLevel >= Level && Soils[i].BottomLevel < Level)
                {
                    index = i;
                    break;
                }
            }

            return Soils[index];
        }

        //计算指定标高以上的上覆土压力
        public double GetP0(double Level, List<SoilParameter> Soils)
        {
            double p = 0;

            foreach (SoilParameter aSoil in Soils)
            {
                if (aSoil.TopLevel > Level)
                {
                    if (aSoil.BottomLevel > Level)
                    {
                        p += (aSoil.TopLevel - aSoil.BottomLevel) * aSoil.Weight;
                    }
                    else
                    {
                        p += (aSoil.TopLevel - Level) * aSoil.Weight;
                    }
                }
            }

            return p;
        }

        //计算反向流动结果（调用穿刺砂土方法）
        public void GetBackFlowResult_Other(ref double Qv, double Level, LegParameter LegParameter, SpudcanParameter SpudcanParameter, List<SoilParameter> Soils, double Hc, ref string Description)
        {
            GetBackFlowResult_PunchSand(ref Qv, Level, LegParameter, SpudcanParameter, Soils, Hc, ref Description);
        }

        //计算穿刺砂土模式下的反向流动结果
        public void GetBackFlowResult_PunchSand(ref double Qv, double Level, LegParameter LegParameter, SpudcanParameter SpudcanParameter, List<SoilParameter> Soils, double Hc, ref string Description)
        {
            double D = Soils[0].TopLevel - Level;
            SoilParameter averageSoil = GetAverageSoilValue(Soils, Level, Soils[0].TopLevel);

            // I=(D - Hc - ((SpudcanParameter.Volume - Vd) / SpudcanParameter.Area),桩靴上部覆土高度
            double tempValue = averageSoil.Weight * (
                SpudcanParameter.Area * (D - Hc) -
                LegParameter.Area * (D - Hc - SpudcanParameter.H3 - SpudcanParameter.H4) -
                SpudcanParameter.Volume
            );

            Qv -= Math.Max(tempValue, 0);

            // 构建描述字符串
            string h4Part = SpudcanParameter.H4 == 0
                ? Math.Round(SpudcanParameter.H3, 3).ToString()
                : $"({Math.Round(SpudcanParameter.H3, 3)}+{Math.Round(SpudcanParameter.H4, 3)})";

            string tempValuePart = tempValue < 0
                ? "0"
                : $"{Math.Round(averageSoil.Weight, 3)}×[{Math.Round(SpudcanParameter.Area)}×({Math.Round(D, 3)}-{Math.Round(Hc, 3)})-{Math.Round(LegParameter.Area, 3)}×({Math.Round(D, 3)}-{Math.Round(Hc, 3)}-{h4Part})-{Math.Round(SpudcanParameter.Volume, 3)}]";

            Description += "-" + tempValuePart;
        }

        //按照黏土常规破坏模式计算抗压承载力
        public double GetQV_Clay(double B,
        double Level,
        LegParameter LegParameter,
        SpudcanParameter SpudcanParameter,
        SoilParameter Soil,
        List<SoilParameter> Soils,
        bool IsBackFlow,
        ref string Description,      // ref 参数移到前面
        double Hc = 0,               // 可选参数
        string QvName = "",          // 可选参数
        double DispersionL = 0)      // 可选参数)
        {
            double Nc = 5.14;
            double Sc = 0;
            double D = Soils[0].TopLevel - Level;
            double Dc = Math.Min(1 + 0.2 * D / B, 1.5);
            double Nq = 1;
            double Nc_Sc_Dc;
            bool outOfRange = false;

            double[,] coeffs = new double[,]
            {
        { 0, 6 },
        { 0.1, 6.3 },
        { 0.25, 6.6 },
        { 0.5, 7 },
        { 1.0, 7.7 },
        { 2.5, 9 }
            };

            Nc_Sc_Dc = GetCoeff(D / B, coeffs, 5, ref outOfRange);

            if (SpudcanParameter.ShapeType == 1 || outOfRange)
            {
                Sc = 1 + (Nq / Nc) * (B / (DispersionL != 0 ? DispersionL : SpudcanParameter.L));
                Nc_Sc_Dc = Nc * Sc * Dc;
            }

            // 考虑到砂土穿刺粘土的扩散，砂土下粘土的常规承载力中面积按扩散面积计算
            double spudcanA = DispersionL != 0
                ? (SpudcanParameter.ShapeType == 1 ? B * DispersionL : Math.PI * Math.Pow(B, 2) / 4)
                : SpudcanParameter.Area;

            double Qv = (Soil.GetSu(Level) * Nc_Sc_Dc + GetP0(Level, Soils)) * spudcanA;

            // 构建描述字符串
            string ncScDcPart = (SpudcanParameter.ShapeType == 1 || outOfRange)
                ? $"Ncscdc"
                : $"(Ncscdc)";

            string backFlowPart = IsBackFlow
                ? $"-γ'[A(D-Hc)-Al(D-Hc-{(SpudcanParameter.H4 == 0 ? "Ht" : "(H2+H3)")})-V]{Environment.NewLine}"
                : Environment.NewLine;

            Description += (string.IsNullOrEmpty(QvName) ? "Qv" : QvName) + $"=(Su{ncScDcPart}+p'0)A" + backFlowPart;

            string ncScDcValuePart = (SpudcanParameter.ShapeType == 1 || outOfRange)
                ? $"{Math.Round(Nc, 3)}×{Math.Round(Sc, 3)}×{Math.Round(Dc, 3)}"
                : $"({Math.Round(Nc_Sc_Dc, 3)})";

            Description += (string.IsNullOrEmpty(QvName) ? "Qv" : QvName) +
                $"=({Math.Round(Soil.GetSu(Level), 3)}×{ncScDcValuePart}+{Math.Round(GetP0(Level, Soils), 3)})×{Math.Round(spudcanA, 3)}";

            if (IsBackFlow)
            {
                GetBackFlowResult_Other(ref Qv, Level, LegParameter, SpudcanParameter, Soils, Hc, ref Description);
            }

            Description += $"={Math.Round(Qv, 3)};";

            return Qv;
        }

        //从系数表中线性插值获取系数值
        public double GetCoeff(double V, double[,] Coeffs, int N, ref bool OutOfRange)
        {
            // 检查是否超出范围（大于最大值）
            if (V > Coeffs[N - 1, 0])
            {
                OutOfRange = true;
                return Coeffs[N - 1, 1];
            }

            // 检查是否超出范围（小于最小值）
            if (V < Coeffs[0, 0])
            {
                OutOfRange = true;
                return Coeffs[0, 1];
            }

            // 线性插值
            for (int i = 0; i < N - 1; i++)
            {
                if (V >= Coeffs[i, 0] && V <= Coeffs[i + 1, 0])
                {
                    return Coeffs[i, 1] +
                           (Coeffs[i + 1, 1] - Coeffs[i, 1]) *
                           (V - Coeffs[i, 0]) /
                           (Coeffs[i + 1, 0] - Coeffs[i, 0]);
                }
            }

            return Coeffs[0, 1];
        }

        //按照砂土常规破坏模式计算抗压承载力
        public double GetQV_Sand(
        bool IsEquivalentToCircleSpudcan,
        double B,
        double Level,
        double NextSoilLevel,
        LegParameter LegParameter,
        SpudcanParameter SpudcanParameter,
        SoilParameter Soil,
        List<SoilParameter> Soils,
        bool IsBackFlow,
        ref string Description,
        double Hc = 0,
        string QvName = "")
        {
            double D = Soils[0].TopLevel - Level; // 插深,桩靴最大截面处下部到海床面的距离
            SoilParameter theAverageSoil = Soil;

            if (NextSoilLevel < Soil.BottomLevel)
            {
                theAverageSoil = GetAverageSoilValue(Soils, NextSoilLevel, Level);
            }

            double soilWeight = theAverageSoil.Weight;
            double dgamma = 1;
            double Nq, Ngamma;
            bool outOfRange = false;

            // 定义砂土参数表
            double[] soilPhis = { 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40 };
            double[] ngammas = { 2.4, 2.9, 3.5, 4.2, 5.1, 6.1, 7.3, 8.8, 10.6, 12.8, 15.5, 18.8, 22.9, 27.9, 34.1, 41.9, 51.6, 63.7, 79.1, 98.7, 123.7 };
            double[] nqs = { 9.6, 10.9, 12.4, 14.1, 16.1, 18.4, 21.1, 24.2, 27.9, 32.2, 37.2, 43.2, 50.3, 58.7, 68.7, 80.8, 95.4, 113, 134.4, 160.5, 192.7 };

            int count = soilPhis.Length;
            double[,] phiNgammas = new double[count, 2];
            double[,] phiNqs = new double[count, 2];

            for (int i = 0; i < count; i++)
            {
                phiNgammas[i, 0] = soilPhis[i] / 180.0 * Math.PI;
                phiNgammas[i, 1] = ngammas[i];
                phiNqs[i, 0] = soilPhis[i] / 180.0 * Math.PI;
                phiNqs[i, 1] = nqs[i];
            }

            Nq = GetCoeff(theAverageSoil.Phi, phiNqs, count, ref outOfRange);

            if ((SpudcanParameter.ShapeType == 1 && !IsEquivalentToCircleSpudcan) || outOfRange)
            {
                Nq = Math.Exp(Math.PI * Math.Tan(theAverageSoil.Phi)) * Math.Pow(Math.Tan(Math.PI / 4 + theAverageSoil.Phi / 2), 2);
            }

            Ngamma = GetCoeff(theAverageSoil.Phi, phiNgammas, count, ref outOfRange);

            if ((SpudcanParameter.ShapeType == 1 && !IsEquivalentToCircleSpudcan) || outOfRange)
            {
                Ngamma = 2 * (Nq + 1) * Math.Tan(theAverageSoil.Phi);
            }

            double P0 = GetP0(Level, Soils); // 插深D范围内的上覆土压力
            double dq = (D / B <= 1)
                ? 1 + 2 * Math.Tan(theAverageSoil.Phi) * Math.Pow(1 - Math.Sin(theAverageSoil.Phi), 2) * (D / B)
                : 1 + 2 * Math.Tan(theAverageSoil.Phi) * Math.Pow(1 - Math.Sin(theAverageSoil.Phi), 2) * Math.Atan(D / B);

            double Qv = (soilWeight * dgamma * Ngamma * B / 2 + P0 * dq * Nq) * SpudcanParameter.Area;

            double Sg = 0, Sq = 0;
            if (!IsEquivalentToCircleSpudcan)
            {
                Sg = 1 - 0.4 * (B / SpudcanParameter.L);
                Sq = 1 + Math.Tan(theAverageSoil.Phi) * (B / SpudcanParameter.L);
                Qv = (soilWeight * dgamma * Ngamma * B / 2 * Sg + P0 * dq * Nq * Sq) * SpudcanParameter.Area;
            }

            // 构建描述字符串
            string qvLabel = string.IsNullOrEmpty(QvName) ? "Qv" : QvName;
            string formulaPart = IsEquivalentToCircleSpudcan
                ? "=(γ'dγNγB/2+p'0dqNq)A"
                : "=(γ'dγsγNγB/2+p'0dqsqNq)A";

            string backFlowPart = IsBackFlow
                ? $"-γ'[A(D-Hc)-Al(D-Hc-{(SpudcanParameter.H4 == 0 ? "Ht" : "(H2+H3)")})-V]{Environment.NewLine}"
                : Environment.NewLine;

            Description += qvLabel + formulaPart + backFlowPart;

            // 构建详细数值描述
            string sgPart = IsEquivalentToCircleSpudcan ? "" : $"×{Math.Round(Sg, 3)}";
            string sqPart = IsEquivalentToCircleSpudcan ? "" : $"×{Math.Round(Sq, 3)}";

            Description += qvLabel +
                $"=({soilWeight}×{dgamma}{sgPart}×{Math.Round(Ngamma, 3)}×{Math.Round(B, 3)}/2+" +
                $"{Math.Round(P0, 3)}×{Math.Round(dq, 3)}{sqPart}×{Math.Round(Nq, 3)})×{Math.Round(SpudcanParameter.Area, 3)}";

            if (IsBackFlow)
            {
                GetBackFlowResult_Other(ref Qv, Level, LegParameter, SpudcanParameter, Soils, Hc, ref Description);
            }

            Description += $"={Math.Round(Qv, 3)};";

            return Qv;
        }

        //按照挤出破坏模式计算抗压承载力
        public double GetQV_Squeeze(
        bool IsEquivalentToCircleSpudcan,
        double B,
        double Level,
        double NextLevel,
        LegParameter LegParameter,
        SpudcanParameter SpudcanParameter,
        SoilParameter Soil,
        List<SoilParameter> Soils,
        bool IsBackFlow,
        ref string Description,
        double Hc = 0,
        string QvName = "")
        {
            string normalString = "挤出破坏模式：";

            if (Soil.SoilType == SoilType.Clay || Soil.SoilType == SoilType.Both)
            {
                double D = Soils[0].TopLevel - Level;
                double T = Math.Max(Level - Soil.BottomLevel, 0.01);
                double Qv;

                // 挤出破坏模式计算的竖向承载力下限值为软土的常规破坏模式承载力计算结果
                // 获得下限值
                string tempDescriptionMin = "";
                double Qv_Min = GetQV_Clay(B, Level, LegParameter, SpudcanParameter, Soil, Soils,
                                           IsBackFlow, ref tempDescriptionMin, Hc, QvName);

                if (B >= 3.45 * T * (1 + 1.025 * D / B) && D / B <= 2.5)
                {
                    double Ass = 5;
                    double Bss = 0.33;
                    double P0 = GetP0(Level, Soils);

                    Qv = SpudcanParameter.Area * ((Ass + Bss * B / T + 1.2 * D / B) * Soil.GetSu(Level) + P0);

                    string tempDescription = (string.IsNullOrEmpty(QvName) ? "Qv" : QvName) +
                        $"=A{{(αs+bsB/T+1.2D/B)Su+p'0" +
                        (IsBackFlow ? $"-γ'[A(D-Hc)-Al(D-Hc-{(SpudcanParameter.H4 == 0 ? "Ht" : "(H2+H3)")})-V]{Environment.NewLine}" : Environment.NewLine) +
                        "-FOA+γ'V=γ'[As(D-Hc)-Al(D-Hc-Ht)-V]";

                    tempDescription += (string.IsNullOrEmpty(QvName) ? "Qv" : QvName) +
                        $"=" + Math.Round(SpudcanParameter.Area, 3).ToString() +
                        $"×(({Ass}+{Bss}×{Math.Round(B, 3)}/{Math.Round(T, 3)}+{1.2}×{D}/{Math.Round(B, 3)})×{Math.Round(Soil.GetSu(Level), 3)}+{Math.Round(P0, 3)})";

                    if (IsBackFlow)
                    {
                        GetBackFlowResult_Other(ref Qv, Level, LegParameter, SpudcanParameter, Soils, Hc, ref tempDescription);
                    }

                    tempDescription += $"={Math.Round(Qv, 3)};";

                    if (Qv < Qv_Min)
                    {
                        normalString += $"挤出破坏结果Qv=Max(挤出Qv({Qv})，常规Qv({Qv_Min})";
                        Qv = Qv_Min;
                        Description += tempDescription + Environment.NewLine + "Qv=Max(Qv，常规Qv)" + Environment.NewLine + tempDescriptionMin;
                        EsMessageReporter.ReportMessageFunction(normalString, EsMessageType.Normal);
                    }
                    else
                    {
                        Description += tempDescription;
                        // 上限值待下层土计算完成后进行比对
                    }

                    return Qv;
                }
                else
                {
                    // 当挤出条件不满足时，按常规破坏模式计算
                    Qv = Qv_Min;
                    Description += tempDescriptionMin;
                    normalString += $"T={Math.Round(T, 2)}，D={Math.Round(D, 2)}，B={Math.Round(B, 2)}，不满足挤出破坏条件（B≥3.45T（1+1.025D/B），且D/B≤2.5），按常规破坏模式计算";
                    EsMessageReporter.ReportMessageFunction(normalString, EsMessageType.Normal);
                    return Qv;
                }
            }
            else
            {
                Description += (string.IsNullOrEmpty(QvName) ? "Qv" : QvName) + "未计算;";
                normalString += "持力层土的土类型为砂土，不满足挤出破坏条件：持力层土承载力小于持力+1层土承载力，即上软下硬";
                EsMessageReporter.ReportMessageFunction(normalString, EsMessageType.Normal);
                return 1e10; // 10^10
            }
        }

        //按照分层土破坏模式计算抗压承载力
        public double GetQV_MultiLayer(
        double B,
        double Level,
        LegParameter LegParameter,
        SpudcanParameter SpudcanParameter,
        SoilParameter Soil,
        List<SoilParameter> Soils,
        bool IsBackFlow,
        ref string Description,
        double Hc = 0,
        string QvName = "")
        {
            double Qv;
            double soilWeight = Soil.Weight;
            double ig, iq, ic;
            double Nq, Ngamma, Nc, Sg, Sq, Sc;
            double P0 = GetP0(Level, Soils); // 插深D范围内的上覆土压力

            ig = 1;
            iq = 1;
            ic = 1;
            Sg = 1 - 0.4 * (B / SpudcanParameter.L);
            Sq = 1 + Math.Tan(Soil.Phi) * (B / SpudcanParameter.L);
            Nq = Math.Exp(Math.PI * Math.Tan(Soil.Phi)) * Math.Pow(Math.Tan(Math.PI / 4 + Soil.Phi / 2), 2);
            Ngamma = 2 * (Nq + 1) * Math.Tan(Soil.Phi);

            if (Soil.Phi == 0)
            {
                string qvLabel = string.IsNullOrEmpty(QvName) ? "Qv" : QvName;
                Description += qvLabel + "未计算;";
                EsMessageReporter.ReportMessageFunction(
                    "分层土破坏模式：持力层土的摩擦角为0，无法计算承载力修正系数Nc，不进行分层土破坏模式计算",
                    EsMessageType.Normal);
                return 1e10; // 10^10
            }

            Nc = (Nq - 1) / Math.Tan(Soil.Phi);
            Sc = 1 + (Nq / Nc) * (B / SpudcanParameter.L);

            Qv = (0.5 * soilWeight * B * Ngamma * Sg * ig +
                  P0 * Nq * Sq * iq +
                  Soil.GetSu(Level) * Nc * Sc * ic) * SpudcanParameter.Area;

            // 构建描述字符串
            string qvLabel2 = string.IsNullOrEmpty(QvName) ? "Qv" : QvName;
            string htPart = SpudcanParameter.H4 == 0 ? "Ht" : "(H2+H3)";
            string backFlowPart = IsBackFlow
                ? $"-γ'[A(D-Hc)-Al(D-Hc-{htPart})-V]{Environment.NewLine}"
                : Environment.NewLine;

            Description += qvLabel2 + "=(0.5γ'BNγsγiγ+p'0Nqsqiq+suNcscic)A" + backFlowPart;

            Description += qvLabel2 +
                $"=({0.5}×{soilWeight}×{Math.Round(B, 3)}×{Math.Round(Ngamma, 3)}×{Math.Round(Sg, 3)}×{ig}+" +
                $"{Math.Round(P0, 3)}×{Math.Round(Nq, 3)}×{Math.Round(Sq, 3)}×{iq}+" +
                $"{Math.Round(Soil.GetSu(Level), 3)}×{Math.Round(Nc, 3)}×{Math.Round(Sc, 3)}×{ic})×{Math.Round(SpudcanParameter.Area, 3)}";

            if (IsBackFlow)
            {
                GetBackFlowResult_Other(ref Qv, Level, LegParameter, SpudcanParameter, Soils, Hc, ref Description);
            }

            Description += $"={Math.Round(Qv, 3)};";

            return Qv;
        }

        //按照穿刺破坏模式计算黏土抗压承载力
        public double GetQV_Punch_Clay(
        double B,
        double Level,
        double NextLevel,
        LegParameter LegParameter,
        SpudcanParameter SpudcanParameter,
        SoilParameter Soil,
        List<SoilParameter> Soils,
        bool IsBackFlow,
        ref string Description,
        double Hc = 0,
        string QvName = "")
        {
            double Qv_Clay, Qv;
            string tempDes = "";
            double D = Soils[0].TopLevel - Level;
            double H;
            double Suto, Subo;
            double Nc = 5.14;
            double Nq = 1;
            double Nc_Sc;

            if (SpudcanParameter.ShapeType == 0)
            {
                Nc_Sc = 6;
            }
            else
            {
                double Sc = 1 + (Nq / Nc) * (B / SpudcanParameter.L);
                Nc_Sc = Nc * Sc;
            }

            Suto = Soil.GetSu(Level);
            double P0 = GetP0(Level, Soils);
            SoilParameter BottomSoil = GetSoil(NextLevel, Soils);

            string tempDescriptionMax = "";
            Qv_Clay = GetQV_Clay(B, Level, LegParameter, SpudcanParameter, Soil, Soils,
                                 IsBackFlow, ref tempDescriptionMax, Hc, QvName);

            string normalString = "穿刺破坏模式：";
            string qvLabel = string.IsNullOrEmpty(QvName) ? "Qv" : QvName;

            if (Level == NextLevel || BottomSoil.SoilType == SoilType.Sand)
            {
                if (Level == NextLevel)
                {
                    Qv = Qv_Clay;
                    Description += tempDescriptionMax;
                    normalString += Level + "为土层最底端，按常规破坏模式计算";
                }
                else
                {
                    Qv = 1e10;
                    tempDes = qvLabel + "未计算;";
                    Description += tempDes;
                    normalString += "持力+1层土的土类型为砂土，不满足穿刺破坏条件：持力层土承载力大于持力+1层土承载力，即上硬下软";
                }
                EsMessageReporter.ReportMessageFunction(normalString, EsMessageType.Normal);
            }
            else
            {
                H = Level - NextLevel;
                Subo = BottomSoil.GetSu(NextLevel);
                Qv = SpudcanParameter.Area * (3 * H / B * Suto + Nc_Sc * (1 + 0.2 * (D + H) / B) * Subo + P0);

                string htPart = SpudcanParameter.H4 == 0 ? "Ht" : "(H2+H3)";
                tempDes += qvLabel + $"=A[3H/BSu,t+(Ncsc)(1+0.2(D+H)/B)Su,b+p'0)]" +
                    (IsBackFlow ? $"-γ'[A(D-Hc)-Al(D-Hc-{htPart})-V]{Environment.NewLine}" : Environment.NewLine);

                tempDes += qvLabel + "=" + Math.Round(SpudcanParameter.Area, 3) +
                    $"×({3}×{Math.Round(H, 3)}/{Math.Round(B, 3)}×{Suto}+" +
                    $"({Math.Round(Nc_Sc, 3)})×(1 + 0.2 ×({D}+{Math.Round(H, 3)})/{Math.Round(B, 3)})×{Subo}+{Math.Round(P0, 3)})";

                if (IsBackFlow)
                {
                    GetBackFlowResult_Other(ref Qv, Level, LegParameter, SpudcanParameter, Soils, Hc, ref tempDes);
                }

                tempDes += $"={Math.Round(Qv, 3)};";

                if (Qv <= Qv_Clay)
                {
                    Description += tempDes;
                }
                else
                {
                    normalString += $"穿刺破坏结果Qv=Min(常规Qv({Qv_Clay}),穿刺Qv({Qv})";
                    Qv = Qv_Clay;
                    Description += tempDescriptionMax;
                    EsMessageReporter.ReportMessageFunction(normalString, EsMessageType.Normal);
                }
            }

            return Qv;
        }

        //按照穿刺破坏模式计算砂土抗压承载力
        public double GetQV_Punch_Sand(
            bool IsEquivalentToCircleSpudcan,
            double B,
            double Level,
            double NextLevel,
            LegParameter LegParameter,
            SpudcanParameter SpudcanParameter,
            SoilParameter Soil,
            List<SoilParameter> Soils,
            bool IsBackFlow,
            ref string Description,
            double Hc = 0,
            string QvName = "")
        {
            double Qv_Sand, Qv;
            double D = Soils[0].TopLevel - Level;
            double H;
            double P0 = GetP0(Level, Soils);

            SoilParameter theAverageSoil = Soil;
            if (NextLevel < Soil.BottomLevel)
            {
                theAverageSoil = GetAverageSoilValue(Soils, NextLevel, Level);
            }

            SoilParameter bottomSoil = GetSoil(NextLevel, Soils);

            string tempDesSand = "";
            Qv_Sand = GetQV_Sand(IsEquivalentToCircleSpudcan, B, Level, NextLevel, LegParameter,
                                 SpudcanParameter, theAverageSoil, Soils, IsBackFlow, ref tempDesSand, Hc, QvName);

            string normalString = "穿刺破坏模式：";
            string qvLabel = string.IsNullOrEmpty(QvName) ? "Qv" : QvName;

            if (Level == NextLevel || bottomSoil.SoilType == SoilType.Sand)
            {
                if (Level == NextLevel)
                {
                    Qv = Qv_Sand;
                    Description += tempDesSand;
                    normalString += Level + "为土层最底端，按常规破坏模式计算";
                    EsMessageReporter.ReportMessageFunction(normalString, EsMessageType.Normal);
                }
                else
                {
                    normalString += $"计算高程={Level}处，多个砂土合并层的底部无软土层，不进行穿刺破坏模式计算!";
                    EsMessageReporter.ReportMessageFunction(normalString, EsMessageType.Normal);
                    Qv = Qv_Sand;
                }
            }
            else
            {
                H = Level - NextLevel;
                string tempDes = "";
                string tempDesClay = "";

                double QV1 = GetQV_Sand(IsEquivalentToCircleSpudcan, B, Level, NextLevel, LegParameter,
                                        SpudcanParameter, theAverageSoil, Soils, false, ref tempDes, 0, "Qsand");
                double QV2 = GetQV_Clay(SpudcanParameter.GetSpudcanB(), NextLevel, LegParameter,
                                        SpudcanParameter, bottomSoil, Soils, false, ref tempDesClay, 0, "Qclay");

                double coeff = QV2 / QV1;
                double weight = theAverageSoil.Weight;

                tempDes = tempDes.TrimEnd(';') + Environment.NewLine;
                tempDesClay = tempDesClay.TrimEnd(';');
                tempDes += tempDesClay + Environment.NewLine;

                double phi25 = 25.0 / 180.0 * Math.PI;
                string phiCompare = Math.Abs(theAverageSoil.Phi - phi25) < 1e-9 ? "=" : (theAverageSoil.Phi < phi25 ? "＜" : "＞");
                string coeffCompare = Math.Abs(coeff - 0.1) < 1e-9 ? "=" : (coeff < 0.1 ? "＜" : "＞");
                tempDes += $"φ{phiCompare}25°，Qclay/Qsand={Math.Round(coeff, 3)}{coeffCompare}0.1{Environment.NewLine}";

                if (theAverageSoil.Phi < phi25 || coeff < 0.1)
                {
                    string tempDesClayB = "";
                    double ns = 3;
                    double dispersionB = (SpudcanParameter.ShapeType == 0) ?
                        B : SpudcanParameter.GetSpudcanB() + 2 * H / ns;
                    double dispersionL = (SpudcanParameter.ShapeType == 0) ?
                        SpudcanParameter.L : SpudcanParameter.L + 2 * H / ns;
                    double W = ((SpudcanParameter.ShapeType == 0) ?
                        0.25 * Math.PI * Math.Pow(dispersionB, 2) : dispersionB * dispersionL) * H * weight;

                    QV2 = GetQV_Clay(dispersionB, NextLevel, LegParameter, SpudcanParameter, bottomSoil,
                                     Soils, false, ref tempDesClayB, 0, "Qv,b", dispersionL);

                    string htPart = SpudcanParameter.H4 == 0 ? "Ht" : "(H2+H3)";
                    tempDes += tempDesClayB.TrimEnd(';') + Environment.NewLine;
                    tempDes += qvLabel + "=Qv,b-W" +
                        (IsBackFlow ? $"-γ'[A(D-Hc)-Al(D-Hc-{htPart})-V]{Environment.NewLine}" : Environment.NewLine);
                    tempDes += qvLabel + "=" + Math.Round(QV2, 3) + "-" + Math.Round(W, 3);
                    Qv = QV2 - W;
                }
                else
                {
                    double KsXXXX = 17.75 * coeff + 1.825;
                    double KsXXXV = 14.6667 * coeff + 0.7333;
                    double KsXXX = 11.875 * coeff + 0.1125;
                    double KsXXV = 7.875 * coeff - 0.0875;

                    double[,] phiKs = new double[,]
                    {
                { 25.0 / 180.0 * Math.PI, KsXXV },
                { 30.0 / 180.0 * Math.PI, KsXXX },
                { 35.0 / 180.0 * Math.PI, KsXXXV },
                { 40.0 / 180.0 * Math.PI, KsXXXX }
                    };

                    bool outOfRange = false;
                    double Ks = GetCoeff(theAverageSoil.Phi, phiKs, 4, ref outOfRange);

                    string htPart = SpudcanParameter.H4 == 0 ? "Ht" : "(H2+H3)";
                    tempDes += tempDesClay.Replace("Qclay", "Qv,b") + Environment.NewLine;
                    tempDes += qvLabel + "=Qv,b-AHγ'+2AH(Hγ'+2p'0)Kstan(φ'/B)" +
                        (IsBackFlow ? $"-γ'[A(D-Hc)-Al(D-Hc-{htPart})-V]{Environment.NewLine}" : Environment.NewLine);
                    tempDes += qvLabel + "=" + Math.Round(QV2, 3) +
                        $"-{Math.Round(SpudcanParameter.Area, 3)}×{Math.Round(H, 3)}×{Math.Round(weight, 3)}+" +
                        $"{2}×{Math.Round(SpudcanParameter.Area, 3)}×{Math.Round(H, 3)}×({Math.Round(H, 3)}×{Math.Round(weight, 3)}+" +
                        $"{2}×{Math.Round(P0, 3)})×{Math.Round(Ks, 3)}×tan({Math.Round(theAverageSoil.Phi, 3)}/{Math.Round(B, 3)})";

                    Qv = QV2 - SpudcanParameter.Area * H * weight +
                         2 * SpudcanParameter.Area * H * (H * weight + 2 * P0) * Ks * Math.Tan(theAverageSoil.Phi / B);
                }

                if (IsBackFlow)
                {
                    //GetBackFlowResult_PunchSand(ref Qv, ref tempDes, Level, LegParameter, SpudcanParameter, Soils, Hc);
                    GetBackFlowResult_Other(ref Qv, Level, LegParameter, SpudcanParameter, Soils, Hc, ref tempDes);
                }

                tempDes += $"={Math.Round(Qv, 3)};";

                if (Qv <= Qv_Sand)
                {
                    Description += tempDes;
                }
                else
                {
                    normalString += $"穿刺破坏结果Qv=Min(常规Qv({Qv_Sand}),穿刺Qv({Qv})";
                    Qv = Qv_Sand;
                    Description += tempDesSand;
                    EsMessageReporter.ReportMessageFunction(normalString, EsMessageType.Normal);
                }
            }

            return Qv;
        }

        //计算黏土抗拔力（含三种工况）
        public double[] GetQb_Clay(
            int DeepType,
            CalculateParameter CalculateParameter,
            double Level,
            LegParameter LegParameter,
            SpudcanParameter SpudcanParameter,
            SoilParameter Soil,
            List<SoilParameter> Soils,
            double fb,
            ref string Description,
            string QbName = "")
        {
            double[] Qb = new double[3];
            double HColumn = Math.Max(Soils[0].TopLevel - Level - CalculateParameter.Hc - SpudcanParameter.H3, 0);
            double Vtop = LegParameter.Area * (HColumn - SpudcanParameter.H4);
            SoilParameter averageSoil = GetAverageSoilValue(Soils, Level, Soils[0].TopLevel);
            SoilParameter upSoil = GetSoil(Level + 0.001, Soils);

            for (int i = 0; i < 3; i++)
            {
                double SuHcol = GetAverageSoilValue(Soils, Level + SpudcanParameter.H3, Soils[0].TopLevel - CalculateParameter.Hc).Su0;
                double DownSu = (upSoil.GetSu(Level) + Soil.GetSu(Level)) / 2;
                double SuHt = GetAverageSoilValue(Soils, Level, Level + SpudcanParameter.H3).Su0;
                double SuHLeg = GetAverageSoilValue(Soils, Level + SpudcanParameter.H3 + SpudcanParameter.H4, Soils[0].TopLevel).Su0;

                double factor = (i == 2) ? fb : i;
                SuHcol *= factor;
                DownSu *= factor;
                SuHt *= factor;
                SuHLeg *= factor;

                Qb[i] = SpudcanParameter.Weight + SpudcanParameter.Area *
                        (CalculateParameter.NBreakout * DownSu * CalculateParameter.fbase + HColumn * averageSoil.Weight) -
                        Vtop * averageSoil.Weight;

                string qbLabel = string.IsNullOrEmpty(QbName) ? "Qu" : (i == 2 ? QbName : QbName + "_C" + i);

                if (DeepType == 1) // 浅埋
                {
                    Qb[i] += SpudcanParameter.Circumference *
                            (HColumn * SuHcol * CalculateParameter.ftop +
                             CalculateParameter.alpha * SpudcanParameter.Ht * SuHt * CalculateParameter.fbase);

                    Description += qbLabel + "=W+C(HcolumnSuftop+αHtSufbase)+A(NbreakoutSufbase+Hcolumnγ')-Vtopγ'" + Environment.NewLine;
                    Description += qbLabel + "=" + SpudcanParameter.Weight + "+" +
                        Math.Round(SpudcanParameter.Circumference, 3) + "×(" +
                        Math.Round(HColumn, 3) + "×" + Math.Round(SuHcol, 3) + "×" + CalculateParameter.ftop + "+" +
                        CalculateParameter.alpha + "×" + SpudcanParameter.Ht + "×" + Math.Round(SuHt, 3) + "×" +
                        CalculateParameter.fbase + ")+";
                }
                else // 深埋
                {
                    double HLeg = Math.Max(HColumn - SpudcanParameter.H4, 0);
                    Qb[i] += CalculateParameter.fleg * LegParameter.Circumference * HLeg * SuHLeg;

                    Description += qbLabel + "=W+flegA'Su+A(NbreakoutSufbase+Hcolumnγ')-Vtopγ'" + Environment.NewLine;
                    Description += qbLabel + "=" + SpudcanParameter.Weight + "+" +
                        CalculateParameter.fleg + "×" + Math.Round(LegParameter.Circumference * HLeg, 3) + "×" +
                        Math.Round(SuHLeg, 3) + "+";
                }

                Description += Math.Round(SpudcanParameter.Area, 3) + "×(" +
                    CalculateParameter.NBreakout + "×" + Math.Round(DownSu, 3) + "×" +
                    CalculateParameter.fbase + "+" + Math.Round(HColumn, 3) + "×" +
                    Math.Round(averageSoil.Weight, 3) + ")-" + Math.Round(Vtop, 3) + "×" +
                    Math.Round(averageSoil.Weight, 3);
                Description += "=" + Math.Round(Qb[i], 3) + ";";
            }

            return Qb;
        }

        //按照浅埋模式计算黏土抗拔力
        public double GetQb_Clay_Shallow(
            CalculateParameter CalculateParameter,
            double Level,
            LegParameter LegParameter,
            SpudcanParameter SpudcanParameter,
            SoilParameter Soil,
            List<SoilParameter> Soils,
            bool HalfDownSu = false)
        {
            double SFR = GetSideFrictionalResistance(CalculateParameter, Level, LegParameter, SpudcanParameter, Soil, Soils);
            double HColumn = Math.Max(Soils[0].TopLevel - Level - CalculateParameter.Hc - SpudcanParameter.H3, 0);
            SoilParameter averageSoil = GetAverageSoilValue(Soils, Level, Soils[0].TopLevel);
            SoilParameter upSoil = GetSoil(Level + 0.001, Soils);
            double upDownSu = GetAverageSoilValue(Soils, Soil.BottomLevel, upSoil.TopLevel).Su0;
            double DownSu = HalfDownSu ? upSoil.GetSu(Level) * 0.5 : upDownSu;
            double Vtop = LegParameter.Area * (HColumn - SpudcanParameter.H4);

            double Qb = SpudcanParameter.Weight + SFR +
                        SpudcanParameter.Area * (CalculateParameter.NBreakout * DownSu * CalculateParameter.fbase + HColumn * averageSoil.Weight) -
                        Vtop * averageSoil.Weight;
            return Qb;
        }

        //按照深埋模式计算黏土抗拔力
        public double GetQb_Clay_Deep(
            CalculateParameter CalculateParameter,
            double Level,
            LegParameter LegParameter,
            SpudcanParameter SpudcanParameter,
            SoilParameter Soil,
            List<SoilParameter> Soils,
            bool HalfDownSu = false)
        {
            double SFR = GetSideFrictionalResistance(CalculateParameter, Level, LegParameter, SpudcanParameter, Soil, Soils);
            double HColumn = Math.Max(Soils[0].TopLevel - Level - CalculateParameter.Hc - SpudcanParameter.H3, 0);
            SoilParameter averageSoil = GetAverageSoilValue(Soils, Level, Soils[0].TopLevel);
            SoilParameter upSoil = GetSoil(Level + 0.001, Soils);
            double upDownSu = GetAverageSoilValue(Soils, Soil.BottomLevel, upSoil.TopLevel).Su0;
            double DownSu = HalfDownSu ? upSoil.GetSu(Level) * 0.5 : upDownSu;
            double Vtop = LegParameter.Area * (HColumn - SpudcanParameter.H4);

            double Qb = SpudcanParameter.Weight + SFR +
                        SpudcanParameter.Area * (CalculateParameter.NBreakout * DownSu * CalculateParameter.fbase + HColumn * averageSoil.Weight) -
                        Vtop * averageSoil.Weight;
            return Qb;
        }

        //计算插深范围内土层的侧摩阻力
        public double GetSideFrictionalResistance(
        CalculateParameter CalculateParameter,
        double Level,
        LegParameter LegParameter,
        SpudcanParameter SpudcanParameter,
        SoilParameter Soil,
        List<SoilParameter> Soils)
        {
            // 获得插深范围内土层侧摩阻力（每层土按土类型进行计算，计算时埋深类型按插深考虑）
            double SFR = 0;
            double SFRClay_Shallow = 0;
            double SFRClay_Deep = 0;
            double SFRSand = 0;

            double D = Soils[0].TopLevel - Level;
            double HColumnLevel = Level + SpudcanParameter.H3;
            double LegLevel = HColumnLevel + SpudcanParameter.H4;
            double HcLevel = Soils[0].TopLevel - CalculateParameter.Hc;

            // 计算桩靴等效宽度
            double spudcanB;
            if (CalculateParameter.IsEquivalentToCircleSpudcan)
            {
                spudcanB = 2 * Math.Pow(SpudcanParameter.Area / Math.PI, 0.5);
            }
            else if (SpudcanParameter.ShapeType == 0)
            {
                spudcanB = SpudcanParameter.Diameter;
            }
            else
            {
                spudcanB = Math.Min(SpudcanParameter.L, SpudcanParameter.B);
            }

            // 黏土侧摩阻力（浅埋）
            if (D <= spudcanB)
            {
                double HColumn_Clay = 0;
                double SuHcol = GetAverageSoilValue(Soils, HColumnLevel, HcLevel, ref HColumn_Clay, (int)SoilType.Clay).Su0;

                double Ht_Clay = 0;
                double SuHt = GetAverageSoilValue(Soils, Level, HColumnLevel, ref Ht_Clay, (int)SoilType.Clay).Su0;

                SFRClay_Shallow += SpudcanParameter.Circumference *
                    (SuHcol * HColumn_Clay * CalculateParameter.ftop +
                     CalculateParameter.alpha * SuHt * Ht_Clay * CalculateParameter.fbase);
            }
            else // 黏土侧摩阻力（深埋）
            {
                double HLeg_Clay = 0;
                double SuHLeg = GetAverageSoilValue(Soils, LegLevel, HcLevel, ref HLeg_Clay, (int)SoilType.Clay).Su0;
                SFRClay_Deep += LegParameter.Circumference * SuHLeg * HLeg_Clay;
            }

            // 砂土侧摩阻力
            double H = GetH(Soil.Phi, spudcanB);
            double S = GetS(Soil.Phi);

            foreach (SoilParameter aSoil in Soils)
            {
                if (aSoil.BottomLevel < HcLevel && aSoil.TopLevel > Level)
                {
                    if (aSoil.SoilType == SoilType.Both || aSoil.SoilType == SoilType.Sand)
                    {
                        // D_Sand：插深D内该土层的砂土高度
                        double D_Sand = Math.Min(aSoil.TopLevel, HcLevel) - Math.Max(aSoil.BottomLevel, Level);
                        double c = aSoil.GetSu(Math.Max(aSoil.BottomLevel, Level)); // 抗剪强度su和粘结力c物理含义相同

                        SFRSand += 2 * c * D_Sand * (spudcanB + SpudcanParameter.L);

                        double Ku = GetKu(aSoil.Phi);

                        if (H < D)
                        {
                            double Phi_H = 0;
                            double H_Sand = 0; // H_Sand：插深D~（D-H）内该土层的砂土高度

                            if (aSoil.BottomLevel < Level + H && aSoil.TopLevel > Level)
                            {
                                H_Sand = Math.Min(aSoil.TopLevel, Level + H) - Math.Max(aSoil.BottomLevel, Level);
                                Phi_H = aSoil.Phi;
                            }

                            SFRSand += aSoil.Weight * Ku *
                                (2 * D_Sand * H_Sand * Math.Tan(aSoil.Phi) - H_Sand * H_Sand * Math.Tan(Phi_H)) *
                                (2 * S * spudcanB + SpudcanParameter.L - spudcanB);
                        }
                        else
                        {
                            SFRSand += aSoil.Weight * Ku * Math.Pow(D_Sand, 2) * Math.Tan(aSoil.Phi) *
                                (2 * S * spudcanB + SpudcanParameter.L - spudcanB);
                        }
                    }
                }
            }

            SFR = SFRClay_Shallow + SFRClay_Deep + SFRSand;
            return SFR;
        }

        //根据摩擦角计算判别深度H
        public double GetH(double SoilPhi, double SpudcanB)
        {
            double[] soilPhis = { 20, 25, 30, 35, 40, 45, 48 };
            double[] multipleValues = { 2.5, 3, 4, 5, 7, 9, 11 };

            int count = soilPhis.Length;
            double[,] coeffs = new double[count, 2];

            for (int i = 0; i < count; i++)
            {
                coeffs[i, 0] = soilPhis[i] / 180.0 * Math.PI;
                coeffs[i, 1] = multipleValues[i];
            }

            bool outOfRange = false;
            double hDivideB = GetCoeff(SoilPhi, coeffs, count, ref outOfRange);

            return hDivideB * SpudcanB;
        }

        //根据摩擦角计算形状系数S
        public double GetS(double SoilPhi)
        {
            double[] soilPhis = { 0, 20, 25, 30, 35, 40, 45, 48 };
            double[] figures = { 1, 1.12, 1.3, 1.6, 2.25, 3.45, 5.5, 7.6 };

            int count = soilPhis.Length;
            double[,] coeffs = new double[count, 2];

            for (int i = 0; i < count; i++)
            {
                coeffs[i, 0] = soilPhis[i] / 180.0 * Math.PI;
                coeffs[i, 1] = figures[i];
            }

            bool outOfRange = false;
            double figureCoefficientS = GetCoeff(SoilPhi, coeffs, count, ref outOfRange);

            return figureCoefficientS;
        }

        //根据摩擦角计算H/B比值或S系数（合并方法）
        public double GetHOrS(double SoilPhi, double SpudcanB = 0)
        {
            double hDivideB = 0;
            double figureCoefficientS = 0;

            double[] soilPhis = { 20, 25, 30, 35, 40, 45, 48 };
            double[] multipleValues = { 2.5, 3, 4, 5, 7, 9, 11 };
            double[] figures = { 1.12, 1.3, 1.6, 2.25, 3.45, 5.5, 7.6 };

            for (int i = 0; i < soilPhis.Length - 1; i++)
            {
                double phi1 = soilPhis[i] / 180.0 * Math.PI;
                double phi2 = soilPhis[i + 1] / 180.0 * Math.PI;

                if (SoilPhi >= phi1 && SoilPhi <= phi2)
                {
                    // 线性插值计算 H/B
                    hDivideB = multipleValues[i] +
                               (SoilPhi - phi1) * (multipleValues[i + 1] - multipleValues[i]) / (phi2 - phi1);

                    // 线性插值计算 S
                    figureCoefficientS = figures[i] +
                                         (SoilPhi - phi1) * (figures[i + 1] - figures[i]) / (phi2 - phi1);
                    break;
                }
            }

            if (SpudcanB != 0)
            {
                return hDivideB * SpudcanB;
            }
            else
            {
                return figureCoefficientS;
            }
        }

        //根据摩擦角计算侧压力系数Ku
        public double GetKu(double SoilPhi)
        {
            double[,] kuPhi = new double[,]
            {
        { 4.0 / 180.0 * Math.PI, 0.7 },
        { 6.0 / 180.0 * Math.PI, 0.72 },
        { 8.0 / 180.0 * Math.PI, 0.74 },
        { 10.0 / 180.0 * Math.PI, 0.76 },
        { 12.0 / 180.0 * Math.PI, 0.78 },
        { 14.0 / 180.0 * Math.PI, 0.79 },
        { 16.0 / 180.0 * Math.PI, 0.82 },
        { 18.0 / 180.0 * Math.PI, 0.83 },
        { 20.0 / 180.0 * Math.PI, 0.85 },
        { 22.0 / 180.0 * Math.PI, 0.87 },
        { 24.0 / 180.0 * Math.PI, 0.88 },
        { 26.0 / 180.0 * Math.PI, 0.89 },
        { 28.0 / 180.0 * Math.PI, 0.9 },
        { 30.0 / 180.0 * Math.PI, 0.92 },
        { 32.0 / 180.0 * Math.PI, 0.93 },
        { 34.0 / 180.0 * Math.PI, 0.94 },
        { 36.0 / 180.0 * Math.PI, 0.946 },
        { 38.0 / 180.0 * Math.PI, 0.953 },
        { 40.0 / 180.0 * Math.PI, 0.958 },
        { 42.0 / 180.0 * Math.PI, 0.961 },
        { 44.0 / 180.0 * Math.PI, 0.962 },
        { 45.0 / 180.0 * Math.PI, 0.962 }
            };

            bool outOfRange = false;
            double ku = GetCoeff(SoilPhi, kuPhi, 22, ref outOfRange);
            return ku;
        }

        //计算砂土抗拔力（含三种工况）
        public double[] GetQb_Sand(
        int DeepType,
        CalculateParameter CalculateParameter,
        double Level,
        LegParameter LegParameter,
        SpudcanParameter SpudcanParameter,
        SoilParameter Soil,
        List<SoilParameter> Soils,
        double fb,
        double H,
        ref string Description,
        string QbName = "")
        {
            double[] Qb = new double[3];
            double D = Soils[0].TopLevel - Level;
            SoilParameter averageSoil = GetAverageSoilValue(Soils, Level, Soils[0].TopLevel);

            for (int i = 0; i < 3; i++)
            {
                double S = GetS(averageSoil.Phi); // 最大形状系数S
                double soilPhi, averageSoilPhi;
                double averageSoilc; // 抗剪强度su和粘结力c物理含义相同

                if (i == 2)
                {
                    soilPhi = fb * Soil.Phi;
                    averageSoilPhi = fb * averageSoil.Phi;
                    averageSoilc = fb * averageSoil.Su0;
                }
                else
                {
                    soilPhi = i * Soil.Phi;
                    averageSoilPhi = i * averageSoil.Phi;
                    averageSoilc = i * averageSoil.Su0;
                }

                double Ku = GetKu(soilPhi);
                double HColumn = Math.Max(Soils[0].TopLevel - Level - CalculateParameter.Hc - SpudcanParameter.H3, 0);
                double Vtop = LegParameter.Area * (HColumn - SpudcanParameter.H4);

                // 基础抗拔力
                Qb[i] = SpudcanParameter.Weight +
                        SpudcanParameter.Area * HColumn * averageSoil.Weight -
                        Vtop * averageSoil.Weight +
                        2 * averageSoilc * D * (SpudcanParameter.B + SpudcanParameter.L);

                // 附加抗拔力（根据深埋/浅埋类型）
                double depthFactor = (DeepType == 1) ? Math.Pow(D, 2) : (2 * D - H) * H;

                Qb[i] += averageSoil.Weight *
                         (2 * S * SpudcanParameter.B + SpudcanParameter.L - SpudcanParameter.B) *
                         Ku * Math.Tan(averageSoilPhi) * depthFactor;

                // 构建描述字符串
                string qbLabel = string.IsNullOrEmpty(QbName) ? "Qu" : (i == 2 ? "Qu_Sand" : "Qu_S" + i);
                string depthPart = (DeepType == 1) ? "D^2" : "(2D-H)H";

                Description += qbLabel + $"=2cD(B+L)+γ{depthPart}(2sB+L-B)Kutanφ+W+AHcolumnγ'-Vtopγ'" + Environment.NewLine;

                Description += qbLabel + "=2×" + Math.Round(averageSoilc, 3) + "×" + D + "×(" +
                               Math.Round(SpudcanParameter.B, 3) + "+" + SpudcanParameter.L + ")+" +
                               Math.Round(averageSoil.Weight, 3) + "×" +
                               ((DeepType == 1) ? D + "^2" : "(2×" + D + "-" + H + ")×" + H);

                Description += "×(2×" + Math.Round(S, 3) + "×" + Math.Round(SpudcanParameter.B, 3) + "+" +
                               SpudcanParameter.L + "-" + Math.Round(SpudcanParameter.B, 3) + ")×" +
                               Ku + "×Tan(" + Math.Round(averageSoilPhi, 3) + ")+" +
                               SpudcanParameter.Weight + "+" +
                               Math.Round(SpudcanParameter.Area, 3) + "×" + Math.Round(HColumn, 3) + "×" +
                               Math.Round(averageSoil.Weight, 3) + "-" +
                               Math.Round(Vtop, 3) + "×" + Math.Round(averageSoil.Weight, 3);

                Description += "=" + Math.Round(Qb[i], 3) + ";";
            }

            return Qb;
        }

        //简化版本，使用侧摩阻力计算砂土抗拔力
        public double GetQb_Sand(
        CalculateParameter CalculateParameter,
        double Level,
        LegParameter LegParameter,
        SpudcanParameter SpudcanParameter,
        SoilParameter Soil,
        List<SoilParameter> Soils)
        {
            double Qb = SpudcanParameter.Weight +
                        GetSideFrictionalResistance(CalculateParameter, Level, LegParameter, SpudcanParameter, Soil, Soils);
            return Qb;
        }

        //有限元法主计算流程
        public void CaculateByFiniteElement()
        {
            try
            {
                double waterWeight = 10;
                int i;

                LegParameter legParameter = GetLegParameter();
                SpudcanParameter spudcanParameter = GetSpudcanParameter();
                CalculateParameter calculateParameter = GetCaculateParameter();
                List<SoilParameter> soils = GetSoils(calculateParameter.UnderWaterPhiSubtractValue);
                List<double> computeLevels = new List<double>();

                foreach (DataRow lRow in MyDataSet.Tables["LS_CalculationLevels"].Select("", "Level DESC"))
                {
                    computeLevels.Add(Math.Min(Convert.ToDouble(lRow["Level"]), soils[0].TopLevel - calculateParameter.Hc2));
                }

                // 清空所有相关表
                MyDataSet.Tables["LS_CalculationMaterials"].Rows.Clear();
                MyDataSet.Tables["LS_CalculationNodes"].Rows.Clear();
                MyDataSet.Tables["LS_CalculationAreas"].Rows.Clear();
                MyDataSet.Tables["LS_CalculationEdges"].Rows.Clear();
                MyDataSet.Tables["LS_Contactors"].Rows.Clear();
                MyDataSet.Tables["LS_MeshNodes"].Rows.Clear();
                MyDataSet.Tables["LS_AreaMeshs"].Rows.Clear();
                MyDataSet.Tables["LS_CoupleNodes"].Rows.Clear();
                MyDataSet.Tables["LS_InfiniteMeshs"].Rows.Clear();
                MyDataSet.Tables["LS_PressResistanceResult"].Rows.Clear();
                MyDataSet.Tables["LS_PullResistanceResult"].Rows.Clear();
                MyDataSet.Tables["Ls_ResultOfNodeDisplacement"].Rows.Clear();
                MyDataSet.Tables["LS_ResultOfFace"].Rows.Clear();

                double soilTopLevel = soils[0].TopLevel;
                double legTopLevel = soils[0].TopLevel + (spudcanParameter.H1 + spudcanParameter.H2 + spudcanParameter.H3 + spudcanParameter.H4 + 1);
                double soilBottomLevel = Math.Min(
                    soils[soils.Count - 1].TopLevel - 5,
                    calculateParameter.DestinationLevel - Math.Max(legTopLevel - calculateParameter.DestinationLevel, 15)
                );

                calculateParameter.CaculateL = Math.Max(
                    Math.Round(soilTopLevel - soilBottomLevel, 0),
                    Math.Max(calculateParameter.CaculateL, Math.Round(spudcanParameter.B * 4, 0))
                );

                double minL = calculateParameter.MeshSize;
                EsApplicationParameter parameter = StructureKit.GetApplicationParameter();

                // 第一阶段：生成几何区域和网格
                for (i = 0; i < computeLevels.Count; i++)
                {
                    EsMessageReporter.ReportProgressFunction(100 * (i + 1) / computeLevels.Count);
                    EsMessageReporter.ReportMessageFunction($"有限元计算准备,计算高程={computeLevels[i]}", EsMessageType.Normal);

                    EsTLArea legArea = GetLegShapeArea(legParameter, spudcanParameter, legTopLevel, computeLevels[i]);
                    List<EsTLArea> soilLayerAreas = GetSoilLayerArea(
                        spudcanParameter.B * 0.5,
                        computeLevels[i],
                        soils,
                        calculateParameter,
                        soilBottomLevel
                    );

                    List<EsTLEdge> legEdges = new List<EsTLEdge>();
                    List<EsTLEdge> soilEdges = new List<EsTLEdge>();
                    List<List<AreaEdge>> legAreas = new List<List<AreaEdge>>();
                    List<List<AreaEdge>> soilAreas = new List<List<AreaEdge>>();

                    GenArea(
                        i + 1,
                        computeLevels[i],
                        legTopLevel,
                        soilBottomLevel,
                        calculateParameter.CaculateL,
                        legArea,
                        soilLayerAreas,
                        legAreas,
                        soilAreas,
                        legEdges,
                        soilEdges
                    );

                    EsMessageReporter.ReportMessageFunction($"网格划分开始！计算高程={Math.Round(computeLevels[i], 2)}", EsMessageType.Normal);

                    GenMeshs(
                        i + 1,
                        soilTopLevel,
                        soilBottomLevel,
                        computeLevels[i],
                        minL,
                        calculateParameter.CaculateL,
                        legAreas,
                        soilAreas,
                        legEdges,
                        soilEdges
                    );

                    EsMessageReporter.ReportMessageFunction($"网格划分结束！计算高程={Math.Round(computeLevels[i], 2)}", EsMessageType.Normal);
                }

                double pressQ = 1000;
                double pullQ = 1000;

#if !WEB
                // 第二阶段：有限元计算
                for (i = 0; i < computeLevels.Count; i++)
                {
                    double computeLevel = computeLevels[i];
                    SoilParameter bottomSoil = GetSoil(computeLevel, soils);
                    double sumH = 0;
                    SoilParameter averageSoil = GetAverageSoilValue(soils, computeLevel, soils[0].TopLevel, ref sumH);

                    Dictionary<int, EsContact> contactDictionary = GetEdgeContact(averageSoil, bottomSoil, computeLevel);
                    Dictionary<int, EsMaterial> materialDictionary = GetMaterials(soils, calculateParameter, averageSoil, waterWeight);

                    EsMessageReporter.ReportProgressFunction(100 * (i + 1) / computeLevels.Count);
                    EsMessageReporter.ReportMessageFunction($"有限元计算中,计算高程={Math.Round(computeLevel, 2)}", EsMessageType.Normal);

                    // 获取桩顶边缘信息
                    DataRow[] edgeRows = MyDataSet.Tables["LS_CalculationEdges"].Select($"LevelID={i + 1} and Type=6");
                    double legTopEdgeLength = 0;
                    int legTopEdgeID = 0;

                    if (edgeRows.Length > 0)
                    {
                        DataRow edgerow = edgeRows[0];
                        legTopEdgeID = Convert.ToInt32(edgerow["EdgeID"]);
                        legTopEdgeLength = Math.Sqrt(
                            Math.Pow(Convert.ToDouble(edgerow["x1"]) - Convert.ToDouble(edgerow["x2"]), 2) +
                            Math.Pow(Convert.ToDouble(edgerow["y1"]) - Convert.ToDouble(edgerow["y2"]), 2)
                        );

                        GetFiniteElementPrepare(
                            i + 1,
                            calculateParameter,
                            computeLevel,
                            soils[0].TopLevel,
                            legTopEdgeID,
                            legTopEdgeLength,
                            pressQ,
                            pullQ,
                            materialDictionary,
                            contactDictionary
                        );
                    }

                    EsMessageReporter.ReportMessageFunction($"有限元计算结束,计算高程={Math.Round(computeLevel, 2)}", EsMessageType.Normal);

                    // 计算土重
                    double soilWeight = (spudcanParameter.Volume + legParameter.Area * (soils[0].TopLevel - computeLevel)) * averageSoil.Weight;

                    // ✅ 计算荷载值（使用已在循环中定义的 legTopEdgeLength）
                    double pressLoad = pressQ * Math.Pow(legTopEdgeLength, 2) * Math.PI;
                    double pullLoad = pullQ * Math.Pow(legTopEdgeLength, 2) * Math.PI;

                    GetFiniteElementResult(
                        i + 1,
                        bottomSoil,
                        computeLevel,
                        spudcanParameter.Area,
                        pressLoad,
                        pullLoad,
                        Math.Max(spudcanParameter.Weight - soilWeight, 0)
                    );
                }

                // 计算深度结果
                double pressLimitValue = Math.Round(calculateParameter.PressForce * 9.8, 6);
                string errorString = "";
                CalculateDepthResult(false, pressLimitValue, ref errorString);
                MyDataSet.AcceptChanges();
                EsMessageReporter.ReportMessageFunction("有限元计算结束", EsMessageType.Normal);
#endif
            }
            catch (Exception ex)
            {
                EsMessageReporter.ReportMessageFunction($"有限元计算错误: {ex.Message}", EsMessageType.Error);
            }
        }

        //获取桩腿形状区域（用于有限元网格）
        public EsTLArea GetLegShapeArea(
        LegParameter LegParameter,
        SpudcanParameter SpudcanParameter,
        double CaculateTopLevel,
        double CaculateBottomLevel)
        {
            LegShape legShape = new LegShape();
            legShape.B2 = Math.Round(LegParameter.Diameter, 2);
            legShape.B1 = Math.Round(Math.Sqrt(SpudcanParameter.Area / Math.PI) * 2, 2);
            legShape.B3 = SpudcanParameter.B1;
            legShape.H1 = SpudcanParameter.H1;
            legShape.H2 = SpudcanParameter.H2;
            legShape.H3 = SpudcanParameter.H3;
            legShape.H4 = SpudcanParameter.H4;

            EsTLArea area = legShape.GetArea(CaculateTopLevel, CaculateBottomLevel);
            int materialID = 1000;
            area.Propertys.Add(new EsTLProperty(1, materialID));

            return area;
        }

        //获取土层计算区域（用于有限元网格）
        public List<EsTLArea> GetSoilLayerArea(
        double LegWidth,
        double LegBottomLevel,
        List<SoilParameter> Soils,
        CalculateParameter CalculateParameter,
        double BottomLevel)
        {
            List<EsTLArea> soilAreas = new List<EsTLArea>();
            List<SoilParameter> copySoils = new List<SoilParameter>();
            double hcLevel = Soils[0].TopLevel - CalculateParameter.Hc2; // 洞口顶高程

            // 通过桩底高程和洞口顶部高程，插入新的土层分界点
            for (int i = 0; i < Soils.Count; i++)
            {
                SoilParameter aSoilParameter = Soils[i].GetCopy();
                copySoils.Add(aSoilParameter);

                // 在桩底高程处插入分界
                if (LegBottomLevel > aSoilParameter.BottomLevel && LegBottomLevel < aSoilParameter.TopLevel)
                {
                    aSoilParameter.BottomLevel = LegBottomLevel;
                    aSoilParameter = Soils[i].GetCopy();
                    aSoilParameter.TopLevel = LegBottomLevel;
                    copySoils.Add(aSoilParameter);
                }

                // 在洞口顶部高程处插入分界
                if (CalculateParameter.Hc2 > 0 && hcLevel > aSoilParameter.BottomLevel && hcLevel < aSoilParameter.TopLevel)
                {
                    aSoilParameter.BottomLevel = hcLevel;
                    aSoilParameter = Soils[i].GetCopy();
                    aSoilParameter.TopLevel = hcLevel;
                    copySoils.Add(aSoilParameter);
                }
            }

            // 桩底高程以上，桩靴宽度范围内换土
            for (int i = 0; i < copySoils.Count; i++)
            {
                SoilParameter soil = copySoils[i];

                if (soil.TopLevel > BottomLevel)
                {
                    double bottom = (i == copySoils.Count - 1) ? BottomLevel : soil.BottomLevel;

                    // 右侧土层区域（桩靴宽度以外）
                    EsTLArea area = new EsTLArea();
                    int materialID = soil.SoilID + 100;
                    area.Propertys.Add(new EsTLProperty(1, materialID));
                    area.Type = 2; // 表示土层区域

                    area.Points.Add(new EsTLPoint2D(0, LegWidth, bottom));
                    area.Points.Add(new EsTLPoint2D(0, CalculateParameter.CaculateL, bottom));
                    area.Points.Add(new EsTLPoint2D(0, CalculateParameter.CaculateL, soil.TopLevel));
                    area.Points.Add(new EsTLPoint2D(0, LegWidth, soil.TopLevel));

                    area.Area = EsTLGeometry.ComputeArea(area.Points, ref area.x, ref area.y);
                    soilAreas.Add(area);

                    // 左侧土层区域（桩靴宽度以内，洞口以下）
                    if ((soil.TopLevel + soil.BottomLevel) * 0.5 < hcLevel)
                    {
                        EsTLArea innerArea = new EsTLArea();
                        int innerMaterialID = ((soil.TopLevel + soil.BottomLevel) * 0.5 > LegBottomLevel)
                            ? soil.SoilID + 400
                            : soil.SoilID + 100;
                        innerArea.Propertys.Add(new EsTLProperty(1, innerMaterialID));
                        innerArea.Type = 2; // 表示土层区域

                        innerArea.Points.Add(new EsTLPoint2D(0, 0, bottom));
                        innerArea.Points.Add(new EsTLPoint2D(0, LegWidth, bottom));
                        innerArea.Points.Add(new EsTLPoint2D(0, LegWidth, soil.TopLevel));
                        innerArea.Points.Add(new EsTLPoint2D(0, 0, soil.TopLevel));

                        innerArea.Area = EsTLGeometry.ComputeArea(innerArea.Points, ref innerArea.x, ref innerArea.y);
                        soilAreas.Add(innerArea);
                    }
                }
            }

            return soilAreas;
        }

        //生成计算区域并写入数据表
        public void GenArea(
        int LevelID,
        double LegDownLevel,
        double LegTopLevel,
        double SoilBottomLevel,
        double SoilWidth,
        EsTLArea LegArea,
        List<EsTLArea> SoilLayerAreas,
        List<List<AreaEdge>> LegAreaEdges,
        List<List<AreaEdge>> SoilAreaEdges,
        List<EsTLEdge> LegEdges,
        List<EsTLEdge> SoilEdges)
        {
            var areas = new List<EsTLArea>();
            var legAreas = new List<EsTLArea>();
            var soilAreas = new List<EsTLArea>();
            var mergedPoints = new List<EsTLPoint2D>();
            var edges = new List<EsTLEdge>();

            int areaEdgeID = 0;
            double localLegDownLevel = LegDownLevel;
            double localLegDownB = 0;

            GetMergerArea(LegArea, SoilLayerAreas, mergedPoints, areas);

            // 写入节点
            foreach (EsTLPoint2D p in mergedPoints)
            {
                DataRow row = MyDataSet.Tables["LS_CalculationNodes"].NewRow();
                row["LevelID"] = LevelID;
                row["NodeID"] = p.ID;
                row["x"] = p.x;
                row["y"] = p.y;
                MyDataSet.Tables["LS_CalculationNodes"].Rows.Add(row);
            }

            // 处理每个区域
            foreach (EsTLArea area in areas)
            {
                var currentSoilAreas = new List<AreaEdge>();
                List<EsTLEdge> areaEdges = area.GetEdges();

                foreach (EsTLEdge areaEdge in areaEdges)
                {
                    areaEdge.ID = 0;
                }

                if (area.Type == 1)
                {
                    edges = LegEdges;
                    LegAreaEdges.Add(currentSoilAreas);
                    legAreas.Add(area);
                }
                else
                {
                    edges = SoilEdges;
                    SoilAreaEdges.Add(currentSoilAreas);
                    soilAreas.Add(area);
                }

                foreach (EsTLEdge areaEdge in areaEdges)
                {
                    var aSoilArea = new AreaEdge();

                    foreach (EsTLEdge edge in edges)
                    {
                        if (areaEdge.IsSameGeometry(edge))
                        {
                            if (areaEdge.P1.IsSame(edge.P2))
                            {
                                aSoilArea.Orientation = 1; // 反向
                            }
                            aSoilArea.Edge = edge;
                            areaEdge.ID = edge.ID;
                            edge.Tag += 1;
                            break;
                        }
                    }

                    if (areaEdge.ID == 0)
                    {
                        areaEdgeID += 1;
                        areaEdge.ID = areaEdgeID;
                        areaEdge.Tag = "0";
                        edges.Add(areaEdge);
                        aSoilArea.Edge = areaEdge;
                    }

                    currentSoilAreas.Add(aSoilArea);
                }
            }

            // 写入腿的边界
            foreach (EsTLEdge edge in LegEdges)
            {
                // 顶部边界
                if (Math.Abs(edge.P1.y - LegTopLevel) < 0.001)
                {
                    edge.Type = 6;
                }

                // 中心边界
                if (Math.Abs(edge.P1.x) < 0.001 && Math.Abs(edge.P2.x) < 0.001)
                {
                    edge.Type = 1;
                }

                // 桩底边界
                double distance = edge.P2.GetDistance(edge.P1);
                if (distance > 0.001)
                {
                    double vx = (edge.P2.x - edge.P1.x) / distance;
                    double vy = (edge.P2.y - edge.P1.y) / distance;

                    if (vx > 0.01 && edge.Tag == "0")
                    {
                        edge.Type = 4;
                        localLegDownLevel = Math.Max(localLegDownLevel, edge.P1.y);
                        localLegDownLevel = Math.Max(localLegDownLevel, edge.P2.y);
                        localLegDownB = Math.Max(localLegDownB, edge.P1.x);
                        localLegDownB = Math.Max(localLegDownB, edge.P2.x);
                    }
                }

                // 桩侧（向左倾斜）
                if (edge.Type == 0 && edge.P2.x + 0.5 < edge.P1.x)
                {
                    edge.Type = 7;
                }

                // 桩侧（默认）
                if (edge.Type == 0)
                {
                    edge.Type = 5;
                }
            }

            // 写入土层边界
            foreach (EsTLEdge edge in SoilEdges)
            {
                // 中心边界
                if (Math.Abs(edge.P1.x) < 0.001 && Math.Abs(edge.P2.x) < 0.001)
                {
                    edge.Type = 1;
                }

                // 底部边界
                if (Math.Abs(edge.P1.y - SoilBottomLevel) < 0.001 && Math.Abs(edge.P2.y - SoilBottomLevel) < 0.001)
                {
                    edge.Type = 2;
                }

                // 侧面边界
                if (Math.Abs(edge.P1.x - SoilWidth) < 0.001 && Math.Abs(edge.P2.x - SoilWidth) < 0.001)
                {
                    edge.Type = 3;
                }

                // 桩底边界
                double distance = edge.P2.GetDistance(edge.P1);
                if (distance > 0.001)
                {
                    double vx = (edge.P2.x - edge.P1.x) / distance;
                    double vy = (edge.P2.y - edge.P1.y) / distance;

                    if (vx < -0.001 &&
                        edge.P1.y < localLegDownLevel + 0.001 &&
                        edge.P2.y < localLegDownLevel + 0.001 &&
                        edge.P1.x < localLegDownB + 0.001 &&
                        edge.P2.x < localLegDownB + 0.001)
                    {
                        edge.Type = 4;
                    }
                }

                // 与桩腿重合的边界
                foreach (EsTLEdge legEdge in LegEdges)
                {
                    if (legEdge.IsSameGeometry(edge))
                    {
                        edge.Type = legEdge.Type;
                        break;
                    }
                }
            }

            // 写入区域数据
            var allAreas = new List<EsTLArea>();
            allAreas.AddRange(legAreas);
            allAreas.AddRange(soilAreas);

            var allAreaEdges = new List<List<AreaEdge>>();
            allAreaEdges.AddRange(LegAreaEdges);
            allAreaEdges.AddRange(SoilAreaEdges);

            for (int i = 0; i < allAreas.Count; i++)
            {
                EsTLArea area = allAreas[i];
                DataRow row = MyDataSet.Tables["LS_CalculationAreas"].NewRow();
                row["LevelID"] = LevelID;
                row["AreaID"] = i + 1;
                row["x0"] = area.x;
                row["y0"] = area.y;
                row["MaterialID"] = area.GetPropertyID(1);

                // 判断位置
                row["Location"] = (area.y > localLegDownLevel) ? 1 : -1;

                if (area.Tag != 0)
                {
                    row["BeforeMaterialID"] = area.Tag;
                }

                row["Edges"] = GetEdgeString(allAreaEdges[i]);
                MyDataSet.Tables["LS_CalculationAreas"].Rows.Add(row);
            }
        }

        //合并桩腿区域和土层区域
        public void GetMergerArea(
        EsTLArea legShapeArea,
        List<EsTLArea> soilLayerAreas,
        List<EsTLPoint2D> mergedPoints,
        List<EsTLArea> areas)
        {
            legShapeArea.ID = 1;
            legShapeArea.Type = 1;

            areas.Clear();
            mergedPoints.Clear();

            List<EsTLArea> addAreas = EsTLArea.GetAddAreas(soilLayerAreas, legShapeArea);

            int areaID = 1;
            int pointID = 1;

            foreach (EsTLArea addArea in addAreas)
            {
                addArea.ID = areaID;
                areaID++;

                // 使用 LINQ 检查点是否已存在
                foreach (EsTLPoint2D point in addArea.Points)
                {
                    bool exists = mergedPoints.Any(p => p.IsSame(point));

                    if (!exists)
                    {
                        point.ID = pointID;
                        pointID++;
                        mergedPoints.Add(point);
                    }
                }

                areas.Add(addArea);
            }
        }

        //将边缘列表转换为字符串格式（EdgeID,Orientation;...）
        public string GetEdgeString(List<AreaEdge> edges)
        {
            string result = "";

            for (int i = 0; i < edges.Count; i++)
            {
                if (i == 0)
                {
                    result = edges[i].Edge.ID.ToString() + "," + edges[i].Orientation.ToString();
                }
                else
                {
                    result = result + ";" + edges[i].Edge.ID.ToString() + "," + edges[i].Orientation.ToString();
                }
            }

            return result;
        }

        //网格划分并写入结果
        public void GenMeshs(
        int LevelID,
        double TopLevel,
        double BottomLevel,
        double LegBottomLevel,
        double MinL,
        double SoilWidth,
        List<List<AreaEdge>> LegAreas,
        List<List<AreaEdge>> SoilAreas,
        List<EsTLEdge> LegEdges,
        List<EsTLEdge> SoilEdges)
        {
            var controlPoints = new List<EsTLMeshControlPoint>();
            var point = new EsTLMeshControlPoint();

            // 对进行网格划分
            var toMeshLegEdges = new List<EsTLEdge>();
            var toMeshLegAreas = new List<List<EsTLAreaEdge>>();
            var legNodes = new List<EsTLPoint>();
            var legTriMeshs = new List<List<EsTLTriMesh>>();

            var toMeshSoilEdges = new List<EsTLEdge>();
            var toMeshSoilAreas = new List<List<EsTLAreaEdge>>();
            var soilNodes = new List<EsTLPoint>();
            var soilTriMeshs = new List<List<EsTLTriMesh>>();

            // 腿的有限元划分
            foreach (var edge in LegEdges)
            {
                toMeshLegEdges.Add(edge);
            }

            foreach (var areaEdge in LegAreas)
            {
                var meshAreaEdge = new List<EsTLAreaEdge>();
                toMeshLegAreas.Add(meshAreaEdge);
                foreach (var edge in areaEdge)
                {
                    meshAreaEdge.Add(edge);
                }
            }

            int startNodeID = 0;
            int startMeshID = 0;

            // 土层的有限元划分
            foreach (var edge in SoilEdges)
            {
                toMeshSoilEdges.Add(edge);
            }

            foreach (var areaEdge in SoilAreas)
            {
                var meshAreaEdge = new List<EsTLAreaEdge>();
                toMeshSoilAreas.Add(meshAreaEdge);
                foreach (var edge in areaEdge)
                {
                    meshAreaEdge.Add(edge);
                }
            }

            // 设置控制点
            foreach (var edge in toMeshLegEdges)
            {
                point = new EsTLMeshControlPoint
                {
                    x = edge.P1.x,
                    y = edge.P1.y,
                    MaxL = MinL,
                    MinL = MinL * 0.25,
                    Radius = MinL * 5,
                    RadiusCoeff = 1
                };
                controlPoints.Add(point);
            }

            point = new EsTLMeshControlPoint
            {
                x = 0,
                y = LegBottomLevel,
                MaxL = MinL * 4,
                MinL = MinL * 0.25,
                Radius = SoilWidth,
                RadiusCoeff = 1
            };
            controlPoints.Add(point);

            var contactorEdges = new Dictionary<EsTLEdge, EsTLEdge>();

            // 设置腿与土层的接触
            int contactorID = 1;
            startNodeID = 0;

            for (int edgeType = 4; edgeType <= 7; edgeType++)
            {
                var edge1s = new List<EsTLEdge>();
                var edge2s = new List<EsTLEdge>();

                foreach (var edge in LegEdges)
                {
                    if (edge.Type == edgeType)
                    {
                        edge1s.Add(edge);
                    }
                }

                foreach (var edge in SoilEdges)
                {
                    if (edge.Type == edgeType)
                    {
                        edge2s.Add(edge);
                    }
                }

                if (edge1s.Count > 0 && edge2s.Count > 0)
                {
                    foreach (var edge1 in edge1s)
                    {
                        foreach (var edge2 in edge2s)
                        {
                            if (edge1.IsSameGeometry(edge2))
                            {
                                GenContactorEdgeMesh(MinL, edge1, controlPoints, ref startNodeID, legNodes);
                                contactorEdges.Add(edge1, edge2);
                            }
                        }
                    }
                }
            }

            // 生成腿的网格
            EsTLDelaunayTriMeshGenerator.GenAreaMeshByEdge(
                MinL, controlPoints, toMeshLegEdges, toMeshLegAreas,
                legNodes, legTriMeshs, ref startNodeID, ref startMeshID);

            // 处理接触边
            var contactorKeys = new List<EsTLEdge>(contactorEdges.Keys);
            for (int i = 0; i < contactorKeys.Count; i++)
            {
                var edge1 = contactorKeys[i];
                var edge2 = contactorEdges[edge1];

                var edgeNodes2 = new List<EsTLPoint>(edge1.MeshNodes);
                edgeNodes2.Reverse();

                foreach (var aPoint in edgeNodes2)
                {
                    var p = new EsTLPoint(0, aPoint.x, aPoint.y, 0);
                    IsNodeExist(soilNodes, ref p, ref startNodeID);
                    edge2.MeshNodes.Add(p);
                }
            }

            // 生成土层的网格
            EsTLDelaunayTriMeshGenerator.GenAreaMeshByEdge(
                MinL, controlPoints, toMeshSoilEdges, toMeshSoilAreas,
                soilNodes, soilTriMeshs, ref startNodeID, ref startMeshID);

            // 合并节点和单元
            var nodes = new List<EsTLPoint>();
            nodes.AddRange(legNodes);
            nodes.AddRange(soilNodes);

            var triMeshs = new List<List<EsTLTriMesh>>();
            triMeshs.AddRange(legTriMeshs);
            triMeshs.AddRange(soilTriMeshs);

            var allEdges = new List<EsTLEdge>();
            allEdges.AddRange(LegEdges);
            allEdges.AddRange(SoilEdges);

            // 无限单元生成参数
            var infiniteMeshGeneratorParameters = new EsInfinite2DMeshGenParamter
            {
                MeshSize = 5
            };
            infiniteMeshGeneratorParameters.EdgeMeshs.Clear();
            infiniteMeshGeneratorParameters.NodeIndexMapToID.Clear();

            // 写入节点
            foreach (var node in nodes)
            {
                var row = MyDataSet.Tables["LS_MeshNodes"].NewRow();
                row["LevelID"] = LevelID;
                row["NodeID"] = node.ID;
                row["x"] = node.x;
                row["y"] = node.y;
                MyDataSet.Tables["LS_MeshNodes"].Rows.Add(row);
                infiniteMeshGeneratorParameters.NodeIndexMapToID.Add(
                    node.ID, new EsNode(node.ID, 0, node.x, node.y, 0));
            }

            // 写入单元
            for (int j = 0; j < triMeshs.Count; j++)
            {
                var meshs = triMeshs[j];
                foreach (var mesh in meshs)
                {
                    var row = MyDataSet.Tables["LS_AreaMeshs"].NewRow();
                    row["LevelID"] = LevelID;
                    row["AreaID"] = j + 1;
                    row["MeshID"] = mesh.ID;
                    row["N1"] = mesh.P1.ID;
                    row["N2"] = mesh.P2.ID;
                    row["N3"] = mesh.P3.ID;
                    MyDataSet.Tables["LS_AreaMeshs"].Rows.Add(row);
                }
            }

            var rightSideNodes = new Dictionary<int, EsTLPoint>();

            // 写入边缘
            foreach (var edge in allEdges)
            {
                var row = MyDataSet.Tables["LS_CalculationEdges"].NewRow();
                row["LevelID"] = LevelID;
                row["EdgeID"] = edge.ID;
                row["x1"] = edge.P1.x;
                row["y1"] = edge.P1.y;
                row["x2"] = edge.P2.x;
                row["y2"] = edge.P2.y;
                row["Type"] = edge.Type;

                switch (edge.Type)
                {
                    case 1:
                        row["SupportID"] = 100; // 水平支撑
                        break;
                    case 3:
                        foreach (var node in edge.MeshNodes)
                        {
                            if (!rightSideNodes.ContainsKey(node.ID) && node.y > BottomLevel + 0.1)
                            {
                                rightSideNodes.Add(node.ID, new EsTLPoint(0, node.x, node.y, node.z));
                            }
                        }
                        break;
                }

                row["MeshNodes"] = GetNodeString(edge.MeshNodes);
                MyDataSet.Tables["LS_CalculationEdges"].Rows.Add(row);
            }

            // 利用边单元划分无限单元
            var infinite2DMesh = new EsInfinite2DMesh();
            var infiniteNodes = new List<EsNode>();
            startNodeID = nodes.Count;
            startMeshID = 1;

            foreach (var edge in SoilEdges)
            {
                if (edge.Type == 2 || edge.Type == 3)
                {
                    var edgeMesh = new EsMesh
                    {
                        LocalCoordinate = new EsAxis(),
                        ID = edge.ID
                    };
                    foreach (var node in edge.MeshNodes)
                    {
                        edgeMesh.NodeIDs.Add(node.ID);
                    }
                    infiniteMeshGeneratorParameters.EdgeMeshs.Add(edgeMesh);
                }
            }

            // 通过边生成无限区域
            infiniteMeshGeneratorParameters.GenMesh(ref startNodeID, ref startMeshID, infinite2DMesh, infiniteNodes);

            // 增加边与无限元的耦合
            int coupleNodeID = 0;
            int nodeID = startNodeID;

            foreach (var rightSideNode in rightSideNodes)
            {
                var row = MyDataSet.Tables["LS_MeshNodes"].NewRow();
                nodeID++;
                row["LevelID"] = LevelID;
                row["NodeID"] = nodeID;
                row["x"] = rightSideNode.Value.x;
                row["y"] = rightSideNode.Value.y;
                MyDataSet.Tables["LS_MeshNodes"].Rows.Add(row);

                foreach (var mesh in infinite2DMesh.Meshs)
                {
                    for (int i = 0; i < mesh.NodeIDs.Count; i++)
                    {
                        if (mesh.NodeIDs[i] == rightSideNode.Key)
                        {
                            mesh.NodeIDs[i] = nodeID;
                        }
                    }
                }

                coupleNodeID++;
                var coupleRow = MyDataSet.Tables["LS_CoupleNodes"].NewRow();
                coupleRow["LevelID"] = LevelID;
                coupleRow["CoupleNodeID"] = coupleNodeID;
                coupleRow["N1"] = rightSideNode.Key;
                coupleRow["N2"] = nodeID;
                coupleRow["CoupleID"] = 100;
                MyDataSet.Tables["LS_CoupleNodes"].Rows.Add(coupleRow);
            }

            // 写入无限节点
            foreach (var node in infiniteNodes)
            {
                var row = MyDataSet.Tables["LS_MeshNodes"].NewRow();
                row["LevelID"] = LevelID;
                row["NodeID"] = node.ID;
                row["x"] = node.x;
                row["y"] = node.y;
                MyDataSet.Tables["LS_MeshNodes"].Rows.Add(row);
            }

            // 写入无限区域
            foreach (var mesh in infinite2DMesh.Meshs)
            {
                var row = MyDataSet.Tables["LS_InfiniteMeshs"].NewRow();
                row["LevelID"] = LevelID;
                row["MeshID"] = mesh.ID;
                row["N1"] = mesh.NodeIDs[0];
                row["N2"] = mesh.NodeIDs[1];
                row["N3"] = mesh.NodeIDs[2];
                row["N4"] = mesh.NodeIDs[3];
                row["N5"] = mesh.NodeIDs[4];
                row["N6"] = mesh.NodeIDs[5];
                MyDataSet.Tables["LS_InfiniteMeshs"].Rows.Add(row);
            }

            // 写入接触器
            for (int i = 0; i < contactorKeys.Count; i++)
            {
                var edge1 = contactorKeys[i];
                var edge2 = contactorEdges[edge1];

                var row = MyDataSet.Tables["LS_Contactors"].NewRow();
                row["LevelID"] = LevelID;
                row["ID"] = i + 1;

                switch (edge1.Type)
                {
                    case 4:
                        row["ContactorID"] = 100; // 桩靴底完全接触
                        break;
                    case 5:
                        row["ContactorID"] = 101; // 桩侧完全接触
                        break;
                    case 7:
                        row["ContactorID"] = 102; // 桩靴面完全接触
                        break;
                }

                row["Edge1s"] = edge1.ID;
                row["Edge2s"] = edge2.ID;

                string nodes1 = "", nodes2 = "";
                for (int j = 0; j < edge1.MeshNodes.Count; j++)
                {
                    if (j == 0)
                        nodes1 = edge1.MeshNodes[j].ID.ToString();
                    else
                        nodes1 = nodes1 + "," + edge1.MeshNodes[j].ID.ToString();
                }

                for (int j = edge2.MeshNodes.Count - 1; j >= 0; j--)
                {
                    if (j == edge2.MeshNodes.Count - 1)
                        nodes2 = edge2.MeshNodes[j].ID.ToString();
                    else
                        nodes2 = nodes2 + "," + edge2.MeshNodes[j].ID.ToString();
                }

                var vx = edge1.GetVector();
                vx.NormalVector();

                var axis = new EsAxis(
                    edge1.MeshNodes[0].x, edge1.MeshNodes[0].y, 0,
                    new EsVector(vx.Vx, vx.Vy, 0),
                    new EsVector(-vx.Vy, vx.Vx, 0),
                    new EsVector(0, 0, 1));

                row["Nodes1"] = nodes1;
                row["Nodes2"] = nodes2;
                row["LocalCoordinate"] = axis.GetString();
                MyDataSet.Tables["LS_Contactors"].Rows.Add(row);
                contactorID++;
            }
        }

        //在接触边上生成网格节点
        public void GenContactorEdgeMesh(
        double MinL,
        EsTLEdge Edge,
        List<EsTLMeshControlPoint> ControlPoints,
        ref int NodeID,
        List<EsTLPoint> Nodes)
        {
            int nControlPoint = ControlPoints.Count;
            double[,] controlPointValues = new double[1000, 6];

            for (int i = 0; i < ControlPoints.Count; i++)
            {
                controlPointValues[i + 1, 0] = ControlPoints[i].x;
                controlPointValues[i + 1, 1] = ControlPoints[i].y;
                controlPointValues[i + 1, 2] = ControlPoints[i].Radius;
                controlPointValues[i + 1, 3] = ControlPoints[i].MinL;
                controlPointValues[i + 1, 4] = ControlPoints[i].MaxL;
                controlPointValues[i + 1, 5] = ControlPoints[i].RadiusCoeff;
            }

            double distance = Edge.P1.GetDistance(Edge.P2);
            double dx = (Edge.P2.x - Edge.P1.x) / distance;
            double dy = (Edge.P2.y - Edge.P1.y) / distance;

            IsNodeExist(Nodes, ref Edge.P1, ref NodeID);
            IsNodeExist(Nodes, ref Edge.P2, ref NodeID);
            Edge.MeshNodes.Add(Edge.P1);

            do
            {
                EsTLPoint lastPoint = Edge.MeshNodes[Edge.MeshNodes.Count - 1];
                double localL = GetLocalL(MinL, lastPoint.x, lastPoint.y, controlPointValues, nControlPoint);

                double distToEnd = Math.Sqrt(
                    Math.Pow(Edge.P2.x - lastPoint.x, 2) +
                    Math.Pow(Edge.P2.y - lastPoint.y, 2)
                );

                if (distToEnd < localL * 1.5)
                {
                    break;
                }
                else
                {
                    NodeID++;
                    var p = new EsTLPoint(
                        NodeID,
                        Math.Round(lastPoint.x + dx * localL, 2),
                        Math.Round(lastPoint.y + dy * localL, 2),
                        0
                    );
                    Nodes.Add(p);
                    Edge.MeshNodes.Add(p);
                }
            } while (true);

            Edge.MeshNodes.Add(Edge.P2);
        }

        //检查节点是否已存在，存在则复用ID，否则创建新节点
        public static void IsNodeExist(List<EsTLPoint> Nodes, ref EsTLPoint Node, ref int NodeID)
        {
            foreach (EsTLPoint aNode in Nodes)
            {
                if (aNode.IsSame(Node))
                {
                    Node.ID = aNode.ID;
                    return;
                }
            }

            NodeID++;
            Node.ID = NodeID;
            Nodes.Add(Node);
        }

        //根据控制点计算局部网格尺寸
        public static double GetLocalL(double MinL, double x, double y, double[,] ControlPoints, int NControlPoint)
        {
            double localL = MinL;

            for (int i = 1; i <= NControlPoint; i++)
            {
                double r = Math.Sqrt(
                    Math.Pow(ControlPoints[i, 0] - x, 2) +
                    Math.Pow(ControlPoints[i, 1] - y, 2)
                );

                double xs = r / ControlPoints[i, 2];
                double minL = ControlPoints[i, 3] * (1 - xs) + MinL * xs;

                if (minL < localL)
                {
                    localL = minL;
                }
            }

            return localL;
        }

        //将节点列表转换为逗号分隔的ID字符串（用于数据存储）
        public string GetNodeString(List<EsTLPoint> points)
        {
            string result = "";

            for (int i = 0; i < points.Count; i++)
            {
                if (i == 0)
                {
                    result = points[i].ID.ToString();
                }
                else
                {
                    result = result + "," + points[i].ID.ToString();
                }
            }

            return result;
        }

        public string GetNodeString(List<EsTLPoint2D> points)
        {
            string result = "";

            for (int i = 0; i < points.Count; i++)
            {
                if (i == 0)
                {
                    result = points[i].ID.ToString();
                }
                else
                {
                    result = result + "," + points[i].ID.ToString();
                }
            }

            return result;
        }

        //创建并返回接触类型字典
        public Dictionary<int, EsContact> GetEdgeContact(
        SoilParameter AverageSoil,
        SoilParameter BottomSoil,
        double BottomLevel)
        {
            double cohesionCoeff = Convert.ToDouble(
                MyDataSet.Tables["LS_CalculationParameter"].Rows[0]["cohesionCoeff"]
            );

            var contactDictionary = new Dictionary<int, EsContact>();

            // 1. 桩靴底完全接触（第一阶段土与土的接触）
            var aContact = new EsContact
            {
                Name = "桩靴底完全接触",
                ID = 100,
                Kt = 10000000000.0,
                Kn = 10000000000.0,
                Cn = 0,
                Ct = 0,
                FrictionCoeff = 0,
                KeepContacting = true,
                Deflection = 0.001,
                InitialState = 0
            };
            contactDictionary.Add(aContact.ID, aContact);

            // 2. 桩侧光滑接触（第一阶段土与土的接触）
            aContact = new EsContact
            {
                Name = "桩侧光滑接触",
                ID = 101,
                Kt = 0,
                Kn = 10000000000.0,
                Cn = 0,
                Ct = 0,
                FrictionCoeff = 0,
                KeepContacting = true,
                Deflection = 0.001,
                InitialState = 0
            };
            contactDictionary.Add(aContact.ID, aContact);

            // 3. 桩靴面完全接触（第一阶段土与土的接触）
            aContact = new EsContact
            {
                Name = "桩靴面完全接触",
                ID = 102,
                Kt = 10000000000.0,
                Kn = 10000000000.0,
                Cn = 0,
                Ct = 0,
                FrictionCoeff = 0,
                KeepContacting = true,
                Deflection = 0.001,
                InitialState = 0
            };
            contactDictionary.Add(aContact.ID, aContact);

            // 4. 桩靴底受压接触
            double su = BottomSoil.GetSu(BottomLevel);
            aContact = new EsContact
            {
                Name = "桩靴底受压接触",
                ID = 103,
                Kt = 10000000000.0,
                Kn = 10000000000.0,
                Ct = su,
                Cn = su * cohesionCoeff,
                FrictionCoeff = 0,
                KeepContacting = false,
                Deflection = 0.001,
                InitialState = 0
            };
            contactDictionary.Add(aContact.ID, aContact);

            // 5. 桩侧完全接触
            aContact = new EsContact
            {
                Name = "桩侧完全接触",
                ID = 104,
                Kt = 10000000000.0,
                Kn = 10000000000.0,
                Cn = 0,
                Ct = 0,
                FrictionCoeff = 0,
                KeepContacting = true,
                Deflection = 0.001,
                InitialState = 0
            };
            contactDictionary.Add(aContact.ID, aContact);

            // 6. 桩靴面受压接触
            aContact = new EsContact
            {
                Name = "桩靴面受压接触",
                ID = 105,
                Kt = 10000000000.0,
                Kn = 10000000000.0,
                Cn = 0,
                Ct = 0,
                FrictionCoeff = 0,
                KeepContacting = false,
                Deflection = 0.001,
                InitialState = 0
            };
            contactDictionary.Add(aContact.ID, aContact);

            return contactDictionary;
        }

        //创建并返回材料字典（钢材、土壤、弹性、折减等）
        public Dictionary<int, EsMaterial> GetMaterials(
        List<SoilParameter> Soils,
        CalculateParameter CalculateParameter,
        SoilParameter AverageSoil,
        double WaterWeight)
        {
            var materialDictionary = new Dictionary<int, EsMaterial>();

            // 1. 桩腿材料（钢材）
            var aMaterial = new EsMaterial
            {
                ID = 1000,
                MaterialType = EsMaterialType.Steel,
                ElasticPlasticType = EsMaterialElasticPlasticType.LinearElastic,
                Name = "桩腿"
            };
            aMaterial.SoilProperty.Cohesion = 0;
            aMaterial.SoilProperty.SaturatedUnitWeight = AverageSoil.Weight + WaterWeight;
            aMaterial.UnitWeight = aMaterial.SoilProperty.SaturatedUnitWeight;
            aMaterial.ElasticProperty.Type = 1;
            aMaterial.ElasticProperty.E = 200000000.0;
            aMaterial.ElasticProperty.Mu = 0.3;
            aMaterial.ElasticProperty.IsAutoK0 = false;
            materialDictionary.Add(aMaterial.ID, aMaterial);

            // 2. 无限区域材料
            var infiniteMaterial = new EsMaterial
            {
                ID = 1001,
                MaterialType = EsMaterialType.Soil,
                ElasticPlasticType = EsMaterialElasticPlasticType.LinearElastic,
                Name = "无限区域"
            };
            infiniteMaterial.PlasticProperty.DruckerPragerProperty.ModeType = 1;
            infiniteMaterial.PlasticProperty.DruckerPragerProperty.FrictionAngle = 0;
            infiniteMaterial.PlasticProperty.DruckerPragerProperty.FlowAngle = 0;
            infiniteMaterial.PlasticProperty.DruckerPragerProperty.Cohesion = 0;
            infiniteMaterial.SoilProperty.Cohesion = 0;
            infiniteMaterial.SoilProperty.SaturatedUnitWeight = AverageSoil.Weight + WaterWeight;
            infiniteMaterial.UnitWeight = AverageSoil.Weight + WaterWeight;
            infiniteMaterial.ElasticProperty.Type = 1;
            infiniteMaterial.ElasticProperty.E = 10000;
            infiniteMaterial.ElasticProperty.Mu = 0.4;
            infiniteMaterial.ElasticProperty.IsAutoK0 = true;
            materialDictionary.Add(infiniteMaterial.ID, infiniteMaterial);

            // 3. 土壤材料
            double sumE = 0;
            double sumMu = 0;
            int nMaterial = 0;

            foreach (DataRow soilRow in MyDataSet.Tables["LS_Soil"].Rows)
            {
                double topLevel = Soils[0].TopLevel;
                foreach (SoilParameter aSoil in Soils)
                {
                    if (aSoil.Name == soilRow["Name"].ToString())
                    {
                        topLevel = aSoil.TopLevel;
                        break;
                    }
                }

                // 3.1 塑性材料（正常）
                aMaterial = new EsMaterial
                {
                    ID = Convert.ToInt32(soilRow["ID"]) + 100,
                    MaterialType = EsMaterialType.Soil,
                    ElasticPlasticType = EsMaterialElasticPlasticType.Plastic,
                    Name = soilRow["Name"].ToString()
                };
                aMaterial.PlasticProperty.Type = 3;
                aMaterial.PlasticProperty.DruckerPragerProperty.ModeType = CalculateParameter.DPType;

                double underWaterPhi = Convert.ToDouble(soilRow["UnderWaterPhi"]);
                if ((SoilType)Convert.ToInt32(soilRow["Type"]) == SoilType.Sand)
                {
                    aMaterial.PlasticProperty.DruckerPragerProperty.FrictionAngle =
                        underWaterPhi - CalculateParameter.UnderWaterPhiSubtractValue;
                }
                else
                {
                    aMaterial.PlasticProperty.DruckerPragerProperty.FrictionAngle = underWaterPhi;
                }
                aMaterial.PlasticProperty.DruckerPragerProperty.FlowAngle =
                    aMaterial.PlasticProperty.DruckerPragerProperty.FrictionAngle;
                aMaterial.PlasticProperty.DruckerPragerProperty.Cohesion = Convert.ToDouble(soilRow["Su0"]);
                aMaterial.PlasticProperty.DruckerPragerProperty.DCohesion = Convert.ToDouble(soilRow["DSu"]);
                aMaterial.PlasticProperty.DruckerPragerProperty.Depth0 = topLevel;
                aMaterial.PlasticProperty.DruckerPragerProperty.LimitStresst =
                    aMaterial.PlasticProperty.DruckerPragerProperty.Cohesion;
                aMaterial.SoilProperty.Cohesion = aMaterial.PlasticProperty.DruckerPragerProperty.Cohesion;
                aMaterial.SoilProperty.SaturatedUnitWeight = Convert.ToDouble(soilRow["UnderWaterWeight"]) + WaterWeight;
                aMaterial.UnitWeight = Convert.ToDouble(soilRow["UnderWaterWeight"]) + WaterWeight;
                aMaterial.ElasticProperty.E = Convert.ToDouble(soilRow["E"]);
                aMaterial.ElasticProperty.Mu = Convert.ToDouble(soilRow["Mu"]);
                aMaterial.ElasticProperty.IsAutoK0 = true;
                materialDictionary.Add(aMaterial.ID, aMaterial);

                // 3.2 弹性材料（桩靴以上）
                aMaterial = new EsMaterial
                {
                    ID = Convert.ToInt32(soilRow["ID"]) + 200,
                    MaterialType = EsMaterialType.Soil,
                    ElasticPlasticType = EsMaterialElasticPlasticType.LinearElastic,
                    Name = soilRow["Name"] + "_弹性(桩靴以上)"
                };
                aMaterial.SoilProperty.SaturatedUnitWeight = Convert.ToDouble(soilRow["UnderWaterWeight"]) + WaterWeight;
                aMaterial.UnitWeight = Convert.ToDouble(soilRow["UnderWaterWeight"]) + WaterWeight;
                aMaterial.ElasticProperty.E = 10;
                aMaterial.ElasticProperty.Mu = Convert.ToDouble(soilRow["Mu"]);
                aMaterial.ElasticProperty.IsAutoK0 = true;
                materialDictionary.Add(aMaterial.ID, aMaterial);

                // 3.3 弹性材料（桩靴以下）
                aMaterial = new EsMaterial
                {
                    ID = Convert.ToInt32(soilRow["ID"]) + 300,
                    MaterialType = EsMaterialType.Soil,
                    ElasticPlasticType = EsMaterialElasticPlasticType.LinearElastic,
                    Name = soilRow["Name"] + "_弹性(桩靴以下)"
                };
                aMaterial.SoilProperty.SaturatedUnitWeight = Convert.ToDouble(soilRow["UnderWaterWeight"]) + WaterWeight;
                aMaterial.UnitWeight = Convert.ToDouble(soilRow["UnderWaterWeight"]) + WaterWeight;
                aMaterial.ElasticProperty.E = Convert.ToDouble(soilRow["E"]);
                aMaterial.ElasticProperty.Mu = Convert.ToDouble(soilRow["Mu"]);
                aMaterial.ElasticProperty.IsAutoK0 = true;
                materialDictionary.Add(aMaterial.ID, aMaterial);

                // 3.4 折减塑性材料
                double onLegStrenthengReduceCoeff = Convert.ToDouble(soilRow["OnLegStrenthengReduceCoeff"]);
                double onLegWeightReduceCoeff = Convert.ToDouble(soilRow["OnLegWeightReduceCoeff"]);
                double onLegEReduceCoeff = Convert.ToDouble(soilRow["OnLegEReduceCoeff"]);
                double onLegMuReduceCoeff = Convert.ToDouble(soilRow["OnLegMuReduceCoeff"]);

                double frictionAngle = Math.Atan(
                    Math.Tan(underWaterPhi / 180.0 * Math.PI) * onLegStrenthengReduceCoeff
                ) * 180.0 / Math.PI;

                aMaterial = new EsMaterial
                {
                    ID = Convert.ToInt32(soilRow["ID"]) + 400,
                    MaterialType = EsMaterialType.Soil,
                    ElasticPlasticType = EsMaterialElasticPlasticType.Plastic,
                    Name = soilRow["Name"] + "_折减"
                };
                aMaterial.PlasticProperty.Type = 3;
                aMaterial.PlasticProperty.DruckerPragerProperty.ModeType = CalculateParameter.DPType;
                aMaterial.PlasticProperty.DruckerPragerProperty.FrictionAngle = Math.Round(frictionAngle, 2);
                aMaterial.PlasticProperty.DruckerPragerProperty.FlowAngle =
                    aMaterial.PlasticProperty.DruckerPragerProperty.FrictionAngle;
                aMaterial.PlasticProperty.DruckerPragerProperty.Cohesion =
                    Convert.ToDouble(soilRow["Su0"]) * onLegStrenthengReduceCoeff;
                aMaterial.PlasticProperty.DruckerPragerProperty.DCohesion =
                    Convert.ToDouble(soilRow["DSu"]) * onLegStrenthengReduceCoeff;
                aMaterial.PlasticProperty.DruckerPragerProperty.Depth0 = topLevel;
                aMaterial.PlasticProperty.DruckerPragerProperty.LimitStresst =
                    aMaterial.PlasticProperty.DruckerPragerProperty.Cohesion;
                aMaterial.SoilProperty.Cohesion = aMaterial.PlasticProperty.DruckerPragerProperty.Cohesion;
                aMaterial.SoilProperty.SaturatedUnitWeight =
                    Convert.ToDouble(soilRow["UnderWaterWeight"]) * onLegWeightReduceCoeff + WaterWeight;
                aMaterial.UnitWeight = aMaterial.SoilProperty.SaturatedUnitWeight;
                aMaterial.ElasticProperty.Type = 1;
                aMaterial.ElasticProperty.E = Convert.ToDouble(soilRow["E"]) * onLegEReduceCoeff;
                aMaterial.ElasticProperty.Mu = Convert.ToDouble(soilRow["mu"]) * onLegMuReduceCoeff;
                aMaterial.ElasticProperty.IsAutoK0 = true;
                materialDictionary.Add(aMaterial.ID, aMaterial);

                // 3.5 折减弹性材料（桩靴以上）
                aMaterial = new EsMaterial
                {
                    ID = Convert.ToInt32(soilRow["ID"]) + 500,
                    MaterialType = EsMaterialType.Soil,
                    ElasticPlasticType = EsMaterialElasticPlasticType.LinearElastic,
                    Name = soilRow["Name"] + "_折减弹性(桩靴以上)"
                };
                aMaterial.SoilProperty.SaturatedUnitWeight =
                    Convert.ToDouble(soilRow["UnderWaterWeight"]) * onLegWeightReduceCoeff + WaterWeight;
                aMaterial.UnitWeight = aMaterial.SoilProperty.SaturatedUnitWeight;
                aMaterial.ElasticProperty.Type = 1;
                aMaterial.ElasticProperty.E = 10;
                aMaterial.ElasticProperty.Mu = Convert.ToDouble(soilRow["mu"]) * onLegMuReduceCoeff * 0.01;
                aMaterial.ElasticProperty.IsAutoK0 = true;
                materialDictionary.Add(aMaterial.ID, aMaterial);

                // 3.6 折减弹性材料（桩靴以下）
                aMaterial = new EsMaterial
                {
                    ID = Convert.ToInt32(soilRow["ID"]) + 600,
                    MaterialType = EsMaterialType.Soil,
                    ElasticPlasticType = EsMaterialElasticPlasticType.LinearElastic,
                    Name = soilRow["Name"] + "_折减弹性(桩靴以下)"
                };
                aMaterial.SoilProperty.SaturatedUnitWeight =
                    Convert.ToDouble(soilRow["UnderWaterWeight"]) * onLegWeightReduceCoeff + WaterWeight;
                aMaterial.UnitWeight = aMaterial.SoilProperty.SaturatedUnitWeight;
                aMaterial.ElasticProperty.Type = 1;
                aMaterial.ElasticProperty.E = Convert.ToDouble(soilRow["E"]) * onLegEReduceCoeff;
                aMaterial.ElasticProperty.Mu = Convert.ToDouble(soilRow["mu"]) * onLegMuReduceCoeff * 0.01;
                aMaterial.ElasticProperty.IsAutoK0 = true;
                materialDictionary.Add(aMaterial.ID, aMaterial);

                // 3.7 置换桩区域材料
                aMaterial = new EsMaterial
                {
                    ID = Convert.ToInt32(soilRow["ID"]) + 700,
                    MaterialType = EsMaterialType.Soil,
                    ElasticPlasticType = EsMaterialElasticPlasticType.LinearElastic,
                    Name = soilRow["Name"] + "_置换桩区域"
                };
                aMaterial.SoilProperty.SaturatedUnitWeight = Convert.ToDouble(soilRow["UnderWaterWeight"]) + WaterWeight;
                aMaterial.UnitWeight = Convert.ToDouble(soilRow["UnderWaterWeight"]) + WaterWeight;
                aMaterial.ElasticProperty.Type = 1;
                aMaterial.ElasticProperty.E = Convert.ToDouble(soilRow["E"]);
                aMaterial.ElasticProperty.Mu = Convert.ToDouble(soilRow["Mu"]);
                aMaterial.ElasticProperty.IsAutoK0 = true;
                materialDictionary.Add(aMaterial.ID, aMaterial);

                nMaterial++;
                sumE += Convert.ToDouble(soilRow["E"]);
                sumMu += Convert.ToDouble(soilRow["mu"]);
            }

            // 无限单元采用平均土的参数
            if (nMaterial > 0)
            {
                infiniteMaterial.ElasticProperty.E = sumE / nMaterial * 10000;
                infiniteMaterial.ElasticProperty.Mu = sumMu / nMaterial;
            }

            return materialDictionary;
        }

        //计算建议插深及对应的抗压/抗拉承载力
        public void CalculateDepthResult(
        bool IsUserAdd,
        double PressLimitValue,
        ref string ErrorString,
        double PullLimitValue = 0)
        {
            // 抗压承载力Qv大于单桩抗压力时的持力土的深度即建议插深，同时获得建议插深处的抗拉承载力Qu
            // 获得Qv和Qu的插值结果

            if (MyDataSet.Tables["LS_CalculationLevels"].Rows.Count == 0)
            {
                ErrorString = "构件未计算！";
                return;
            }

            if (!IsUserAdd)
            {
                MyDataSet.Tables["LS_DepthResult"].Clear();
            }
            else
            {
                DataRow[] rowsToDelete = MyDataSet.Tables["LS_DepthResult"].Select("IsUserAdd=True");
                foreach (DataRow row in rowsToDelete)
                {
                    MyDataSet.Tables["LS_DepthResult"].Rows.Remove(row);
                }
            }

            var errorStrings = new List<string>();
            bool selectSingleDrilling = Convert.ToBoolean(
                MyDataSet.Tables["LS_Common"].Rows[0]["UseSingleDrilling"]
            );

            Dictionary<int, Dictionary<double, int>> levelIDByDrillingDic = GetLevelIDByDrillingDic();

            foreach (int drillingID in levelIDByDrillingDic.Keys)
            {
                // 获得建议插深（抗压）
                GetDepthValue(IsUserAdd, drillingID, PressLimitValue, true, ref ErrorString);
                if (!errorStrings.Contains(ErrorString))
                {
                    errorStrings.Add(ErrorString);
                }

                if (IsUserAdd)
                {
                    // 获得建议插深（抗拉）
                    GetDepthValue(IsUserAdd, drillingID, PullLimitValue, false, ref ErrorString);
                    if (!errorStrings.Contains(ErrorString))
                    {
                        errorStrings.Add(ErrorString);
                    }
                }
            }

            // 合并错误信息
            ErrorString = "";
            for (int i = 0; i < errorStrings.Count; i++)
            {
                if (!string.IsNullOrEmpty(errorStrings[i]))
                {
                    ErrorString += errorStrings[i] +
                        (i == errorStrings.Count - 1 ? "" : Environment.NewLine);
                }
            }
        }

        //获取每个钻孔对应的土层标高和SoilID字典
        public Dictionary<int, Dictionary<double, int>> GetLevelIDByDrillingDic()
        {
            var levelIDByDrillingDic = new Dictionary<int, Dictionary<double, int>>();
            bool selectSingleDrilling = Convert.ToBoolean(
                MyDataSet.Tables["LS_Common"].Rows[0]["UseSingleDrilling"]
            );

            if (selectSingleDrilling)
            {
                // 单钻孔模式：从 LS_LegSoilLayer 读取
                DataRow[] rows = MyDataSet.Tables["LS_LegSoilLayer"].Select("", "TopLevel DESC");

                foreach (DataRow aRow in rows)
                {
                    int drillingID = Convert.ToInt32(aRow["DrillingID"]);
                    double topLevel = Convert.ToDouble(aRow["TopLevel"]);
                    int soilID = Convert.ToInt32(aRow["SoilID"]);

                    if (!levelIDByDrillingDic.ContainsKey(drillingID))
                    {
                        levelIDByDrillingDic.Add(drillingID, new Dictionary<double, int>());
                    }
                    levelIDByDrillingDic[drillingID].Add(topLevel, soilID);
                }
            }
            else
            {
                // 多钻孔模式：从 LS_SoilDrilling 读取
                foreach (DataRow aRow in MyDataSet.Tables["LS_SoilDrilling"].Rows)
                {
                    int drillingID = Convert.ToInt32(aRow["ID"]);

                    if (!levelIDByDrillingDic.ContainsKey(drillingID))
                    {
                        levelIDByDrillingDic.Add(drillingID, new Dictionary<double, int>());
                    }

                    string soilLayers = aRow["SoilLayers"].ToString();
                    if (!string.IsNullOrEmpty(soilLayers))
                    {
                        string[] layers = soilLayers.Split(';');
                        foreach (string layer in layers)
                        {
                            if (string.IsNullOrEmpty(layer)) continue;

                            string[] parts = layer.Split(',');
                            if (parts.Length < 2) continue;

                            string soilName = parts[0].Trim();
                            double topLevel = Convert.ToDouble(parts[1]);

                            DataRow[] soilRows = MyDataSet.Tables["LS_Soil"].Select($"Name='{soilName}'");
                            if (soilRows.Length > 0)
                            {
                                int soilID = Convert.ToInt32(soilRows[0]["ID"]);
                                levelIDByDrillingDic[drillingID].Add(topLevel, soilID);
                            }
                        }
                    }
                }
            }

            return levelIDByDrillingDic;
        }

        //获取指定极限值对应的深度值
        public void GetDepthValue(
        bool IsUserAdd,
        int DrillingID,
        double LimitValue,
        bool IsPressValue,
        ref string ErrorString)
        {
            // 获得Qv和Qu的插值结果
            DataTable resultTab;
            DataTable anotherResultTab;
            string selectParam;
            string anotherSelectParam;

            if (IsPressValue)
            {
                resultTab = MyDataSet.Tables["LS_PressResistanceResult"];
                selectParam = "Qv";
                anotherResultTab = MyDataSet.Tables["LS_PullResistanceResult"];
                anotherSelectParam = "Qu";
            }
            else
            {
                resultTab = MyDataSet.Tables["LS_PullResistanceResult"];
                selectParam = "Qu";
                anotherResultTab = MyDataSet.Tables["LS_PressResistanceResult"];
                anotherSelectParam = "Qv";
            }

            double topLevel = Convert.ToDouble(
                MyDataSet.Tables["LS_CalculationLevels"].Compute("Max(Level)", $"DrillingID={DrillingID}")
            );

            ErrorString = "不在范围内！";

            string filter = $"DrillingID={DrillingID} and {selectParam}<>'-'";
            DataRow[] rows = resultTab.Select(filter, "Level DESC");

            if (rows.Length > 0)
            {
                foreach (DataRow theRow in rows)
                {
                    double value = Convert.ToDouble(theRow[selectParam]);

                    if (value > LimitValue)
                    {
                        double level = Convert.ToDouble(theRow["Level"]);

                        DataRow newRow = MyDataSet.Tables["LS_DepthResult"].NewRow();
                        newRow["LimitForce"] = double.Parse(LimitValue.ToString("N2"));
                        newRow["IsUserAdd"] = IsUserAdd;
                        newRow["DrillingID"] = DrillingID;
                        newRow[selectParam] = theRow[selectParam];

                        if (selectParam == "Qu")
                        {
                            newRow["Qu0"] = theRow["Qu0"];
                            newRow["Qu1"] = theRow["Qu1"];
                        }

                        newRow["SuggestedDepth"] = double.Parse(level.ToString("N2"));
                        newRow["SupportSoilID"] = theRow["SoilID"];

                        DataRow[] soilRows = MyDataSet.Tables["LS_Soil"].Select($"ID={theRow["SoilID"]}");
                        if (soilRows.Length > 0)
                        {
                            DataRow soilRow = soilRows[0];
                            if (Convert.ToInt32(soilRow["Type"]) == 1) // 砂土
                            {
                                newRow["SupportSoilStrength"] = Convert.ToDouble(soilRow["UnderWaterPhi"]);
                            }
                            else // 黏土
                            {
                                newRow["SupportSoilStrength"] = Convert.ToDouble(soilRow["Su0"]);
                            }
                        }

                        // 获取对应的另一个结果
                        string anotherFilter = $"DrillingID={DrillingID} and Level={level}";
                        DataRow[] anotherRows = anotherResultTab.Select(anotherFilter, "Level DESC");
                        if (anotherRows.Length > 0)
                        {
                            DataRow anotherRow = anotherRows[0];
                            newRow[anotherSelectParam] = anotherRow[anotherSelectParam].ToString() == "-" ? "-" : anotherRow[anotherSelectParam];

                            if (anotherSelectParam == "Qu")
                            {
                                newRow["Qu0"] = anotherRow["Qu0"];
                                newRow["Qu1"] = anotherRow["Qu1"];
                            }
                        }

                        MyDataSet.Tables["LS_DepthResult"].Rows.Add(newRow);
                        ErrorString = "";
                        return;
                    }
                }
            }
        }

        //准备有限元计算数据（材料、接触、几何、荷载）
        public void GetFiniteElementPrepare(
        int LevelID,
        CalculateParameter CalculateParameter,
        double ComputeLevel,
        double TopLevel,
        int LegTopEdgeID,
        double LegTopEdgeLength,
        double PressQ,
        double PullQ,
        Dictionary<int, EsMaterial> MaterialDictionaty,
        Dictionary<int, EsContact> ContactDictionaty)
        {
            // 有限元计算准备
            var selectDataValue = new EsSelectDataValue
            {
                DefaultValue = true,
                ModelGeometry = false,
                StructureGeometry = true,
                StructureStandarResult = true,
                StructureCombinationResult = true,
                StructureSectionValue = true
            };
            StructureKit.StructureData.ClearData(selectDataValue);

            // 写入材料信息
            MyDataSet.Tables["Material"].Clear();
            foreach (var material in MaterialDictionaty.Values)
            {
                StructureKit.StructureData.AddMaterial(material);

                var faceSection = new EsFaceSection
                {
                    ID = material.ID,
                    MaterialID = material.ID,
                    h1 = 1,
                    PlanStrainStressType = EsElementPlanStrainStressType.AxisSymetric,
                    Name = material.Name
                };
                StructureKit.StructureData.AddFaceSection(faceSection);
            }

            // 写入接触
            foreach (var contact in ContactDictionaty.Values)
            {
                StructureKit.StructureData.AddContact(contact);
            }

            // 写入水平支撑
            var support = new EsSupport
            {
                Name = "左侧水平支撑",
                ID = 100,
                Ux = true
            };
            StructureKit.StructureData.AddSupport(support);

            // 写入边缘释放
            var edgeRelease = new EsEdgeRelease
            {
                Name = "右侧边自由度释放",
                ID = 100,
                Lx = true
            };
            StructureKit.StructureData.AddEdgeRelease(edgeRelease);

            // 写入耦合（右侧边X耦合）
            var couple = new EsCouple
            {
                Name = "右侧边X耦合",
                ID = 100,
                Ux = true,
                Uy = false,
                Uz = false,
                Rotx = false,
                Roty = false,
                Rotz = false
            };
            StructureKit.StructureData.AddCouple(couple);

            // 写入耦合（右侧边XY耦合）
            couple = new EsCouple
            {
                Name = "右侧边XY耦合",
                ID = 101,
                Ux = true,
                Uy = true,
                Uz = false,
                Rotx = false,
                Roty = false,
                Rotz = false
            };
            StructureKit.StructureData.AddCouple(couple);

            MyDataSet.Tables["PhaseSolveChangeToProperty"].Clear();

            // 分阶段属性 - 耦合
            AddPhaseProperty(2, 2, 100, 101);
            AddPhaseProperty(3, 2, 100, 100);
            AddPhaseProperty(4, 2, 100, 101);

            // 写入几何信息
            var plates = new List<EsPlate>();
            string filter = $"LevelID={LevelID}";

            foreach (DataRow row in MyDataSet.Tables["LS_CalculationAreas"].Select(filter))
            {
                var plate = new EsPlate
                {
                    ID = Convert.ToInt32(row["AreaID"]),
                    Tag = Convert.ToInt32(row["AreaID"]),
                    ElementType = EsElementComputeType.Stress2DType
                };

                int beforeMaterialID = row["BeforeMaterialID"] != DBNull.Value ? Convert.ToInt32(row["BeforeMaterialID"]) : 0;
                int materialID = Convert.ToInt32(row["MaterialID"]);

                if (beforeMaterialID != 0)
                {
                    // 初始为土材料，抗力计算为桩靴
                    plate.SectionID = (beforeMaterialID < 400) ? beforeMaterialID + 600 : beforeMaterialID + 300;

                    AddPhaseProperty(2, 9, plate.SectionID, 1000);
                    AddPhaseProperty(3, 9, plate.SectionID, plate.SectionID);
                    AddPhaseProperty(4, 9, plate.SectionID, 1000);
                }
                else
                {
                    if (materialID == 1000)
                    {
                        plate.SectionID = 1000;
                    }
                    else
                    {
                        int location = Convert.ToInt32(row["Location"]);
                        plate.SectionID = (location > 0) ? materialID + 100 : materialID;

                        if (location > 0)
                        {
                            AddPhaseProperty(3, 9, plate.SectionID, plate.SectionID - 100);
                        }
                        else
                        {
                            AddPhaseProperty(3, 9, plate.SectionID, plate.SectionID + 200);
                        }
                    }
                }

                // 构建边界
                var bound = new EsPlaneBound();
                string edgesString = row["Edges"].ToString();
                if (!string.IsNullOrEmpty(edgesString))
                {
                    string[] edgeIDAndOrientations = edgesString.Split(';');
                    foreach (string edgeIDAndOrientation in edgeIDAndOrientations)
                    {
                        string[] parts = edgeIDAndOrientation.Split(',');
                        if (parts.Length < 2) continue;

                        int edgeID = int.Parse(parts[0]);
                        int orientation = int.Parse(parts[1]);

                        string edgeFilter = $"LevelID={LevelID} and EdgeID={edgeID}";
                        foreach (DataRow edgeRow in MyDataSet.Tables["LS_CalculationEdges"].Select(edgeFilter))
                        {
                            var lineEdge = new EsLineEdge
                            {
                                ID = Convert.ToInt32(edgeRow["EdgeID"]),
                                Orientation = orientation,
                                SupportID = edgeRow["SupportID"] != DBNull.Value ? Convert.ToInt32(edgeRow["SupportID"]) : 0,
                                ReleaseID = edgeRow["ReleaseID"] != DBNull.Value ? Convert.ToInt32(edgeRow["ReleaseID"]) : 0
                            };
                            lineEdge.Edge.x1 = Convert.ToDouble(edgeRow["x1"]);
                            lineEdge.Edge.x2 = Convert.ToDouble(edgeRow["x2"]);
                            lineEdge.Edge.y1 = Convert.ToDouble(edgeRow["y1"]);
                            lineEdge.Edge.y2 = Convert.ToDouble(edgeRow["y2"]);
                            lineEdge.Edge.z1 = 0;
                            lineEdge.Edge.z2 = 0;
                            bound.Edges.Add(lineEdge);
                        }
                    }
                }
                plate.Bounds.Add(bound);
                plates.Add(plate);
            }

            // 几何形状
            var shapeList = EsPlate.GetPlaneBrepData(plates);

            // 写入节点信息
            foreach (DataRow row in MyDataSet.Tables["LS_MeshNodes"].Select(filter))
            {
                var node = new EsNode
                {
                    ID = Convert.ToInt32(row["NodeID"]),
                    Tag = Convert.ToInt32(row["NodeID"]),
                    IsMeshNode = true,
                    x = Convert.ToDouble(row["x"]),
                    y = Convert.ToDouble(row["y"])
                };
                if (Math.Abs(node.x) < 0.001)
                {
                    node.SupportID = 100;
                }
                shapeList.Nodes.Add(node);
            }

            // 写入边界单元
            foreach (DataRow row in MyDataSet.Tables["LS_CalculationEdges"].Select(filter))
            {
                var edgeMesh = new EsEdgeMesh
                {
                    ID = Convert.ToInt32(row["EdgeID"]),
                    SupportID = row["SupportID"] != DBNull.Value ? Convert.ToInt32(row["SupportID"]) : 0
                };

                double x1 = Convert.ToDouble(row["x1"]);
                double x2 = Convert.ToDouble(row["x2"]);
                double y1 = Convert.ToDouble(row["y1"]);
                double y2 = Convert.ToDouble(row["y2"]);

                var vx = new EsVector(x2 - x1, y2 - y1, 0);
                var vy = new EsVector(y1 - y2, x2 - x1, 0);
                var vz = new EsVector(0, 0, 1);
                vx.NormalVector();
                vy.NormalVector();

                LoadMesh(edgeMesh, row);

                foreach (var node in edgeMesh.Nodes)
                {
                    edgeMesh.NodeVectorXs.Add(vx);
                    edgeMesh.NodeVectorYs.Add(vy);
                    edgeMesh.NodeVectorZs.Add(vz);
                }
                shapeList.EdgeMeshs.Add(edgeMesh);
            }

            // 写入耦合节点
            foreach (DataRow row in MyDataSet.Tables["LS_CoupleNodes"].Select(filter))
            {
                var coupleNode = new EsCoupleNode
                {
                    ID = Convert.ToInt32(row["CoupleNodeID"]),
                    Tag = Convert.ToInt32(row["CoupleNodeID"]),
                    N1 = Convert.ToInt32(row["N1"]),
                    CoupleID = Convert.ToInt32(row["CoupleID"])
                };
                coupleNode.N2.Add(Convert.ToInt32(row["N2"]));
                shapeList.CoupleNodes.Add(coupleNode);
            }

            // 写入面单元
            for (int j = 0; j < plates.Count; j++)
            {
                var faceMesh = new EsFaceMesh { ID = j + 1 };
                string areaFilter = $"LevelID={LevelID} and AreaID={j + 1}";

                foreach (DataRow row in MyDataSet.Tables["LS_AreaMeshs"].Select(areaFilter))
                {
                    var mesh = new EsMesh
                    {
                        Type = EsMeshType.MeshTypeTri3,
                        ID = Convert.ToInt32(row["MeshID"])
                    };
                    mesh.NodeIDs.Add(Convert.ToInt32(row["N1"]));
                    mesh.NodeIDs.Add(Convert.ToInt32(row["N2"]));
                    mesh.NodeIDs.Add(Convert.ToInt32(row["N3"]));
                    faceMesh.Meshs.Add(mesh);
                }

                if (faceMesh.Meshs.Count > 0)
                {
                    faceMesh.Meshs[0].LocalCoordinate = new EsAxis();
                }
                shapeList.FaceMeshs.Add(faceMesh);
            }

            // 无限单元
            var infinite2DMesh = new EsInfinite2DMesh
            {
                ID = 1,
                ElementType = EsElementComputeType.Stress2DType,
                MaterialID = 1001,
                WithPhase = true,
                CreatePhase = 1,
                DemolishPhase = 100000
            };
            infinite2DMesh.LocalCoordinate.SetString("0,0,0;1,0,0;0,1,0;0,0,1");

            foreach (DataRow row in MyDataSet.Tables["LS_InfiniteMeshs"].Select(filter))
            {
                var mesh = new EsMesh
                {
                    Type = EsMeshType.MeshTypeInfiniteQuad6,
                    ID = Convert.ToInt32(row["MeshID"])
                };
                mesh.NodeIDs.Add(Convert.ToInt32(row["N1"]));
                mesh.NodeIDs.Add(Convert.ToInt32(row["N2"]));
                mesh.NodeIDs.Add(Convert.ToInt32(row["N3"]));
                mesh.NodeIDs.Add(Convert.ToInt32(row["N4"]));
                mesh.NodeIDs.Add(Convert.ToInt32(row["N5"]));
                mesh.NodeIDs.Add(Convert.ToInt32(row["N6"]));
                infinite2DMesh.Meshs.Add(mesh);
            }
            shapeList.Infinite2DMeshs.Add(infinite2DMesh);

            // 分阶段属性 - 接触
            AddPhaseProperty(2, 10, 102, 105); // 桩靴上部接触
            AddPhaseProperty(2, 10, 101, 104); // 侧面接触
            AddPhaseProperty(3, 10, 102, 102); // 桩靴上部接触
            AddPhaseProperty(3, 10, 101, 101); // 侧面接触
            AddPhaseProperty(4, 10, 100, 103); // 桩靴底部接触
            AddPhaseProperty(4, 10, 102, 102); // 桩靴上部接触

            // 写入接触器
            int contactorMeshID = 1;
            foreach (DataRow row in MyDataSet.Tables["LS_Contactors"].Select(filter))
            {
                var objectContactor = new EsObjectContactor
                {
                    IsAutoContact = false,
                    ContactorType = EsObjectContactorType.EdgeToEdge,
                    ContactID = Convert.ToInt32(row["ContactorID"]),
                    ID = Convert.ToInt32(row["ID"])
                };
                objectContactor.ObjectList1.AddRange(EsCommonFunction.GetIDList(row["Edge1s"].ToString()));
                objectContactor.ObjectList2.AddRange(EsCommonFunction.GetIDList(row["Edge2s"].ToString()));
                shapeList.ObjectContactors.Add(objectContactor);

                var contactorMesh = new EsObjectContactorMesh
                {
                    ID = Convert.ToInt32(row["ID"]),
                    ContactID = Convert.ToInt32(row["ContactorID"]),
                    ElementType = EsElementComputeType.ContactorStress1DType,
                    ContactorType = EsObjectContactorType.EdgeToEdge
                };
                shapeList.ObjectContactorMeshs.Add(contactorMesh);

                var nodes1 = EsCommonFunction.GetIDList(row["Nodes1"].ToString());
                var nodes2 = EsCommonFunction.GetIDList(row["Nodes2"].ToString());
                var contactorLocalCoordinate = new EsAxis(row["LocalCoordinate"].ToString());

                for (int i = 0; i < nodes1.Count - 1; i++)
                {
                    var mesh = new EsMesh
                    {
                        ID = contactorMeshID,
                        Type = EsMeshType.MeshTypeContactorLine2,
                        LocalCoordinate = contactorLocalCoordinate
                    };
                    mesh.NodeIDs.Add(nodes1[i]);
                    mesh.NodeIDs.Add(nodes1[i + 1]);
                    mesh.NodeIDs.Add(nodes2[i]);
                    mesh.NodeIDs.Add(nodes2[i + 1]);
                    contactorMesh.Meshs.Add(mesh);
                    contactorMeshID++;
                }
            }

            // 输入几何形状等数据
            StructureKit.StructureData.AddShapeList(shapeList, "");

            // 写入计算荷载
            DataRow analyseRow = MyDataSet.Tables["AnalyseSet"].Rows[0];
            analyseRow["WeightDirection"] = 0;

            // 更新非线性分析设置
            foreach (DataRow loadNonlinearRow in MyDataSet.Tables["LoadNonlinearAnalyseSet"].Rows)
            {
                int loadID = Convert.ToInt32(loadNonlinearRow["LoadID"]);
                foreach (DataRow loadRow in MyDataSet.Tables["Load"].Select($"ID={loadID}"))
                {
                    loadRow["IsNonlinear"] = true;
                    loadRow["IsNonlinearGeometryObject"] = loadNonlinearRow["IsNonlinearGeometryObject"];
                    loadRow["NTime"] = loadNonlinearRow["NTime"];
                    loadRow["NNonlinearIteration"] = loadNonlinearRow["NNonlinearIteration"];
                    loadRow["IsNonlinearReComputeMatrixInIteration"] = loadNonlinearRow["IsNonlinearReComputeMatrixInIteration"];
                    loadRow["NNonlinearAdjust"] = loadNonlinearRow["NNonlinearAdjust"];
                    loadRow["NonlinearConvergentCriterion"] = loadNonlinearRow["NonlinearConvergentCriterion"];
                    loadRow["UpdateMethod"] = loadNonlinearRow["UpdateMethod"];
                    loadRow["AnalyseMethod"] = loadNonlinearRow["AnalyseMethod"];
                    loadRow["SelectedNodeID"] = loadNonlinearRow["SelectedNodeID"];
                    loadRow["SelectedNodeValueIndex"] = loadNonlinearRow["SelectedNodeValueIndex"];
                    loadRow["SelectedNodeMaxValue"] = loadNonlinearRow["SelectedNodeMaxValue"];
                    loadRow["AnalyseEndType"] = loadNonlinearRow["AnalyseEndType"];
                    loadRow["FromTime"] = loadNonlinearRow["FromTime"];
                    loadRow["ToTime"] = loadNonlinearRow["ToTime"];
                    loadRow["KeepStructureEnergyResult"] = loadNonlinearRow["KeepStructureEnergyResult"];
                }
            }

            // 写入荷载
            MyDataSet.Tables["Load"].Clear();

            DataRow loadRow1 = MyDataSet.Tables["Load"].NewRow();
            loadRow1["ID"] = 1;
            loadRow1["Name"] = "极限压力初始应力场";
            loadRow1["LoadType"] = 104;
            loadRow1["phase"] = 1;
            loadRow1["IsNonlinear"] = true;
            loadRow1["NNonlinearIteration"] = 20;
            loadRow1["NonlinearConvergentCriterion"] = CalculateParameter.DCoeff;
            MyDataSet.Tables["Load"].Rows.Add(loadRow1);

            DataRow loadRow2 = MyDataSet.Tables["Load"].NewRow();
            loadRow2["ID"] = 2;
            loadRow2["Name"] = "极限压力";
            loadRow2["LoadType"] = 501;
            loadRow2["phase"] = 2;
            loadRow2["NonlinearConvergentCriterion"] = CalculateParameter.DCoeff;
            MyDataSet.Tables["Load"].Rows.Add(loadRow2);

            if (LevelID > 1)
            {
                DataRow loadRow3 = MyDataSet.Tables["Load"].NewRow();
                loadRow3["ID"] = 3;
                loadRow3["Name"] = "极限拉力初始应力场";
                loadRow3["LoadType"] = 104;
                loadRow3["phase"] = 3;
                loadRow3["IsNonlinear"] = true;
                loadRow3["NNonlinearIteration"] = 20;
                loadRow3["NonlinearConvergentCriterion"] = CalculateParameter.DCoeff;
                MyDataSet.Tables["Load"].Rows.Add(loadRow3);

                DataRow loadRow4 = MyDataSet.Tables["Load"].NewRow();
                loadRow4["ID"] = 4;
                loadRow4["Name"] = "极限拔力";
                loadRow4["LoadType"] = 501;
                loadRow4["phase"] = 4;
                loadRow4["NonlinearConvergentCriterion"] = CalculateParameter.DCoeff;
                MyDataSet.Tables["Load"].Rows.Add(loadRow4);
            }

            // 写入水位
            MyDataSet.Tables["WaterLevelSet"].Clear();
            DataRow waterRow = MyDataSet.Tables["WaterLevelSet"].NewRow();
            waterRow["ID"] = 1;
            waterRow["Name"] = "设计水位";
            waterRow["WaterLevel"] = TopLevel + 50;
            waterRow["WaterLevelType"] = 1;
            MyDataSet.Tables["WaterLevelSet"].Rows.Add(waterRow);

            // 写入初始应力
            MyDataSet.Tables["InitialSoilStress"].Clear();
            DataRow stressRow1 = MyDataSet.Tables["InitialSoilStress"].NewRow();
            stressRow1["LoadID"] = 1;
            stressRow1["WaterLevelID"] = 1;
            MyDataSet.Tables["InitialSoilStress"].Rows.Add(stressRow1);

            DataRow stressRow2 = MyDataSet.Tables["InitialSoilStress"].NewRow();
            stressRow2["LoadID"] = 3;
            stressRow2["WaterLevelID"] = 1;
            MyDataSet.Tables["InitialSoilStress"].Rows.Add(stressRow2);

            // 写入塑性稳定分析设置
            MyDataSet.Tables["PlasticStableAnalyseSet"].Clear();

            DataRow plasticRow1 = MyDataSet.Tables["PlasticStableAnalyseSet"].NewRow();
            plasticRow1["LoadID"] = 2;
            plasticRow1["InitialStressID"] = 1;
            plasticRow1["MinDTime"] = 0.01;
            plasticRow1["MaxTime"] = 1000;
            plasticRow1["NNonlinearIteration"] = 20;
            plasticRow1["NonlinearConvergentCriterion"] = CalculateParameter.DCoeff;
            MyDataSet.Tables["PlasticStableAnalyseSet"].Rows.Add(plasticRow1);

            DataRow plasticRow2 = MyDataSet.Tables["PlasticStableAnalyseSet"].NewRow();
            plasticRow2["LoadID"] = 4;
            plasticRow2["InitialStressID"] = 3;
            plasticRow2["MinDTime"] = 0.01;
            plasticRow2["MaxTime"] = 1000;
            plasticRow2["NNonlinearIteration"] = 20;
            plasticRow2["NonlinearConvergentCriterion"] = CalculateParameter.DCoeff;
            MyDataSet.Tables["PlasticStableAnalyseSet"].Rows.Add(plasticRow2);

            // 写入边缘荷载
            var pressEdgeLoad = new EsLoadUniformOnEdge
            {
                ID = LegTopEdgeID,
                IsRelative = true,
                x1 = 0,
                x2 = 1,
                Qy1 = -PressQ,
                Qy2 = -PressQ,
                AddLoadType = EsAddLoadType.Linear
            };

            var pullEdgeLoad = new EsLoadUniformOnEdge
            {
                ID = LegTopEdgeID,
                IsRelative = true,
                x1 = 0,
                x2 = 1,
                Qy1 = PullQ,
                Qy2 = PullQ,
                AddLoadType = EsAddLoadType.Linear
            };

            var pressLoad = new EsStructureLoad();
            pressLoad.EdgeLoads.UniformLoads.Add(pressEdgeLoad);
            pressLoad.LoadID = 2;
            StructureKit.StructureData.AddLoad(pressLoad, 1);

            var pullLoad = new EsStructureLoad();
            pullLoad.EdgeLoads.UniformLoads.Add(pullEdgeLoad);
            pullLoad.LoadID = 4;
            StructureKit.StructureData.AddLoad(pullLoad, 1);

            // 写入自重荷载
            foreach (DataRow row in MyDataSet.Tables["LS_CalculationAreas"].Select(filter))
            {
                int areaID = Convert.ToInt32(row["AreaID"]);

                DataRow selfWeightRow1 = MyDataSet.Tables["LoadSelfWeightOnFace"].NewRow();
                selfWeightRow1["LoadID"] = 1;
                selfWeightRow1["FaceID"] = areaID;
                selfWeightRow1["WaterLevelID"] = 1;
                MyDataSet.Tables["LoadSelfWeightOnFace"].Rows.Add(selfWeightRow1);

                DataRow selfWeightRow2 = MyDataSet.Tables["LoadSelfWeightOnFace"].NewRow();
                selfWeightRow2["LoadID"] = 3;
                selfWeightRow2["FaceID"] = areaID;
                selfWeightRow2["WaterLevelID"] = 1;
                MyDataSet.Tables["LoadSelfWeightOnFace"].Rows.Add(selfWeightRow2);
            }
        }

        //添加分阶段属性到数据表
        private void AddPhaseProperty(int phase, int propertyType, int propertyID1, int propertyID2)
        {
            DataRow newRow = MyDataSet.Tables["PhaseSolveChangeToProperty"].NewRow();
            newRow["Phase"] = phase;
            newRow["PropertyType"] = propertyType;
            newRow["PropertyID1"] = propertyID1;
            newRow["PropertyID2"] = propertyID2;
            MyDataSet.Tables["PhaseSolveChangeToProperty"].Rows.Add(newRow);
        }

        //从数据行读取节点ID列表并加载到网格对象
        public void LoadMesh(EsMesh mesh, DataRow item)
        {
            if (item["MeshNodes"] != DBNull.Value && !string.IsNullOrEmpty(item["MeshNodes"].ToString()))
            {
                string[] nodes = item["MeshNodes"].ToString().Split(',');
                for (int i = 0; i < nodes.Length; i++)
                {
                    if (!string.IsNullOrEmpty(nodes[i]))
                    {
                        mesh.NodeIDs.Add(int.Parse(nodes[i]));
                    }
                }
            }
        }

        //提取有限元计算结果并写入数据表
        public void GetFiniteElementResult(
        int LevelID,
        SoilParameter Soil,
        double ComputeLevel,
        double SpudcanA,
        double PressForce,
        double PullForce,
        double AddtionWeight)
        {
            // 提取抗压结果
            foreach (DataRow resultRow in MyDataSet.Tables["PlasticStableResult"].Select("LoadID=2"))
            {
                DataRow row = MyDataSet.Tables["LS_PressResistanceResult"].NewRow();
                row["DrillingID"] = 1;
                row["ID"] = LevelID;
                row["Level"] = ComputeLevel;
                row["SoilID"] = Soil.SoilID;
                row["IsSand"] = Soil.SoilType == SoilType.Sand;

                double stableCoeff = Convert.ToDouble(resultRow["StableCoeff"]);
                row["QvP"] = Math.Round((stableCoeff * PressForce - AddtionWeight) / SpudcanA, 2);
                row["Qv"] = Math.Round(stableCoeff * PressForce - AddtionWeight, 2);
                MyDataSet.Tables["LS_PressResistanceResult"].Rows.Add(row);
            }

            // 提取抗拉结果
            if (LevelID > 1)
            {
                foreach (DataRow resultRow in MyDataSet.Tables["PlasticStableResult"].Select("LoadID=4"))
                {
                    DataRow row = MyDataSet.Tables["LS_PullResistanceResult"].NewRow();
                    row["DrillingID"] = 1;
                    row["ID"] = LevelID;
                    row["Level"] = ComputeLevel;
                    row["SoilID"] = Soil.SoilID;

                    double stableCoeff = Convert.ToDouble(resultRow["StableCoeff"]);
                    row["Qu"] = Math.Round(stableCoeff * PullForce + AddtionWeight, 2);
                    MyDataSet.Tables["LS_PullResistanceResult"].Rows.Add(row);
                }
            }
            else
            {
                DataRow row = MyDataSet.Tables["LS_PullResistanceResult"].NewRow();
                row["DrillingID"] = 1;
                row["ID"] = LevelID;
                row["Level"] = ComputeLevel;
                row["SoilID"] = Soil.SoilID;
                row["Qu"] = Math.Round(AddtionWeight, 2);
                MyDataSet.Tables["LS_PullResistanceResult"].Rows.Add(row);
            }

            // 提取节点位移
            foreach (DataRow resultRow in MyDataSet.Tables["ResultOfNodeDisplacement"].Rows)
            {
                DataRow row = MyDataSet.Tables["LS_ResultOfNodeDisplacement"].NewRow();
                row["LevelID"] = LevelID;
                row["LoadID"] = resultRow["LoadID"];
                row["NodeID"] = resultRow["NodeID"];
                row["Ux"] = resultRow["Ux"];
                row["Uy"] = resultRow["Uy"];
                MyDataSet.Tables["LS_ResultOfNodeDisplacement"].Rows.Add(row);
            }

            // 提取区域应力
            foreach (DataRow resultRow in MyDataSet.Tables["ResultOfFace"].Rows)
            {
                DataRow row = MyDataSet.Tables["LS_ResultOfFace"].NewRow();
                row["LevelID"] = LevelID;
                row["LoadID"] = resultRow["LoadID"];
                row["FaceID"] = resultRow["FaceID"];
                row["NodeID"] = resultRow["NodeID"];
                row["Sx"] = resultRow["Sx"];
                row["Sy"] = resultRow["Sy"];
                row["Sz"] = resultRow["Sz"];
                row["Sxy"] = resultRow["Sxy"];
                row["ex"] = resultRow["ex"];
                row["ey"] = resultRow["ey"];
                row["ez"] = resultRow["ez"];
                row["exy"] = resultRow["exy"];
                row["epx"] = resultRow["epx"];
                row["epy"] = resultRow["epy"];
                row["epz"] = resultRow["epz"];
                row["epxy"] = resultRow["epxy"];
                MyDataSet.Tables["LS_ResultOfFace"].Rows.Add(row);
            }
        }

        //综合分析抗压承载力（根据土层类型选择计算模式）
        public void AnalysisOfQv(
        int DrillingID,
        int ComputeLevelI,
        List<double> ComputeLevels,
        List<SoilParameter> Soils,
        LegParameter LegParameter,
        SpudcanParameter SpudcanParameter,
        CalculateParameter CalculateParameter)
        {
            double Qv1 = 1e10;
            double Qv1Sand = 1e10;
            double Qv1Clay = 1e10;
            double Qv2 = 1e10;
            double Qv3 = 1e10;
            double Qv3Sand = 1e10;
            double Qv3Clay = 1e10;
            double Qv4 = 1e10;
            string QvDescription = "";

            double computeLevel = ComputeLevels[ComputeLevelI];
            SoilParameter soil = GetSoil(computeLevel, Soils);
            double calSpudcanB = SpudcanParameter.GetSpudcanB();

            // 如果是砂土且需要等效圆形，重新计算桩靴宽度
            if (soil.SoilType != SoilType.Clay)
            {
                calSpudcanB = CalculateParameter.IsEquivalentToCircleSpudcan
                    ? 2 * Math.Pow(SpudcanParameter.Area / Math.PI, 0.5)
                    : SpudcanParameter.GetSpudcanB();
            }

            // 判断计算高程以下 B/2 内是否有 3 种或以上土层
            if (GetIsDownSoilTypeExtra(computeLevel, calSpudcanB, Soils))
            {
                EsMessageReporter.ReportMessageFunction(
                    $"场地内{ComputeLevels[ComputeLevelI]}m处有软、硬土层交错分布风险，请谨慎使用计算结果",
                    EsMessageType.Warning
                );
            }

            bool useNormal = false;
            double nextLevel = (ComputeLevelI == ComputeLevels.Count - 1) ? computeLevel : ComputeLevels[ComputeLevelI + 1];
            double nextSoilLevel = Math.Min(nextLevel, soil.BottomLevel);
            bool mergeSandSoil = false;
            nextSoilLevel = Math.Min(nextSoilLevel, GetDownSandLayersNextLevel(computeLevel, Soils, ref mergeSandSoil));

            if (computeLevel - nextSoilLevel > calSpudcanB)
            {
                useNormal = true;
                EsMessageReporter.ReportMessageFunction(
                    $"抗压承载力计算：持力层土厚度{Math.Round(computeLevel - nextSoilLevel, 2)}大于B({Math.Round(calSpudcanB, 2)})，按常规破坏模式计算",
                    EsMessageType.Normal
                );

                // 当多层砂土需要合并土层，且计算高程不为最底计算高程
                if (mergeSandSoil && ComputeLevelI != ComputeLevels.Count - 1)
                {
                    nextSoilLevel = computeLevel - calSpudcanB;
                }
            }
            else
            {
                MyDataSet.Tables["LS_CalculationLevels"].Compute("Min(Level)", $"DrillingID={DrillingID}");
                EsMessageReporter.ReportMessageFunction(
                    $"抗压承载力计算：持力层土厚度{Math.Round(computeLevel - nextSoilLevel, 2)}小于等于B({Math.Round(calSpudcanB, 2)})，按挤出破坏模式和穿刺破坏模式计算",
                    EsMessageType.Normal
                );
            }

            // 砂土承载力计算
            if (soil.SoilType != SoilType.Clay)
            {
                if (useNormal)
                {
                    Qv1Sand = GetQV_Sand(
                        CalculateParameter.IsEquivalentToCircleSpudcan,
                        calSpudcanB,
                        computeLevel,
                        nextSoilLevel,
                        LegParameter,
                        SpudcanParameter,
                        soil,
                        Soils,
                        CalculateParameter.IsBackFlow,
                        ref QvDescription,
                        CalculateParameter.Hc,
                        "Qv1_Sand"
                    );
                }
                else
                {
                    Qv3Sand = GetQV_Punch_Sand(
                        CalculateParameter.IsEquivalentToCircleSpudcan,
                        calSpudcanB,
                        computeLevel,
                        nextSoilLevel,
                        LegParameter,
                        SpudcanParameter,
                        soil,
                        Soils,
                        CalculateParameter.IsBackFlow,
                        ref QvDescription,
                        CalculateParameter.Hc,
                        "Qv3_Sand"
                    );
                }
            }

            // 黏土承载力计算
            if (soil.SoilType != SoilType.Sand)
            {
                if (useNormal)
                {
                    Qv1Clay = GetQV_Clay(
                        calSpudcanB,
                        computeLevel,
                        LegParameter,
                        SpudcanParameter,
                        soil,
                        Soils,
                        CalculateParameter.IsBackFlow,
                        ref QvDescription,
                        CalculateParameter.Hc,
                        "Qv1_Clay"
                    );
                }
                else
                {
                    Qv3Clay = GetQV_Punch_Clay(
                        calSpudcanB,
                        computeLevel,
                        nextSoilLevel,
                        LegParameter,
                        SpudcanParameter,
                        soil,
                        Soils,
                        CalculateParameter.IsBackFlow,
                        ref QvDescription,
                        CalculateParameter.Hc,
                        "Qv3_Clay"
                    );
                }
            }

            // 根据土层类型选择结果
            switch (soil.SoilType)
            {
                case SoilType.Both:
                    if (useNormal)
                    {
                        Qv1 = Math.Min(Qv1Sand, Qv1Clay);
                        QvDescription += "Qv3未计算;Qv3_Clay未计算;Qv3_Sand未计算;";
                    }
                    else
                    {
                        Qv3 = Math.Min(Qv3Sand, Qv3Clay);
                        QvDescription += "Qv1未计算;Qv1_Clay未计算;Qv1_Sand未计算;";
                    }
                    break;

                case SoilType.Sand:
                    if (useNormal)
                    {
                        Qv1 = Qv1Sand;
                        QvDescription += "Qv3未计算;Qv3_Sand未计算;";
                    }
                    else
                    {
                        Qv3 = Qv3Sand;
                        QvDescription += "Qv1未计算;Qv1_Sand未计算;";
                    }
                    QvDescription += "Qv1_Clay未计算;Qv3_Clay未计算;";
                    break;

                case SoilType.Clay:
                    if (useNormal)
                    {
                        Qv1 = Qv1Clay;
                        QvDescription += "Qv3未计算;Qv3_Clay未计算;";
                    }
                    else
                    {
                        Qv3 = Qv3Clay;
                        QvDescription += "Qv1未计算;Qv1_Clay未计算;";
                    }
                    QvDescription += "Qv1_Sand未计算;Qv3_Sand未计算;";
                    break;
            }

            // 挤出破坏模式
            if (useNormal)
            {
                QvDescription += "Qv2未计算;";
            }
            else
            {
                Qv2 = GetQV_Squeeze(
                    CalculateParameter.IsEquivalentToCircleSpudcan,
                    calSpudcanB,
                    computeLevel,
                    nextLevel,
                    LegParameter,
                    SpudcanParameter,
                    soil,
                    Soils,
                    CalculateParameter.IsBackFlow,
                    ref QvDescription,
                    CalculateParameter.Hc,
                    "Qv2"
                );
            }

            // 多层土破坏模式
            Qv4 = GetQV_MultiLayer(
                calSpudcanB,
                computeLevel,
                LegParameter,
                SpudcanParameter,
                soil,
                Soils,
                CalculateParameter.IsBackFlow,
                ref QvDescription,
                CalculateParameter.Hc,
                "Qv4"
            );

            // 写入结果
            AnalysisOfQv_WriteResult(
                DrillingID,
                ComputeLevelI,
                computeLevel,
                soil,
                SpudcanParameter.Area,
                ref Qv1,
                ref Qv1Sand,
                ref Qv1Clay,
                ref Qv2,
                ref Qv3,
                ref Qv3Sand,
                ref Qv3Clay,
                ref Qv4,
                ref QvDescription
            );
        }

        //将抗压承载力分析结果写入数据表
        public void AnalysisOfQv_WriteResult(
        int drillingID,
        int computeLevelIndex,
        double computeLevel,
        SoilParameter soil,
        double spudcanParameterA,
        ref double qv1,
        ref double qv1Sand,
        ref double qv1Clay,
        ref double qv2,
        ref double qv3,
        ref double qv3Sand,
        ref double qv3Clay,
        ref double qv4,
        ref string qvDescription)
        {
            const double largeValue = 1e10;

            // 清理描述字符串
            qvDescription = CleanDescription(qvDescription);

            // 创建结果行
            DataRow newRow = CreateResultRow(
                drillingID, computeLevelIndex, computeLevel, soil,
                qv1, qv1Sand, qv1Clay, qv2, qv3, qv3Sand, qv3Clay, qv4,
                qvDescription
            );

            // 计算最终 Qv 和 Qvp
            int selectMode = GetSelectMode(computeLevel, drillingID);
            SetFinalQvValues(newRow, qv1, qv2, qv3, qv4, spudcanParameterA, selectMode);

            // 替换大值
            ReplaceLargeValues(newRow, largeValue);

            // 添加到数据表
            MyDataSet.Tables["LS_PressResistanceResult"].Rows.Add(newRow);

            // 检查上层挤出破坏
            CheckPreviousSqueezeResult(computeLevelIndex, newRow);
        }

        private string CleanDescription(string description)
        {
            if (!string.IsNullOrEmpty(description) && description.EndsWith(";"))
            {
                return description.Remove(description.Length - 1, 1);
            }
            return description;
        }

        private DataRow CreateResultRow(
            int drillingID,
            int computeLevelIndex,
            double computeLevel,
            SoilParameter soil,
            double qv1, double qv1Sand, double qv1Clay,
            double qv2, double qv3, double qv3Sand, double qv3Clay,
            double qv4, string qvDescription)
        {
            DataRow row = MyDataSet.Tables["LS_PressResistanceResult"].NewRow();
            row["DrillingID"] = drillingID;
            row["ID"] = computeLevelIndex + 1;
            row["Level"] = Math.Round(computeLevel, 2);
            row["Qv1"] = Math.Round(qv1, 2);
            row["Qv1_Sand"] = Math.Round(qv1Sand, 2);
            row["Qv1_Clay"] = Math.Round(qv1Clay, 2);
            row["Qv2"] = Math.Round(qv2, 2);
            row["Qv3"] = Math.Round(qv3, 2);
            row["Qv3_Sand"] = Math.Round(qv3Sand, 2);
            row["Qv3_Clay"] = Math.Round(qv3Clay, 2);
            row["Qv4"] = Math.Round(qv4, 2);
            row["Description"] = qvDescription;
            row["SoilID"] = soil.SoilID;
            row["IsSand"] = (int)soil.SoilType;
            return row;
        }

        private int GetSelectMode(double computeLevel, int drillingID)
        {
            DataRow[] rows = MyDataSet.Tables["LS_CalculationLevels"].Select(
                $"Level={Math.Round(computeLevel, 2)} and DrillingID={drillingID}"
            );
            return rows.Length > 0 ? Convert.ToInt32(rows[0]["SelectMode_Qv"]) : 0;
        }

        private void SetFinalQvValues(
            DataRow row,
            double qv1, double qv2, double qv3, double qv4,
            double spudcanParameterA,
            int selectMode)
        {
            const double largeValue = 1e10;
            double minQv = Math.Min(qv1, Math.Min(qv2, qv3));

            switch (selectMode)
            {
                case 0:
                    row["Qvp"] = (minQv >= largeValue) ? largeValue : Math.Round(minQv / spudcanParameterA, 2);
                    row["Qv"] = Math.Round(minQv, 2);
                    row["SelectMode"] = 0;
                    break;
                case 1:
                    row["Qvp"] = (qv1 >= largeValue) ? largeValue : Math.Round(qv1 / spudcanParameterA, 2);
                    row["Qv"] = Math.Round(qv1, 2);
                    row["SelectMode"] = 1;
                    break;
                case 2:
                    row["Qvp"] = (qv4 >= largeValue) ? largeValue : Math.Round(qv4 / spudcanParameterA, 2);
                    row["Qv"] = Math.Round(qv4, 2);
                    row["SelectMode"] = 2;
                    break;
                case 3:
                    row["Qvp"] = (qv2 >= largeValue) ? largeValue : Math.Round(qv2 / spudcanParameterA, 2);
                    row["Qv"] = Math.Round(qv2, 2);
                    row["SelectMode"] = 3;
                    break;
                case 4:
                    row["Qvp"] = (qv3 >= largeValue) ? largeValue : Math.Round(qv3 / spudcanParameterA, 2);
                    row["Qv"] = Math.Round(qv3, 2);
                    row["SelectMode"] = 4;
                    break;
            }
        }

        private void ReplaceLargeValues(DataRow row, double largeValue)
        {
            string[] titles = { "QvP", "Qv", "Qv1", "Qv1_Sand", "Qv1_Clay", "Qv2", "Qv3", "Qv3_Sand", "Qv3_Clay", "Qv4" };
            foreach (string title in titles)
            {
                object value = row[title];
                if (value != DBNull.Value && Convert.ToDouble(value) >= largeValue)
                {
                    row[title] = "-";
                }
            }
        }

        private void CheckPreviousSqueezeResult(int computeLevelIndex, DataRow currentRow)
        {
            int rowCount = MyDataSet.Tables["LS_PressResistanceResult"].Rows.Count;
            if (computeLevelIndex < 1 || rowCount < 2) return;

            DataRow previousRow = MyDataSet.Tables["LS_PressResistanceResult"].Rows[rowCount - 2];
            object prevQv2 = previousRow["Qv2"];

            if (prevQv2 == DBNull.Value || prevQv2.ToString() == "-") return;

            double prevQv2Value = Convert.ToDouble(prevQv2);
            double currentQv = Convert.ToDouble(currentRow["Qv"]);

            if (prevQv2Value <= currentQv) return;

            // 更新上一行结果
            if (previousRow["Qv"].ToString() == previousRow["Qv2"].ToString())
            {
                previousRow["Qv"] = currentRow["Qv"];
                previousRow["Qvp"] = currentRow["Qvp"];
            }

            // 更新描述
            UpdatePreviousDescription(previousRow, currentRow);

            // 记录日志
            string normalString = $"{previousRow["Level"]}m挤出破坏模式：挤出破坏结果Qv=Min(挤出Qv({previousRow["Qv2"]})，持力+1层土Qv({currentQv})";
            previousRow["Qv2"] = currentRow["Qv"];
            EsMessageReporter.ReportMessageFunction(normalString, EsMessageType.Normal);
        }

        private void UpdatePreviousDescription(DataRow previousRow, DataRow currentRow)
        {
            string[] qvFields = { "Qv1_Sand", "Qv1_Clay", "Qv2", "Qv3_Sand", "Qv3_Clay", "Qv4", "Qv", "QvP" };
            for (int i = 0; i < qvFields.Length - 2; i++)
            {
                if (currentRow["Qv"].ToString() == currentRow[qvFields[i]].ToString())
                {
                    previousRow["Description"] = GetQvDescription(
                        previousRow["Description"].ToString(),
                        currentRow["Description"].ToString(),
                        qvFields[i],
                        "Qv2",
                        "Qv=Min(Qv，持力+1层土Qv)"
                    );
                    break;
                }
            }
        }

        //合并新旧承载力描述信息
        public string GetQvDescription(
        string OldDescription,
        string NewDescription,
        string QvSelectName,
        string QvShowName,
        string AddDescription)
        {
            // 删除旧对应备注
            string qvDescription = RemoveSelectQvDescription(OldDescription, QvShowName);

            // 获得旧对应备注
            string tempOldQvDescription = GetSelectQvDescription(OldDescription, QvShowName, QvShowName);

            // 获得新对应备注
            string tempNewQvDescription = GetSelectQvDescription(NewDescription, QvSelectName, QvShowName);

            // 拼接新旧备注
            return qvDescription + ";" + tempOldQvDescription + Environment.NewLine +
                   AddDescription + Environment.NewLine + tempNewQvDescription;
        }

        //从描述字符串中移除指定承载力相关的备注
        public string RemoveSelectQvDescription(string Description, string QvSelectName)
        {
            string tempQvDescription = "";
            string[] tempTip = Description.Split(new string[] { QvSelectName }, StringSplitOptions.RemoveEmptyEntries);

            for (int k = 0; k < tempTip.Length; k++)
            {
                if (k == tempTip.Length - 1)
                {
                    string[] parts = tempTip[k].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 1)
                    {
                        tempQvDescription += parts[1];
                    }
                }
                else
                {
                    if (tempTip[k].Contains(";"))
                    {
                        tempQvDescription += tempTip[k];
                    }
                }
            }

            return tempQvDescription;
        }

        //从描述字符串中提取指定承载力相关的备注
        public string GetSelectQvDescription(string Description, string QvSelectName, string QvShowName)
        {
            string tempQvDescription = "";
            string[] tempTip = Description.Split(new string[] { QvSelectName }, StringSplitOptions.RemoveEmptyEntries);

            for (int k = 0; k < tempTip.Length; k++)
            {
                if (k == tempTip.Length - 1)
                {
                    string[] parts = tempTip[k].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 0)
                    {
                        tempQvDescription += QvShowName + parts[0];
                    }
                }
                else
                {
                    if (!tempTip[k].Contains(";"))
                    {
                        if (tempTip[k].StartsWith("="))
                        {
                            tempQvDescription += QvShowName + tempTip[k];
                        }
                        else
                        {
                            tempQvDescription += tempTip[k];
                        }
                    }
                }
            }

            return tempQvDescription;
        }

        //评估穿刺风险并写入评估结果
        public void AssessmentOfPunctureRisk(
        int DrillingID,
        double PressLimitValue,
        SpudcanParameter SpudcanParameter)
        {
            const double LargeValue = 1e10;
            double spudcanB = SpudcanParameter.GetSpudcanB();
            double betaBDepth = 1 * spudcanB;

            double P1 = PressLimitValue;
            double P2 = -LargeValue;
            double P2Level = LargeValue;
            double P3 = LargeValue;
            double P3Level = LargeValue;

            string selectString = $"DrillingID={DrillingID} and Qv<>'-'";
            double theLevel = 0;

            // 查找第一个 Qv >= PressLimitValue 的层级
            DataRow[] pressRows = MyDataSet.Tables["LS_PressResistanceResult"].Select(selectString, "Level DESC");

            foreach (DataRow pRRRow in pressRows)
            {
                double qvValue = Convert.ToDouble(pRRRow["Qv"]);
                if (qvValue >= PressLimitValue)
                {
                    theLevel = Convert.ToDouble(pRRRow["Level"]);

                    // 计算 P3（指定范围内的最小值）
                    selectString = $"DrillingID={DrillingID} and Qv<>'-' and Level>={theLevel - betaBDepth} and Level<{theLevel}";
                    DataRow[] rows = MyDataSet.Tables["LS_PressResistanceResult"].Select(selectString, "Level DESC");

                    foreach (DataRow row in rows)
                    {
                        double qv = Convert.ToDouble(row["Qv"]);
                        if (qv < P3)
                        {
                            P3 = qv;
                            P3Level = Convert.ToDouble(row["Level"]);
                        }
                    }

                    // 计算 P2（指定范围内的最大值，需连续递减）
                    P2 = -LargeValue;
                    P2Level = LargeValue;
                    selectString = $"DrillingID={DrillingID} and Qv<>'-' and Level>={theLevel - betaBDepth} and Level<={theLevel}";
                    rows = MyDataSet.Tables["LS_PressResistanceResult"].Select(selectString, "Level DESC");

                    foreach (DataRow row in rows)
                    {
                        double qv = Convert.ToDouble(row["Qv"]);
                        if (P2 <= qv)
                        {
                            P2 = qv;
                            P2Level = Convert.ToDouble(row["Level"]);
                        }
                        else
                        {
                            break;
                        }
                    }
                    break;
                }
            }

            // 如果 P3Level 等于 P2Level，则 P3 无效
            if (Math.Abs(P3Level - P2Level) < 0.001)
            {
                P3 = LargeValue;
            }

            // 写入评估结果
            DataRow newRow = MyDataSet.Tables["LS_PunctureRiskAssessmentResult"].NewRow();
            newRow["DrillingID"] = DrillingID;
            newRow["P1"] = Math.Round(P1 / SpudcanParameter.Area, 2);
            newRow["P2"] = (P2 <= -LargeValue + 1) ? "-" : Math.Round(P2 / SpudcanParameter.Area, 2).ToString();
            newRow["P3"] = (P3 >= LargeValue - 1) ? "-" : Math.Round(P3 / SpudcanParameter.Area, 2).ToString();
            newRow["Fs1"] = (P2 <= -LargeValue + 1) ? "-" : Math.Round(P2 / P1, 2).ToString();
            newRow["Fs2"] = (P3 >= LargeValue - 1) ? "-" : Math.Round(P3 / P1, 2).ToString();

            bool isPunctureRiskOK = (P2 > -LargeValue + 1 && P2 / P1 >= 1.5) ||
                                    (P3 >= LargeValue - 1 || P3 / P1 >= 1.2);
            newRow["IsPunctureRiskOK"] = isPunctureRiskOK;

            MyDataSet.Tables["LS_PunctureRiskAssessmentResult"].Rows.Add(newRow);
        }

        //综合分析抗拔承载力（砂土/黏土/互层）
        public void AnalysisOfQb(
        int DrillingID,
        int ComputeLevelI,
        List<double> ComputeLevels,
        List<SoilParameter> Soils,
        LegParameter LegParameter,
        SpudcanParameter SpudcanParameter,
        CalculateParameter CalculateParameter)
        {
            const double LargeValue = 1e10;

            double[] Qb = new double[3];
            double[] QbSand = new double[3];
            double[] QbClay = new double[3];
            int DeepType = 0;
            int DeepTypeSand = 0;
            int DeepTypeClay = 0;
            string QbDescription = "";

            // 初始化数组
            for (int i = 0; i < 3; i++)
            {
                Qb[i] = LargeValue;
                QbSand[i] = LargeValue;
                QbClay[i] = LargeValue;
            }

            double computeLevel = ComputeLevels[ComputeLevelI];
            SoilParameter soil = GetSoil(computeLevel, Soils);
            double fb = CalculateParameter.fb;
            double B = SpudcanParameter.GetSpudcanB();
            double D = Soils[0].TopLevel - computeLevel;
            SoilParameter averageSoil = GetAverageSoilValue(Soils, computeLevel, Soils[0].TopLevel);
            double H = GetH(averageSoil.Phi, SpudcanParameter.B);
            bool isSameSoilType = GetIsSameUpSoilType(computeLevel, Soils);

            if (isSameSoilType)
            {
                // 均质土 - 砂土
                if (soil.SoilType != SoilType.Clay)
                {
                    EsMessageReporter.ReportMessageFunction(
                        "抗拔承载力计算：桩靴穿过均质土，持力层土类型为砂土，按砂土拔桩力计算",
                        EsMessageType.Normal
                    );

                    DeepTypeSand = (H < D) ? 3 : 1;
                    EsMessageReporter.ReportMessageFunction(
                        $"砂土拔桩力：判别深度H({H}m){(H < D ? "＜" : "≥")}插深D({D}m)，按{(H < D ? "深埋" : "浅埋")}计算",
                        EsMessageType.Normal
                    );

                    QbSand = GetQb_Sand(DeepTypeSand, CalculateParameter, computeLevel, LegParameter,
                                        SpudcanParameter, soil, Soils, fb, H, ref QbDescription, "Qu_Sand");
                }

                // 均质土 - 黏土
                if (soil.SoilType != SoilType.Sand)
                {
                    EsMessageReporter.ReportMessageFunction(
                        "抗拔承载力计算：桩靴穿过均质土，持力层土类型为粘土，按粘土拔桩力计算",
                        EsMessageType.Normal
                    );

                    DeepTypeClay = (B < D) ? 3 : 1;
                    EsMessageReporter.ReportMessageFunction(
                        $"粘土拔桩力：桩靴宽度B({B}m){(B < D ? "＜" : "≥")}插深D({D}m)，按{(B < D ? "深埋" : "浅埋")}计算",
                        EsMessageType.Normal
                    );

                    QbClay = GetQb_Clay(DeepTypeClay, CalculateParameter, computeLevel, LegParameter,
                                        SpudcanParameter, soil, Soils, fb, ref QbDescription, "Qu_Clay");
                }
            }
            else
            {
                // 多层砂粘土互层
                EsMessageReporter.ReportMessageFunction(
                    "抗拔承载力计算：桩靴穿过多层砂粘土互层，分别按砂土拔桩力和粘土拔桩力计算",
                    EsMessageType.Normal
                );

                DeepTypeSand = (H < D) ? 3 : 1;
                EsMessageReporter.ReportMessageFunction(
                    $"砂土拔桩力：判别深度H({H}m){(H < D ? "＜" : "≥")}插深D({D}m)，按{(H < D ? "深埋" : "浅埋")}计算",
                    EsMessageType.Normal
                );
                QbSand = GetQb_Sand(DeepTypeSand, CalculateParameter, computeLevel, LegParameter,
                                    SpudcanParameter, soil, Soils, fb, H, ref QbDescription, "Qu_Sand");

                DeepTypeClay = (B < D) ? 3 : 1;
                EsMessageReporter.ReportMessageFunction(
                    $"粘土拔桩力：桩靴宽度B({B}m){(B < D ? "＜" : "≥")}插深D({D}m)，按{(B < D ? "深埋" : "浅埋")}计算",
                    EsMessageType.Normal
                );
                QbClay = GetQb_Clay(DeepTypeClay, CalculateParameter, computeLevel, LegParameter,
                                    SpudcanParameter, soil, Soils, fb, ref QbDescription, "Qu_Clay");
            }

            // 选择最终结果
            for (int fbi = 0; fbi < 3; fbi++)
            {
                if (soil.SoilType == SoilType.Both || !isSameSoilType)
                {
                    // 混合土层：取砂土和黏土的最大值
                    Qb[fbi] = Math.Max(QbSand[fbi], QbClay[fbi]);
                    DeepType = (Qb[fbi] == QbClay[fbi]) ? DeepTypeClay : DeepTypeSand;
                }
                else
                {
                    if (soil.SoilType == SoilType.Clay)
                    {
                        Qb[fbi] = QbClay[fbi];
                        DeepType = DeepTypeClay;
                        DeepTypeSand = 0;
                        string label = (fbi == 2) ? "Qu_Sand" : $"Qu_S{fbi}";
                        QbDescription += label + "未计算;";
                    }
                    else // SoilType.Sand
                    {
                        Qb[fbi] = QbSand[fbi];
                        DeepType = DeepTypeSand;
                        DeepTypeClay = 0;
                        string label = (fbi == 2) ? "Qu_Clay" : $"Qu_C{fbi}";
                        QbDescription += label + "未计算;";
                    }
                }
            }

            AnalysisOfQb_WriteResult(
                DrillingID,
                ComputeLevelI,
                computeLevel,
                soil,
                ref Qb,
                ref QbSand,
                ref QbClay,
                ref DeepType,
                ref DeepTypeSand,
                ref DeepTypeClay,
                ref QbDescription
            );
        }

        //将抗拔承载力分析结果写入数据表
        public void AnalysisOfQb_WriteResult(
        int DrillingID,
        int ComputeLevelI,
        double ComputeLevel,
        SoilParameter Soil,
        ref double[] Qb,
        ref double[] QbSand,
        ref double[] QbClay,
        ref int DeepType,
        ref int DeepTypeSand,
        ref int DeepTypeClay,
        ref string QbDescription)
        {
            const double LargeValue = 1e10;

            // 移除描述字符串末尾的分号
            if (!string.IsNullOrEmpty(QbDescription) && QbDescription.EndsWith(";"))
            {
                QbDescription = QbDescription.Remove(QbDescription.Length - 1, 1);
            }

            DataRow newRow = MyDataSet.Tables["LS_PullResistanceResult"].NewRow();
            newRow["DrillingID"] = DrillingID;
            newRow["ID"] = ComputeLevelI + 1;
            newRow["SoilID"] = Soil.SoilID;
            newRow["Level"] = Math.Round(ComputeLevel, 2);
            newRow["DeepType_Sand"] = DeepTypeSand;
            newRow["Qu_Sand"] = Math.Round(QbSand[2], 2);
            newRow["DeepType_Clay"] = DeepTypeClay;
            newRow["Qu_Clay"] = Math.Round(QbClay[2], 2);
            newRow["Description"] = QbDescription;

            // 根据选择模式计算最终 Qu 和 QuP
            int quSelectMode = 0;
            DataRow[] levelRows = MyDataSet.Tables["LS_CalculationLevels"].Select(
                $"Level={Math.Round(ComputeLevel, 2)} and DrillingID={DrillingID}"
            );
            if (levelRows.Length > 0)
            {
                quSelectMode = Convert.ToInt32(levelRows[0]["SelectMode_Qb"]);
            }

            switch (quSelectMode)
            {
                case 0:
                    newRow["Qu"] = Math.Round(Qb[2], 2);
                    newRow["QuP"] = (Qb[2] >= LargeValue) ? LargeValue : Math.Round(Qb[2] / 9.8, 2);
                    newRow["DeepType"] = DeepType;
                    newRow["SelectMode"] = 0;
                    newRow["Qu0"] = Math.Round(Qb[0], 2);
                    newRow["Qu1"] = Math.Round(Qb[1], 2);
                    break;
                case 1:
                    newRow["Qu"] = Math.Round(QbSand[2], 2);
                    newRow["QuP"] = (QbSand[2] >= LargeValue) ? LargeValue : Math.Round(QbSand[2] / 9.8, 2);
                    newRow["DeepType"] = DeepTypeSand;
                    newRow["SelectMode"] = 1;
                    newRow["Qu0"] = Math.Round(QbSand[0], 2);
                    newRow["Qu1"] = Math.Round(QbSand[1], 2);
                    break;
                case 2:
                    newRow["Qu"] = Math.Round(QbClay[2], 2);
                    newRow["QuP"] = (QbClay[2] >= LargeValue) ? LargeValue : Math.Round(QbClay[2] / 9.8, 2);
                    newRow["DeepType"] = DeepTypeClay;
                    newRow["SelectMode"] = 2;
                    newRow["Qu0"] = Math.Round(QbClay[0], 2);
                    newRow["Qu1"] = Math.Round(QbClay[1], 2);
                    break;
            }

            // 将大值替换为 "-"
            string[] resultTitles = { "QuP", "Qu", "Qu_Sand", "Qu_Clay", "Qu0", "Qu1" };
            foreach (string title in resultTitles)
            {
                object value = newRow[title];
                if (value != DBNull.Value && Convert.ToDouble(value) >= LargeValue)
                {
                    newRow[title] = "-";
                }
            }

            MyDataSet.Tables["LS_PullResistanceResult"].Rows.Add(newRow);
        }

        //通过插值计算指定极限值对应的深度和承载力
        public void GetInterpolationValue(
        int DrillingID,
        double LimitValue,
        bool IsPressValue,
        ref string ErrorString)
        {
            const double LargeValue = 1e10;

            // 获取顶层标高
            double topLevel = Convert.ToDouble(
                MyDataSet.Tables["LS_CalculationLevels"].Compute("Max(Level)", $"DrillingID={DrillingID}")
            );

            // 获取钻孔土层字典
            Dictionary<int, Dictionary<double, int>> levelIDByDrillingDic = GetLevelIDByDrillingDic();

            // 选择结果表
            DataTable resultTab;
            DataTable anotherResultTab;
            string selectParam;
            string anotherSelectParam;

            if (IsPressValue)
            {
                resultTab = MyDataSet.Tables["LS_PressResistanceResult"];
                selectParam = "Qv";
                anotherResultTab = MyDataSet.Tables["LS_PullResistanceResult"];
                anotherSelectParam = "Qu";
            }
            else
            {
                resultTab = MyDataSet.Tables["LS_PullResistanceResult"];
                selectParam = "Qu";
                anotherResultTab = MyDataSet.Tables["LS_PressResistanceResult"];
                anotherSelectParam = "Qv";
            }

            double upLevel = 0, upValue = 0;
            double downLevel = 0, downValue = 0;
            var upResultDic = new Dictionary<double, double>();
            var downResultDic = new Dictionary<double, double>();

            // 遍历结果，分别收集上下界数据
            string filter = $"DrillingID={DrillingID} and {selectParam}<>'-'";
            DataRow[] rows = resultTab.Select(filter, "Level DESC");

            foreach (DataRow aRow in rows)
            {
                double level = Convert.ToDouble(aRow["Level"]);
                double value = Convert.ToDouble(aRow[selectParam]);

                if (value <= LimitValue && !upResultDic.ContainsKey(level))
                {
                    upResultDic.Add(level, value);
                }

                if (value >= LimitValue && !downResultDic.ContainsKey(level))
                {
                    downResultDic.Add(level, value);
                }
            }

            // 如果上下界都存在，进行插值计算
            if (upResultDic.Count > 0 && downResultDic.Count > 0)
            {
                // 获取下界（第一个大于等于LimitValue的点）
                downLevel = GetFirstKey(downResultDic);
                downValue = downResultDic[downLevel];

                // 获取上界（最后一个小于等于LimitValue的点）
                upLevel = GetLastKey(upResultDic);
                upValue = upResultDic[upLevel];

                // 线性插值计算深度
                double level = downLevel + (downValue - LimitValue) * (upLevel - downLevel) / (downValue - upValue);

                // 创建结果行
                DataRow newRow = MyDataSet.Tables["LS_DepthResult"].NewRow();
                newRow["LimitForce"] = LimitValue.ToString("N2");
                newRow["IsUserAdd"] = true;
                newRow["DrillingID"] = DrillingID;
                newRow[selectParam] = LimitValue.ToString("N2");
                newRow["SuggestedDepth"] = (topLevel - level).ToString("N2");

                // 获取对应的SoilID和另一个承载力值
                int soilID = 0;
                double tempResult = 0;

                if (levelIDByDrillingDic.ContainsKey(DrillingID) &&
                    levelIDByDrillingDic[DrillingID].ContainsKey(Math.Round(level, 2)))
                {
                    // 精确匹配标高
                    soilID = levelIDByDrillingDic[DrillingID][Math.Round(level, 2)];

                    string anotherFilter = $"DrillingID={DrillingID} and Level={level}";
                    DataRow[] anotherRows = anotherResultTab.Select(anotherFilter, "Level DESC");
                    if (anotherRows.Length > 0)
                    {
                        if (!double.TryParse(anotherRows[0][anotherSelectParam].ToString(), out tempResult))
                        {
                            tempResult = LargeValue;
                        }
                    }
                    else
                    {
                        tempResult = LargeValue;
                    }
                }
                else
                {
                    // 插值获取SoilID
                    soilID = GetSoilIDByInterpolation(levelIDByDrillingDic, DrillingID, level);

                    // 插值获取另一个承载力值
                    tempResult = InterpolateAnotherValue(anotherResultTab, DrillingID, level, anotherSelectParam);
                }

                newRow[anotherSelectParam] = (tempResult >= LargeValue) ? "-" : tempResult.ToString("N2");
                newRow["SupportSoilID"] = soilID;

                // 获取持力层强度
                DataRow[] soilRows = MyDataSet.Tables["LS_Soil"].Select($"ID={soilID}");
                if (soilRows.Length > 0)
                {
                    DataRow soilRow = soilRows[0];
                    if (Convert.ToInt32(soilRow["Type"]) == 1) // 砂土
                    {
                        newRow["SupportSoilStrength"] = Convert.ToDouble(soilRow["UnderWaterPhi"]);
                    }
                    else // 黏土
                    {
                        newRow["SupportSoilStrength"] = Convert.ToDouble(soilRow["Su0"]);
                    }
                }

                MyDataSet.Tables["LS_DepthResult"].Rows.Add(newRow);
                ErrorString = "";
            }
            else
            {
                ErrorString = "不在范围内！";
            }
        }

        private double GetFirstKey(Dictionary<double, double> dict)
        {
            foreach (double key in dict.Keys)
            {
                return key;
            }
            return 0;
        }

        private double GetLastKey(Dictionary<double, double> dict)
        {
            double lastKey = 0;
            foreach (double key in dict.Keys)
            {
                lastKey = key;
            }
            return lastKey;
        }

        private int GetSoilIDByInterpolation(
            Dictionary<int, Dictionary<double, int>> levelIDByDrillingDic,
            int drillingID,
            double level)
        {
            if (!levelIDByDrillingDic.ContainsKey(drillingID))
            {
                return 0;
            }

            var levelDict = levelIDByDrillingDic[drillingID];
            List<double> sortedKeys = new List<double>(levelDict.Keys);
            sortedKeys.Sort();
            sortedKeys.Reverse(); // 从大到小排序

            for (int i = 0; i < sortedKeys.Count - 1; i++)
            {
                double level1 = sortedKeys[i];
                double level2 = sortedKeys[i + 1];

                if (level1 > level && level2 < level)
                {
                    return levelDict[level2];
                }
            }

            return 0;
        }

        private double InterpolateAnotherValue(
            DataTable anotherResultTab,
            int drillingID,
            double level,
            string anotherSelectParam)
        {
            string filter = $"DrillingID={drillingID} and {anotherSelectParam}<>'-'";
            DataRow[] rows = anotherResultTab.Select(filter, "Level DESC");

            DataRow downRow = null;
            DataRow upRow = null;

            foreach (DataRow row in rows)
            {
                double rowLevel = Convert.ToDouble(row["Level"]);
                if (rowLevel >= level && downRow == null)
                {
                    downRow = row;
                }
                if (rowLevel <= level)
                {
                    upRow = row;
                    break;
                }
            }

            if (downRow != null && upRow != null)
            {
                double downLevel = Convert.ToDouble(downRow["Level"]);
                double upLevel = Convert.ToDouble(upRow["Level"]);
                double downValue = Convert.ToDouble(downRow[anotherSelectParam]);
                double upValue = Convert.ToDouble(upRow[anotherSelectParam]);

                return downValue + (downLevel - level) * (upValue - downValue) / (downLevel - upLevel);
            }

            return 1e10;
        }

        //公式法主计算流程（遍历钻孔计算承载力）
        public void CaculateByEquation(bool Boats = false)
        {
            try
            {
                WarningMessageList = new List<string>();
                ErrorMessageList = new List<string>();

                LegParameter legParameter = GetLegParameter();
                SpudcanParameter spudcanParameter = GetSpudcanParameter();
                CalculateParameter calculateParameter = GetCaculateParameter();
                bool selectSingleDrilling = Convert.ToBoolean(
                    MyDataSet.Tables["LS_Common"].Rows[0]["UseSingleDrilling"]
                );

                // 清空结果表
                MyDataSet.Tables["LS_Holl"].Clear();
                MyDataSet.Tables["LS_PressResistanceResult"].Clear();
                MyDataSet.Tables["LS_PullResistanceResult"].Clear();
                MyDataSet.Tables["LS_PunctureRiskAssessmentResult"].Clear();

                double pressLimitValue = Math.Round(calculateParameter.PressForce * 9.8, 6);

                // 获取钻孔ID列表
                List<int> drillingIDs = GetDrillingIDs(selectSingleDrilling);

                foreach (int drillingID in drillingIDs)
                {
                    EsMessageReporter.ReportMessageFunction(
                        $"计算准备,计算编号{drillingID}钻孔>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>",
                        EsMessageType.Normal
                    );

                    string errorString = "";
                    List<SoilParameter> soils = GetSoils(
                        calculateParameter.UnderWaterPhiSubtractValue,
                        drillingID,
                        selectSingleDrilling,
                        ref errorString,
                        Boats
                    );

                    if (!string.IsNullOrEmpty(errorString))
                    {
                        EsMessageReporter.ReportMessageFunction(errorString, EsMessageType.Error);
                        return;
                    }

                    // 获取计算高程列表
                    List<double> computeLevels = new List<double>();
                    string filter = $"DrillingID={drillingID}";
                    foreach (DataRow lRow in MyDataSet.Tables["LS_CalculationLevels"].Select(filter, "Level DESC"))
                    {
                        computeLevels.Add(Convert.ToDouble(lRow["Level"]));
                    }

                    // 计算极限洞深
                    GetHc(drillingID, spudcanParameter, calculateParameter, soils);

                    // 计算承载力
                    for (int i = 0; i < computeLevels.Count; i++)
                    {
                        EsMessageReporter.ReportProgressFunction(100 * (i + 1) / computeLevels.Count);
                        EsMessageReporter.ReportMessageFunction($"计算高程={computeLevels[i]}", EsMessageType.Normal);

                        // 计算抗压
                        AnalysisOfQv(drillingID, i, computeLevels, soils, legParameter, spudcanParameter, calculateParameter);

                        // 计算抗拔
                        AnalysisOfQb(drillingID, i, computeLevels, soils, legParameter, spudcanParameter, calculateParameter);
                    }

                    // 穿刺风险评估
                    AssessmentOfPunctureRisk(drillingID, pressLimitValue, spudcanParameter);
                }

                // 汇总计算深度结果
                string errorStringResult = "";
                CalculateDepthResult(false, pressLimitValue, ref errorStringResult);
                MyDataSet.AcceptChanges();
                EsMessageReporter.ReportMessageFunction($"公式法计算结束{Environment.NewLine}", EsMessageType.Normal);
            }
            catch (Exception ex)
            {
                EsMessageReporter.ReportMessageFunction($"公式法计算错误: {ex.Message}", EsMessageType.Error);
            }
        }

        private List<int> GetDrillingIDs(bool selectSingleDrilling)
        {
            List<int> drillingIDs = new List<int>();

            if (selectSingleDrilling)
            {
                foreach (DataRow aRow in MyDataSet.Tables["LS_LegSoilLayer"].Rows)
                {
                    int drillingID = Convert.ToInt32(aRow["DrillingID"]);
                    if (!drillingIDs.Contains(drillingID))
                    {
                        drillingIDs.Add(drillingID);
                    }
                }
            }
            else
            {
                foreach (DataRow aRow in MyDataSet.Tables["LS_SoilDrilling"].Rows)
                {
                    int drillingID = Convert.ToInt32(aRow["ID"]);
                    if (!drillingIDs.Contains(drillingID))
                    {
                        drillingIDs.Add(drillingID);
                    }
                }
            }

            return drillingIDs;
        }



    }

}
