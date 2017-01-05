namespace AuctionsApi.Models.Business.Abstract
{
    public interface IMapper<TSource, TDestination>
    {
        TDestination Map(TSource source, params object[] parameters);
    }
}
