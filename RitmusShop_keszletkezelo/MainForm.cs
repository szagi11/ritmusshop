using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using RitmusShop_keszletkezelo.Services;
using RitmusShop_keszletkezelo.ViewModels;

namespace RitmusShop_keszletkezelo
{
    public partial class MainForm : Form
    {
        private readonly HotcakesApiService _service;

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
                MessageBox.Show("Hiányzó konfiguráció!",
                    "Konfigurációs hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }

            _service = new HotcakesApiService(baseUrl, apiKey);

            txtSearch.TextChanged += TxtSearch_TextChanged;
            btnBulkApply.Click += BtnBulkApply_Click;
            btnSelectAll.Click += BtnSelectAll_Click;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _service?.Dispose();
            base.OnFormClosed(e);
        }

        // -----------------------------------------------------------------
        // KATEGÓRIÁK BETÖLTÉSE
        // -----------------------------------------------------------------

        private async void MainForm_Load(object? sender, EventArgs e)
        {
            try
            {
                var categories = await _service.GetCategoriesAsync();

                foreach (var cat in categories
                    .Where(c => string.IsNullOrEmpty(c.ParentId))
                    .OrderBy(c => c.SortOrder))
                {
                    var btnCat = new Button
                    {
                        Text = cat.Name,
                        Tag = cat.Bvin,
                        Width = flpCategories.Width - 10,
                        Height = 50
                    };
                    btnCat.Click += CategoryButton_Click;
                    flpCategories.Controls.Add(btnCat);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kategóriák betöltése sikertelen:\n{ex.Message}",
                    "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void CategoryButton_Click(object? sender, EventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not string categoryId) return;

            Cursor = Cursors.WaitCursor;
            flpProducts.Controls.Clear();
            txtSearch.Text = string.Empty;

            try
            {
                var page = await _service.GetProductsForCategoryAsync(categoryId);

                if (page?.Products == null || page.Products.Count == 0)
                {
                    var lbl = new Label { Text = "Ebben a kategóriában nincs termék.", AutoSize = true };
                    flpProducts.Controls.Add(lbl);
                    UpdateSelectionCounter();
                    return;
                }

                var fetchTasks = page.Products.Select(async product =>
                {
                    var variantsTask = _service.GetVariantsForProductAsync(product.Bvin);
                    var inventoryTask = _service.GetInventoryForProductAsync(product.Bvin);
                    var optionsTask = _service.GetOptionsForProductAsync(product.Bvin);
                    await Task.WhenAll(variantsTask, inventoryTask, optionsTask);

                    return InventoryItemViewModel.Build(
                        product, variantsTask.Result, inventoryTask.Result, optionsTask.Result);
                });

                var viewModels = await Task.WhenAll(fetchTasks);

                flpProducts.SuspendLayout();
                foreach (var vm in viewModels)
                {
                    var item = new ProductListItem { Width = flpProducts.Width - 25 };
                    item.Setup(_service, vm);
                    item.SelectionChanged += (s, ev) => UpdateSelectionCounter();
                    flpProducts.Controls.Add(item);
                }
                flpProducts.ResumeLayout();
                UpdateSelectionCounter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba a termékek lekérésekor:\n{ex.Message}",
                    "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        // -----------------------------------------------------------------
        // KERESÕ
        // -----------------------------------------------------------------

        private void TxtSearch_TextChanged(object? sender, EventArgs e)
        {
            var query = txtSearch.Text.Trim();
            flpProducts.SuspendLayout();
            try
            {
                foreach (var ctrl in flpProducts.Controls.OfType<ProductListItem>())
                    ctrl.Visible = ctrl.MatchesFilter(query);
            }
            finally
            {
                flpProducts.ResumeLayout();
            }
        }

        // -----------------------------------------------------------------
        // KIJELÖLÉSI SZÁMLÁLÓ
        // -----------------------------------------------------------------

        private void UpdateSelectionCounter()
        {
            int total = flpProducts.Controls.OfType<ProductListItem>()
                .Sum(p => p.CountSelected());
            lblBulkInfo.Text = $"Kijelölt: {total} sor";
        }

        // -----------------------------------------------------------------
        // MIND KIJELÖLÉSE / TÖRLÉSE — minden látható termékre
        // -----------------------------------------------------------------

        private void BtnSelectAll_Click(object? sender, EventArgs e)
        {
            var visibleItems = flpProducts.Controls.OfType<ProductListItem>()
                .Where(i => i.Visible).ToList();

            if (visibleItems.Count == 0) return;

            bool anySelected = visibleItems.Any(i => i.HasAnySelection());

            foreach (var item in visibleItems)
            {
                if (anySelected) item.ClearAllSelections();
                else item.SelectAll();
            }

            UpdateSelectionCounter();
        }

        // -----------------------------------------------------------------
        // TÖMEGES MÓDOSÍTÁS — csak a kijelölt sorokra
        // -----------------------------------------------------------------

        private async void BtnBulkApply_Click(object? sender, EventArgs e)
        {
            if (!int.TryParse(txtBulkDelta.Text, out int delta) || delta == 0)
            {
                MessageBox.Show("Adj meg egy nem nulla egész számot.",
                    "Érvénytelen érték", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var items = flpProducts.Controls.OfType<ProductListItem>()
                .Where(i => i.HasAnySelection()).ToList();

            int totalSelected = items.Sum(i => i.CountSelected());
            if (totalSelected == 0)
            {
                MessageBox.Show("Nincs kijelölt sor.", "Tömeges módosítás",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"Biztosan módosítod {totalSelected} kijelölt sor készletét {delta:+#;-#;0}-val/vel?",
                "Megerõsítés", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            try
            {
                btnBulkApply.Enabled = false;
                Cursor = Cursors.WaitCursor;

                var tasks = items.Select(item => item.ApplySelectedAsync(delta));
                var results = await Task.WhenAll(tasks);

                int success = results.Sum(r => r.SuccessCount);
                int failed = results.Sum(r => r.FailedCount);

                MessageBox.Show($"Kész. Sikeres: {success}, sikertelen: {failed}.",
                    "Tömeges módosítás", MessageBoxButtons.OK,
                    failed == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

                txtBulkDelta.Text = "0";
            }
            finally
            {
                btnBulkApply.Enabled = true;
                Cursor = Cursors.Default;
            }
        }
    }
}