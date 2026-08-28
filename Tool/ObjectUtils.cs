using System.Reflection;
using System.Text.Json;

namespace Tool
{
    /// <summary>
    /// 对象操作辅助类
    /// </summary>
    public class ObjectUtils
    {
        #region 创建实例

        /// <summary>
        /// 创建 T 类型的实例
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <returns>实例</returns>
        public static T Create<T>() => Activator.CreateInstance<T>();

        /// <summary>
        /// 创建指定类型的实例
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>实例</returns>
        public static object Create(Type type) => Activator.CreateInstance(type);

        #endregion

        #region 属性读写

        /// <summary>
        /// 获取对象指定属性的值
        /// </summary>
        /// <param name="instance">对象</param>
        /// <param name="propertyName">属性名称</param>
        /// <returns>属性值</returns>
        public static object GetPropertyValue(object instance, string propertyName)
        {
            if (instance == null) return null;
            if (string.IsNullOrEmpty(propertyName)) return null;

            var type = instance.GetType();
            var property = type.GetProperty(propertyName);
            if (property != null && property.CanRead)
            {
                return property.GetValue(instance, null);
            }

            var field = type.GetField(propertyName);
            if (field != null)
            {
                return field.GetValue(instance);
            }

            return null;
        }

        /// <summary>
        /// 设置对象指定属性的值（支持隐式类型转换）
        /// </summary>
        /// <param name="instance">对象</param>
        /// <param name="propertyName">属性名称</param>
        /// <param name="value">值</param>
        /// <returns>对象本身（支持链式调用）</returns>
        public static object SetPropertyValue(object instance, string propertyName, object value)
        {
            if (instance == null) return null;
            if (string.IsNullOrEmpty(propertyName)) return instance;

            var type = instance.GetType();

            // 优先查找属性
            var property = type.GetProperty(propertyName);
            if (property != null && property.CanWrite)
            {
                var convertedValue = ChangeType(value, property.PropertyType);
                property.SetValue(instance, convertedValue, null);
                return instance;
            }

            // 查找字段
            var field = type.GetField(propertyName);
            if (field != null)
            {
                var convertedValue = ChangeType(value, field.FieldType);
                field.SetValue(instance, convertedValue);
                return instance;
            }

            return instance;
        }

        /// <summary>
        /// 判断对象是否包含指定属性
        /// </summary>
        /// <param name="instance">对象</param>
        /// <param name="propertyName">属性名称</param>
        /// <returns>是否包含</returns>
        public static bool ContainProperty(object instance, string propertyName)
        {
            if (instance == null || string.IsNullOrEmpty(propertyName)) return false;
            return instance.GetType().GetProperty(propertyName) != null;
        }

        #endregion

        #region 对象复制

        /// <summary>
        /// 默认忽略的属性列表（审计字段）
        /// </summary>
        public static readonly List<string> DefaultSkipList = new List<string>
        {
            "ID", "Id", "id", "guid", "GUID",
            "CreatedBy", "CreatedByID", "CreatedByTime", "CreateBy", "CreateTime", "CreateByID",
            "CreatedTime", "CreatedByName", "CreateByName",
            "UpdatedBy", "UpdatedByID", "UpdatedByTime", "UpdateBy", "UpdateTime", "UpdateByID",
            "UpdatedTime", "UpdatedByName", "UpdatedTime"
        };

        /// <summary>
        /// 默认忽略的字段字符串（逗号分隔）
        /// </summary>
        public static readonly string DefaultSkipListStr = string.Join(",", DefaultSkipList);

        /// <summary>
        /// 根据源对象给目标对象的属性赋值（仅赋值给有相同名称和类别的属性）
        /// </summary>
        /// <param name="source">源对象</param>
        /// <param name="destination">目标对象</param>
        /// <param name="skipProperties">忽略的属性名列表</param>
        /// <param name="fillEmptyOnly">是否只给空值赋值（默认为否）</param>
        /// <param name="sourceProperties">仅复制传入的属性值</param>
        /// <returns>影响属性数量</returns>
        public static int CopyObjectValue(object source, object destination, List<string> skipProperties = null,
            bool fillEmptyOnly = false, List<string> sourceProperties = null)
        {
            if (source == null || destination == null) return 0;

            var count = 0;
            var typeSource = source.GetType();
            var typeDestination = destination.GetType();

            var propsSource = typeSource.GetProperties();
            var propsDestination = typeDestination.GetProperties();

            skipProperties ??= new List<string>();

            // 过滤源属性
            propsSource = propsSource
                .Where(p => !skipProperties.Contains(p.Name))
                .Where(p => sourceProperties == null || sourceProperties.Contains(p.Name))
                .Where(p => p.CanRead)
                .ToArray();

            foreach (var prop in propsSource)
            {
                // 在目标对象中寻找相同名称和类型的属性
                var propDestination = propsDestination
                    .FirstOrDefault(p => p.Name == prop.Name && p.PropertyType == prop.PropertyType);

                if (propDestination == null || !propDestination.CanWrite) continue;

                var sourceValue = prop.GetValue(source, null);
                var destValue = propDestination.GetValue(destination, null);

                // 如果只为空值赋值，且目标值不为空，则跳过
                if (fillEmptyOnly && destValue != null) continue;

                // 如果值不同，则赋值
                if (!Equals(sourceValue, destValue))
                {
                    propDestination.SetValue(destination, sourceValue, null);
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// 深拷贝：递归复制所有属性（包括引用类型）
        /// </summary>
        public static T DeepCopy<T>(T source) where T : class, new()
        {
            if (source == null) return null;

            var dest = new T();
            DeepCopyInternal(source, dest, new HashSet<object>());
            return dest;
        }

        private static void DeepCopyInternal(object source, object dest, HashSet<object> visited)
        {
            if (source == null || dest == null) return;
            if (visited.Contains(source)) return;
            visited.Add(source);

            var type = source.GetType();
            var props = type.GetProperties();

            foreach (var prop in props)
            {
                if (!prop.CanRead) continue;
                var value = prop.GetValue(source, null);
                if (value == null) continue;

                // 如果是值类型或字符串，直接复制
                var propType = prop.PropertyType;
                if (propType.IsValueType || propType == typeof(string))
                {
                    if (prop.CanWrite)
                        prop.SetValue(dest, value, null);
                }
                // 如果是集合类型，递归复制
                else if (typeof(System.Collections.IEnumerable).IsAssignableFrom(propType))
                {
                    // 集合的深拷贝逻辑（略）
                }
                // 如果是引用类型，递归深拷贝
                else if (prop.CanWrite)
                {
                    var newValue = Activator.CreateInstance(propType);
                    DeepCopyInternal(value, newValue, visited);
                    prop.SetValue(dest, newValue, null);
                }
            }
        }

        #endregion

        #region 类型判断与转换

        /// <summary>
        /// 判断类型是否为可空类型
        /// </summary>
        public static bool IsNullable(Type t) =>
            t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);

        /// <summary>
        /// 获取可空类型的内部类型
        /// </summary>
        public static Type GetNullableInnerType(Type t) =>
            IsNullable(t) ? t.GetGenericArguments()[0] : t;

        /// <summary>
        /// 判断对象是否为内置类型（命名空间为 System）
        /// </summary>
        public static bool IsBuildInType(object obj) =>
            obj != null && obj.GetType().Namespace == "System";

        /// <summary>
        /// 判断类型 T 是否为可空类型
        /// </summary>
        public static bool IsNullable<T>(T obj) =>
            obj == null || !typeof(T).IsValueType || Nullable.GetUnderlyingType(typeof(T)) != null;

        /// <summary>
        /// 类型转换（支持可空类型和隐式转换）
        /// </summary>
        public static object ChangeType(object value, Type type)
        {
            if (value == null)
            {
                // 如果是可空类型，返回 null
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    return null;
                // 如果是值类型，返回默认值
                if (type.IsValueType)
                    return Activator.CreateInstance(type);
                return null;
            }

            var targetType = Nullable.GetUnderlyingType(type) ?? type;

            // 如果类型相同，直接返回
            if (targetType == value.GetType())
                return value;

            // 处理枚举
            if (targetType.IsEnum)
            {
                if (value is string str)
                    return Enum.Parse(targetType, str);
                return Enum.ToObject(targetType, value);
            }

            // 处理 Guid
            if (targetType == typeof(Guid))
            {
                if (value is string guidStr)
                    return new Guid(guidStr);
                if (value is byte[] bytes)
                    return new Guid(bytes);
            }

            // 处理 Version
            if (targetType == typeof(Version) && value is string versionStr)
                return new Version(versionStr);

            // 处理 IConvertible
            if (value is IConvertible)
            {
                try
                {
                    return Convert.ChangeType(value, targetType);
                }
                catch (InvalidCastException)
                {
                    // 转换失败，尝试其他方式
                }
            }

            // 最后尝试直接转换（支持隐式转换的类型）
            try
            {
                return Convert.ChangeType(value, targetType);
            }
            catch
            {
                return value;
            }
        }

        #endregion

        #region 对象转字典

        /// <summary>
        /// 将对象转换为字典（只包含基本类型和字符串）
        /// </summary>
        public static Dictionary<string, object> ObjectToDictionary<T>(T obj)
        {
            var dict = new Dictionary<string, object>();

            if (obj == null) return dict;

            var type = obj.GetType();

            // 处理字段
            foreach (var field in type.GetFields())
            {
                var t = field.FieldType;
                var innerType = GetNullableInnerType(t);
                if (innerType.IsPrimitive || innerType == typeof(string))
                {
                    dict[field.Name] = field.GetValue(obj);
                }
            }

            // 处理属性
            foreach (var prop in type.GetProperties())
            {
                if (!prop.CanRead) continue;
                var t = prop.PropertyType;
                var innerType = GetNullableInnerType(t);
                if (innerType.IsPrimitive || innerType == typeof(string))
                {
                    dict[prop.Name] = prop.GetValue(obj, null);
                }
            }

            return dict;
        }

        #endregion

        #region 按字段名读写值

        /// <summary>
        /// 从对象中按字段名获取值
        /// </summary>
        public static T GetValueByFieldName<T>(object obj, string fieldName, T defValue = default)
        {
            if (obj == null || string.IsNullOrEmpty(fieldName)) return defValue;

            var prop = obj.GetType().GetProperty(fieldName);
            if (prop != null && prop.CanRead)
            {
                var value = prop.GetValue(obj, null);
                if (value != null)
                {
                    try
                    {
                        return (T)ChangeType(value, typeof(T));
                    }
                    catch
                    {
                        return defValue;
                    }
                }
                return defValue;
            }

            var field = obj.GetType().GetField(fieldName);
            if (field != null)
            {
                var value = field.GetValue(obj);
                if (value != null)
                {
                    try
                    {
                        return (T)ChangeType(value, typeof(T));
                    }
                    catch
                    {
                        return defValue;
                    }
                }
            }

            return defValue;
        }

        /// <summary>
        /// 从 JSON 字符串中按字段名获取值（使用 System.Text.Json）
        /// </summary>
        public static T GetValueByFieldName<T>(string objStr, string fieldName, T defValue = default)
        {
            if (string.IsNullOrEmpty(objStr) || string.IsNullOrEmpty(fieldName)) return defValue;

            var json = objStr.StartsWith("{") ? objStr : "{" + objStr + "}";
            try
            {
                using var document = JsonDocument.Parse(json);
                if (document.RootElement.TryGetProperty(fieldName, out var element))
                {
                    var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

                    // 特殊处理 DateTime
                    if (targetType == typeof(DateTime))
                    {
                        if (element.TryGetDateTime(out var dateTime))
                        {
                            return (T)(object)dateTime;
                        }
                        return defValue;
                    }

                    // 特殊处理 DateTimeOffset
                    if (targetType == typeof(DateTimeOffset))
                    {
                        if (element.TryGetDateTimeOffset(out var dateTimeOffset))
                        {
                            return (T)(object)dateTimeOffset;
                        }
                        return defValue;
                    }

                    // 特殊处理 Guid
                    if (targetType == typeof(Guid))
                    {
                        if (element.TryGetGuid(out var guid))
                        {
                            return (T)(object)guid;
                        }
                        return defValue;
                    }

                    // 其他类型使用标准反序列化
                    return JsonSerializer.Deserialize<T>(element.GetRawText()) ?? defValue;
                }
            }
            catch
            {
                // JSON 解析失败，返回默认值
            }

            return defValue;
        }

        /// <summary>
        /// 按字段名设置对象的值
        /// </summary>
        public static bool SetValueByFieldName<T>(object obj, string fieldName, T value)
        {
            if (obj == null || string.IsNullOrEmpty(fieldName)) return false;

            try
            {
                var prop = obj.GetType().GetProperty(fieldName);
                if (prop != null && prop.CanWrite)
                {
                    var convertedValue = ChangeType(value, prop.PropertyType);
                    prop.SetValue(obj, convertedValue, null);
                    return true;
                }

                var field = obj.GetType().GetField(fieldName);
                if (field != null)
                {
                    var convertedValue = ChangeType(value, field.FieldType);
                    field.SetValue(obj, convertedValue);
                    return true;
                }
            }
            catch
            {
                // 忽略异常
            }

            return false;
        }

        #endregion

        #region 获取变更属性

        /// <summary>
        /// 获取两个对象之间变更的属性列表
        /// </summary>
        public static List<PropertyField> GetChangedProperties(object originalObject, object newObject, bool checkType = false)
        {
            var result = new List<PropertyField>();

            if (originalObject == null || newObject == null) return result;

            var typeOriginal = originalObject.GetType();
            var typeNew = newObject.GetType();

            var propsOriginal = typeOriginal.GetProperties();
            var propsNew = typeNew.GetProperties();

            // 使用 HashSet 记录已处理的路径，防止循环引用
            var processedPaths = new HashSet<string>();

            foreach (var prop in propsOriginal)
            {
                if (!prop.CanRead) continue;

                var propNew = propsNew.FirstOrDefault(p => p.Name == prop.Name && p.PropertyType == prop.PropertyType);
                if (propNew == null || !propNew.CanRead) continue;

                var originalValue = prop.GetValue(originalObject, null);
                var newValue = propNew.GetValue(newObject, null);

                if (!Equals(originalValue, newValue))
                {
                    var field = new PropertyField();
                    field.SetValue(prop.Name, newValue);

                    // 如果是复杂类型，递归比较
                    if (!field.IsBuildInType && originalValue != null && newValue != null)
                    {
                        var pathKey = $"{prop.Name}";
                        if (!processedPaths.Contains(pathKey))
                        {
                            processedPaths.Add(pathKey);
                            var childChanges = GetChangedProperties(originalValue, newValue, checkType);
                            foreach (var child in childChanges)
                            {
                                child.Prefix(prop.Name);
                            }
                            result.AddRange(childChanges);
                        }
                    }

                    result.Add(field);
                }
            }

            return result;
        }

        #endregion

        #region 清理无效日期

        /// <summary>
        /// 将对象中所有 DateTime/DateTime? 类型的属性/字段中，
        /// 年份小于等于 1900 的值设置为 null。
        /// 用于处理前端控件（如 Element Plus）或历史遗留数据产生的无效日期。
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">需要清理的对象</param>
        /// <returns>是否成功执行</returns>
        public static bool ClearDateTime1900<T>(T obj)
        {
            if (obj == null) return false;

            var type = typeof(T);

            // 获取所有 DateTime? 类型的字段（public + private + 继承）
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(f => f.FieldType == typeof(DateTime?))
                .ToList();

            // 获取所有 DateTime? 类型的属性（public + private + 继承）
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(p => p.PropertyType == typeof(DateTime?) && p.CanWrite)
                .ToList();

            foreach (var field in fields)
            {
                var value = (DateTime?)field.GetValue(obj);
                if (value.HasValue && value.Value.Year <= 1900)
                {
                    field.SetValue(obj, null);
                }
            }

            foreach (var prop in properties)
            {
                var value = (DateTime?)prop.GetValue(obj);
                if (value.HasValue && value.Value.Year <= 1900)
                {
                    prop.SetValue(obj, null);
                }
            }

            return true;
        }

        #endregion
    }
    /// <summary>
    /// 属性字段信息，用于记录对象属性变更
    /// </summary>
    public class PropertyField
    {
        /// <summary>
        /// 路径（支持嵌套属性，如 "User.Address.City"）
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// 属性名称
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 完整名称
        /// </summary>
        public string FullName { get; private set; }

        /// <summary>
        /// 属性值
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// 属性类型
        /// </summary>
        public Type FieldType { get; private set; }

        /// <summary>
        /// 是否为内置类型（命名空间为 System）
        /// </summary>
        public bool IsBuildInType => FieldType?.Namespace == "System";

        /// <summary>
        /// 从对象中设置属性值
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">目标对象</param>
        /// <param name="prop">属性信息</param>
        public void SetValue<T>(T obj, PropertyInfo prop)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (prop == null) throw new ArgumentNullException(nameof(prop));

            Name = prop.Name;
            Path = prop.Name;
            FullName = prop.PropertyType.FullName ?? prop.PropertyType.Name;
            Value = prop.GetValue(obj, null);
            FieldType = prop.PropertyType;
        }

        /// <summary>
        /// 直接设置属性值
        /// </summary>
        /// <param name="name">属性名称</param>
        /// <param name="value">属性值</param>
        public void SetValue(string name, object value)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

            Name = name;
            Path = name;
            Value = value;
            FieldType = value?.GetType() ?? typeof(object);
            FullName = name;
        }

        /// <summary>
        /// 为路径添加前缀（用于嵌套属性）
        /// </summary>
        /// <param name="prefix">前缀</param>
        public void Prefix(string prefix)
        {
            if (string.IsNullOrEmpty(prefix)) return;
            Path = prefix + "." + Path;
        }

        /// <summary>
        /// 判断两个 PropertyField 是否相等（基于路径）
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is PropertyField other && Path == other.Path;
        }

        /// <summary>
        /// 获取哈希值
        /// </summary>
        public override int GetHashCode()
        {
            return Path?.GetHashCode() ?? 0;
        }

        /// <summary>
        /// 返回字符串表示
        /// </summary>
        public override string ToString()
        {
            return $"{Path} = {Value} ({FieldType?.Name})";
        }
    }
}
