using Model.Tech.Cloud.Webgis;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Towing
{
    public class EChartBase
    {
        public string Title { set; get; }
        public List<string> Legend { set; get; }
        public List<List<string>> LegendDataSet { set; get; }
        public List<List<object>> DataSet { set; get; }
        public EChartBase()
        {
            Title = "";
            Legend = new List<string>();
            LegendDataSet = new List<List<string>>();
            DataSet = new List<List<object>>();
        }

        public void AddData(string legend, string legendData, int data)
            => AddData(legend, legendData, (decimal)data);
        public void AddData(string legend, string legendData, double data)
            => AddData(legend, legendData, (decimal)data);
        public void AddData(string legend, string legendData, decimal data)
            => AddData(legend, legendData, (object)data);
        public void AddData(string legend, string legendData, object data)
        {
            if (!Legend.Contains(legend))
            {
                Legend.Add(legend);
                LegendDataSet.Add(new List<string>());
                DataSet.Add(new List<object>());
            }
            var idx = Legend.IndexOf(legend);
            if (LegendDataSet[idx] == null) LegendDataSet[idx] = new List<string>();
            LegendDataSet[idx].Add(legendData);
            if (DataSet[idx] == null) DataSet[idx] = new List<object>();
            DataSet[idx].Add(data);
        }
    }
}
