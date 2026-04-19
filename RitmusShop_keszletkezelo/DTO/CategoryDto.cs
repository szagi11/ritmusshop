using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RitmusShop_keszletkezelo.DTO
{
    public class CategoryDto
    {
        public string Bvin { get; set; }
        public int StoreId { get; set; }
        public string ParentId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
        public bool Hidden { get; set; }
        public string RewriteUrl { get; set; }
    }
}
