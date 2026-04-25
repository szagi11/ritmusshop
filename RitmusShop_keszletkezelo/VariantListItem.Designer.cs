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
        private System.Windows.Forms.TextBox txtDelta;
        private System.Windows.Forms.Button btnApply;

        private void InitializeComponent()
        {
            chkSelect = new System.Windows.Forms.CheckBox();
            lblVariantName = new System.Windows.Forms.Label();
            lblCurrentStock = new System.Windows.Forms.Label();
            txtDelta = new System.Windows.Forms.TextBox();
            btnApply = new System.Windows.Forms.Button();
            SuspendLayout();

            // chkSelect — bal szélen
            chkSelect.Location = new System.Drawing.Point(5, 6);
            chkSelect.Size = new System.Drawing.Size(20, 22);
            //
            // lblVariantName
            lblVariantName.Location = new System.Drawing.Point(28, 6);
            lblVariantName.Size = new System.Drawing.Size(70, 20);
            //
            // lblCurrentStock
            lblCurrentStock.Location = new System.Drawing.Point(100, 6);
            lblCurrentStock.Size = new System.Drawing.Size(40, 20);
            //
            // txtDelta
            txtDelta.Location = new System.Drawing.Point(145, 4);
            txtDelta.Size = new System.Drawing.Size(45, 25);
            //
            // btnApply
            btnApply.Location = new System.Drawing.Point(195, 3);
            btnApply.Size = new System.Drawing.Size(75, 26);
            //
            // VariantListItem
            Size = new System.Drawing.Size(280, 32);


            Controls.Add(chkSelect);
            Controls.Add(btnApply);
            Controls.Add(txtDelta);
            Controls.Add(lblCurrentStock);
            Controls.Add(lblVariantName);
            Name = "VariantListItem";
            Size = new System.Drawing.Size(490, 36);
            ResumeLayout(false);
            PerformLayout();
        }
    }
}