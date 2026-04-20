using Microsoft.Extensions.Configuration;
using RitmusShop_keszletkezelo.Api;

namespace RitmusShop_keszletkezelo
{
    public partial class MainForm : Form
    {
        private readonly HotcakesApiClient _apiClient;

        public MainForm()
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

        private async void btnLoadCategories_Click(object sender, EventArgs e)
        {
        }


        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _apiClient?.Dispose();
            base.OnFormClosed(e);
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                var categories = await _apiClient.GetCategoriesAsync();

                foreach (var cat in categories
                    .Where(c => string.IsNullOrEmpty(c.ParentId))
                    .OrderBy(c => c.SortOrder))
                {
                    Button btnCat = new Button();
                    btnCat.Text = cat.Name;
                    btnCat.Tag = cat.Bvin; // A Tag-ben tároljuk az ID-t, ez késõbb nagyon fontos lesz!

                    // Alapméretek (ezt késõbb lehet finomítani)
                    btnCat.Width = flpCategories.Width - 10;
                    btnCat.Height = 50;

                    btnCat.Click += CategoryButton_Click;
                    // Hozzáadjuk a panelhez
                    flpCategories.Controls.Add(btnCat);
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
        }

        private async void CategoryButton_Click(object sender, EventArgs e)
        {
            Button clickedBtn = sender as Button;
            if (clickedBtn == null) return;

            string categoryId = clickedBtn.Tag.ToString();

            // Homokóra kurzor, amíg tölt
            Cursor = Cursors.WaitCursor;

            try
            {
                flpProducts.Controls.Clear();


                var response = await _apiClient.GetProductsForCategoryAsync(categoryId, 1, 100);

                if (response != null && response.Products != null)
                {
                    foreach (var prod in response.Products)
                    {

                        ProductListItem item = new ProductListItem();

                        item.Setup(prod.Bvin, prod.ProductName, prod.Sku);

                        item.Width = flpProducts.Width - 25;

                        flpProducts.Controls.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba a termékek lekérésekor:\n{ex.Message}");
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
    }
}