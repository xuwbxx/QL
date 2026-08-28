using System.Data;
using System.Globalization;
using System.Reflection;

namespace Tool
{
    public class ListUtils
    {

        #region DataTable转指定模型的List
        /// <summary>
        /// 通用方法：将DataTable转换为指定模型的List<T>
        /// </summary>
        /// <typeparam name="T">目标模型类型（需有无参构造函数）</typeparam>
        /// <param name="dataTable">待转换的DataTable</param>
        /// <returns>模型列表List<T></returns>
        /// <exception cref="ArgumentNullException">DataTable为空</exception>
        public static List<T> ConvertDataTableToList<T>(DataTable dataTable) where T : new()
        {
            // 空值校验
            if (dataTable == null || dataTable.Rows.Count == 0)
            {
                throw new ArgumentNullException(nameof(dataTable), "DataTable不能为空或无数据");
            }

            List<T> resultList = new List<T>();
            // 获取模型的所有属性（忽略大小写匹配列名）
            PropertyInfo[] modelProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // 遍历DataTable的每一行
            foreach (DataRow row in dataTable.Rows)
            {
                // 创建模型实例（约束T必须有无参构造函数）
                T model = new T();

                // 遍历模型的每个属性，匹配DataTable的列
                foreach (PropertyInfo property in modelProperties)
                {
                    try
                    {
                        // 1. 检查DataTable是否包含该列（忽略大小写）
                        DataColumn column = dataTable.Columns.Cast<DataColumn>()
                            .FirstOrDefault(c => c.ColumnName.Equals(property.Name, StringComparison.OrdinalIgnoreCase));

                        if (column == null) continue; // 列名不匹配则跳过

                        // 2. 获取单元格值（处理DBNull）
                        object cellValue = row[column] == DBNull.Value ? null : row[column];
                        if (cellValue == null || string.IsNullOrEmpty(cellValue.ToString()))
                        {
                            continue; // 空值跳过赋值
                        }

                        // 3. 类型转换并赋值给模型属性
                        Type targetType = property.PropertyType;
                        // 处理可空类型（如int?、decimal?）
                        targetType = Nullable.GetUnderlyingType(targetType) ?? targetType;

                        object convertedValue = null;
                        if (targetType == typeof(string))
                        {
                            convertedValue = cellValue.ToString().Trim();
                        }
                        else if (targetType == typeof(int) || targetType == typeof(int?))
                        {
                            convertedValue = int.TryParse(cellValue.ToString(), out int intVal) ? intVal : 0;
                        }
                        else if (targetType == typeof(decimal) || targetType == typeof(decimal?))
                        {
                            convertedValue = decimal.TryParse(cellValue.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal decVal) ? decVal : 0m;
                        }
                        else if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
                        {
                            convertedValue = DateTime.TryParse(cellValue.ToString(), out DateTime dtVal) ? dtVal : DateTime.MinValue;
                        }
                        else if (targetType == typeof(bool) || targetType == typeof(bool?))
                        {
                            convertedValue = bool.TryParse(cellValue.ToString(), out bool boolVal) ? boolVal : false;
                        }
                        else
                        {
                            // 其他类型直接转换（如long、double等）
                            convertedValue = Convert.ChangeType(cellValue, targetType, CultureInfo.InvariantCulture);
                        }

                        // 给模型属性赋值
                        if (property.CanWrite) // 确保属性可写
                        {
                            property.SetValue(model, convertedValue, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        // 单个属性赋值失败不影响整体，记录日志（此处可替换为你的日志组件）
                        Console.WriteLine($"属性[{property.Name}]赋值失败：{ex.Message}");
                        continue;
                    }
                }

                resultList.Add(model);
            }

            return resultList;
        }
        #endregion

    }
}
