using System.Collections.Generic;

namespace AuctionsApi.Business.Models.Abstract
{
    public enum ResultTypes { Success, BadRequest, NotFound };

    public interface ICommandResult<T> where T : class
    {
        IList<string> Errors { get; }
        ResultTypes ResultType { get; }
        T Details { get; }
    }

    public interface IPlainCommandResult : ICommandResult<object>
    {

    }
}