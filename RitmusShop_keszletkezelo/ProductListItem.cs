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
        private const int CardWidth = 290;
        private const int CardCollapsedHeight = 145;
        private const int VariantRowHeight = 36;
        private const int VariantBulkRowHeight = 38;
        private const int VariantPanelTopPadding = 5;

        private HotcakesApiService _service = null!;
        private InventoryItemViewModel _vm = null!;
        private bool _variantsLoaded;
        private bool _suppressCheckEvent;

        private TextBox? _txtVariantBulkDelta;
        private Button? _btnVariantBulkApply;
        private Button? _btnSelectAllVariants;

        // ----- ESEMÉNYEK -----
        public event EventHandler? SelectionChanged;
        public event EventHandler? ExpandRequested;

        public ProductListItem()
        {
            InitializeComponent();
            ApplyCardStyling();
            pnlVariants.Visible = false;
            this.Size = new Size(CardWidth, CardCollapsedHeight);
            btnExpand.Text = "Méretek ▾";
            chkSelect.CheckedChanged += ChkSelect_CheckedChanged;
        }

        private void ApplyCardStyling()
        {
            this.BackColor = UiTheme.CardBackground;
            this.Margin = new Padding(8);

            this.Paint += (s, e) =>
            {
                var rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
                using var pen = new Pen(UiTheme.CardBorder, 1);
                e.Graphics.DrawRectangle(pen, rect);
            };

            lblProductName.Font = UiTheme.SubheadingFont;
            lblProductName.ForeColor = UiTheme.TextPrimary;

            lblSku.Font = UiTheme.BodyFont;
            lblSku.ForeColor = UiTheme.TextSecondary;

            lblStockLabel.Font = UiTheme.BodyFont;
            lblStockLabel.ForeColor = UiTheme.TextSecondary;

            lblCurrentStock.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblCurrentStock.ForeColor = UiTheme.TextPrimary;

            txtDelta.Font = UiTheme.BodyFont;
            txtDelta.BorderStyle = BorderStyle.FixedSingle;

            btnApply.FlatStyle = FlatStyle.Flat;
            btnApply.BackColor = UiTheme.Accent;
            btnApply.ForeColor = Color.White;
            btnApply.FlatAppearance.BorderSize = 0;
            btnApply.Font = UiTheme.ButtonFont;

            btnExpand.FlatStyle = FlatStyle.Flat;
            btnExpand.BackColor = UiTheme.CardBackground;
            btnExpand.ForeColor = UiTheme.TextPrimary;
            btnExpand.FlatAppearance.BorderColor = UiTheme.CardBorder;
            btnExpand.Font = UiTheme.ButtonFont;

            pnlVariants.BackColor = UiTheme.CardBackground;
        }

        public void Setup(HotcakesApiService service, InventoryItemViewModel vm)
        {
            _service = service;
            _vm = vm;

            lblProductName.Text = vm.ProductName;
            lblSku.Text = vm.Sku;
            lblCurrentStock.Text = vm.TotalQuantityOnHand.ToString();
            txtDelta.Text = "0";

            if (vm.HasVariants)
            {
                txtDelta.Visible = false;
                btnApply.Visible = false;
                btnExpand.Visible = true;
            }
            else
            {
                txtDelta.Visible = true;
                btnApply.Visible = true;
                btnExpand.Visible = false;
            }

            UpdateProductCheckboxState();
        }

        // ----- KÜLSŐ KEZELÉS: kibontás/bezárás -----

        public bool IsExpanded => pnlVariants.Visible;

        public void Collapse()
        {
            if (pnlVariants.Visible)
            {
                pnlVariants.Visible = false;
                this.Size = new Size(CardWidth, CardCollapsedHeight);
                btnExpand.Text = "Méretek ▾";
            }
        }

        // ----- KIJELÖLT SOROK -----

        public IEnumerable<Hotcakes.CommerceDTO.v1.Catalog.ProductInventoryDTO> GetSelectedInventories()
        {
            if (!_vm.HasVariants)
            {
                if (_vm.IsSelected && _vm.MainInventory != null)
                    yield return _vm.MainInventory;
            }
            else
            {
                foreach (var variant in _vm.Variants.Where(v => v.IsSelected && v.Inventory != null))
                    yield return variant.Inventory!;
            }
        }

        public bool MatchesFilter(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return true;
            if (_vm == null) return true;

            return ContainsCi(_vm.ProductName, query)
                || ContainsCi(_vm.Sku, query)
                || _vm.Variants.Any(v =>
                    ContainsCi(v.Sku, query) || ContainsCi(v.DisplayName, query));
        }

        private static bool ContainsCi(string? haystack, string needle) =>
            haystack != null && haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;

        // ----- CHECKBOX -----

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

            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateProductCheckboxState()
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
        }

        private void OnVariantSelectionChanged(object? sender, EventArgs e)
        {
            UpdateProductCheckboxState();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        // ----- KIBONTÁS -----

        private void btnExpand_Click(object? sender, EventArgs e)
        {
            if (pnlVariants.Visible)
            {
                pnlVariants.Visible = false;
                this.Size = new Size(CardWidth, CardCollapsedHeight);
                btnExpand.Text = "Méretek ▾";
            }
            else
            {
                ExpandRequested?.Invoke(this, EventArgs.Empty);

                if (!_variantsLoaded) PopulateVariantPanel();

                pnlVariants.Visible = true;
                int variantsHeight = _vm.Variants.Count * VariantRowHeight + VariantBulkRowHeight + 8;
                this.Size = new Size(CardWidth, CardCollapsedHeight + VariantPanelTopPadding + variantsHeight);
                btnExpand.Text = "Méretek ▴";
                this.Parent?.PerformLayout();
            }
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
                row.SelectionChanged += OnVariantSelectionChanged;
                pnlVariants.Controls.Add(row);
                y += VariantRowHeight;
            }

            _btnSelectAllVariants = new Button
            {
                Location = new Point(5, y + 4),
                Size = new Size(95, 28),
                Text = "Mind ki",
                FlatStyle = FlatStyle.Flat,
                BackColor = UiTheme.CardBackground,
                Font = UiTheme.ButtonFont,
                UseVisualStyleBackColor = true
            };
            _btnSelectAllVariants.FlatAppearance.BorderColor = UiTheme.CardBorder;
            _btnSelectAllVariants.Click += BtnSelectAllVariants_Click;

            _txtVariantBulkDelta = new TextBox
            {
                Location = new Point(110, 5 + y),
                Size = new Size(50, 25),
                TextAlign = HorizontalAlignment.Right,
                Text = "0",
                Font = UiTheme.BodyFont,
                BorderStyle = BorderStyle.FixedSingle
            };
            _btnVariantBulkApply = new Button
            {
                Location = new Point(165, 4 + y),
                Size = new Size(115, 28),
                Text = "Kijelöltekre",
                FlatStyle = FlatStyle.Flat,
                BackColor = UiTheme.Accent,
                ForeColor = Color.White,
                Font = UiTheme.ButtonFont
            };
            _btnVariantBulkApply.FlatAppearance.BorderSize = 0;
            _btnVariantBulkApply.Click += BtnVariantBulkApply_Click;

            pnlVariants.Controls.Add(_btnSelectAllVariants);
            pnlVariants.Controls.Add(_txtVariantBulkDelta);
            pnlVariants.Controls.Add(_btnVariantBulkApply);

            pnlVariants.Height = y + VariantBulkRowHeight + 4;
            pnlVariants.ResumeLayout();
            _variantsLoaded = true;
        }

        // ----- VARIÁNS-SZINTŰ TÖMEGES -----

        private void BtnSelectAllVariants_Click(object? sender, EventArgs e)
        {
            bool allSelected = _vm.Variants.All(v => v.IsSelected);
            bool newState = !allSelected;

            foreach (var variant in _vm.Variants)
                variant.IsSelected = newState;
            foreach (var row in pnlVariants.Controls.OfType<VariantListItem>())
                row.SetSelectedSilently(newState);

            UpdateProductCheckboxState();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private async void BtnVariantBulkApply_Click(object? sender, EventArgs e)
        {
            if (_txtVariantBulkDelta == null || _btnVariantBulkApply == null) return;

            if (!int.TryParse(_txtVariantBulkDelta.Text, out int delta) || delta == 0)
            {
                MessageBox.Show("Adj meg egy nem nulla egész számot.",
                    "Érvénytelen érték", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selected = _vm.Variants.Where(v => v.IsSelected && v.Inventory != null).ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show("Nincs kijelölt méret.",
                    "Tömeges módosítás", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                _btnVariantBulkApply.Enabled = false;
                Cursor = Cursors.WaitCursor;

                int success = 0, failed = 0;
                foreach (var variant in selected)
                {
                    if (await TryUpdateInventory(variant.Inventory!, delta)) success++;
                    else failed++;
                }

                lblCurrentStock.Text = _vm.TotalQuantityOnHand.ToString();
                RefreshVariantRowLabels();
                _txtVariantBulkDelta.Text = "0";

                if (failed > 0)
                    MessageBox.Show($"Sikeres: {success}, sikertelen: {failed}.",
                        "Tömeges módosítás", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                _btnVariantBulkApply.Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        // ----- KÜLSŐ BULK -----

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

        public bool HasAnySelection() => GetSelectedInventories().Any();
        public int CountSelected() => GetSelectedInventories().Count();

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

        // ----- HELPER -----

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

        // ----- KÖZVETLEN MENTÉS -----

        private async void btnApply_Click(object? sender, EventArgs e)
        {
            if (_vm?.MainInventory == null)
            {
                MessageBox.Show("Ehhez a termékhez nincs készletsor.",
                    "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtDelta.Text, out int delta))
            {
                MessageBox.Show("Csak egész szám adható meg.",
                    "Érvénytelen érték", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnApply.Enabled = false;
                Cursor = Cursors.WaitCursor;

                if (await TryUpdateInventory(_vm.MainInventory, delta))
                {
                    lblCurrentStock.Text = _vm.TotalQuantityOnHand.ToString();
                    txtDelta.Text = "0";
                }
                else
                {
                    MessageBox.Show("Sikertelen mentés vagy 0 alá menne.",
                        "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            finally
            {
                btnApply.Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        public class BulkResult
        {
            public int SuccessCount { get; set; }
            public int FailedCount { get; set; }
        }
    }
}