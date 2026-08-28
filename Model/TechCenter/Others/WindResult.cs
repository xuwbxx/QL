namespace Model.TechCenter.Others
{
    public class WindData
    {
        public DateTime valid_time { get; set; }
        public double wind { get; set; }
        public double wind_dir { get; set; }
        public double Hs { get; set; }

        public double Swell_period { get; set; }
    }

    public class WindResultYear
    {
        public WindResultYear()
        {
            Months = new List<WindResultMonth>();
        }

        public int Year { set; get; }

        public List<WindResultMonth> Months { set; get; }

    }

    public class WindResultMonth
    {
        public WindResultMonth()
        {
            Days = new List<WindResultDay>();
        }

        public int Month { set; get; }

        public List<WindResultDay> Days { set; get; }

    }

    public class WindResultDay
    {
        public WindResultDay()
        {
            Hours = new List<WindResultHour>();
        }

        public int Day { set; get; }

        public List<WindResultHour> Hours { set; get; }

    }

    public class WindResultHour
    {
        public int Hour { set; get; }

        public bool Do { set; get; }
    }
}
