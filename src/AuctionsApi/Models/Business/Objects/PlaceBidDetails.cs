using System.ComponentModel.DataAnnotations;

namespace AuctionsApi.Models.Business.Objects
{
    public class PlaceBidDetails
    {
        [Required]
        [Range(0, int.MaxValue)]
        public int BidAmount { get; set; }
    }
}
