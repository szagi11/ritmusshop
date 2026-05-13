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
            lblVariantName.Font = UiTheme.BodyFont;
            lblOnHandValue.Font = UiTheme.BodyFont;
            lblReservedValue.Font = UiTheme.BodyFont;
            lblAvailableValue.Font = UiTheme.BodyFont;

            lblVariantName.ForeColor = UiTheme.TextPrimary;
            lblOnHandValue.ForeColor = UiTheme.TextPrimary;
            lblReservedValue.ForeColor = UiTheme.TextSecondary;
            lblAvailableValue.ForeColor = UiTheme.TextPrimary;
        }

        public void Setup(VariantViewModel variant)
        {
            _variant = variant;

            lblVariantName.Text = variant.DisplayName;
            RefreshStockValues();
            chkSelect.Checked = variant.IsSelected;

            UpdateBackgroundForSelection();
        }

        private void RefreshStockValues()
        {
            if (_variant == null) return;
            lblOnHandValue.Text = _variant.QuantityOnHand.ToString();
            lblReservedValue.Text = _variant.QuantityReserved.ToString();
            lblAvailableValue.Text = _variant.Available.ToString();
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
            if (_variant == null) return;
            lblVariantName.Text = _variant.DisplayName;
            RefreshStockValues();
        }

        private void UpdateBackgroundForSelection()
        {
            bool isSelected = _variant != null && _variant.IsSelected;
            var bg = isSelected ? UiTheme.AccentLight : UiTheme.CardBackground;

            this.BackColor = bg;
            chkSelect.BackColor = bg;
            lblVariantName.BackColor = bg;
            lblOnHandValue.BackColor = bg;
            lblReservedValue.BackColor = bg;
            lblAvailableValue.BackColor = bg;
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
