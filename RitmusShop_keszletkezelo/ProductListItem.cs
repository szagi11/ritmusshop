using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
        private const int VariantPanelTopPadding = 5;


        private IHotcakesApiService _service = null!;
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
            pnlVariants.Resize += PnlVariants_Resize;
            this.Height = CardCollapsedHeight;
            btnExpand.Text = "Méretek ▾";
            chkSelect.CheckedChanged += ChkSelect_CheckedChanged;
        }

        private void PnlVariants_Resize(object? sender, EventArgs e)
        {
            if (!_variantsLoaded) return;

            foreach (var row in pnlVariants.Controls.OfType<VariantListItem>())
            {
                row.Width = CalcVariantRowWidth();
                row.Left = CalcVariantRowX(row.Width);
            }
        }

        private int CalcVariantRowWidth()
        {
            var available = pnlVariants.Width - 10;
            if (available <= 0) return 0;

            return Math.Min(280, available);
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

            lblCurrentStock.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblCurrentStock.ForeColor = UiTheme.TextPrimary;

            btnExpand.FlatStyle = FlatStyle.Flat;
            btnExpand.BackColor = UiTheme.CardBackground;
            btnExpand.ForeColor = UiTheme.TextPrimary;
            btnExpand.FlatAppearance.BorderColor = UiTheme.CardBorder;
            btnExpand.Font = UiTheme.ButtonFont;
        }

        public void Setup(IHotcakesApiService service, InventoryItemViewModel vm)
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
                row.Setup(variant);

                var rowWidth = CalcVariantRowWidth();
                row.Location = new Point(CalcVariantRowX(rowWidth), y);
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