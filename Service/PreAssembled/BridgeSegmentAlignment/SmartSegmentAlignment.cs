using MathNet.Numerics.LinearAlgebra;
using Tool;

namespace Service.PreAssembled.BridgeSegmentAlignment
{
    /// <summary>
    /// 智能方向检测与自动配准
    /// </summary>
    public class SmartSegmentAlignment
    {
        /// <summary>
        /// 自动检测并处理拼接方向变化
        /// </summary>
        public static Matrix<double> GeschaAutoDirection(
            Matrix<double> P,      // 理论坐标 (3, n)
            Matrix<double> Q,      // 实测坐标 (3, n)  
            Matrix<double> D,      // 待预测坐标 (3, m)
            Matrix<double> R)      // 权重 (3, n)
        {
            // --- 步骤1: 自动检测拼接方向 ---
            var directionInfo = DetectConstructionDirection(P, Q);

            Console.WriteLine($"检测到 {directionInfo.DirectionChanges + 1} 个拼接段");
            Console.WriteLine($"拼接方向: {string.Join(" → ", directionInfo.SegmentDirections)}");
            Console.WriteLine($"分段点: {string.Join(", ", directionInfo.ChangePoints)}");

            // --- 步骤2: 根据方向信息调整配准策略 ---
            if (directionInfo.DirectionChanges == 0)
            {
                // 方向一致，直接配准
                Console.WriteLine("所有节段方向一致，使用标准SVD配准");
                return Gescha(P, Q, D, R);
            }
            else
            {
                // 存在方向变化，分段处理
                Console.WriteLine($"检测到方向变化，采用分段配准策略");
                return GeschaWithAutoDirection(P, Q, D, R, directionInfo);
            }
        }

        /// <summary>
        /// 自动检测拼接方向
        /// </summary>
        private static ConstructionDirectionInfo DetectConstructionDirection(
            Matrix<double> P, Matrix<double> Q)
        {
            int n = P.ColumnCount;

            // 计算每个节段的中心位置（使用理论值和实测值的平均值）
            var centers = new double[n];
            for (int i = 0; i < n; i++)
            {
                // 使用理论和实测的平均位置作为节段中心
                double x_p = P[0, i];
                double x_q = Q[0, i];
                centers[i] = (x_p + x_q) / 2.0;
            }

            // --- 方法1: 基于空间位置排序 ---
            // 按X坐标排序，得到节段的物理顺序
            var sortedIndices = Enumerable.Range(0, n)
                                         .OrderBy(i => centers[i])
                                         .ToArray();

            // --- 方法2: 计算相邻节段的方向向量 ---
            var directions = new List<int>();
            var changePoints = new List<int>();

            // 从第二个节段开始，判断每个节段相对于前一个节段的延伸方向
            for (int i = 1; i < sortedIndices.Length; i++)
            {
                int prevIdx = sortedIndices[i - 1];
                int currIdx = sortedIndices[i];

                // 计算两个节段中心的位移向量
                double dx = centers[currIdx] - centers[prevIdx];

                // 判断方向：正值为从左向右，负值为从右向左
                // 注意：这里假设桥梁沿X方向延伸
                int dir = Math.Sign(dx);
                directions.Add(dir);

                // 如果方向发生变化，记录变化点
                if (i > 1 && directions[i - 2] != dir)
                {
                    changePoints.Add(currIdx);
                }
            }

            // --- 方法3: 计算每个节段的局部坐标轴方向 ---
            var localDirections = new int[n];
            for (int i = 0; i < n; i++)
            {
                localDirections[i] = DetermineLocalDirection(P, Q, i);
            }

            // --- 综合判断 ---
            var finalDirections = new List<int>();
            int currentDir = 1;  // 默认从左向右

            for (int i = 0; i < n; i++)
            {
                // 如果有明确的局部方向信息，使用它
                if (localDirections[i] != 0)
                {
                    finalDirections.Add(localDirections[i]);
                }
                else if (i < directions.Count)
                {
                    finalDirections.Add(directions[i]);
                }
                else
                {
                    finalDirections.Add(currentDir);
                }

                // 更新当前方向
                currentDir = finalDirections.Last();
            }

            return new ConstructionDirectionInfo
            {
                SegmentDirections = finalDirections.ToArray(),
                ChangePoints = changePoints.ToArray(),
                DirectionChanges = changePoints.Count
            };
        }

        /// <summary>
        /// 判断单个节段的局部方向
        /// </summary>
        private static int DetermineLocalDirection(Matrix<double> P, Matrix<double> Q, int index)
        {
            // 方法：分析节段的几何形状
            // 如果是预制节段，可能有明显的长轴方向

            // 1. 计算节段的理论坐标范围
            double xMin = P[0, index] - 0.5;  // 假设节段宽度
            double xMax = P[0, index] + 0.5;

            // 2. 分析相邻节段的位置
            // 如果节段左侧有邻居，且右侧没有，说明是起点
            // 如果节段右侧有邻居，且左侧没有，说明是终点

            // 这里简化处理：使用实测值判断
            // 实际项目中可以根据节段的几何数据判断

            return 0;  // 0表示无法判断，使用全局推断
        }

        /// <summary>
        /// 带自动方向的分段配准
        /// </summary>
        private static Matrix<double> GeschaWithAutoDirection(
            Matrix<double> P, Matrix<double> Q, Matrix<double> D,
            Matrix<double> R, ConstructionDirectionInfo directionInfo)
        {
            int n = P.ColumnCount;

            // --- 根据方向信息将节段分组 ---
            var groups = new List<List<int>>();
            var currentGroup = new List<int>();
            int currentDir = directionInfo.SegmentDirections[0];

            for (int i = 0; i < n; i++)
            {
                int dir = directionInfo.SegmentDirections[i];
                if (dir != currentDir && currentGroup.Count > 0)
                {
                    // 方向变化，开始新组
                    groups.Add(currentGroup);
                    currentGroup = new List<int>();
                    currentDir = dir;
                }
                currentGroup.Add(i);
            }
            if (currentGroup.Count > 0)
            {
                groups.Add(currentGroup);
            }

            Console.WriteLine($"将节段分为 {groups.Count} 组");
            for (int g = 0; g < groups.Count; g++)
            {
                Console.WriteLine($"  组{g + 1}: 节段 {string.Join(", ", groups[g].Select(i => i + 1))}");
            }

            // --- 对每组分别配准 ---
            var results = new List<Matrix<double>>();
            var weights = new List<double>();

            foreach (var group in groups)
            {
                var indices = group.ToArray();

                // 提取该组的节段
                var P_group = ExtractColumns(P, indices);
                var Q_group = ExtractColumns(Q, indices);
                var R_group = ExtractColumns(R, indices);

                // 对于从右向左的组，翻转坐标
                int groupDir = directionInfo.SegmentDirections[indices[0]];
                if (groupDir == -1)
                {
                    P_group = FlipCoordinates(P_group);
                    Q_group = FlipCoordinates(Q_group);
                }

                // 执行标准配准
                var result = Gescha(P_group, Q_group, D, R_group);
                results.Add(result);

                // 计算该组的权重（基于节段数量）
                weights.Add(indices.Length);
            }

            // --- 合并结果 ---
            return MergeResults(results, weights);
        }

        /// <summary>
        /// 提取指定列
        /// </summary>
        private static Matrix<double> ExtractColumns(Matrix<double> matrix, int[] indices)
        {
            var result = Matrix<double>.Build.Dense(matrix.RowCount, indices.Length);
            for (int i = 0; i < indices.Length; i++)
            {
                result.SetColumn(i, matrix.Column(indices[i]));
            }
            return result;
        }

        /// <summary>
        /// 翻转坐标（处理从右向左的节段）
        /// </summary>
        private static Matrix<double> FlipCoordinates(Matrix<double> matrix)
        {
            var result = matrix.Clone();
            // 翻转X轴（假设桥梁沿X方向）
            for (int i = 0; i < result.ColumnCount; i++)
            {
                result[0, i] = -result[0, i];
            }
            return result;
        }

        /// <summary>
        /// 合并多个配准结果
        /// </summary>
        private static Matrix<double> MergeResults(List<Matrix<double>> results, List<double> weights)
        {
            if (results.Count == 1)
                return results[0];

            // 加权平均
            double totalWeight = weights.Sum();
            var result = Matrix<double>.Build.Dense(results[0].RowCount, results[0].ColumnCount);

            for (int i = 0; i < results.Count; i++)
            {
                double w = weights[i] / totalWeight;
                result += results[i] * w;
            }

            return result;
        }

        /// <summary>
        /// 直接翻译自 Python 的 gescha 函数
        /// </summary>
        public static Matrix<double> Gescha(Matrix<double> P, Matrix<double> Q, Matrix<double> D, Matrix<double> R)
        {
            // Python: nrow, ncol = P.shape
            int nrow = P.RowCount;
            int ncol = P.ColumnCount;

            // Python: nrow1, ncol1 = D.shape
            int nrow1 = D.RowCount;
            int ncol1 = D.ColumnCount;

            // Python: wP = P * R  (逐元素乘法)
            var wP = P.PointwiseMultiply(R);

            // Python: wQ = Q * R  (逐元素乘法)
            var wQ = Q.PointwiseMultiply(R);

            // Python: G1 = np.sum(wP, axis=1) / np.sum(R, axis=1)
            var G1 = wP.RowSums().ElementWiseDivide(R.RowSums());

            // Python: G2 = np.sum(wQ, axis=1) / np.sum(R, axis=1)
            var G2 = wQ.RowSums().ElementWiseDivide(R.RowSums());

            // Python: PP = np.zeros((nrow, ncol))
            var PP = Matrix<double>.Build.Dense(nrow, ncol);

            // Python: QQ = np.zeros((nrow, ncol))
            var QQ = Matrix<double>.Build.Dense(nrow, ncol);

            // Python: for i in range(3): for j in range(ncol): PP[i,j] = P[i,j] - G1[i]
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < ncol; j++)
                {
                    PP[i, j] = P[i, j] - G1[i];
                    QQ[i, j] = Q[i, j] - G2[i];
                }
            }

            // Python: PT = np.transpose(PP)
            var PT = PP.Transpose();

            // Python: QT = np.transpose(QQ)
            var QT = QQ.Transpose();

            // Python: H = (R * PP) @ QT
            var H = (R.PointwiseMultiply(PP)) * QT;

            // Python: U, S, VT = np.linalg.svd(H)
            var svd = H.Svd();
            var U = svd.U;
            var S = svd.S;
            var VT = svd.VT;

            // Python: V = np.transpose(VT)
            var V = VT.Transpose();

            // Python: UT = np.transpose(U)
            var UT = U.Transpose();

            // Python: R = np.dot(V, UT)  (注意：此处R开始是旋转矩阵)
            var rotation = V * UT;

            // Python: det_R = np.linalg.det(R)
            double det_R = rotation.Determinant();

            // Python: cha = np.diag([1, 1, -1])
            var cha = Matrix<double>.Build.Diagonal(new double[] { 1.0, 1.0, -1.0 });

            // Python: T = np.zeros((3, ncol1))
            var T = Matrix<double>.Build.Dense(3, ncol1);

            // Python: if det_R > 0: t = G2 - np.dot(R, G1)
            Vector<double> t;
            if (det_R > 0)
            {
                t = G2 - rotation * G1;
            }
            else
            {
                // Python: RR = np.dot(V, cha)
                var RR = V * cha;

                // Python: R = np.dot(RR, UT)
                rotation = RR * UT;

                // Python: t = G2 - np.dot(R, G1)
                t = G2 - rotation * G1;
            }

            // Python: for i in range(3): for j in range(ncol1): T[i,j] = t[i]
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < ncol1; j++)
                {
                    T[i, j] = t[i];
                }
            }

            // Python: D1 = np.dot(R, D) + T
            var D1 = rotation * D + T;

            // Python: return D1
            return D1;
        }

        /// <summary>
        /// 直接翻译自 Python 的 ZERO 函数
        /// </summary>
        public static Matrix<double> ZeroOutColumn(Matrix<double> R, int columnIndex)
        {
            // Python: for j in range(3): R[j, i-1] = 0
            var result = R.Clone();
            int colIdx = columnIndex - 1;
            for (int j = 0; j < 3; j++)
            {
                result[j, colIdx] = 0;
            }
            return result;
        }
    }
}
