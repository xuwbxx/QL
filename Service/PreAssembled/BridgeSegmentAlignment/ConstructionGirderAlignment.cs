using System;
using System.Collections.Generic;
using System.Linq;

namespace Service.PreAssembled.BridgeSegmentAlignment
{
    /// <summary>
    /// 主类：短线预制梁节段线型对齐计算（短线匹配法预制节段梁的端面放样与坐标修正）
    /// 移植自修复版 b_8.15_update_fixed.py（数值已与 Python 原版逐位对齐）。
    /// 入口：Run(double) / RunDecimal(decimal，内部转 double 运算，结果默认保留4位小数)。
    /// 另含 PrintReport 几何报告算法（等价 Python 的 PR(K)，见 #region PrintReport）。
    /// </summary>
    /// <remarks>
    /// 坐标与数据约定（算法全局通用）：
    /// - 坐标系：X=顺桥向（纵向），Y=横桥向（横向），Z=竖向（高程方向）；单位：米。
    /// - 端面节点 9 列布局：[E.X,E.Y,E.Z, C.X,C.Y,C.Z, W.X,W.Y,W.Z]，
    ///   即一个端面上的三个特征点：E=East(东侧角点)、C=Center(中心点)、W=West(西侧角点)。
    /// - K  ：设计/理论端面坐标表（每行一个端面，9 列）。
    /// - KT ：现场拼装（实测并修正后）端面坐标表，与 K 同构。
    /// - KCT：实测测点坐标表（每个已浇节段南/北两端各一行，共 2m 行）。
    /// - m  ：当前节段编号（0=首段/0号段，≥1=标准段）。
    /// - d  ：梁轴线行进方向符号（恒为 ±1），由相邻端面 C 点 X 差的符号确定。
    /// </remarks>
    public class ConstructionGirderAlignment
    {
        /// <summary>
        /// 【原始double入口】核心算法
        /// </summary>
        public ComputeOutput Run(ComputeInput input)
        {
            var output = new ComputeOutput();
            double[][] KK = new double[5][];
            for (int i = 0; i < 5; i++) KK[i] = new double[9];

            //====实测数据 E 的来源选择：优先用强类型实体（0号段14项/标准段16项），否则用原始数组====
            //====（使用局部 eArr，不修改外部 input 对象）====
            double[] eArr;
            if (input.M == 0 && input.ZerothMeasuredE != null)
            {
                eArr = input.ZerothMeasuredE.ToArray();
            }
            else if (input.M != 0 && input.StandardMeasuredE != null)
            {
                eArr = input.StandardMeasuredE.ToArray();
            }
            else
            {
                eArr = input.E;
            }

            // d=梁轴线行进方向符号（恒为±1）：由第 0、1 两个端面中心点 C 的 X 差的符号决定。
            // 局部坐标系中 X 分量统一乘 d，使"沿浇筑前进方向"恒为正。
            double d = (input.K[0][3] - input.K[1][3]) / Math.Abs(input.K[0][3] - input.K[1][3]);
            int m = input.M;          // 当前节段编号（0=首段/0号段）
            int nSegType = 0;         // 节段类型：0=首段(0号段) 1=第二段 2=标准段

            // KK[0]/KK[1]：当前节段南(匹配)端与北(前)端的设计节点
            for (int i = 0; i < 9; i++)
            {
                KK[0][i] = input.K[m][i];
                KK[1][i] = input.K[m + 1][i];
            }
            if (m != 0)
            {
                // KK[3]/KK[4]：上一已浇节段南/北两侧的实测测点（KCT 每节段 2 行）
                for (int i = 0; i < 9; i++)
                {
                    KK[3][i] = input.KCT[2 * (m - 1)][i];
                    KK[4][i] = input.KCT[2 * m - 1][i];
                }
            }

            CalcResult calcRes;
            if (m < 2)
            {
                nSegType = m;
                for (int i = 0; i < 9; i++)
                    KK[2][i] = input.K[m][i];
                calcRes = Calc(KK, d, nSegType, input.A, input.B, input.C, input.H);
            }
            else
            {
                nSegType = 2;
                for (int i = 0; i < 9; i++)
                    KK[2][i] = input.KT[m][i];
                calcRes = Calc(KK, d, nSegType, input.A, input.B, input.C, input.H);
            }

            if (m == 0)
            {
                output.LogMessage.Add($"No.{m} Floating Bulkhead Setup:");
                output.LogMessage.Add($"East Elev:{calcRes.FloatingBulkheadEastElev}");
                output.LogMessage.Add($"West Elev:{calcRes.FloatingBulkheadWestElev}");
                output.LogMessage.Add($"Center Offset:{calcRes.FloatingBulkheadCenterOffset}");
                output.LogMessage.Add($"East Length:{calcRes.WetCastEastLength}");
                output.LogMessage.Add($"West Length:{calcRes.WetCastWestLength}");
            }
            else
            {
                output.LogMessage.Add($"No.{m} Match-Cast Set-up:");
                output.LogMessage.Add($"Length:{calcRes.MatchCenterLength}");
                output.LogMessage.Add($"Center Offset:");
                output.LogMessage.Add($"South:{calcRes.MatchSouthCenterOffset}");
                output.LogMessage.Add($"North:{calcRes.MatchNorthCenterOffset}");
                output.LogMessage.Add($"South East Elev:{calcRes.MatchSouthEastElev}");
                output.LogMessage.Add($"South West Elev:{calcRes.MatchSouthWestElev}");
                output.LogMessage.Add($"North East Elev:{calcRes.MatchNorthEastElev}");
                output.LogMessage.Add($"North West Elev:{calcRes.MatchNorthWestElev}");
            }

            if (input.St == 1)
            {
                output.LogMessage.Add("进行坐标修正");
                bool judgeOk = Judge(eArr, nSegType, 1);
                if (!judgeOk)
                {
                    output.LogMessage.Add("退出运行");
                    output.JudgePass = false;
                    return output;
                }
                output.JudgePass = true;

                double[] KCBR = null, KCFR = null, KBR = null, KFR = null;
                if (nSegType == 0)
                {
                    output.LogMessage.Add("perfect cast:");
                    ZeroErr(eArr, input.Bo, input.UseB, input.A, input.C, calcRes, KK, d, calcRes.cs,
                        out KCBR, out KCFR, out KBR, out KFR);
                }
                else
                {
                    output.LogMessage.Add("perfect cast:");
                    OneErr(eArr, input.Bo, input.UseB, input.A, input.C, input.B, input.H, calcRes, KK, d, calcRes.cs,
                        out KCBR, out KCFR, out KFR);
                }

                var KT = DeepCopy2D(input.KT);
                var KCT = DeepCopy2D(input.KCT);

                if (m != 0)
                {
                    for (int i = 0; i < 9; i++)
                        KT[m + 1][i] = KFR[i];
                }
                else
                {
                    for (int i = 0; i < 9; i++)
                    {
                        KT[m][i] = KBR[i];
                        KT[m + 1][i] = KFR[i];
                    }
                }
                for (int i = 0; i < 9; i++)
                {
                    KCT[2 * m][i] = KCBR[i];
                    KCT[2 * m + 1][i] = KCFR[i];
                }

                double LT = Dist3D(
                    input.K[m][3], input.K[m][4], input.K[m][5],
                    input.K[m + 1][3], input.K[m + 1][4], input.K[m + 1][5]);

                double LA = Dist3D(
                    KT[m][3], KT[m][4], KT[m][5],
                    KT[m + 1][3], KT[m + 1][4], KT[m + 1][5]);

                double LTT = Dist3D(
                    input.K[0][3], input.K[0][4], input.K[0][5],
                    input.K[m + 1][3], input.K[m + 1][4], input.K[m + 1][5]);

                double LAT = Dist3D(
                    KT[0][3], KT[0][4], KT[0][5],
                    KT[m + 1][3], KT[m + 1][4], KT[m + 1][5]);

                output.LogMessage.Add($"Length on Center:");
                output.LogMessage.Add($"Theoretical:{LT} Total:{LTT}");
                output.LogMessage.Add($"As-Cast:{LA} Total:{LAT}");
                output.LogMessage.Add("现场拼装坐标已更新");
                output.TheoreticalCenterLength = LT;
                output.TheoreticalTotalCenterLength = LTT;
                output.AsCastCenterLength = LA;
                output.AsCastTotalCenterLength = LAT;
                output.KT = KT;
                output.KCT = KCT;
                output.K = DeepCopy2D(input.K);

                // 偏差曲线：对已确定的节段（KT 前 mm 行，按首列非零判定有效行数），
                // 计算 修正后端面(KT) 与 设计端面(K) 的逐节段偏差。
                // 同时保留两套口径：
                //   旧口径（errL/errHP/errH）= 中心线 X 向长度 / Y 向偏距 / Z 向高程；
                //   新口径（errHE/errHC/errHW）= 东侧 / 中心 / 西侧测点的 Z 向高程。
                List<double> errL = new List<double>();
                List<double> errHP = new List<double>();
                List<double> errH = new List<double>();
                List<double> errHE = new List<double>();
                List<double> errHC = new List<double>();
                List<double> errHW = new List<double>();
                int mm = 0;
                for (int i = 0; i < KT.Length; i++)
                {
                    if (Math.Abs(KT[i][0]) > 1e-12) mm++;
                    else break;
                }
                for (int i = 0; i < mm; i++)
                {
                    errL.Add(KT[i][3] - output.K[i][3]);
                    errHP.Add(KT[i][4] - output.K[i][4]);
                    errH.Add(KT[i][5] - output.K[i][5]);
                    // 新口径：端面 9 列布局 [E.X,E.Y,E.Z, C.X,C.Y,C.Z, W.X,W.Y,W.Z]
                    errHE.Add(KT[i][2] - output.K[i][2]);
                    errHC.Add(KT[i][5] - output.K[i][5]);
                    errHW.Add(KT[i][8] - output.K[i][8]);
                }
                output.ErrL = errL.ToArray();
                output.ErrHP = errHP.ToArray();
                output.ErrH = errH.ToArray();
                output.ErrHE = errHE.ToArray();
                output.ErrHC = errHC.ToArray();
                output.ErrHW = errHW.ToArray();
            }
            else
            {
                output.LogMessage.Add("不进行坐标修正");
                output.JudgePass = true;
                output.K = DeepCopy2D(input.K);
                output.KT = DeepCopy2D(input.KT);
                output.KCT = DeepCopy2D(input.KCT);
            }
            output.CalcResult = calcRes;
            return output;
        }

        /// <summary>
        /// 【Decimal 对外入口】内部转 double 运算（核心公式只有一份），完成后转回 decimal；
        /// 入参与输出统一按 decimalDigits（默认4位）小数舍入（四舍五入 AwayFromZero）。
        /// </summary>
        public ComputeOutputDecimal RunDecimal(ComputeInputDecimal decimalInput, int decimalDigits = 4)
        {
            var doubleInput = ConvertToComputeInput(decimalInput, decimalDigits);
            var doubleOutput = Run(doubleInput);
            var decimalOutput = ConvertToComputeOutputDecimal(doubleOutput, decimalDigits);
            return decimalOutput;
        }

        /// <summary>double 版本自测：用预设参数调用 Run。
        /// 注意：预设的 K/KT/KCT 全为 0（d=0/0 得 NaN），结果会全是 NaN，仅验证流程不抛异常；
        /// 真实回归测试请使用真实工程数据并对照金标准断言。</summary>
        public static void Test()
        {
            var input = new ComputeInput
            {
                A = 0.1,
                B = 2.9,
                C = 0.1,
                H = 1.0,
                M = 1,
                St = 1,
                UseB = false,
                Bo = new double[]
                {
                    0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1,
                    2.9, 2.9, 2.9, 2.9
                },
                E = new double[]
                {
                    0,2.9950,0,-0.0043,0.9996,1.0004,0.9992,1.0025,
                    0.9986,1.005,0.9944,1.0059,0.9985,1.005,0,0
                }
            };
            input.K = new double[10][];
            for (int i = 0; i < 10; i++) input.K[i] = new double[9];
            input.KT = new double[10][];
            for (int i = 0; i < 10; i++) input.KT[i] = new double[9];
            input.KCT = new double[18][];
            for (int i = 0; i < 18; i++) input.KCT[i] = new double[9];

            var algo = new ConstructionGirderAlignment();
            var result = algo.Run(input);

            Console.WriteLine("=====【double版本】测试运行输出日志=====");
            foreach (var msg in result.LogMessage)
            {
                Console.WriteLine(msg);
            }
        }

        /// <summary>decimal版本自测，默认保留4位小数</summary>
        public static void TestDecimal(int decimalDigits = 4)
        {
            var input = new ComputeInputDecimal
            {
                A = 0.1m,
                B = 2.9m,
                C = 0.1m,
                H = 1.0m,
                M = 1,
                St = 1,
                UseB = false,
                Bo = new decimal[]
                {
                    0.1m, 0.1m, 0.1m, 0.1m, 0.1m, 0.1m, 0.1m, 0.1m, 0.1m,
                    2.9m, 2.9m, 2.9m, 2.9m
                },
                E = new decimal[]
                {
                    0m,2.9950m,0m,-0.0043m,0.9996m,1.0004m,0.9992m,1.0025m,
                    0.9986m,1.005m,0.9944m,1.0059m,0.9985m,1.005m,0m,0m
                }
            };
            input.K = new decimal[10][];
            for (int i = 0; i < 10; i++) input.K[i] = new decimal[9];
            input.KT = new decimal[10][];
            for (int i = 0; i < 10; i++) input.KT[i] = new decimal[9];
            input.KCT = new decimal[18][];
            for (int i = 0; i < 18; i++) input.KCT[i] = new decimal[9];

            var algo = new ConstructionGirderAlignment();
            var result = algo.RunDecimal(input, decimalDigits);

            Console.WriteLine($"\r\n=====【decimal版本】测试运行输出日志（保留{decimalDigits}位小数）=====");
            foreach (var msg in result.LogMessage)
            {
                Console.WriteLine(msg);
            }
        }

        #region 私有转换工具 decimal <-> double
        private static double DecimalToDouble(decimal val, int digits)
        {
            return (double)Math.Round(val, digits, MidpointRounding.AwayFromZero);
        }

        private static decimal DoubleToDecimal(double val, int digits)
        {
            return Math.Round((decimal)val, digits, MidpointRounding.AwayFromZero);
        }

        private static ComputeInput ConvertToComputeInput(ComputeInputDecimal src, int digits)
        {
            var dst = new ComputeInput();
            dst.A = DecimalToDouble(src.A, digits);
            dst.B = DecimalToDouble(src.B, digits);
            dst.C = DecimalToDouble(src.C, digits);
            dst.H = DecimalToDouble(src.H, digits);
            dst.M = src.M;
            dst.St = src.St;
            dst.UseB = src.UseB;

            dst.Bo = src.Bo?.Select(x => DecimalToDouble(x, digits)).ToArray() ?? Array.Empty<double>();

            //====处理decimal版E实体，生成局部数组，不修改src====
            decimal[] eDecArr;
            if (src.M == 0 && src.ZerothMeasuredE != null)
            {
                eDecArr = src.ZerothMeasuredE.ToArray();
            }
            else if (src.M != 0 && src.StandardMeasuredE != null)
            {
                eDecArr = src.StandardMeasuredE.ToArray();
            }
            else
            {
                eDecArr = src.E;
            }
            dst.E = eDecArr?.Select(x => DecimalToDouble(x, digits)).ToArray() ?? Array.Empty<double>();

            if (src.K != null)
            {
                dst.K = new double[src.K.Length][];
                for (int i = 0; i < src.K.Length; i++)
                    dst.K[i] = src.K[i].Select(x => DecimalToDouble(x, digits)).ToArray();
            }
            if (src.KT != null)
            {
                dst.KT = new double[src.KT.Length][];
                for (int i = 0; i < src.KT.Length; i++)
                    dst.KT[i] = src.KT[i].Select(x => DecimalToDouble(x, digits)).ToArray();
            }
            if (src.KCT != null)
            {
                dst.KCT = new double[src.KCT.Length][];
                for (int i = 0; i < src.KCT.Length; i++)
                    dst.KCT[i] = src.KCT[i].Select(x => DecimalToDouble(x, digits)).ToArray();
            }
            return dst;
        }

        private static ComputeOutputDecimal ConvertToComputeOutputDecimal(ComputeOutput src, int digits)
        {
            var dst = new ComputeOutputDecimal();
            dst.JudgePass = src.JudgePass;
            dst.LogMessage = new List<string>(src.LogMessage);

            if (src.K != null)
            {
                dst.K = new decimal[src.K.Length][];
                for (int i = 0; i < src.K.Length; i++)
                    dst.K[i] = src.K[i].Select(x => DoubleToDecimal(x, digits)).ToArray();
            }
            if (src.KT != null)
            {
                dst.KT = new decimal[src.KT.Length][];
                for (int i = 0; i < src.KT.Length; i++)
                    dst.KT[i] = src.KT[i].Select(x => DoubleToDecimal(x, digits)).ToArray();
            }
            if (src.KCT != null)
            {
                dst.KCT = new decimal[src.KCT.Length][];
                for (int i = 0; i < src.KCT.Length; i++)
                    dst.KCT[i] = src.KCT[i].Select(x => DoubleToDecimal(x, digits)).ToArray();
            }

            dst.TheoreticalCenterLength = DoubleToDecimal(src.TheoreticalCenterLength, digits);
            dst.TheoreticalTotalCenterLength = DoubleToDecimal(src.TheoreticalTotalCenterLength, digits);
            dst.AsCastCenterLength = DoubleToDecimal(src.AsCastCenterLength, digits);
            dst.AsCastTotalCenterLength = DoubleToDecimal(src.AsCastTotalCenterLength, digits);

            dst.ErrL = src.ErrL?.Select(x => DoubleToDecimal(x, digits)).ToArray() ?? Array.Empty<decimal>();
            dst.ErrHP = src.ErrHP?.Select(x => DoubleToDecimal(x, digits)).ToArray() ?? Array.Empty<decimal>();
            dst.ErrH = src.ErrH?.Select(x => DoubleToDecimal(x, digits)).ToArray() ?? Array.Empty<decimal>();
            dst.ErrHE = src.ErrHE?.Select(x => DoubleToDecimal(x, digits)).ToArray() ?? Array.Empty<decimal>();
            dst.ErrHC = src.ErrHC?.Select(x => DoubleToDecimal(x, digits)).ToArray() ?? Array.Empty<decimal>();
            dst.ErrHW = src.ErrHW?.Select(x => DoubleToDecimal(x, digits)).ToArray() ?? Array.Empty<decimal>();

            var cr = src.CalcResult;
            if (cr != null)
            {
                dst.CalcResultDecimal = new CalcResultDecimal
                {
                    MatchCenterLength = DoubleToDecimal(cr.MatchCenterLength, digits),
                    MatchSouthCenterOffset = DoubleToDecimal(cr.MatchSouthCenterOffset, digits),
                    MatchNorthCenterOffset = DoubleToDecimal(cr.MatchNorthCenterOffset, digits),
                    MatchSouthEastElev = DoubleToDecimal(cr.MatchSouthEastElev, digits),
                    MatchSouthWestElev = DoubleToDecimal(cr.MatchSouthWestElev, digits),
                    MatchNorthEastElev = DoubleToDecimal(cr.MatchNorthEastElev, digits),
                    MatchNorthWestElev = DoubleToDecimal(cr.MatchNorthWestElev, digits),
                    WetCastSouthEastElev = DoubleToDecimal(cr.WetCastSouthEastElev, digits),
                    WetCastSouthWestElev = DoubleToDecimal(cr.WetCastSouthWestElev, digits),
                    WetCastNorthEastElev = DoubleToDecimal(cr.WetCastNorthEastElev, digits),
                    WetCastNorthWestElev = DoubleToDecimal(cr.WetCastNorthWestElev, digits),
                    WetCastSouthCenterOffset = DoubleToDecimal(cr.WetCastSouthCenterOffset, digits),
                    WetCastNorthCenterOffset = DoubleToDecimal(cr.WetCastNorthCenterOffset, digits),
                    WetCastEastLength = DoubleToDecimal(cr.WetCastEastLength, digits),
                    WetCastWestLength = DoubleToDecimal(cr.WetCastWestLength, digits),
                    FloatingBulkheadEastElev = DoubleToDecimal(cr.FloatingBulkheadEastElev, digits),
                    FloatingBulkheadWestElev = DoubleToDecimal(cr.FloatingBulkheadWestElev, digits),
                    FloatingBulkheadCenterOffset = DoubleToDecimal(cr.FloatingBulkheadCenterOffset, digits),
                    FixedBulkheadEastElev = DoubleToDecimal(cr.FixedBulkheadEastElev, digits),
                    FixedBulkheadWestElev = DoubleToDecimal(cr.FixedBulkheadWestElev, digits),
                    FixedBulkheadCenterOffset = DoubleToDecimal(cr.FixedBulkheadCenterOffset, digits),
                    // 给Twist赋值
                    MatchCastTwist = DoubleToDecimal(cr.MatchCastTwist, digits)
                };
            }
            return dst;
        }
        #endregion

        private const double Epsilon = 1e-12;
        private const decimal DecEpsilon = 1e-12m;

        #region 底层算法函数
        /// <summary>
        /// 理论放样计算总调度：按节段类型 n 分派（n=0 首段 / n=1 第二段 / n=2 标准段）。
        /// 统一返回当前节段南/北端面的放样要素：节段长、偏距、四角计算高程、
        /// 五个关键节点坐标（局部系）与三次旋转参数 cs。
        /// </summary>
        private CalcResult Calc(double[][] K, double d, int n, double A, double B, double C, double H)
        {
            if (n == 0) return Zero(K, d, A, B, C, H);
            if (n == 1) return One(K, d, A, B, C, H);
            if (n == 2) return Standard(K, d, A, B, C, H);
            throw new ArgumentException($"未知节段类型 n = {n}");
        }

        private CalcResult Zero(double[][] K, double d, double A, double B, double C, double H)
        {
            double[] k1 = K[0];
            double[] k2 = K[1];
            var (arr1, cs) = AxisCha(K, k1, d, A, B, C);
            var (arr2, _) = AxisCha(K, k2, d, A, B, C);
            var theoreticalJointS = ArrayToJoint(arr1);
            var theoreticalJointN = ArrayToJoint(arr2);

            CalcResult calcRes = new CalcResult();
            // 首段无匹配段：Match* 即本段设计端面（沿用原口径，未拆分为 Match/WetCast 两组），
            // 故 WetCast* 四角与偏移在 Zero 分支不赋值（保持 0），前端首段按 IsFirst 走 Match* 分支。
            calcRes.MatchCenterLength = theoreticalJointS.C.X;
            calcRes.MatchSouthCenterOffset = theoreticalJointS.C.Y;
            calcRes.MatchNorthCenterOffset = theoreticalJointN.C.Y;
            calcRes.MatchSouthEastElev = H + theoreticalJointS.E.Z;
            calcRes.MatchSouthWestElev = H + theoreticalJointS.W.Z;
            calcRes.MatchNorthEastElev = H + theoreticalJointN.E.Z;
            calcRes.MatchNorthWestElev = H + theoreticalJointN.W.Z;

            // 浇筑段东西侧长度：首段唯一取值来源就是南端面（活动端模板）的设计纵向坐标，
            // 与 One/Standard 用同一表达式，保持口径一致。
            calcRes.WetCastEastLength = theoreticalJointS.E.X;
            calcRes.WetCastWestLength = theoreticalJointS.W.X;
            calcRes.WetCastCenterLength = theoreticalJointS.C.X;

            // 活动端模板（Floating Bulkhead）目标值：恒定取 South 侧（首段独有，标准段无此端头）
            calcRes.FloatingBulkheadEastElev = H + theoreticalJointS.E.Z;
            calcRes.FloatingBulkheadWestElev = H + theoreticalJointS.W.Z;
            calcRes.FloatingBulkheadCenterOffset = theoreticalJointS.C.Y;

            // 固定端模板（FixedBulkhead）目标值：固定端恒取 North 侧计算结果，
            // 首段同样存在固定端与活动端两组模板，故 Zero 分支也必须输出（与 One/Standard 保持一致）。
            calcRes.FixedBulkheadEastElev = H + theoreticalJointN.E.Z;
            calcRes.FixedBulkheadWestElev = H + theoreticalJointN.W.Z;
            calcRes.FixedBulkheadCenterOffset = theoreticalJointN.C.Y;

            calcRes.TheoreticalJointS = DeepCopyJoint(theoreticalJointS);
            calcRes.TheoreticalJointN = DeepCopyJoint(theoreticalJointN);
            calcRes.cs = cs;
            return calcRes;
        }

        /// <summary>
        /// n=1 第二段：与首段匹配浇注。匹配基准（KK[2]）取首段理论节点，
        /// 南/北"实际点"取上一节段实测测点（KK[3]/KK[4]）。
        /// 额外输出 WetCast*（湿接缝侧=首段理论端面四角高程）与 Bulkhead*（固定端头处要素）。
        /// </summary>
        private CalcResult One(double[][] K, double d, double A, double B, double C, double H)
        {
            // 注意：n=1 与 n=2 的 k1/k5 语义互换（对应原版 res1/res5 的口径，见 Standard 注释）
            double[] k1 = K[0];   // 首段理论节点         → theoreticalJointS（南端理论）
            double[] k2 = K[1];   // 北端节点             → theoreticalJointN（北端理论）
            double[] k3 = K[3];   // 上一节段南侧实测测点 → actualPointS（南端实际）
            double[] k4 = K[4];   // 上一节段北侧实测测点 → actualPointN（北端实际）
            double[] k5 = K[2];   // 匹配基准节点         → matchJointN（匹配浇注北端）

            var (arr1, cs) = AxisCha(K, k1, d, A, B, C);
            var (arr2, _) = AxisCha(K, k2, d, A, B, C);
            var (arr3, _) = AxisCha(K, k3, d, A, B, C);
            var (arr4, _) = AxisCha(K, k4, d, A, B, C);
            var (arr5, _) = AxisCha(K, k5, d, A, B, C);

            var theoreticalJointS = ArrayToJoint(arr1);
            var theoreticalJointN = ArrayToJoint(arr2);
            var matchJointN = ArrayToJoint(arr5);
            var actualPointS = ArrayToJoint(arr3);
            var actualPointN = ArrayToJoint(arr4);

            CalcResult calcRes = new CalcResult();
            calcRes.MatchCenterLength = theoreticalJointS.C.X;
            calcRes.MatchSouthCenterOffset = actualPointS.C.Y;
            calcRes.MatchNorthCenterOffset = actualPointN.C.Y;
            calcRes.MatchSouthEastElev = H + actualPointS.E.Z;
            calcRes.MatchSouthWestElev = H + actualPointS.W.Z;
            calcRes.MatchNorthEastElev = H + actualPointN.E.Z;
            calcRes.MatchNorthWestElev = H + actualPointN.W.Z;

            calcRes.WetCastSouthEastElev = H + theoreticalJointS.E.Z;
            calcRes.WetCastSouthWestElev = H + theoreticalJointS.W.Z;
            calcRes.WetCastNorthEastElev = H + theoreticalJointN.E.Z;
            calcRes.WetCastNorthWestElev = H + theoreticalJointN.W.Z;
            calcRes.WetCastSouthCenterOffset = theoreticalJointS.C.Y;
            calcRes.WetCastNorthCenterOffset = theoreticalJointN.C.Y;
            calcRes.WetCastEastLength = theoreticalJointS.E.X;
            calcRes.WetCastWestLength = theoreticalJointS.W.X;
            calcRes.WetCastCenterLength = theoreticalJointS.C.X;

            calcRes.FixedBulkheadEastElev = H + theoreticalJointN.E.Z;
            calcRes.FixedBulkheadWestElev = H + theoreticalJointN.W.Z;
            calcRes.FixedBulkheadCenterOffset = theoreticalJointN.C.Y;

            calcRes.TheoreticalJointS = DeepCopyJoint(theoreticalJointS);
            calcRes.TheoreticalJointN = DeepCopyJoint(theoreticalJointN);
            calcRes.MatchJointN = DeepCopyJoint(matchJointN);
            calcRes.ActualPointS = DeepCopyJoint(actualPointS);
            calcRes.ActualPointN = DeepCopyJoint(actualPointN);
            calcRes.cs = cs;
            return calcRes;
        }

        /// <summary>
        /// n=2 标准段：与上一已浇节段匹配浇注。匹配基准（KK[2]）取实测端面 KT[m]。
        /// 注意 k1/k5 与 n=1 互换：此处 k1=K[2]→matchJointN、k5=K[0]→theoreticalJointS。
        /// </summary>
        private CalcResult Standard(double[][] K, double d, double A, double B, double C, double H)
        {
            // 与 n=1 相比 k1/k5 语义互换（R[1]/R[5] 口径随节段类型而变）
            double[] k1 = K[2];   // 匹配基准（实测端面）→ matchJointN
            double[] k2 = K[1];   // 北端节点            → theoreticalJointN
            double[] k3 = K[3];   // 南侧实测测点        → actualPointS
            double[] k4 = K[4];   // 北侧实测测点        → actualPointN
            double[] k5 = K[0];   // 南端理论节点        → theoreticalJointS

            var (arr1, cs) = AxisCha(K, k1, d, A, B, C);
            var (arr2, _) = AxisCha(K, k2, d, A, B, C);
            var (arr3, _) = AxisCha(K, k3, d, A, B, C);
            var (arr4, _) = AxisCha(K, k4, d, A, B, C);
            var (arr5, _) = AxisCha(K, k5, d, A, B, C);

            var matchJointN = ArrayToJoint(arr1);
            var theoreticalJointN = ArrayToJoint(arr2);
            var actualPointS = ArrayToJoint(arr3);
            var actualPointN = ArrayToJoint(arr4);
            var theoreticalJointS = ArrayToJoint(arr5);

            CalcResult calcRes = new CalcResult();
            calcRes.MatchCenterLength = matchJointN.C.X;
            calcRes.MatchSouthCenterOffset = actualPointS.C.Y;
            calcRes.MatchNorthCenterOffset = actualPointN.C.Y;
            calcRes.MatchSouthEastElev = H + actualPointS.E.Z;
            calcRes.MatchSouthWestElev = H + actualPointS.W.Z;
            calcRes.MatchNorthEastElev = H + actualPointN.E.Z;
            calcRes.MatchNorthWestElev = H + actualPointN.W.Z;

            calcRes.WetCastSouthEastElev = H + matchJointN.E.Z;
            calcRes.WetCastSouthWestElev = H + matchJointN.W.Z;
            calcRes.WetCastNorthEastElev = H + theoreticalJointN.E.Z;
            calcRes.WetCastNorthWestElev = H + theoreticalJointN.W.Z;
            calcRes.WetCastSouthCenterOffset = matchJointN.C.Y;
            calcRes.WetCastNorthCenterOffset = theoreticalJointN.C.Y;
            calcRes.WetCastEastLength = theoreticalJointS.E.X;
            calcRes.WetCastWestLength = theoreticalJointS.W.X;
            calcRes.WetCastCenterLength = theoreticalJointS.C.X;

            calcRes.FixedBulkheadEastElev = H + theoreticalJointN.E.Z;
            calcRes.FixedBulkheadWestElev = H + theoreticalJointN.W.Z;
            calcRes.FixedBulkheadCenterOffset = theoreticalJointN.C.Y;

            calcRes.TheoreticalJointS = DeepCopyJoint(theoreticalJointS);
            calcRes.TheoreticalJointN = DeepCopyJoint(theoreticalJointN);
            calcRes.MatchJointN = DeepCopyJoint(matchJointN);
            calcRes.ActualPointS = DeepCopyJoint(actualPointS);
            calcRes.ActualPointN = DeepCopyJoint(actualPointN);
            calcRes.cs = cs;
            return calcRes;
        }

        private (double[] K23, RotationParams cs) AxisCha(double[][] K, double[] k, double d, double A, double B, double C)
        {
            double n = 1e-200;
            List<double> K1 = new List<double>(), K2 = new List<double>(), K3 = new List<double>(), K4 = new List<double>();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if ((3 * i + j + 1) % 3 == 0)
                    {
                        K1.Add(K[0][3 * i + j] - K[1][3 + j]);
                        K2.Add(k[3 * i + j] - K[1][3 + j]);
                        K3.Add(K[2][3 * i + j] - K[1][3 + j]);
                        K4.Add(K[1][3 * i + j] - K[1][3 + j]);
                    }
                    else
                    {
                        K1.Add(d * (K[0][3 * i + j] - K[1][3 + j]));
                        K2.Add(d * (k[3 * i + j] - K[1][3 + j]));
                        K3.Add(d * (K[2][3 * i + j] - K[1][3 + j]));
                        K4.Add(d * (K[1][3 * i + j] - K[1][3 + j]));
                    }
                }
            }
            double[] arrK1 = K1.ToArray();
            double[] arrK2 = K2.ToArray();
            double[] arrK3 = K3.ToArray();
            double[] arrK4 = K4.ToArray();

            // 第 1 次旋转（绕 Z 轴，水平面）：以 K[0]−K[1] 向量在 XY 面的方位为准，
            // 使梁轴在水平面上对准 X 轴。LXY=该向量水平投影长度，Sin1/Cos1=转角正余弦。
            double LXY = Math.Sqrt(arrK1[3] * arrK1[3] + arrK1[4] * arrK1[4]);
            double Sin1 = Math.Abs(arrK1[4] / LXY) * ((arrK1[3] + n) / Math.Abs(arrK1[3] + n)) * ((-arrK1[4] + n) / Math.Abs(-arrK1[4] + n));
            double Cos1 = Math.Abs(arrK1[3] / LXY);

            double[] K11 = (double[])arrK1.Clone();
            double[] K21 = (double[])arrK2.Clone();
            double[] K31 = (double[])arrK3.Clone();
            double[] K41 = (double[])arrK4.Clone();
            for (int i = 0; i < 3; i++)
            {
                K11[3 * i] = Cos1 * arrK1[3 * i] - Sin1 * arrK1[3 * i + 1];
                K11[3 * i + 1] = Sin1 * arrK1[3 * i] + Cos1 * arrK1[3 * i + 1];

                K21[3 * i] = Cos1 * arrK2[3 * i] - Sin1 * arrK2[3 * i + 1];
                K21[3 * i + 1] = Sin1 * arrK2[3 * i] + Cos1 * arrK2[3 * i + 1];

                K31[3 * i] = Cos1 * arrK3[3 * i] - Sin1 * arrK3[3 * i + 1];
                K31[3 * i + 1] = Sin1 * arrK3[3 * i] + Cos1 * arrK3[3 * i + 1];

                K41[3 * i] = Cos1 * arrK4[3 * i] - Sin1 * arrK4[3 * i + 1];
                K41[3 * i + 1] = Sin1 * arrK4[3 * i] + Cos1 * arrK4[3 * i + 1];
            }

            double LXZ = Math.Sqrt(K31[3] * K31[3] + K31[5] * K31[5]);
            double Sin2 = Math.Abs(K31[5] / LXZ) * ((K31[3] + n) / Math.Abs(K31[3] + n)) * ((-K31[5] + n) / Math.Abs(-K31[5] + n));
            double Cos2 = Math.Abs(K31[3] / LXZ);

            double[] K12 = (double[])K11.Clone();
            double[] K22 = (double[])K21.Clone();
            double[] K32 = (double[])K31.Clone();
            double[] K42 = (double[])K41.Clone();
            for (int i = 0; i < 3; i++)
            {
                K12[3 * i] = Cos2 * K11[3 * i] - Sin2 * K11[3 * i + 2];
                K12[3 * i + 2] = Sin2 * K11[3 * i] + Cos2 * K11[3 * i + 2];

                K22[3 * i] = Cos2 * K21[3 * i] - Sin2 * K21[3 * i + 2];
                K22[3 * i + 2] = Sin2 * K21[3 * i] + Cos2 * K21[3 * i + 2];

                K32[3 * i] = Cos2 * K31[3 * i] - Sin2 * K31[3 * i + 2];
                K32[3 * i + 2] = Sin2 * K31[3 * i] + Cos2 * K31[3 * i + 2];

                K42[3 * i] = Cos2 * K41[3 * i] - Sin2 * K41[3 * i + 2];
                K42[3 * i + 2] = Sin2 * K41[3 * i] + Cos2 * K41[3 * i + 2];
            }

            // 第 3 次旋转（绕 X 轴，横断面滚转）：以端面自身 W−C 向量（K4）在 YZ 面的方位为准，
            // 使横断面水平（消除端面横坡）。LYZ=该向量 YZ 投影长度。
            double LYZ = Math.Sqrt(K42[7] * K42[7] + K42[8] * K42[8]);
            double Sin3 = Math.Abs(K42[8] / LYZ) * ((K42[8] + n) / Math.Abs(K42[8] + n)) * ((-K42[7] + n) / Math.Abs(-K42[7] + n));
            double Cos3 = Math.Abs(K42[7] / LYZ);

            // 对 K1..K4 执行绕 X 轴旋转（每点取 y,z 分量）
            double[] K13 = (double[])K12.Clone();
            double[] K23 = (double[])K22.Clone();
            double[] K33 = (double[])K32.Clone();
            double[] K43 = (double[])K42.Clone();
            for (int i = 0; i < 3; i++)
            {
                K13[3 * i + 1] = Cos3 * K12[3 * i + 1] - Sin3 * K12[3 * i + 2];
                K13[3 * i + 2] = Sin3 * K12[3 * i + 1] + Cos3 * K12[3 * i + 2];

                K23[3 * i + 1] = Cos3 * K22[3 * i + 1] - Sin3 * K22[3 * i + 2];
                K23[3 * i + 2] = Sin3 * K22[3 * i + 1] + Cos3 * K22[3 * i + 2];

                K33[3 * i + 1] = Cos3 * K32[3 * i + 1] - Sin3 * K32[3 * i + 2];
                K33[3 * i + 2] = Sin3 * K32[3 * i + 1] + Cos3 * K32[3 * i + 2];

                K43[3 * i + 1] = Cos3 * K42[3 * i + 1] - Sin3 * K42[3 * i + 2];
                K43[3 * i + 2] = Sin3 * K42[3 * i + 1] + Cos3 * K42[3 * i + 2];
            }

            RotationParams cs = new RotationParams
            {
                sin1 = Sin1,
                cos1 = Cos1,
                sin2 = Sin2,
                cos2 = Cos2,
                sin3 = Sin3,
                cos3 = Cos3
            };
            return (K23, cs);
        }

        private double[] RevAxisCha(double[][] K, double d, double[] k, RotationParams cs)
        {
            double Sin1 = cs.sin1;
            double Cos1 = cs.cos1;
            double Sin2 = cs.sin2;
            double Cos2 = cs.cos2;
            double Sin3 = cs.sin3;
            double Cos3 = cs.cos3;

            double[] K0 = (double[])k.Clone();
            for (int i = 0; i < 3; i++)
            {
                K0[3 * i + 1] = Cos3 * k[3 * i + 1] + Sin3 * k[3 * i + 2];
                K0[3 * i + 2] = -Sin3 * k[3 * i + 1] + Cos3 * k[3 * i + 2];
            }
            double[] K1 = (double[])K0.Clone();
            for (int i = 0; i < 3; i++)
            {
                K1[3 * i] = Cos2 * K0[3 * i] + Sin2 * K0[3 * i + 2];
                K1[3 * i + 2] = -Sin2 * K0[3 * i] + Cos2 * K0[3 * i + 2];
            }
            double[] K2 = (double[])K1.Clone();
            for (int i = 0; i < 3; i++)
            {
                K2[3 * i] = Cos1 * K1[3 * i] + Sin1 * K1[3 * i + 1];
                K2[3 * i + 1] = -Sin1 * K1[3 * i] + Cos1 * K1[3 * i + 1];
            }
            List<double> K3 = new List<double>();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if ((3 * i + j + 1) % 3 == 0)
                        K3.Add(K2[3 * i + j] + K[1][3 + j]);
                    else
                        K3.Add(d * K2[3 * i + j] + K[1][3 + j]);
                }
            }
            return K3.ToArray();
        }

        /// <summary>
        /// 【空间姿态修正】以实测南端实际点为新基准，对 9 列坐标做"平移+姿态旋转"修正：
        /// 1) 全部点减去基准点（actualPointS 的 C 点）及实测偏差（rerr[20] 横向、rerr[8] 竖向）；
        /// 2) 用 rerr 中由实测误差求出的三组 sin/cos（绕 Z/Y/X，顺序同 AxisCha）旋转；
        /// 3) 加回基准点；4) E/W 两角点分别 ±rerr[9]/2 的反向高程修正（消除横断面扭曲）。
        /// 数学方法：三次平面旋转 + 平移（与 AxisCha 同构的欧拉角分解，角度来自实测）。
        /// </summary>
        /// <param name="actualPointS">南端实测实际点（旋转中心）</param>
        /// <param name="rerr">OneErr 求出的 24 项误差参数（逐项含义见 OneErr 注释）</param>
        /// <param name="k">待修正的 9 列坐标（原地修改并返回）</param>
        private double[] SpAxis(Joint actualPointS, double[] rerr, double[] k)
        {
            double[] ka = { actualPointS.C.X, actualPointS.C.Y, actualPointS.C.Z };
            double[] rk = new double[9];
            for (int i = 0; i < 3; i++)
            {
                rk[3 * i] = k[3 * i] - ka[0];
                rk[3 * i + 1] = k[3 * i + 1] - ka[1] - rerr[20];
                rk[3 * i + 2] = k[3 * i + 2] - ka[2] - rerr[8];
            }
            double sin1 = rerr[14];
            double cos1 = rerr[15];
            double sin2 = rerr[18];
            double cos2 = rerr[19];
            double sin3 = rerr[22];
            double cos3 = rerr[23];

            double[] rk1 = (double[])rk.Clone();
            for (int i = 0; i < 3; i++)
            {
                rk1[3 * i] = rk[3 * i] * cos1 - rk[3 * i + 1] * sin1;
                rk1[3 * i + 1] = rk[3 * i] * sin1 + rk[3 * i + 1] * cos1;
            }
            double[] rk2 = (double[])rk1.Clone();
            for (int i = 0; i < 3; i++)
            {
                rk2[3 * i] = rk1[3 * i] * cos2 - rk1[3 * i + 2] * sin2;
                rk2[3 * i + 2] = rk1[3 * i] * sin2 + rk1[3 * i + 2] * cos2;
            }
            double[] rk3 = (double[])rk2.Clone();
            for (int i = 0; i < 3; i++)
            {
                rk3[3 * i + 1] = rk2[3 * i + 1] * cos3 - rk2[3 * i + 2] * sin3;
                rk3[3 * i + 2] = rk2[3 * i + 1] * sin3 + rk2[3 * i + 2] * cos3;
            }
            for (int i = 0; i < 3; i++)
            {
                rk[3 * i] = rk3[3 * i] + ka[0];
                rk[3 * i + 1] = rk3[3 * i + 1] + ka[1];
                rk[3 * i + 2] = rk3[3 * i + 2] + ka[2];
            }
            rk[2] = rk[2] + rerr[9] / 2.0;
            rk[8] = rk[8] - rerr[9] / 2.0;
            return rk;
        }

        /// <summary>
        /// 【0 号段实测修正】ZeroErr。
        /// <para>业务：0 号段无匹配段，南端=活动端模板、北端=固定端模板。本方法把现场实测 E 与测点偏位 Bo
        /// 叠加到理论端面，得到 as-cast 端面/测点坐标，再经 <see cref="RevAxisCha"/> 逆变换回工程坐标系。</para>
        /// <para>入参获取：</para>
        /// <list type="bullet">
        /// <item><description><paramref name="E"/>：前端「设站及测量」录入的 0 号段 14 项实测值，顺序见 <see cref="MeasuredEZerothSegment"/>；</description></item>
        /// <item><description><paramref name="bo"/>：测点偏位，固定 10 项 [aNE,aNW,aSE,aSW,cN,cS,bNE,bNW,bSE,bSW]，
        /// 由 biz_concrete_beam_segment 的 BoltOffset* 列装配；</description></item>
        /// <item><description><paramref name="useB"/>：是否填写实测 b（BoltOffsetBUseMeasured）。false 时 b 走旧表达式；
        /// true 时直接取 bNE/bNW/bSE/bSW；a 不受此开关影响，始终用四角落库值；</description></item>
        /// <item><description><paramref name="A"/>/<paramref name="C"/>：项目信息中的 A、C 偏位参数；
        /// <paramref name="calcRes"/>/<paramref name="KK"/>/<paramref name="d"/>/<paramref name="cs"/>：Calc/AxisCha 的产出。</description></item>
        /// </list>
        /// <para>输出：KCBR/KCFR（南/北端螺栓与钢筋测点）、KBR/KFR（南/北端节点），均为工程坐标系 9 列
        /// [E.X,E.Y,E.Z, C.X,C.Y,C.Z, W.X,W.Y,W.Z]。</para>
        /// </summary>
        private void ZeroErr(double[] E, double[] bo, bool useB, double A, double C, CalcResult calcRes, double[][] KK, double d, RotationParams cs,
            out double[] KCBR, out double[] KCFR, out double[] KBR, out double[] KFR)
        {
            // bo 固定 10 项布局（0 号段）：
            // [0]aNE [1]aNW [2]aSE [3]aSW [4]cN [5]cS [6]bNE [7]bNW [8]bSE [9]bSW
            double ANE = bo[0], ANW = bo[1], ASE = bo[2], ASW = bo[3];
            double CN = bo[4], CS = bo[5];
            double BNE = bo[6], BNW = bo[7], BSE = bo[8], BSW = bo[9];
            var theoreticalJointS = calcRes.TheoreticalJointS;
            var theoreticalJointN = calcRes.TheoreticalJointN;
            double matchSouthEastElev = calcRes.MatchSouthEastElev;
            double matchSouthWestElev = calcRes.MatchSouthWestElev;
            double matchNorthEastElev = calcRes.MatchNorthEastElev;
            double matchNorthWestElev = calcRes.MatchNorthWestElev;

            // KCB：南端(浮动端头侧)三个测点 [0..2]=East [3..5]=Center [6..8]=West。
            // 高程分量 = 理论Z + 实测高程 − 理论计算高程（把实测偏差叠加到理论位置上）。
            // a 始终使用四角落库值；旧口径由保存时写 aNW=aNE、aSW=aSE 保证，算法不做归一化分支。
            // b 分量按 useB 分支：true 直接取 b 四角值；false 保留修改前算法的中间量表达式。
            double[] KCB = new double[9];
            KCB[0] = E[3] - ASE;                                     // East测点X：实测东长 − 南东角纵向偏位
            KCB[1] = useB ? BSE : (E[2] + theoreticalJointS.E.Y);    // East测点Y：新=bSE；旧=浮动端头中心偏移+理论Y
            KCB[2] = theoreticalJointS.E.Z + E[4] - matchSouthEastElev; // East测点Z：理论Z + 实测高程 − 计算高程
            KCB[6] = E[1] - ASW;                                     // West测点X：实测西长 − 南西角纵向偏位
            KCB[7] = useB ? -BSW : (E[2] + theoreticalJointS.W.Y);   // West测点Y：新=−bSW；旧=浮动端头中心偏移+理论Y
            KCB[8] = theoreticalJointS.W.Z + E[6] - matchSouthWestElev; // West测点Z
            KCB[3] = (E[1] + E[3]) / 2.0 - CS;                       // Center测点X：东西实测均值 − 南中心偏位
            KCB[4] = theoreticalJointS.C.Y + E[12];                  // Center测点Y：理论Y + 实测南偏距
            KCB[5] = theoreticalJointS.C.Z;                          // Center测点Z：理论Z

            double[] KCF = new double[9];
            KCF[0] = theoreticalJointN.E.X + ANE;
            KCF[1] = useB ? BNE : theoreticalJointN.E.Y;
            KCF[2] = theoreticalJointN.E.Z + E[5] - matchNorthEastElev;
            KCF[6] = theoreticalJointN.W.X + ANW;
            KCF[7] = useB ? -BNW : theoreticalJointN.W.Y;
            KCF[8] = theoreticalJointN.W.Z + E[7] - matchNorthWestElev;
            KCF[3] = theoreticalJointN.C.X + CN;
            KCF[4] = theoreticalJointN.C.Y + E[13];
            KCF[5] = theoreticalJointN.C.Z;

            double[] KB = new double[9];
            KB[0] = E[3];
            KB[1] = KCB[1];
            KB[2] = E[8] - matchSouthEastElev;
            KB[6] = E[1];
            KB[7] = KCB[7];
            // 西角 Z 必须用西侧实测高程作基准（既有 bug 修复：原为 matchSouthEastElev），始终生效，不随 useB。
            KB[8] = E[9] - matchSouthWestElev;
            KB[3] = (KB[0] + KB[6]) / 2.0;
            KB[4] = E[2];
            KB[5] = KCB[5];

            double[] KF = new double[9];
            KF[0] = theoreticalJointN.E.X;
            KF[1] = theoreticalJointN.E.Y;
            KF[2] = theoreticalJointN.E.Z + E[10] - matchNorthEastElev;
            KF[6] = theoreticalJointN.W.X;
            KF[7] = theoreticalJointN.W.Y;
            KF[8] = theoreticalJointN.W.Z + E[11] - matchNorthWestElev;
            KF[3] = theoreticalJointN.C.X;
            KF[4] = theoreticalJointN.C.Y;
            KF[5] = theoreticalJointN.C.Z;

            KCBR = RevAxisCha(KK, d, KCB, cs);
            KCFR = RevAxisCha(KK, d, KCF, cs);
            KBR = RevAxisCha(KK, d, KB, cs);
            KFR = RevAxisCha(KK, d, KF, cs);
        }

        /// <summary>
        /// 【1 号段 / 标准段实测修正】OneErr。
        /// <para>业务：本段与上一段匹配浇筑。先把 16 项实测值转换成 24 项误差参数 rerr，
        /// 再以实测南端为基准做 <see cref="SpAxis"/> 姿态修正，最后经 <see cref="RevAxisCha"/> 回工程坐标系。</para>
        /// <para>入参获取：</para>
        /// <list type="bullet">
        /// <item><description><paramref name="E"/>：前端「设站及测量」录入的标准段 16 项实测值，顺序见 <see cref="MeasuredEStandardSegment"/>；</description></item>
        /// <item><description><paramref name="bo"/>：固定 13 项 [aNE1,aNW1,cN1,aNE2,aNW2,aSE2,aSW2,cN2,cS2,bNE2,bNW2,bSE2,bSW2]，
        /// 1=上一段北面、2=本段。aNE1/aNW1 直接取上一段落库值（保存时已按“两侧不一致”归一化）；</description></item>
        /// <item><description><paramref name="useB"/>：是否填写实测 b。false 时 b 走旧表达式
        /// （KCB[1]=actualPointN.E.Y+rerr[13]、KCF[1]=KCF[4]+B 等）；true 时直接取 b 四角值；</description></item>
        /// <item><description><paramref name="A"/>/<paramref name="C"/>/<paramref name="B"/>/<paramref name="H"/>：项目信息参数；
        /// <paramref name="calcRes"/>/<paramref name="KK"/>/<paramref name="d"/>/<paramref name="cs"/>：Calc/AxisCha 产出。</description></item>
        /// </list>
        /// <para>输出：KCBR/KCFR（南/北端螺栓与钢筋测点）、KFR（北端节点），工程坐标系 9 列
        /// [E.X,E.Y,E.Z, C.X,C.Y,C.Z, W.X,W.Y,W.Z]。</para>
        /// </summary>
        private void OneErr(double[] E, double[] bo, bool useB, double A, double C, double B, double H, CalcResult calcRes, double[][] KK, double d, RotationParams cs,
            out double[] KCBR, out double[] KCFR, out double[] KFR)
        {
            double n = 1e-200;
            // bo 固定 13 项布局（标准段）：
            // [0]aNE1 [1]aNW1 [2]cN1 [3]aNE2 [4]aNW2 [5]aSE2 [6]aSW2 [7]cN2 [8]cS2
            // [9]bNE2 [10]bNW2 [11]bSE2 [12]bSW2
            double ANE1 = bo[0], ANW1 = bo[1], CN1 = bo[2];
            double ANE2 = bo[3], ANW2 = bo[4], ASE2 = bo[5], ASW2 = bo[6];
            double CN2 = bo[7], CS2 = bo[8];
            double BNE2 = bo[9], BNW2 = bo[10], BSE2 = bo[11], BSW2 = bo[12];
            var theoreticalJointS = calcRes.TheoreticalJointS;
            var theoreticalJointN = calcRes.TheoreticalJointN;
            var matchJointN = calcRes.MatchJointN;
            var actualPointS = calcRes.ActualPointS;
            var actualPointN = calcRes.ActualPointN;

            Joint wetCastJoint;
            Joint matchCastJoint;
            if (Math.Abs(calcRes.WetCastSouthEastElev - (H + theoreticalJointS.E.Z)) < 1e-12)
            {
                wetCastJoint = theoreticalJointS;
                matchCastJoint = matchJointN;
            }
            else
            {
                wetCastJoint = matchJointN;
                matchCastJoint = theoreticalJointS;
            }

            double matchCenterLength = calcRes.MatchCenterLength;
            double matchSouthCenterOffset = calcRes.MatchSouthCenterOffset;
            double matchNorthCenterOffset = calcRes.MatchNorthCenterOffset;
            double wetCastSouthEastElev = calcRes.WetCastSouthEastElev;
            double wetCastSouthWestElev = calcRes.WetCastSouthWestElev;
            double wetCastNorthEastElev = calcRes.WetCastNorthEastElev;
            double wetCastNorthWestElev = calcRes.WetCastNorthWestElev;

            // ===== 24 项误差参数 rerr（下标含义）=====
            double[] rerr = new double[24];
            // rerr[0..3]：四角"实测高程 − 计算高程"误差（SE/SW/NE/NW）
            rerr[0] = E[6] - calcRes.MatchSouthEastElev;    // 南东角高程误差
            rerr[1] = E[7] - calcRes.MatchSouthWestElev;    // 南西角高程误差
            rerr[2] = E[8] - calcRes.MatchNorthEastElev;    // 北东角高程误差
            rerr[3] = E[9] - calcRes.MatchNorthWestElev;    // 北西角高程误差

            rerr[8] = (rerr[0] + rerr[1]) / 2.0;
            rerr[9] = (rerr[0] + rerr[3]) - (rerr[1] + rerr[2]);
            rerr[10] = (rerr[2] + rerr[3]) / 2.0;
            Console.WriteLine($"Twist:{rerr[9]}");
            // 给Twist赋值
            calcRes.MatchCastTwist = rerr[9];

            // rerr[11]：由南北误差线性外插到匹配断面处的推断高程误差（后续公式未使用）
            rerr[11] = (actualPointN.C.Z + rerr[10]) - ((rerr[8] - rerr[10]) / (actualPointS.C.X - actualPointN.C.X)) * (actualPointN.C.X - matchCastJoint.C.X);

            // rerr[4..7]：去除平均后的四角局部高程误差（北东/北西侧再叠加扭曲的一半）
            rerr[4] = rerr[0] - rerr[8];
            rerr[5] = rerr[1] - rerr[8];
            rerr[6] = rerr[2] + rerr[9] / 2.0 - rerr[10];
            rerr[7] = rerr[3] - rerr[9] / 2.0 - rerr[10];

            rerr[12] = E[1] - matchCenterLength;
            rerr[13] = E[3] - matchNorthCenterOffset;
            double dxNNSS = KK[4][3] - KK[3][3];
            rerr[16] = Math.Sqrt((actualPointN.C.X - actualPointS.C.X) * (actualPointN.C.X - actualPointS.C.X)
                + (actualPointN.C.Y - actualPointS.C.Y) * (actualPointN.C.Y - actualPointS.C.Y));
            rerr[17] = Math.Sqrt((actualPointN.C.X - actualPointS.C.X) * (actualPointN.C.X - actualPointS.C.X)
                + (actualPointN.C.Z - actualPointS.C.Z) * (actualPointN.C.Z - actualPointS.C.Z));
            rerr[20] = E[2] - matchSouthCenterOffset;
            rerr[21] = Math.Sqrt((actualPointS.W.Y - actualPointS.E.Y) * (actualPointS.W.Y - actualPointS.E.Y)
                + (actualPointS.W.Z - actualPointS.E.Z) * (actualPointS.W.Z - actualPointS.E.Z));

            // rerr[14]/[15]：绕 Z 轴水平转角的 sin/cos——由南北横向偏差差，按圆弧弦长公式
            //   sin(2·asin(δ/2L)) 求转角（δ=偏差差，L=rerr[16]），符号因子保证方向正确
            rerr[14] = Math.Sin(2 * Math.Asin(Math.Abs(rerr[13] - rerr[20]) / 2.0 / rerr[16]))
                * ((rerr[20] - rerr[13] + n) / Math.Abs(rerr[20] - rerr[13] + n))
                * ((d * dxNNSS + n) / Math.Abs(d * dxNNSS + n));

            rerr[18] = Math.Sin(2 * Math.Asin(Math.Abs(rerr[8] - rerr[10]) / 2.0 / rerr[17]))
                * ((rerr[8] - rerr[10] + n) / Math.Abs(rerr[8] - rerr[10] + n))
                * ((d * dxNNSS + n) / Math.Abs(d * dxNNSS + n));

            rerr[22] = Math.Sin(2 * Math.Asin(Math.Abs(rerr[6]) / rerr[21]))
                * ((rerr[6] + n) / Math.Abs(rerr[6] + n))
                * ((-actualPointN.E.Y + n) / Math.Abs(-actualPointN.E.Y + n));

            // 对应 cos 值：cos = sqrt(1 − sin²)
            rerr[15] = Math.Sqrt(1.0 - rerr[14] * rerr[14]);
            rerr[19] = Math.Sqrt(1.0 - rerr[18] * rerr[18]);
            rerr[23] = Math.Sqrt(1.0 - rerr[22] * rerr[22]);

            double[] KCB = new double[9];
            KCB[0] = actualPointN.E.X - ANE1 - ASE2 + actualPointN.E.Y * rerr[14];
            KCB[1] = useB ? BSE2 : (actualPointN.E.Y + rerr[13]);
            KCB[2] = wetCastJoint.E.Z + E[10] - wetCastSouthEastElev;

            KCB[3] = actualPointN.C.X - CN1 - CS2;
            KCB[4] = matchCastJoint.C.Y + E[14];
            KCB[5] = wetCastJoint.C.Z;

            KCB[6] = actualPointN.W.X - ANW1 - ASW2 + actualPointN.W.Y * rerr[14];
            KCB[7] = useB ? -BSW2 : (actualPointN.W.Y + rerr[13]);
            KCB[8] = wetCastJoint.W.Z + E[11] - wetCastSouthWestElev;

            double[] KCF = new double[9];
            KCF[3] = theoreticalJointN.C.X + CN2 - rerr[12];
            KCF[4] = theoreticalJointN.C.Y + E[15];
            KCF[5] = theoreticalJointN.C.Z;
            KCF[0] = KCF[3] - CN2 + ANE2;
            KCF[1] = useB ? BNE2 : (KCF[4] + B);
            KCF[2] = theoreticalJointN.E.Z + E[12] - wetCastNorthEastElev;
            KCF[6] = KCF[3] - CN2 + ANW2;
            KCF[7] = useB ? -BNW2 : (KCF[4] - B);
            KCF[8] = theoreticalJointN.W.Z + E[13] - wetCastNorthWestElev;

            double[] KF = new double[9];
            KF[0] = theoreticalJointN.E.X - rerr[12];
            KF[1] = theoreticalJointN.E.Y;
            KF[2] = theoreticalJointN.E.Z + E[4] - H;
            KF[3] = theoreticalJointN.C.X - rerr[12];
            KF[4] = theoreticalJointN.C.Y;
            KF[5] = theoreticalJointN.C.Z;
            KF[6] = theoreticalJointN.W.X - rerr[12];
            KF[7] = theoreticalJointN.W.Y;
            KF[8] = theoreticalJointN.W.Z + E[5] - H;

            // 以实测南端为基准做空间姿态修正（平移 + 三轴旋转 + 扭曲修正）
            double[] KCBr = SpAxis(actualPointS, rerr, KCB);
            double[] KCFr = SpAxis(actualPointS, rerr, KCF);
            double[] KFr = SpAxis(actualPointS, rerr, KF);

            // 逆变换回工程坐标系
            KCBR = RevAxisCha(KK, d, KCBr, cs);
            KCFR = RevAxisCha(KK, d, KCFr, cs);
            KFR = RevAxisCha(KK, d, KFr, cs);
        }

        private bool Judge(double[] k, int m, int s)
        {
            if (s == 0)
            {
                for (int i = 0; i < k.Length; i++)
                {
                    if (Math.Abs(k[i]) < 1e-12)
                    {
                        if (m - 1 <= i)
                        {
                            Console.WriteLine("判定通过！");
                            return true;
                        }
                        else
                        {
                            Console.WriteLine($"判定不通过！当前记录允许计算第{i + 1}节段");
                            return false;
                        }
                    }
                }
                return true;
            }
            else
            {
                if (m == 0)
                {
                    if (k.Length == 14)
                    {
                        Console.WriteLine("实测数据输入正确");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("请检查实测数据输入格式！");
                        return false;
                    }
                }
                else
                {
                    if (k.Length == 16)
                    {
                        Console.WriteLine("实测数据输入正确");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("请检查实测数据输入格式！");
                        return false;
                    }
                }
            }
        }

        public Joint ArrayToJoint(double[] arr)
        {
            return new Joint(
                new Vec3(arr[0], arr[1], arr[2]),
                new Vec3(arr[3], arr[4], arr[5]),
                new Vec3(arr[6], arr[7], arr[8])
            );
        }
        /// <summary>Joint 深拷贝（E/C/W 与坐标值全部复制）</summary>
        public Joint DeepCopyJoint(Joint src)
        {
            return new Joint(
                new Vec3(src.E.X, src.E.Y, src.E.Z),
                new Vec3(src.C.X, src.C.Y, src.C.Z),
                new Vec3(src.W.X, src.W.Y, src.W.Z)
            );
        }
        public double[][] DeepCopy2D(double[][] src)
        {
            double[][] res = new double[src.Length][];
            for (int i = 0; i < src.Length; i++)
                res[i] = (double[])src[i].Clone();
            return res;
        }
        /// <summary>三维空间两点距离（欧氏距离）</summary>
        public double Dist3D(double x1, double y1, double z1, double x2, double y2, double z2)
        {
            double dx = x1 - x2;
            double dy = y1 - y2;
            double dz = z1 - z2;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }
        #endregion

        #region PrintReport 几何报告算法（新增，实例公开入口 + 私有几何工具）
        /// <summary>
        /// 等价Python PR(K)
        /// </summary>
        /// <param name="K">double[][]，每行 [E.X,E.Y,E.Z, C.X,C.Y,C.Z, W.X,W.Y,W.Z]</param>
        /// <returns>列表，第一项为表头，后续为ReportRow.ToObjectRow()</returns>
        public List<object> PrintReport(double[][] K)
        {
            if (K == null) throw new ArgumentNullException(nameof(K));
            var result = new List<object>();
            result.Add(ReportRow.GetHeader());

            int rowCount = K.Length;
            for (int i = 0; i < rowCount; i++)
            {
                var r = K[i];
                if (r == null || r.Length != 9)
                    throw new InvalidOperationException($"K[{i}]行数据异常，必须9个元素");

                double Ex = r[0], Ey = r[1], Ez = r[2];
                double Cx = r[3], Cy = r[4], Cz = r[5];
                double Wx = r[6], Wy = r[7], Wz = r[8];

                double BE = Dist3(Ex, Ey, Ez, Cx, Cy, Cz);
                double BW = Dist3(Wx, Wy, Wz, Cx, Cy, Cz);

                ReportRow row = new ReportRow
                {
                    Index = i + 1, // 从1开始
                    BE = BE,
                    BW = BW
                };

                if (i == 0)
                {
                    // 首行特判（与 Python 一致）：折角/偏距/竖向趋势无参照记 NaN；
                    // 横坡分母用水平距离 BEL/BWL（后续行用空间距离 BE/BW，口径不同系原版如此）
                    double BEL = Dist2(Ex, Ey, Cx, Cy);
                    double BWL = Dist2(Wx, Wy, Cx, Cy);

                    row.AE = null;
                    row.AW = null;
                    row.LOE = null;
                    row.LOW = null;
                    row.ZA = "NaN";

                    row.CSE = BEL < Epsilon ? double.NaN : (Ez - Cz) / BEL;
                    row.CSW = BWL < Epsilon ? double.NaN : (Cz - Wz) / BWL;
                }
                else
                {
                    var prev = K[i - 1];
                    double pEx = prev[0], pEy = prev[1], pEz = prev[2];
                    double pCx = prev[3], pCy = prev[4], pCz = prev[5];
                    double pWx = prev[6], pWy = prev[7], pWz = prev[8];

                    double[] L1 = { pCx - pEx, pCy - pEy, pCz - pEz };
                    double[] L2 = { Ex - pEx, Ey - pEy, Ez - pEz };
                    double[] L3 = { pCx - pWx, pCy - pWy, pCz - pWz };
                    double[] L4 = { Wx - pWx, Wy - pWy, Wz - pWz };

                    double aE = CalculateAngle(L1, L2);
                    double aW = CalculateAngle(L3, L4);

                    // LC=上一端面C点→当前C点的中心线向量；LOE/LOW=LC 与侧边线 LE/LW 的投影（点积/|LC|）
                    double[] LC = { pCx - Cx, pCy - Cy, pCz - Cz };
                    double normLc = Norm(LC); // Math.Sqrt(LC[0] * LC[0] + LC[1] * LC[1] + LC[2] * LC[2]);

                    double[] LE = { Ex - Cx, Ey - Cy, Ez - Cz };
                    double[] LW = { Wx - Cx, Wy - Cy, Wz - Cz };

                    double loe = double.NaN;
                    double low = double.NaN;
                    if (normLc > Epsilon)
                    {
                        loe = (LC[0] * LE[0] + LC[1] * LE[1] + LC[2] * LE[2]) / normLc;
                        low = (LC[0] * LW[0] + LC[1] * LW[1] + LC[2] * LW[2]) / normLc;
                    }

                    row.AE = double.IsNaN(aE) ? null : (double?)aE;
                    row.AW = double.IsNaN(aW) ? null : (double?)aW;
                    row.LOE = double.IsNaN(loe) ? null : (double?)loe;
                    row.LOW = double.IsNaN(low) ? null : (double?)low;

                    // 横坡：高差/空间距离；ZA：与上一行 C 点 Z 比较，上升记 Upward，否则 Check
                    row.CSE = BE < Epsilon ? double.NaN : (Ez - Cz) / BE;
                    row.CSW = BW < Epsilon ? double.NaN : (Cz - Wz) / BW;

                    row.ZA = (pCz < Cz) ? "Upward" : "Check";
                }
                result.Add(row.ToObjectRow());
            }
            return result;
        }
        // 三维向量点积
        private static double Dot(double[] a, double[] b)
        {
            return a[0] * b[0] + a[1] * b[1] + a[2] * b[2];
        }
        // 向量L2范数，等价np.linalg.norm，和侧Norm完全镜像
        private static double Norm(double[] vec)
        {
            return Math.Sqrt(Dot(vec, vec));
        }
        // 三维向量点积 decimal版本
        private static decimal DotDec(decimal[] a, decimal[] b)
        {
            return a[0] * b[0] + a[1] * b[1] + a[2] * b[2];
        }

        // 向量L2范数，等价np.linalg.norm，和double侧Norm完全镜像
        private static double NormDec(decimal[] vec)
        {
            decimal sqSum = DotDec(vec, vec);
            return Math.Sqrt((double)sqSum);
        }
        /// <summary>List&lt;Joint&gt;重载入口</summary>
        public List<object> PrintReport(List<Joint> jointList)
        {
            if (jointList == null) throw new ArgumentNullException(nameof(jointList));
            int cnt = jointList.Count;
            double[][] K = new double[cnt][];
            for (int i = 0; i < cnt; i++)
            {
                var j = jointList[i];
                j.ThrowIfInvalid(i);
                K[i] = new double[9]
                {
                    j.E.X, j.E.Y, j.E.Z,
                    j.C.X, j.C.Y, j.C.Z,
                    j.W.X, j.W.Y, j.W.Z
                };
            }
            return PrintReport(K);
        }

        /// <summary>decimal[][]版本入口</summary>
        public List<object> PrintReportDecimal(decimal[][] K)
        {
            if (K == null) throw new ArgumentNullException(nameof(K));
            var result = new List<object>();
            result.Add(ReportRow.GetHeader());
            int rowCount = K.Length;

            for (int i = 0; i < rowCount; i++)
            {
                var r = K[i];
                if (r == null || r.Length != 9)
                    throw new InvalidOperationException($"K[{i}]行数据异常，必须9个元素");

                decimal Ex = r[0], Ey = r[1], Ez = r[2];
                decimal Cx = r[3], Cy = r[4], Cz = r[5];
                decimal Wx = r[6], Wy = r[7], Wz = r[8];

                decimal BE = Dist3Dec(Ex, Ey, Ez, Cx, Cy, Cz);
                decimal BW = Dist3Dec(Wx, Wy, Wz, Cx, Cy, Cz);

                ReportRow row = new ReportRow
                {
                    Index = i,
                    BE = (double)BE,
                    BW = (double)BW
                };

                if (i == 0)
                {
                    decimal BEL = Dist2Dec(Ex, Ey, Cx, Cy);
                    decimal BWL = Dist2Dec(Wx, Wy, Cx, Cy);
                    row.AE = null;
                    row.AW = null;
                    row.LOE = null;
                    row.LOW = null;
                    row.ZA = "NaN";

                    row.CSE = BEL < DecEpsilon ? double.NaN : (double)((Ez - Cz) / BEL);
                    row.CSW = BWL < DecEpsilon ? double.NaN : (double)((Cz - Wz) / BWL);
                }
                else
                {
                    var prev = K[i - 1];
                    decimal pEx = prev[0], pEy = prev[1], pEz = prev[2];
                    decimal pCx = prev[3], pCy = prev[4], pCz = prev[5];
                    decimal pWx = prev[6], pWy = prev[7], pWz = prev[8];

                    decimal[] L1 = { pCx - pEx, pCy - pEy, pCz - pEz };
                    decimal[] L2 = { Ex - pEx, Ey - pEy, Ez - pEz };
                    decimal[] L3 = { pCx - pWx, pCy - pWy, pCz - pWz };
                    decimal[] L4 = { Wx - pWx, Wy - pWy, Wz - pWz };

                    double aE = CalculateAngleDec(L1, L2);
                    double aW = CalculateAngleDec(L3, L4);

                    decimal[] LC = { pCx - Cx, pCy - Cy, pCz - Cz };
                    decimal[] LE = { Ex - Cx, Ey - Cy, Ez - Cz };
                    decimal[] LW = { Wx - Cx, Wy - Cy, Wz - Cz };

                    double normLc = NormDec(LC);

                    double loe = double.NaN;
                    double low = double.NaN;
                    if (normLc > (double)DecEpsilon)
                    {
                        decimal dotLcLe = DotDec(LC, LE);
                        decimal dotLcLw = DotDec(LC, LW);
                        loe = (double)dotLcLe / normLc;
                        low = (double)dotLcLw / normLc;
                    }

                    row.AE = double.IsNaN(aE) ? null : (double?)aE;
                    row.AW = double.IsNaN(aW) ? null : (double?)aW;
                    row.LOE = double.IsNaN(loe) ? null : (double?)loe;
                    row.LOW = double.IsNaN(low) ? null : (double?)low;

                    row.CSE = BE < DecEpsilon ? double.NaN : (double)((Ez - Cz) / BE);
                    row.CSW = BW < DecEpsilon ? double.NaN : (double)((Cz - Wz) / BW);

                    row.ZA = (pCz < Cz) ? "Upward" : "Check";
                }
                result.Add(row.ToObjectRow());
            }
            return result;
        }

        //====私有几何辅助工具====
        private static double CalculateAngle(double[] L1, double[] L2)
        {
            double dot = L1[0] * L2[0] + L1[1] * L2[1] + L1[2] * L2[2];
            double n1 = Math.Sqrt(L1[0] * L1[0] + L1[1] * L1[1] + L1[2] * L1[2]);
            double n2 = Math.Sqrt(L2[0] * L2[0] + L2[1] * L2[1] + L2[2] * L2[2]);
            if (n1 < Epsilon || n2 < Epsilon)
                return double.NaN;
            double cosTheta = dot / (n1 * n2);
            cosTheta = Math.Clamp(cosTheta, -1.0, 1.0);
            double rad = Math.Acos(cosTheta);
            return rad * 180.0 / Math.PI;
        }
        /// <summary>
        /// 三维点距
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="z1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="z2"></param>
        /// <returns></returns>
        private static double Dist3(double x1, double y1, double z1, double x2, double y2, double z2)
        {
            double dx = x1 - x2;
            double dy = y1 - y2;
            double dz = z1 - z2;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }
        /// <summary>
        /// 二维点距
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <returns></returns>

        private static double Dist2(double x1, double y1, double x2, double y2)
        {
            double dx = x1 - x2;
            double dy = y1 - y2;
            return Math.Sqrt(dx * dx + dy * dy);
        }
        /// <summary>
        /// 三维点距decimal版
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="z1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="z2"></param>
        /// <returns></returns>
        private static decimal Dist3Dec(decimal x1, decimal y1, decimal z1, decimal x2, decimal y2, decimal z2)
        {
            decimal dx = x1 - x2;
            decimal dy = y1 - y2;
            decimal dz = z1 - z2;
            return (decimal)Math.Sqrt((double)(dx * dx + dy * dy + dz * dz));
        }
        /// <summary>平面两点距离（XY 面欧氏距离），decimal 版</summary>
        private static decimal Dist2Dec(decimal x1, decimal y1, decimal x2, decimal y2)
        {
            decimal dx = x1 - x2;
            decimal dy = y1 - y2;
            return (decimal)Math.Sqrt((double)(dx * dx + dy * dy));
        }

        /// <summary>CalculateAngle 的 decimal 版（内部转 double 求角度，结果与 double 版一致）</summary>
        private static double CalculateAngleDec(decimal[] L1, decimal[] L2)
        {
            decimal dot = L1[0] * L2[0] + L1[1] * L2[1] + L1[2] * L2[2];
            decimal n1Sq = L1[0] * L1[0] + L1[1] * L1[1] + L1[2] * L1[2];
            decimal n2Sq = L2[0] * L2[0] + L2[1] * L2[1] + L2[2] * L2[2];
            double n1 = Math.Sqrt((double)n1Sq);
            double n2 = Math.Sqrt((double)n2Sq);
            if (n1 < (double)DecEpsilon || n2 < (double)DecEpsilon)
                return double.NaN;
            double cosTheta = (double)dot / (n1 * n2);
            cosTheta = Math.Clamp(cosTheta, -1.0, 1.0);
            double rad = Math.Acos(cosTheta);
            return rad * 180.0 / Math.PI;
        }
        #endregion

    }

    #region 实体定义
    /// <summary>三维坐标点（X=顺桥向，Y=横桥向，Z=竖向；单位米）</summary>
    public class Vec3
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public Vec3()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }
        public Vec3(decimal x = 0, decimal y = 0, decimal z = 0)
        {
            X = (double)x;
            Y = (double)y;
            Z = (double)z;
        }
        public Vec3(double x = 0, double y = 0, double z = 0)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>Vec3自身有效性校验（值类型X/Y/Z无null，永远有效）</summary>
        /// <returns>总是true，保留接口用于统一链式校验</returns>
        public bool IsValid()
        {
            // X/Y/Z为double值类型，不存在null；业务如果需要可扩展NaN/Infinity校验
            return !(double.IsNaN(X) || double.IsInfinity(X)
                     || double.IsNaN(Y) || double.IsInfinity(Y)
                     || double.IsNaN(Z) || double.IsInfinity(Z));
        }

        /// <summary>不满足IsValid则抛出异常</summary>
        /// <param name="memberName">字段名，用于异常提示，例如 nameof(Joint.E)</param>
        public void ThrowIfInvalid(string memberName)
        {
            if (!IsValid())
            {
                throw new InvalidOperationException($"{memberName} 的坐标包含 NaN 或无穷大数值：X={X},Y={Y},Z={Z}");
            }
        }
    }


    /// <summary>一个端面的三个特征点：E=East(东侧角点)、C=Center(中心点)、W=West(西侧角点)。
    /// 与 9 列数组布局 [E.XYZ, C.XYZ, W.XYZ] 一一对应（见 ArrayToJoint）。</summary>
    public class Joint
    {
        public Vec3 E { get; set; }   // 东侧角点
        public Vec3 C { get; set; }   // 中心点
        public Vec3 W { get; set; }   // 西侧角点

        public Joint(decimal[] array)
        {
            if (array.Length != 9) throw new Exception($"array数组长度应为9");
            if (array.Any(a => a == null)) throw new Exception($"array数组的任一值不可为null");
            E = new Vec3(array[0], array[1], array[2]);
            C = new Vec3(array[3], array[4], array[5]);
            W = new Vec3(array[6], array[7], array[8]);
        }
        public Joint(double[] array)
        {
            if (array.Length != 9) throw new Exception($"array数组长度应为9");
            if (array.Any(a => a == null)) throw new Exception($"array数组的任一值不可为null");
            E = new Vec3(array[0], array[1], array[2]);
            C = new Vec3(array[3], array[4], array[5]);
            W = new Vec3(array[6], array[7], array[8]);
        }
        public Joint(Vec3 e = null, Vec3 c = null, Vec3 w = null)
        {
            E = e ?? new Vec3();
            C = c ?? new Vec3();
            W = w ?? new Vec3();
        }

        /// <summary>校验Joint对象：E/C/W不为null，且每个Vec3自身IsValid()</summary>
        public bool IsValid(out string errorMessage)
        {
            errorMessage = string.Empty;
            if (E == null)
            {
                errorMessage = "Joint.E 不能为null";
                return false;
            }
            if (C == null)
            {
                errorMessage = "Joint.C 不能为null";
                return false;
            }
            if (W == null)
            {
                errorMessage = "Joint.W 不能为null";
                return false;
            }
            if (!E.IsValid())
            {
                errorMessage = "Joint.E 包含NaN/无穷大";
                return false;
            }
            if (!C.IsValid())
            {
                errorMessage = "Joint.C 包含NaN/无穷大";
                return false;
            }
            if (!W.IsValid())
            {
                errorMessage = "Joint.W 包含NaN/无穷大";
                return false;
            }
            return true;
        }

        /// <summary>无out参数重载，仅返回bool</summary>
        public bool IsValid()
        {
            return IsValid(out _);
        }

        /// <summary>校验失败直接抛出异常，带上集合索引，用于集合循环校验场景</summary>
        /// <param name="index">集合中的索引，-1代表无索引</param>
        public void ThrowIfInvalid(int index = -1)
        {
            if (!IsValid(out var msg))
            {
                if (index >= 0)
                {
                    throw new InvalidOperationException($"索引[{index}]：{msg}");
                }
                else
                {
                    throw new InvalidOperationException(msg);
                }
            }
        }

        public double[] ToArray()
        {
            ThrowIfInvalid();
            return new double[] { E.X, E.Y, E.Z, C.X, C.Y, C.Z, W.X, W.Y, W.Z };
        }
    }


    public class RotationParams
    {
        public double sin1 { get; set; }
        public double cos1 { get; set; }
        public double sin2 { get; set; }
        public double cos2 { get; set; }
        public double sin3 { get; set; }
        public double cos3 { get; set; }
    }

    /// <summary>
    /// 一次 Run 的立模/放样产出。字段按「归属端面」分三组，命名与 MeasuredE* 实测实体的词表一致：
    ///   Match*        —— 匹配端（上一梁段）的 as-cast 端面数据；首段无匹配段，取本段设计端面（沿用原口径）
    ///   WetCast*      —— 本段（浇筑段）的目标端面数据
    ///   *Bulkhead*    —— 端头模板目标值：FloatingBulkhead=活动端（South 侧浮动端头），FixedBulkhead=固定端（North 侧）
    /// </summary>
    public class CalcResult
    {
        /// <summary>匹配端中心长度</summary>
        public double MatchCenterLength { get; set; }
        /// <summary>匹配端南侧中心偏移量</summary>
        public double MatchSouthCenterOffset { get; set; }
        /// <summary>匹配端北侧中心偏移量</summary>
        public double MatchNorthCenterOffset { get; set; }
        /// <summary>匹配端南端面东侧高程</summary>
        public double MatchSouthEastElev { get; set; }
        /// <summary>匹配端南端面西侧高程</summary>
        public double MatchSouthWestElev { get; set; }
        /// <summary>匹配端北端面东侧高程</summary>
        public double MatchNorthEastElev { get; set; }
        /// <summary>匹配端北端面西侧高程</summary>
        public double MatchNorthWestElev { get; set; }

        /// <summary>浇筑段南端面东侧高程</summary>
        public double WetCastSouthEastElev { get; set; }
        /// <summary>浇筑段南端面西侧高程</summary>
        public double WetCastSouthWestElev { get; set; }
        /// <summary>浇筑段北端面东侧高程</summary>
        public double WetCastNorthEastElev { get; set; }
        /// <summary>浇筑段北端面西侧高程</summary>
        public double WetCastNorthWestElev { get; set; }
        /// <summary>浇筑段南端面中心偏移量</summary>
        public double WetCastSouthCenterOffset { get; set; }
        /// <summary>浇筑段北端面中心偏移量</summary>
        public double WetCastNorthCenterOffset { get; set; }
        /// <summary>浇筑段东侧长度（局部系下南端面东点纵向坐标）</summary>
        public double WetCastEastLength { get; set; }
        /// <summary>浇筑段西侧长度（局部系下南端面西点纵向坐标）</summary>
        public double WetCastWestLength { get; set; }
        /// <summary>
        /// 浇筑段中线长度（局部系下南端面中心点纵向坐标，与 WetCastEast/WestLength 同源同口径）。
        /// 局部系以北端设计面为原点、轴向指向南为正，故该值即「南端面到北端设计面的中线距离」＝本段设计中线长。
        /// </summary>
        public double WetCastCenterLength { get; set; }

        /// <summary>活动端模板（Floating Bulkhead，South 侧浮动端头）东侧高程目标值</summary>
        public double FloatingBulkheadEastElev { get; set; }
        /// <summary>活动端模板（Floating Bulkhead）西侧高程目标值</summary>
        public double FloatingBulkheadWestElev { get; set; }
        /// <summary>活动端模板（Floating Bulkhead）中心偏移量目标值</summary>
        public double FloatingBulkheadCenterOffset { get; set; }

        /// <summary>固定端模板（Fixed Bulkhead，North 侧）东侧高程目标值</summary>
        public double FixedBulkheadEastElev { get; set; }
        /// <summary>固定端模板（Fixed Bulkhead）西侧高程目标值</summary>
        public double FixedBulkheadWestElev { get; set; }
        /// <summary>固定端模板（Fixed Bulkhead）中心偏移量目标值</summary>
        public double FixedBulkheadCenterOffset { get; set; }

        /// <summary>
        /// 匹配浇筑扭转：匹配端与当前浇筑端之间的扭转量（四角高程误差的对角差），
        /// 是唯一的跨端面耦合量，故不归入 Match* / WetCast* 任一组。0 号段与 St=0 时恒为 0。
        /// </summary>
        public double MatchCastTwist { get; set; }

        /// <summary>
        /// 南侧端面设计值
        /// </summary>
        public Joint TheoreticalJointS { get; set; }
        /// <summary>
        /// 北侧端面设计值
        /// </summary>
        public Joint TheoreticalJointN { get; set; }
        /// <summary>
        /// 匹配南侧端面
        /// </summary>
        public Joint MatchJointN { get; set; }
        /// <summary>
        /// 南侧端面实测点位
        /// </summary>
        public Joint ActualPointS { get; set; }
        /// <summary>
        /// 北侧端面实测点位
        /// </summary>
        public Joint ActualPointN { get; set; }
        /// <summary>
        /// 传递用Sin/Cos集合
        /// </summary>
        public RotationParams cs { get; set; }
        public CalcResult()
        {
            TheoreticalJointS = new Joint();
            TheoreticalJointN = new Joint();
            MatchJointN = new Joint();
            ActualPointS = new Joint();
            ActualPointN = new Joint();
            cs = new RotationParams();
        }
    }

    /// <summary>
    /// double 版计算入参（对应 Python main 的全部入参）。
    /// <para>调用方装配建议（C# 侧 ConcreteBeamService）：</para>
    /// <list type="bullet">
    /// <item><description>K ← biz_concrete_beam_design_coordinate_calculation（设计坐标 + 预拱度），按端面顺序取 K[m]、K[m+1]；</description></item>
    /// <item><description>KT ← biz_concrete_beam_ascast_coordinate（前序算法产出的 as-cast 端面）；</description></item>
    /// <item><description>KCT ← biz_concrete_beam_ascast_measure（前序算法产出的测点，每节段南/北两行）；</description></item>
    /// <item><description>ZerothMeasuredE / StandardMeasuredE ← 前端 SetupNSurveyDTO 的实测录入（0 号段 14 项 / 标准段 16 项）；</description></item>
    /// <item><description>Bo ← biz_concrete_beam_segment 的 BoltOffset* 列；UseB ← BoltOffsetBUseMeasured。</description></item>
    /// </list>
    /// </summary>
    public class ComputeInput
    {
        /// <summary>设计/理论端面坐标表（每行一个端面，9 列 [E,C,W]×XYZ）。来源：design_coordinate_calculation。</summary>
        public double[][] K { get; set; }
        /// <summary>
        /// 现场拼装（实测/修正后）端面坐标表，与 K 同构。来源：ascast_coordinate（前序算法产出）。
        /// </summary>
        public double[][] KT { get; set; }
        /// <summary>
        /// 实测测点坐标表（每个已浇节段南/北两端各一行，共 2m 行）。来源：ascast_measure（前序算法产出）。
        /// </summary>
        public double[][] KCT { get; set; }
        /// <summary>
        /// 根据List<Joint>翻译成算法用的double[][]
        /// </summary>
        /// <param name="list"></param>
        public void SetK(List<Joint> list)
        {
            K = ConvertJointToDouble(list);
        }
        public List<Joint> GetK()
        {
            return GetJointInput(K);
        }
        /// <summary>
        /// 根据List<Joint>翻译成算法用的double[][]
        /// </summary>
        /// <param name="list"></param>
        public void SetKT(List<Joint> list)
        {
            KT = ConvertJointToDouble(list);
        }
        /// <summary>
        /// 获取实测端面坐标的List<Joint>格式
        /// </summary>
        /// <returns></returns>
        public List<Joint> GetKT()
        {
            return GetJointInput(KT);
        }
        /// <summary>
        /// 根据List<Joint>翻译成算法用的double[][]
        /// </summary>
        /// <param name="list"></param>
        public void SetKCT(List<Joint> list)
        {
            KCT = ConvertJointToDouble(list);
        }
        /// <summary>
        /// 获取实测测点坐标的List<Joint>格式
        /// </summary>
        /// <returns></returns>
        public List<Joint> GetKCT()
        {
            return GetJointInput(KCT);
        }
        /// <summary>
        /// 将Joint集合转为算法使用的double[][]二维数组
        /// 防御校验：集合非空、元素非null、Joint.E/C/W 全部不为null，检测NaN/无穷大
        /// </summary>
        /// <param name="jointInput">Joint集合</param>
        /// <returns>转换后的二维数组</returns>
        /// <exception cref="ArgumentNullException">jointInput为null</exception>
        /// <exception cref="InvalidOperationException">集合元素或内部E/C/W为null，或坐标含非法浮点数</exception>
        public double[][] ConvertJointToDouble(List<Joint> jointInput)
        {
            if (jointInput == null)
                throw new ArgumentNullException(nameof(jointInput), "Joint列表不能为null");

            var doubleInput = new double[jointInput.Count][];
            for (var i = 0; i < jointInput.Count; i++)
            {
                var item = jointInput[i];
                if (item == null)
                {
                    throw new InvalidOperationException($"Joint列表索引[{i}]的元素为null，不允许传入null的Joint对象");
                }
                // 复用Joint内部自查，不再重复写E/C/W判空
                // item.ThrowIfInvalid(i);

                var kitem = item.ToArray();
                doubleInput[i] = kitem;
            }
            return doubleInput;
        }

        /// <summary>
        /// 根据double[][]格式转化位List<Joint>格式
        /// </summary>
        /// <param name="doubleInput"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public List<Joint> GetJointInput(double[][] doubleInput)
        {
            if (doubleInput == null) throw new ArgumentNullException();
            var list = new List<Joint>();
            for (var i = 0; i < doubleInput.Length; i++)
            {
                var data = doubleInput[i];
                if (data == null) continue;
                var item = new Joint()
                {
                    E = new Vec3() { X = data[0], Y = data[1], Z = data[2] },
                    C = new Vec3() { X = data[3], Y = data[4], Z = data[5] },
                    W = new Vec3() { X = data[6], Y = data[7], Z = data[8] }
                };
                list.Add(item);
            }
            return list;
        }

        /// <summary>项目信息偏位 A（米）。来源：biz_concrete_beam_config.OffsetA。</summary>
        public double A { get; set; }
        /// <summary>项目信息偏位 B（米）。来源：biz_concrete_beam_config.OffsetB；UseB=false 时作为旧 b 公式的常量。</summary>
        public double B { get; set; }
        /// <summary>项目信息偏位 C（米）。来源：biz_concrete_beam_config.OffsetC。</summary>
        public double C { get; set; }
        /// <summary>参考高程 H（米）。来源：biz_concrete_beam_config.OffsetH。</summary>
        public double H { get; set; }

        /// <summary>当前梁段几何序号（0 起），等于 biz_concrete_beam_segment.SegmentIndex，也是跨端面 m..m+1 的 m。</summary>
        public int M { get; set; }
        /// <summary>0=理论放样（PerfectCast，不做实测修正）；1=实测修正（SaveSurvey，输出浇筑坐标）。</summary>
        public int St { get; set; }

        /// <summary>
        /// 测点偏位数组（固定 10/13 项，不随 UseB 变化）。
        /// <para>0 号段（10 项）：[aNE, aNW, aSE, aSW, cN, cS, bNE, bNW, bSE, bSW]；</para>
        /// <para>标准段（13 项）：[aNE1, aNW1, cN1, aNE2, aNW2, aSE2, aSW2, cN2, cS2, bNE2, bNW2, bSE2, bSW2]。</para>
        /// 数据来源：biz_concrete_beam_segment 的 BoltOffset* 列（上一段取 prev 行，本段取当前行）。
        /// a 始终取落库的四角值；b 是否参与计算由 <see cref="UseB"/> 决定。
        /// </summary>
        public double[] Bo { get; set; }

        /// <summary>
        /// 是否填写实测 b（对应 biz_concrete_beam_segment.BoltOffsetBUseMeasured）。
        /// <para>false（默认）：b 相关计算走修改前算法（ZeroErr/OneErr 内的旧表达式）；</para>
        /// <para>true：使用 Bo 中的 bNE/bNW/bSE/bSW 四角值（绝对横向坐标，东 +、西 -）。</para>
        /// 注意：a 值不受此开关影响，始终使用 Bo 中的 aNE/aNW/aSE/aSW。
        /// </summary>
        public bool UseB { get; set; }
        /// <summary>向后兼容，原始E数组（14/16 项，来源同 <see cref="ZerothMeasuredE"/>/<see cref="StandardMeasuredE"/>）</summary>
        public double[] E { get; set; }

        /// <summary>0号段m=0专用实测E实体</summary>
        public MeasuredEZerothSegment ZerothMeasuredE { get; set; }
        /// <summary>标准段m≠0专用实测E实体</summary>
        public MeasuredEStandardSegment StandardMeasuredE { get; set; }
    }

    /// <summary>
    /// 计算结果。C# 侧消费去向（ConcreteBeamService.SaveSurvey）：
    /// KT → ascast_coordinate（端面级）、KCT → ascast_measure（测点级）、
    /// CalcResult → biz_concrete_beam_segment 的 Match*/WetCast*/Bulkhead* 字段、Err* → 偏差曲线/报告。
    /// </summary>
    public class ComputeOutput
    {
        /// <summary>算法判定是否通过；false 时调用方不应落库。</summary>
        public bool JudgePass { get; set; }
        /// <summary>算法日志（供排查；业务文案由调用方按需双语化）。</summary>
        public List<string> LogMessage { get; set; } = new List<string>();
        /// <summary>设计坐标（大地坐标），原样回传。</summary>
        public double[][] K { get; set; }
        /// <summary>修正后的 as-cast 端面坐标（大地坐标）。去向：ascast_coordinate。</summary>
        public double[][] KT { get; set; }
        /// <summary>修正后的实测测点坐标（大地坐标）。去向：ascast_measure。</summary>
        public double[][] KCT { get; set; }
        /// <summary>
        /// X 向(长度方向)偏差
        /// </summary>
        public double[] ErrL { get; set; }
        /// <summary>
        /// Y 向(水平)偏差
        /// </summary>
        public double[] ErrHP { get; set; }
        /// <summary>
        /// Z 向(高程)偏差（中心测点，旧口径）
        /// </summary>
        public double[] ErrH { get; set; }
        /// <summary>
        /// 东侧测点 Z 向高程偏差（新口径：KT[i][2] − K[i][2]）
        /// </summary>
        public double[] ErrHE { get; set; }
        /// <summary>
        /// 中心测点 Z 向高程偏差（新口径：KT[i][5] − K[i][5]）
        /// </summary>
        public double[] ErrHC { get; set; }
        /// <summary>
        /// 西侧测点 Z 向高程偏差（新口径：KT[i][8] − K[i][8]）
        /// </summary>
        public double[] ErrHW { get; set; }
        /// <summary>
        /// 设计中心长度
        /// </summary>
        public double TheoreticalCenterLength { get; set; }
        /// <summary>
        /// 设计中心总长
        /// </summary>
        public double TheoreticalTotalCenterLength  { get; set; }
        /// <summary>
        /// 浇筑中心长度
        /// </summary>
        public double AsCastCenterLength { get; set; }
        /// <summary>
        /// 浇筑中心总长
        /// </summary>
        public double AsCastTotalCenterLength { get; set; }
        /// <summary>
        /// 算法产出的立模参数。去向：biz_concrete_beam_segment 的 Match*/WetCast*/Bulkhead*/MatchCastTwist 字段。
        /// </summary>
        public CalcResult CalcResult { get; set; }
    }

    public class CalcResultDecimal
    {
        public decimal MatchCenterLength { get; set; }
        public decimal MatchSouthCenterOffset { get; set; }
        public decimal MatchNorthCenterOffset { get; set; }
        public decimal MatchSouthEastElev { get; set; }
        public decimal MatchSouthWestElev { get; set; }
        public decimal MatchNorthEastElev { get; set; }
        public decimal MatchNorthWestElev { get; set; }

        public decimal WetCastSouthEastElev { get; set; }
        public decimal WetCastSouthWestElev { get; set; }
        public decimal WetCastNorthEastElev { get; set; }
        public decimal WetCastNorthWestElev { get; set; }
        public decimal WetCastSouthCenterOffset { get; set; }
        public decimal WetCastNorthCenterOffset { get; set; }
        public decimal WetCastEastLength { get; set; }
        public decimal WetCastWestLength { get; set; }
        public decimal WetCastCenterLength { get; set; }

        public decimal FloatingBulkheadEastElev { get; set; }
        public decimal FloatingBulkheadWestElev { get; set; }
        public decimal FloatingBulkheadCenterOffset { get; set; }

        public decimal FixedBulkheadEastElev { get; set; }
        public decimal FixedBulkheadWestElev { get; set; }
        public decimal FixedBulkheadCenterOffset { get; set; }

        public decimal MatchCastTwist { get; set; }
    }

    public class ComputeInputDecimal
    {
        public decimal[][] K { get; set; }
        public decimal[][] KT { get; set; }
        public decimal[][] KCT { get; set; }

        public decimal A { get; set; }
        public decimal B { get; set; }
        public decimal C { get; set; }
        public decimal H { get; set; }

        public int M { get; set; }
        public int St { get; set; }

        public decimal[] Bo { get; set; }

        /// <summary>
        /// 是否填写实测 b（对应 BoltOffsetBUseMeasured）。语义同 <see cref="ComputeInput.UseB"/>。
        /// </summary>
        public bool UseB { get; set; }

        public decimal[] E { get; set; }

        public MeasuredEZerothSegmentDecimal ZerothMeasuredE { get; set; }
        public MeasuredEStandardSegmentDecimal StandardMeasuredE { get; set; }
    }

    /// <summary>decimal 版计算输出（字段与 ComputeOutput 一一对应）</summary>
    public class ComputeOutputDecimal
    {
        public bool JudgePass { get; set; }
        public List<string> LogMessage { get; set; } = new List<string>();

        public decimal[][] K { get; set; }
        public decimal[][] KT { get; set; }
        public decimal[][] KCT { get; set; }

        public decimal[] ErrL { get; set; }
        public decimal[] ErrHP { get; set; }
        public decimal[] ErrH { get; set; }
        public decimal[] ErrHE { get; set; }
        public decimal[] ErrHC { get; set; }
        public decimal[] ErrHW { get; set; }


        /// <summary>
        /// 设计中心长度
        /// </summary>
        public decimal TheoreticalCenterLength { get; set; }
        /// <summary>
        /// 设计中心总长
        /// </summary>
        public decimal TheoreticalTotalCenterLength { get; set; }
        /// <summary>
        /// 浇筑中心长度
        /// </summary>
        public decimal AsCastCenterLength { get; set; }
        /// <summary>
        /// 浇筑中心总长
        /// </summary>
        public decimal AsCastTotalCenterLength { get; set; }

        public CalcResultDecimal CalcResultDecimal { get; set; }
    }

    #region E强类型实体 拆分两套（0号段 / 标准段）
    /// <summary>
    /// 0号段(m=0)实测数据，共14项；对应b_8.15.py L808-812注释
    /// 备注：下标0(FixedBulkheadCenterOffset)原版算法不参与计算，仅做记录占位
    /// </summary>
    public class MeasuredEZerothSegment
    {
        public double FixedBulkheadCenterOffset { get; set; }      //0，仅记录，不参与运算
        public double WetCastWestLength { get; set; }              //1
        public double FloatingBulkheadCenterOffset { get; set; }   //2
        public double WetCastEastLength { get; set; }              //3
        public double WetCastSouthEastElev { get; set; }           //4
        public double WetCastNorthEastElev { get; set; }           //5
        public double WetCastSouthWestElev { get; set; }           //6
        public double WetCastNorthWestElev { get; set; }           //7
        public double FloatingBulkheadEastElev { get; set; }       //8
        public double FloatingBulkheadWestElev { get; set; }       //9
        public double FixedBulkheadEastElev { get; set; }          //10
        public double FixedBulkheadWestElev { get; set; }          //11
        public double WetCastCenterOffsetSouth { get; set; }             //12
        public double WetCastCenterOffsetNorth { get; set; }             //13

        public double[] ToArray()
        {
            return new double[]
            {
                FixedBulkheadCenterOffset,
                WetCastWestLength,
                FloatingBulkheadCenterOffset,
                WetCastEastLength,
                WetCastSouthEastElev,
                WetCastNorthEastElev,
                WetCastSouthWestElev,
                WetCastNorthWestElev,
                FloatingBulkheadEastElev,
                FloatingBulkheadWestElev,
                FixedBulkheadEastElev,
                FixedBulkheadWestElev,
                WetCastCenterOffsetSouth,
                WetCastCenterOffsetNorth
            };
        }

        public static MeasuredEZerothSegment FromArray(double[] arr)
        {
            if (arr == null) throw new ArgumentNullException(nameof(arr));
            if (arr.Length != 14) throw new ArgumentException("0号段实测E数组长度必须等于14");
            return new MeasuredEZerothSegment
            {
                FixedBulkheadCenterOffset = arr[0],
                WetCastWestLength = arr[1],
                FloatingBulkheadCenterOffset = arr[2],
                WetCastEastLength = arr[3],
                WetCastSouthEastElev = arr[4],
                WetCastNorthEastElev = arr[5],
                WetCastSouthWestElev = arr[6],
                WetCastNorthWestElev = arr[7],
                FloatingBulkheadEastElev = arr[8],
                FloatingBulkheadWestElev = arr[9],
                FixedBulkheadEastElev = arr[10],
                FixedBulkheadWestElev = arr[11],
                WetCastCenterOffsetSouth = arr[12],
                WetCastCenterOffsetNorth = arr[13]
            };
        }
    }

    /// <summary>
    /// 标准段(m≥1)实测数据，共16项；对应b_8.15.py L818-821注释
    /// 备注：下标0(FixedBulkheadCenterOffset)原版算法不参与计算，仅做记录占位
    /// </summary>
    public class MeasuredEStandardSegment
    {
        public double FixedBulkheadCenterOffset { get; set; }        //0，仅记录，不参与运算
        /// <summary>实测浇筑中线长度（标准段只录一个长度，即中线长；对应 SurveyWetCenterLength）。</summary>
        public double WetCastCenterLength { get; set; }         //1
        public double MatchCastCenterOffsetSouth { get; set; }  //2
        public double MatchCastCenterOffsetNorth { get; set; }  //3
        public double FixedBulkheadEastElev { get; set; }            //4
        public double FixedBulkheadWestElev { get; set; }            //5
        public double MatchCastSouthEastElev { get; set; }            //6
        public double MatchCastSouthWestElev { get; set; }            //7
        public double MatchCastNorthEastElev { get; set; }            //8
        public double MatchCastNorthWestElev { get; set; }            //9
        public double WetCastSouthEastElev { get; set; }              //10
        public double WetCastSouthWestElev { get; set; }              //11
        public double WetCastNorthEastElev { get; set; }              //12
        public double WetCastNorthWestElev { get; set; }              //13
        public double WetCastCenterOffsetSouth { get; set; }          //14
        public double WetCastCenterOffsetNorth { get; set; }          //15

        public double[] ToArray()
        {
            return new double[]
            {
                FixedBulkheadCenterOffset,
                WetCastCenterLength,
                MatchCastCenterOffsetSouth,
                MatchCastCenterOffsetNorth,
                FixedBulkheadEastElev,
                FixedBulkheadWestElev,
                MatchCastSouthEastElev,
                MatchCastSouthWestElev,
                MatchCastNorthEastElev,
                MatchCastNorthWestElev,
                WetCastSouthEastElev,
                WetCastSouthWestElev,
                WetCastNorthEastElev,
                WetCastNorthWestElev,
                WetCastCenterOffsetSouth,
                WetCastCenterOffsetNorth
            };
        }

        public static MeasuredEStandardSegment FromArray(double[] arr)
        {
            if (arr == null) throw new ArgumentNullException(nameof(arr));
            if (arr.Length != 16) throw new ArgumentException("标准段实测E数组长度必须等于16");
            return new MeasuredEStandardSegment
            {
                FixedBulkheadCenterOffset = arr[0],
                WetCastCenterLength = arr[1],
                MatchCastCenterOffsetSouth = arr[2],
                MatchCastCenterOffsetNorth = arr[3],
                FixedBulkheadEastElev = arr[4],
                FixedBulkheadWestElev = arr[5],
                MatchCastSouthEastElev = arr[6],
                MatchCastSouthWestElev = arr[7],
                MatchCastNorthEastElev = arr[8],
                MatchCastNorthWestElev = arr[9],
                WetCastSouthEastElev = arr[10],
                WetCastSouthWestElev = arr[11],
                WetCastNorthEastElev = arr[12],
                WetCastNorthWestElev = arr[13],
                WetCastCenterOffsetSouth = arr[14],
                WetCastCenterOffsetNorth = arr[15]
            };
        }
    }

    /// <summary>decimal版本-0号段m=0实测E实体</summary>
    public class MeasuredEZerothSegmentDecimal
    {
        public decimal FixedBulkheadCenterOffset { get; set; }
        public decimal WetCastWestLength { get; set; }
        public decimal FloatingBulkheadCenterOffset { get; set; }
        public decimal WetCastEastLength { get; set; }
        public decimal WetCastSouthEastElev { get; set; }
        public decimal WetCastNorthEastElev { get; set; }
        public decimal WetCastSouthWestElev { get; set; }
        public decimal WetCastNorthWestElev { get; set; }
        public decimal FloatingBulkheadEastElev { get; set; }
        public decimal FloatingBulkheadWestElev { get; set; }
        public decimal FixedBulkheadEastElev { get; set; }
        public decimal FixedBulkheadWestElev { get; set; }
        public decimal WetCastCenterOffsetSouth { get; set; }
        public decimal WetCastCenterOffsetNorth { get; set; }

        public decimal[] ToArray()
        {
            return new decimal[]
            {
                FixedBulkheadCenterOffset,
                WetCastWestLength,
                FloatingBulkheadCenterOffset,
                WetCastEastLength,
                WetCastSouthEastElev,
                WetCastNorthEastElev,
                WetCastSouthWestElev,
                WetCastNorthWestElev,
                FloatingBulkheadEastElev,
                FloatingBulkheadWestElev,
                FixedBulkheadEastElev,
                FixedBulkheadWestElev,
                WetCastCenterOffsetSouth,
                WetCastCenterOffsetNorth
            };
        }

        public static MeasuredEZerothSegmentDecimal FromArray(decimal[] arr)
        {
            if (arr == null) throw new ArgumentNullException(nameof(arr));
            if (arr.Length != 14) throw new ArgumentException("0号段实测E数组长度必须等于14");
            return new MeasuredEZerothSegmentDecimal
            {
                FixedBulkheadCenterOffset = arr[0],
                WetCastWestLength = arr[1],
                FloatingBulkheadCenterOffset = arr[2],
                WetCastEastLength = arr[3],
                WetCastSouthEastElev = arr[4],
                WetCastNorthEastElev = arr[5],
                WetCastSouthWestElev = arr[6],
                WetCastNorthWestElev = arr[7],
                FloatingBulkheadEastElev = arr[8],
                FloatingBulkheadWestElev = arr[9],
                FixedBulkheadEastElev = arr[10],
                FixedBulkheadWestElev = arr[11],
                WetCastCenterOffsetSouth = arr[12],
                WetCastCenterOffsetNorth = arr[13]
            };
        }
    }

    /// <summary>decimal版本-标准段m≥1实测E实体</summary>
    public class MeasuredEStandardSegmentDecimal
    {
        public decimal FixedBulkheadCenterOffset { get; set; }
        public decimal WetCastCenterLength { get; set; }
        public decimal MatchCastCenterOffsetSouth { get; set; }
        public decimal MatchCastCenterOffsetNorth { get; set; }
        public decimal FixedBulkheadEastElev { get; set; }
        public decimal FixedBulkheadWestElev { get; set; }
        public decimal MatchCastSouthEastElev { get; set; }
        public decimal MatchCastSouthWestElev { get; set; }
        public decimal MatchCastNorthEastElev { get; set; }
        public decimal MatchCastNorthWestElev { get; set; }
        public decimal WetCastSouthEastElev { get; set; }
        public decimal WetCastSouthWestElev { get; set; }
        public decimal WetCastNorthEastElev { get; set; }
        public decimal WetCastNorthWestElev { get; set; }
        public decimal WetCastCenterOffsetSouth { get; set; }
        public decimal WetCastCenterOffsetNorth { get; set; }

        public decimal[] ToArray()
        {
            return new decimal[]
            {
                FixedBulkheadCenterOffset,
                WetCastCenterLength,
                MatchCastCenterOffsetSouth,
                MatchCastCenterOffsetNorth,
                FixedBulkheadEastElev,
                FixedBulkheadWestElev,
                MatchCastSouthEastElev,
                MatchCastSouthWestElev,
                MatchCastNorthEastElev,
                MatchCastNorthWestElev,
                WetCastSouthEastElev,
                WetCastSouthWestElev,
                WetCastNorthEastElev,
                WetCastNorthWestElev,
                WetCastCenterOffsetSouth,
                WetCastCenterOffsetNorth
            };
        }

        public static MeasuredEStandardSegmentDecimal FromArray(decimal[] arr)
        {
            if (arr == null) throw new ArgumentNullException(nameof(arr));
            if (arr.Length != 16) throw new ArgumentException("标准段实测E数组长度必须等于16");
            return new MeasuredEStandardSegmentDecimal
            {
                FixedBulkheadCenterOffset = arr[0],
                WetCastCenterLength = arr[1],
                MatchCastCenterOffsetSouth = arr[2],
                MatchCastCenterOffsetNorth = arr[3],
                FixedBulkheadEastElev = arr[4],
                FixedBulkheadWestElev = arr[5],
                MatchCastSouthEastElev = arr[6],
                MatchCastSouthWestElev = arr[7],
                MatchCastNorthEastElev = arr[8],
                MatchCastNorthWestElev = arr[9],
                WetCastSouthEastElev = arr[10],
                WetCastSouthWestElev = arr[11],
                WetCastNorthEastElev = arr[12],
                WetCastNorthWestElev = arr[13],
                WetCastCenterOffsetSouth = arr[14],
                WetCastCenterOffsetNorth = arr[15]
            };
        }
    }
    #endregion

    /// <summary>Excel 读写占位（按交付要求保留接口签名；实际由业务层实现：
    /// 读取 Excel 生成 ComputeInput / 将 ComputeOutput 组织后写出 Excel）</summary>
    public static class ReadWriteExcelHelper
    {
        public static ComputeInput ReadExcelToInput(string excelFilePath)
        {
            throw new NotImplementedException("业务层实现Excel读取，填充K/KT/KCT等数组");
        }
        public static void WriteOutputToExcel(ComputeOutput output, string saveExcelPath)
        {
            throw new NotImplementedException("业务层实现Excel写出");
        }
        public static ComputeInputDecimal ReadExcelToInputDecimal(string excelFilePath)
        {
            throw new NotImplementedException("业务层实现decimal版本Excel读取");
        }
        public static void WriteOutputToExcelDecimal(ComputeOutputDecimal output, string saveExcelPath)
        {
            throw new NotImplementedException("业务层实现decimal版本Excel写出");
        }
    }
    #endregion

    /// <summary>PR报告输出行实体，与主类并列</summary>
    public class ReportRow
    {
        /// <summary>节段索引i</summary>
        public int Index { get; set; }
        public double BE { get; set; }
        public double BW { get; set; }
        public double? AE { get; set; }
        public double? AW { get; set; }
        public double? LOE { get; set; }
        public double? LOW { get; set; }
        public double CSE { get; set; }
        public double CSW { get; set; }
        /// <summary>"NaN" / "Upward" / "Check"</summary>
        public string ZA { get; set; }

        /// <summary>转为对象行，用于打印/导出csv</summary>
        public List<object> ToObjectRow()
        {
            var row = new List<object>();
            row.Add(Index);
            row.Add(BE);
            row.Add(BW);
            row.Add(AE.HasValue ? (object)AE.Value : "NaN");
            row.Add(AW.HasValue ? (object)AW.Value : "NaN");
            row.Add(LOE.HasValue ? (object)LOE.Value : "NaN");
            row.Add(LOW.HasValue ? (object)LOW.Value : "NaN");
            row.Add(CSE);
            row.Add(CSW);
            row.Add(ZA ?? "NaN");
            return row;
        }

        /// <summary>获取表头（列名需与 Python PR 输出一致，保持 ASCII 连字符 '-'，
        /// 下游按列名匹配列，勿用 U+2011 等特殊字符）</summary>
        public static List<string> GetHeader()
        {
            return new List<string>
            {
                "Joint", "B_East", "B_West", "Bulkhead Angle(East)", "Bulkhead Angle(West)",
                "Long Offset(East)", "Long Offset(West)", "Cross-Slope(East)", "Cross-Slope(West)", "Z-Axis"
            };
        }
    }
}
