namespace RitmusShop_keszletkezelo
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            pnlHeader = new Panel();
            lblTitle = new Label();
            pnlMain = new Panel();
            pnlRight = new Panel();
            flpProducts = new FlowLayoutPanel();
            pnlBulkBar = new Panel();
            btnSelectAll = new Button();
            lblBulkInfo = new Label();
            txtBulkDelta = new TextBox();
            btnBulkApply = new Button();
            pnlSearchBar = new Panel();
            txtSearch = new TextBox();
            lblSearch = new Label();
            pnlLeft = new Panel();
            flpCategories = new FlowLayoutPanel();
            lblFilters = new Label();
            lblSubcategory = new Label();
            cmbSubcategory = new ComboBox();
            pnlHeader.SuspendLayout();
            pnlMain.SuspendLayout();
            pnlRight.SuspendLayout();
            pnlBulkBar.SuspendLayout();
            pnlSearchBar.SuspendLayout();
            pnlLeft.SuspendLayout();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.FromArgb(234, 234, 234);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Padding = new Padding(20, 15, 20, 10);
            pnlHeader.Size = new Size(1162, 70);
            pnlHeader.TabIndex = 1;
            // 
            // lblTitle
            // 
            lblTitle.BackColor = Color.White;
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.Font = new Font("Georgia", 14F, FontStyle.Bold);
            lblTitle.Location = new Point(20, 15);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(1122, 45);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "RitmusShop — Készletkezelő";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pnlMain
            // 
            pnlMain.Controls.Add(pnlRight);
            pnlMain.Controls.Add(pnlLeft);
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.Location = new Point(0, 70);
            pnlMain.Name = "pnlMain";
            pnlMain.Padding = new Padding(20, 0, 20, 20);
            pnlMain.Size = new Size(1162, 730);
            pnlMain.TabIndex = 0;
            // 
            // pnlRight
            // 
            pnlRight.Controls.Add(flpProducts);
            pnlRight.Controls.Add(pnlBulkBar);
            pnlRight.Controls.Add(pnlSearchBar);
            pnlRight.Dock = DockStyle.Fill;
            pnlRight.Location = new Point(300, 0);
            pnlRight.Name = "pnlRight";
            pnlRight.Padding = new Padding(15, 0, 0, 0);
            pnlRight.Size = new Size(842, 710);
            pnlRight.TabIndex = 0;
            // 
            // flpProducts
            // 
            flpProducts.AutoScroll = true;
            flpProducts.AutoSize = true;
            flpProducts.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flpProducts.BackColor = Color.White;
            flpProducts.Dock = DockStyle.Fill;
            flpProducts.FlowDirection = FlowDirection.TopDown;
            flpProducts.Location = new Point(15, 50);
            flpProducts.Name = "flpProducts";
            flpProducts.Padding = new Padding(10);
            flpProducts.Size = new Size(827, 610);
            flpProducts.TabIndex = 0;
            flpProducts.WrapContents = false;
            flpProducts.SizeChanged += flpProducts_SizeChanged;
            // 
            // pnlBulkBar
            // 
            pnlBulkBar.BackColor = Color.White;
            pnlBulkBar.BorderStyle = BorderStyle.FixedSingle;
            pnlBulkBar.Controls.Add(btnSelectAll);
            pnlBulkBar.Controls.Add(lblBulkInfo);
            pnlBulkBar.Controls.Add(txtBulkDelta);
            pnlBulkBar.Controls.Add(btnBulkApply);
            pnlBulkBar.Dock = DockStyle.Bottom;
            pnlBulkBar.Location = new Point(15, 660);
            pnlBulkBar.Name = "pnlBulkBar";
            pnlBulkBar.Padding = new Padding(10, 8, 10, 8);
            pnlBulkBar.Size = new Size(827, 50);
            pnlBulkBar.TabIndex = 1;
            // 
            // btnSelectAll
            // 
            btnSelectAll.BackColor = Color.FromArgb(184, 146, 60);
            btnSelectAll.FlatStyle = FlatStyle.Flat;
            btnSelectAll.ForeColor = Color.White;
            btnSelectAll.Location = new Point(8, 9);
            btnSelectAll.Name = "btnSelectAll";
            btnSelectAll.Size = new Size(155, 30);
            btnSelectAll.TabIndex = 0;
            btnSelectAll.Text = "Összes kijelölése";
            btnSelectAll.UseVisualStyleBackColor = false;
            // 
            // lblBulkInfo
            // 
            lblBulkInfo.Location = new Point(169, 12);
            lblBulkInfo.Name = "lblBulkInfo";
            lblBulkInfo.Size = new Size(200, 25);
            lblBulkInfo.TabIndex = 1;
            lblBulkInfo.Text = "Kijelölt: 0 sor";
            // 
            // txtBulkDelta
            // 
            txtBulkDelta.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtBulkDelta.BorderStyle = BorderStyle.FixedSingle;
            txtBulkDelta.Font = new Font("Segoe UI", 10F);
            txtBulkDelta.Location = new Point(591, 9);
            txtBulkDelta.Name = "txtBulkDelta";
            txtBulkDelta.Size = new Size(80, 30);
            txtBulkDelta.TabIndex = 2;
            txtBulkDelta.Text = "0";
            txtBulkDelta.TextAlign = HorizontalAlignment.Right;
            // 
            // btnBulkApply
            // 
            btnBulkApply.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBulkApply.BackColor = Color.FromArgb(184, 146, 60);
            btnBulkApply.FlatAppearance.BorderSize = 0;
            btnBulkApply.FlatStyle = FlatStyle.Flat;
            btnBulkApply.Font = new Font("Segoe UI", 9F);
            btnBulkApply.ForeColor = Color.White;
            btnBulkApply.Location = new Point(677, 8);
            btnBulkApply.Name = "btnBulkApply";
            btnBulkApply.Size = new Size(135, 31);
            btnBulkApply.TabIndex = 3;
            btnBulkApply.Text = "Alkalmazás";
            btnBulkApply.UseVisualStyleBackColor = false;
            // 
            // pnlSearchBar
            // 
            pnlSearchBar.BackColor = Color.FromArgb(245, 242, 236);
            pnlSearchBar.Controls.Add(txtSearch);
            pnlSearchBar.Controls.Add(lblSearch);
            pnlSearchBar.Dock = DockStyle.Top;
            pnlSearchBar.Location = new Point(15, 0);
            pnlSearchBar.Name = "pnlSearchBar";
            pnlSearchBar.Padding = new Padding(10);
            pnlSearchBar.Size = new Size(827, 50);
            pnlSearchBar.TabIndex = 2;
            // 
            // txtSearch
            // 
            txtSearch.AutoCompleteMode = AutoCompleteMode.Suggest;
            txtSearch.BackColor = Color.FromArgb(234, 234, 234);
            txtSearch.Dock = DockStyle.Fill;
            txtSearch.ForeColor = SystemColors.MenuText;
            txtSearch.Location = new Point(85, 10);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "Szűrés SKU vagy név alapján...";
            txtSearch.Size = new Size(732, 27);
            txtSearch.TabIndex = 0;
            // 
            // lblSearch
            // 
            lblSearch.Dock = DockStyle.Left;
            lblSearch.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblSearch.Location = new Point(10, 10);
            lblSearch.Name = "lblSearch";
            lblSearch.Size = new Size(75, 30);
            lblSearch.TabIndex = 1;
            lblSearch.Text = "Keresés:";
            lblSearch.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlLeft
            // 
            pnlLeft.BackColor = Color.White;
            pnlLeft.Controls.Add(flpCategories);
            pnlLeft.Controls.Add(lblFilters);
            pnlLeft.Dock = DockStyle.Left;
            pnlLeft.Location = new Point(20, 0);
            pnlLeft.Name = "pnlLeft";
            pnlLeft.Padding = new Padding(15);
            pnlLeft.Size = new Size(280, 710);
            pnlLeft.TabIndex = 1;
            // 
            // flpCategories
            // 
            flpCategories.AutoScroll = true;
            flpCategories.Dock = DockStyle.Fill;
            flpCategories.FlowDirection = FlowDirection.TopDown;
            flpCategories.Location = new Point(15, 50);
            flpCategories.Name = "flpCategories";
            flpCategories.Size = new Size(250, 645);
            flpCategories.TabIndex = 0;
            flpCategories.WrapContents = false;
            // 
            // lblFilters
            // 
            lblFilters.Dock = DockStyle.Top;
            lblFilters.FlatStyle = FlatStyle.System;
            lblFilters.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblFilters.Location = new Point(15, 15);
            lblFilters.Name = "lblFilters";
            lblFilters.Size = new Size(250, 35);
            lblFilters.TabIndex = 1;
            lblFilters.Text = "Kategóriák";
            lblFilters.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblSubcategory
            // 
            lblSubcategory.Location = new Point(0, 0);
            lblSubcategory.Name = "lblSubcategory";
            lblSubcategory.Size = new Size(100, 23);
            lblSubcategory.TabIndex = 0;
            // 
            // cmbSubcategory
            // 
            cmbSubcategory.Location = new Point(0, 0);
            cmbSubcategory.Name = "cmbSubcategory";
            cmbSubcategory.Size = new Size(121, 28);
            cmbSubcategory.TabIndex = 0;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(234, 234, 234);
            ClientSize = new Size(1162, 800);
            Controls.Add(pnlMain);
            Controls.Add(pnlHeader);
            MinimumSize = new Size(900, 600);
            Name = "MainForm";
            Text = "RitmusShop Készletkezelő";
            Load += MainForm_Load;
            pnlHeader.ResumeLayout(false);
            pnlMain.ResumeLayout(false);
            pnlRight.ResumeLayout(false);
            pnlRight.PerformLayout();
            pnlBulkBar.ResumeLayout(false);
            pnlBulkBar.PerformLayout();
            pnlSearchBar.ResumeLayout(false);
            pnlSearchBar.PerformLayout();
            pnlLeft.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlHeader;
        private Label lblTitle;
        private Panel pnlMain;
        private Panel pnlLeft;
        private Label lblFilters;
        private FlowLayoutPanel flpCategories;
        private Panel pnlRight;
        private Panel pnlSearchBar;
        private Label lblSearch;
        private TextBox txtSearch;
        private FlowLayoutPanel flpProducts;
        private Panel pnlBulkBar;
        private Button btnSelectAll;
        private Label lblBulkInfo;
        private TextBox txtBulkDelta;
        private Button btnBulkApply;
        private Label lblSubcategory;
        private ComboBox cmbSubcategory;
    }
}