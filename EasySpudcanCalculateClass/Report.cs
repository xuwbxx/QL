using Easy.EsWord;
using EasyFiniteElement.EasyStructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Easy.EasyPlot;


namespace EasySpudcanCalculateClass
{
    public class Report
    {
        EsWordDocument SETTING_DOCUMENT;
        Color HeaderColor, FooterColor;
        Font HeaderFont, FooterFont, Heading1Font, Heading2Font, TableFont, BodyFont, CharacterFont;
        EsWordTextAlign HeaderAlign, FooterAlign;
        DataSet mydataset;
        int SectionNumber, SubSectionNumber, ChapterNumber;
        private string[,] Table;
        public long[] TableWidths;
        Dictionary<int, string> SoilNames;
        private string MainReportPath = "";

        public Report(EasyStructureKit StructureKit)
        {
            SETTING_DOCUMENT = new EsWordDocument(EsWordDocumentFormat.A4);
            HeaderColor = Color.FromArgb(0, 0, 0);
            FooterColor = Color.FromArgb(0, 0, 0);
            HeaderFont = new System.Drawing.Font("宋体", 9, FontStyle.Regular); //小五
            FooterFont = new System.Drawing.Font("宋体", 9, FontStyle.Regular);
            Heading1Font = new System.Drawing.Font("黑体", 14, FontStyle.Regular);
            Heading2Font = new System.Drawing.Font("黑体", 12, FontStyle.Regular);
            TableFont = new System.Drawing.Font("宋体", 10, FontStyle.Regular);
            BodyFont = new System.Drawing.Font("宋体", 12, FontStyle.Regular);
            CharacterFont = new System.Drawing.Font("Times New Roman", 12, FontStyle.Regular);
            HeaderAlign = EsWordTextAlign.Center;
            FooterAlign = EsWordTextAlign.Right;
            mydataset = StructureKit.StructureData.GetData();
            ChapterNumber = 0;
            SoilNames = new Dictionary<int, string>();
        }

        /// <summary>
        /// 单船版报告
        /// </summary>
        /// <param name="OutputPath"></param>
        //public void BeginWrite(string OutputPath = "")
        //{
        //    try
        //    {
        //        Setting_HeaderAndFooter();
        //        Setting_Cover();

        //        Setting_Text_StructureData();
        //        Setting_Text_CalculationParameter();
        //        Setting_Text_Result();

        //        if (OutputPath == "")
        //        {
        //            SaveFileDialog SaveReportDialog = new SaveFileDialog();
        //            SaveReportDialog.Filter = "(*.doc)|*.doc";
        //            if (SaveReportDialog.ShowDialog() == DialogResult.OK)
        //            {
        //                if (!string.IsNullOrEmpty(SaveReportDialog.FileName))
        //                {
        //                    if (System.IO.File.Exists(SaveReportDialog.FileName))
        //                    {
        //                        System.IO.File.Delete(SaveReportDialog.FileName);
        //                    }
        //                    SETTING_DOCUMENT.SaveToFile(SaveReportDialog.FileName);
        //                }
        //                else
        //                {
        //                    return;
        //                }
        //                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(SaveReportDialog.FileName) { UseShellExecute = true });
        //            }
        //        }
        //        else
        //        {
        //            OutputPath += "\\计算报告.doc";
        //            if (System.IO.File.Exists(OutputPath))
        //            {
        //                System.IO.File.Delete(OutputPath);
        //            }
        //            SETTING_DOCUMENT.SaveToFile(OutputPath);
        //        }
        //    }
        //    catch
        //    {
        //        //等价VB On Error Resume Next：发生异常直接忽略，不抛出
        //    }
        //}

        //public void BeginWrite_Template(string TaskName, string ProjectName, string OutputPath = "", bool UseMetaFile = true, int DrillingID = 0)
        //{
        //    try
        //    {
        //        Heading1Font = new Font("宋体", 16, FontStyle.Bold); //New Font("Times New Roman", 14, FontStyle.Bold)
        //        Heading2Font = new Font("宋体", 14, FontStyle.Bold);
        //        BodyFont = new Font("宋体", 12, FontStyle.Regular);
        //        TableFont = new Font("宋体", 10.5f, FontStyle.Regular);

        //        HeaderAlign = EsWordTextAlign.Right;
        //        FooterAlign = EsWordTextAlign.Right;

        //        if (string.IsNullOrEmpty(ProjectName))
        //        {
        //            ProjectName = mydataset.Tables["LS_StructureData"].Rows[0]["WindFieldName"].ToString();
        //        }

        //        string headerText = string.IsNullOrEmpty(TaskName) ? ProjectName : TaskName;
        //        Setting_Template_HeaderAndFooter(headerText);
        //        Setting_Template(ProjectName, DrillingID, UseMetaFile);

        //        string DrillingName = "";

        //        if (string.IsNullOrEmpty(OutputPath))
        //        {
        //            //主报告
        //            if (DrillingID == 0)
        //            {
        //                SaveFileDialog SaveReportDialog = new SaveFileDialog();
        //                SaveReportDialog.Filter = "(*.doc)|*.doc";
        //                if (SaveReportDialog.ShowDialog() == DialogResult.OK)
        //                {
        //                    if (!string.IsNullOrEmpty(SaveReportDialog.FileName))
        //                    {
        //                        MainReportPath = SaveReportDialog.FileName;
        //                        if (File.Exists(SaveReportDialog.FileName))
        //                        {
        //                            File.Delete(SaveReportDialog.FileName);
        //                        }
        //                        SETTING_DOCUMENT.SaveToFile(SaveReportDialog.FileName);
        //                        System.Diagnostics.Process.Start(SaveReportDialog.FileName);
        //                    }
        //                }
        //                else
        //                {
        //                    return;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            //分报告
        //            if (DrillingID != 0)
        //            {
        //                DataRow row = mydataset.Tables["LS_TempSoilDrilling"].Select($"DrillingID={DrillingID}", "DrillingID")[0];
        //                DrillingName = "_" + row["DrillingName"].ToString();
        //            }
        //            string fileName = string.IsNullOrEmpty(TaskName) ? ProjectName : TaskName;
        //            OutputPath = Path.Combine(OutputPath, $"{fileName}{DrillingName}.doc");

        //            MainReportPath = OutputPath;
        //            if (File.Exists(OutputPath))
        //            {
        //                File.Delete(OutputPath);
        //            }
        //            SETTING_DOCUMENT.SaveToFile(OutputPath);
        //            //System.Diagnostics.Process.Start(OutputPath);
        //        }
        //    }
        //    catch
        //    {
        //        //VB On Error Resume Next，出错直接忽略
        //    }
        //}

        /// <summary>
        /// 生成报告模板，直接保存到传入的完整文件路径，类库内部不再弹出保存对话框
        /// </summary>
        /// <param name="TaskName">任务名称</param>
        /// <param name="ProjectName">项目名称</param>
        /// <param name="OutputPath">完整输出文件路径（必填，例：D:\\xxx\\报告.doc）</param>
        /// <param name="UseMetaFile">是否使用元文件</param>
        /// <param name="DrillingID">钻孔ID，0为主报告</param>
        public void BeginWrite_Template(string TaskName, string ProjectName, string OutputPath, bool UseMetaFile = true, int DrillingID = 0)
        {
            try
            {
                Heading1Font = new Font("宋体", 16, FontStyle.Bold);
                Heading2Font = new Font("宋体", 14, FontStyle.Bold);
                BodyFont = new Font("宋体", 12, FontStyle.Regular);
                TableFont = new Font("宋体", 10.5f, FontStyle.Regular);

                HeaderAlign = EsWordTextAlign.Right;
                FooterAlign = EsWordTextAlign.Right;

                if (string.IsNullOrEmpty(ProjectName))
                {
                    ProjectName = mydataset.Tables["LS_StructureData"].Rows[0]["WindFieldName"].ToString();
                }

                string headerText = string.IsNullOrEmpty(TaskName) ? ProjectName : TaskName;
                Setting_Template_HeaderAndFooter(headerText);
                Setting_Template(ProjectName, DrillingID, UseMetaFile);

                string DrillingName = "";

                //分报告：DrillingID不为0，拼接文件名后缀
                if (DrillingID != 0)
                {
                    DataRow row = mydataset.Tables["LS_TempSoilDrilling"].Select($"DrillingID={DrillingID}", "DrillingID")[0];
                    DrillingName = "_" + row["DrillingName"].ToString();
                }

                string fileName = string.IsNullOrEmpty(TaskName) ? ProjectName : TaskName;
                //从传入的完整路径中取出目录，重新拼接带钻孔名称的文件名
                string dir = Path.GetDirectoryName(OutputPath);
                OutputPath = Path.Combine(dir, $"{fileName}{DrillingName}.doc");

                MainReportPath = OutputPath;

                //文件存在则删除旧文件
                if (File.Exists(OutputPath))
                {
                    File.Delete(OutputPath);
                }
                //执行保存
                SETTING_DOCUMENT.SaveToFile(OutputPath);
            }
            catch
            {
                //等价VB On Error Resume Next，异常直接忽略
            }
        }


        private void Setting_HeaderAndFooter()
        {
            try
            {
                SETTING_DOCUMENT.HeaderStart();
                SETTING_DOCUMENT.SetFont(new System.Drawing.Font(HeaderFont.FontFamily, HeaderFont.Size, FontStyle.Regular));
                SETTING_DOCUMENT.SetForegroundColor(HeaderColor);
                SETTING_DOCUMENT.SetTextAlign(HeaderAlign);
                SETTING_DOCUMENT.Write("自升式平台插拔桩计算软件 V1.0 计算报告书");
                SETTING_DOCUMENT.HeaderEnd();

                SETTING_DOCUMENT.FooterStart();
                SETTING_DOCUMENT.SetFont(new System.Drawing.Font(FooterFont.FontFamily, FooterFont.Size));
                SETTING_DOCUMENT.SetForegroundColor(FooterColor);
                SETTING_DOCUMENT.SetTextAlign(FooterAlign);
                SETTING_DOCUMENT.Write("中交第三航务工程局有限公司");
                SETTING_DOCUMENT.SetPageNumbering(1);
                SETTING_DOCUMENT.FooterEnd();
            }
            catch
            {
                //等价 On Error Resume Next，忽略异常
            }
        }
        private void Setting_Cover()
        {
            try
            {
                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center);
                SETTING_DOCUMENT.WriteLine("");

                SETTING_DOCUMENT.SetFont(new System.Drawing.Font("宋体", 12, FontStyle.Bold));
                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Right);
                SETTING_DOCUMENT.Write('\r' + "编号:______________");
                SETTING_DOCUMENT.SetFont(new System.Drawing.Font("宋体", 14, FontStyle.Regular));
                SETTING_DOCUMENT.WriteLine("");
                SETTING_DOCUMENT.WriteLine("");
                SETTING_DOCUMENT.WriteLine("");
                SETTING_DOCUMENT.WriteLine("");
                SETTING_DOCUMENT.WriteLine("");
                SETTING_DOCUMENT.SetFont(new System.Drawing.Font("宋体", 26, FontStyle.Bold));
                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center);
                SETTING_DOCUMENT.WriteLine("设 计 计 算 书");
                SETTING_DOCUMENT.SetFont(new System.Drawing.Font("宋体", 14, FontStyle.Regular));
                SETTING_DOCUMENT.WriteLine("");
                SETTING_DOCUMENT.WriteLine("");
                SETTING_DOCUMENT.WriteLine("");
                SETTING_DOCUMENT.WriteLine("");
                SETTING_DOCUMENT.WriteLine("");
                SETTING_DOCUMENT.SetFont(new System.Drawing.Font("宋体", 15, FontStyle.Bold));
                SETTING_DOCUMENT.Write('\r' + "工程名称______________________________");
                SETTING_DOCUMENT.SetFont(new System.Drawing.Font("宋体", 14, FontStyle.Regular));
                SETTING_DOCUMENT.WriteLine("");
                SETTING_DOCUMENT.WriteLine("");
                SETTING_DOCUMENT.SetFont(new System.Drawing.Font("宋体", 15, FontStyle.Bold));
                SETTING_DOCUMENT.Write('\r' + "设计阶段___________ 专业_____页数_____");
                SETTING_DOCUMENT.SetFont(new System.Drawing.Font("宋体", 14, FontStyle.Regular));
                SETTING_DOCUMENT.WriteLine("");
                SETTING_DOCUMENT.WriteLine("");
                SETTING_DOCUMENT.SetFont(new System.Drawing.Font("宋体", 15, FontStyle.Bold));
                SETTING_DOCUMENT.Write('\r' + "计算书名称____________________________");
                SETTING_DOCUMENT.SetFont(new System.Drawing.Font("宋体", 14, FontStyle.Regular));
                SETTING_DOCUMENT.WriteLine("");
                SETTING_DOCUMENT.WriteLine("");
                SETTING_DOCUMENT.WriteLine("");
                SETTING_DOCUMENT.WriteLine("");
                SETTING_DOCUMENT.WriteLine("");
                SETTING_DOCUMENT.WriteLine("");
                SETTING_DOCUMENT.WriteLine("");

                SETTING_DOCUMENT.SetFont(new System.Drawing.Font("宋体", 14, FontStyle.Bold));
                SETTING_DOCUMENT.Write('\r' + "计算:_______________日期_______________");
                SETTING_DOCUMENT.SetFont(new System.Drawing.Font("宋体", 14, FontStyle.Regular));
                SETTING_DOCUMENT.WriteLine("");
                SETTING_DOCUMENT.WriteLine("");
                SETTING_DOCUMENT.SetFont(new System.Drawing.Font("宋体", 14, FontStyle.Bold));
                SETTING_DOCUMENT.Write('\r' + "校核:_______________日期_______________");
                SETTING_DOCUMENT.SetFont(new System.Drawing.Font("宋体", 14, FontStyle.Regular));
                SETTING_DOCUMENT.WriteLine("");
                SETTING_DOCUMENT.WriteLine("");
                SETTING_DOCUMENT.SetFont(new System.Drawing.Font("宋体", 14, FontStyle.Bold));
                SETTING_DOCUMENT.Write('\r' + "审核:_______________日期_______________");
                SETTING_DOCUMENT.WriteLine("");
                SETTING_DOCUMENT.WriteLine("");
                SETTING_DOCUMENT.WriteLine("");
                SETTING_DOCUMENT.WriteLine("");
                SETTING_DOCUMENT.WriteLine("");
                SETTING_DOCUMENT.WriteLine("");
                SETTING_DOCUMENT.WriteLine("");
                SETTING_DOCUMENT.SetFont(new System.Drawing.Font("宋体", 18, FontStyle.Bold));
                SETTING_DOCUMENT.WriteLine("中交第三航务工程局有限公司");
                SETTING_DOCUMENT.SetFont(new System.Drawing.Font("宋体", 14, FontStyle.Regular));
                SETTING_DOCUMENT.WriteLine("\r");
                SETTING_DOCUMENT.NewPage();
            }
            catch
            {
                //等价 On Error Resume Next，忽略异常
            }
        }
        private void Setting_Text_StructureData()
        {
            try
            {
                ChapterNumber += 1;
                SectionNumber = 0;
                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center);
                SETTING_DOCUMENT.SetFont(Heading1Font);
                SETTING_DOCUMENT.WriteLine("第" + GetChineseNumber() + "章 结构信息");

                int J;
                DataRow Irow;
                DataRow[] Rows;
                string TabName;
                int TabColNumber;
                string[] TabColNames;
                string[] TabColShowNames;

                //工程信息
                SectionNumber += 1;
                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left);
                SETTING_DOCUMENT.SetFont(Heading2Font);
                SETTING_DOCUMENT.WriteLine('\r' + ChapterNumber + "." + SectionNumber + " 工程信息");
                SETTING_DOCUMENT.SetFont(TableFont);
                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center);

                Irow = mydataset.Tables["LS_StructureData"].Rows[0];
                Table = new string[2, 9];
                Table[0, 0] = "计算人姓名";
                Table[0, 1] = "联系方式";
                Table[0, 2] = "风场名";
                Table[0, 3] = "船名";
                Table[0, 4] = "拔桩能力(t)";
                Table[0, 5] = "风场区域水深(m)";
                Table[0, 6] = "气隙(m)";
                Table[0, 7] = "冲桩系统是否具备";
                Table[0, 8] = "工作状态是否良好";

                TableWidths = new long[9];
                for (int i = 0; i <= 8; i++)
                {
                    TableWidths[i] = 900;
                }
                TableWidths[0] = 1300;
                TableWidths[1] = 1300;

                Table[1, 0] = Irow["UserName"].ToString();
                Table[1, 1] = Irow["ContactNumber"].ToString();
                Table[1, 2] = Irow["WindFieldName"].ToString();
                Table[1, 3] = Irow["BoatName"].ToString();
                Table[1, 4] = Irow["PullingCapacity"].ToString();
                Table[1, 5] = Irow["WindFieldWaterHeight"].ToString();
                Table[1, 6] = Irow["AirGap"].ToString();
                Table[1, 7] = (bool)Irow["GetJettingSystem"] ? "是" : "否";
                Table[1, 8] = (bool)Irow["GoodWorking"] ? "是" : "否";

                inset_a_table(ref SETTING_DOCUMENT, Table, 9, 2, TableWidths);
                SETTING_DOCUMENT.WriteLine("\r"); //回车

                //桩腿
                SectionNumber += 1;
                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left);
                SETTING_DOCUMENT.SetFont(Heading2Font);
                SETTING_DOCUMENT.WriteLine('\r' + ChapterNumber + "." + SectionNumber + " 桩腿");
                SETTING_DOCUMENT.SetFont(TableFont);
                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center);

                Irow = mydataset.Tables["LS_Leg"].Rows[0];
                Table = new string[3, 5];
                Table[0, 0] = "类型";
                Table[0, 1] = ((int)Irow["Type"] == 1) ? "等效直径(m)" : "桁架边长(m)";
                Table[0, 2] = "等效周长(m)";
                Table[0, 3] = "等效截面积(m{\\super 2})";
                Table[0, 4] = "有效长度(m)";

                TableWidths = new long[5];
                for (int i = 0; i <= 4; i++)
                {
                    TableWidths[i] = 1300;
                }

                Table[1, 0] = ((int)Irow["Type"] == 1) ? "圆柱式" : "桁架式";
                Table[1, 1] = Irow["Diameter"].ToString();
                Table[1, 2] = Irow["Circumference"].ToString();
                Table[1, 3] = Irow["Area"].ToString();
                Table[1, 4] = Irow["ActiveLength"].ToString();

                inset_a_table(ref SETTING_DOCUMENT, Table, 5, 2, TableWidths);
                SETTING_DOCUMENT.WriteLine("\r"); //回车

                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified);
                SETTING_DOCUMENT.SetFont(BodyFont);
                SETTING_DOCUMENT.WriteLine("*等效截面积用于计算回流土体体积。");

                //桩靴
                SectionNumber += 1;
                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left);
                SETTING_DOCUMENT.SetFont(Heading2Font);
                SETTING_DOCUMENT.WriteLine('\r' + ChapterNumber + "." + SectionNumber + " 桩靴");
                SETTING_DOCUMENT.SetFont(TableFont);
                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center);

                int SuInputType = (int)mydataset.Tables["Ls_Common"].Rows[0]["SuInputType"];

                Table = new string[3, 7];
                Table[0, 0] = "类型";
                Table[0, 1] = "等效直径(m)";
                Table[0, 2] = "周长(m)";
                Table[0, 3] = "面积(m{\\super 2})";
                Table[0, 4] = "体积(m{\\super 3})";
                Table[0, 5] = "(含桩腿)水下重量(kN)";
                Table[0, 6] = "几何参数";

                TableWidths = new long[7];
                for (int i = 0; i <= 6; i++)
                {
                    TableWidths[i] = 1000;
                }
                TableWidths[6] = 1500;

                Irow = mydataset.Tables["LS_Spudcan"].Rows[0];
                Table[1, 0] = ((int)Irow["Type"] == 1) ? "类四边形" : "类圆形";
                Table[1, 1] = Irow["Diameter"].ToString();
                Table[1, 2] = Irow["Circumference"].ToString();
                Table[1, 3] = Irow["Area"].ToString();
                Table[1, 4] = Irow["Volume"].ToString();
                Table[1, 5] = Irow["Weight"].ToString();
                Table[1, 6] = Irow["Parameter"].ToString();

                inset_a_table(ref SETTING_DOCUMENT, Table, 7, 2, TableWidths);
                SETTING_DOCUMENT.WriteLine("\r"); //回车

                //地层-土质物理指标
                SectionNumber += 1;
                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left);
                SETTING_DOCUMENT.SetFont(Heading2Font);
                SETTING_DOCUMENT.WriteLine('\r' + ChapterNumber + "." + SectionNumber + " 土质物理指标");
                SETTING_DOCUMENT.SetFont(TableFont);
                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center);

                int soilRowCount = mydataset.Tables["LS_Soil"].Rows.Count;
                Table = new string[soilRowCount + 1, 12];
                Table[0, 0] = "名称";
                Table[0, 1] = "类型";

                int NColumn;
                if (SuInputType == 1)
                {
                    Table[0, 2] = "不排水抗剪强度Su0(kPa)";
                    Table[0, 3] = "强度增长系数(kPa/m)";
                    Table[0, 4] = "饱和重度kN/m{\\super 3}";
                    Table[0, 5] = "内摩擦角(°)";
                    Table[0, 6] = "弹性模量(kN/m{\\super 3})";
                    Table[0, 7] = "泊松比";
                    Table[0, 8] = "重度折减系数";
                    Table[0, 9] = "强度折减系数";
                    Table[0, 10] = "弹性模量折减系数";
                    Table[0, 11] = "泊松比折减系数";
                    NColumn = 12;
                }
                else
                {
                    Table[0, 2] = "不排水抗剪强度(kPa)";
                    Table[0, 3] = "饱和重度kN/m{\\super 3}";
                    Table[0, 4] = "水下摩擦角(°)";
                    Table[0, 5] = "弹性模量(kN/m{\\super 3})";
                    Table[0, 6] = "泊松比";
                    Table[0, 7] = "重度折减系数";
                    Table[0, 8] = "强度折减系数";
                    Table[0, 9] = "弹性模量折减系数";
                    Table[0, 10] = "泊松比折减系数";
                    NColumn = 11;
                }

                TableWidths = new long[13];
                for (int i = 0; i <= 12; i++)
                {
                    TableWidths[i] = 900;
                }
                TableWidths[0] = 1500;

                J = 0;
                foreach (DataRow Arow in mydataset.Tables["LS_Soil"].Rows)
                {
                    J++;
                    SoilNames.Add((int)Arow["ID"], Arow["Name"].ToString());
                    Table[J, 0] = Arow["Name"].ToString();
                    Table[J, 1] = mydataset.Tables["LS_SoilType"].Select("ID=" + Arow["Type"])[0]["Name"].ToString();

                    if (SuInputType == 1)
                    {
                        Table[J, 2] = Arow["Su0"].ToString();
                        Table[J, 3] = Arow["DSu"].ToString();
                        Table[J, 4] = Arow["UnderWaterWeight"].ToString();
                        Table[J, 5] = Arow["UnderWaterPhi"].ToString();
                        Table[J, 6] = Arow["E"].ToString();
                        Table[J, 7] = Arow["mu"].ToString();
                        Table[J, 8] = Arow["OnLegWeightReduceCoeff"].ToString();
                        Table[J, 9] = Arow["OnLegStrenthengReduceCoeff"].ToString();
                        Table[J, 10] = Arow["OnLegEReduceCoeff"].ToString();
                        Table[J, 11] = Arow["OnLegMuReduceCoeff"].ToString();
                    }
                    else
                    {
                        Table[J, 2] = Arow["Su"].ToString();
                        Table[J, 3] = Arow["UnderWaterWeight"].ToString();
                        Table[J, 4] = Arow["UnderWaterPhi"].ToString();
                        Table[J, 5] = Arow["E"].ToString();
                        Table[J, 6] = Arow["mu"].ToString();
                        Table[J, 7] = Arow["OnLegWeightReduceCoeff"].ToString();
                        Table[J, 8] = Arow["OnLegStrenthengReduceCoeff"].ToString();
                        Table[J, 9] = Arow["OnLegEReduceCoeff"].ToString();
                        Table[J, 10] = Arow["OnLegMuReduceCoeff"].ToString();
                    }
                }

                inset_a_table(ref SETTING_DOCUMENT, Table, NColumn, mydataset.Tables["LS_Soil"].Rows.Count + 1, TableWidths);
                SETTING_DOCUMENT.WriteLine("\r"); //回车

                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left);
                SETTING_DOCUMENT.SetFont(BodyFont);
                if (SuInputType == 1)
                {
                    SETTING_DOCUMENT.WriteLine("注：Su0为该土层顶部不排水抗剪强度，Su从Su0开始按强度增长系数随高程线性变化。");
                }
                else
                {
                    SETTING_DOCUMENT.WriteLine("注：Su随高程线性变化。");
                }

                SectionNumber += 1;
                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left);
                SETTING_DOCUMENT.SetFont(Heading2Font);
                SETTING_DOCUMENT.WriteLine('\r' + ChapterNumber + "." + SectionNumber + " 地层");
                SETTING_DOCUMENT.SetFont(TableFont);
                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center);

                Irow = mydataset.Tables["LS_Common"].Rows[0];
                bool UseSingleDrilling = (bool)Irow["UseSingleDrilling"];

                if (UseSingleDrilling)
                {
                    TabColShowNames = new string[] { "土层名称", "层顶高程(m)" };
                    TabColNames = new string[] { "SoilID", "TopLevel" };
                    Rows = mydataset.Tables["LS_LegSoilLayer"].Select("");
                }
                else
                {
                    TabColShowNames = new string[] { "编号", "钻孔名称", "钻孔x(m)", "钻孔y(m)", "地层" };
                    TabColNames = new string[] { "ID", "Name", "x", "y", "SoilLayers" };
                    Rows = mydataset.Tables["LS_SoilDrilling"].Select("");
                }

                TabColNumber = TabColShowNames.Length;
                Table = new string[Rows.Length + 1, TabColNumber];
                TableWidths = new long[TabColNumber];

                for (int i = 0; i <= TabColNumber - 1; i++)
                {
                    Table[0, i] = TabColShowNames[i];
                    TableWidths[i] = 1000;
                }

                if (UseSingleDrilling)
                {
                    TableWidths[0] = 2000;
                }
                else
                {
                    TableWidths[0] = 600;
                    TableWidths[TabColNumber - 1] = 4000;
                }

                J = 0;
                foreach (DataRow Arow in Rows)
                {
                    J++;
                    for (int i = 0; i <= TabColNumber - 1; i++)
                    {
                        Table[J, i] = Arow[TabColNames[i]].ToString();
                        if (TabColNames[i] == "SoilID")
                        {
                            int sid = (int)Arow["SoilID"];
                            Table[J, i] = SoilNames[sid];
                        }
                    }
                }

                inset_a_table(ref SETTING_DOCUMENT, Table, TabColNumber, J + 1, TableWidths);
                SETTING_DOCUMENT.WriteLine("\r");
                SETTING_DOCUMENT.NewPage();
            }
            catch
            {
                //等价 On Error Resume Next
            }
        }

        /// <summary>
        /// 插入Word表格，超宽自动拆分续表，支持指定列纵向合并单元格
        /// </summary>
        /// <param name="RTB">EsWord文档对象</param>
        /// <param name="table">二维字符串表格数据 [行,列]</param>
        /// <param name="cols">总列数</param>
        /// <param name="rows">总行数</param>
        /// <param name="TableWidths">每列宽度数组</param>
        /// <param name="NMergeColumn">需要合并的列数量，默认0不合并</param>
        /// <param name="MergeColumnIndex">需要纵向合并的列下标数组</param>
        private void inset_a_table(ref EsWordDocument RTB, string[,] table, int cols, int rows, long[] TableWidths, int NMergeColumn = 0, int[] MergeColumnIndex = null)
            {
                Font regular = new Font("Helvetica", 10, FontStyle.Regular);
                EsWordTable rt;

                int I, J, Index, StartIndex, EndIndex;
                int W = 0, Irow;
                int m = 0, StartRow, RowIndex;

                // 合并标记数组
                bool[] ColumnIsMerge = new bool[cols];
                int[] NMergeTable = new int[cols];
                int[,,] MergeTable = new int[rows, cols, 2];

                //初始化合并标记
                for (I = 0; I <= cols - 1; I++)
                {
                    ColumnIsMerge[I] = false;
                    NMergeTable[I] = 0;
                }

                if (NMergeColumn != 0 && MergeColumnIndex != null)
                {
                    for (I = 0; I <= NMergeColumn - 1; I++)
                    {
                        int colIdx = MergeColumnIndex[I];
                        ColumnIsMerge[colIdx] = true;
                        StartRow = 0;
                        RowIndex = 0;

                        for (J = 0; J <= rows - 1; J++)
                        {
                            if (table[J, colIdx] != table[StartRow, colIdx])
                            {
                                MergeTable[RowIndex, colIdx, 0] = StartRow;
                                MergeTable[RowIndex, colIdx, 1] = J - StartRow;
                                RowIndex++;
                                StartRow = J;
                            }
                        }
                        MergeTable[RowIndex, colIdx, 0] = StartRow;
                        MergeTable[RowIndex, colIdx, 1] = rows - StartRow;
                        NMergeTable[colIdx] = RowIndex;
                    }
                }

                //计算表格总宽度
                W = 0;
                for (J = 0; J <= cols - 1; J++)
                {
                    W += (int)TableWidths[J];
                }

                //总宽度大于A4限制9000，循环拆分表格输出续表
                if (W > 9000)
                {
                    StartIndex = 0;
                    EndIndex = 0;
                    Index = 0;
                    while (EndIndex != cols - 1)
                    {
                        W = 0;
                        EndIndex = 0;
                        for (J = StartIndex; J <= cols - 1; J++)
                        {
                            W += (int)TableWidths[J];
                            if (W > 9000)
                            {
                                EndIndex = J - 1;
                                break;
                            }
                        }
                        if (EndIndex == 0)
                        {
                            EndIndex = cols - 1;
                        }

                        //新建局部表格
                        rt = RTB.NewTable(regular, Color.Black, rows, EndIndex - StartIndex + 1, 0);
                        rt.Alignment = EsWordTextAlign.Center;
                        rt.SetBorders(Color.Black, 2, true, true, true, true);

                        //设置本块每列宽度
                        for (J = StartIndex; J <= EndIndex; J++)
                        {
                            rt.Columns[J - StartIndex].SetWidth(Convert.ToInt32(TableWidths[J]));
                        }

                        //填充单元格内容
                        for (J = StartIndex; J <= EndIndex; J++)
                        {
                            if (NMergeColumn == 0 || ColumnIsMerge[J] == false)
                            {
                                for (I = 0; I <= rows - 1; I++)
                                {
                                    rt.Rows[I][J - StartIndex].Write(table[I, J]);
                                }
                            }
                            else
                            {
                                for (I = 0; I <= NMergeTable[J]; I++)
                                {
                                    Irow = MergeTable[I, J, 0];
                                    rt.Rows[Irow][J - StartIndex].Write(table[Irow, J]);
                                    rt.Rows[Irow][J - StartIndex].RowSpan = MergeTable[I, J, 1];
                                    rt.SetBorders(Color.Black, 2, true, true, true, true);
                                }
                            }
                        }

                        //输出续表标题
                        if (Index > 0)
                        {
                            Font bold1 = new Font("Tahoma", 10, FontStyle.Bold);
                            RTB.SetFont(bold1);
                            RTB.WriteLine("续表：");
                        }

                        rt.SaveToDocument(W, 0);
                        StartIndex = EndIndex + 1;
                        Index++;
                    }
                }
                else
                {
                    //表格宽度正常，直接输出整张表
                    rt = RTB.NewTable(regular, Color.Black, rows, cols, 0);
                    rt.Alignment = EsWordTextAlign.Center;
                    rt.SetBorders(Color.Black, 2, true, true, true, true);

                    for (J = 0; J <= cols - 1; J++)
                    {
                        rt.Columns[J].SetWidth(Convert.ToInt32(TableWidths[J]));
                        if (NMergeColumn == 0 || ColumnIsMerge[J] == false)
                        {
                            for (I = 0; I <= rows - 1; I++)
                            {
                                rt.Rows[I][J].Write(table[I, J]);
                            }
                        }
                        else
                        {
                            for (I = 0; I <= NMergeTable[J]; I++)
                            {
                                Irow = MergeTable[I, J, 0];
                                rt.Rows[Irow][J].Write(table[Irow, J]);
                                rt.Rows[Irow][J].RowSpan = MergeTable[I, J, 1];
                                rt.SetBorders(Color.Black, 2, true, true, true, true);
                            }
                        }
                    }
                    rt.SaveToDocument(W, 0);
                }
            }

        private string GetChineseNumber()
        {
            string[] ChineseNumber = { "一", "二", "三", "四", "五", "六", "七", "八", "九", "十" };
            int idx = ChapterNumber - 1;
            if (idx < 0 || idx >= ChineseNumber.Length)
                return ChapterNumber.ToString();
            return ChineseNumber[idx];
        }

        private void Setting_Text_CalculationParameter()
        {
            try
            {
                ChapterNumber += 1;
                SectionNumber = 0;
                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center);
                SETTING_DOCUMENT.SetFont(Heading1Font);
                SETTING_DOCUMENT.WriteLine("第" + GetChineseNumber() + "章 计算参数");

                int J;
                DataRow Irow;
                string TabName;
                int TabColNumber;
                string[] TabColNames;
                string[] TabColShowNames;

                SectionNumber += 1;
                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left);
                SETTING_DOCUMENT.SetFont(Heading2Font);
                SETTING_DOCUMENT.WriteLine("\r" + ChapterNumber + "." + SectionNumber + " 计算参数");
                SETTING_DOCUMENT.SetFont(TableFont);

                Irow = mydataset.Tables["LS_CalculationParameter"].Rows[0];

                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left);

                string calcMethod = ((int)Irow["CalculationMethod"] == 1)
                    ? "BS EN ISO 19905‑1:2016"
                    : "弹性塑性有限元法";

                SETTING_DOCUMENT.WriteLine("    计算方法：" + calcMethod + ";");
                SETTING_DOCUMENT.WriteLine("    计算桩靴底部高程(m)=" + Irow["DestinationLevel"] + ";");
                SETTING_DOCUMENT.WriteLine("    计算高程点数量=" + Irow["NCalculatePoint"] + ";");
                SETTING_DOCUMENT.WriteLine("    单腿预压力(t)=" + Irow["PressForce"] + ";");

                if ((int)Irow["CalculationMethod"] == 2)
                {
                    SETTING_DOCUMENT.WriteLine("    计算单元尺寸(m)=" + Irow["MeshSize"] + ";");
                    SETTING_DOCUMENT.WriteLine("    计算屈服准则：DP" + Irow["DPType"] + ";");
                }
                else
                {
                    SETTING_DOCUMENT.WriteLine("    考虑土体回流：" + ((bool)Irow["IsBackFlow"] ? "是" : "否") + ";");
                    SETTING_DOCUMENT.WriteLine("    自动计算极限孔洞深度Hc：" + ((bool)Irow["AutoGetHc"] ? "是" : "否") + ";");

                    if (!(bool)Irow["AutoGetHc"])
                    {
                        SETTING_DOCUMENT.WriteLine("    Hc(m)=" + Irow["Hc"] + ";");
                    }
                    SETTING_DOCUMENT.WriteLine("    突破系数Nbreakout=" + Irow["NBreakout"] + ";");
                    SETTING_DOCUMENT.WriteLine("    桩土间粗糙度α=" + Irow["SoilCoarseCoeff"] + ";");
                    SETTING_DOCUMENT.WriteLine("    土体强度折减系数ftop=" + Irow["ftop"] + "，桩靴上部土体因扰动产生的强度降低，与工作时间相关;");
                    SETTING_DOCUMENT.WriteLine("    强度增长系数fbase=" + Irow["fbase"] + ",桩靴下部土体在荷载作用下再固结而产生强度增加，与工作时间相关。");
                }

                SETTING_DOCUMENT.NewPage();
            }
            catch
            {
                //等价 On Error Resume Next
            }
        }

        private void Setting_Text_Result()
        {
            try
            {
                ChapterNumber += 1;
                SectionNumber = 0;

                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center);
                SETTING_DOCUMENT.SetFont(Heading1Font);
                SETTING_DOCUMENT.WriteLine($"第{GetChineseNumber()}章 承载力曲线结果");

                int J = 0;
                DataRow Irow = mydataset.Tables["LS_CalculationParameter"].Rows[0];
                int CalculationMethod = (int)Irow["CalculationMethod"];

                Irow = mydataset.Tables["LS_Common"].Rows[0];
                bool UseSingleDrilling = (bool)Irow["UseSingleDrilling"];

                List<int> DrillingID = new List<int>();
                if (UseSingleDrilling)
                {
                    DrillingID.Add(1);
                }
                else
                {
                    foreach (DataRow Arow in mydataset.Tables["LS_SoilDrilling"].Select("", "ID"))
                    {
                        int idVal = (int)Arow["ID"];
                        if (!DrillingID.Contains(idVal))
                        {
                            DrillingID.Add(idVal);
                        }
                    }
                }

                //===== 计算结果简表 =====
                double LimitValue1 = (double)mydataset.Tables["LS_CalculationParameter"].Rows[0]["GroundPressure"];
                SectionNumber += 1;
                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left);
                SETTING_DOCUMENT.SetFont(Heading2Font);
                SETTING_DOCUMENT.WriteLine("\r" + ChapterNumber + "." + SectionNumber + " 计算结果简表");
                SETTING_DOCUMENT.SetFont(TableFont);

                string[] TabColShowNames = { "钻孔号", "测试力(kN)", "建议插深(m)", "持力层", "持力层土强度参数", "抗拔力(kN)", "抗压力(kN)", "抗压承载力计算模式" };
                string[] TabColNames = { "DrillingID", "LimitForce", "SuggestedDepth", "SupportSoilID", "SupportSoilStrength", "Qu", "Qv", "SelectMode_Qv" };

                int TabColNumber = UseSingleDrilling ? (TabColShowNames.Length - 1) : TabColShowNames.Length;
                TabColNumber = CalculationMethod == 1 ? TabColNumber : TabColNumber - 1;

                Table = new string[DrillingID.Count + 1, TabColNumber];
                TableWidths = new long[TabColNumber];

                for (int i = 0; i < TabColNumber; i++)
                {
                    Table[0, i] = UseSingleDrilling ? TabColShowNames[i + 1] : TabColShowNames[i];
                    TableWidths[i] = 1000;
                }

                if (UseSingleDrilling)
                {
                    TableWidths[2] = 1300;
                }
                else
                {
                    TableWidths[0] = 600;
                    TableWidths[3] = 1300;
                }
                if (CalculationMethod == 1)
                {
                    TableWidths[TabColNumber - 1] = 1300;
                }

                Irow = mydataset.Tables["LS_StructureData"].Rows[0];
                double AirGap = (double)Irow["AirGap"];
                double WindFieldWaterHeight = (double)Irow["WindFieldWaterHeight"];

                Irow = mydataset.Tables["LS_Leg"].Rows[0];
                double LegActiveLength = (double)Irow["ActiveLength"];

                string DepthOKString = "";
                bool IsDepthOK = true;

                J = 0;
                DataRow[] depthRows = mydataset.Tables["LS_DepthResult"].Select("IsUserAdd=False", "DrillingID ASC");
                foreach (DataRow Arow in depthRows)
                {
                    J++;
                    for (int i = 0; i < TabColNumber; i++)
                    {
                        string TabCName = UseSingleDrilling ? TabColNames[i + 1] : TabColNames[i];
                        Table[J, i] = Arow[TabCName]?.ToString();

                        if (TabCName == "SupportSoilStrength")
                        {
                            DataRow[] soilSel = mydataset.Tables["LS_Soil"].Select("ID=" + Arow["SupportSoilID"]);
                            string suffix = soilSel != null && soilSel.Length > 0 && (int)soilSel[0]["Type"] == 0 ? "kPa" : "°";
                            Table[J, i] = Arow[TabCName] + suffix;
                        }

                            if (TabCName == "SupportSoilID")
                            {
                                // 读取当前单元格的值（就是ID）
                                string cellVal = Table[J, i];
                                if (int.TryParse(cellVal, out int soilIdx))
                                {
                                    // 判断字典是否包含这个key，不是比较Count！
                                    if (SoilNames.ContainsKey(soilIdx))
                                    {
                                        // 翻译：用ID取土壤名称，写回单元格
                                        Table[J, i] = SoilNames[soilIdx];
                                    }
                                    else
                                    {
                                        // key不存在，保留原来ID文本
                                        Table[J, i] = cellVal;
                                    }
                                }
                                else
                                {
                                    // 不能转数字，直接保留原值
                                    Table[J, i] = cellVal;
                                }
                            }

                            if (TabCName == "SelectMode_Qv")
                        {
                            DataRow[] modeRow = mydataset.Tables["LS_ComputingModelType_Qv"].Select("ID=" + Arow[TabCName]);
                            if (modeRow != null && modeRow.Length > 0)
                            {
                                Table[J, i] = modeRow[0]["Name"]?.ToString();
                            }
                        }
                    }

                    string prefix = UseSingleDrilling ? "    " : $"    钻孔#{Arow["DrillingID"]}";
                    double sugDepth = Convert.ToDouble(Arow["SuggestedDepth"]);
                    string cmpText = sugDepth + AirGap + WindFieldWaterHeight < LegActiveLength ? "小于" : "大于或等于";
                    DepthOKString += $"{prefix}建议插深结果为{sugDepth}m,插深、风场区域水深、气隙之和{cmpText}桩腿有效长度({LegActiveLength}m)。{Environment.NewLine}";
                }

                inset_a_table(ref SETTING_DOCUMENT, Table, TabColNumber, J + 1, TableWidths);
                if (depthRows.Length > 0)
                {
                    SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left);
                    SETTING_DOCUMENT.WriteLine(DepthOKString + "    注：默认计算模式抗压承载力取常规破坏模式、挤出模式和穿刺模式三种模式下的最小结果，其中常规破坏模式和穿刺破坏模式按砂土和黏土进行计算。");
                }
                SETTING_DOCUMENT.WriteLine("\r");

                //===== 极限孔洞深度结果 =====
                if (CalculationMethod == 1)
                {
                    SectionNumber += 1;
                    SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left);
                    SETTING_DOCUMENT.SetFont(Heading2Font);
                    SETTING_DOCUMENT.WriteLine("\r" + ChapterNumber + "." + SectionNumber + " 极限孔洞深度结果");
                    SETTING_DOCUMENT.SetFont(TableFont);

                    TabColShowNames = new string[] { "钻孔号", "极限孔洞深度Hc(m)" };
                    TabColNames = new string[] { "DrillingID", "Hc" };
                    TabColNumber = UseSingleDrilling ? (TabColShowNames.Length - 1) : TabColShowNames.Length;

                    Table = new string[DrillingID.Count + 1, TabColNumber];
                    TableWidths = new long[TabColNumber];

                    for (int i = 0; i < TabColNumber; i++)
                    {
                        Table[0, i] = UseSingleDrilling ? TabColShowNames[i + 1] : TabColShowNames[i];
                        TableWidths[i] = 2000;
                    }
                    if (UseSingleDrilling)
                        TableWidths[0] = 1500;
                    else
                        TableWidths[0] = 1000;

                    J = 0;
                    foreach (DataRow Arow in mydataset.Tables["LS_Holl"].Select("", "DrillingID"))
                    {
                        J++;
                        for (int i = 0; i < TabColNumber; i++)
                        {
                            string col = UseSingleDrilling ? TabColNames[i + 1] : TabColNames[i];
                            Table[J, i] = Arow[col]?.ToString();
                        }
                    }
                    inset_a_table(ref SETTING_DOCUMENT, Table, TabColNumber, J + 1, TableWidths);
                    SETTING_DOCUMENT.WriteLine("\r");
                }

                //===== 地基承载力结果 =====
                SectionNumber += 1;
                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left);
                SETTING_DOCUMENT.SetFont(Heading2Font);
                SETTING_DOCUMENT.WriteLine("\r" + ChapterNumber + "." + SectionNumber + " 地基承载力结果");
                SETTING_DOCUMENT.SetFont(TableFont);

                if (CalculationMethod == 1)
                {
                    SETTING_DOCUMENT.WriteLine("    对于单一黏土层，若不排水抗剪强度不变或变化较小，常规破坏模式的地基极限竖向承载力按下式计算：");
                    SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center);
                    SETTING_DOCUMENT.WriteLine("Q{\\sub V}=(S{\\sub u}N{\\sub c}s{\\sub c}d{\\sub c}+p'{\\sub 0})A");
                    SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left);
                    SETTING_DOCUMENT.WriteLine("    对于单一均质砂土层，常规破坏模式的极限竖向承载力按下式计算：");
                    SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center);
                    SETTING_DOCUMENT.WriteLine("Q{\\sub V}=(γ'd{\\sub γ}N{\\sub γ}B/2+p'{\\sub 0}d{\\sub q}N{\\sub q})A");
                    SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left);
                    SETTING_DOCUMENT.WriteLine("    软黏土层厚度较小且下方存在硬土层时，应考虑挤出破坏，极限竖向承载力按下式计算：");
                    SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center);
                    SETTING_DOCUMENT.WriteLine("Q{\\sub V}=A{(α{\\sub s}+b{\\sub s}B/T+1.2D/B)S{\\sub u}+p'{\\sub 0}}");
                    SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left);
                    SETTING_DOCUMENT.WriteLine("    当硬黏土层覆盖在软黏土层上时，应考虑穿刺破坏，极限竖向承载力按下式进行验算：");
                    SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center);
                    SETTING_DOCUMENT.WriteLine("Q{\\sub V}=A[3H/BS{\\sub u,t}+N{\\sub c}s{\\sub c}(1+0.2(D+H)/B)S{\\sub u,b}+p'{\\sub 0})]");
                    SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left);
                    SETTING_DOCUMENT.WriteLine("    当砂土层覆盖在软黏土层上时，应考虑穿刺破坏，极限竖向承载力可按下列公式进行计算：");
                    SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center);
                    SETTING_DOCUMENT.WriteLine("Q{\\sub V}=Q{\\sub V,b}-AHγ'+2AH(Hγ'+2p'{\\sub 0})K{\\sub S}tan(φ'/B)");
                    SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left);
                    SETTING_DOCUMENT.WriteLine("    考虑分层土承载力计算模式，极限竖向承载力可按下列公式进行计算：");
                    SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center);
                    SETTING_DOCUMENT.WriteLine("Q{\\sub V}=(0.5γ'BN{\\sub γ}s{\\sub γ}i{\\sub γ}+p'{\\sub 0}N{\\sub q}s{\\sub q}i{\\sub q}+s{\\sub u}N{\\sub c}s{\\sub c}i{\\sub c})A");
                }
                else
                {
                    SETTING_DOCUMENT.WriteLine("    注：极限抗压承载力和抗压承载力按照塑性有限元法计算，不断增加压力或拔力，当计算不稳定时即为临界荷载。");
                }
                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center);

                if (CalculationMethod == 1)
                {
                    TabColShowNames = new string[] { "钻孔号", "序号", "桩靴底部高程(m)", "进入土层", "是否进入砂土", "砂土常规破坏模式Qv(kN)", "黏土常规破坏模式Qv(kN)", "挤出破坏模式Qv(kN))", "砂土穿刺破坏模式Qv(kN)", "黏土穿刺破坏模式Qv(kN)", "分层土破坏模式Qv(kN)", "选择计算模式" };
                    TabColNames = new string[] { "DrillingID", "ID", "Level", "SoilID", "IsSand", "QV1_Sand", "QV1_Clay", "QV2", "QV3_Sand", "QV3_Clay", "QV4", "SelectMode" };
                    TabColNumber = UseSingleDrilling ? (TabColShowNames.Length - 1) : TabColShowNames.Length;

                    Table = new string[mydataset.Tables["LS_PressResistanceResult"].Rows.Count + 1, TabColNumber];
                    TableWidths = new long[TabColNumber];

                    for (int i = 0; i < TabColNumber; i++)
                    {
                        Table[0, i] = UseSingleDrilling ? TabColShowNames[i + 1] : TabColShowNames[i];
                        TableWidths[i] = 800;
                    }
                    TableWidths[0] = 600;
                    if (UseSingleDrilling)
                    {
                        TableWidths[1] = 800;
                        TableWidths[3] = 600;
                    }
                    else
                    {
                        TableWidths[1] = 600;
                        TableWidths[2] = 800;
                        TableWidths[3] = 800;
                        TableWidths[4] = 600;
                    }

                    J = 0;
                    foreach (DataRow Arow in mydataset.Tables["LS_PressResistanceResult"].Rows)
                    {
                        J++;
                        for (int i = 0; i < TabColNumber; i++)
                        {
                            string ColName = UseSingleDrilling ? TabColNames[i + 1] : TabColNames[i];
                            switch (ColName)
                            {
                                case "SoilID":
                                    object sidObj = Arow[ColName];
                                    if (int.TryParse(sidObj?.ToString(), out int sIdx) && sIdx >= 0 && SoilNames.ContainsKey(sIdx))
                                    {
                                        Table[J, i] = SoilNames[sIdx];
                                    }
                                    else
                                    {
                                        Table[J, i] = sidObj?.ToString();
                                    }
                                    break;
                                case "IsSand":
                                    Table[J, i] = (bool)Arow[ColName] ? "是" : "否";
                                    break;
                                case "SelectMode":
                                    DataRow[] smQv = mydataset.Tables["LS_ComputingModelType_Qv"].Select("ID=" + Arow[ColName]);
                                    Table[J, i] = smQv != null && smQv.Length > 0 ? smQv[0]["Name"]?.ToString() : "";
                                    break;
                                default:
                                    Table[J, i] = Arow[ColName]?.ToString();
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    TabColShowNames = new string[] { "钻孔号", "序号", "桩靴底部高程(m)", "地基承载力Qv(kPa)", "地基承载力Qv(kN)" };
                    TabColNames = new string[] { "DrillingID", "ID", "Level", "QVp", "QV" };
                    TabColNumber = UseSingleDrilling ? (TabColShowNames.Length - 1) : TabColShowNames.Length;

                    Table = new string[mydataset.Tables["LS_PressResistanceResult"].Rows.Count + 1, TabColNumber];
                    TableWidths = new long[TabColNumber];

                    for (int i = 0; i < TabColNumber; i++)
                    {
                        Table[0, i] = UseSingleDrilling ? TabColShowNames[i + 1] : TabColShowNames[i];
                        TableWidths[i] = 1200;
                    }
                    TableWidths[0] = 600;
                    if (!UseSingleDrilling) TableWidths[1] = 600;

                    J = 0;
                    foreach (DataRow Arow in mydataset.Tables["LS_PressResistanceResult"].Rows)
                    {
                        J++;
                        for (int i = 0; i < TabColNumber; i++)
                        {
                            string ColName = UseSingleDrilling ? TabColNames[i + 1] : TabColNames[i];
                            Table[J, i] = Arow[ColName]?.ToString();
                        }
                    }
                }
                inset_a_table(ref SETTING_DOCUMENT, Table, TabColNumber, J + 1, TableWidths);
                SETTING_DOCUMENT.WriteLine("\r");

                if (J > 0)
                {
                    foreach (int DID in DrillingID)
                    {
                        string DName = "";
                        if (!UseSingleDrilling)
                        {
                            DataRow[] dr = mydataset.Tables["LS_SoilDrilling"].Select("ID=" + DID);
                            if (dr != null && dr.Length > 0)
                                DName = dr[0]["Name"] + "-";
                        }
                        EsPLCurveTable CurveTable = new EsPLCurveTable();
                        CurveTable.Curves.Clear();
                        SETTING_DOCUMENT.PutImage(SpudcanDB.DrawPressCurve(mydataset, CurveTable, LimitValue1, 1000, 500, 2, 1, DID), 600, 300);
                        SETTING_DOCUMENT.WriteLine("\r");
                        SETTING_DOCUMENT.WriteLine(DName + "地基承载力曲线");
                        SETTING_DOCUMENT.WriteLine("\r");
                    }
                }

                //===== 拔桩力结果 =====
                SectionNumber += 1;
                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left);
                SETTING_DOCUMENT.SetFont(Heading2Font);
                SETTING_DOCUMENT.WriteLine("\r" + ChapterNumber + "." + SectionNumber + " 拔桩力结果");
                SETTING_DOCUMENT.SetFont(TableFont);

                if (CalculationMethod == 1)
                {
                    SETTING_DOCUMENT.WriteLine("    粘性土中，浅埋状态拔桩力可按下式计算：");
                    SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center);
                    SETTING_DOCUMENT.WriteLine("Q{\\sub breakout}=W+C(H{\\sub column}S{\\sub u}f{\\sub top}+αH{\\sub t}S{\\sub u}f{\\sub base})+A(N{\\sub breakout}S{\\sub u}f{\\sub base}+H{\\sub column}γ')-V{\\sub top}γ'");
                    SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left);
                    SETTING_DOCUMENT.WriteLine("    粘性土中，深埋状态时拔桩力可按下式计算：");
                    SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center);
                    SETTING_DOCUMENT.WriteLine("Q{\\sub breakout}=W+A(N{\\sub breakout}S{\\sub u}f{\\sub base}+H{\\sub column}γ')+A'S{\\sub u}-V{\\sub top}γ'");
                    SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left);
                    SETTING_DOCUMENT.WriteLine("    砂性土中，浅埋状态拔桩力可按下式计算：");
                    SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center);
                    SETTING_DOCUMENT.WriteLine("Q{\\sub u}=2cD(B+L)+γD{\\super 2}(2sB+L-B)K{\\sub u}tanφ+W");
                    SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left);
                    SETTING_DOCUMENT.WriteLine("    砂性土中，深埋状态时拔桩力可按下式计算：");
                    SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center);
                    SETTING_DOCUMENT.WriteLine("Q{\\sub u}=2cD(B+L)+γ(2D‑H)H(2sB+L‑B)K{\\sub u}tanφ+W");
                    SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left);
                    SETTING_DOCUMENT.WriteLine("\r");
                    SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center);

                    TabColShowNames = new string[] { "钻孔号", "序号", "桩靴底部高程(m)", "进入土层", "砂土拔桩力Qu(kN)", "砂土插深类型", "黏土拔桩力Qu(kN)", "黏土插深类型", "选择计算模式" };
                    TabColNames = new string[] { "DrillingID", "ID", "Level", "SoilID", "Qu_Sand", "DeepType_Sand", "Qu_Clay", "DeepType_Clay", "SelectMode" };
                    TabColNumber = UseSingleDrilling ? (TabColShowNames.Length - 1) : TabColShowNames.Length;

                    Table = new string[mydataset.Tables["LS_PullResistanceResult"].Rows.Count + 1, TabColNumber];
                    TableWidths = new long[TabColNumber];

                    for (int i = 0; i < TabColNumber; i++)
                    {
                        Table[0, i] = UseSingleDrilling ? TabColShowNames[i + 1] : TabColShowNames[i];
                        TableWidths[i] = 1000;
                    }
                    TableWidths[0] = 600;

                    J = 0;
                    foreach (DataRow Arow in mydataset.Tables["LS_PullResistanceResult"].Rows)
                    {
                        J++;
                        for (int i = 0; i < TabColNumber; i++)
                        {
                            string ColName = UseSingleDrilling ? TabColNames[i + 1] : TabColNames[i];
                            switch (ColName)
                            {
                                case "SoilID":
                                    object sidObj = Arow[ColName];
                                    if (int.TryParse(sidObj?.ToString(), out int sIdx) && sIdx >= 0 && SoilNames.ContainsKey(sIdx))
                                    {
                                        Table[J, i] = SoilNames[sIdx];
                                    }
                                    else
                                    {
                                        Table[J, i] = sidObj?.ToString();
                                    }
                                    break;
                                case "DeepType_Sand":
                                case "DeepType_Clay":
                                case "DeepType":
                                    DataRow[] dtRow = mydataset.Tables["LS_DeepType"].Select("ID=" + Arow[ColName]);
                                    Table[J, i] = dtRow != null && dtRow.Length > 0 ? dtRow[0]["Name"]?.ToString() : "";
                                    break;
                                case "SelectMode":
                                    DataRow[] smQb = mydataset.Tables["LS_ComputingModelType_Qb"].Select("ID=" + Arow[ColName]);
                                    Table[J, i] = smQb != null && smQb.Length > 0 ? smQb[0]["Name"]?.ToString() : "";
                                    break;
                                default:
                                    Table[J, i] = Arow[ColName]?.ToString();
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center);
                    TabColShowNames = new string[] { "钻孔号", "序号", "桩靴底部高程(m)", "拔桩力Qu(t)", "拔桩力Qu(kN)" };
                    TabColNames = new string[] { "DrillingID", "ID", "Level", "QuP", "Qu" };
                    TabColNumber = UseSingleDrilling ? (TabColShowNames.Length - 1) : TabColShowNames.Length;

                    Table = new string[mydataset.Tables["LS_PullResistanceResult"].Rows.Count + 1, TabColNumber];
                    TableWidths = new long[TabColNumber];

                    for (int i = 0; i < TabColNumber; i++)
                    {
                        Table[0, i] = UseSingleDrilling ? TabColShowNames[i + 1] : TabColShowNames[i];
                        TableWidths[i] = 1200;
                    }
                    TableWidths[0] = 600;
                    if (!UseSingleDrilling) TableWidths[1] = 600;

                    J = 0;
                    foreach (DataRow Arow in mydataset.Tables["LS_PullResistanceResult"].Rows)
                    {
                        J++;
                        for (int i = 0; i < TabColNumber; i++)
                        {
                            string ColName = UseSingleDrilling ? TabColNames[i + 1] : TabColNames[i];
                            Table[J, i] = Arow[ColName]?.ToString();
                        }
                    }
                }
                inset_a_table(ref SETTING_DOCUMENT, Table, TabColNumber, J + 1, TableWidths);

                if (J > 0)
                {
                    foreach (int DID in DrillingID)
                    {
                        string DName = "";
                        if (!UseSingleDrilling)
                        {
                            DataRow[] dr = mydataset.Tables["LS_SoilDrilling"].Select("ID=" + DID);
                            if (dr != null && dr.Length > 0)
                                DName = dr[0]["Name"].ToString();
                        }
                        EsPLCurveTable CurveTable = new EsPLCurveTable();
                        CurveTable.Curves.Clear();
                        SETTING_DOCUMENT.PutImage(SpudcanDB.DrawPullCurve(mydataset, CurveTable, 1000, 500, 2, 1, DID), 600, 300);

                        SETTING_DOCUMENT.WriteLine("\r");
                        SETTING_DOCUMENT.WriteLine(DName + "拔桩力曲线");
                        SETTING_DOCUMENT.WriteLine("\r");
                    }
                }
            }
            catch
            {
                // 等价原VB On Error Resume Next，发生异常直接跳过
            }
        }

        /// <summary>
        /// 设置Word模板页眉页脚
        /// </summary>
        /// <param name="TaskName">页眉显示文本</param>
        public void Setting_Template_HeaderAndFooter(string TaskName)
        {
            try
            {
                //页眉
                SETTING_DOCUMENT.HeaderStart();
                SETTING_DOCUMENT.SetFont(new Font(HeaderFont.FontFamily, HeaderFont.Size, FontStyle.Regular));
                SETTING_DOCUMENT.SetForegroundColor(HeaderColor);
                SETTING_DOCUMENT.SetTextAlign(HeaderAlign);
                SETTING_DOCUMENT.Write(TaskName);
                SETTING_DOCUMENT.HeaderEnd();

                //页脚：输出当前时间 + 页码
                SETTING_DOCUMENT.FooterStart();
                SETTING_DOCUMENT.SetFont(new Font(FooterFont.FontFamily, FooterFont.Size));
                SETTING_DOCUMENT.SetForegroundColor(FooterColor);
                SETTING_DOCUMENT.SetTextAlign(FooterAlign);
                SETTING_DOCUMENT.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                SETTING_DOCUMENT.SetPageNumbering(1);
                SETTING_DOCUMENT.FooterEnd();
            }
            catch
            {
                //等价VB On Error Resume Next，出错忽略
            }
        }

        public void Setting_Template(string ProjectName, int DrillingID, bool UseMetaFile = true)
        {
            try
            {
                string[] TabColNames;
                string[] TabColShowNames;
                int N;
                int J;

                ChapterNumber += 1;
                SectionNumber = 0;

                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center);
                SETTING_DOCUMENT.SetFont(Heading1Font);
                SETTING_DOCUMENT.WriteLine("计算报告");

                SectionNumber += 1;
                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left);
                SETTING_DOCUMENT.SetFont(Heading2Font);
                SETTING_DOCUMENT.WriteLine($"{SectionNumber}.计算参数");
                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified);
                SETTING_DOCUMENT.SetFont(BodyFont);
                SETTING_DOCUMENT.WriteLine($"项目名称：{ProjectName}");

                string DrillingName = "";
                DataRow[] Rows;

                if (DrillingID != 0)
                {
                    DataRow drTemp = mydataset.Tables["LS_TempSoilDrilling"].Select($"DrillingID={DrillingID}", "DrillingID")[0];
                    DrillingName = drTemp["DrillingName"].ToString();
                    SETTING_DOCUMENT.WriteLine($"计算钻孔：{DrillingName}");
                }

                foreach (DataRow Row in mydataset.Tables["LS_Boat"].Select("IsCount=1"))
                {
                    SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center);
                    SETTING_DOCUMENT.SetFont(TableFont);
                    SETTING_DOCUMENT.WriteLine($"{Row["Name"]}船舶参数");

                    string[] ColumnNames = { "LegCircumference", "LegA", "SpudcanL", "SpudcanB", "SpudcanH", "SpudcanA", "SpudcanCircumference", "SpudcanV", "W", "LegPressForce", "SumW", "PullingCapacity", "GroundPressure", "LegHLN", "AirGap" };
                    string[] EColumnNames = { "桩腿周长", "桩腿截面积（用于计算回流土体体积）", "桩靴长度 L", "桩靴宽度 B", "桩靴高度 H", "桩靴面积 A", "桩靴最大截面周长", "桩靴体积 V", "桩腿、桩靴自重 W", "桩腿预压力", "计算预压荷载", "拔桩力", "对地比压", "有效桩腿长度（船底到靴底）", "气隙（船底到水面）" };
                    string[] Units = { "m", "m{\\super 2}", "m", "m", "m", "m{\\super 2}", "m", "m{\\super 3}", "t", "t", "t", "t", "kpa", "m", "m" };

                    N = ColumnNames.Length;
                    string[,] Table = new string[N, 3];
                    long[] TableWidths = new long[3];
                    for (int i = 0; i <= 2; i++)
                    {
                        TableWidths[i] = 2000;
                    }
                    TableWidths[0] = 4000;

                    for (int i = 0; i <= N - 1; i++)
                    {
                        Table[i, 0] = EColumnNames[i];
                        object cellVal;
                        if (ColumnNames[i] == "SpudcanH")
                        {
                            string[] spParts = Row["SpudcanParameter"].ToString().Split('=');
                            cellVal = spParts[spParts.Length - 1];
                        }
                        else
                        {
                            cellVal = Row[ColumnNames[i]];
                        }
                        double val = Convert.ToDouble(cellVal);
                        Table[i, 1] = Math.Round(val, 2).ToString();
                        Table[i, 2] = Units[i];

                        if (Convert.ToInt32(Row["SpudcanShapeType"]) == 0)
                        {
                            if (EColumnNames[i] == "桩靴长度 L")
                            {
                                Table[i, 0] = "桩靴截面是否为圆形";
                                Table[i, 1] = "是";
                                Table[i, 2] = "";
                            }
                            if (EColumnNames[i] == "桩靴宽度 B")
                            {
                                Table[i, 0] = "桩靴直径 B";
                            }
                        }
                        if (Convert.ToInt32(Row["LegType"]) == 2)
                        {
                            if (EColumnNames[i] == "桩腿直径")
                            {
                                Table[i, 0] = "桁架式桩腿弦杆间距";
                            }
                        }
                    }
                    inset_a_table(ref SETTING_DOCUMENT, Table, 3, N, TableWidths);
                    SETTING_DOCUMENT.WriteLine("\r");
                }

                if (DrillingID != 0)
                {
                    SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center);
                    SETTING_DOCUMENT.SetFont(TableFont);
                    SETTING_DOCUMENT.WriteLine("土层参数");
                    Rows = mydataset.Tables["LS_SoilDrillingParameter"].Select($"DrillingID={DrillingID} and BoatID=1", "TopLevel DESC");

                    string[,] TableSoil = new string[Rows.Length + 1, 5];
                    TableSoil[0, 0] = "层顶高程";
                    TableSoil[0, 1] = "土层名称";
                    TableSoil[0, 2] = "浮重度（kN/m{\super 3}）";
                    TableSoil[0, 3] = "不排水抗剪强度Su（kPa）";
                    TableSoil[0, 4] = "摩擦角（°）";

                    long[] TableWidthsSoil = new long[5];
                    for (int i = 0; i <= 4; i++)
                    {
                        TableWidthsSoil[i] = 900;
                    }
                    TableWidthsSoil[1] = 1500;

                    J = 0;
                    foreach (DataRow Arow in Rows)
                    {
                        J += 1;
                        SoilNames.Add(Convert.ToInt32(Arow["ID"]), Arow["Name"].ToString());
                        TableSoil[J, 0] = Arow["TopLevel"]?.ToString();
                        TableSoil[J, 1] = Arow["Name"]?.ToString();
                        TableSoil[J, 2] = Arow["UnderWaterWeight"]?.ToString();
                        TableSoil[J, 3] = Arow["Su"]?.ToString();
                        TableSoil[J, 4] = Arow["UnderWaterPhi"]?.ToString();
                    }
                    inset_a_table(ref SETTING_DOCUMENT, TableSoil, 5, J + 1, TableWidthsSoil);
                    SETTING_DOCUMENT.WriteLine("\r");
                }

                SectionNumber += 1;
                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left);
                SETTING_DOCUMENT.SetFont(Heading2Font);
                SETTING_DOCUMENT.WriteLine($"{SectionNumber}.计算说明");
                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified);
                SETTING_DOCUMENT.SetFont(BodyFont);
                SETTING_DOCUMENT.WriteLine("（1）计算拔桩力时，底部持力层为粘土层时，考虑粘土强度恢复及固结和由于粘土渗透性差导致的吸附力；底部持力层为砂层时不考虑吸附力。\r\n（2）冲桩减阻系统完全发挥作用时的拔桩力是假定桩靴周围土体均已发生破坏，即土体的抗剪强度为0。");

                SectionNumber += 1;
                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left);
                SETTING_DOCUMENT.SetFont(Heading2Font);
                SETTING_DOCUMENT.WriteLine($"{SectionNumber}.计算结果");
                SubSectionNumber = 0;

                Dictionary<int, List<int>> DrillingDic = new Dictionary<int, List<int>>();
                Dictionary<int, Dictionary<int, List<int>>> SameDrillingDic = new Dictionary<int, Dictionary<int, List<int>>>();

                foreach (DataRow Row in mydataset.Tables["LS_Boat"].Select("IsCount=1", "ID"))
                {
                    string filterTempSoil = $"BoatID={Row["ID"]}";
                    if (DrillingID != 0)
                    {
                        filterTempSoil += $" and DrillingID={DrillingID}";
                    }
                    foreach (DataRow Irow in mydataset.Tables["LS_TempSoilDrilling"].Select(filterTempSoil))
                    {
                        int boatId = Convert.ToInt32(Row["ID"]);
                        int drillId = Convert.ToInt32(Irow["DrillingID"]);
                        if (!DrillingDic.ContainsKey(boatId))
                        {
                            DrillingDic.Add(boatId, new List<int>());
                        }
                        if (!DrillingDic[boatId].Contains(drillId))
                        {
                            DrillingDic[boatId].Add(drillId);
                        }

                        List<int> DriIDs = SpudcanDB.GetDrillingIDs(mydataset, drillId, boatId);
                        if (!SameDrillingDic.ContainsKey(boatId))
                        {
                            SameDrillingDic.Add(boatId, new Dictionary<int, List<int>>());
                        }
                        if (!SameDrillingDic[boatId].ContainsKey(drillId) && DriIDs.Count == 3)
                        {
                            SameDrillingDic[boatId].Add(drillId, DriIDs);
                        }
                    }
                }

                foreach (DataRow Row in mydataset.Tables["LS_Boat"].Select("IsCount=1"))
                {
                    SubSectionNumber += 1;
                    SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified);
                    SETTING_DOCUMENT.SetParagraph(2);
                    SETTING_DOCUMENT.SetFont(BodyFont);
                    SETTING_DOCUMENT.WriteLine($"({SubSectionNumber}){Row["Name"]}");

                    if (DrillingID != 0)
                    {
                        SETTING_DOCUMENT.WriteLine("插桩力计算结果：");
                    }

                    if (DrillingID == 0)
                    {
                        Rows = mydataset.Tables["LS_DepthResult"].Select($"IsUserAdd=0 and BoatID={Row["ID"]}");
                        TabColNames = new string[] { "DrillingID", "", "", "", "SuggestedDepth", "", "SupportSoilID", "Qv", "Qu0", "Qu1" };
                        TabColShowNames = new string[] { "机位号", "勘察孔", "平台船名", "泥面标高（m）", "插桩标高（m）", "理论计算插深（m）", "持力层", "桩靴底部地基承载力（kPa）", "冲桩系统完全发挥作用时的拔桩力（t）", "不计减阻系统的最大拔桩力（t）" };
                    }
                    else
                    {
                        Rows = mydataset.Tables["LS_PressResistanceResult"].Select($"BoatID={Row["ID"]} and DrillingID={DrillingID}");
                        TabColNames = new string[] { "DrillingID", "", "", "Level", "", "SoilID", "SelectMode", "Qv" };
                        TabColShowNames = new string[] { "机位号", "勘察孔", "泥面标高（m）", "插桩标高（m）", "插深（m）", "持力层", "计算模式", "桩靴底部地基承载力（kPa）" };
                    }

                    List<string> TabTitles = new List<string>();
                    string[] TabTitleSuffixes = { "强度小值", "强度中值", "强度大值" };
                    string tabTitleBase = $"表{SectionNumber}.{SubSectionNumber} {Row["Name"]}计算{(DrillingID == 0 ? "机位建议插桩深度与对应的拔桩力汇总" : $"{DrillingName}机位不同插桩深度与对应的承载力")}";
                    int GraphNumber = 0;
                    bool ShowOneTab = DrillingID > 0 || SameDrillingDic[Convert.ToInt32(Row["ID"])].Count == 0;

                    int loopMax = ShowOneTab ? 0 : 2;
                    for (int SufI = 0; SufI <= loopMax; SufI++)
                    {
                        if (!ShowOneTab)
                        {
                            GraphNumber += 1;
                            tabTitleBase = $"表{SectionNumber}.{SubSectionNumber}-{GraphNumber} {Row["Name"]}计算{(DrillingID == 0 ? "机位建议插桩深度与对应的拔桩力汇总" : $"{DrillingName}机位不同插桩深度与对应的承载力")}";
                        }
                        string titleItem = ShowOneTab ? tabTitleBase : $"{tabTitleBase}（{TabTitleSuffixes[SufI]}）";
                        TabTitles.Add(titleItem);
                    }

                    foreach (string ATabTitle in TabTitles)
                    {
                        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center);
                        SETTING_DOCUMENT.SetFont(TableFont);
                        SETTING_DOCUMENT.WriteLine(ATabTitle);
                        N = TabColShowNames.Length;
                        int rowCountTable = DrillingID != 0 ? Rows.Length : DrillingDic[Convert.ToInt32(Row["ID"])].Count;
                        string[,] Table = new string[rowCountTable + 1, N];
                        J = 0;
                        for (int i = 0; i <= N - 1; i++)
                        {
                            Table[J, i] = TabColShowNames[i];
                        }
                        long[] TableWidths = new long[N];
                        for (int i = 0; i <= N - 1; i++)
                        {
                            TableWidths[i] = 900;
                        }
                        TableWidths[0] = 400;
                        string NoteNoResult = "";

                        for (int di = 0; di <= DrillingDic[Convert.ToInt32(Row["ID"])].Count - 1; di++)
                        {
                            bool NoResult = true;
                            foreach (DataRow Irow in Rows)
                            {
                                int iDrillId = Convert.ToInt32(Irow["DrillingID"]);
                                if (iDrillId == DrillingDic[Convert.ToInt32(Row["ID"])][di])
                                {
                                    bool cond1 = DrillingID > 0;
                                    bool cond2 = !SameDrillingDic[Convert.ToInt32(Row["ID"])].ContainsKey(iDrillId);
                                    bool cond3 = SameDrillingDic[Convert.ToInt32(Row["ID"])].ContainsKey(iDrillId)
                                        && (TabTitles.IndexOf(ATabTitle) >= 0
                                        && iDrillId == SameDrillingDic[Convert.ToInt32(Row["ID"])][iDrillId][TabTitles.IndexOf(ATabTitle)]);

                                    if (cond1 || cond2 || cond3)
                                    {
                                        string[,] myTable = new string[100, 30];
                                        GetResult(ref J, Row, Irow, N, TabColNames, TabColShowNames, true, myTable, mydataset);
                                        NoResult = false;
                                        if (DrillingID == 0)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        inset_a_table(ref SETTING_DOCUMENT, Table, N, J + 1, TableWidths);
                        SETTING_DOCUMENT.WriteLine("\r");
                        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified);
                        SETTING_DOCUMENT.SetParagraph(2);
                        SETTING_DOCUMENT.SetFont(BodyFont);
                        if (!string.IsNullOrEmpty(NoteNoResult))
                        {
                            SETTING_DOCUMENT.WriteLine($"  注：对于机位{NoteNoResult}输入的地勘各土层不足，计算的地基承载力均小于桩靴对地压强，计算未得到上述机位的理论计算插深及持力层，同理无法输出上拔力计算结果。");
                        }

                        if (DrillingID != 0)
                        {
                            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center);
                            SETTING_DOCUMENT.SetFont(TableFont);
                            double LimitValue1 = Convert.ToDouble(Row["GroundPressure"]);
                            EsPLCurveTable CurveTable = new EsPLCurveTable();
                            SETTING_DOCUMENT.PutImage(SpudcanDB.DrawPressCurve(mydataset, CurveTable, LimitValue1, 660, 659, 1, 3, DrillingID, Convert.ToInt32(Row["ID"]), UseMetaFile), 562, 561);
                            SETTING_DOCUMENT.WriteLine("\r");

                            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified);
                            SETTING_DOCUMENT.SetParagraph(2);
                            SETTING_DOCUMENT.SetFont(BodyFont);
                            SETTING_DOCUMENT.WriteLine("拔桩力计算结果：");
                            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center);
                            SETTING_DOCUMENT.SetFont(TableFont);
                            SETTING_DOCUMENT.WriteLine($"表{SectionNumber}.{SubSectionNumber + 1} {Row["Name"]}计算{DrillingName}机位不同深度与对应的抗拔力");

                            TabColNames = new string[] { "DrillingID", "", "", "Level", "", "SoilID", "DeepType", "Qu0", "Qu1" };
                            TabColShowNames = new string[] { "机位号", "勘察孔", "泥面标高（m）", "插桩标高（m）", "插深（m）", "进入土层", "计算模式", "冲桩系统完全发挥作用时的拔桩力（t）", "不计减阻系统的最大拔桩力（t）" };
                            N = TabColShowNames.Length;
                            Rows = mydataset.Tables["LS_PullResistanceResult"].Select($"BoatID={Row["ID"]} and DrillingID={DrillingID}");
                            string[,] tablePull = new string[Rows.Length + 1, N];
                            J = 0;
                            for (int i = 0; i <= N - 1; i++)
                            {
                                tablePull[J, i] = TabColShowNames[i];
                            }
                            long[] twPull = new long[N];
                            for (int i = 0; i <= N - 1; i++)
                            {
                                twPull[i] = 900;
                            }
                            twPull[0] = 400;
                            foreach (DataRow Irow in Rows)
                            {
                                GetResult(ref J, Row, Irow, N, TabColNames, TabColShowNames, false);
                            }
                            inset_a_table(ref SETTING_DOCUMENT, tablePull, N, J + 1, twPull);
                            SETTING_DOCUMENT.WriteLine("\r");
                            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified);
                            SETTING_DOCUMENT.SetParagraph(2);
                            SETTING_DOCUMENT.SetFont(BodyFont);
                            SETTING_DOCUMENT.WriteLine("桩靴底部地基承载力随插深标高变化情况见下图：");

                            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center);
                            SETTING_DOCUMENT.SetFont(TableFont);
                            EsPLCurveTable curvePull = new EsPLCurveTable();
                            curvePull.Curves.Clear();
                            SETTING_DOCUMENT.PutImage(SpudcanDB.DrawPullCurve(mydataset, curvePull, 660, 659, 2, 1, DrillingID), 562, 561);
                            SETTING_DOCUMENT.WriteLine("\r");
                        }
                    }

                    if (DrillingID == 0)
                    {
                        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified);
                        SETTING_DOCUMENT.SetFont(BodyFont);
                        SETTING_DOCUMENT.WriteLine("桩靴底部地基承载力随插深标高变化情况见附录一。");
                        SETTING_DOCUMENT.WriteLine("\r");

                        SubSectionNumber += 1;
                        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center);
                        SETTING_DOCUMENT.SetFont(TableFont);
                        SETTING_DOCUMENT.WriteLine($"表{SectionNumber}.{SubSectionNumber} {Row["Name"]}穿刺相对安全系数结果");

                        TabColNames = new string[] { "DrillingID", "P1", "P2", "P3", "Fs1", "Fs2", "IsPunctureRiskOK" };
                        TabColShowNames = new string[] { "孔位", "P1（kPa）", "P2（kPa）", "P3（kPa）", "Fs1", "Fs2", "是否满足" };
                        int boatIdInt = Convert.ToInt32(Row["ID"]);
                        Rows = mydataset.Tables["LS_PunctureRiskAssessmentResult"].Select($"BoatID={boatIdInt}");
                        N = TabColShowNames.Length;
                        string[,] TablePuncture = new string[DrillingDic[boatIdInt].Count + 1, N];
                        J = 0;
                        for (int i = 0; i <= N - 1; i++)
                        {
                            TablePuncture[J, i] = TabColShowNames[i];
                        }
                        long[] twPuncture = new long[N];
                        for (int i = 0; i <= N - 1; i++)
                        {
                            twPuncture[i] = 900;
                        }

                        for (int di = 0; di <= DrillingDic[boatIdInt].Count - 1; di++)
                        {
                            bool NoResult = true;
                            foreach (DataRow Irow in Rows)
                            {
                                int drId = Convert.ToInt32(Irow["DrillingID"]);
                                if (drId == DrillingDic[boatIdInt][di])
                                {
                                    J += 1;
                                    DataRow tempDrRow = mydataset.Tables["LS_TempSoilDrilling"].Select($"BoatID={boatIdInt} and DrillingID={DrillingDic[boatIdInt][di]}")[0];
                                    TablePuncture[J, 0] = tempDrRow["DrillingName"].ToString();
                                    TablePuncture[J, 1] = Irow["P1"].ToString();
                                    TablePuncture[J, 2] = Irow["P2"].ToString();
                                    TablePuncture[J, 3] = Irow["P3"].ToString();
                                    TablePuncture[J, 4] = Irow["Fs1"].ToString();
                                    TablePuncture[J, 5] = Irow["Fs2"].ToString();
                                    TablePuncture[J, 6] = Convert.ToBoolean(Irow["IsPunctureRiskOK"]) ? "是" : "否";
                                    NoResult = false;
                                }
                            }
                        }
                        inset_a_table(ref SETTING_DOCUMENT, TablePuncture, N, J + 1, twPuncture);
                        SETTING_DOCUMENT.WriteLine("\r");
                        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified);
                        SETTING_DOCUMENT.SetParagraph(2);
                        SETTING_DOCUMENT.SetFont(BodyFont);
                        SETTING_DOCUMENT.WriteLine("  注：穿刺相对安全系数按《海洋井场调查规范》有关规定计算。");
                    }
                }

                Setting_Template_Conclusion(DrillingID);
                if (DrillingID == 0)
                {
                    SETTING_DOCUMENT.NewPage();
                    SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left);
                    SETTING_DOCUMENT.SetFont(Heading2Font);
                    SETTING_DOCUMENT.WriteLine("附录一");
                    SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified);
                    SETTING_DOCUMENT.SetParagraph(2);
                    SETTING_DOCUMENT.SetFont(BodyFont);
                    SETTING_DOCUMENT.WriteLine("桩靴底部地基承载力随插深标高变化情况见下图。");
                    EsPLCurveTable CurveTableApp = new EsPLCurveTable();
                    foreach (DataRow Row in mydataset.Tables["LS_Boat"].Select("IsCount=1"))
                    {
                        int boatId = Convert.ToInt32(Row["ID"]);
                        for (int di = 0; di <= DrillingDic[boatId].Count - 1; di++)
                        {
                            int ADrillingID = DrillingDic[boatId][di];
                            if (!SameDrillingDic[boatId].ContainsKey(ADrillingID) || ADrillingID == SameDrillingDic[boatId][ADrillingID][1])
                            {
                                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center);
                                SETTING_DOCUMENT.SetFont(TableFont);
                                double LimitValue1 = Convert.ToDouble(Row["GroundPressure"]);
                                CurveTableApp = new EsPLCurveTable();
                                SETTING_DOCUMENT.PutImage(SpudcanDB.DrawPressCurve(mydataset, CurveTableApp, LimitValue1, 660, 659, 1, 3, ADrillingID, boatId, UseMetaFile), 562, 561);
                                SETTING_DOCUMENT.WriteLine("\r");
                            }
                        }
                    }
                }
            }
            catch
            {
                //On Error Resume Next
            }
        }

        /// <summary>
        /// VB Sub GetResult 转换C#
        /// </summary>
        /// <param name="J">行索引，ByRef</param>
        /// <param name="Row">平台船DataRow</param>
        /// <param name="Irow">计算结果DataRow</param>
        /// <param name="N">列总数</param>
        /// <param name="TabColNames">数据库字段名数组</param>
        /// <param name="TabColShowNames">显示列名数组</param>
        /// <param name="IsPressResult">是否为压桩结果</param>
        /// <param name="Table">输出表格二维数组 string[,]</param>
        /// <param name="mydataset">全局数据集</param>
        internal static void GetResult(ref int J, DataRow Row, DataRow Irow, int N, string[] TabColNames, string[] TabColShowNames, bool IsPressResult, string[,] Table, DataSet mydataset)
        {
            J += 1;
            for (int i = 0; i <= N - 1; i++)
            {
                if (!string.IsNullOrEmpty(TabColNames[i]))
                {
                    switch (TabColShowNames[i])
                    {
                        case "持力层":
                        case "进入土层":
                            {
                                string filter = $"BoatID={Row["ID"]} and DrillingID={Irow["DrillingID"]} and ID={Irow[TabColNames[i]]}";
                                DataRow dr = mydataset.Tables["LS_SoilDrillingParameter"].Select(filter)[0];
                                Table[J, i] = dr["Name"].ToString();
                                break;
                            }
                        case "桩靴底部地基承载力（kPa）":
                            {
                                double val = Convert.ToDouble(Irow[TabColNames[i]]) / Convert.ToDouble(Row["SpudcanA"]);
                                Table[J, i] = Math.Round(val, 2).ToString();
                                break;
                            }
                        case "不计减阻系统的最大拔桩力（t）":
                        case "冲桩系统完全发挥作用时的拔桩力（t）":
                            {
                                double val = Convert.ToDouble(Irow[TabColNames[i]]) / 9.8;
                                Table[J, i] = Math.Round(val, 2).ToString();
                                break;
                            }
                        case "计算模式":
                            {
                                if (IsPressResult)
                                {
                                    bool flag = Convert.ToBoolean(Irow[TabColNames[i]]);
                                    if (flag)
                                    {
                                        string filter = $"ID={Irow[TabColNames[i]]}";
                                        DataRow dr = mydataset.Tables["LS_ComputingModelType_Qv"].Select(filter)[0];
                                        Table[J, i] = dr["Name"].ToString();
                                    }
                                    else
                                    {
                                        if (Convert.ToDouble(Irow["Qv"]) == Convert.ToDouble(Irow["Qv1"]))
                                        {
                                            Table[J, i] = "常规破坏";
                                        }
                                        if (Convert.ToDouble(Irow["Qv"]) == Convert.ToDouble(Irow["Qv2"]))
                                        {
                                            Table[J, i] = "挤出破坏";
                                        }
                                        if (Convert.ToDouble(Irow["Qv"]) == Convert.ToDouble(Irow["Qv3"]))
                                        {
                                            Table[J, i] = "穿刺破坏";
                                        }
                                    }
                                }
                                else
                                {
                                    string filter = $"ID={Irow[TabColNames[i]]}";
                                    DataRow dr = mydataset.Tables["LS_TempDeepType1"].Select(filter)[0];
                                    Table[J, i] = dr["Name"].ToString();
                                }
                                break;
                            }
                        default:
                            {
                                Table[J, i] = Irow[TabColNames[i]]?.ToString() ?? "";
                                break;
                            }
                    }
                }
                else
                {
                    switch (TabColShowNames[i])
                    {
                        case "勘察孔":
                            {
                                string filter = $"BoatID={Row["ID"]} and DrillingID={Irow["DrillingID"]}";
                                DataRow dr = mydataset.Tables["LS_TempSoilDrilling"].Select(filter)[0];
                                Table[J, i] = dr["DrillingName"].ToString();
                                break;
                            }
                        case "平台船名":
                            {
                                Table[J, i] = Row["Name"]?.ToString() ?? "";
                                break;
                            }
                        case "泥面标高（m）":
                            {
                                string filter = $"BoatID={Row["ID"]} and DrillingID={Irow["DrillingID"]}";
                                object objVal = mydataset.Tables["LS_SoilDrillingParameter"].Compute("Max(TopLevel)", filter);
                                Table[J, i] = objVal?.ToString() ?? "";
                                break;
                            }
                        case "理论计算插深（m）":
                        case "插深（m）":
                            {
                                double v1 = double.Parse(Table[J, i - 2]);
                                double v2 = double.Parse(Table[J, i - 1]);
                                Table[J, i] = (v1 - v2).ToString();
                                break;
                            }
                    }
                }
            }
        }
        internal static void Setting_Template_Conclusion(int DrillingID)
        {
            Dictionary<int, List<double>> LevelDic = new Dictionary<int, List<double>>();
            Dictionary<int, List<double>> DepthDic = new Dictionary<int, List<double>>();
            Dictionary<int, List<string>> PunctureRiskDic = new Dictionary<int, List<string>>();
            Dictionary<int, List<string>> Qu0DrillingNameDic = new Dictionary<int, List<string>>();
            Dictionary<int, List<string>> Qu1DrillingNameDic = new Dictionary<int, List<string>>();
            Dictionary<int, List<string>> Qu0OkDrillingNameDic = new Dictionary<int, List<string>>();

            Dictionary<int, List<int>> DrillingDic = new Dictionary<int, List<int>>();

            foreach (DataRow Row in mydataset.Tables["LS_Boat"].Select("IsCount=1", "ID"))
            {
                string filterIrow = $"BoatID={Row["ID"]}";
                if (DrillingID != 0)
                {
                    filterIrow += $" and DrillingID={DrillingID}";
                }
                foreach (DataRow Irow in mydataset.Tables["LS_TempSoilDrilling"].Select(filterIrow))
                {
                    if (!DrillingDic.ContainsKey((int)Row["ID"]))
                    {
                        DrillingDic.Add((int)Row["ID"], new List<int>());
                    }
                    int did = (int)Irow["DrillingID"];
                    if (!DrillingDic[(int)Row["ID"]].Contains(did))
                    {
                        DrillingDic[(int)Row["ID"]].Add(did);
                    }
                }
            }

            foreach (DataRow Row in mydataset.Tables["LS_Boat"].Select("IsCount=1"))
            {
                int boatId = (int)Row["ID"];
                LevelDic.Add(boatId, new List<double>());
                DepthDic.Add(boatId, new List<double>());
                PunctureRiskDic.Add(boatId, new List<string>());
                Qu0DrillingNameDic.Add(boatId, new List<string>());
                Qu1DrillingNameDic.Add(boatId, new List<string>());
                Qu0OkDrillingNameDic.Add(boatId, new List<string>());

                DataRow[] Rows = mydataset.Tables["LS_DepthResult"].Select($"IsUserAdd=0 and BoatID={boatId}");
                for (int di = 0; di <= DrillingDic[boatId].Count - 1; di++)
                {
                    int currentDrillingId = DrillingDic[boatId][di];
                    foreach (DataRow Irow in Rows)
                    {
                        if ((int)Irow["DrillingID"] == currentDrillingId)
                        {
                            double suggestedDepth = Convert.ToDouble(Irow["SuggestedDepth"]);
                            LevelDic[boatId].Add(suggestedDepth);

                            string filterMud = $"BoatID={boatId} and DrillingID={Irow["DrillingID"]}";
                            double MudLevel = Convert.ToDouble(mydataset.Tables["LS_SoilDrillingParameter"].Compute("Max(TopLevel)", filterMud));
                            double Level = suggestedDepth;
                            DepthDic[boatId].Add(MudLevel - Level);

                            string filterDname = $"BoatID={boatId} and DrillingID={Irow["DrillingID"]}";
                            string DName = mydataset.Tables["LS_TempSoilDrilling"].Select(filterDname)[0]["DrillingName"].ToString();

                            double qu0Val = Convert.ToDouble(Irow["Qu0"]);
                            double pullingCapacity = Convert.ToDouble(Row["PullingCapacity"]);
                            if (qu0Val / 9.8 >= pullingCapacity)
                            {
                                Qu0DrillingNameDic[boatId].Add(DName);
                            }
                            else
                            {
                                Qu0OkDrillingNameDic[boatId].Add(DName);
                            }

                            double qu1Val = Convert.ToDouble(Irow["Qu1"]);
                            if (qu1Val / 9.8 >= pullingCapacity)
                            {
                                Qu1DrillingNameDic[boatId].Add(DName);
                            }
                            break;
                        }
                    }

                    DataRow[] PRRows = mydataset.Tables["LS_PunctureRiskAssessmentResult"].Select($"BoatID={boatId}");
                    foreach (DataRow Irow in PRRows)
                    {
                        if ((int)Irow["DrillingID"] == currentDrillingId)
                        {
                            bool isPunctureRiskOK = Convert.ToBoolean(Irow["IsPunctureRiskOK"]);
                            if (!isPunctureRiskOK)
                            {
                                string filterDname = $"BoatID={boatId} and DrillingID={Irow["DrillingID"]}";
                                string DName = mydataset.Tables["LS_TempSoilDrilling"].Select(filterDname)[0]["DrillingName"].ToString();
                                PunctureRiskDic[boatId].Add(DName);
                            }
                        }
                    }
                }
            }

            SectionNumber += 1;
            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left);
            SETTING_DOCUMENT.SetFont(Heading2Font);
            SETTING_DOCUMENT.WriteLine($"{SectionNumber}.结论与建议");
            SubSectionNumber = 0;
            SubSectionNumber += 1;
            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified);
            SETTING_DOCUMENT.SetParagraph(2);
            SETTING_DOCUMENT.SetFont(BodyFont);
            SETTING_DOCUMENT.WriteLine($"({SubSectionNumber})插深方面：");

            foreach (DataRow Row in mydataset.Tables["LS_Boat"].Select("IsCount=1"))
            {
                int boatId = (int)Row["ID"];
                List<double> Level = LevelDic[boatId];
                List<double> Depth = DepthDic[boatId];

                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified);
                SETTING_DOCUMENT.SetParagraph(2);
                SETTING_DOCUMENT.SetFont(BodyFont);

                string depthStr;
                if (Depth.Count == 0)
                {
                    depthStr = "无理论插深结果";
                }
                else
                {
                    if (Depth.Count == 1)
                        depthStr = $"{Math.Round(Depth.Min, 2)}m";
                    else
                        depthStr = $"{Math.Round(Depth.Min, 2)}~{Math.Round(Depth.Max, 2)}m";
                }

                string levelStr;
                if (Level.Count == 0)
                {
                    levelStr = "无标高结果";
                }
                else
                {
                    if (Level.Count == 1)
                        levelStr = $"{Math.Round(Level.Min, 2)}m";
                    else
                        levelStr = $"{Math.Round(Level.Max, 2)}~{Math.Round(Level.Min, 2)}m";
                }

                string name = Row["Name"].ToString();
                double sumW = Math.Round(Convert.ToDouble(Row["SumW"]), 2);
                double groundPressure = Math.Round(Convert.ToDouble(Row["GroundPressure"]), 2);

                string line = $"{name}：预压荷载为{sumW}t时，桩靴对地压强为{groundPressure}kPa。{depthStr}，{levelStr}。在同一机位的不同钻孔地层分布变化较大，相应的插深变化较大，施工中应需注意地层变化带来的施工风险。需结合插深和实际气隙等情况综合判断桩腿长度是否满足要求。";
                SETTING_DOCUMENT.WriteLine(line);
            }

            SETTING_DOCUMENT.WriteLine("\r");
            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified);
            SETTING_DOCUMENT.SetParagraph(2);
            SETTING_DOCUMENT.SetFont(BodyFont);
            SETTING_DOCUMENT.WriteLine("此外，应注意以下两个因素：1）船机实际插腿位置的地层分布情况与勘察报告、钻孔报告显示的分布情况可能有所不同；2）桩靴下方持力层发生一定压缩量。");

            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified);
            SETTING_DOCUMENT.SetParagraph(2);
            Font underlineFont = new Font("宋体", 12, FontStyle.Underline);
            SETTING_DOCUMENT.SetFont(underlineFont);
            SETTING_DOCUMENT.WriteLine("以上因素可能导致实际插深与计算插深、预测插深有些差异，无法做到精确预测，尤其是在缺乏现场实操数据的情况下，故桩腿长度应留有一定富余量。建议开展典型工艺试验，并将工艺试验结果反馈至技术中心，以便结合原位测试资料对插深及拔桩力进行修正分析。");

            SETTING_DOCUMENT.WriteLine("\r");
            SubSectionNumber += 1;
            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified);
            SETTING_DOCUMENT.SetParagraph(2);
            SETTING_DOCUMENT.SetFont(BodyFont);
            SETTING_DOCUMENT.WriteLine($"({SubSectionNumber})拔桩力方面：");

            foreach (DataRow Row in mydataset.Tables["LS_Boat"].Select("IsCount=1"))
            {
                int boatId = (int)Row["ID"];
                List<string> Qu1DrillingName = Qu1DrillingNameDic[boatId];
                List<string> Qu0DrillingName = Qu0DrillingNameDic[boatId];
                List<string> Qu0OkDrillingName = Qu0OkDrillingNameDic[boatId];

                string Qu0String = "";
                string Qu1String = "";
                string Qu0OkString = "";

                foreach (var QS in Qu0DrillingName) Qu0String += QS + ",";
                foreach (var QS in Qu0OkDrillingName) Qu0OkString += QS + ",";
                foreach (var QS in Qu1DrillingName) Qu1String += QS + ",";

                if (Qu0DrillingName.Count > 0)
                {
                    Qu0String = Qu0String.Remove(Qu0String.Length - 1, 1);
                    double pct = Math.Round((double)Qu0DrillingName.Count / (Qu0DrillingName.Count + Qu1DrillingName.Count) * 100, 2);
                    Qu0String += $"(占比{pct}%)";
                }
                if (Qu0OkDrillingName.Count > 0)
                {
                    Qu0OkString = Qu0OkString.Remove(Qu0OkString.Length - 1, 1);
                    double pct = Math.Round((double)Qu0OkDrillingName.Count / (Qu0DrillingName.Count + Qu1DrillingName.Count) * 100, 2);
                    Qu0OkString += $"(占比{pct}%)";
                }
                if (Qu1DrillingName.Count > 0)
                {
                    Qu1String = Qu1String.Remove(Qu1String.Length - 1, 1);
                }

                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified);
                SETTING_DOCUMENT.SetParagraph(2);
                SETTING_DOCUMENT.SetFont(BodyFont);

                string part1;
                if (!string.IsNullOrEmpty(Qu1String))
                    part1 = $"超过最大拔桩能力，{Qu1String}";
                else
                    part1 = "小于最大拔桩能力，未";

                string part2;
                if (!string.IsNullOrEmpty(Qu0String))
                {
                    if (!string.IsNullOrEmpty(Qu0OkString))
                        part2 = $"{Qu0OkString}满足拔桩力要求，其余{Qu0String}需特别注意拔桩能力问题。";
                    else
                        part2 = "所有机位均不满足拔桩力要求，";
                }
                else
                {
                    part2 = "所有机位满足拔桩力要求。";
                }

                string text = $"{Row["Name"]}：不计减阻系统的最大拔桩阻力{part1}存在拔桩力不足的问题。由于船体自带冲桩减阻系统，冲桩减阻系统完全发挥作用时，假定桩靴周围土体均已发生破坏，即土体的抗剪强度为0。若考虑冲桩减阻系统完全发挥作用，{part2}";
                SETTING_DOCUMENT.WriteLine(text);
            }

            SETTING_DOCUMENT.WriteLine("\r");
            SubSectionNumber += 1;
            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified);
            SETTING_DOCUMENT.SetParagraph(2);
            SETTING_DOCUMENT.SetFont(BodyFont);
            SETTING_DOCUMENT.WriteLine("（3）穿刺风险评估：场地内砂层、粘土层交错分布，砂土层层厚较薄，尽管考虑了穿刺破坏模式验算，但并不能完全排除穿刺风险，实际施工中应特别谨慎操作，反复插拔、增加保压时间，防止液化、穿刺风险。");

            foreach (DataRow Row in mydataset.Tables["LS_Boat"].Select("IsCount=1"))
            {
                int boatId = (int)Row["ID"];
                string RiskDrillingNames = "";
                foreach (var RDName in PunctureRiskDic[boatId])
                {
                    RiskDrillingNames += RDName + ",";
                }
                if (!string.IsNullOrEmpty(RiskDrillingNames))
                {
                    RiskDrillingNames = RiskDrillingNames.Remove(RiskDrillingNames.Length - 1, 1);
                    SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified);
                    SETTING_DOCUMENT.SetParagraph(2);
                    SETTING_DOCUMENT.SetFont(BodyFont);
                    SETTING_DOCUMENT.WriteLine($"{Row["Name"]}：需注意{RiskDrillingNames}等机位的穿刺风险。");
                }
            }

            SETTING_DOCUMENT.WriteLine("\r");
            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified);
            SETTING_DOCUMENT.SetParagraph(2);
            SETTING_DOCUMENT.SetFont(BodyFont);
            string finalText = @"（4）本报告所采用的船舶相关参数由项目部提供，尤其是平台船的桩腿预压力和拔桩力对桩腿是否能够安全作业至关重要，因此项目部的桩腿插拔决策应结合船舶运营方管理要求、船舶操作手册中的船舶与装载信息、相关技术人员与操船人员的经验进行综合考虑。实际施工时须依据船舶操作手册以及相关规定进行施工，确保作业工况合规，保证平台稳性。
（5）地质资料方面：由于目前勘察资料主要服务于风机基础设计，其工况与自升式平台船插拔桩作业工况有较大差异，勘察资料提供的土质参数可能对软件计算结果造成一定偏差。此外，插拔桩作业桩靴实际停留位置与钻孔位置不尽相同，土层分布也会有所变化，也会对计算结果造成偏差。提供的勘察资料为中间成果报告可能会对计算结果造成一定偏差。
（6）当插深超过船舶已有施工经验时建议开展专题论证分析。
（7）自升式平台船桩靴插拔作业分析涉及较为复杂的岩土力学问题，对于插拔频繁的自升式风电安装船，国内外尚不存在能够完全准确预报桩腿插拔计算的模型，基于目前有限的相关研究和资料，计算假定破坏模式可能和实际的插拔桩破坏模式存在差别，这可能会导致计算结果的偏差。因而本报告仅供参考。";
            SETTING_DOCUMENT.WriteLine(finalText);
        }

    }

}
