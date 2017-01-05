using AuctionsApi.Business.Models.Abstract;
using AuctionsApi.Business.Models.Impl;

namespace AuctionsApi.Models.Business.Makers
{
    public class CommandResultMakers
    {
        public static IPlainCommandResult GetPlainCommandResultByType(ResultTypes resultType)
        {
            switch (resultType)
            {
                case ResultTypes.NotFound:
                    return CommandResult.NotFound("Not found message");
                case ResultTypes.BadRequest:
                    return CommandResult.BadRequest("Bad request message");
                default:
                    return CommandResult.Success();
            }
        }
    }
}
