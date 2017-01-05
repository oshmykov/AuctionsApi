using System.Collections.Generic;

namespace AuctionsApi.Models.Business.Abstract
{
    public interface IAuctionSpecificationsFactory<T>
    {
        Specification<T> IsActive { get; }

        Specification<T> Not(Specification<T> specification);

        Specification<T> DidPlaceBid(string[] myAuctions);

        Specification<T> IsOwningTheHighestBid(string[] myAuctions, string participantId);

        Specification<T> GetSpecification(string label, string[] myAuctions, string participantId);
    }
}
