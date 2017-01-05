namespace AuctionsApi.Models.Business.Abstract
{
    public class IQueryWithPagination
    {
        public string SortBy { get; set; }
        public bool Ascending { get; set; }
        public int? Skip { get; set; }
        public int? Take { get; set; }
    }
}
