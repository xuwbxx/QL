using DataFactory.KingBase;
using DocumentFormat.OpenXml.Drawing;
using MathNet.Numerics.LinearAlgebra;
using Model.tech.QL.DTO.BizSteelBeam;
using NPOI.HSSF.Record;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Service.PreAssembled.Reports;
using Tool;

namespace Service.PreAssembled.BridgeSegmentAlignment
{
    public static class SteelGirderAlignment
    {
        /// <summary>
        /// 根据选择的若干待拼装梁段，计算这些待拼装梁段的坐标
        ///     1. 获取当前梁的总已拼装测点理论坐标
        ///     2. 获取当前梁的总已拼装测点实测坐标（来自顶推）
        ///     3. 根据选择的待拼装梁段，获取其测点理论坐标
        ///     4. 获取所有点位加权值
        /// </summary>
        /// <param name="projID"></param>
        /// <param name="girderID"></param>
        /// <param name="beamList"></param>
        /// <returns></returns>
        public static decimal[,] GetComputeData(int projID, int girderID, string[] beamList)
        {
            // 
            return null;
        }

        public static List<T> Gescha<T>(List<T> P, List<T> Q, List<T> D, List<T> R, decimal? scaleS = 1.0m)
            where T : class, ISteelBeamPointRow, new()
        {
            var p = new List<List<decimal>>() { new(), new(), new() };
            var q = new List<List<decimal>>() { new(), new(), new() };
            var d = new List<List<decimal>>() { new(), new(), new() };
            var r = new List<List<decimal>>() { new(), new(), new() };
            for (var i = 0; i < P.Count; i++)
            {
                p[0].Add(P[i].X);
                p[1].Add(P[i].Y);
                p[2].Add(P[i].Z);
                q[0].Add(Q[i].X);
                q[1].Add(Q[i].Y);
                q[2].Add(Q[i].Z);
                d[0].Add(D[i].X);
                d[1].Add(D[i].Y);
                d[2].Add(D[i].Z);
                r[0].Add(R[i].X);
                r[1].Add(R[i].Y);
                r[2].Add(R[i].Z);
            }

            var result = Gescha(p, q, d, r, scaleS);
            var Result = new List<T>();
            foreach (var itm in result)
            {
                // 补充new T()的代码
            }
            return Result;
        }

        /// <summary>
        /// 【数据库decimal重载】加权Procrustes配准Gescha
        /// 入参全部decimal[,]，返回decimal[,]；内部转double计算
        /// P,Q:3×N 理论、实测点集
        /// D:3×M 待变换点集
        /// R:3×N 权重矩阵
        /// </summary>
        /// <param name="P">已施工理论点位矩阵（行x/y/z，列点位）</param>
        /// <param name="Q">已施工实测点位矩阵（行x/y/z，列点位）</param>
        /// <param name="D">待施工理论点位矩阵（行x/y/z，列点位）</param>
        /// <param name="R">点位坐标加权矩阵（行x/y/z，列点位）</param>
        /// <returns>待施工预测点位矩阵(D1 3×M 变换后坐标)</returns>
        public static List<List<decimal>> Gescha(List<List<decimal>> P, List<List<decimal>> Q, List<List<decimal>> D, List<List<decimal>> R, decimal? scaleS = 1.0m)
        {
            var result = Gescha(P.To2DArray(), Q.To2DArray(), D.To2DArray(), R.To2DArray(), scaleS);
            return result.ToListList();
        }

        /// <summary>
        /// 【数据库decimal重载】加权Procrustes配准Gescha
        /// 入参全部decimal[,]，返回decimal[,]；内部转double计算
        /// P,Q:3×N 理论、实测点集
        /// D:3×M 待变换点集
        /// R:3×N 权重矩阵
        /// </summary>
        /// <param name="P">已施工理论点位矩阵（行x/y/z，列点位）</param>
        /// <param name="Q">已施工实测点位矩阵（行x/y/z，列点位）</param>
        /// <param name="D">待施工理论点位矩阵（行x/y/z，列点位）</param>
        /// <param name="R">点位坐标加权矩阵（行x/y/z，列点位）</param>
        /// <returns>待施工预测点位矩阵(D1 3×M 变换后坐标)</returns>
        public static decimal[,] Gescha(decimal[,] P, decimal[,] Q, decimal[,] D, decimal[,] R, decimal? scaleS = 1.0m)
        {
            // decimal → double
            double[,] pDouble = ToDoubleMatrix(P);
            double[,] qDouble = ToDoubleMatrix(Q);
            double[,] dDouble = ToDoubleMatrix(D);
            double[,] rDouble = ToDoubleMatrix(R);

            // 调用原有double核心算法
            double[,] resDouble = Gescha(pDouble, qDouble, dDouble, rDouble, (double)scaleS);

            // double → decimal
            return ToDecimalMatrix(resDouble);
        }

        /// <summary>
        /// decimal[,] 矩阵 => double[,]，用于Gescha(double版本)
        /// 先四舍五入保留4位小数，匹配业务坐标精度
        /// </summary>
        public static double[,] ToDoubleMatrix(decimal[,] matDec)
        {
            int rows = matDec.GetLength(0);
            int cols = matDec.GetLength(1);
            double[,] matDbl = new double[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    var val = Math.Round(matDec[i, j], 4, MidpointRounding.AwayFromZero);
                    matDbl[i, j] = (double)val;
                }
            }
            return matDbl;
        }

        /// <summary>
        /// Gescha输出double[,]结果矩阵转回decimal[,]，用于业务DTO
        /// 转回时同样保留4位小数
        /// </summary>
        public static decimal[,] ToDecimalMatrix(double[,] matDbl)
        {
            int rows = matDbl.GetLength(0);
            int cols = matDbl.GetLength(1);
            decimal[,] matDec = new decimal[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    matDec[i, j] = Math.Round((decimal)matDbl[i, j], 4, MidpointRounding.AwayFromZero);
                }
            }
            return matDec;
        }

        /// <summary>
        /// Gescha相似变换 / 加权刚体Procrustes
        /// 【P输入矩阵：3行N列，已施工构件的理论坐标点集；每一列代表一个点(X,Y,Z)】
        /// 【Q输入矩阵：3行N列，已施工构件的实测坐标点集；每一列代表一个点(X,Y,Z)】
        /// 【D输入矩阵：3行M列，待预测变换的目标点集（未施工构件理论坐标）；每一列代表一个点(X,Y,Z)】
        /// 【R输入矩阵：权重矩阵。两种格式：
        ///      格式1（逐维权重）：3行N列，第0/1/2行分别是X/Y/Z三个维度的权重；
        ///                        同一测点的X/Y/Z可各不相同，也可相同。
        ///      格式2（逐点权重）：1行N列，每个测点仅一个权重标量，X/Y/Z共用该值。
        ///      通过 R.GetLength(0) 自动识别：==3 为逐维权重，==1 为逐点权重】
        /// scaleS：尺度因子，默认=1.0（刚体Procrustes）；传入null则自动求解Gescha尺度s
        /// 返回 D1 3×M：待施工构件预测坐标
        /// </summary>
        /// <param name="P">已施工理论点位矩阵（行x/y/z，列点位）</param>
        /// <param name="Q">已施工实测点位矩阵（行x/y/z，列点位）</param>
        /// <param name="D">待施工理论点位矩阵（行x/y/z，列点位）</param>
        /// <param name="R">点位坐标加权矩阵（行x/y/z，列点位）</param>
        /// <param name="scaleS">尺度因子。1.0（默认）=刚体配准；null=自动求解Gescha尺度；其他值=固定尺度</param>
        /// <returns>待施工预测点位矩阵(D1 3×M 变换后坐标)</returns>
        public static double[,] Gescha(double[,] P, double[,] Q, double[,] D, double[,] R, double? scaleS = 1.0)
        {
            // P输入矩阵：3行N列，已施工构件的理论坐标点集；每一列代表一个点(X,Y,Z)
            int nrow = P.GetLength(0);
            // Q输入矩阵：3行N列，已施工构件的实测坐标点集；每一列代表一个点(X,Y,Z)
            int ncol = P.GetLength(1);
            // D输入矩阵：3行M列，待预测变换的目标点集（未施工构件理论坐标）；每一列代表一个点(X,Y,Z)
            int nrow1 = D.GetLength(0);
            // D输入矩阵：列数M；代表待预测变换的点数量
            int ncol1 = D.GetLength(1);
            // 权重矩阵行数：==3 表示逐维权重（X/Y/Z可各自独立）；==1 表示逐点权重（每点一个标量，XYZ共用）
            int weightRowCount = R.GetLength(0);
            if (weightRowCount != 1 && weightRowCount != 3)
                throw new ArgumentException("权重矩阵R行数必须为1（独立权重）或3（xyz共权重），当前行数=" + weightRowCount);
            if (scaleS == null && weightRowCount == 3)
            {
                for (int j = 0; j < ncol; j++)
                {
                    if (Math.Abs(R[1, j] - R[0, j]) > 1e-12 ||
                        Math.Abs(R[2, j] - R[0, j]) > 1e-12)
                    {
                        throw new NotSupportedException(
                            "自动求解尺度s不支持X/Y/Z独立分量权重，请使用scaleS固定值模式");
                    }
                }
            }
            if (R.GetLength(1) != ncol)
                throw new ArgumentException("权重矩阵R列数必须与控制点数量N一致");
            if (nrow != 3 || nrow1 != 3)
                throw new ArgumentException("P、Q、D矩阵必须为3行（X,Y,Z）");


            // 将C#二维数组转换为MathNet矩阵对象，用于线性代数运算
            var matP = Matrix<double>.Build.DenseOfArray(P);
            var matQ = Matrix<double>.Build.DenseOfArray(Q);
            var matD = Matrix<double>.Build.DenseOfArray(D);
            // 权重矩阵：若为1×N独立权重，先扩展为3×N（每行重复同一权重值），统一后续按3×N处理
            var matR = Matrix<double>.Build.DenseOfArray(R);
            if (weightRowCount == 1)
            {
                // 逐点权重(1×N) → 复制为3×N，每个测点X/Y/Z共用同一个权重，便于统一计算
                var expanded = Matrix<double>.Build.Dense(3, ncol);
                for (int j = 0; j < ncol; j++)
                {
                    double w = matR[0, j];
                    expanded[0, j] = w;
                    expanded[1, j] = w;
                    expanded[2, j] = w;
                }
                matR = expanded;
            }
            // 验证权重非负
            for (int j = 0; j < ncol; j++)
                for (int i = 0; i < 3; i++)
                    if (matR[i, j] < 0) throw new ArgumentException("权重必须非负");

            //# --- 步骤1: 应用权重到坐标矩阵 ---
            //# 施工中，某些节段可能测量精度高（如已精调），某些可能精度低（如临时支撑）
            //# 通过权重矩阵R，让高精度节段在配准中起主导作用
            // wP：加权后的理论坐标矩阵；点乘：每个坐标分量乘以对应权重，PointwiseMultiply为逐元素哈达玛积
            var wP = matP.PointwiseMultiply(matR);
            // wQ：加权后的实测坐标矩阵；逐元素哈达玛积，每个坐标分量乘以对应权重
            var wQ = matQ.PointwiseMultiply(matR);

            //# --- 步骤2: 计算加权重心 ---
            //# 计算"加权平均"位置，作为旋转中心
            //# 权重越大，该节段对整体变换的影响越大
            // sumWpRow：对wP做行求和，得到加权理论坐标累加向量；RowSums：矩阵按行求和
            Vector<double> sumWpRow = wP.RowSums();
            // sumRRow：权重矩阵按行求和，得到权重累加向量；RowSums：矩阵按行求和
            Vector<double> sumRRow = matR.RowSums();
            // GP：加权重心(质心)，理论点集的加权平均坐标；PointwiseDivide逐元素相除，加权重心=加权和 / 权重总和
            Vector<double> GP = sumWpRow.PointwiseDivide(sumRRow);

            // sumWqRow：对wQ做行求和，得到加权实测坐标累加向量；RowSums：矩阵按行求和
            Vector<double> sumWqRow = wQ.RowSums();
            // GQ：加权重心(质心)，实测点集的加权平均坐标；PointwiseDivide逐元素相除，加权重心=加权和 / 权重总和
            Vector<double> GQ = sumWqRow.PointwiseDivide(sumRRow);

            //# --- 步骤3: 去中心化（将点云移到原点） ---
            //# 目的是分离旋转和平移，使SVD只求解旋转部分
            //# 相当于把所有节段相对于其重心进行定位
            // PP：去中心化后的理论偏差矩阵，每个点减去理论重心GP；将理论点集平移至重心原点
            Matrix<double> PP = Matrix<double>.Build.Dense(nrow, ncol);
            // QQ：去中心化后的实测偏差矩阵，每个点减去实测重心GQ；将实测点集平移至重心原点
            Matrix<double> QQ = Matrix<double>.Build.Dense(nrow, ncol);
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < ncol; j++)
                {
                    //# 理论值 - 理论重心
                    PP[i, j] = matP[i, j] - GP[i];
                    //# 实测值 - 实测重心
                    QQ[i, j] = matQ[i, j] - GQ[i];
                }
            }

            //# --- 步骤4: 构建协方差矩阵 H ---
            //# H = Σ(权重 * 理论偏差向量 * 实测偏差向量^T)
            //# 这个矩阵包含了理论点云和实测点云之间的相关性
            //# 施工中：反映了已完工节段的"理论→实测"变形模式
            // PT：去中心化理论偏差矩阵的转置矩阵；Transpose()矩阵转置运算
            var PT = PP.Transpose();
            // QT：去中心化实测偏差矩阵的转置矩阵；Transpose()矩阵转置运算
            var QT = QQ.Transpose();

            // H：加权协方差矩阵；PointwiseMultiply(PP)权重乘去中心化理论矩阵，再矩阵乘法乘以QT；Procrustes配准核心输入矩阵
            var H = matR.PointwiseMultiply(PP) * QT;

            //# --- 步骤5: 奇异值分解 (SVD) 求解最优旋转 ---
            //# SVD分解 H = U * S * V^T
            //# 通过SVD，可以找到最优旋转矩阵，使"理论点云旋转后"最接近"实测点云"
            //# 这相当于求解施工中的"姿态偏差"（旋转角）
            // 对协方差矩阵H执行SVD奇异值分解 H = U * Σ * VT；Svd()奇异值分解算法，求解旋转矩阵的标准步骤
            var svd = H.Svd();
            // U：SVD分解输出左奇异向量矩阵
            var U = svd.U;
            // VT：SVD分解输出右奇异向量的转置矩阵；Transpose()矩阵转置运算
            var VT = svd.VT;
            // V：右奇异向量矩阵，VT做转置还原得到V；Transpose()矩阵转置
            var V = VT.Transpose();
            // UT：左奇异向量矩阵U的转置矩阵；Transpose()矩阵转置
            var UT = U.Transpose();

            //# --- 步骤6: 计算旋转矩阵 rotMat ---
            //# 此处rotMat开始，是指旋转矩阵，不再是前面的权重矩阵matR
            //# rotMat = V * U^T 是最优旋转矩阵（最小二乘意义下）
            // rotMat：初步计算得到旋转矩阵候选；矩阵乘法 V * UT
            Matrix<double> rotMat = V * UT;
            // detR：旋转矩阵rotMat的行列式；Determinant()行列式计算；det>0代表纯旋转，det<0说明发生镜像反射，需要修正
            double detR = rotMat.Determinant();

            //# --- 步骤7: 反射检测与修正 ---
            //# 桥梁施工中，理论上不应该出现镜像翻转（左右手坐标系变换）
            //# 如果出现 detR < 0，说明SVD求出了包含反射的解
            //# 需要修正为纯旋转（避免出现"翻转"这种不合理的变换）
            // 计算尺度因子s（Gescha），scaleS=null自动求解；传入固定值则使用给定s
            double s;
            if (scaleS.HasValue)
            {
                s = scaleS.Value;
            }
            else
            {
                // 加权Gescha尺度 s = sum(w_j * ||q'_j||²) / sum(w_j * ||p'_j||²)
                // 权重已统一扩展为3×N，x取[0, n]，y取[1, n]，z取[2, n]
                double sumWqNorm2 = 0.0;
                double sumWpNorm2 = 0.0;
                for (int j = 0; j < ncol; j++)
                {
                    double wx = matR[0, j], wy = matR[1, j], wz = matR[2, j];
                    double px = PP[0, j], py = PP[1, j], pz = PP[2, j];
                    double qx = QQ[0, j], qy = QQ[1, j], qz = QQ[2, j];

                    sumWpNorm2 += wx * px * px + wy * py * py + wz * pz * pz;
                    sumWqNorm2 += wx * qx * qx + wy * qy * qy + wz * qz * qz;
                }
                s = sumWqNorm2 / sumWpNorm2;
            }

            // diagData：镜像修正对角线向量 [1,1,-1]，用于消除反射，保证输出为合法右手旋转矩阵
            var diagData = Vector<double>.Build.Dense(new double[] { 1.0, 1.0, -1.0 });
            // cha：3×3对角修正矩阵diag(1,1,-1)；DenseOfDiagonalVector根据向量构建对角方阵，用于行列式为负时的镜像校正
            // 1, 0, 0
            // 0, 1, 0
            // 0, 0,-1
            var cha = Matrix<double>.Build.DenseOfDiagonalVector(diagData);

            // t：平移向量；t = GQ − s * rotMat * GP；重心之间经过缩放+旋转后的偏移量
            Vector<double> t;
            if (detR > 0)
            {
                // 行列式>0，无镜像反射，旋转矩阵有效，直接计算平移量
                //# 正常情况：纯旋转，直接使用
                //# 平移量 = 实测重心 - s * 旋转后的理论重心
                t = GQ - s * rotMat * GP;
            }
            else
            {
                // 行列式<0，存在镜像反射，引入cha对角矩阵修正旋转矩阵，消除反射分量
                //# 异常情况：出现反射，需要修正
                //# 将V的第三列取反，消除反射成分
                var RR = V * cha; // V 右奇异向量矩阵
                rotMat = RR * UT; // # 重新计算旋转矩阵（保证detR>0）
                                  //# 平移量 = 实测重心 - s * 旋转后的理论重心
                t = GQ - s * rotMat * GP;
            }

            //# --- 步骤8: 构建平移矩阵 ---
            //# 将所有待安装节段使用相同的平移量
            //# 施工中：相当于"整体平移"所有剩余节段
            // Tmat：3行M列平移矩阵，每一列全部填充平移向量t；后续矩阵运算时每个点统一叠加该平移量
            Matrix<double> Tmat = Matrix<double>.Build.Dense(3, ncol1);
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < ncol1; j++)
                {
                    Tmat[i, j] = t[i];
                }
            }

            //# --- 步骤9: 应用到待安装节段 ---
            //# D1 = s * rotMat * D + T
            //# 即：待安装节段的预测实际坐标 = s * 旋转 * 理论坐标 + 平移
            //# 这就是通过已完工节段的偏差，预测未完工节段的实际安装位置
            var D1 = s * rotMat * matD + Tmat;
            // ToArray() 将MathNet矩阵对象转回C#原生double二维数组返回
            return D1.ToArray();
        }

        /// <summary>
        /// 对 biz_steel_beam_theoretical 测点集合，按【测位PositionName分组】
        /// 三维(X,Y,Z)拟合梁纵向轴线，Z高程参与投影计算
        /// 生成 PositionOrder（沿轴线顺序编号0,1,2...）
        /// 同时计算并赋值 DistanceFromStart：距本组测位起点沿梁轴线的里程（>=0）
        /// </summary>
        /// <param name="theoCoods">全部理论测点</param>
        public static void CalcPointPositionOrderAndDistanceFromStart(List<biz_steel_beam_theoretical> theoCoods)
        {
            // 按测位分组
            var groups = theoCoods.GroupBy(p => p.PositionName).ToList();

            foreach (var group in groups)
            {
                var posName = group.Key;
                var points = group.ToList();
                if (points.Count == 0) continue;

                // 固定基准点规则：X最小，X相同取Y最小，保证多次执行结果稳定
                var basePt = points.OrderBy(p => p.DesignX).ThenBy(p => p.DesignY).First();

                // 1. 所有测点相对基准点的三维向量
                var vecList = points.Select(p => new
                {
                    Item = p,
                    Dx = p.DesignX - basePt.DesignX,
                    Dy = p.DesignY - basePt.DesignY,
                    Dz = p.DesignZ - basePt.DesignZ
                }).ToList();

                // 2. 拟合轴线主方向向量：所有相对向量求和
                decimal sumDx = vecList.Sum(v => v.Dx);
                decimal sumDy = vecList.Sum(v => v.Dy);
                decimal sumDz = vecList.Sum(v => v.Dz);

                // 边界：全部点重合，方向向量为0
                if (sumDx == 0m && sumDy == 0m && sumDz == 0m)
                {
                    var sorted = points.OrderBy(p => p.PointCode).ToList();
                    for (int i = 0; i < sorted.Count; i++)
                    {
                        sorted[i].PositionOrder = i;
                        sorted[i].DistanceFromStart = 0m;
                    }
                    continue;
                }

                // 三维向量模长，转为double开根号，转回decimal
                double vecLenDouble = Math.Sqrt((double)(sumDx * sumDx + sumDy * sumDy + sumDz * sumDz));
                decimal len = (decimal)vecLenDouble;

                // 三维单位方向向量
                decimal eX = sumDx / len;
                decimal eY = sumDy / len;
                decimal eZ = sumDz / len;

                // 3. 三维点积，得到轴向投影（可正负，用于排序）
                var projList = vecList.Select(v => new
                {
                    v.Item,
                    Proj = v.Dx * eX + v.Dy * eY + v.Dz * eZ
                }).ToList();

                // 4. 按投影升序，赋值PositionOrder
                var sortedPoints = projList.OrderBy(x => x.Proj).ToList();
                for (int i = 0; i < sortedPoints.Count; i++)
                {
                    sortedPoints[i].Item.PositionOrder = i;
                }

                // 5. 计算DistanceFromStart：以排序后第一个点作为里程零点
                var startPoint = sortedPoints[0].Item;
                decimal startX = startPoint.DesignX;
                decimal startY = startPoint.DesignY;
                decimal startZ = startPoint.DesignZ;

                foreach (var item in sortedPoints)
                {
                    var p = item.Item;
                    decimal dx = p.DesignX - startX;
                    decimal dy = p.DesignY - startY;
                    decimal dz = p.DesignZ - startZ;
                    //三维点积：沿轴线投影里程 >=0
                    decimal dist = dx * eX + dy * eY + dz * eZ;
                    p.DistanceFromStart = Math.Round(dist, 4, MidpointRounding.AwayFromZero);
                }
            }
        }

        /// <summary>
        /// Zero函数：i为1‑based测点序号，将该测点对应的权重列全部置0，模拟测点失效
        /// </summary>
        /// <param name="R">3行N列权重矩阵</param>
        /// <param name="i">1‑based测点编号</param>
        /// <returns>修改后的权重矩阵，仅i对应列全部置0，其余列不变</returns>
        public static double[,] ZeroOutColumn(double[,] R, int i)
        {
            // 将传入的权重二维数组转为MathNet矩阵对象
            var mat = Matrix<double>.Build.DenseOfArray(R);
            // i是1‑based序号，转换为0‑based矩阵列下标
            int colIdx = i - 1;
            // 将该测点列X/Y/Z三行权重全部赋值为0，该点不再参与加权Procrustes运算
            for (int j = 0; j < 3; j++)
            {
                mat[j, colIdx] = 0.0;
            }
            // 矩阵转回C#原生double[,]数组输出
            return mat.ToArray();
        }

        /// <summary>
        /// 复刻Python matrix函数：读取Excel指定sheet，输出double[,]矩阵
        /// </summary>
        public static double[,] Matrix(string filePath, int sheetIndex)
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            IWorkbook workbook = new XSSFWorkbook(fs);
            ISheet table = workbook.GetSheetAt(sheetIndex);

            int rowCount = table.LastRowNum + 1;
            int colCount = 0;
            for (int r = 0; r < rowCount; r++)
            {
                var row = table.GetRow(r);
                if (row == null) continue;
                if (row.LastCellNum > colCount) colCount = row.LastCellNum;
            }

            double[,] datamatrix = new double[rowCount, colCount];

            for (int x = 0; x < colCount; x++)
            {
                double[] colData = new double[rowCount];
                for (int r = 0; r < rowCount; r++)
                {
                    var row = table.GetRow(r);
                    var cell = row?.GetCell(x);
                    colData[r] = GetCellValue(cell);
                }
                for (int r = 0; r < rowCount; r++)
                {
                    datamatrix[r, x] = colData[r];
                }
            }
            return datamatrix;
        }

        private static double GetCellValue(ICell? cell)
        {
            if (cell == null) return 0.0;
            return cell.CellType switch
            {
                CellType.Numeric => cell.NumericCellValue,
                CellType.String => double.TryParse(cell.StringCellValue, out var d) ? d : 0.0,
                CellType.Boolean => cell.BooleanCellValue ? 1.0 : 0.0,
                _ => 0.0
            };
        }

        /// <summary>
        /// 主运行逻辑，对应Python脚本主体（离线Excel蒙特卡洛仿真）
        /// 模拟部分测点失效，评估配准算法在测点缺失工况下的最大误差
        /// </summary>
        /// <param name="excelPath">输入Excel文件路径，读取6个sheet：0=P1，1=P2，2=P3，3=P4，4=R1，5=R2</param>
        /// <param name="outputExcel">输出结果Excel路径，输出err工作表，保存每一轮失效测点编号与对应最大误差</param>
        /// <param name="n">每一轮仿真随机失效的测点数量（1‑based测点编号范围1~54）</param>
        /// <param name="m">蒙特卡洛仿真总迭代轮数</param>
        /// <param name="r">取误差的目标行下标：0=X方向，1=Y方向，2=Z方向</param>
        public static void RunSimulation(string excelPath, string outputExcel, int n, int m, int r)
        {
            // 读取Excel各个sheet，得到对应矩阵，3行N列点集/权重矩阵
            double[,] P1 = Matrix(excelPath, 0);
            double[,] P2 = Matrix(excelPath, 1);
            double[,] P3 = Matrix(excelPath, 2);
            double[,] P4 = Matrix(excelPath, 3);
            double[,] R1 = Matrix(excelPath, 4);
            double[,] R2 = Matrix(excelPath, 5);

            // 第一轮加权Procrustes配准：P1理论点，P2实测点，变换P3待变换点，权重R1，输出P5
            double[,] P5 = Gescha(P1, P2, P3, R1);
            // 第二轮加权Procrustes配准：以P5作为理论输入，P4实测点，变换P5自身，权重R2，得到基准结果P6（真值基准）
            double[,] P6 = Gescha(P5, P4, P5, R2);

            // err[m, n+1]结果矩阵：m行仿真轮次；前n列存储本轮失效测点编号；最后一列存储该轮最大误差值
            double[,] err = new double[m, n + 1];
            Random rand = new Random();

            // 循环m次蒙特卡洛仿真
            for (int i = 0; i < m; i++)
            {
                // 从1~54测点中，随机抽取n个不重复测点编号，作为本轮要失效的测点
                List<int> numbers = SampleRange(rand, 1, 54, n);
                // 将本轮失效测点编号填入err数组前n列
                for (int j = 0; j < numbers.Count; j++)
                {
                    err[i, j] = numbers[j];
                }

                // 克隆原始权重矩阵R2，不修改原始输入数据
                double[,] R22 = (double[,])R2.Clone();
                // 遍历本轮失效测点，逐个将对应测点权重列置0，模拟测点失效，不再参与配准计算
                foreach (int kNum in numbers)
                {
                    R22 = ZeroOutColumn(R22, kNum);
                }

                // 使用失效后的权重R22执行配准，得到测点失效后的计算结果P7
                double[,] P7 = Gescha(P5, P4, P5, R22);

                // 将数组转为MathNet矩阵对象，用于误差计算
                var matP7 = Matrix<double>.Build.DenseOfArray(P7);
                var matP6 = Matrix<double>.Build.DenseOfArray(P6);
                var matR22 = Matrix<double>.Build.DenseOfArray(R22);

                // E = (失效后结果 − 基准真值) 逐元素哈达玛乘积 × 当前权重矩阵R22；仅保留有效测点的误差项，失效测点权重为0误差自动清零
                var E = (matP7 - matP6).PointwiseMultiply(matR22);

                // 取出指定r方向（X/Y/Z）的误差行向量
                Vector<double> row = E.Row(r);
                // 获取该行绝对值最大的元素下标
                int maxIndex = row.AbsoluteMaximumIndex();
                // 获取该行绝对值最大误差数值
                double maxElement = row[maxIndex];
                // 将本轮仿真最大误差存入结果数组最后一列
                err[i, n] = maxElement;
            }

            // 将err仿真结果矩阵写入Excel，工作表名称"err"
            WriteExcel(outputExcel, err, "err");
        }


        private static List<int> SampleRange(Random rand, int min, int max, int count)
        {
            List<int> full = new List<int>();
            for (int v = min; v <= max; v++) full.Add(v);
            List<int> result = new List<int>();
            for (int s = 0; s < count; s++)
            {
                int idx = rand.Next(full.Count);
                result.Add(full[idx]);
                full.RemoveAt(idx);
            }
            return result;
        }

        private static void WriteExcel(string filePath, double[,] data, string sheetName)
        {
            IWorkbook wb = new XSSFWorkbook();
            ISheet sheet = wb.CreateSheet(sheetName);
            int rows = data.GetLength(0);
            int cols = data.GetLength(1);
            ICellStyle cellStyle = wb.CreateCellStyle();
            IDataFormat fmt = wb.CreateDataFormat();
            cellStyle.DataFormat = fmt.GetFormat("0.00000");

            for (int i = 0; i < rows; i++)
            {
                IRow row = sheet.CreateRow(i);
                for (int j = 0; j < cols; j++)
                {
                    ICell cell = row.CreateCell(j);
                    cell.SetCellValue(data[i, j]);
                    cell.CellStyle = cellStyle;
                }
            }
            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            wb.Write(fs);
        }


        // 向量结构体，decimal版本，适配你的工程坐标
        internal readonly struct Vec3D
        {
            public decimal Dx { get; }
            public decimal Dy { get; }
            public decimal Dz { get; }
            public string PointCodePair { get; }

            public Vec3D(decimal x1, decimal y1, decimal z1, decimal x2, decimal y2, decimal z2, string pointCodeStart, string pointCodeEnd)
            {
                Dx = x2 - x1;
                Dy = y2 - y1;
                Dz = z2 - z1;
                PointCodePair = $"[{pointCodeStart}]→[{pointCodeEnd}]";
            }

            /// <summary>点积运算</summary>
            public decimal Dot(in Vec3D other)
            {
                return Dx * other.Dx + Dy * other.Dy + Dz * other.Dz;
            }

            /// <summary>判断是否零向量（两点完全重合）</summary>
            public bool IsZero()
            {
                return Dx == 0m && Dy == 0m && Dz == 0m;
            }
            /// <summary>
            /// 判断是否零向量（两点保留N位小数的情况下重合）
            /// </summary>
            /// <param name="scale">默认4位小数</param>
            /// <param name="mr">默认取四舍五入</param>
            /// <returns></returns>
            public bool IsZeroByScale(int scale = 4, MidpointRounding mr = MidpointRounding.AwayFromZero)
            {
                // vec: PStart -> PEnd
                decimal dx = Math.Round(this.Dx, scale, mr);
                decimal dy = Math.Round(this.Dy, scale, mr);
                decimal dz = Math.Round(this.Dz, scale, mr);
                return dx == 0m && dy == 0m && dz == 0m;
            }
        }

        /// <summary>
        /// 测点序列方向校验
        /// 1.全局校验：PointCode不能重复；4位小数精度下坐标不可重合
        /// 2.同测位PositionName组内校验：严格按照Excel原始导入顺序，逐行流式校验本组前进方向
        /// 3.跨测位校验：所有有效测位基准向量之间方向保持一致
        /// 状态缓存：
        ///   prevRowDict：每个PositionName最近出现的一行，用来构造相邻向量
        ///   posBaseDict：每个PositionName的基准向量，该测位读到第2行时生成；前两点重合则不存入
        /// </summary>
        /// <typeparam name="T">测点行类型</typeparam>
        /// <param name="parsed">解析结果容器</param>
        public static void ValidateSequenceDirection<T>(SteelBeamParseResult<T> parsed)
            where T : ISteelBeamPointRow
        {
            var rows = parsed.Rows;
            if (rows.Count < 2)
                return;

            // ========== 全局校验缓存 ==========
            var pointCodeSet = new HashSet<string>();
            var seenCoordList = new List<(decimal X4, decimal Y4, decimal Z4, string PointCode)>();

            // ========== 流式状态字典 ==========
            // key:PositionName value:该测位上一条测点
            var prevRowDict = new Dictionary<string, T>();
            // key:PositionName value:该测位基准向量（读取到本组第2个点且两点不重合才存入）
            var posBaseDict = new Dictionary<string, Vec3D>();

            foreach (var row in rows)
            {
                // -------------------- 全局校验1：PointCode非空+全局唯一 --------------------
                if (string.IsNullOrWhiteSpace(row.PointCode))
                {
                    parsed.Errors.Add(new SteelBeamImportError(
                        ExcelRow: row.ExcelRow,
                        PointCode: row.PointCode,
                        FieldName: nameof(ISteelBeamPointRow.PointCode),
                        OriginalValue: row.PointCode,
                        Reason: $"测点编号不能为空或空白"));
                }
                else if (!pointCodeSet.Add(row.PointCode))
                {
                    parsed.Errors.Add(new SteelBeamImportError(
                        ExcelRow: row.ExcelRow,
                        PointCode: row.PointCode,
                        FieldName: nameof(ISteelBeamPointRow.PointCode),
                        OriginalValue: row.PointCode,
                        Reason: $"测点编号[{row.PointCode}]在导入列表中重复，同一文件内PointCode必须唯一"));
                }

                // -------------------- 全局校验2：4位小数坐标全局查重 --------------------
                decimal x4 = Math.Round(row.X, 4, MidpointRounding.AwayFromZero);
                decimal y4 = Math.Round(row.Y, 4, MidpointRounding.AwayFromZero);
                decimal z4 = Math.Round(row.Z, 4, MidpointRounding.AwayFromZero);

                bool coordDuplicateFound = false;
                foreach (var existed in seenCoordList)
                {
                    if (x4 == existed.X4 && y4 == existed.Y4 && z4 == existed.Z4)
                    {
                        parsed.Errors.Add(new SteelBeamImportError(
                            ExcelRow: row.ExcelRow,
                            PointCode: row.PointCode,
                            FieldName: $"{nameof(ISteelBeamPointRow.X)},{nameof(ISteelBeamPointRow.Y)},{nameof(ISteelBeamPointRow.Z)}",
                            OriginalValue: $"X={row.X},Y={row.Y},Z={row.Z}",
                            Reason: $"测点[{row.PointCode}]坐标（4位小数精度下）与前面测点[{existed.PointCode}]坐标重合，不允许坐标完全重合测点"));
                        coordDuplicateFound = true;
                        break;
                    }
                }
                if (!coordDuplicateFound)
                {
                    seenCoordList.Add((x4, y4, z4, row.PointCode));
                }

                // -------------------- 同测位 流式方向校验核心逻辑 --------------------
                string posName = row.PositionName;
                if (prevRowDict.TryGetValue(posName, out var lastRow))
                {
                    // 存在同测位上一行，构造【上一行 → 当前行】向量
                    var segVec = new Vec3D(lastRow.X, lastRow.Y, lastRow.Z, row.X, row.Y, row.Z, lastRow.PointCode, row.PointCode);

                    if (posBaseDict.TryGetValue(posName, out var baseVec))
                    {
                        // 该测位基准向量已存在（>=第3个测点）：执行方向校验
                        decimal dot = baseVec.Dot(segVec);
                        if (dot <= 0m)
                        {
                            parsed.Errors.Add(new SteelBeamImportError(
                                ExcelRow: row.ExcelRow,
                                PointCode: row.PointCode,
                                FieldName: $"{nameof(ISteelBeamPointRow.X)},{nameof(ISteelBeamPointRow.Y)},{nameof(ISteelBeamPointRow.Z)}",
                                OriginalValue: $"X={row.X},Y={row.Y},Z={row.Z}",
                                Reason: $"测位[{posName}]测点序列前进方向与本组基准({baseVec.PointCodePair})不一致；当前段[{lastRow.PointCode}]→[{row.PointCode}]向量夹角大于等于90°"));
                        }
                    }
                    else
                    {
                        // 当前是该测位【第2条测点】，生成基准向量
                        var newBaseVec = new Vec3D(lastRow.X, lastRow.Y, lastRow.Z, row.X, row.Y, row.Z, lastRow.PointCode, row.PointCode);
                        if (!newBaseVec.IsZeroByScale())
                        {
                            posBaseDict[posName] = newBaseVec;
                            // 扩展：给Vec3D增加辅助字段，保存基准点PointCode，用于报错文案
                            // public string PointCodePair { get; } = $"{p0}→{p1}";
                        }
                        else
                        {
                            // 本组前两点重合，基准无效，后续该测位不再做方向校验
                            parsed.Errors.Add(new SteelBeamImportError(
                                ExcelRow: row.ExcelRow,
                                PointCode: row.PointCode,
                                FieldName: $"{nameof(ISteelBeamPointRow.X)},{nameof(ISteelBeamPointRow.Y)},{nameof(ISteelBeamPointRow.Z)}",
                                OriginalValue: $"X={row.X},Y={row.Y},Z={row.Z}",
                                Reason: $"测位[{posName}]基准测点{lastRow.PointCode}与{row.PointCode}在4位小数精度下坐标重合，无法确定该测位基准前进方向"));
                            // 不加入posBaseDict，后续本测位不再校验方向
                        }
                    }
                }

                // 更新：当前行作为本测位的上一行，供下一次循环使用
                prevRowDict[posName] = row;
            }

            // ========== 循环结束：跨测位基准向量两两比对 ==========
            var vecList = posBaseDict.ToList();
            for (int i = 0; i < vecList.Count; i++)
            {
                for (int j = i + 1; j < vecList.Count; j++)
                {
                    var (posA, vecA) = vecList[i];
                    var (posB, vecB) = vecList[j];
                    decimal crossDot = vecA.Dot(vecB);
                    if (crossDot <= 0m)
                    {
                        parsed.Errors.Add(new SteelBeamImportError(
                            ExcelRow: null,
                            PointCode: string.Empty,
                            FieldName: string.Empty,
                            OriginalValue: string.Empty,
                            Reason: $"跨测位方向校验失败：测位[{posA}]和测位[{posB}]的前进基准方向不一致，夹角≥90°"));
                    }
                }
            }
        }



        /// <summary>
        /// List<List<decimal>> 转 decimal[,] 二维数组
        /// </summary>
        public static decimal[,] To2DArray(this List<List<decimal>> source)
        {
            if (source == null || source.Count == 0)
                return new decimal[0, 0];

            int rows = source.Count;
            int cols = source[0].Count;
            // 校验：所有行长度相同
            if (source.Any(r => r.Count != cols))
                throw new ArgumentException("各行的列数量不一致，无法转为矩形二维数组");

            decimal[,] result = new decimal[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                var row = source[i];
                for (int j = 0; j < cols; j++)
                {
                    result[i, j] = row[j];
                }
            }
            return result;
        }

        /// <summary>
        /// decimal[,] 二维数组 转 List<List<decimal>>
        /// 行优先：arr[i,j] → 外层List索引i（行号），内层List索引j（列号）
        /// 适配你的3行N列：0=X，1=Y，2=Z，每一行的list存放全部测点
        /// </summary>
        public static List<List<decimal>> ToListList(this decimal[,] arr)
        {
            if (arr == null)
                throw new ArgumentNullException(nameof(arr));

            int rows = arr.GetLength(0);
            int cols = arr.GetLength(1);

            var result = new List<List<decimal>>(rows);
            for (int i = 0; i < rows; i++)
            {
                var rowList = new List<decimal>(cols);
                for (int j = 0; j < cols; j++)
                {
                    rowList.Add(arr[i, j]);
                }
                result.Add(rowList);
            }
            return result;
        }
    }


}
