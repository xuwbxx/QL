namespace Model.TechApi
{
    public class BaseApiRequest<T>
    {
        public BaseApiRequest() { }

        public T Data { set; get; }

        public string Requester { set; get; }

        public string Token { set; get; }
    }
}
