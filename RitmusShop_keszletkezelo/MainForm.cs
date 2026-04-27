using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Hotcakes.CommerceDTO.v1.Catalog;
using Microsoft.Extensions.Configuration;
using RitmusShop_keszletkezelo.Services;
using RitmusShop_keszletkezelo.ViewModels;

namespace RitmusShop_keszletkezelo
{
    public partial class MainForm : Form
    {
        private readonly HotcakesApiService _service;
        private readonly HttpClient _httpClient;
        private Button? _activeCategoryButton;

        private List<CategorySnapshotDTO> _allCategories = new();
        private CategorySnapshotDTO? _currentParentCategory;

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
                MessageBox.Show("Hiányzó konfiguráció! Ellenőrizd az appsettings.json-t.",
                    "Konfigurációs hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }


            if (!baseUrl.EndsWith("/")) baseUrl += "/";

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
            _service = new HotcakesApiService(_httpClient, apiKey);

            flpProducts.Resize += (s, e) => ResizeAllCards();
            txtSearch.TextChanged += TxtSearch_TextChanged;
            btnBulkApply.Click += BtnBulkApply_Click;
            btnSelectAll.Click += BtnSelectAll_Click;
            cmbSubcategory.SelectedIndexChanged += CmbSubcategory_SelectedIndexChanged;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _service?.Dispose();
            _httpClient?.Dispose();
            base.OnFormClosed(e);
        }

        // -----------------------------------------------------------------
        // KATEG�RI�K BET�LT�SE (indul�skor egyszer)
        // -----------------------------------------------------------------

        private async void MainForm_Load(object? sender, EventArgs e)
        {
            try
            {
                _allCategories = await _service.GetCategoriesAsync();

                foreach (var cat in _allCategories
                    .Where(c => string.IsNullOrEmpty(c.ParentId))
                    .OrderBy(c => c.SortOrder))
                {
                    var btnCat = new Button
                    {
                        Text = cat.Name,
                        Tag = cat,
                        Width = 250,
                        Height = 45,
                        Font = UiTheme.BodyFont,
                        FlatStyle = FlatStyle.Flat,
                        BackColor = UiTheme.CardBackground,
                        ForeColor = UiTheme.TextPrimary,
                        TextAlign = ContentAlignment.MiddleLeft,
                        Padding = new Padding(15, 0, 0, 0),
                        Margin = new Padding(0, 3, 0, 3),
                        Cursor = Cursors.Hand,
                        AutoEllipsis = true
                    };
                    btnCat.FlatAppearance.BorderColor = UiTheme.CardBorder;
                    btnCat.FlatAppearance.BorderSize = 1;
                    btnCat.FlatAppearance.MouseOverBackColor = UiTheme.AccentLight;
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
            if (sender is not Button btn || btn.Tag is not CategorySnapshotDTO category) return;

            // Akt�v kateg�ria vizu�lis kiemel�se
            if (_activeCategoryButton != null)
            {
                _activeCategoryButton.BackColor = UiTheme.CardBackground;
                _activeCategoryButton.ForeColor = UiTheme.TextPrimary;
                _activeCategoryButton.FlatAppearance.BorderColor = UiTheme.CardBorder;
                _activeCategoryButton.Font = UiTheme.BodyFont;
            }
            btn.BackColor = UiTheme.AccentLight;
            btn.ForeColor = UiTheme.Accent;
            btn.FlatAppearance.BorderColor = UiTheme.Accent;
            btn.Font = new Font(UiTheme.BodyFont, FontStyle.Bold);
            _activeCategoryButton = btn;

            _currentParentCategory = category;

            PopulateSubcategoryDropdown(category);
            await LoadProductsForCategoryAsync(category.Bvin ?? string.Empty);
        }

        // -----------------------------------------------------------------
        // ALKATEG�RIA COMBOBOX
        // -----------------------------------------------------------------

        private void PopulateSubcategoryDropdown(CategorySnapshotDTO parent)
        {
            cmbSubcategory.SelectedIndexChanged -= CmbSubcategory_SelectedIndexChanged;
            cmbSubcategory.Items.Clear();

            // Els� elem: "�sszes alkateg�ria"
            cmbSubcategory.Items.Add(new SubcategoryItem
            {
                DisplayText = "Összes alkategória",
                CategoryBvin = parent.Bvin ?? string.Empty
            });

            // A kiv�lasztott parent al� tartoz� alkateg�ri�k
            var subs = _allCategories
                .Where(c => c.ParentId == parent.Bvin)
                .OrderBy(c => c.SortOrder)
                .ToList();

            foreach (var sub in subs)
            {
                cmbSubcategory.Items.Add(new SubcategoryItem
                {
                    DisplayText = sub.Name ?? string.Empty,
                    CategoryBvin = sub.Bvin ?? string.Empty
                });
            }

            cmbSubcategory.SelectedIndex = 0;
            cmbSubcategory.SelectedIndexChanged += CmbSubcategory_SelectedIndexChanged;
        }

        private async void CmbSubcategory_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbSubcategory.SelectedItem is not SubcategoryItem selected) return;
            try
            {
                await LoadProductsForCategoryAsync(selected.CategoryBvin);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba az alkategória betöltésekor:\n{ex.Message}",
                    "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // -----------------------------------------------------------------
        // TERM�KEK BET�LT�SE
        // -----------------------------------------------------------------

        private int CalcCardWidth()
        {
            var padding = flpProducts.Padding.Horizontal;
            var scrollBar = flpProducts.VerticalScroll.Visible ? SystemInformation.VerticalScrollBarWidth : 0;
            var width = flpProducts.ClientSize.Width - padding - scrollBar;

            return Math.Max(200, width);
        }

        private async Task LoadProductsForCategoryAsync(string categoryBvin)
        {
            Cursor = Cursors.WaitCursor;
            flpProducts.Controls.Clear();
            txtSearch.Text = string.Empty;

            try
            {
                var page = await _service.GetProductsForCategoryAsync(categoryBvin);

                if (page?.Products == null || page.Products.Count == 0)
                {
                    var lbl = new Label
                    {
                        Text = "Ebben a kategóriában nincs term�k.",
                        AutoSize = true,
                        Font = UiTheme.BodyFont,
                        ForeColor = UiTheme.TextSecondary
                    };
                    flpProducts.Controls.Add(lbl);
                    UpdateSelectionCounter();
                    return;
                }

                var fetchTasks = page.Products.Select(async product =>
                {
                    var variantsTask = _service.GetVariantsForProductAsync(product.Bvin);
                    var inventoryTask = _service.GetInventoryForProductAsync(product.Bvin);
                    var optionsTask = SafeGetOptionsAsync(product.Bvin);
                    var catsTask = SafeGetCategoriesForProductAsync(product.Bvin);
                    await Task.WhenAll(variantsTask, inventoryTask, optionsTask, catsTask);

                    return InventoryItemViewModel.Build(
                        product,
                        variantsTask.Result,
                        inventoryTask.Result,
                        optionsTask.Result,
                        catsTask.Result,
                        _allCategories);
                });

                var viewModels = await Task.WhenAll(fetchTasks);

                flpProducts.SuspendLayout();
                foreach (var vm in viewModels)
                {
                    var item = new ProductListItem();
                    item.Setup(_service, vm);
                    item.Width = CalcCardWidth();
                    item.SelectionChanged += (s, ev) => UpdateSelectionCounter();
                    item.ExpandRequested += ProductItem_ExpandRequested;
                    flpProducts.Controls.Add(item);
                }
                flpProducts.ResumeLayout();

                ResizeAllCards();
                UpdateSelectionCounter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba a termékek lekérdezésekor:\n{ex.Message}",
                    "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private async Task<List<OptionDTO>> SafeGetOptionsAsync(string productBvin)
        {
            try { return await _service.GetOptionsForProductAsync(productBvin); }
            catch { return new List<OptionDTO>(); }
        }

        private async Task<List<CategorySnapshotDTO>> SafeGetCategoriesForProductAsync(string productBvin)
        {
            try { return await _service.GetCategoriesForProductAsync(productBvin); }
            catch { return new List<CategorySnapshotDTO>(); }
        }

        // -----------------------------------------------------------------
        // KIBONT�S � csak egy k�rtya egyszerre nyitva
        // -----------------------------------------------------------------

        private void ProductItem_ExpandRequested(object? sender, EventArgs e)
        {
            if (sender is not ProductListItem opener) return;

            foreach (var other in flpProducts.Controls.OfType<ProductListItem>())
            {
                if (other != opener && other.IsExpanded)
                    other.Collapse();
            }
            opener.ToggleExpanded();

            flpProducts.PerformLayout();
            ResizeAllCards();
        }

        // -----------------------------------------------------------------
        // K�RTYA SZ�LESS�G �JRASZ�M�T�S
        // -----------------------------------------------------------------

        private void ResizeAllCards()
        {
            int newWidth = CalcCardWidth();
            if (newWidth < 200) return;

            flpProducts.SuspendLayout();
            try
            {
                foreach (var item in flpProducts.Controls.OfType<ProductListItem>())
                {
                    if (item.Width != newWidth)
                        item.Width = newWidth;
                }
            }
            finally
            {
                flpProducts.ResumeLayout();
            }
        }

        // -----------------------------------------------------------------
        // KERES�
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
        // KIJEL�L�SI SZ�ML�L�
        // -----------------------------------------------------------------

        private void UpdateSelectionCounter()
        {
            int total = flpProducts.Controls.OfType<ProductListItem>()
                .Sum(p => p.CountSelected());
            lblBulkInfo.Text = $"Kijelö" +
                $"lt: {total} sor";
        }

        // -----------------------------------------------------------------
        // MIND KIJEL�L�SE / T�RL�SE
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
        // T�MEGES M�DOS�T�S
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
                "Megerősítés", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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

        // -----------------------------------------------------------------
        // SEGÉD TÍPUS � alkateg�ria ComboBox elem
        // -----------------------------------------------------------------

        private class SubcategoryItem
        {
            public string DisplayText { get; set; } = string.Empty;
            public string CategoryBvin { get; set; } = string.Empty;
            public override string ToString() => DisplayText;
        }

        private void flpProducts_SizeChanged(object sender, EventArgs e)
        {
            flpProducts.SuspendLayout();

            foreach (Control ctrl in flpProducts.Controls)
            {

                ctrl.Width = flpProducts.ClientSize.Width - ctrl.Margin.Left - ctrl.Margin.Right;
            }

            flpProducts.ResumeLayout();
        }
    }
}