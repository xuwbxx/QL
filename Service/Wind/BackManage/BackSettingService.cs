using DataFactory.Factory;
using DataFactory.KingBase.CloudWind;
using Microsoft.AspNetCore.Hosting;
using Tool;

namespace Service.Wind.BackManage
{
    public class BackSettingService
    {
        private readonly CloudWind_KingBase_UnitOfWorkFactory _cloudWindUowFactory;
        private readonly IWebHostEnvironment _env;

        public BackSettingService(CloudWind_KingBase_UnitOfWorkFactory cloudWindUowFactory, IWebHostEnvironment env)
        {
            _cloudWindUowFactory = cloudWindUowFactory;
            _env = env;
        }

        public string DataSave(string oldProjectCode, string newProjectCode)
        {
            string oldCode = oldProjectCode.Trim();
            string newCode = newProjectCode.Trim();

            try
            {
                using (var uow = _cloudWindUowFactory.Create())
                {
                    // 查找旧项目编号是否存在
                    var projectRepo = uow.GetRepository<Wind_Project>();
                    var oldProject = projectRepo.FindFirst(a => !a.IsDelete && a.ProjectCode == oldCode);
                    if (oldProject == null)
                    {
                        return "数据库里没有 " + oldCode + " 这个编号";
                    }

                    // 检查新项目编号是否已存在
                    var newProject = projectRepo.FindFirst(a => !a.IsDelete && a.ProjectCode == newCode);
                    if (newProject != null)
                    {
                        return "数据库里已经存在 " + newCode + " 这个编号,请换一个新编号。";
                    }

                    // Project相关 - 更新编号
                    oldProject.ProjectCode = newCode;

                    // ProjectFile 文件路径更新
                    var projectFileRepo = uow.GetRepository<Wind_ProjectFile>();
                    var projectFiles = projectFileRepo.Find(a => !a.IsDelete && a.ProjectID == oldProject.ID);
                    foreach (var a in projectFiles)
                    {
                        a.FilePath = a.FilePath.Replace(oldCode, newCode);
                    }

                    // Library_Geology 文件路径更新
                    var geologyRepo = uow.GetRepository<Library_Geology>();
                    var geologies = geologyRepo.Find(a => !a.IsDelete && a.ProjectID == oldProject.ID);
                    foreach (var a in geologies)
                    {
                        a.FilePath = a.FilePath.Replace(oldCode, newCode);
                    }

                    // Task相关 - 收集TaskIDs
                    var taskRepo = uow.GetRepository<Wind_Task>();
                    var tasks = taskRepo.Find(a => !a.IsDelete && a.ProjectID == oldProject.ID);
                    List<int> taskIDs = tasks.Select(a => a.ID).ToList();

                    if (taskIDs.Count > 0)
                    {
                        // Flow_Task_ShipFile
                        var shipFileRepo = uow.GetRepository<Flow_Task_ShipFile>();
                        foreach (var a in shipFileRepo.Find(a => !(a.IsDelete == true) && taskIDs.Contains(a.TaskID.Value)))
                        {
                            a.FilePath = a.FilePath.Replace(oldCode, newCode);
                        }

                        // Flow_Task_DKFile
                        var dkFileRepo = uow.GetRepository<Flow_Task_DKFile>();
                        foreach (var a in dkFileRepo.Find(a => !(a.IsDelete == true) && taskIDs.Contains(a.TaskID.Value)))
                        {
                            a.FilePath = a.FilePath.Replace(oldCode, newCode);
                        }

                        // Flow_Task_CommentFile
                        var commentFileRepo = uow.GetRepository<Flow_Task_CommentFile>();
                        foreach (var a in commentFileRepo.Find(a => !(a.IsDelete == true) && taskIDs.Contains(a.TaskID.Value)))
                        {
                            a.FilePath = a.FilePath.Replace(oldCode, newCode);
                        }

                        // Library_Pile
                        var pileRepo = uow.GetRepository<Library_Pile>();
                        foreach (var a in pileRepo.Find(a => !(a.IsDelete == true) && taskIDs.Contains(a.TaskID.Value)))
                        {
                            a.FilePath = a.FilePath.Replace(oldCode, newCode);
                        }

                        // Wind_TaskFile
                        var taskFileRepo = uow.GetRepository<Wind_TaskFile>();
                        foreach (var a in taskFileRepo.Find(a => !(a.IsDelete == true) && taskIDs.Contains(a.TaskID.Value)))
                        {
                            a.FilePath = a.FilePath.Replace(oldCode, newCode);
                        }

                        // Wind_TaskInfoImg_ZJCZ
                        var imgRepo = uow.GetRepository<Wind_TaskInfoImg_ZJCZ>();
                        foreach (var a in imgRepo.Find(a => !(a.IsDelete == true) && taskIDs.Contains(a.TaskID.Value)))
                        {
                            a.FilePath = a.FilePath.Replace(oldCode, newCode);
                        }

                        // Wind_TaskReport
                        var reportRepo = uow.GetRepository<Wind_TaskReport>();
                        foreach (var a in reportRepo.Find(a => !(a.IsDelete == true) && taskIDs.Contains(a.TaskID.Value)))
                        {
                            a.ReportPath = a.ReportPath.Replace(oldCode, newCode);
                        }
                    }

                    // 先把旧的文件夹重命名（避免冲突）
                    string projectDir = Path.Combine(_env.WebRootPath, "File", "Project");
                    string resultOld = FileUtils.RenameDirectory(
                        Path.Combine(projectDir, newCode),
                        newCode + DateTime.UtcNow.ToString("yyyyMMddHHmmss"));

                    // 文件夹名字修改
                    string result = FileUtils.RenameDirectory(
                        Path.Combine(projectDir, oldCode),
                        newCode);

                    if (!string.IsNullOrEmpty(result))
                    {
                        return result;
                    }

                    uow.Save();
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(BackSettingService));
                return "发生错误";
            }

            return "";
        }
    }
}
