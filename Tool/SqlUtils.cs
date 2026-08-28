using System.Reflection;
using System.Text;

namespace Tool
{

    public static class SqlUtils
    {
        /// <summary>
        /// 实体对象转 SQL 插入语句（直接拼接值，非参数化）
        /// </summary>
        /// <typeparam name="T">实体类类型</typeparam>
        /// <param name="entity">带具体数据的实体对象</param>
        /// <param name="tableName">数据库表名（需与实体字段对应）</param>
        /// <returns>完整的 SQL 插入语句</returns>
        public static string ToInsertSql<T>(this T entity, string tableName)
            where T : class, new()
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "实体对象不能为 null");
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("表名不能为空", nameof(tableName));

            // 1. 获取实体所有公共可读写属性（排除 ID 自增字段，如需包含可删除 Where 条件）
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite && p.Name != "ID") // 排除自增 ID，按需调整
                .ToList();

            if (properties.Count == 0)
                throw new InvalidOperationException("实体类没有可插入的有效属性");

            // 2. 拼接字段名（UserCode, RealName, UserName...）
            var columnNames = string.Join(", ", properties.Select(p => p.Name));

            // 3. 拼接值（"2018001515", "程伟", "CHENGWEI"...）
            var valueBuilder = new StringBuilder();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(entity);
                valueBuilder.Append(FormatValue(value, prop.PropertyType));
                valueBuilder.Append(", ");
            }
            // 移除最后一个多余的 ", "
            var values = valueBuilder.ToString().TrimEnd(',', ' ');

            // 4. 构建完整 SQL
            return $"insert into {tableName}({columnNames}) values({values})";
        }

        /// <summary>
        /// 格式化值为 SQL 兼容格式（无需扩展方法，纯内置逻辑）
        /// </summary>
        private static string FormatValue(object? value, Type propertyType)
        {
            // 处理 null 值（数据库兼容的 NULL）
            if (value == null || value == DBNull.Value)
                return "NULL";

            // --------------- 字符串类型（含 string 和 string?）---------------
            // 逻辑：string? 编译后本质是 string，直接判断 propertyType == typeof(string) 即可
            if (propertyType == typeof(string))
            {
                // 转义单引号（避免 SQL 语法错误，如 "程'伟" → "程''伟"）
                var strValue = value.ToString()?.Replace("'", "''") ?? string.Empty;
                return $"'{strValue}'";
            }

            // --------------- DateTime 类型（含 DateTime 和 DateTime?）---------------
            // 先获取可空类型的底层类型（如 DateTime? 的底层类型是 DateTime）
            var underlyingType = Nullable.GetUnderlyingType(propertyType);
            bool isDateTimeType = propertyType == typeof(DateTime) || underlyingType == typeof(DateTime);
            if (isDateTimeType)
            {
                var dateValue = (DateTime)value;
                // 格式化为 Kingbase datetime 支持的 yyyy-MM-dd HH:mm:ss.fff
                return $"'{dateValue.ToString("yyyy-MM-dd HH:mm:ss.fff")}'";
            }

            // --------------- bool 类型（含 bool 和 bool?）---------------
            bool isBoolType = propertyType == typeof(bool) || underlyingType == typeof(bool);
            if (isBoolType)
            {
                // 数据库兼容：true → 1，false → 0
                return ((bool)value) ? "1" : "0";
            }

            // --------------- 数值类型（int, long, double, decimal 等，含可空版本）---------------
            bool isNumericType = propertyType.IsValueType &&
                                (propertyType.IsPrimitive || propertyType == typeof(decimal) ||
                                 (underlyingType != null && (underlyingType.IsPrimitive || underlyingType == typeof(decimal))));
            if (isNumericType)
            {
                // 数值直接转字符串（如 3 → "3"，100.5 → "100.5"）
                return value.ToString() ?? "0";
            }

            // --------------- 其他未明确类型（默认按字符串处理）---------------
            return $"'{value.ToString()?.Replace("'", "''") ?? string.Empty}'";
        }
    }

}
