using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Hotcakes.CommerceDTO.v1.Catalog;
using RitmusShop_keszletkezelo.Services;
using RitmusShop_keszletkezelo.ViewModels;
using System.Drawing;

namespace RitmusShop_keszletkezelo
{
    public partial class VariantListItem : UserControl
    {
        private HotcakesApiService _service = null!;
        private VariantViewModel _variant = null!;

        public event EventHandler? SelectionChanged;

        public VariantListItem()
        {
            InitializeComponent();
            chkSelect.CheckedChanged += ChkSelect_CheckedChanged;

            this.BackColor = UiTheme.CardBackground;
            foreach (Control c in this.Controls)
                c.Font = UiTheme.BodyFont;

            btnApply.FlatStyle = FlatStyle.Flat;
            btnApply.BackColor = UiTheme.Accent;
            btnApply.ForeColor = Color.White;
            btnApply.FlatAppearance.BorderSize = 0;
            btnApply.Font = UiTheme.ButtonFont;

            txtDelta.BorderStyle = BorderStyle.FixedSingle;
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
        public void SetSelectedSilently(bool selected)
        {
            _variant.IsSelected = selected;
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