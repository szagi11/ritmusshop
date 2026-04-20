using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RitmusShop_keszletkezelo.DTO
{
    public class PageOfProductsDto
    {
        public List<ProductDto> Products { get; set; } = new List<ProductDto>();
        public long TotalRowCount { get; set; }
    }
}
