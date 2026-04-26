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
            // chkSelect — bal felső sarok
            // 
            chkSelect.Location = new Point(15, 18);
            chkSelect.Size = new Size(20, 24);
            chkSelect.ThreeState = true;
            chkSelect.UseVisualStyleBackColor = true;
            // 
            // lblProductName — termék neve, kártya teteje (a checkbox után)
            // 
            lblProductName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblProductName.Location = new Point(45, 15);
            lblProductName.Size = new Size(950, 28);
            lblProductName.AutoEllipsis = true;
            lblProductName.Text = "Termék neve";
            // 
            // lblSku — második sor bal oldalt
            // 
            lblSku.Location = new Point(45, 50);
            lblSku.Size = new Size(280, 22);
            lblSku.Text = "SKU";
            // 
            // lblCategory — második sor folytatása
            // 
            lblCategory.Location = new Point(335, 50);
            lblCategory.Size = new Size(300, 22);
            lblCategory.Text = "";
            // 
            // lblStockLabel — harmadik sor: "Készlet:" felirat
            // 
            lblStockLabel.Location = new Point(45, 90);
            lblStockLabel.Size = new Size(70, 22);
            lblStockLabel.Text = "Készlet:";
            lblStockLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblCurrentStock — érték
            // 
            lblCurrentStock.Location = new Point(115, 90);
            lblCurrentStock.Size = new Size(60, 22);
            lblCurrentStock.Text = "0";
            lblCurrentStock.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnExpand — JOBBRA anchorolt: a kártya jobb szélén
            // 
            btnExpand.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnExpand.Location = new Point(870, 87);
            btnExpand.Size = new Size(115, 28);
            btnExpand.UseVisualStyleBackColor = true;
            btnExpand.Click += btnExpand_Click;
            // 
            // pnlVariants — a kártya alján, a kibontott méretek paneljéhez
            // 
            pnlVariants.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlVariants.Location = new Point(45, 135);
            pnlVariants.Size = new Size(950, 5);
            pnlVariants.BackColor = System.Drawing.Color.FromArgb(252, 250, 245);
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
            Name = "ProductListItem";
            Size = new Size(1010, 145);
            Margin = new Padding(0, 0, 0, 8);
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