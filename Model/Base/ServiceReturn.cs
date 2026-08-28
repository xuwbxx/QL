namespace Model.Base
{
    public class ServiceReturn<T>
    {

        public T Data { set; get; }

        public bool Success { set; get; } = false;

        public string Message { set; get; }

    }
}
