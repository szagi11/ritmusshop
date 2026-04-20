using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RitmusShop_keszletkezelo.Api
{
    public class ApiException : Exception
    {
        public List<ApiError> ApiErrors { get; } = new List<ApiError>();

        public ApiException(string message) : base(message) { }

        public ApiException(string message, Exception inner) : base(message, inner) { }

        public ApiException(string message, List<ApiError> errors) : base(message)
        {
            ApiErrors = errors;
        }
    }
}
