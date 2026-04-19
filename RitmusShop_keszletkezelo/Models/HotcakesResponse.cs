using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RitmusShop_keszletkezelo.Models
{
    public class HotcakesResponse<T>
    {
        public T? Content { get; set; }
        public List<HotcakesError> Errors { get; set; } = new();
    }

    public class HotcakesError
    {
        public string Code { get; set; } = "";
        public string Description { get; set; } = "";
    }
}
