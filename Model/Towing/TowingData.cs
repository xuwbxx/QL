using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Towing
{
    public class TowingData
    {
        public TowingData()
        {
            PositionTime = new TowingPosition();
            PositionHistory = new List<TowingPosition>();
        }
        public TowingPosition PositionTime { set; get; }

        public List<TowingPosition> PositionHistory { set; get; }
    }

    public class TowingPosition
    {
        public string Lon { set; get; }
        
        public string Lat { set; get; }

        public string Time { set; get; }
    }

}
