namespace Model.tech.QL
{
    /// <summary>
    /// Element Plus Api Paged List
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EPPageListBase<T>
    {
        /// <summary>
        /// Page Index (from 1)
        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// Page Size
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// Total Count
        /// </summary>
        public int Total { get; set; }
        /// <summary>
        /// Current Page Items
        /// </summary>
        List<T> Items { get; set; }
    }
}
