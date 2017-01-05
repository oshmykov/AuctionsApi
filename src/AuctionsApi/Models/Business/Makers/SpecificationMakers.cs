using AuctionsApi.Models.Business.Abstract;
using AuctionsApi.Models.Business.Impl.Mongo;
using AuctionsApi.Models.Data.Impl.Mongo.Documents;

namespace AuctionsApi.Models.Business.Makers
{
    public class SpecificationMakers
    {
        public static Specification<AuctionDoc> GetDefault
        {
            get
            {
                return new IsActiveAuctionSpecification();
            }
        }
    }
}
