using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RitmusShop_keszletkezelo.DTO
{
    public class ProductDto
    {
        public string Bvin { get; set; }
        public string Sku { get; set; }
        public string ProductName { get; set; }
        public decimal SitePrice { get; set; }

    }
}
