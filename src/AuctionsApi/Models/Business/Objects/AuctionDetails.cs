using System.Collections.Generic;

namespace AuctionsApi.Business.Models.Objects
{
    public class AuctionDetails : AuctionSummary
    {
        public IEnumerable<string> Images { get; set; }
        public string Description { get; set; }
    }
}
