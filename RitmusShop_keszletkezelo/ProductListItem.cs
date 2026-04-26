using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using RitmusShop_keszletkezelo.Services;
using RitmusShop_keszletkezelo.ViewModels;

namespace RitmusShop_keszletkezelo
{
    public partial class ProductListItem : UserControl
    {
        private const int CardCollapsedHeight = 145;
        private const int VariantRowHeight = 36;
        private const int VariantPanelTopPadding = 5;

        private HotcakesApiService _service = null!;
        private InventoryItemViewModel _vm = null!;
        private bool _variantsLoaded;
        private bool _suppressCheckEvent;

        public event EventHandler? SelectionChanged;
        public event EventHandler? ExpandRequested;

        public bool IsExpanded => pnlVariants.Visible && pnlVariants.Height > 10;

        public ProductListItem()
        {
            InitializeComponent();
            ApplyCardStyling();
            this.Height = CardCollapsedHeight;
            btnExpand.Text = "Méretek ▾";
            chkSelect.CheckedChanged += ChkSelect_CheckedChanged;
        }

        private void ApplyCardStyling()
        {
            this.BackColor = UiTheme.CardBackground;
            this.Margin = new Padding(0, 0, 0, 8);

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

            lblCurrentStock.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblCurrentStock.ForeColor = UiTheme.TextPrimary;

            btnExpand.FlatStyle = FlatStyle.Flat;
            btnExpand.BackColor = UiTheme.CardBackground;
            btnExpand.ForeColor = UiTheme.TextPrimary;
            btnExpand.FlatAppearance.BorderColor = UiTheme.CardBorder;
            btnExpand.Font = UiTheme.ButtonFont;
        }

        public void Setup(HotcakesApiService service, InventoryItemViewModel vm)
        {
            _service = service;
            _vm = vm;

            lblProductName.Text = vm.ProductName;
            lblSku.Text = vm.Sku;
            lblCategory.Text = string.IsNullOrEmpty(vm.CategoryDisplay) ? "" : $"Kategória: {vm.CategoryDisplay}";
            lblCurrentStock.Text = vm.TotalQuantityOnHand.ToString();

            btnExpand.Visible = vm.HasVariants;
            btnExpand.Text = "Méretek ▾";

            UpdateProductCheckboxState();
            UpdateBackgroundForSelection();
        }

        /// <summary>
        /// A kártya belső kontrolljainak hátterét beigazítja a kijelölési állapothoz,
        /// hogy az egész kártya egységesen színezett legyen, ne csak a háttér-rétege.
        /// </summary>
        private void UpdateBackgroundForSelection()
        {
            bool isSelected = _vm != null && HasAnySelection();
            var bg = isSelected ? UiTheme.AccentLight : UiTheme.CardBackground;

            this.BackColor = bg;
            chkSelect.BackColor = bg;
            lblProductName.BackColor = bg;
            lblSku.BackColor = bg;
            lblCategory.BackColor = bg;
            lblStockLabel.BackColor = bg;
            lblCurrentStock.BackColor = bg;
            btnExpand.BackColor = bg;
        }

        public IEnumerable<Hotcakes.CommerceDTO.v1.Catalog.ProductInventoryDTO> GetSelectedInventories()
             => _vm.GetSelectedInventories();

        public bool HasAnySelection() => _vm.HasAnySelection();

        public int CountSelected() => _vm.CountSelected();

        public bool MatchesFilter(string query)
        {
            if (_vm == null) return true;
            return _vm.MatchesFilter(query);
        }

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

        public void ToggleExpanded()
        {
            if (IsExpanded) Collapse();
            else Expand();
        }

        public void Collapse()
        {
            pnlVariants.Visible = false;
            pnlVariants.Height = 5;
            this.Height = CardCollapsedHeight;
            btnExpand.Text = "Méretek ▾";
        }

        private void Expand()
        {
            if (!_variantsLoaded) PopulateVariantPanel();
            pnlVariants.Visible = true;
            int variantsHeight = _vm.Variants.Count * VariantRowHeight + 8;
            pnlVariants.Height = variantsHeight;
            this.Height = CardCollapsedHeight + VariantPanelTopPadding + variantsHeight;
            btnExpand.Text = "Méretek ▴";
            this.Parent?.PerformLayout();
        }

        private void PopulateVariantPanel()
        {
            pnlVariants.SuspendLayout();
            pnlVariants.Controls.Clear();

            int y = 0;
            foreach (var variant in _vm.Variants)
            {
                var row = new VariantListItem();
                row.Setup(_service, variant);
                row.Location = new Point(5, y);
                row.Width = pnlVariants.Width - 10;
                row.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                row.SelectionChanged += OnVariantSelectionChanged;
                pnlVariants.Controls.Add(row);
                y += VariantRowHeight;
            }

            pnlVariants.Height = y + 4;
            pnlVariants.ResumeLayout();
            _variantsLoaded = true;
        }

        // -----------------------------------------------------------------
        // KÜLSŐ BULK — a MainForm hívja az alsó sávból
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

            lblCurrentStock.Text = _vm.TotalQuantityOnHand.ToString();
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

        private async Task<bool> TryUpdateInventory(
            Hotcakes.CommerceDTO.v1.Catalog.ProductInventoryDTO inv, int delta)
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