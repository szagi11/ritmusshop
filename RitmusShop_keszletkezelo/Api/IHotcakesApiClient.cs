using RitmusShop_keszletkezelo.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RitmusShop_keszletkezelo.Api
{
    public interface IHotcakesApiClient
    {
        // Kapcsolat tesztelése
        Task<bool> TestConnectionAsync();

        // Kategóriák
        Task<List<CategoryDto>> GetCategoriesAsync();

        Task<PageOfProductsDto> GetProductsForCategoryAsync(string categoryBvin, int pageNumber, int pageSize);

    }
}
