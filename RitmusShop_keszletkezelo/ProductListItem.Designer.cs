namespace RitmusShop_keszletkezelo
{
    partial class ProductListItem
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblProductName = new Label();
            lblSku = new Label();
            btnExpand = new Button();
            pnlVariants = new Panel();
            SuspendLayout();
            // 
            // lblProductName
            // 
            lblProductName.AutoSize = true;
            lblProductName.Location = new Point(31, 14);
            lblProductName.Name = "lblProductName";
            lblProductName.Size = new Size(50, 20);
            lblProductName.TabIndex = 0;
            lblProductName.Text = "label1";
            // 
            // lblSku
            // 
            lblSku.AutoSize = true;
            lblSku.Location = new Point(356, 12);
            lblSku.Name = "lblSku";
            lblSku.Size = new Size(50, 20);
            lblSku.TabIndex = 1;
            lblSku.Text = "label1";
            // 
            // btnExpand
            // 
            btnExpand.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnExpand.Location = new Point(530, 8);
            btnExpand.Name = "btnExpand";
            btnExpand.Size = new Size(94, 29);
            btnExpand.TabIndex = 2;
            btnExpand.UseVisualStyleBackColor = true;
            btnExpand.Click += btnExpand_Click;
            // 
            // pnlVariants
            // 
            pnlVariants.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlVariants.Location = new Point(31, 45);
            pnlVariants.Name = "pnlVariants";
            pnlVariants.Size = new Size(513, 155);
            pnlVariants.TabIndex = 3;
            // 
            // ProductListItem
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(pnlVariants);
            Controls.Add(btnExpand);
            Controls.Add(lblSku);
            Controls.Add(lblProductName);
            Name = "ProductListItem";
            Size = new Size(700, 40);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblProductName;
        private Label lblSku;
        private Button btnExpand;
        private Panel pnlVariants;
    }
}
