using Hotcakes.CommerceDTO.v1.Catalog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RitmusShop_keszletkezelo.Services
{
    public interface IHotcakesApiService : IDisposable
    {
        Task<List<CategorySnapshotDTO>> GetCategoriesAsync();
        Task<PageOfProducts> GetProductsForCategoryAsync(string categoryBvin, int pageNumber = 1, int pageSize = 100);
        Task<List<VariantDTO>> GetVariantsForProductAsync(string productBvin);
        Task<List<ProductInventoryDTO>> GetInventoryForProductAsync(string productBvin);
        Task<List<OptionDTO>> GetOptionsForProductAsync(string productBvin);
        Task<List<CategorySnapshotDTO>> GetCategoriesForProductAsync(string productBvin);
        Task<ProductInventoryDTO?> UpdateInventoryAsync(ProductInventoryDTO inventory);
    }
}
