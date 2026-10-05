using System;
using System.Collections.Generic;
using System.Text;

namespace Cessa.Application.Common.Exceptions
{
    public class ValidationException : Exception
    {
        public IDictionary<string, string[]> Errors { get; }
        public ValidationException(IDictionary<string, string[]> errors)
            : base("One or more validation failures have occurred.") => Errors = errors;
    }
}
