namespace RitmusShop_keszletkezelo
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            flpCategories = new FlowLayoutPanel();
            flpProducts = new FlowLayoutPanel();
            txtSearch = new TextBox();
            lblSearch = new Label();
            pnlBulkBar = new Panel();
            btnSelectAll = new Button();
            lblBulkInfo = new Label();
            txtBulkDelta = new TextBox();
            btnBulkApply = new Button();
            SuspendLayout();
            // 
            // lblSearch
            // 
            lblSearch.AutoSize = true;
            lblSearch.Location = new Point(268, 5);
            lblSearch.Name = "lblSearch";
            lblSearch.Size = new Size(60, 20);
            lblSearch.TabIndex = 0;
            lblSearch.Text = "Keresés:";
            // 
            // txtSearch
            // 
            txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSearch.Location = new Point(335, 2);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "Szűrés SKU vagy név alapján...";
            txtSearch.Size = new Size(815, 27);
            txtSearch.TabIndex = 1;
            // 
            // flpCategories
            // 
            flpCategories.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            flpCategories.AutoScroll = true;
            flpCategories.FlowDirection = FlowDirection.TopDown;
            flpCategories.Location = new Point(12, 35);
            flpCategories.Name = "flpCategories";
            flpCategories.Size = new Size(250, 1175);
            flpCategories.TabIndex = 2;
            // 
            // flpProducts
            // 
            flpProducts.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            flpProducts.AutoScroll = true;
            flpProducts.FlowDirection = FlowDirection.TopDown;
            flpProducts.Location = new Point(268, 35);
            flpProducts.Name = "flpProducts";
            flpProducts.Size = new Size(882, 1125);
            flpProducts.TabIndex = 3;
            flpProducts.WrapContents = false;
            // 
            // pnlBulkBar
            // 
            pnlBulkBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlBulkBar.BorderStyle = BorderStyle.FixedSingle;
            pnlBulkBar.Controls.Add(btnSelectAll);
            pnlBulkBar.Controls.Add(lblBulkInfo);
            pnlBulkBar.Controls.Add(txtBulkDelta);
            pnlBulkBar.Controls.Add(btnBulkApply);
            pnlBulkBar.Location = new Point(268, 1170);
            pnlBulkBar.Name = "pnlBulkBar";
            pnlBulkBar.Size = new Size(882, 40);
            pnlBulkBar.TabIndex = 4;
            // 
            // btnSelectAll
            // 
            btnSelectAll.Location = new Point(8, 5);
            btnSelectAll.Name = "btnSelectAll";
            btnSelectAll.Size = new Size(160, 29);
            btnSelectAll.TabIndex = 0;
            btnSelectAll.Text = "Mind ki/eltávolít";
            btnSelectAll.UseVisualStyleBackColor = true;
            // 
            // lblBulkInfo
            // 
            lblBulkInfo.AutoSize = true;
            lblBulkInfo.Location = new Point(180, 9);
            lblBulkInfo.Name = "lblBulkInfo";
            lblBulkInfo.Size = new Size(100, 20);
            lblBulkInfo.TabIndex = 1;
            lblBulkInfo.Text = "Kijelölt: 0 sor";
            // 
            // txtBulkDelta
            // 
            txtBulkDelta.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtBulkDelta.Location = new Point(650, 5);
            txtBulkDelta.Name = "txtBulkDelta";
            txtBulkDelta.Size = new Size(80, 27);
            txtBulkDelta.TabIndex = 2;
            txtBulkDelta.TextAlign = HorizontalAlignment.Right;
            txtBulkDelta.Text = "0";
            // 
            // btnBulkApply
            // 
            btnBulkApply.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBulkApply.Location = new Point(740, 4);
            btnBulkApply.Name = "btnBulkApply";
            btnBulkApply.Size = new Size(135, 29);
            btnBulkApply.TabIndex = 3;
            btnBulkApply.Text = "Kijelöltekre";
            btnBulkApply.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1162, 1266);
            Controls.Add(pnlBulkBar);
            Controls.Add(flpProducts);
            Controls.Add(flpCategories);
            Controls.Add(txtSearch);
            Controls.Add(lblSearch);
            Name = "MainForm";
            Text = "Ritmus Shop Készletkezelő";
            Load += MainForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private FlowLayoutPanel flpCategories;
        private FlowLayoutPanel flpProducts;
        private TextBox txtSearch;
        private Label lblSearch;
        private Panel pnlBulkBar;
        private Button btnSelectAll;
        private Label lblBulkInfo;
        private TextBox txtBulkDelta;
        private Button btnBulkApply;
    }
}