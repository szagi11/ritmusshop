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
            lblCategory = new Label();
            lblStockLabel = new Label();
            lblCurrentStock = new Label();
            btnExpand = new Button();
            pnlVariants = new Panel();
            SuspendLayout();
            // 
            // chkSelect
            // 
            chkSelect.Location = new Point(15, 18);
            chkSelect.Name = "chkSelect";
            chkSelect.Size = new Size(20, 24);
            chkSelect.TabIndex = 7;
            chkSelect.ThreeState = true;
            chkSelect.UseVisualStyleBackColor = true;
            // 
            // lblProductName
            // 
            lblProductName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblProductName.AutoEllipsis = true;
            lblProductName.Location = new Point(45, 15);
            lblProductName.Name = "lblProductName";
            lblProductName.Size = new Size(950, 28);
            lblProductName.TabIndex = 6;
            lblProductName.Text = "Termék neve";
            // 
            // lblSku
            // 
            lblSku.Location = new Point(45, 45);
            lblSku.Name = "lblSku";
            lblSku.Size = new Size(280, 22);
            lblSku.TabIndex = 5;
            lblSku.Text = "SKU";
            // 
            // lblCategory
            // 
            lblCategory.Location = new Point(335, 45);
            lblCategory.Name = "lblCategory";
            lblCategory.Size = new Size(300, 22);
            lblCategory.TabIndex = 4;
            // 
            // lblStockLabel
            // 
            lblStockLabel.Location = new Point(45, 75);
            lblStockLabel.Name = "lblStockLabel";
            lblStockLabel.Size = new Size(70, 22);
            lblStockLabel.TabIndex = 3;
            lblStockLabel.Text = "Készlet:";
            lblStockLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblCurrentStock
            // 
            lblCurrentStock.Location = new Point(115, 75);
            lblCurrentStock.Name = "lblCurrentStock";
            lblCurrentStock.Size = new Size(386, 22);
            lblCurrentStock.TabIndex = 2;
            lblCurrentStock.Text = "0";
            lblCurrentStock.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnExpand
            // 
            btnExpand.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnExpand.AutoSize = true;
            btnExpand.Location = new Point(870, 72);
            btnExpand.Name = "btnExpand";
            btnExpand.Size = new Size(115, 28);
            btnExpand.TabIndex = 1;
            btnExpand.UseVisualStyleBackColor = true;
            btnExpand.Click += btnExpand_Click;
            // 
            // pnlVariants
            // 
            pnlVariants.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlVariants.BackColor = Color.FromArgb(252, 250, 245);
            pnlVariants.Location = new Point(45, 105);
            pnlVariants.Name = "pnlVariants";
            pnlVariants.Size = new Size(950, 5);
            pnlVariants.TabIndex = 0;
            // 
            // ProductListItem
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(pnlVariants);
            Controls.Add(btnExpand);
            Controls.Add(lblCurrentStock);
            Controls.Add(lblStockLabel);
            Controls.Add(lblCategory);
            Controls.Add(lblSku);
            Controls.Add(lblProductName);
            Controls.Add(chkSelect);
            Margin = new Padding(0, 0, 0, 8);
            Name = "ProductListItem";
            Size = new Size(1010, 115);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CheckBox chkSelect;
        private Label lblProductName;
        private Label lblSku;
        private Label lblCategory;
        private Label lblStockLabel;
        private Label lblCurrentStock;
        private Button btnExpand;
        private Panel pnlVariants;
    }
}