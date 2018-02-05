﻿using AspNetSkeleton.Common.DataTransfer;
using AspNetSkeleton.Common.Utils;
using System.ComponentModel.DataAnnotations;

namespace AspNetSkeleton.Service.Contract
{
    public enum QueryErrorCode
    {
        Unknown = ServiceErrorCode.Unknown,

        [Display(Name = "Value for parameter {0} was not specified.")]
        ParamNotSpecified = ServiceErrorCode.ParamNotSpecified,

        [Display(Name = "Value of parameter {0} is not valid.")]
        ParamNotValid = ServiceErrorCode.ParamNotValid,

        [Display(Name = "Entity identified by parameter {0} was not found.")]
        EntityNotFound = ServiceErrorCode.EntityNotFound,
    }

    public class QueryErrorException : ServiceErrorException
    {
        public QueryErrorException(ErrorData error) : base(error) { }

        public QueryErrorException(QueryErrorCode errorCode, params object[] args)
            : this(new ErrorData { Code = (int)errorCode, Args = args }) { }

        public new QueryErrorCode ErrorCode => (QueryErrorCode)Error.Code;

        public override string Message
        {
            get
            {
                var displayText = ErrorCode.DisplayText();
                return
                    displayText != null ?
                    string.Format(displayText, Error.Args) :
                    $"Query execution failed with error code {ErrorCode}.";
            }
        }
    }
}