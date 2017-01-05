using AuctionsApi.Business.Models.Abstract;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AuctionsApi.Business.Models.Impl
{
    public class CommandResult : IPlainCommandResult
    {
        private readonly IList<string> errors;
        private readonly ResultTypes resultType;
        private readonly object details;

        private CommandResult(IList<string> errors, ResultTypes resultType, object details)
        {
            this.errors = errors;
            this.resultType = resultType;
            this.details = details;
        }

        public object Details
        {
            get
            {
                return details;
            }
        }

        public IList<string> Errors
        {
            get
            {
                return errors;
            }
        }

        public ResultTypes ResultType
        {
            get
            {
                return resultType;
            }
        }

        private static IPlainCommandResult GetInstance(IList<string> errors, ResultTypes resultType, object details = null)
        {
            return new CommandResult(errors, resultType, details);
        }

        public static IPlainCommandResult NotFound(string errorMessage = "")
        {
            return GetInstance(new List<string> { errorMessage }, ResultTypes.NotFound);
        }

        public static IPlainCommandResult BadRequest(string errorMessage = "")
        {
            return GetInstance(new List<string> { errorMessage }, ResultTypes.BadRequest);
        }

        public static IPlainCommandResult Success(object details = null)
        {
            return GetInstance(new List<string>(), ResultTypes.Success, details);
        }
    }
}
