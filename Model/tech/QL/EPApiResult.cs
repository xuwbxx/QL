namespace Model.tech.QL
{
    /// <summary>
    /// Element Plus Api Result Base
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EPApiResult<T>
    {
        public int Code { get; set; }
        public string Msg { get; set; }
        public T Data { get; set; }
    }

    public class EPApiResult : EPApiResult<object>
    {
        public static EPApiResult<T> Success<T>(T data, string msg = "ok")
        {
            return new EPApiResult<T> { Code = 200, Msg = msg, Data = data };
        }

        public static EPApiResult<object> Fail(string msg)
        {
            return new EPApiResult<object> { Code = 500, Msg = msg, Data = null };
        }
    }
}
