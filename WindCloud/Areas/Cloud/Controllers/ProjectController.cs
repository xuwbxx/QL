using Microsoft.AspNetCore.Mvc;
using Model.Base;
using Model.Tech.Cloud;
using Service.Base;
using Service.Base.Filter;
using Service.Shj;
using Service.Wind;
using System.Reflection;
using System.Web;
using Tool;
using WindCloud.Areas.Base.Controllers;

namespace WindCloud.Areas.Cloud.Controllers
{
    [Area("Cloud")]
    public class ProjectController : ProjectTaskBaseController
    {
        public LoginService _loginService;
        public CloudWindInfoService _cloudWindInfoService;
        public ProjectController(CookieService cookieService, LoginService loginService, ProjectService projectService, CloudWindInfoService cloudWindInfoService)
            : base(cookieService, projectService)
        {
            _loginService = loginService;
            _cloudWindInfoService = cloudWindInfoService;
        }


        public IActionResult Main()
        {
            //处理手机端和电脑端判断

            string paramString = HttpContext.Request.Query["paramString"];

            LoggerUtils.Info(paramString, typeof(ProjectController));

            if (string.IsNullOrEmpty(paramString))
            {
                return RedirectToAction("ErrorPage", "CloudBase", new { ErrorText = "身份信息错误" });
            }

            ViewData["Token"] = paramString;

            return View();
        }

        public IActionResult Redirect()
        {
            try
            {
                string paramString = HttpContext.Request.Query["paramString"];
                string device = HttpContext.Request.Query["device"];

                if (string.IsNullOrEmpty(device))
                {
                    return RedirectToAction("ErrorPage", "CloudBase", new { ErrorText = "发生错误，请联系管理员。" });
                }

                //var UserID = HttpContext.Request.QueryString["UserID"];
                SHJUserInfo userinfo = new SHJUserInfo();
                if (!string.IsNullOrEmpty(paramString))
                {

                    LoggerUtils.Info(paramString, typeof(ProjectController));
                    string token = HttpUtility.UrlDecode(paramString);

                    //解密token
                    SingleSinOnModel ssoModel = new SingleSinOnModel();
                    string ssoResult = _cloudWindInfoService.AESSingleSignOn(token, out ssoModel);
                    if (!string.IsNullOrEmpty(ssoResult))
                    {
                        //身份失效界面
                        return RedirectToAction("ErrorPage", "CloudBase", new { ErrorText = "身份信息错误" });
                    }
                    //获取用户信息
                    var ShjUser = _cloudWindInfoService.GetShjUserInfo(ssoModel.usrCode);
                    if (ShjUser.StatusCode != 200 || ShjUser.Data == null || ShjUser.Data.Count != 1)
                    {
                        //用户信息问题
                        return RedirectToAction("ErrorPage", "CloudBase", new { ErrorText = "身份信息错误" });
                    }

                    userinfo.UserID = ShjUser.Data[0].ID;
                    userinfo.UserName = ShjUser.Data[0].namespell;
                    userinfo.UserCode = ShjUser.Data[0].empcode;
                    userinfo.RealName = ShjUser.Data[0].name;
                    userinfo.DepartName = ShjUser.Data[0].wholeDeptPath;
                    userinfo.Email = ShjUser.Data[0].email;
                    userinfo.Mobile = ShjUser.Data[0].phone;
                    userinfo.Birthday = ShjUser.Data[0].birthday;
                    userinfo.JobName = ShjUser.Data[0].jobname;
                }

                else
                {
                    return RedirectToAction("ErrorPage", "CloudBase", new { ErrorText = "身份错误，请联系管理员。" });
                }

                //登录信息保存cookie
                _cookieService.SetUserCookie(userinfo);

                //保存登录信息
                _loginService.JJTLoginRecord(userinfo);

                if (device.Equals("tablet"))
                {
                    //pad
                    return Redirect("/WindDoor/Main/Pad");
                }
                else if (device.Equals("desktop"))
                {
                    //pc
                    return Redirect("/WindDoor/Main/Index");
                }
                else
                {
                    return RedirectToAction("ErrorPage", "CloudBase", new { ErrorText = "发生错误，请联系管理员。" });
                }

            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(ProjectController));
                return RedirectToAction("ErrorPage", "CloudBase", new { ErrorText = "身份错误，请联系管理员。" });
            }

        }


        [TypeFilter(typeof(WindCloudFilter))]
        public IActionResult Index()
        {

            var CloudID = HttpContext.Request.Query["CloudID"].ToString();
            //判断权限
            if (!string.IsNullOrEmpty(CloudID))
            {
                int ProjectID = Convert.ToInt32(CloudID);
                var Project = _projectService.getProjectByID(ProjectID);
                if (Project == null)
                {
                    return RedirectToAction("ErrorPage", "CloudBase", new { ErrorText = "项目数据错误" });
                }
                //管理员、抄送人、
                var IsHasRight = _projectService.hasProjectViewRight(ProjectID, CurrentUser.UserCode);
                if (!IsHasRight)
                {
                    return RedirectToAction("ErrorPage", "CloudBase", new { ErrorText = "没有权限浏览" });
                }
            }

            ViewData["CloudID"] = CloudID ?? "";

            var Companys = _projectService.getCompanys();
            ViewData["Companys"] = Companys;

            ViewData["PostToken"] = CreateWebToken(CurrentUser.UserCode);

            return View();
        }

        public async Task<IActionResult> ProjectListQuery([FromForm] CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            try
            {
                var (list, msg) = await _projectService.GetProjectList(request);

                if (!string.IsNullOrEmpty(msg))
                {
                    ret.Success = false;
                    ret.Message = msg;
                    return Ok(ret);
                }

                //分页(假的。。。)
                int Count = list.Count();
                ret.TotalCount = Count;
                ret.TotalPage = Count % request.PageSize == 0 ? (Count / request.PageSize) : (Count / request.PageSize) + 1;

                var CurrentQuery = new List<CloudWindProject>();
                if (!string.IsNullOrEmpty(request.Order) && !string.IsNullOrEmpty(request.OrderName))
                {
                    if (request.Order.Equals("asc"))
                    {
                        if (request.OrderName.Equals("ApplyTime"))
                        {
                            CurrentQuery = list.OrderBy(a => a.ApplyTime).ToList();
                        }
                        else if (request.OrderName.Equals("ProjectCode"))
                        {
                            CurrentQuery = list.OrderBy(a => a.ProjectCode).ToList();
                        }
                    }
                    else
                    {
                        if (request.OrderName.Equals("ApplyTime"))
                        {
                            CurrentQuery = list.OrderByDescending(a => a.ApplyTime).ToList();
                        }
                        else if (request.OrderName.Equals("ProjectCode"))
                        {
                            CurrentQuery = list.OrderByDescending(a => a.ProjectCode).ToList();
                        }
                    }
                    CurrentQuery = CurrentQuery.Skip(request.GetSkipCount()).Take(request.PageSize).ToList();
                }
                else
                {
                    CurrentQuery = list.OrderByDescending(a => a.ID).Skip(request.GetSkipCount()).Take(request.PageSize).ToList();
                }

                ret.Data = CurrentQuery;
                ret.Success = true;
                ret.PageIndex = request.PageIndex;

            }
            catch (Exception ex)
            {
                ret.Success = false;
                //记录日志
                LoggerUtils.Error(ex.ToString(), typeof(ProjectController));
            }
            return Ok(ret);
        }

        public async Task<IActionResult> ProjectQuery([FromForm] CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            if (request.ProjectID == 0)
            {
                ret.Message = "项目数据错误";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(CurrentUser.UserCode))
            {
                ret.Message = "用户信息失效，请重新交建通登录";
                return Ok(ret);
            }
            try
            {
                CloudWindProjectFlow data = new CloudWindProjectFlow();

                string msg = "";
                (data, msg) = await _projectService.GetProject(request);

                if (string.IsNullOrEmpty(msg))
                {
                    ret.Data = data;
                    ret.Success = true;
                }
                else
                {
                    ret.Data = data;
                    ret.Message = msg;
                    ret.Success = false;
                }


            }
            catch (Exception ex)
            {
                ret.Success = false;
                //记录日志
                LoggerUtils.Error(ex.ToString(), typeof(ProjectController));
            }
            return Ok(ret);
        }

        public async Task<IActionResult> ProjectEditQuery([FromForm] CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            if (request.ProjectID == 0)
            {
                ret.Message = "项目数据错误";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(CurrentUser.UserCode))
            {
                ret.Message = "用户信息失效，请重新交建通登录";
                return Ok(ret);
            }
            try
            {
                CloudWindProjectFlow data = new CloudWindProjectFlow();

                string msg = "";
                (data, msg) = await _projectService.GetProjectEdit(request);

                if (string.IsNullOrEmpty(msg))
                {
                    ret.Data = data;
                    ret.Success = true;
                }
                else
                {
                    ret.Data = data;
                    ret.Message = msg;
                    ret.Success = false;
                }

            }
            catch (Exception ex)
            {
                ret.Success = false;
                //记录日志
                LoggerUtils.Error(ex.ToString(), typeof(ProjectController));
            }
            return Ok(ret);
        }


        public async Task<IActionResult> ProjectPositionUpload([FromForm] CloudWindRequest request, IFormFile ProjectPositionImport)
        {
            BaseReturn ret = new BaseReturn();
            if (request.ProjectID == 0)
            {
                ret.Message = "项目数据错误";
                return Ok(ret);
            }
            try
            {
                //excel文件
                if (ProjectPositionImport == null)
                {
                    ret.Message = "没有选择任何文件";
                    return Ok(ret);
                }

                string msg = "";
                var (list, errorMsg) = await _projectService.ProjectPositionUpload(request, ProjectPositionImport);
                if (string.IsNullOrEmpty(errorMsg))
                {
                    ret.Data = list;
                    ret.Success = true;
                }
                else
                {
                    ret.Data = list;
                    ret.Message = errorMsg;
                    ret.Success = false;
                }

            }
            catch (Exception ex)
            {
                ret.Success = false;
                //记录日志
                LoggerUtils.Error(ex.ToString(), typeof(ProjectController));
            }
            return Ok(ret);
        }

        public async Task<IActionResult> ProjectFanPositionUpload([FromForm] CloudWindRequest request, IFormFile ProjectFanPositionImport)
        {
            BaseReturn ret = new BaseReturn();
            if (request.ProjectID == 0)
            {
                ret.Message = "项目数据错误";
                return Ok(ret);
            }
            try
            {
                //excel文件
                if (ProjectFanPositionImport == null)
                {
                    ret.Message = "没有选择任何文件";
                    return Ok(ret);
                }

                string msg = "";
                var (list, errorMsg) = await _projectService.ProjectFanPositionUpload(request, ProjectFanPositionImport);
                if (string.IsNullOrEmpty(errorMsg))
                {
                    ret.Data = list;
                    ret.Success = true;
                }
                else
                {
                    ret.Data = list;
                    ret.Message = errorMsg;
                    ret.Success = false;
                }

            }
            catch (Exception ex)
            {
                ret.Success = false;
                //记录日志
                LoggerUtils.Error(ex.ToString(), typeof(ProjectController));
            }
            return Ok(ret);
        }

        public async Task<IActionResult> ProjectFileInfoUpload([FromForm] CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (request.ProjectID == 0)
            {
                ret.Message = "项目数据错误";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(request.FlowHandle))
            {
                ret.Message = "项目数据错误";
                return Ok(ret);
            }
            try
            {
                //获取文件数量
                var FileCount = Request.Form["ProjectFileCount"];
                if (string.IsNullOrEmpty(FileCount))
                {
                    ret.Message = "没有选择任何文件";
                    return Ok(ret);
                }

                List<IFormFile> Files = new List<IFormFile>();
                for (int i = 1; i <= Convert.ToInt32(FileCount); i++)
                {
                    var File = Request.Form.Files[$"ProjectFile{i}"];
                    if (File != null)
                    {
                        Files.Add(File);
                    }
                }

                string msg = "";
                var (list, errorMsg) = await _projectService.ProjectFileInfoUpload(request, Files);
                if (string.IsNullOrEmpty(errorMsg))
                {
                    ret.Data = list;
                    ret.Success = true;
                }
                else
                {
                    ret.Data = list;
                    ret.Message = errorMsg;
                    ret.Success = false;
                }

            }
            catch (Exception ex)
            {
                ret.Success = false;
                //记录日志
                LoggerUtils.Error(ex.ToString(), typeof(ProjectController));
            }
            return Ok(ret);
        }

        public async Task<IActionResult> ProjectDKFileInfoUpload([FromForm] CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (request.ProjectID == 0)
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
                //获取文件数量
                var FileCount = Request.Form["ProjectFileCount"];
                if (string.IsNullOrEmpty(FileCount))
                {
                    ret.Message = "没有选择任何文件";
                    return Ok(ret);
                }

                List<IFormFile> Files = new List<IFormFile>();
                for (int i = 1; i <= Convert.ToInt32(FileCount); i++)
                {
                    var File = Request.Form.Files[$"ProjectFile{i}"];
                    if (File != null)
                    {
                        Files.Add(File);
                    }
                }

                string msg = "";
                var (list, errorMsg) = await _projectService.ProjectDKFileInfoUpload(request, Files);
                if (string.IsNullOrEmpty(errorMsg))
                {
                    ret.Data = list;
                    ret.Success = true;
                }
                else
                {
                    ret.Data = list;
                    ret.Message = errorMsg;
                    ret.Success = false;
                }

            }
            catch (Exception ex)
            {
                ret.Success = false;
                //记录日志
                LoggerUtils.Error(ex.ToString(), typeof(ProjectController));
            }
            return Ok(ret);
        }


        public async Task<IActionResult> ProjectSubmit([FromForm] CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            //if (!ValidateWebToken(request.PostToken))
            //{
            //    ret.Message = "身份信息失效，请重新登录。";
            //    return Json(ret, JsonRequestBehavior.DenyGet);
            //}
            if (request.ProjectID == 0)
            {
                ret.Message = "风场数据错误";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(request.ProjectName))
            {
                ret.Message = "风场名不能为空";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(request.ProjectManagerName) || string.IsNullOrEmpty(request.ProjectManagerUserCode))
            {
                ret.Message = "项目负责人信息错误";
                return Ok(ret);
            }

            if (string.IsNullOrEmpty(request.ProjectStartTime) || string.IsNullOrEmpty(request.ProjectEndTime))
            {
                ret.Message = "需要输入云平台使用期限";
                return Ok(ret);
            }
            if (CurrentUser == null || string.IsNullOrEmpty(CurrentUser.UserCode))
            {
                ret.Message = "用户信息失效，请从交建通重新登录云平台";
                return Ok(ret);
            }
            if (request.CompanyID == 0 || request.Status == 0 || string.IsNullOrEmpty(request.WaterDepthMax) || string.IsNullOrEmpty(request.WaterDepthMin))
            {
                ret.Message = "要输入项目基本信息";
                return Ok(ret);
            }

            try
            {
                string msg = "";
                msg = await _projectService.ProjectSubmit(request);
                if (string.IsNullOrEmpty(msg))
                {
                    ret.Success = true;
                }
                else
                {
                    ret.Message = msg;
                    ret.Success = false;
                }


            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = "注意，文件名不能重名。";
                //记录日志
                LoggerUtils.Error(ex.ToString(), typeof(ProjectController));
            }
            return Ok(ret);

        }

        public async Task<IActionResult> ProjectApprovalSubmit([FromForm] CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (request.ProjectID == 0)
            {
                ret.Message = "项目信息出错";
                return Ok(ret);
            }
            if (request.ApprovalType != 0 && request.ApprovalType != 2)
            {
                ret.Message = "流程错误";
                return Ok(ret);
            }
            if (CurrentUser == null || string.IsNullOrEmpty(CurrentUser.UserCode))
            {
                ret.Message = "用户信息失效，请从交建通重新登录云平台";
                return Ok(ret);
            }
            try
            {
                string msg = await _projectService.ProjectApprovalSubmit(request);
                if (string.IsNullOrEmpty(msg))
                {
                    ret.Success = true;
                }
                else
                {
                    ret.Message = msg;
                    ret.Success = false;
                }
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = "出错,请联系程序员";
                LoggerUtils.Error(ex.ToString(), typeof(ProjectController));
            }
            return Ok(ret);
        }

        public async Task<IActionResult> QueryFlowHistory([FromForm] CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (request.ProjectID == 0)
            {
                ret.Message = "项目数据错误";
                return Ok(ret);
            }
            try
            {
                var (list, msg) = await _projectService.QueryFlowHistory(request);
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
                LoggerUtils.Error(ex.ToString(), typeof(ProjectController));
            }
            return Ok(ret);
        }

        public async Task<IActionResult> QueryDKData([FromForm] CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (request.ProjectID == 0)
            {
                ret.Message = "风场数据错误";
                return Ok(ret);
            }
            try
            {
                var (data, msg) = await _projectService.QueryDKData(request);
                if (string.IsNullOrEmpty(msg))
                {
                    ret.Data = data;
                    ret.Success = true;
                }
                else
                {
                    ret.Data = data;
                    ret.Message = msg;
                    ret.Success = false;
                }
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = "出错,请联系程序员";
                LoggerUtils.Error(ex.ToString(), typeof(ProjectController));
            }
            return Ok(ret);
        }


        public async Task<IActionResult> SaveDKMatch([FromForm] CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (!ValidateWebToken(request.PostToken))
            {
                ret.Message = "身份信息失效，请重新登录。";
                return Ok(ret);
            }
            if (request.ProjectID == 0)
            {
                ret.Message = "风场数据错误";
                return Ok(ret);
            }
            try
            {
                string msg = "";

                msg = await _projectService.SaveDKMatch(request);
                if (string.IsNullOrEmpty(msg))
                {
                    //ret.Data = data;
                    ret.Success = true;
                }
                else
                {
                    //ret.Data = data;
                    ret.Message = msg;
                    ret.Success = false;
                }
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = "出错,请联系程序员";
                //记录日志
                LoggerUtils.Error(ex.ToString(), typeof(ProjectController));
            }
            return Ok(ret);
        }

        public async Task<IActionResult> ProjectEditSubmit([FromForm] CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (request.ProjectID == 0)
            {
                ret.Message = "风场数据错误";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(request.ProjectName))
            {
                ret.Message = "风场名不能为空";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(request.ProjectStartTime) || string.IsNullOrEmpty(request.ProjectEndTime))
            {
                ret.Message = "需要输入云平台使用期限";
                return Ok(ret);
            }
            if (string.IsNullOrEmpty(request.ProjectManagerName) || string.IsNullOrEmpty(request.ProjectManagerUserCode))
            {
                ret.Message = "项目经理不能为空";
                return Ok(ret);
            }
            if (CurrentUser == null || string.IsNullOrEmpty(CurrentUser.UserCode))
            {
                ret.Message = "用户信息失效，请从交建通重新登录云平台";
                return Ok(ret);
            }

            try
            {
                string msg = await _projectService.ProjectEditSubmit(request);
                if (string.IsNullOrEmpty(msg))
                {
                    ret.Success = true;
                }
                else
                {
                    ret.Message = msg;
                    ret.Success = false;
                }
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = "出错了";
                LoggerUtils.Error(ex.ToString(), typeof(ProjectController));
            }
            return Ok(ret);
        }

        public async Task<IActionResult> DeleteProjectFlow([FromForm] CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (request.ProjectID == 0)
            {
                ret.Message = "项目数据错误";
                return Ok(ret);
            }

            try
            {
                string msg = await _projectService.DeleteProjectFlow(request);
                if (string.IsNullOrEmpty(msg))
                {
                    ret.Success = true;
                }
                else
                {
                    ret.Message = msg;
                    ret.Success = false;
                }
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Message = "出错了";
                LoggerUtils.Error(ex.ToString(), typeof(ProjectController));
            }
            return Ok(ret);
        }

        public async Task<IActionResult> DeleteProjectFile([FromForm] CloudWindRequest request)
        {
            BaseReturn ret = new BaseReturn();
            if (request.ID == 0)
            {
                ret.Message = "数据错误";
                return Ok(ret);
            }

            try
            {
                var (list, msg) = await _projectService.DeleteProjectFile(request.ID);
                if (string.IsNullOrEmpty(msg))
                {
                    ret.Data = list;
                    ret.Success = true;
                }
                else
                {
                    ret.Message = msg;
                    ret.Success = false;
                }
            }
            catch (Exception ex)
            {
                ret.Success = false;
                LoggerUtils.Error(ex.ToString(), typeof(ProjectController));
            }
            return Ok(ret);
        }
    }
}
