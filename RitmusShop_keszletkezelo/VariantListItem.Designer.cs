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
        private System.Windows.Forms.Label lblStockLabel;
        private System.Windows.Forms.Label lblCurrentStock;

        private void InitializeComponent()
        {
            chkSelect = new CheckBox();
            lblVariantName = new Label();
            lblStockLabel = new Label();
            lblCurrentStock = new Label();
            SuspendLayout();
            // 
            // chkSelect
            // 
            chkSelect.FlatAppearance.BorderColor = Color.FromArgb(184, 146, 60);
            chkSelect.FlatAppearance.CheckedBackColor = Color.FromArgb(184, 146, 60);
            chkSelect.Location = new Point(5, 6);
            chkSelect.Name = "chkSelect";
            chkSelect.Size = new Size(20, 22);
            chkSelect.TabIndex = 0;
            chkSelect.UseVisualStyleBackColor = true;
            // 
            // lblVariantName
            // 
            lblVariantName.Location = new Point(28, 6);
            lblVariantName.Name = "lblVariantName";
            lblVariantName.Size = new Size(60, 20);
            lblVariantName.TabIndex = 2;
            lblVariantName.Text = "Variáns";
            // 
            // lblStockLabel
            // 
            lblStockLabel.Location = new Point(95, 6);
            lblStockLabel.Name = "lblStockLabel";
            lblStockLabel.Size = new Size(65, 20);
            lblStockLabel.TabIndex = 3;
            lblStockLabel.Text = "Készlet:";
            // 
            // lblCurrentStock
            // 
            lblCurrentStock.Location = new Point(160, 6);
            lblCurrentStock.Name = "lblCurrentStock";
            lblCurrentStock.Size = new Size(60, 20);
            lblCurrentStock.TabIndex = 1;
            lblCurrentStock.Text = "0";
            lblCurrentStock.TextAlign = ContentAlignment.MiddleRight;
            // 
            // VariantListItem
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(lblStockLabel);
            Controls.Add(chkSelect);
            Controls.Add(lblCurrentStock);
            Controls.Add(lblVariantName);
            Name = "VariantListItem";
            Size = new Size(280, 32);
            ResumeLayout(false);
        }
    }
}