using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Hotcakes.CommerceDTO.v1.Catalog;
using RitmusShop_keszletkezelo.Services;
using RitmusShop_keszletkezelo.ViewModels;

namespace RitmusShop_keszletkezelo
{
    public partial class ProductListItem : UserControl
    {
        private const int CardCollapsedHeight = 145;
        private const int VariantRowHeight = 36;
        private const int VariantHeaderHeight = 28;
        private const int VariantPanelTopPadding = 5;


        private IHotcakesApiService _service = null!;
        private InventoryItemViewModel _vm = null!;
        private List<CategorySnapshotDTO> _allCategories = new();
        private bool _variantsLoaded;
        private bool _optionsLoaded;
        private bool _categoryLabelLoaded;
        private bool _suppressCheckEvent;

        public event EventHandler? SelectionChanged;
        public event EventHandler? ExpandRequested;
        public event EventHandler? CategoryLoaded;

        public bool IsExpanded => pnlVariants.Visible && pnlVariants.Height > 10;

        public ProductListItem()
        {
            InitializeComponent();
            ApplyCardStyling();
            pnlVariants.Resize += PnlVariants_Resize;
            this.Height = CardCollapsedHeight;
            btnExpand.Text = "Méretek ▾";

            // A kártya checkboxa ThreeState = true (mert programból megjeleníti
            // a részleges kijelölést — Indeterminate, amikor csak némelyik
            // variáns van bepipálva). De a FELHASZNÁLÓI kattintás csak
            // Checked ↔ Unchecked között válthat, különben kétszer kellene
            // kattintani a kipipálás megszüntetéséhez (mert egy köztes
            // Indeterminate állapoton kellene áthaladni).
            chkSelect.AutoCheck = false;
            chkSelect.Click += ChkSelect_Click;
            chkSelect.CheckedChanged += ChkSelect_CheckedChanged;
        }

        /// <summary>
        /// A felhasználói kattintást „kétállapotúvá" simítjuk:
        ///   - Checked       → Unchecked
        ///   - Unchecked     → Checked
        ///   - Indeterminate → Checked (a részleges kijelölés „mindet kijelöl"-re vált)
        /// </summary>
        private void ChkSelect_Click(object? sender, EventArgs e)
        {
            chkSelect.CheckState = chkSelect.CheckState == CheckState.Checked
                ? CheckState.Unchecked
                : CheckState.Checked;
        }

        private void PnlVariants_Resize(object? sender, EventArgs e)
        {
            if (!_variantsLoaded) return;

            var rowWidth = CalcVariantRowWidth();
            var rowX = CalcVariantRowX(rowWidth);

            foreach (Control ctrl in pnlVariants.Controls)
            {
                if (ctrl is VariantListItem || ctrl.Name == "variantHeader")
                {
                    ctrl.Width = rowWidth;
                    ctrl.Left = rowX;
                }
            }
        }

        private int CalcVariantRowWidth()
        {
            var available = pnlVariants.Width - 10;
            if (available <= 0) return 0;

            return Math.Min(370, available);
        }

        private int CalcVariantRowX(int rowWidth) =>
            rowWidth <= 0 ? 0 : Math.Max(0, (pnlVariants.Width - rowWidth) / 2);

        private void ApplyCardStyling()
        {
            this.BackColor = UiTheme.CardBackground;
            this.Margin = new Padding(0, 0, 0, 8);
            pnlVariants.BackColor = UiTheme.CardBackground;

            this.Paint += (s, e) =>
            {
                bool isSelected = _vm != null && HasAnySelection();
                var rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);

                using var pen = new Pen(isSelected ? UiTheme.Accent : UiTheme.CardBorder, isSelected ? 2 : 1);
                e.Graphics.DrawRectangle(pen, rect);
            };

            lblProductName.Font = UiTheme.SubheadingFont;
            lblProductName.ForeColor = UiTheme.TextPrimary;

            lblSku.Font = UiTheme.BodyFont;
            lblSku.ForeColor = UiTheme.TextSecondary;

            lblCategory.Font = UiTheme.BodyFont;
            lblCategory.ForeColor = UiTheme.TextSecondary;

            lblStockLabel.Font = UiTheme.BodyFont;
            lblStockLabel.ForeColor = UiTheme.TextSecondary;

            lblCurrentStock.Font = UiTheme.BodyFont;
            lblCurrentStock.ForeColor = UiTheme.TextPrimary;

            btnExpand.FlatStyle = FlatStyle.Flat;
            btnExpand.BackColor = UiTheme.CardBackground;
            btnExpand.ForeColor = UiTheme.TextPrimary;
            btnExpand.FlatAppearance.BorderColor = UiTheme.CardBorder;
            btnExpand.Font = UiTheme.ButtonFont;
        }

        public void Setup(
            IHotcakesApiService service,
            InventoryItemViewModel vm,
            List<CategorySnapshotDTO> allCategories)
        {
            _service = service;
            _vm = vm;
            _allCategories = allCategories ?? new List<CategorySnapshotDTO>();

            lblProductName.Text = vm.ProductName;
            lblSku.Text = vm.Sku;
            lblCategory.Text = string.IsNullOrEmpty(vm.CategoryDisplay) ? "" : $"Kategória: {vm.CategoryDisplay}";
            RefreshStockLabel();

            btnExpand.Visible = vm.HasVariants;
            btnExpand.Text = "Méretek ▾";

            UpdateProductCheckboxState();
            UpdateBackgroundForSelection();
        }

        /// <summary>
        /// Egysoros készlet-összegzés a kártya tetején:
        ///   "50  •  Foglalt: 20  •  Eladható: 30"
        /// </summary>
        private void RefreshStockLabel()
        {
            lblCurrentStock.Text =
                $"{_vm.TotalQuantityOnHand}" +
                $"  •  Foglalt: {_vm.TotalQuantityReserved}" +
                $"  •  Eladható: {_vm.TotalAvailable}";
        }

        private void UpdateBackgroundForSelection()
        {
            this.BackColor = UiTheme.CardBackground;
            chkSelect.BackColor = UiTheme.CardBackground;
            lblProductName.BackColor = UiTheme.CardBackground;
            lblSku.BackColor = UiTheme.CardBackground;
            lblCategory.BackColor = UiTheme.CardBackground;
            lblStockLabel.BackColor = UiTheme.CardBackground;
            lblCurrentStock.BackColor = UiTheme.CardBackground;
            btnExpand.BackColor = UiTheme.CardBackground;

            this.Invalidate();
        }

        public IEnumerable<ProductInventoryDTO> GetSelectedInventories()
            => _vm.GetSelectedInventories();

        public bool HasAnySelection() => _vm.HasAnySelection();

        public int CountSelected() => _vm.CountSelected();

        public bool MatchesFilter(string query, string typeFilter = "Mind")
        {
            if (_vm == null) return true;

            // Típus-szűrő: csak akkor szűkítünk, ha specifikus típus van
            // kiválasztva ÉS a kategórianév már be is töltődött. Ezzel
            // elkerüljük, hogy a háttér-kategóriabetöltés alatt a kártya
            // tévesen eltűnjön a szűrőből.
            if (!string.IsNullOrEmpty(typeFilter)
                && !string.Equals(typeFilter, "Mind", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrEmpty(_vm.CategoryDisplay))
            {
                if (!string.Equals(_vm.CategoryDisplay, typeFilter,
                        StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return _vm.MatchesFilter(query);
        }

        /// <summary>
        /// Az aktuálisan tárolt kategórianév — a típus-szűrő ehhez hasonlít.
        /// </summary>
        public string CategoryDisplay => _vm?.CategoryDisplay ?? string.Empty;

        private void ChkSelect_CheckedChanged(object? sender, EventArgs e)
        {
            if (_suppressCheckEvent) return;
            bool selected = chkSelect.CheckState == CheckState.Checked;

            if (_vm.HasVariants)
            {
                foreach (var variant in _vm.Variants)
                    variant.IsSelected = selected;

                if (_variantsLoaded)
                {
                    foreach (var row in pnlVariants.Controls.OfType<VariantListItem>())
                        row.SetSelectedSilently(selected);
                }
            }
            else
            {
                _vm.IsSelected = selected;
            }

            UpdateBackgroundForSelection();
            this.Invalidate();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateProductCheckboxState()
        {
            _suppressCheckEvent = true;
            try
            {
                if (!_vm.HasVariants)
                {
                    chkSelect.CheckState = _vm.IsSelected ? CheckState.Checked : CheckState.Unchecked;
                }
                else
                {
                    int total = _vm.Variants.Count;
                    int selected = _vm.Variants.Count(v => v.IsSelected);
                    chkSelect.CheckState = selected switch
                    {
                        0 => CheckState.Unchecked,
                        var n when n == total => CheckState.Checked,
                        _ => CheckState.Indeterminate
                    };
                }
            }
            finally
            {
                _suppressCheckEvent = false;
            }
            UpdateBackgroundForSelection();
            this.Invalidate();
        }

        private void OnVariantSelectionChanged(object? sender, EventArgs e)
        {
            UpdateProductCheckboxState();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        // -----------------------------------------------------------------
        // KIBONTÁS
        // -----------------------------------------------------------------

        private void btnExpand_Click(object? sender, EventArgs e)
        {
            ExpandRequested?.Invoke(this, EventArgs.Empty);
        }

        public async Task ToggleExpandedAsync()
        {
            if (IsExpanded) Collapse();
            else await ExpandAsync();
        }

        // Szinkron verzió megtartva — tesztekhez / külső hívókhoz.
        public void ToggleExpanded() => _ = ToggleExpandedAsync();

        public void Collapse()
        {
            pnlVariants.Visible = false;
            pnlVariants.Height = 5;
            this.Height = CardCollapsedHeight;
            btnExpand.Text = "Méretek ▾";
        }

        private async Task ExpandAsync()
        {
            // Csak az első kibontáskor töltjük le az opciókat (méretnevek).
            // Ez a halasztás az, ami a kategória-kattintás idejét megfelezi.
            if (!_optionsLoaded)
            {
                btnExpand.Enabled = false;
                try
                {
                    await EnsureOptionsLoadedAsync();
                }
                finally
                {
                    btnExpand.Enabled = true;
                }
            }

            if (!_variantsLoaded) PopulateVariantPanel();
            pnlVariants.Visible = true;
            int variantsHeight = VariantHeaderHeight + _vm.Variants.Count * VariantRowHeight + 8;
            pnlVariants.Height = variantsHeight;
            this.Height = CardCollapsedHeight + VariantPanelTopPadding + variantsHeight;
            btnExpand.Text = "Méretek ▴";
            this.Parent?.PerformLayout();
        }

        /// <summary>
        /// Lekéri a termék opcióit, és frissíti a variánsok DisplayName mezőit.
        /// Hibatűrő: ha a hívás megbukik, marad az SKU-fallback név.
        /// </summary>
        private async Task EnsureOptionsLoadedAsync()
        {
            if (_optionsLoaded) return;
            _optionsLoaded = true;

            try
            {
                var options = await _service.GetOptionsForProductAsync(_vm.ProductBvin);

                var labelLookup = options
                    .Where(o => o.Items != null)
                    .SelectMany(o => o.Items)
                    .Where(item => !item.IsLabel && !string.IsNullOrEmpty(item.Bvin))
                    .GroupBy(item => InventoryItemViewModel.NormalizeGuid(item.Bvin))
                    .ToDictionary(g => g.Key, g => g.First().Name ?? string.Empty);

                foreach (var vvm in _vm.Variants)
                {
                    if (vvm.Source != null)
                        vvm.DisplayName = InventoryItemViewModel
                            .BuildVariantDisplayName(vvm.Source, labelLookup);
                }
            }
            catch
            {
                // Az opciók csak kozmetikai célt szolgálnak — ha nem jönnek
                // meg, marad az SKU-fallback név. Csendben elnyelve.
            }
        }

        /// <summary>
        /// Háttérben fut a MainForm-ból; a „Kategória: …" feliratot pótolja
        /// a kártyán, miután a kártyák már megjelentek.
        /// </summary>
        public async Task LoadCategoryLabelAsync(CancellationToken ct)
        {
            if (_categoryLabelLoaded) return;
            _categoryLabelLoaded = true;

            try
            {
                var cats = await _service.GetCategoriesForProductAsync(_vm.ProductBvin)
                    .ConfigureAwait(false);
                if (ct.IsCancellationRequested) return;

                var display = InventoryItemViewModel
                    .ResolveCategoryDisplay(cats, _allCategories);
                _vm.CategoryDisplay = display;

                if (IsDisposed || !IsHandleCreated || ct.IsCancellationRequested) return;
                BeginInvoke(new Action(() =>
                {
                    if (IsDisposed) return;
                    lblCategory.Text = string.IsNullOrEmpty(display)
                        ? ""
                        : $"Kategória: {display}";
                    CategoryLoaded?.Invoke(this, EventArgs.Empty);
                }));
            }
            catch
            {
                // Csak kozmetikai felirat — ha nem jön, marad üresen.
            }
        }

        private void PopulateVariantPanel()
        {
            pnlVariants.SuspendLayout();
            pnlVariants.Controls.Clear();

            var rowWidth = CalcVariantRowWidth();
            var rowX = CalcVariantRowX(rowWidth);
            int y = 0;

            // FEJLÉC — csak egyszer, a panel tetején
            var header = BuildVariantHeader(rowWidth);
            header.Name = "variantHeader";
            header.Location = new Point(rowX, y);
            pnlVariants.Controls.Add(header);
            y += VariantHeaderHeight;

            // Adatsorok — csak a számokat tartalmazzák, igazítva a fejléchez
            foreach (var variant in _vm.Variants)
            {
                var row = new VariantListItem();
                row.Setup(variant);

                row.Location = new Point(rowX, y);
                row.Width = rowWidth;
                row.Anchor = AnchorStyles.Top;

                row.SelectionChanged += OnVariantSelectionChanged;
                pnlVariants.Controls.Add(row);
                y += VariantRowHeight;
            }

            pnlVariants.Height = y + 4;
            pnlVariants.ResumeLayout();
            _variantsLoaded = true;
        }

        /// <summary>
        /// Egyszer megjelenő fejlécsor a méret-panel tetején: "Méret | Készlet | Foglalt | Összes".
        /// Az x-koordinátáknak meg KELL egyezniük a VariantListItem.Designer.cs-ben
        /// definiált oszlop-koordinátákkal (lblOnHandValue, lblReservedValue, lblAvailableValue).
        /// </summary>
        private static Panel BuildVariantHeader(int width)
        {
            var headerFont = UiTheme.BodyFont;

            var p = new Panel
            {
                Width = width,
                Height = VariantHeaderHeight,
                BackColor = UiTheme.CardBackground
            };

            p.Controls.Add(new Label
            {
                Text = "Méret",
                Location = new Point(28, 4),
                Size = new Size(65, 20),
                Font = headerFont,
                ForeColor = UiTheme.TextSecondary,
                TextAlign = ContentAlignment.MiddleLeft
            });
            p.Controls.Add(new Label
            {
                Text = "Készlet",
                Location = new Point(95, 4),
                Size = new Size(85, 20),
                Font = headerFont,
                ForeColor = UiTheme.TextSecondary,
                TextAlign = ContentAlignment.MiddleCenter
            });
            p.Controls.Add(new Label
            {
                Text = "Foglalt",
                Location = new Point(185, 4),
                Size = new Size(85, 20),
                Font = headerFont,
                ForeColor = UiTheme.TextSecondary,
                TextAlign = ContentAlignment.MiddleCenter
            });
            p.Controls.Add(new Label
            {
                Text = "Összes",
                Location = new Point(275, 4),
                Size = new Size(85, 20),
                Font = headerFont,
                ForeColor = UiTheme.TextSecondary,
                TextAlign = ContentAlignment.MiddleCenter
            });

            // Vékony elválasztó a fejléc alatt
            p.Paint += (s, e) =>
            {
                using var pen = new Pen(UiTheme.CardBorder, 1);
                e.Graphics.DrawLine(pen, 0, p.Height - 1, p.Width, p.Height - 1);
            };

            return p;
        }

        // -----------------------------------------------------------------
        // BULK 
        // -----------------------------------------------------------------

        public async Task<BulkResult> ApplySelectedAsync(int delta)
        {
            var result = new BulkResult();
            var selectedInvs = GetSelectedInventories().ToList();

            foreach (var inv in selectedInvs)
            {
                if (await TryUpdateInventory(inv, delta)) result.SuccessCount++;
                else result.FailedCount++;
            }

            RefreshStockLabel();
            RefreshVariantRowLabels();
            return result;
        }

        public void ClearAllSelections()
        {
            _vm.IsSelected = false;
            foreach (var v in _vm.Variants) v.IsSelected = false;

            if (_variantsLoaded)
            {
                foreach (var row in pnlVariants.Controls.OfType<VariantListItem>())
                    row.SetSelectedSilently(false);
            }
            UpdateProductCheckboxState();
        }

        public void SelectAll()
        {
            if (_vm.HasVariants)
            {
                foreach (var v in _vm.Variants) v.IsSelected = true;
                if (_variantsLoaded)
                {
                    foreach (var row in pnlVariants.Controls.OfType<VariantListItem>())
                        row.SetSelectedSilently(true);
                }
            }
            else
            {
                _vm.IsSelected = true;
            }
            UpdateProductCheckboxState();
        }

        private async Task<bool> TryUpdateInventory(ProductInventoryDTO inv, int delta)
        {
            int oldQty = inv.QuantityOnHand;
            int newQty = oldQty + delta;
            if (newQty < 0) return false;

            try
            {
                inv.QuantityOnHand = newQty;
                var updated = await _service.UpdateInventoryAsync(inv);
                if (updated != null) inv.QuantityOnHand = updated.QuantityOnHand;
                return true;
            }
            catch
            {
                inv.QuantityOnHand = oldQty;
                return false;
            }
        }

        private void RefreshVariantRowLabels()
        {
            foreach (var row in pnlVariants.Controls.OfType<VariantListItem>())
                row.RefreshDisplay();
        }

        public class BulkResult
        {
            public int SuccessCount { get; set; }
            public int FailedCount { get; set; }
        }
    }
}