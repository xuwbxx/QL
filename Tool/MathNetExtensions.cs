using MathNet.Numerics.LinearAlgebra;

namespace Tool
{
    public static class MathNetExtensions
    {
        /// <summary>
        /// 行求和 (对应 np.sum(axis=1))
        /// </summary>
        public static Vector<double> RowSums(this Matrix<double> matrix)
        {
            var result = Vector<double>.Build.Dense(matrix.RowCount);
            for (int i = 0; i < matrix.RowCount; i++)
            {
                double sum = 0;
                for (int j = 0; j < matrix.ColumnCount; j++)
                {
                    sum += matrix[i, j];
                }
                result[i] = sum;
            }
            return result;
        }

        /// <summary>
        /// 向量逐元素除法 (对应 Python 的 /)
        /// </summary>
        public static Vector<double> ElementWiseDivide(this Vector<double> a, Vector<double> b)
        {
            var result = Vector<double>.Build.Dense(a.Count);
            for (int i = 0; i < a.Count; i++)
            {
                result[i] = b[i] != 0 ? a[i] / b[i] : 0;
            }
            return result;
        }

        /// <summary>
        /// 提取矩阵列 (对应 Python 的 [:, indices])
        /// </summary>
        public static Matrix<double> ExtractColumns(this Matrix<double> matrix, int[] indices)
        {
            var result = Matrix<double>.Build.Dense(matrix.RowCount, indices.Length);
            for (int i = 0; i < indices.Length; i++)
            {
                result.SetColumn(i, matrix.Column(indices[i]));
            }
            return result;
        }
    }
}
