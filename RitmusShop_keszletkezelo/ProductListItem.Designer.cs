namespace RitmusShop_keszletkezelo
{
    partial class ProductListItem
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            chkSelect = new CheckBox();
            lblProductName = new Label();
            lblSku = new Label();
            lblCurrentStock = new Label();
            txtDelta = new TextBox();
            btnApply = new Button();
            btnExpand = new Button();
            pnlVariants = new Panel();
            SuspendLayout();
            // 
            // chkSelect
            // 
            chkSelect.Location = new Point(8, 12);
            chkSelect.Size = new Size(20, 24);
            chkSelect.ThreeState = true;
            chkSelect.UseVisualStyleBackColor = true;
            // 
            // lblProductName
            // 
            lblProductName.AutoSize = false;
            lblProductName.Location = new Point(35, 12);
            lblProductName.Name = "lblProductName";
            lblProductName.Size = new Size(260, 20);
            lblProductName.Text = "Termék neve";
            // 
            // lblSku
            // 
            lblSku.AutoSize = false;
            lblSku.Location = new Point(305, 12);
            lblSku.Name = "lblSku";
            lblSku.Size = new Size(130, 20);
            lblSku.Text = "SKU";
            // 
            // lblCurrentStock
            // 
            lblCurrentStock.AutoSize = false;
            lblCurrentStock.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            lblCurrentStock.Location = new Point(445, 12);
            lblCurrentStock.Name = "lblCurrentStock";
            lblCurrentStock.Size = new Size(60, 20);
            lblCurrentStock.Text = "0";
            // 
            // txtDelta
            // 
            txtDelta.Location = new Point(515, 8);
            txtDelta.Name = "txtDelta";
            txtDelta.Size = new Size(60, 27);
            txtDelta.TextAlign = HorizontalAlignment.Right;
            txtDelta.Text = "0";
            // 
            // btnApply
            // 
            btnApply.Location = new Point(580, 7);
            btnApply.Name = "btnApply";
            btnApply.Size = new Size(90, 29);
            btnApply.Text = "Alkalmaz";
            btnApply.UseVisualStyleBackColor = true;
            btnApply.Click += btnApply_Click;
            // 
            // btnExpand
            // 
            btnExpand.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnExpand.Location = new Point(680, 7);
            btnExpand.Name = "btnExpand";
            btnExpand.Size = new Size(110, 29);
            btnExpand.UseVisualStyleBackColor = true;
            btnExpand.Click += btnExpand_Click;
            // 
            // pnlVariants
            // 
            pnlVariants.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlVariants.Location = new Point(35, 45);
            pnlVariants.Name = "pnlVariants";
            pnlVariants.Size = new Size(755, 155);
            // 
            // ProductListItem
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(pnlVariants);
            Controls.Add(btnExpand);
            Controls.Add(btnApply);
            Controls.Add(txtDelta);
            Controls.Add(lblCurrentStock);
            Controls.Add(lblSku);
            Controls.Add(lblProductName);
            Controls.Add(chkSelect);
            Name = "ProductListItem";
            Size = new Size(800, 40);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CheckBox chkSelect;
        private Label lblProductName;
        private Label lblSku;
        private Label lblCurrentStock;
        private TextBox txtDelta;
        private Button btnApply;
        private Button btnExpand;
        private Panel pnlVariants;
    }
}