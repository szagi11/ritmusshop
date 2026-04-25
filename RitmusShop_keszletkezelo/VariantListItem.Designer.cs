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
            // 
            // chkSelect
            // 
            chkSelect.Location = new System.Drawing.Point(5, 8);
            chkSelect.Size = new System.Drawing.Size(20, 24);
            chkSelect.UseVisualStyleBackColor = true;
            // 
            // lblVariantName
            // 
            lblVariantName.AutoSize = false;
            lblVariantName.Location = new System.Drawing.Point(30, 8);
            lblVariantName.Size = new System.Drawing.Size(200, 20);
            lblVariantName.Text = "Variáns";
            // 
            // lblCurrentStock
            // 
            lblCurrentStock.AutoSize = false;
            lblCurrentStock.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            lblCurrentStock.Location = new System.Drawing.Point(235, 8);
            lblCurrentStock.Size = new System.Drawing.Size(60, 20);
            lblCurrentStock.Text = "0";
            // 
            // txtDelta
            // 
            txtDelta.Location = new System.Drawing.Point(305, 5);
            txtDelta.Size = new System.Drawing.Size(70, 27);
            txtDelta.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            txtDelta.Text = "0";
            // 
            // btnApply
            // 
            btnApply.Location = new System.Drawing.Point(385, 4);
            btnApply.Size = new System.Drawing.Size(90, 28);
            btnApply.Text = "Alkalmaz";
            btnApply.UseVisualStyleBackColor = true;
            btnApply.Click += btnApply_Click;
            // 
            // VariantListItem
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
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