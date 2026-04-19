using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RitmusShop_keszletkezelo.DTO;

namespace RitmusShop_keszletkezelo.Api
{
    /// A Hotcakes REST API HTTP kliense.
    /// A kérésekhez automatikusan hozzáfűzi az API kulcsot, és kezeli a JSON de/serialisationt.

    public class HotcakesApiClient : IHotcakesApiClient, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public HotcakesApiClient(string baseUrl, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentException("A base URL nem lehet üres.", nameof(baseUrl));
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("Az API kulcs nem lehet üres.", nameof(apiKey));

            // A base URL mindig / karakterre végződjön, hogy a relatív URL-ek helyesen fűződjenek hozzá
            string normalizedUrl = baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/";
            _apiKey = apiKey;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(normalizedUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };

            _httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        private string BuildUrl(string endpoint, Dictionary<string, string> queryParams = null)
        {
            var sb = new StringBuilder(endpoint);
            sb.Append("?key=").Append(Uri.EscapeDataString(_apiKey));

            if (queryParams != null)
            {
                foreach (var kv in queryParams)
                {
                    sb.Append("&")
                        .Append(Uri.EscapeDataString(kv.Key))
                        .Append("=")
                        .Append(Uri.EscapeDataString(kv.Value));
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// GET kérés végrehajtása és a válasz Content mezőjének visszaadása.
        /// </summary>
        private async Task<T> GetAsync<T>(string url)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<T>>(json);

                if (apiResponse == null)
                    throw new ApiException("Üres vagy érvénytelen válasz az API-tól.");

                if (apiResponse.Errors != null && apiResponse.Errors.Count > 0)
                    throw new ApiException("Az API hibát adott vissza.", apiResponse.Errors);

                return apiResponse.Content;
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException("Hálózati hiba a GET hívás során.", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new ApiException("A kérés időtúllépés miatt megszakadt.", ex);
            }
            catch (JsonException ex)
            {
                throw new ApiException("Nem sikerült a válasz JSON feldolgozása.", ex);
            }
        }

        // ─────────────────────────────────────────
        // Publikus végpontok
        // ─────────────────────────────────────────

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                await GetCategoriesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<CategoryDto>> GetCategoriesAsync()
        {
            // FONTOS: a pontos URL-t Postman-ben kell tesztelni!
            // A dokumentáció CategoriesFindAll() metódust említ, 
            // ami a REST API-ban valószínűleg "categories" lesz.
            string url = BuildUrl("categories");
            return await GetAsync<List<CategoryDto>>(url);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
    
}
