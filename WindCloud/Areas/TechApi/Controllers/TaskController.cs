using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model.Base;
using Model.Tech.WebApi;
using Service.Api;

namespace WindCloud.Areas.TechApi.Controllers
{
    [Route("CCSHJTechApi/[controller]/[action]")]
    [ApiController]
    public class TaskController : BaseApiController
    {

        private readonly WindApiService _windApiService;

        public TaskController(WindApiService windApiService)
        {
            _windApiService = windApiService;
        }

        /// <summary>
        /// 发送插拔桩文件
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public WindCloudBaseReturn SendTaskFile(TechCloudWindApiRequest request)
        {
            var ret = new WindCloudBaseReturn();

            if (!Authored(request.PostToken))
            {
                ret.Success = false;
                ret.Message = "接口失效";
                return ret;
            }

            if (request.TaskID == 0)
            {
                ret.Message = "TaskID不能为空";
                ret.Success = false;
                return ret;
            }

            if (request.TaskTypeID == 0)
            {
                ret.Message = "TaskTypeID不能为空";
                ret.Success = false;
                return ret;
            }

            return _windApiService.SendTaskFile(request);
        }

        /// <summary>
        /// 接收插拔桩文件
        /// </summary>
        [HttpPost]
        public WindCloudBaseReturn ReceiveTaskFile(TechCloudWindApiRequest request)
        {
            var ret = new WindCloudBaseReturn();

            if (!Authored(request.PostToken))
            {
                ret.Success = false;
                ret.Message = "接口失效";
                return ret;
            }

            if (request.TaskID == 0)
            {
                ret.Message = "TaskID不能为空";
                ret.Success = false;
                return ret;
            }

            if (request.TaskFiles == null || request.TaskFiles.Count == 0)
            {
                ret.Message = "没有报告文件";
                ret.Success = false;
                return ret;
            }

            return _windApiService.ReceiveTaskFile(request);
        }

        /// <summary>
        /// 接收可作业预报文件接口
        /// </summary>
        [HttpPost]
        public WindCloudBaseReturn ReceiveShipPreReport(TechCloudWindApiRequest request)
        {
            var ret = new WindCloudBaseReturn();

            if (!InterAuthored(request.PostToken, request.Platform))
            {
                ret.Success = false;
                ret.Message = "接口失效";
                return ret;
            }

            if (request.TaskFiles == null || request.TaskFiles.Count == 0)
            {
                ret.Message = "没有报告文件";
                ret.Success = false;
                return ret;
            }

            return _windApiService.ReceiveShipPreReport(request);
        }

        /// <summary>
        /// 发送可作业预报计算文件
        /// </summary>
        [HttpPost]
        public WindCloudBaseReturn SendShipPreFile(TechCloudWindApiRequest request)
        {
            var ret = new WindCloudBaseReturn();

            if (!InterAuthored(request.PostToken, request.Platform))
            {
                ret.Success = false;
                ret.Message = "接口失效";
                return ret;
            }

            return _windApiService.SendShipPreFile(request);
        }

        /// <summary>
        /// 接收拖航报告文件
        /// </summary>
        [HttpPost]
        public WindCloudBaseReturn ReceiveTowingReport(TechCloudWindApiRequest request)
        {
            var ret = new WindCloudBaseReturn();

            if (!InterAuthored(request.PostToken, request.Platform))
            {
                ret.Success = false;
                ret.Message = "接口失效";
                return ret;
            }

            if (request.TaskFiles == null || request.TaskFiles.Count == 0)
            {
                ret.Message = "没有报告文件";
                ret.Success = false;
                return ret;
            }

            return _windApiService.ReceiveTowingReport(request);
        }

        /// <summary>
        /// 接收云服务文件
        /// </summary>
        [HttpPost]
        public WindCloudBaseReturn ReceiveCloudFile(TechCloudWindApiRequest request)
        {
            var ret = new WindCloudBaseReturn();

            if (!InterAuthored(request.PostToken, request.Platform))
            {
                ret.Success = false;
                ret.Message = "接口失效";
                return ret;
            }

            if (request.TaskFiles == null || request.TaskFiles.Count == 0)
            {
                ret.Message = "没有报告文件";
                ret.Success = false;
                return ret;
            }

            return _windApiService.ReceiveCloudFile(request);
        }

        /// <summary>
        /// 获取拖航数据
        /// </summary>
        [HttpPost]
        public WindCloudBaseReturn GetTowingData(TechCloudWindApiRequest request)
        {
            var ret = new WindCloudBaseReturn();

            if (!InterAuthored(request.PostToken, request.Platform))
            {
                ret.Success = false;
                ret.Message = "接口失效";
                return ret;
            }

            return _windApiService.GetTowingData(request);
        }

    }
}
