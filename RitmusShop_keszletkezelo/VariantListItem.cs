using System;
using System.Drawing;
using System.Windows.Forms;
using RitmusShop_keszletkezelo.ViewModels;

namespace RitmusShop_keszletkezelo
{
    public partial class VariantListItem : UserControl
    {
        private VariantViewModel _variant = null!;

        public event EventHandler? SelectionChanged;

        public VariantListItem()
        {
            InitializeComponent();
            chkSelect.CheckedChanged += ChkSelect_CheckedChanged;

            this.BackColor = UiTheme.CardBackground;
            foreach (Control c in this.Controls)
                c.Font = UiTheme.BodyFont;
        }

        public void Setup(VariantViewModel variant)
        {
            _variant = variant;

            lblVariantName.Text = variant.DisplayName;
            lblCurrentStock.Text = variant.QuantityOnHand.ToString();
            chkSelect.Checked = variant.IsSelected;

            UpdateBackgroundForSelection();
        }

        public void SetSelectedSilently(bool selected)
        {
            _variant.IsSelected = selected;
            chkSelect.CheckedChanged -= ChkSelect_CheckedChanged;
            chkSelect.Checked = selected;
            chkSelect.CheckedChanged += ChkSelect_CheckedChanged;
            UpdateBackgroundForSelection();
            this.Invalidate();
        }

        public void RefreshDisplay()
        {
            if (_variant != null)
                lblCurrentStock.Text = _variant.QuantityOnHand.ToString();
        }

        private void UpdateBackgroundForSelection()
        {
            bool isSelected = _variant != null && _variant.IsSelected;
            var bg = isSelected ? UiTheme.AccentLight : UiTheme.CardBackground;

            this.BackColor = bg;
            chkSelect.BackColor = bg;
            lblVariantName.BackColor = bg;
            lblStockLabel.BackColor = bg;
            lblCurrentStock.BackColor = bg;
        }

        private void ChkSelect_CheckedChanged(object? sender, EventArgs e)
        {
            _variant.IsSelected = chkSelect.Checked;
            UpdateBackgroundForSelection();
            this.Invalidate();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}