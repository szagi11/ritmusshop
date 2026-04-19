using Microsoft.Extensions.Configuration;
using RitmusShop_keszletkezelo.Api;
using RitmusShop_keszletkezelo.DTO;

namespace RitmusShop_keszletkezelo
{
    public partial class Form1 : Form
    {
        private readonly HotcakesApiClient _apiClient;

        public Form1()
        {
            InitializeComponent();

            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var baseUrl = config["HotcakesApi:BaseUrl"];
            var apiKey = config["HotcakesApi:ApiKey"];

            if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
            {
                MessageBox.Show("Hiányzó konfiguráció! Ellenõrizd az appsettings.json-t.",
                    "Konfigurációs hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }

            _apiClient = new HotcakesApiClient(baseUrl, apiKey);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private async void btnLoadCategories_Click(object sender, EventArgs e)
        {
            btnLoadCategories.Enabled = false;
            lstCategories.Items.Clear();

            try
            {
                var categories = await _apiClient.GetCategoriesAsync();

                foreach (var cat in categories
                    .Where(c => string.IsNullOrEmpty(c.ParentId))
                    .OrderBy(c => c.SortOrder))
                {
                    lstCategories.Items.Add($"{cat.Name} ({cat.Bvin})");
                }
            }
            catch (ApiException ex)
            {
                MessageBox.Show($"API hiba:\n{ex.Message}", "Hiba",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Váratlan hiba:\n{ex.Message}", "Hiba",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnLoadCategories.Enabled = true;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _apiClient?.Dispose();
            base.OnFormClosed(e);
        }
    }
}