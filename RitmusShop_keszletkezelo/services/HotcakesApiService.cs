using Hotcakes.CommerceDTO.v1.Catalog;
using Newtonsoft.Json;
using RitmusShop_keszletkezelo.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RitmusShop_keszletkezelo.Services
{
    public class HotcakesApiService : IHotcakesApiService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private readonly string _basePath;

        // ------------------------------------------------------------------
        // CACHE — termékenként és kategóriánként tartjuk a Task-okat, hogy
        // a háttér-előmelegítés és a felhasználói kattintás ne duplikáljon
        // hálózati kérést. Lazy<Task<T>> garantálja, hogy egy adott kulcsra
        // csak EGY fetch indul el versenyhelyzetben is.
        // Hibás eredmény nincs cache-elve — kivételnél töröljük a kulcsot,
        // hogy a következő hívás újrapróbálkozhasson.
        // ------------------------------------------------------------------
        private readonly ConcurrentDictionary<string, Lazy<Task<PageOfProducts>>> _productsCache = new();
        private readonly ConcurrentDictionary<string, Lazy<Task<List<VariantDTO>>>> _variantsCache = new();
        private readonly ConcurrentDictionary<string, Lazy<Task<List<ProductInventoryDTO>>>> _inventoryCache = new();
        private readonly ConcurrentDictionary<string, Lazy<Task<List<OptionDTO>>>> _optionsCache = new();
        private readonly ConcurrentDictionary<string, Lazy<Task<List<CategorySnapshotDTO>>>> _productCategoriesCache = new();
        private Lazy<Task<List<CategorySnapshotDTO>>>? _categoriesCache;

        /// <param name="baseUrl">A bolt host gyökér URL-je
        /// (pl. "http://20.234.52.47/").</param>
        /// <param name="apiKey">Hotcakes admin API kulcs.</param>
        public HotcakesApiService(
            HttpClient httpClient,
            string apiKey,
            string basePath = "DesktopModules/Hotcakes/API/rest/v1/")
        {
            _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentNullException(nameof(apiKey));

            _apiKey = apiKey;
            _basePath = basePath;
        }

        // =================================================================
        // PUBLIKUS API — UGYANAZ A SZERZŐDÉS, MINT A GYÁRI VERZIÓNÁL VOLT
        // =================================================================

        public Task<List<CategorySnapshotDTO>> GetCategoriesAsync()
        {
            // Egyetlen kulcsú cache — a teljes kategóriafa egyszer.
            var lazy = LazyInitializer.EnsureInitialized(
                ref _categoriesCache,
                () => new Lazy<Task<List<CategorySnapshotDTO>>>(FetchAllCategoriesAsync));
            return UnwrapOrEvictAsync(lazy, () => _categoriesCache = null);
        }

        private async Task<List<CategorySnapshotDTO>> FetchAllCategoriesAsync()
        {
            var resp = await GetAsync<List<CategorySnapshotDTO>>(
                "categories/", "Kategóriák lekérése");
            return resp ?? new List<CategorySnapshotDTO>();
        }

        public Task<PageOfProducts> GetProductsForCategoryAsync(
            string categoryBvin, int pageNumber = 1, int pageSize = 100)
        {
            // A paginált kulcs része a pageNumber + pageSize is, hogy különböző
            // oldalkérések ne ütközzenek. A gyakorlatban mindenhol az alap (1/100)
            // hívást használjuk, így gyakorlatilag egy bejegyzés / kategória.
            var cacheKey = $"{categoryBvin}|p={pageNumber}|s={pageSize}";
            var lazy = _productsCache.GetOrAdd(cacheKey,
                k => new Lazy<Task<PageOfProducts>>(
                    () => FetchProductsForCategoryAsync(categoryBvin, pageNumber, pageSize)));
            return UnwrapOrEvictAsync(lazy, () => _productsCache.TryRemove(cacheKey, out _));
        }

        private async Task<PageOfProducts> FetchProductsForCategoryAsync(
            string categoryBvin, int pageNumber, int pageSize)
        {
            var resp = await GetAsync<PageOfProducts>(
                $"products/?bycategory={Uri.EscapeDataString(categoryBvin)}" +
                $"&page={pageNumber}&pagesize={pageSize}",
                $"Termékek lekérése (kategória: {categoryBvin})");
            return resp ?? new PageOfProducts();
        }

        public Task<List<VariantDTO>> GetVariantsForProductAsync(string productBvin)
        {
            var lazy = _variantsCache.GetOrAdd(productBvin,
                k => new Lazy<Task<List<VariantDTO>>>(() => FetchVariantsAsync(k)));
            return UnwrapOrEvictAsync(lazy, () => _variantsCache.TryRemove(productBvin, out _));
        }

        private async Task<List<VariantDTO>> FetchVariantsAsync(string productBvin)
        {
            // Megjegyzés: a gyári proxy itt "productvariant" (egyes szám) endpointot hív.
            var resp = await GetAsync<List<VariantDTO>>(
                $"productvariant/?productid={Uri.EscapeDataString(productBvin)}",
                $"Variánsok lekérése (termék: {productBvin})");
            return resp ?? new List<VariantDTO>();
        }

        public Task<List<ProductInventoryDTO>> GetInventoryForProductAsync(string productBvin)
        {
            var lazy = _inventoryCache.GetOrAdd(productBvin,
                k => new Lazy<Task<List<ProductInventoryDTO>>>(() => FetchInventoryAsync(k)));
            return UnwrapOrEvictAsync(lazy, () => _inventoryCache.TryRemove(productBvin, out _));
        }

        private async Task<List<ProductInventoryDTO>> FetchInventoryAsync(string productBvin)
        {
            var resp = await GetAsync<List<ProductInventoryDTO>>(
                $"productinventory/?byproduct={Uri.EscapeDataString(productBvin)}",
                $"Készlet lekérése (termék: {productBvin})");
            return resp ?? new List<ProductInventoryDTO>();
        }

        public async Task<ProductInventoryDTO?> UpdateInventoryAsync(ProductInventoryDTO inventory)
        {
            if (inventory == null) throw new ArgumentNullException(nameof(inventory));
            return await PostAsync<ProductInventoryDTO>(
                $"productinventory/{Uri.EscapeDataString(inventory.Bvin ?? string.Empty)}",
                inventory,
                "Készlet frissítése");
        }

        // =================================================================
        // BELSŐ HTTP RÉTEG
        // =================================================================

        private async Task<T?> GetAsync<T>(string relativePath, string operationName)
        {
            var url = AppendApiKey(relativePath);
            HttpResponseMessage httpResp;
            try
            {
                httpResp = await _http.GetAsync(_basePath + url);
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException(
                    $"{operationName}: hálózati hiba — {ex.Message}", ex);
            }

            return await DeserializeResponseAsync<T>(httpResp, operationName);
        }

        private async Task<T?> PostAsync<T>(string relativePath, object body, string operationName)
        {
            var url = AppendApiKey(relativePath);
            var json = JsonConvert.SerializeObject(body);
            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage httpResp;
            try
            {
                httpResp = await _http.PostAsync(_basePath + url, content);
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException(
                    $"{operationName}: hálózati hiba — {ex.Message}", ex);
            }

            return await DeserializeResponseAsync<T>(httpResp, operationName);
        }

        private static async Task<T?> DeserializeResponseAsync<T>(
            HttpResponseMessage httpResp, string operationName)
        {
            var raw = await httpResp.Content.ReadAsStringAsync();

            if (!httpResp.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"{operationName}: HTTP {(int)httpResp.StatusCode} {httpResp.ReasonPhrase}. " +
                    $"Válasz: {Truncate(raw, 500)}");
            }

            ApiResponseEnvelope<T>? envelope;
            try
            {
                envelope = JsonConvert.DeserializeObject<ApiResponseEnvelope<T>>(raw);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    $"{operationName}: a válasz nem értelmezhető JSON. {ex.Message}\n" +
                    $"Nyers válasz: {Truncate(raw, 500)}", ex);
            }

            if (envelope == null)
                throw new InvalidOperationException($"{operationName}: üres válasz az API-tól.");

            if (envelope.Errors != null && envelope.Errors.Count > 0)
            {
                var msg = string.Join("; ", envelope.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"{operationName}: {msg}");
            }

            return envelope.Content;
        }

        public Task<List<OptionDTO>> GetOptionsForProductAsync(string productBvin)
        {
            var lazy = _optionsCache.GetOrAdd(productBvin,
                k => new Lazy<Task<List<OptionDTO>>>(() => FetchOptionsAsync(k)));
            return UnwrapOrEvictAsync(lazy, () => _optionsCache.TryRemove(productBvin, out _));
        }

        private async Task<List<OptionDTO>> FetchOptionsAsync(string productBvin)
        {
            try
            {
                var resp = await GetAsync<List<OptionDTO>>(
                    $"productoptions/?productbvin={Uri.EscapeDataString(productBvin)}",
                    $"Opciók lekérése (termék: {productBvin})");
                return resp ?? new List<OptionDTO>();
            }
            catch (InvalidOperationException ex) when (
                ex.Message.Contains("No options were found", StringComparison.OrdinalIgnoreCase))
            {
                return new List<OptionDTO>();
            }
        }

        public Task<List<CategorySnapshotDTO>> GetCategoriesForProductAsync(string productBvin)
        {
            var lazy = _productCategoriesCache.GetOrAdd(productBvin,
                k => new Lazy<Task<List<CategorySnapshotDTO>>>(() => FetchCategoriesForProductAsync(k)));
            return UnwrapOrEvictAsync(lazy, () => _productCategoriesCache.TryRemove(productBvin, out _));
        }

        private async Task<List<CategorySnapshotDTO>> FetchCategoriesForProductAsync(string productBvin)
        {
            var resp = await GetAsync<List<CategorySnapshotDTO>>(
                $"categories/?byproduct={Uri.EscapeDataString(productBvin)}",
                $"Termék kategóriái lekérése (termék: {productBvin})");
            return resp ?? new List<CategorySnapshotDTO>();
        }

        /// <summary>
        /// A Lazy.Value Task-jára vár; ha kivételt dob, eltávolítja a cache-ből,
        /// hogy a következő hívás újrapróbálkozhasson.
        /// </summary>
        private static async Task<T> UnwrapOrEvictAsync<T>(
            Lazy<Task<T>> lazy, Action evict)
        {
            try
            {
                return await lazy.Value.ConfigureAwait(false);
            }
            catch
            {
                evict();
                throw;
            }
        }

        private string AppendApiKey(string relativePath)
        {
            var sep = relativePath.Contains('?') ? "&" : "?";
            return $"{relativePath}{sep}key={Uri.EscapeDataString(_apiKey)}";
        }

        private static string Truncate(string s, int max) =>
            s.Length <= max ? s : s.Substring(0, max) + "…";

        public void Dispose() { }

        private class ApiResponseEnvelope<T>
        {
            [JsonProperty("Content")]
            public T? Content { get; set; }

            [JsonProperty("Errors")]
            public List<ApiErrorDto>? Errors { get; set; }
        }

        private class ApiErrorDto
        {
            [JsonProperty("Code")]
            public string? Code { get; set; }

            [JsonProperty("Description")]
            public string? Description { get; set; }
        }
    }
}