using System.Text;

namespace Tool
{
    public static class StringUtilis
    {
        /// <summary>
        /// PascalCase 转 snake_case
        /// 示例：BizName → biz_name；CreateTime → create_time
        /// </summary>
        public static string GetSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (char.IsUpper(c))
                {
                    if (i > 0)
                    {
                        sb.Append('_');
                    }
                    sb.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// PascalCase 转 snake_case
        /// 示例：BizName → biz_name；CreateTime → create_time
        /// </summary>
        /// <returns></returns>
        public static string ToSnakeCase(this string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            return GetSnakeCase(str);
        }
    }
}
