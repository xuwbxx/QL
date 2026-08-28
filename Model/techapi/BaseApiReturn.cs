namespace Model.TechApi
{
    public class BaseApiReturn<T>
    {
        public BaseApiReturn()
        {
            Success = false;
        }
        public bool Success { set; get; }
        public string Message { set; get; }

        public T Data { set; get; }
    }
}
