using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RitmusShop_keszletkezelo.DTO
{
    public interface IHotcakesApiClient
    {
        // Kapcsolat tesztelése
        Task<bool> TestConnectionAsync();

        // Kategóriák
        Task<List<CategoryDto>> GetCategoriesAsync();

        // Termékek (egyelőre kommenteljük ki, később hozzáadjuk)
        // Task<PageOfProductsDto> GetProductsForCategoryAsync(string categoryBvin, int pageNumber, int pageSize);
        // Task<ProductDto> GetProductAsync(string bvin);

        // Variánsok
        // Task<List<VariantDto>> GetVariantsByProductAsync(string productBvin);

        // Készlet
        // Task<List<ProductInventoryDto>> GetInventoryForProductAsync(string productBvin);
        // Task<ProductInventoryDto> GetInventoryAsync(string bvin);
        // Task<ProductInventoryDto> CreateInventoryAsync(ProductInventoryDto item);
        // Task<ProductInventoryDto> UpdateInventoryAsync(ProductInventoryDto item);
    }
}
