using Service.Towing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Business.Towing
{
    public class ResponseBase<T>
    {
        public string Status { set; get; }
        public string Message { set; get; }
        public string ErrorMessage { set; get; }
        public T Data { set; get; }
        public List<T> List { set; get; }
        public int TotalRowNum { set; get; }
        public int PageCount { set; get; }
        public int PageIndex { set; get; }
        public int PageSize { set; get; }

        public bool IsValid => Status != ((int)HttpStatusCode.InternalServerError).ToString();

        public ResponseBase()
        {
            this.Data = Utils.CreateInstance<T>();
            this.List = new List<T>();// Utils.CreateInstance<List<T>>();
        }

        public void SetError(string message)
        {
            this.ErrorMessage = message;
            this.Status = ((int)HttpStatusCode.InternalServerError).ToString();
        }
        public void SetError(Exception ex)
        {
            this.SetError(ex.Message);
        }
        public void SetOK(string message = "")
        {
            this.Message = string.IsNullOrEmpty(message) ? "success" : $"success:{message}";
            this.Status = ((int)HttpStatusCode.OK).ToString();
        }
    }
}
