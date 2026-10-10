using DataFactory.KingBase.CloudWind;
using Microsoft.AspNetCore.Mvc;
using Model.Base;
using Model.Tech.Cloud;
using Service.Base;
using Service.Wind;
using Tool;

namespace WindCloud.Areas.Cloud.Controllers
{
    public class ProjectTaskBaseController : Controller
    {
        public CookieService _cookieService { get; }
        public ProjectService _projectService;
        protected TaskService _taskService;

        public ProjectTaskBaseController(CookieService cookieService, ProjectService projectService, TaskService taskService)
        {
            _cookieService = cookieService;
            _projectService = projectService;
            _taskService = taskService;
        }

        public SHJUserInfo? CurrentUser
        {
            get
            {
                try
                {
                    return _cookieService.GetUserCookie();

                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        private string EnKey = AppSettingUtils.GetSetting("AppSettings:TripleEncrypt:Key").ToString();
        private string EnIV = AppSettingUtils.GetSetting("AppSettings:TripleEncrypt:IV").ToString();

        protected string CreateWebToken(string UserCode)
        {
            string token = CryptographyUtils.TripleDESEncrypt(UserCode, EnKey, EnIV);
            return token;
        }
        protected bool ValidateWebToken(string token)
        {
            bool Success = false;
            try
            {
                if (!string.IsNullOrEmpty(token))
                {
                    token = token.Substring(0, 24);

                    string UserCode = CryptographyUtils.TripleDESDecrypt(token, EnKey, EnIV);

                    if (UserCode.Equals(UserCode))
                    {
                        Success = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Success = false;
                LoggerUtils.Error(ex.ToString(), typeof(ProjectTaskBaseController));
            }
            return Success;
        }
        public async Task<IActionResult> FindShjUser(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(request.Name))
            {
                return Ok(ret);
            }
            try
            {
                string msg = "";
                var list = _projectService.FindShjUser(request.Name, out msg);

                if (string.IsNullOrEmpty(msg))
                {
                    ret.Data = list;
                    ret.Success = true;
                }
                else
                {
                    ret.Data = list;
                    ret.Message = msg;
                    ret.Success = false;
                }

            }
            catch (Exception ex)
            {
                ret.Success = false;
                //记录日志
                LoggerUtils.Error(ex.ToString(), typeof(ProjectTaskBaseController));
            }
            return Ok(ret);
        }

        public IActionResult ProjectPositionDownload(CloudWindRequest request)
        {
            if (request.ProjectID == 0)
            {
                return Content("");
            }

            try
            {
                var (bytes, fileName, msg) = _projectService.ProjectPositionDownload(request.ProjectID);

                if (!string.IsNullOrEmpty(msg) || bytes == null)
                {
                    return Content("");
                }

                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(ProjectTaskBaseController));
                return Content("");
            }
        }

        public IActionResult ProjectFanPositionDownload(CloudWindRequest request)
        {
            if (request.ProjectID == 0)
            {
                return Content("");
            }

            try
            {
                var (bytes, fileName, msg) = _projectService.ProjectFanPositionDownload(request.ProjectID);

                if (!string.IsNullOrEmpty(msg) || bytes == null)
                {
                    return Content("");
                }

                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(ProjectTaskBaseController));
                return Content("");
            }
        }


        /// <summary>
        /// 加载可作业性船舶列表
        /// </summary>
        [HttpPost]
        public IActionResult LoadKZYShip(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            try
            {
                var shipList = _taskService.GetKZYShipList();
                ret.Data = shipList;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                LoggerUtils.Error(ex.ToString(), typeof(ProjectTaskBaseController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 加载拖航航线位置
        /// </summary>
        [HttpPost]
        public IActionResult LoadTowingPosition(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            try
            {
                var positionList = _taskService.GetPlanRouteList();
                ret.Data = positionList;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                LoggerUtils.Error(ex.ToString(), typeof(ProjectTaskBaseController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 获取文件保存路径（物理路径）
        /// </summary>
        protected string GetFileSavePath(int taskId, string flowHandle)
        {
            return _taskService.GetFileSavePath(taskId, flowHandle);
        }

        /// <summary>
        /// 获取文件下载路径（URL路径）
        /// </summary>
        protected string GetFileDownloadPath(int taskId, string flowHandle)
        {
            return _taskService.GetFileDownloadPath(taskId, flowHandle);
        }

        /// <summary>
        /// 获取项目的地勘资料
        /// </summary>
        [HttpPost]
        public IActionResult LoadDKZL(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            if (request.ProjectID == 0)
            {
                return Ok(ret);
            }
            if (request.TaskID == 0)
            {
                ret.Message = "任务数据错误";
                return Ok(ret);
            }
            try
            {
                CloudWindTaskDK data = _taskService.LoadDKZL(request);
                ret.Data = data;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(ProjectTaskBaseController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 船舶库
        /// </summary>
        [HttpPost]
        public IActionResult LoadShip(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            if (request.TaskID == 0)
            {
                ret.Message = "任务数据错误";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(request.FlowHandle))
            {
                ret.Message = "任务数据错误";
                return Ok(ret);
            }
            try
            {
                CloudWindTaskShip data = _taskService.LoadShip(request);
                ret.Data = data;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(ProjectTaskBaseController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 查询船舶信息
        /// </summary>
        [HttpPost]
        public IActionResult OpenShip(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            if (request.ShipID == 0)
            {
                ret.Message = "船舶数据错误";
                return Ok(ret);
            }
            try
            {
                CloudWindLibraryShip data = _taskService.OpenShip(request);
                ret.Data = data;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(ProjectTaskBaseController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 更新平台船库
        /// </summary>
        [HttpPost]
        public IActionResult SaveLibraryShip(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            if (request.ShipID == 0)
            {
                ret.Message = "船舶数据错误";
                return Ok(ret);
            }
            try
            {
                if (string.IsNullOrEmpty(request.ShipZTYYL) || string.IsNullOrEmpty(request.ShipJSYYHZ))
                {
                    ret.Message = "船舶参数数据错误";
                    return Ok(ret);
                }

                _taskService.SaveLibraryShip(request);
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(ProjectTaskBaseController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 上传船舶库
        /// </summary>
        [HttpPost]
        public IActionResult CreateLibraryShip(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (string.IsNullOrEmpty(request.ShipName))
            {
                ret.Message = "船名不能是空的";
                return Ok(ret);
            }
            if (request.TaskID == 0)
            {
                ret.Message = "任务数据错误";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(request.FlowHandle))
            {
                ret.Message = "任务数据错误";
                return Ok(ret);
            }
            try
            {
                var file = Request.Form.Files["LibraryShipFile"];
                if (file == null)
                {
                    ret.Message = "没有选择文件";
                    return Ok(ret);
                }

                List<Library_Ship> ships = _taskService.CreateLibraryShip(request, file);
                ret.Data = ships;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Message = ex.Message;
                ret.Success = false;
                LoggerUtils.Error(ex.ToString(), typeof(ProjectTaskBaseController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 上传可作业船舶
        /// </summary>
        [HttpPost]
        public IActionResult CreateLibraryKZYShip(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (string.IsNullOrEmpty(request.ShipName))
            {
                ret.Message = "船名不能是空的";
                return Ok(ret);
            }
            try
            {
                var file = Request.Form.Files["LibraryShipFile"];
                if (file == null)
                {
                    ret.Message = "没有选择文件";
                    return Ok(ret);
                }

                List<Library_Ship_KZY> ships = _taskService.CreateLibraryKZYShip(request, file);
                ret.Data = ships;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Message = "发生错误";
                ret.Success = false;
                LoggerUtils.Error(ex.ToString(), typeof(ProjectTaskBaseController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 生成平台船文件excel
        /// </summary>
        [HttpPost]
        public IActionResult CreateShipFile(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            if (request.TaskID == 0)
            {
                ret.Message = "任务数据错误";
                return Ok(ret);
            }
            if (request.ShipIDs == null || request.ShipIDs.Count == 0)
            {
                ret.Message = "没有选择平台船";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(request.FlowHandle))
            {
                ret.Message = "任务数据错误";
                return Ok(ret);
            }
            try
            {
                List<CloudProjectFile> list = _taskService.CreateShipFile(request);
                ret.Data = list;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                LoggerUtils.Error(ex.ToString(), typeof(ProjectTaskBaseController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 查询下一流程节点
        /// </summary>
        [HttpPost]
        public IActionResult NextNodeQuery(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            if (request.SoftwareID == 0)
            {
                ret.Message = "信息错误";
                return Ok(ret);
            }
            try
            {
                CloudWindNextNode node = _taskService.NextNodeQuery(request);
                ret.Data = node;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                LoggerUtils.Error(ex.ToString(), typeof(ProjectTaskBaseController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 上传任务评论文件
        /// </summary>
        [HttpPost]
        public IActionResult TaskCommentFileUpload(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (request.TaskID == 0)
            {
                ret.Message = "项目数据错误";
                return Ok(ret);
            }
            try
            {
                var fileCountStr = Request.Form["TaskCommentFileCount"];
                if (string.IsNullOrEmpty(fileCountStr))
                {
                    return Ok(ret);
                }

                List<IFormFile> files = new List<IFormFile>();
                for (int i = 1; i <= Convert.ToInt32(fileCountStr); i++)
                {
                    var file = Request.Form.Files["TaskCommentFile" + i];
                    if (file != null) files.Add(file);
                }

                List<CloudProjectFile> list = _taskService.UploadCommentFile(request, files);
                ret.Data = list;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                LoggerUtils.Error(ex.ToString(), typeof(ProjectTaskBaseController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 上传任务修改报告
        /// </summary>
        [HttpPost]
        public IActionResult TaskModifyReportUpload(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (request.TaskID == 0)
            {
                ret.Message = "项目数据错误";
                return Ok(ret);
            }
            try
            {
                var fileCountStr = Request.Form["TaskModifyReportCount"];
                if (string.IsNullOrEmpty(fileCountStr))
                {
                    return Ok(ret);
                }

                List<IFormFile> files = new List<IFormFile>();
                for (int i = 1; i <= Convert.ToInt32(fileCountStr); i++)
                {
                    var file = Request.Form.Files["TaskModifyReportFile" + i];
                    if (file != null) files.Add(file);
                }

                List<CloudProjectFile> list = _taskService.UploadModifyReport(request, files);
                ret.Data = list;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                LoggerUtils.Error(ex.ToString(), typeof(ProjectTaskBaseController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 加载任务文件
        /// </summary>
        [HttpPost]
        public IActionResult LoadTaskFile(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            if (request.TaskID == 0)
            {
                ret.Message = "数据错误";
                return Ok(ret);
            }
            try
            {
                List<CloudProjectFile> list = _taskService.LoadTaskFile(request);
                ret.Data = list;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                LoggerUtils.Error(ex.ToString(), typeof(ProjectTaskBaseController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 删除修改报告文件
        /// </summary>
        [HttpPost]
        public IActionResult DeleteModifyFile(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            if (request.ID == 0)
            {
                ret.Message = "数据错误";
                return Ok(ret);
            }
            try
            {
                List<CloudProjectFile> list = _taskService.DeleteModifyFile(request);
                ret.Data = list;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(ProjectTaskBaseController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 加载地勘文件
        /// </summary>
        [HttpPost]
        public IActionResult LoadDKFile(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            if (request.ProjectID == 0 && request.TaskID == 0)
            {
                ret.Message = "数据错误";
                return Ok(ret);
            }
            try
            {
                CloudWindDKFile file = _taskService.LoadDKFile(request);
                ret.Data = file;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(ProjectTaskBaseController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 删除地勘文件
        /// </summary>
        [HttpPost]
        public IActionResult DeleteDKFile(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            if (request.ID == 0)
            {
                ret.Message = "数据错误";
                return Ok(ret);
            }
            try
            {
                List<CloudProjectFile> files = _taskService.DeleteDKFile(request);
                ret.Data = files;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(ProjectTaskBaseController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 获取任务报告文件
        /// </summary>
        [HttpPost]
        public IActionResult GetTaskReport(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            if (request.TaskID == 0)
            {
                ret.Message = "数据错误";
                return Ok(ret);
            }
            try
            {
                CloudWindTaskFile file = _taskService.GetTaskReport(request);
                ret.Data = file;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(ProjectTaskBaseController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 上传报告文件
        /// </summary>
        [HttpPost]
        public IActionResult UploadReportFile(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (request.TaskID == 0)
            {
                ret.Message = "项目数据错误";
                return Ok(ret);
            }
            if (request.FileType == 0)
            {
                ret.Message = "项目数据错误";
                return Ok(ret);
            }
            try
            {
                var fileCountStr = Request.Form["TaskFileReportCount"];
                if (string.IsNullOrEmpty(fileCountStr))
                {
                    return Ok(ret);
                }

                List<IFormFile> files = new List<IFormFile>();
                for (int i = 1; i <= Convert.ToInt32(fileCountStr); i++)
                {
                    var file = Request.Form.Files["TaskFileReportFile" + i];
                    if (file != null) files.Add(file);
                }

                List<CloudProjectFile> list = _taskService.UploadFileReport(request, files);
                ret.Data = list;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                LoggerUtils.Error(ex.ToString(), typeof(ProjectTaskBaseController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 删除报告文件
        /// </summary>
        [HttpPost]
        public IActionResult DeleteReportFile(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            if (request.ID == 0)
            {
                ret.Message = "数据错误";
                return Ok(ret);
            }
            try
            {
                List<CloudProjectFile> list = _taskService.DeleteReportFile(request);
                ret.Data = list;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(ProjectTaskBaseController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 加载项目位置
        /// </summary>
        [HttpPost]
        public IActionResult LoadProjectPosition(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            if (request.ProjectID == 0)
            {
                ret.Message = "数据错误";
                return Ok(ret);
            }
            try
            {
                string position = _taskService.LoadProjectPosition(request);
                ret.Data = position;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(ProjectTaskBaseController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 拖航路线坐标下载
        /// </summary>
        [HttpPost]
        public IActionResult TowingPlanRouteDownload(CloudWindRequest request)
        {
            if (request.ProjectID == 0)
            {
                return Ok(new BaseReturn { Success = false, Message = "项目数据错误" });
            }
            try
            {
                var (bytes, fileName) = _taskService.TowingPlanRouteDownload(request);
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(ProjectTaskBaseController));
                return Ok(new BaseReturn { Success = false, Message = "发生错误，请检查路径是否存在。" });
            }
        }

        /// <summary>
        /// 上传选船分析报告
        /// </summary>
        [HttpPost]
        public IActionResult TaskXCFXReportUpload(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (request.TaskID == 0)
            {
                ret.Message = "项目数据错误";
                return Ok(ret);
            }
            try
            {
                var fileCountStr = Request.Form["TaskUploadReportCount"];
                if (string.IsNullOrEmpty(fileCountStr))
                {
                    return Ok(ret);
                }

                List<IFormFile> files = new List<IFormFile>();
                for (int i = 1; i <= Convert.ToInt32(fileCountStr); i++)
                {
                    var file = Request.Form.Files["TaskUploadReportFile" + i];
                    if (file != null) files.Add(file);
                }

                List<CloudProjectFile> list = _taskService.UploadReport_XCFX(request, files);
                ret.Data = list;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(ProjectTaskBaseController));
            }
            return Ok(ret);
        }

        /// <summary>
        /// 删除选船分析报告文件
        /// </summary>
        [HttpPost]
        public IActionResult DeleteXCFXFile(CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            if (request.ID == 0)
            {
                ret.Message = "数据错误";
                return Ok(ret);
            }
            try
            {
                List<CloudProjectFile> list = _taskService.DeleteXCFXFile(request);
                ret.Data = list;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = ex.Message;
                LoggerUtils.Error(ex.ToString(), typeof(ProjectTaskBaseController));
            }
            return Ok(ret);
        }

    }
}
