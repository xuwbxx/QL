namespace Model.Base
{
    public class BaseApiReturn<T>
    {
        public BaseApiReturn()
        {
            Success = false;
        }
        public T? Data { set; get; }

        public bool Success { set; get; }

        public string? Message { set; get; }

    }
}
