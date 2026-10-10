using DataFactory.Factory;
using DataFactory.KingBase.Towing;
using Model.Tech.Cloud;
using Tool;

namespace Service.Towing
{
    public class TowingFileInfo
    {
        public int ID { set; get; }
        public string FileName { set; get; }
        public string FilePath { set; get; }
        public string FileType { set; get; }
        public string FileExtension { set; get; }
        public string CreateTime { set; get; }
        public string StartTime { set; get; }
        public string EndTime { set; get; }
    }

    public class FileInfoService
    {
        private readonly Towing_KingBase_UnitOfWorkFactory _towingKingBaseUowFactory;
        private readonly CloudWind_KingBase_UnitOfWorkFactory _cloudWindUowFactory;

        public FileInfoService(
            Towing_KingBase_UnitOfWorkFactory towingKingBaseUowFactory,
            CloudWind_KingBase_UnitOfWorkFactory cloudWindUowFactory)
        {
            _towingKingBaseUowFactory = towingKingBaseUowFactory;
            _cloudWindUowFactory = cloudWindUowFactory;
        }

        public List<TowingFileInfo> GetReportFile(DateTime? startTime = null, DateTime? endTime = null)
        {
            var res = new List<TowingFileInfo>();
            try
            {
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var repo = uow.GetRepository<Data_File>();

                    if (startTime == null || endTime == null)
                    {
                        repo.Find(a => !a.IsDelete && a.Type == 1)
                            .OrderByDescending(a => a.CreateTime)
                            .Take(100)
                            .ToList()
                            .ForEach(a =>
                            {
                                res.Add(MapToFileInfo(a));
                            });
                    }
                    else
                    {
                        repo.Find(a => !a.IsDelete && a.Type == 1 && a.CreateTime >= startTime && a.CreateTime <= endTime)
                            .OrderByDescending(a => a.CreateTime)
                            .Take(100)
                            .ToList()
                            .ForEach(a =>
                            {
                                res.Add(MapToFileInfo(a));
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(FileInfoService));
            }
            return res;
        }

        public List<TowingFileInfo> GetForecastFile(DateTime? startTime = null, DateTime? endTime = null)
        {
            var res = new List<TowingFileInfo>();
            try
            {
                int projectID = 61;

                using (var uow = _cloudWindUowFactory.Create())
                {
                    var taskRepo = uow.GetRepository<DataFactory.KingBase.CloudWind.View_Wind_ProjectTask>();
                    var taskFileRepo = uow.GetRepository<DataFactory.KingBase.CloudWind.Wind_TaskFile>();

                    var taskIDs = taskRepo.Find(a => a.ProjectID == projectID && a.SoftwareID == (int)CloudWindSoftware.拖航可行性分析与预报)
                        .Select(a => a.id)
                        .ToList();

                    if (startTime == null || endTime == null)
                    {
                        taskFileRepo.Find(a => a.IsDelete != true && taskIDs.Contains(a.TaskID.Value) && a.TypeID == (int)CloudWindTaskSendFileType.云推送文件)
                            .OrderByDescending(a => a.CreateTime)
                            .Take(100)
                            .ToList()
                            .ForEach(a =>
                            {
                                res.Add(MapTaskFileToFileInfo(a));
                            });
                    }
                    else
                    {
                        taskFileRepo.Find(a => a.IsDelete != true && a.CreateTime >= startTime && a.CreateTime <= endTime && taskIDs.Contains(a.TaskID.Value) && a.TypeID == (int)CloudWindTaskSendFileType.云推送文件)
                            .OrderByDescending(a => a.CreateTime)
                            .Take(100)
                            .ToList()
                            .ForEach(a =>
                            {
                                res.Add(MapTaskFileToFileInfo(a));
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(FileInfoService));
            }
            return res;
        }

        private static TowingFileInfo MapToFileInfo(Data_File a)
        {
            return new TowingFileInfo()
            {
                ID = a.id,
                FileName = a.FileName,
                FileExtension = FileUtils.GetFileExtension(a.FileName).ToLower().Trim('.'),
                FilePath = FileUtils.GetRelativeFilePath(a.FilePath),
                CreateTime = a.CreateTime == null ? "" : a.CreateTime.Value.ToString("yyyy-MM-dd HH:mm:ss")
            };
        }

        private static TowingFileInfo MapTaskFileToFileInfo(DataFactory.KingBase.CloudWind.Wind_TaskFile a)
        {
            return new TowingFileInfo()
            {
                ID = a.id,
                FileName = a.FileName,
                FileExtension = FileUtils.GetFileExtension(a.FileName).ToLower().Trim('.'),
                FilePath = FileUtils.GetRelativeFilePath(a.FilePath),
                CreateTime = a.CreateTime == null ? "" : a.CreateTime.Value.ToString("yyyy-MM-dd HH:mm:ss")
            };
        }
    }
}
