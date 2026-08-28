namespace Model.Tech.Cloud.Webgis
{
    public class GisDataReport
    {
        public GisDataReport()
        {
            ProjectFile = new ReportModel();

            CBZExportFile = new ReportModel();
            CBZImportFile = new ReportModel();

            KZYExportFile = new ReportModel();
            KZYImportFile = new ReportModel();

            PileExportFile = new ReportModel();
            PileImportFile = new ReportModel();
        }

        public ReportModel ProjectFile { set; get; }

        public ReportModel CBZExportFile { set; get; }
        public ReportModel CBZImportFile { set; get; }

        public ReportModel KZYExportFile { set; get; }
        public ReportModel KZYImportFile { set; get; }

        public ReportModel PileExportFile { set; get; }
        public ReportModel PileImportFile { set; get; }
    }


    public class ReportModel
    {
        public ReportModel()
        {
            Files = new List<CloudProjectFile>();
        }

        public string TabName { set; get; }

        public List<CloudProjectFile> Files { set; get; }
    }

}
