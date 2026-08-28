using System.Text.Json;

namespace Tool
{
    public class JsonUtils
    {

        public static string Serialize<T>(T obj)
        {
            try
            {
                return JsonSerializer.Serialize(obj);
            }
            catch (Exception ex)
            {
                // 可根据需求添加日志记录等操作
                //System.Console.WriteLine($"序列化出错: {ex.Message}");
                return null;
            }
        }

        public static T Deserialize<T>(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                // 可根据需求添加日志记录等操作
                //System.Console.WriteLine($"反序列化出错: {ex.Message}");
                return default;
            }
        }

    }
}
