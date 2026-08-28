using Microsoft.AspNetCore.Mvc;
using Model.Base;
using Model.Tech.Cloud.BackManage;
using Service.Base;
using Service.Wind.BackManage;
using Tool;
using WindCloud.Areas.Base.Controllers;

namespace WindCloud.Areas.BackManage.Controllers
{
    [Area("BackManage")]
    public class LibraryController : WebEncryptionController
    {
        private readonly BackLibraryService _libraryService;

        public LibraryController(CookieService cookieService, Service.Wind.WebValidateService webValidateService, BackLibraryService libraryService)
            : base(cookieService, webValidateService)
        {
            _libraryService = libraryService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Ship()
        {
            if (!await RightValidate())
            {
                return Redirect("/Cloud/CloudBase/ErrorPage?ErrorText=没有权限，不能进入后台配置界面。");
            }

            ViewData["PostToken"] = CreateWebToken();
            return View();
        }

        [HttpPost]
        public IActionResult ShipListQuery(CloudWindBackManageRequest request)
        {
            BaseReturn ret = new BaseReturn();
            try
            {
                var (list, totalCount, pageIndex, msg) = _libraryService.ShipListQuery(request);

                if (!string.IsNullOrEmpty(msg))
                {
                    ret.Message = msg;
                    return Ok(ret);
                }

                ret.Data = list;
                ret.TotalCount = totalCount;
                ret.TotalPage = totalCount % request.PageSize == 0 ? (totalCount / request.PageSize) : (totalCount / request.PageSize) + 1;
                ret.Success = true;
                ret.PageIndex = pageIndex;
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(LibraryController));
                ret.Success = false;
                ret.Message = "发生错误";
            }
            return Ok(ret);
        }

        [HttpPost]
        public IActionResult ShipDataQuery(CloudWindBackManageRequest request)
        {
            BaseReturn ret = new BaseReturn();

            if (request.ShipID == 0)
            {
                return Ok(ret);
            }

            try
            {
                var (ship, msg) = _libraryService.ShipDataQuery(request);

                if (!string.IsNullOrEmpty(msg))
                {
                    ret.Message = msg;
                    return Ok(ret);
                }

                ret.Data = ship;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(LibraryController));
                ret.Success = false;
                ret.Message = "发生错误";
            }
            return Ok(ret);
        }

        [HttpPost]
        public IActionResult ShipDataSave(CloudWindBackManageRequest request)
        {
            BaseReturn ret = new BaseReturn();

            if (request.ShipID == 0)
            {
                ret.Message = "船舶数据错误";
                return Ok(ret);
            }

            try
            {
                var msg = _libraryService.ShipDataSave(request);

                if (!string.IsNullOrEmpty(msg))
                {
                    ret.Success = false;
                    ret.Message = msg;
                    return Ok(ret);
                }

                ret.Success = true;
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(LibraryController));
                ret.Success = false;
                ret.Message = "发生错误，请联系管理员";
            }
            return Ok(ret);
        }

        public async Task<IActionResult> DK()
        {
            if (!await RightValidate())
            {
                return Redirect("/Cloud/CloudBase/ErrorPage?ErrorText=不能进入后台配置界面。");
            }

            ViewData["PostToken"] = CreateWebToken();
            return View();
        }

        [HttpPost]
        public IActionResult ProjectListQuery(CloudWindBackManageRequest request)
        {
            BaseReturn ret = new BaseReturn();
            try
            {
                var (list, totalCount, pageIndex, msg) = _libraryService.ProjectListQuery(request);

                if (!string.IsNullOrEmpty(msg))
                {
                    ret.Message = msg;
                    return Ok(ret);
                }

                ret.Data = list;
                ret.TotalCount = totalCount;
                ret.TotalPage = totalCount % request.PageSize == 0 ? (totalCount / request.PageSize) : (totalCount / request.PageSize) + 1;
                ret.Success = true;
                ret.PageIndex = pageIndex;
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(LibraryController));
                ret.Success = false;
                ret.Message = "发生错误";
            }
            return Ok(ret);
        }

        [HttpPost]
        public IActionResult ProjectDataQuery(CloudWindBackManageRequest request)
        {
            BaseReturn ret = new BaseReturn();

            if (request.ProjectID == 0)
            {
                return Ok(ret);
            }

            try
            {
                var (project, msg) = _libraryService.ProjectDataQuery(request);

                if (!string.IsNullOrEmpty(msg))
                {
                    ret.Message = msg;
                    return Ok(ret);
                }

                ret.Data = project;
                ret.Success = true;
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(LibraryController));
                ret.Success = false;
                ret.Message = "发生错误";
            }
            return Ok(ret);
        }

        public IActionResult DKFileExport(CloudWindBackManageRequest request)
        {
            if (request.ProjectID == 0)
            {
                return NotFound();
            }

            try
            {
                var (filePath, fileName, msg) = _libraryService.DKFileExport(request);

                if (!string.IsNullOrEmpty(msg))
                {
                    return NotFound();
                }

                return PhysicalFile(filePath, "application/x-zip-compressed", fileName);
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(LibraryController));
                return NotFound();
            }
        }

        [HttpPost]
        public IActionResult DKFileDelete(CloudWindBackManageRequest request)
        {
            BaseReturn ret = new BaseReturn();

            if (request.ID == 0)
            {
                ret.Message = "地勘数据错误";
                return Ok(ret);
            }

            try
            {
                var msg = _libraryService.DKFileDelete(request);

                if (!string.IsNullOrEmpty(msg))
                {
                    ret.Success = false;
                    ret.Message = msg;
                    return Ok(ret);
                }

                ret.Success = true;
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(LibraryController));
                ret.Success = false;
                ret.Message = "发生错误，请联系管理员";
            }
            return Ok(ret);
        }




    }
}
