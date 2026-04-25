// File: Services/HotcakesApiService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Hotcakes.CommerceDTO.v1.Catalog;
using Newtonsoft.Json;

namespace RitmusShop_keszletkezelo.Services
{
    /// <summary>
    /// Saját Hotcakes REST kliens, HttpClient + Newtonsoft.Json alapon.
    /// Azért nem használjuk a gyári Hotcakes.CommerceDTO.v1.Client.Api proxyt,
    /// mert annak belső JSON parsere a System.Web.Extensions.dll-re támaszkodik,
    /// ami nem létezik .NET 8 alatt. A gyári DTO-kat (CategorySnapshotDTO, ProductDTO,
    /// VariantDTO, ProductInventoryDTO, PageOfProducts) viszont megtartjuk.
    /// </summary>
    public class HotcakesApiService : IDisposable
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private readonly string _basePath;  // pl. "DesktopModules/Hotcakes/API/rest/v1/"

        /// <param name="baseUrl">A bolt host gyökér URL-je
        /// (pl. "http://20.234.52.47/").</param>
        /// <param name="apiKey">Hotcakes admin API kulcs.</param>
        public HotcakesApiService(string baseUrl, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentNullException(nameof(baseUrl));
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentNullException(nameof(apiKey));

            // Garantáljuk, hogy a host '/'-ra végződjön — különben a relatív URI rosszul fűződik.
            if (!baseUrl.EndsWith("/")) baseUrl += "/";

            _http = new HttpClient
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
            _apiKey = apiKey;
            _basePath = "DesktopModules/Hotcakes/API/rest/v1/";
        }

        // =================================================================
        // PUBLIKUS API — UGYANAZ A SZERZŐDÉS, MINT A GYÁRI VERZIÓNÁL VOLT
        // =================================================================

        public async Task<List<CategorySnapshotDTO>> GetCategoriesAsync()
        {
            var resp = await GetAsync<List<CategorySnapshotDTO>>(
                "categories/", "Kategóriák lekérése");
            return resp ?? new List<CategorySnapshotDTO>();
        }

        public async Task<PageOfProducts> GetProductsForCategoryAsync(
            string categoryBvin, int pageNumber = 1, int pageSize = 100)
        {
            var resp = await GetAsync<PageOfProducts>(
                $"products/?bycategory={Uri.EscapeDataString(categoryBvin)}" +
                $"&page={pageNumber}&pagesize={pageSize}",
                $"Termékek lekérése (kategória: {categoryBvin})");
            return resp ?? new PageOfProducts();
        }

        public async Task<List<VariantDTO>> GetVariantsForProductAsync(string productBvin)
        {
            // Megjegyzés: a gyári proxy itt "productvariant" (egyes szám) endpointot hív.
            var resp = await GetAsync<List<VariantDTO>>(
                $"productvariant/?productid={Uri.EscapeDataString(productBvin)}",
                $"Variánsok lekérése (termék: {productBvin})");
            return resp ?? new List<VariantDTO>();
        }

        public async Task<List<ProductInventoryDTO>> GetInventoryForProductAsync(string productBvin)
        {
            var resp = await GetAsync<List<ProductInventoryDTO>>(
                $"productinventory/?byproduct={Uri.EscapeDataString(productBvin)}",
                $"Készlet lekérése (termék: {productBvin})");
            return resp ?? new List<ProductInventoryDTO>();
        }

        public async Task<ProductInventoryDTO?> UpdateInventoryAsync(ProductInventoryDTO inventory)
        {
            if (inventory == null) throw new ArgumentNullException(nameof(inventory));
            // A gyári proxy a Bvin-t teszi az URL-be, és a teljes objektumot a body-ba.
            return await PostAsync<ProductInventoryDTO>(
                $"productinventory/{Uri.EscapeDataString(inventory.Bvin ?? string.Empty)}",
                inventory,
                "Készlet frissítése");
        }

        // =================================================================
        // BELSŐ HTTP RÉTEG
        // =================================================================

        /// <summary>Generikus GET. A relatív útvonal végéhez magunk fűzzük az API kulcsot.</summary>
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

        /// <summary>Generikus POST. A body Newtonsoft-tal sorosítva megy.</summary>
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

        /// <summary>
        /// A választ ApiResponseEnvelope-ba deszerializálja, kicsomagolja a Content-et,
        /// és ha vannak Errors, kivételt dob érthető üzenettel.
        /// </summary>
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

        public async Task<List<OptionDTO>> GetOptionsForProductAsync(string productBvin)
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

        private string AppendApiKey(string relativePath)
        {
            var sep = relativePath.Contains('?') ? "&" : "?";
            return $"{relativePath}{sep}key={Uri.EscapeDataString(_apiKey)}";
        }

        private static string Truncate(string s, int max) =>
            s.Length <= max ? s : s.Substring(0, max) + "…";

        public void Dispose()
        {
            _http.Dispose();
        }

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