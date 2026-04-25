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
            lblStockLabel = new Label();
            lblCurrentStock = new Label();
            txtDelta = new TextBox();
            btnApply = new Button();
            btnExpand = new Button();
            pnlVariants = new Panel();
            SuspendLayout();
            // 
            // chkSelect — bal felső sarok
            // 
            chkSelect.Location = new Point(10, 12);
            chkSelect.Size = new Size(20, 24);
            chkSelect.ThreeState = true;
            chkSelect.UseVisualStyleBackColor = true;
            // 
            // lblProductName — termék neve, kártya teteje
            // 
            lblProductName.Location = new Point(35, 10);
            lblProductName.Size = new Size(245, 35);
            lblProductName.Name = "lblProductName";
            lblProductName.Text = "Termék neve";
            // AutoEllipsis True: ha hosszú a név, "..."-tal vágja
            lblProductName.AutoEllipsis = true;
            // 
            // lblSku
            // 
            lblSku.Location = new Point(35, 47);
            lblSku.Size = new Size(245, 20);
            lblSku.Name = "lblSku";
            lblSku.Text = "SKU";
            // 
            // lblStockLabel — "Készlet:" felirat
            // 
            lblStockLabel.Location = new Point(10, 75);
            lblStockLabel.Size = new Size(70, 22);
            lblStockLabel.Name = "lblStockLabel";
            lblStockLabel.Text = "Készlet:";
            lblStockLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblCurrentStock — érték
            // 
            lblCurrentStock.Location = new Point(80, 75);
            lblCurrentStock.Size = new Size(50, 22);
            lblCurrentStock.Name = "lblCurrentStock";
            lblCurrentStock.Text = "0";
            lblCurrentStock.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtDelta — kis szám textbox (kártyán belül)
            // 
            txtDelta.Location = new Point(140, 73);
            txtDelta.Size = new Size(50, 25);
            txtDelta.Name = "txtDelta";
            txtDelta.TextAlign = HorizontalAlignment.Right;
            txtDelta.Text = "0";
            // 
            // btnApply
            // 
            btnApply.Location = new Point(195, 72);
            btnApply.Size = new Size(85, 28);
            btnApply.Name = "btnApply";
            btnApply.Text = "Alkalmaz";
            btnApply.UseVisualStyleBackColor = true;
            btnApply.Click += btnApply_Click;
            // 
            // btnExpand — a kártya alja, középen
            // 
            btnExpand.Location = new Point(80, 105);
            btnExpand.Size = new Size(130, 28);
            btnExpand.Name = "btnExpand";
            btnExpand.UseVisualStyleBackColor = true;
            btnExpand.Click += btnExpand_Click;
            // 
            // pnlVariants — a kártya alá nyíló panel
            // 
            pnlVariants.Location = new Point(0, 145);
            pnlVariants.Size = new Size(290, 155);
            pnlVariants.Name = "pnlVariants";
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
            Controls.Add(lblStockLabel);
            Controls.Add(lblSku);
            Controls.Add(lblProductName);
            Controls.Add(chkSelect);
            Name = "ProductListItem";
            Size = new Size(290, 145);
            Margin = new Padding(8);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CheckBox chkSelect;
        private Label lblProductName;
        private Label lblSku;
        private Label lblStockLabel;
        private Label lblCurrentStock;
        private TextBox txtDelta;
        private Button btnApply;
        private Button btnExpand;
        private Panel pnlVariants;
    }
}