using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Tech.WebApi
{
    public class WindCloudBaseReturn
    {
        public WindCloudBaseReturn()
        {
            Success = false;
            CBZinfo = new WindCloudCBZ();
        }
        public bool Success { set; get; }
        public string Message { set; get; }
        public object Data { set; get; }

        public WindCloudCBZ CBZinfo { set; get; }
    }

    public class WindCloudCBZ
    {
        public string ProjectName { set; get; }
        public string TaskName { set; get; }
        public string WaterDepth { set; get; }
    }

    public class WindCloudKZY
    {
        
    }
}
