using System;
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
        private const int CollapsedHeight = 40;
        private const int VariantRowHeight = 40;
        private const int VariantBulkRowHeight = 40;

        private HotcakesApiService _service = null!;
        private InventoryItemViewModel _vm = null!;
        private bool _variantsLoaded;
        private bool _suppressCheckEvent;

        // A kibontott panel saját bulk vezérlői
        private TextBox? _txtVariantBulkDelta;
        private Button? _btnVariantBulkApply;
        private Button? _btnSelectAllVariants;

        /// <summary>Esemény, hogy a MainForm tudja, mikor változott a kijelölés.</summary>
        public event EventHandler? SelectionChanged;

        public ProductListItem()
        {
            InitializeComponent();
            pnlVariants.Visible = false;
            this.Height = CollapsedHeight;
            btnExpand.Text = "🔽 Mutat";
            chkSelect.CheckedChanged += ChkSelect_CheckedChanged;
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

        // -----------------------------------------------------------------
        // KIJELÖLT SOROK ÖSSZEGYŰJTÉSE — a MainForm hívja a tömeges műveletnél
        // -----------------------------------------------------------------

        /// <summary>
        /// Visszaadja az összes kijelölt InventoryDTO-t ebből a termékből.
        /// Variáns nélküli terméknél a MainInventory; variánsosnál a kijelölt
        /// VariantViewModel-ek Inventory-ja.
        /// </summary>
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

        // -----------------------------------------------------------------
        // KERESŐ ILLESZKEDÉS
        // -----------------------------------------------------------------

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

        // -----------------------------------------------------------------
        // CHECKBOX-LOGIKA
        // -----------------------------------------------------------------

        /// <summary>
        /// A felhasználó kattintott a termék-checkboxra. A tri-state miatt
        /// csak Checked ↔ Unchecked között váltunk; Indeterminate-be csak
        /// programozottan kerül (variánsok eltérő állapota esetén).
        /// </summary>
        private void ChkSelect_CheckedChanged(object? sender, EventArgs e)
        {
            if (_suppressCheckEvent) return;

            // CheckState értelmezése: ha a felhasználó kattint, az Indeterminate
            // után automatikusan Unchecked-be megy. A logikánk: bool kijelölést
            // a Checked szerint igazítjuk.
            bool selected = chkSelect.CheckState == CheckState.Checked;

            if (_vm.HasVariants)
            {
                // Minden variáns kijelölődik / törlődik
                foreach (var variant in _vm.Variants)
                    variant.IsSelected = selected;

                // A megjelenített VariantListItem-ek checkboxait is szinkronizáljuk
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

        /// <summary>
        /// A termék-checkbox vizuális állapotát igazítja a variánsok valós
        /// állapotához (mind/néhány/egyik sem). Akkor hívjuk, ha egy variáns
        /// változott egyedileg.
        /// </summary>
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

        /// <summary>
        /// A VariantListItem hívja vissza, amikor a saját checkboxa változott.
        /// </summary>
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
            if (pnlVariants.Visible)
            {
                pnlVariants.Visible = false;
                this.Size = new Size(this.Width, CollapsedHeight);
                btnExpand.Text = "🔽 Mutat";
            }
            else
            {
                if (!_variantsLoaded) PopulateVariantPanel();

                pnlVariants.Visible = true;
                int variantsHeight = _vm.Variants.Count * VariantRowHeight + VariantBulkRowHeight + 8;
                this.Size = new Size(this.Width, CollapsedHeight + 8 + variantsHeight);
                btnExpand.Text = "🔼 Elrejt";
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
                row.Location = new Point(0, y);
                row.Width = pnlVariants.Width;
                row.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                row.SelectionChanged += OnVariantSelectionChanged;
                pnlVariants.Controls.Add(row);
                y += VariantRowHeight;
            }

            // "Mind ki/eltávolít" + tömeges textbox + alkalmaz gomb
            _btnSelectAllVariants = new Button
            {
                Location = new Point(5, y + 5),
                Size = new Size(150, 28),
                Text = "Mind ki/eltávolít",
                UseVisualStyleBackColor = true
            };
            _btnSelectAllVariants.Click += BtnSelectAllVariants_Click;

            _txtVariantBulkDelta = new TextBox
            {
                Location = new Point(305, y + 5),
                Size = new Size(70, 27),
                TextAlign = HorizontalAlignment.Right,
                Text = "0"
            };
            _btnVariantBulkApply = new Button
            {
                Location = new Point(385, y + 4),
                Size = new Size(120, 28),
                Text = "Kijelöltekre",
                UseVisualStyleBackColor = true
            };
            _btnVariantBulkApply.Click += BtnVariantBulkApply_Click;

            pnlVariants.Controls.Add(_btnSelectAllVariants);
            pnlVariants.Controls.Add(_txtVariantBulkDelta);
            pnlVariants.Controls.Add(_btnVariantBulkApply);

            pnlVariants.Height = y + VariantBulkRowHeight + 4;
            pnlVariants.ResumeLayout();
            _variantsLoaded = true;
        }

        // -----------------------------------------------------------------
        // VARIÁNS-SZINTŰ MŰVELETEK
        // -----------------------------------------------------------------

        private void BtnSelectAllVariants_Click(object? sender, EventArgs e)
        {
            // Ha minden ki van jelölve, töröljük; egyébként mindet kijelöljük (toggle).
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
                MessageBox.Show("Nincs kijelölt variáns ebben a termékben.",
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

        // -----------------------------------------------------------------
        // KÖZÖS BULK SEGÉD — a MainForm-ról is hívható tömeges művelethez
        // -----------------------------------------------------------------

        /// <summary>
        /// Kívülről (MainForm-ról) meghívva alkalmazza a delta-t a saját
        /// kijelölt soraira (variánsosnál a bepipált variánsok, kiegészítőnél
        /// a saját MainInventory ha be van pipálva).
        /// </summary>
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

        // -----------------------------------------------------------------
        // KÖZÖS HELPER
        // -----------------------------------------------------------------

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

        // -----------------------------------------------------------------
        // KÖZVETLEN MENTÉS — variáns nélküli termékek
        // -----------------------------------------------------------------

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
                    MessageBox.Show("Sikertelen mentés vagy 0 alá menne a készlet.",
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