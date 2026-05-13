using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Threading;
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

        /// <summary>
        /// A háttér-kategórialekérdezéseket szakítja meg, amikor a felhasználó
        /// másik kategóriára vált (különben régi kártyákra próbálnánk írni).
        /// </summary>
        private CancellationTokenSource? _backgroundCts;

        /// <summary>
        /// Form-élettartamra szóló CTS: a kezdeti cache-előmelegítést
        /// (termékek + variánsok + inventory előtöltése csendben) ezzel
        /// szakítjuk meg, amikor a form bezárul.
        /// </summary>
        private CancellationTokenSource? _warmupCts;

        private const string TypeFilterAll = "Mind";
        private string _currentTypeFilter = TypeFilterAll;

        public MainForm()
        {
            InitializeComponent();

            string? baseUrl = null;
            string? apiKey = null;

            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false)
                    .Build();

                baseUrl = config["HotcakesApi:BaseUrl"];
                apiKey = config["HotcakesApi:ApiKey"];


                if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
                {
                    throw new Exception("A BaseUrl vagy az ApiKey hiányzik a konfigurációból.");
                }
            }
            catch (Exception)
            {

                string hibaUzenet = "Ellenőrizze, hogy az appsettings.json létezik-e, ellenőrizze az URL helyességét és az API kulcsot is. Ha az appsettings.json nem létezik akkor az appsettings.example.json-t másolja le nevezze át appsettings.json-re majd írja be a megfelelő URL-t és API kulcsot és mentse el";

                MessageBox.Show(hibaUzenet, "Konfigurációs hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            cmbTypeFilter.SelectedIndexChanged += CmbTypeFilter_SelectedIndexChanged;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // Háttérfeladatok rendezett leállítása
            _warmupCts?.Cancel();
            _warmupCts?.Dispose();
            _backgroundCts?.Cancel();
            _backgroundCts?.Dispose();

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

                ResetTypeFilterToDefault();

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

                // Csendes háttér-előmelegítés: végigjárjuk az összes kategóriát
                // és előtöltjük a termékek + variánsok + inventory adatait a
                // szolgáltatás cache-ébe. Amikor a felhasználó rákattint egy
                // kategóriára, a kártyák cache-ből épülnek — gyakorlatilag
                // azonnal megjelennek.
                _warmupCts = new CancellationTokenSource();
                _ = WarmUpCacheInBackgroundAsync(_warmupCts.Token);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kategóriák betöltése sikertelen:\n{ex.Message}",
                    "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Form indulásakor lefutó háttérfeladat. Lassan, a szervert kímélve
        /// előtölti az összes kategória termékadatait a service cache-ébe.
        /// A felhasználói kattintásokat NEM blokkolja — ha a felhasználó
        /// közben rákattint egy kategóriára, az ugyanazokat a cache-bejegyzéseket
        /// használja (Lazy&lt;Task&lt;T&gt;&gt;), így nincs dupla hálózati hívás.
        /// </summary>
        private async Task WarmUpCacheInBackgroundAsync(CancellationToken ct)
        {
            try
            {
                // Rövid várakozás, hogy a form előbb rendeződjön és a
                // felhasználói első kattintás (ha van) elsőbbséget kapjon.
                await Task.Delay(1500, ct).ConfigureAwait(false);

                // Egyszerre maximum 2 termék variant+inventory párhuzamosan.
                // A DNN/IIS app pool ezt simán elviseli, és nem akadályozza
                // a felhasználói kattintás kéréseit.
                using var sem = new SemaphoreSlim(2, 2);

                foreach (var cat in _allCategories)
                {
                    if (ct.IsCancellationRequested) return;
                    if (string.IsNullOrEmpty(cat.Bvin)) continue;

                    try
                    {
                        // 1) Kategória termékei (cache-be kerül)
                        var page = await _service
                            .GetProductsForCategoryAsync(cat.Bvin)
                            .ConfigureAwait(false);

                        if (page?.Products == null || page.Products.Count == 0)
                            continue;

                        // 2) Minden termékre variants + inventory (cache-be)
                        var tasks = page.Products.Select(async product =>
                        {
                            if (ct.IsCancellationRequested) return;
                            await sem.WaitAsync(ct).ConfigureAwait(false);
                            try
                            {
                                if (ct.IsCancellationRequested) return;
                                var vt = _service.GetVariantsForProductAsync(product.Bvin);
                                var it = _service.GetInventoryForProductAsync(product.Bvin);
                                await Task.WhenAll(vt, it).ConfigureAwait(false);
                            }
                            catch
                            {
                                // Háttér-előtöltés hibái némán elnyelődnek —
                                // a felhasználói klikk újrapróbálkozik (a
                                // service hibás eredményt nem cache-el).
                            }
                            finally
                            {
                                sem.Release();
                            }
                        });

                        await Task.WhenAll(tasks).ConfigureAwait(false);

                        // Kis szünet kategóriák között, hogy a szervernek
                        // ne legyen állandó terhelés.
                        await Task.Delay(300, ct).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) { return; }
                    catch
                    {
                        // Egyetlen kategória hibája nem akasztja meg a többit.
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Normális leállás form-zárásnál.
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
            PopulateTypeFilterForCategory(category);
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

            // Régi háttér-feladatok leállítása (a felhasználó váltott kategóriát)
            _backgroundCts?.Cancel();
            _backgroundCts?.Dispose();
            _backgroundCts = new CancellationTokenSource();
            var ct = _backgroundCts.Token;

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

                // Csak a bulk-művelethez ELENGEDHETETLEN adatokat töltjük be:
                //   - variánsok  -> a bulk select tudja, mit válasszon ki
                //   - inventory  -> a bulk apply ezeket az objektumokat módosítja
                // A többi (opciók = méretnevek, kategória-felirat) később, halasztva.
                var fetchTasks = page.Products.Select(async product =>
                {
                    var variantsTask = _service.GetVariantsForProductAsync(product.Bvin);
                    var inventoryTask = _service.GetInventoryForProductAsync(product.Bvin);
                    await Task.WhenAll(variantsTask, inventoryTask);

                    return InventoryItemViewModel.Build(
                        product,
                        variantsTask.Result,
                        inventoryTask.Result,
                        new List<OptionDTO>(),            // halasztott: kibontáskor
                        new List<CategorySnapshotDTO>(),  // halasztott: háttérben
                        _allCategories);
                });

                var viewModels = await Task.WhenAll(fetchTasks);

                flpProducts.SuspendLayout();
                foreach (var vm in viewModels)
                {
                    var item = new ProductListItem();
                    item.Setup(_service, vm, _allCategories);
                    item.Width = CalcCardWidth();
                    item.SelectionChanged += (s, ev) => UpdateSelectionCounter();
                    item.ExpandRequested += ProductItem_ExpandRequested;
                    item.CategoryLoaded += (s, ev) => ApplyAllFilters();
                    flpProducts.Controls.Add(item);
                }
                flpProducts.ResumeLayout();

                // A friss kártyákra is alkalmazzuk a már beállított típus-szűrőt
                ApplyAllFilters();

                // Háttérben pótoljuk a "Kategória: X" feliratokat — fojtott
                // párhuzamossággal, hogy a szerver app pool ne fulladjon meg.
                _ = FillCategoryLabelsInBackgroundAsync(ct);
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
        // HÁTTÉR-FELADATOK
        // -----------------------------------------------------------------

        /// <summary>
        /// A kártyák már megjelentek; csendben pótoljuk minden termékhez a
        /// „Kategória: …" feliratot. Maximum 6 párhuzamos kéréssel, hogy a
        /// szerver app pool ne torlódjon be.
        /// </summary>
        private async Task FillCategoryLabelsInBackgroundAsync(CancellationToken ct)
        {
            var items = flpProducts.Controls.OfType<ProductListItem>().ToList();
            using var sem = new SemaphoreSlim(6, 6);

            var tasks = items.Select(async item =>
            {
                if (ct.IsCancellationRequested) return;
                await sem.WaitAsync(ct).ConfigureAwait(false);
                try
                {
                    if (ct.IsCancellationRequested) return;
                    await item.LoadCategoryLabelAsync(ct).ConfigureAwait(false);
                }
                finally
                {
                    sem.Release();
                }
            });

            try { await Task.WhenAll(tasks).ConfigureAwait(false); }
            catch (OperationCanceledException) { /* normális vált�skor */ }
        }

        // -----------------------------------------------------------------
        // KIBONT�S � csak egy k�rtya egyszerre nyitva
        // -----------------------------------------------------------------

        private async void ProductItem_ExpandRequested(object? sender, EventArgs e)
        {
            if (sender is not ProductListItem opener) return;

            foreach (var other in flpProducts.Controls.OfType<ProductListItem>())
            {
                if (other != opener && other.IsExpanded)
                    other.Collapse();
            }
            await opener.ToggleExpandedAsync();

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
            ApplyAllFilters();
        }

        // -----------------------------------------------------------------
        // TÍPUS-SZŰRŐ (Mind / Tánccipő / Táncruha / …)
        // -----------------------------------------------------------------

        /// <summary>
        /// Alaphelyzet a forma indulásakor — csak "Mind" van a szűrőben,
        /// amíg a felhasználó nem kattint főkategóriára.
        /// </summary>
        private void ResetTypeFilterToDefault()
        {
            SetTypeFilterItems(Array.Empty<string>());
        }

        /// <summary>
        /// Egy adott főkategóriára kattintáskor a szűrőbe csak ennek a
        /// főkategóriának a közvetlen gyermekei kerülnek be:
        ///   - "Verseny"  → Mind, Tánccipő, Táncruha
        ///   - "Amatőr"   → Mind, Tánccipő, Táncruha
        ///   - "Kiegészítők" → Mind  (nincs gyermek)
        /// </summary>
        private void PopulateTypeFilterForCategory(CategorySnapshotDTO parent)
        {
            var childNames = _allCategories
                .Where(c => !string.IsNullOrEmpty(c.ParentId)
                            && c.ParentId == parent.Bvin)
                .Select(c => c.Name ?? string.Empty)
                .Where(n => !string.IsNullOrEmpty(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(n => n, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            SetTypeFilterItems(childNames);
        }

        /// <summary>
        /// Egy közös segédfüggvény: a megadott neveket "Mind" után pakolja
        /// a szűrőbe, kikapcsolja az eseményt a feltöltés idejére, és a
        /// `_currentTypeFilter`-t alaphelyzetbe állítja.
        /// </summary>
        private void SetTypeFilterItems(IEnumerable<string> names)
        {
            cmbTypeFilter.SelectedIndexChanged -= CmbTypeFilter_SelectedIndexChanged;
            cmbTypeFilter.Items.Clear();
            cmbTypeFilter.Items.Add(TypeFilterAll);
            foreach (var name in names)
                if (!cmbTypeFilter.Items.Contains(name))
                    cmbTypeFilter.Items.Add(name);

            cmbTypeFilter.SelectedIndex = 0;
            _currentTypeFilter = TypeFilterAll;
            cmbTypeFilter.SelectedIndexChanged += CmbTypeFilter_SelectedIndexChanged;
        }

        private void CmbTypeFilter_SelectedIndexChanged(object? sender, EventArgs e)
        {
            _currentTypeFilter = cmbTypeFilter.SelectedItem?.ToString() ?? TypeFilterAll;
            ApplyAllFilters();
        }

        /// <summary>
        /// A keresőszöveget és a típus-szűrőt egyszerre alkalmazza minden
        /// jelenleg betöltött termékkártyára.
        /// </summary>
        private void ApplyAllFilters()
        {
            var query = txtSearch.Text.Trim();
            var typeFilter = _currentTypeFilter;

            flpProducts.SuspendLayout();
            try
            {
                foreach (var ctrl in flpProducts.Controls.OfType<ProductListItem>())
                    ctrl.Visible = ctrl.MatchesFilter(query, typeFilter);
            }
            finally
            {
                flpProducts.ResumeLayout();
            }

            UpdateSelectionCounter();
        }

        // -----------------------------------------------------------------
        // KIJEL�L�SI SZML�L�
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

                MessageBox.Show($"Sikeres: {success}, sikertelen: {failed}.",
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