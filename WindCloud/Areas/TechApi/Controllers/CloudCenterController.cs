using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model.Base;
using Model.Tech.IOT;
using Model.Tech.WebApi;
using Model.TechApi;
using Model.TechApi.KuLun;
using Service.Api;
using System.Text.Json;

namespace WindCloud.Areas.TechApi.Controllers
{
    [Route("CCSHJTechApi/[controller]/[action]")]
    [ApiController]
    public class CloudCenterController : BaseApiController
    {
        private readonly WindApiService _windApiService;
        private readonly SHXAService _shxaService;

        public CloudCenterController(WindApiService windApiService, SHXAService shxaService)
        {
            _windApiService = windApiService;
            _shxaService = shxaService;
        }

        [HttpGet]
        public string ApiTest()
        {
            return "success";
        }

        [HttpPost]
        public string ApiTest2()
        {
            return "success2";
        }


        [HttpPost]
        public BaseReturn SSOLoginChecking(TechCloudApiRequest request)
        {
            return _windApiService.SSOLoginChecking(request);
        }

        #region 拖航监控系统数据接收

        [HttpPost]
        public async Task<int> TowingMonitorApi(CloudWindMonitorRequest request)
        {
            return await _windApiService.TowingMonitorApi(request);
        }

        [HttpPost]
        public async Task<int> TowingMonitorMRUApi(CloudWindMonitorRequest request)
        {
            return await _windApiService.TowingMonitorMRUApi(request);
        }

        [HttpPost]
        public string HikPicUpload(CloudWindMonitorRequest request)
        {
            return _windApiService.HikPicUpload(request);
        }

        [HttpPost]
        public BaseReturn TowingMonitorGetData()
        {
            return _windApiService.TowingMonitorGetData();
        }

        [HttpPost]
        public string TowingMonitorSetData(CloudWindMonitorRequest request)
        {
            return _windApiService.TowingMonitorSetData(request);
        }

        #endregion


        #region 物联网

        /// <summary>
        /// 三航翔安
        /// </summary>
        [HttpPost]
        public string GetData_SHXA(BaseApiRequest<List<SHXARequest>> request)
        {
            return _shxaService.GetData_SHXA(request);
        }

        /// <summary>
        /// Sql参数查询
        /// </summary>
        [HttpPost]
        public Model.TechApi.BaseApiReturn<List<SHXATable>> QueryData_SHXA(SqlParamRequest request)
        {
            return _shxaService.QueryData_SHXA(request);
        }

        /// <summary>
        /// 增删改
        /// </summary>
        [HttpPost]
        public Model.TechApi.BaseApiReturn<int> OperateData_SHXA(SqlParamRequest request)
        {
            return _shxaService.OperateData_SHXA(request);
        }

        /// <summary>
        /// 接收图片
        /// </summary>
        [HttpPost]
        public string GetPic_SHXA(BaseApiRequest<List<SHXAFileRequest>> request)
        {
            return _shxaService.GetPic_SHXA(request);
        }



        #endregion


        #region 岩土所-库仑监测平台接口

        [HttpPost]
        public async Task<BaseReturn> queryProjects(WindCloudApiKuLunRequest request)
        {
            BaseReturn ret = new BaseReturn();

            if (string.IsNullOrEmpty(request.Token))
            {
                ret.Message = "token是空值";
                return ret;
            }

            string validateTokenRet = KuLunService.ValidateToken(request.Token);

            if (!string.IsNullOrEmpty(validateTokenRet))
            {
                ret.Message = validateTokenRet;
                return ret;
            }

            string data = await KuLunService.queryProjectsOrig();

            ret.Data = data;
            ret.Success = true;

            return ret;
        }

        [HttpPost]
        public async Task<BaseReturn> queryMonitorPoints(WindCloudApiKuLunRequest request)
        {
            BaseReturn ret = new BaseReturn();

            if (string.IsNullOrEmpty(request.Token))
            {
                ret.Message = "token是空值";
                return ret;
            }

            string validateTokenRet = KuLunService.ValidateToken(request.Token);

            if (!string.IsNullOrEmpty(validateTokenRet))
            {
                ret.Message = validateTokenRet;
                return ret;
            }

            if (request.DataRequest.pid == 0)
            {
                ret.Message = "没有项目编号pid";
                return ret;
            }

            string data = await KuLunService.queryMonitorPointsOrig(request.DataRequest.pid.Value);

            ret.Data = data;
            ret.Success = true;

            return ret;
        }

        [HttpPost]
        public async Task<BaseReturn> batchImportMonitorRecords(WindCloudApiKuLunRequest request)
        {
            BaseReturn ret = new BaseReturn();

            if (string.IsNullOrEmpty(request.Token))
            {
                ret.Message = "token是空值";
                return ret;
            }

            string validateTokenRet = KuLunService.ValidateToken(request.Token);

            if (!string.IsNullOrEmpty(validateTokenRet))
            {
                ret.Message = validateTokenRet;
                return ret;
            }

            if (request.DataRequest.pid == 0 || request.DataRequest.records == null || request.DataRequest.records.Count == 0)
            {
                ret.Message = "没有项目编号pid或没有records数据";
                return ret;
            }

            string data = await KuLunService.batchImportMonitorRecordsOrig(request.DataRequest);

            ret.Data = data;
            ret.Success = true;

            return ret;
        }

        #endregion
    }

}
