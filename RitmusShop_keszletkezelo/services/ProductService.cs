using RitmusShop_keszletkezelo.Api;
using RitmusShop_keszletkezelo.DTO;

namespace RitmusShop_keszletkezelo.Services
{
    public class ProductService
    {
        private readonly IHotcakesApiClient _apiClient;

        public ProductService(IHotcakesApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        // Az aggregált, deduplikált terméklistát adja vissza több levél-kategóriából
        public async Task<List<ProductDto>> GetProductsForLeafCategoriesAsync(
            List<string> leafBvins)
        {
            var allProducts = new List<ProductDto>();

            foreach (var bvin in leafBvins)
            {
                var page = await _apiClient.GetProductsForCategoryAsync(bvin, 1, 200);
                allProducts.AddRange(page.Products);
            }

            // Deduplikálás Bvin alapján — ha egy termék több kategóriában is van
            return allProducts
                .GroupBy(p => p.Bvin)
                .Select(g => g.First())
                .ToList();
        }
    }
}