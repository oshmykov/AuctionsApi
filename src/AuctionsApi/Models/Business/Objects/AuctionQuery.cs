using AuctionsApi.Models.Business.Abstract;

namespace AuctionsApi.Models.Business.Objects
{
    public class AuctionQuery : IQueryWithPagination
    {
        public string Label { get; set; }
    }
}
