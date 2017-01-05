using System.Threading;
using System.Threading.Tasks;

namespace AuctionsApi.Models.Data.Abstract
{
    public interface IRepository<T> : IReadOnlyRepository<T> where T : class
    {
        bool HasPendingChanges { get; }
        void Create(T item);
        void Update(T item);
        void Delete(T item);
        Task SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
