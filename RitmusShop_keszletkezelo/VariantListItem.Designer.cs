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
        private System.Windows.Forms.Label lblCurrentStock;

        private void InitializeComponent()
        {
            chkSelect = new System.Windows.Forms.CheckBox();
            lblVariantName = new System.Windows.Forms.Label();
            lblCurrentStock = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // chkSelect — bal szélen
            // 
            chkSelect.Location = new System.Drawing.Point(5, 6);
            chkSelect.Size = new System.Drawing.Size(20, 22);
            chkSelect.UseVisualStyleBackColor = true;
            // 
            // lblVariantName
            // 
            lblVariantName.AutoSize = false;
            lblVariantName.Location = new System.Drawing.Point(28, 6);
            lblVariantName.Size = new System.Drawing.Size(150, 20);
            lblVariantName.Text = "Variáns";
            // 
            // lblCurrentStock
            // 
            lblCurrentStock.AutoSize = false;
            lblCurrentStock.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            lblCurrentStock.Location = new System.Drawing.Point(180, 6);
            lblCurrentStock.Size = new System.Drawing.Size(60, 20);
            lblCurrentStock.Text = "0";
            // 
            // VariantListItem
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(chkSelect);
            Controls.Add(lblCurrentStock);
            Controls.Add(lblVariantName);
            Name = "VariantListItem";
            Size = new System.Drawing.Size(280, 32);
            ResumeLayout(false);
            PerformLayout();
        }
    }
}