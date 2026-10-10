using Model.Tech.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Base
{
    //[Serializable]
    public class BaseApiRequest
    {
        public BaseApiRequest()
        {
            
        }

        public string? Token { set; get; }

        /// <summary>
        /// 项目编号
        /// </summary>
        public string? ProjectCode { set; get; }

        /// <summary>
        /// 请求方
        /// </summary>
        public string? Requester { set; get; }

        /// <summary>
        /// 密钥
        /// </summary>
        public string? Code { set; get; }

        /// <summary>
        /// 内容
        /// </summary>
        public string? Content { set; get; }

        /// <summary>
        /// 表名
        /// </summary>
        public string? TableName { set; get; }

    }


    public class TechCloudApiRequest
    {
        /// <summary>
        /// token
        /// </summary>
        public string? Token { set; get; }

        /// <summary>
        /// 项目编号
        /// </summary>
        public string? ProjectCode { set; get; }

        /// <summary>
        /// 平台名称
        /// </summary>
        public string? Requester { set; get; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string? UserID { set; get; }

        /// <summary>
        /// 登录时间
        /// </summary>
        public string? LoginTime { set; get; }

        /// <summary>
        /// 操作内容
        /// </summary>
        public string? Operation { set; get; }

        /// <summary>
        /// 操作时间
        /// </summary>
        public string? OperationTime { set; get; }


        /// <summary>
        /// IP地址
        /// </summary>
        public string? IPAddress { set; get; }


        /// <summary>
        /// Mac地址
        /// </summary>
        public string? MacAddress { set; get; }


        
    }

    public class TechCloudWindApiRequest
    {
        public TechCloudWindApiRequest()
        {
            TaskFiles = new List<CloudWindTaskFileResponse>();
        }
        /// <summary>
        /// 任务类型(CloudWindSoftware)
        /// </summary>
        public int? TaskTypeID { set; get; }

        /// <summary>
        /// 任务编号
        /// </summary>
        public int? TaskID { set; get; }

        public string? TaskCode { set; get; }

        public string? PostToken { set; get; }

        public string? Platform { set; get; }
        
        public List<CloudWindTaskFileResponse>? TaskFiles { set; get; }

    }



    public class SqlParamRequest : BaseApiRequest
    {
        public SqlParamRequest()
        {
            Params = new List<SqlParam>();
        }

        public string Sql { set; get; }

        public List<SqlParam> Params { set; get; }
    }

    public class SqlParam
    {
        public string? Name { get; set; }
        public object? Value { get; set; }
    }


    
}
