using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AuctionsApi.Models.Data.Abstract
{
    public interface IReadOnlyRepository<T>
    {
        Task<IEnumerable<T>> ReadAsync(
            Expression<Func<T, bool>> filter = null,
            string sortBy = default(string),
            bool ascending = default(bool),
            int? skip = default(int?),
            int? take = default(int?));

        Task<T> ReadOneAsync(
            Expression<Func<T, bool>> filter = null);
    }
}
