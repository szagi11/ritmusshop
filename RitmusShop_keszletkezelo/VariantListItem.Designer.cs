namespace RitmusShop_keszletkezelo
{
    partial class VariantListItem
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private System.Windows.Forms.CheckBox chkSelect;
        private System.Windows.Forms.Label lblVariantName;
        private System.Windows.Forms.Label lblOnHandValue;
        private System.Windows.Forms.Label lblReservedValue;
        private System.Windows.Forms.Label lblAvailableValue;

        private void InitializeComponent()
        {
            chkSelect = new CheckBox();
            lblVariantName = new Label();
            lblOnHandValue = new Label();
            lblReservedValue = new Label();
            lblAvailableValue = new Label();
            SuspendLayout();
            //
            // chkSelect
            //
            chkSelect.FlatAppearance.BorderColor = Color.FromArgb(184, 146, 60);
            chkSelect.FlatAppearance.CheckedBackColor = Color.FromArgb(184, 146, 60);
            chkSelect.Location = new Point(8, 6);
            chkSelect.Name = "chkSelect";
            chkSelect.Size = new Size(20, 22);
            chkSelect.TabIndex = 0;
            chkSelect.UseVisualStyleBackColor = true;
            //
            // lblVariantName
            //
            lblVariantName.Location = new Point(34, 6);
            lblVariantName.Name = "lblVariantName";
            lblVariantName.Size = new Size(55, 20);
            lblVariantName.TabIndex = 1;
            lblVariantName.Text = "";
            lblVariantName.TextAlign = ContentAlignment.MiddleLeft;
            //
            // lblOnHandValue
            //
            lblOnHandValue.Location = new Point(95, 6);
            lblOnHandValue.Name = "lblOnHandValue";
            lblOnHandValue.Size = new Size(85, 20);
            lblOnHandValue.TabIndex = 2;
            lblOnHandValue.Text = "";
            lblOnHandValue.TextAlign = ContentAlignment.MiddleCenter;
            //
            // lblReservedValue
            //
            lblReservedValue.Location = new Point(185, 6);
            lblReservedValue.Name = "lblReservedValue";
            lblReservedValue.Size = new Size(85, 20);
            lblReservedValue.TabIndex = 3;
            lblReservedValue.Text = "";
            lblReservedValue.TextAlign = ContentAlignment.MiddleCenter;
            //
            // lblAvailableValue
            //
            lblAvailableValue.Location = new Point(275, 6);
            lblAvailableValue.Name = "lblAvailableValue";
            lblAvailableValue.Size = new Size(85, 20);
            lblAvailableValue.TabIndex = 4;
            lblAvailableValue.Text = "";
            lblAvailableValue.TextAlign = ContentAlignment.MiddleCenter;
            //
            // VariantListItem
            //
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(chkSelect);
            Controls.Add(lblVariantName);
            Controls.Add(lblOnHandValue);
            Controls.Add(lblReservedValue);
            Controls.Add(lblAvailableValue);
            Name = "VariantListItem";
            Size = new Size(370, 32);
            ResumeLayout(false);
        }
    }
}
