using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Hotcakes.CommerceDTO.v1.Catalog;
using RitmusShop_keszletkezelo.Services;
using RitmusShop_keszletkezelo.ViewModels;

namespace RitmusShop_keszletkezelo
{
    public partial class VariantListItem : UserControl
    {
        private HotcakesApiService _service = null!;
        private VariantViewModel _variant = null!;

        /// <summary>Eseményt vált ki, ha a checkbox állapota változik (a parent figyeli).</summary>
        public event EventHandler? SelectionChanged;

        public VariantListItem()
        {
            InitializeComponent();
            chkSelect.CheckedChanged += ChkSelect_CheckedChanged;
        }

        public void Setup(HotcakesApiService service, VariantViewModel variant)
        {
            _service = service;
            _variant = variant;

            lblVariantName.Text = variant.DisplayName;
            lblCurrentStock.Text = variant.QuantityOnHand.ToString();
            txtDelta.Text = "0";
            chkSelect.Checked = variant.IsSelected;
        }

        /// <summary>
        /// Külsőleg állítja a checkbox állapotát (pl. amikor a termék-checkbox 
        /// minden variánsát egyszerre jelöli ki). A SuppressEvent flaggel
        /// elkerüljük, hogy ez visszafelé is triggerelje a parent-et.
        /// </summary>
        public void SetSelectedSilently(bool selected)
        {
            _variant.IsSelected = selected;
            // Eltávolítjuk az event handlert, beállítjuk, visszatesszük
            chkSelect.CheckedChanged -= ChkSelect_CheckedChanged;
            chkSelect.Checked = selected;
            chkSelect.CheckedChanged += ChkSelect_CheckedChanged;
        }

        public void RefreshDisplay()
        {
            if (_variant != null)
                lblCurrentStock.Text = _variant.QuantityOnHand.ToString();
        }

        private void ChkSelect_CheckedChanged(object? sender, EventArgs e)
        {
            _variant.IsSelected = chkSelect.Checked;
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private async void btnApply_Click(object? sender, EventArgs e)
        {
            if (_variant?.Inventory == null)
            {
                MessageBox.Show("Ehhez a variánshoz nincs készletsor.",
                    "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtDelta.Text, out int delta))
            {
                MessageBox.Show("Csak egész szám adható meg.",
                    "Érvénytelen érték", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int oldQty = _variant.Inventory.QuantityOnHand;
            int newQty = oldQty + delta;
            if (newQty < 0)
            {
                MessageBox.Show("A készlet nem mehet 0 alá.",
                    "Érvénytelen érték", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnApply.Enabled = false;
                Cursor = Cursors.WaitCursor;

                _variant.Inventory.QuantityOnHand = newQty;
                var updated = await _service.UpdateInventoryAsync(_variant.Inventory);
                if (updated != null) _variant.Inventory = updated;

                lblCurrentStock.Text = _variant.QuantityOnHand.ToString();
                txtDelta.Text = "0";
            }
            catch (Exception ex)
            {
                _variant.Inventory.QuantityOnHand = oldQty;
                MessageBox.Show($"Sikertelen mentés:\n{ex.Message}",
                    "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnApply.Enabled = true;
                Cursor = Cursors.Default;
            }
        }
    }
}