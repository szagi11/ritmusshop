using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RitmusShop_keszletkezelo.Api
{
    public class ApiResponse<T>
    {
        public T Content { get; set; }
        public List<ApiError> Errors { get; set; }
    }

    public class ApiError
    {
        public string Code { get; set; }
        public string Description { get; set; }
    }
    
}
