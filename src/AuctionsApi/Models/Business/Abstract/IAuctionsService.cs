using System.Threading.Tasks;
using AuctionsApi.Business.Models.Objects;
using System.Collections.Generic;
using AuctionsApi.Models.Business.Objects;

namespace AuctionsApi.Business.Models.Abstract
{
    public interface IAuctionsService
    {
        Task<IEnumerable<AuctionSummary>> GetAuctionsAsync(AuctionQuery queryParameters, string participantId);

        Task<AuctionSummary> GetAuctionAsync(string auctionId, string participantId);

        Task<IPlainCommandResult> PlaceBidAsync(string auctionId, string participantId, int bidAmount);
    }
}
