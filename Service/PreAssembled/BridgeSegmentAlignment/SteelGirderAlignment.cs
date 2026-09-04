using MathNet.Numerics.LinearAlgebra;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.PreAssembled.BridgeSegmentAlignment
{
    public class SteelGirderAlignment
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
        public decimal[,] GetComputeData(int projID, int girderID, string[] beamList)
        {
            // 
            return null;
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
        public decimal[,] Gescha(decimal[,] P, decimal[,] Q, decimal[,] D, decimal[,] R)
        {
            // decimal → double
            double[,] pDouble = ToDoubleMatrix(P);
            double[,] qDouble = ToDoubleMatrix(Q);
            double[,] dDouble = ToDoubleMatrix(D);
            double[,] rDouble = ToDoubleMatrix(R);

            // 调用原有double核心算法
            double[,] resDouble = Gescha(pDouble, qDouble, dDouble, rDouble);

            // double → decimal
            return ToDecimalMatrix(resDouble);
        }

        /// <summary>
        /// decimal[,] → double[,]
        /// </summary>
        public static double[,] ToDoubleMatrix(decimal[,] src)
        {
            int rows = src.GetLength(0);
            int cols = src.GetLength(1);
            double[,] dst = new double[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    dst[i, j] = (double)src[i, j];
                }
            }
            return dst;
        }

        /// <summary>
        /// double[,] → decimal[,]
        /// </summary>
        public static decimal[,] ToDecimalMatrix(double[,] src)
        {
            int rows = src.GetLength(0);
            int cols = src.GetLength(1);
            decimal[,] dst = new decimal[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    dst[i, j] = Convert.ToDecimal(src[i, j]);
                }
            }
            return dst;
        }

        /// <summary>
        /// 原版double核心实现（不变，供重载和离线Excel调用）
        /// P,Q: 3×N；D:3×M；R权重矩阵 3×N
        /// 返回 D1 3×M
        /// </summary>
        /// <param name="P">已施工理论点位矩阵（行x/y/z，列点位）</param>
        /// <param name="Q">已施工实测点位矩阵（行x/y/z，列点位）</param>
        /// <param name="D">待施工理论点位矩阵（行x/y/z，列点位）</param>
        /// <param name="R">点位坐标加权矩阵（行x/y/z，列点位）</param>
        /// <returns>待施工预测点位矩阵(D1 3×M 变换后坐标)</returns>
        public double[,] Gescha(double[,] P, double[,] Q, double[,] D, double[,] R)
        {
            // P输入矩阵：3行N列，已施工构件的理论坐标点集；每一列代表一个点(X,Y,Z)
            int nrow = P.GetLength(0);
            // Q输入矩阵：3行N列，已施工构件的实测坐标点集；每一列代表一个点(X,Y,Z)
            int ncol = P.GetLength(1);
            // D输入矩阵：3行M列，待预测变换的目标点集（未施工构件理论坐标）；每一列代表一个点(X,Y,Z)
            int nrow1 = D.GetLength(0);
            // R输入矩阵：3行N列，各个坐标点对应的权重矩阵，每个点三个维度共用同一个权重值
            int ncol1 = D.GetLength(1);

            // 将C#二维数组转换为MathNet矩阵对象，用于线性代数运算
            var matP = Matrix<double>.Build.DenseOfArray(P);
            var matQ = Matrix<double>.Build.DenseOfArray(Q);
            var matD = Matrix<double>.Build.DenseOfArray(D);
            var matR = Matrix<double>.Build.DenseOfArray(R);

            
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
            // VT：SVD分解输出右奇异向量的转置矩阵
            var VT = svd.VT;
            // V：右奇异向量矩阵，VT做转置还原得到V；Transpose()矩阵转置
            var V = VT.Transpose();
            // UT：左奇异向量矩阵U的转置矩阵；Transpose()矩阵转置
            var UT = U.Transpose();


            //# --- 步骤6: 计算旋转矩阵 R ---
            //# 此处R开始，是指旋转矩阵，不再是前面的权重矩阵
            //# R = V * U^T 是最优旋转矩阵（最小二乘意义下）
            // Rmat：初步计算得到旋转矩阵候选；矩阵乘法 V * UT
            Matrix<double> Rmat = V * UT;
            // detR：旋转矩阵Rmat的行列式；Determinant()行列式计算；det>0代表纯旋转，det<0说明发生镜像反射，需要修正
            double detR = Rmat.Determinant();


            //# --- 步骤7: 反射检测与修正 ---
            //# 桥梁施工中，理论上不应该出现镜像翻转（左右手坐标系变换）
            //# 如果出现 det_R < 0，说明SVD求出了包含反射的解
            //# 需要修正为纯旋转（避免出现"翻转"这种不合理的变换）
            // diagData：镜像修正对角线向量 [1,1,-1]，用于消除反射，保证输出为合法右手旋转矩阵
            var diagData = Vector<double>.Build.Dense(new double[] { 1.0, 1.0, -1.0 });
            // cha：3×3对角修正矩阵diag(1,1,-1)；DenseOfDiagonalVector根据向量构建对角方阵，用于行列式为负时的镜像校正
            // 1, 0, 0
            // 0, 1, 0
            // 0, 0,-1
            var cha = Matrix<double>.Build.DenseOfDiagonalVector(diagData);

            // t：平移向量；t = GQ − Rmat * GP；重心之间经过旋转后的偏移量
            Vector<double> t;
            if (detR > 0)
            {
                // 行列式>0，无镜像反射，旋转矩阵有效，直接计算平移向量
                //# 正常情况：纯旋转，直接使用
                //# 平移量 = 实测重心 - 旋转后的理论重心
                t = GQ - Rmat * GP;
            }
            else
            {
                // 行列式<0，存在镜像反射，引入cha对角矩阵修正旋转矩阵，消除反射分量
                //# 异常情况：出现反射，需要修正
                //# 将V的第三列取反，消除反射成分
                var RR = V * cha; // V 右奇异向量矩阵
                Rmat = RR * UT; // # 重新计算旋转矩阵（保证det_R>0）
                //# 平移量 = 实测重心 - 旋转后的理论重心
                t = GQ - Rmat * GP;
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
            //# D1 = R * D + T
            //# 即：待安装节段的预测实际坐标 = 旋转 * 理论坐标 + 平移
            //# 这就是通过已完工节段的偏差，预测未完工节段的实际安装位置
            // D1：最终变换结果矩阵；Rmat * matD + Tmat；对目标点集做旋转 + 平移刚性变换，输出3行M列坐标矩阵
            var D1 = Rmat * matD + Tmat;
            // ToArray() 将MathNet矩阵对象转回C#原生double二维数组返回
            return D1.ToArray();
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
        public void RunSimulation(string excelPath, string outputExcel, int n, int m, int r)
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


        private List<int> SampleRange(Random rand, int min, int max, int count)
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

        private void WriteExcel(string filePath, double[,] data, string sheetName)
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
    }
}
