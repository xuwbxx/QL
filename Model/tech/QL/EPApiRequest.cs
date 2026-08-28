namespace Model.tech.QL
{
    /// <summary>
    /// Element Plus Api Request Base
    /// </summary>
    public class EPApiRequest
    {
        public int? PageIndex { set; get; }
        public int? PageSize { set; get; }
        public string? Sort { set; get; }
    }
}
