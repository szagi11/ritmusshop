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
            SuspendLayout();
            // 
            // flpCategories
            // 
            flpCategories.FlowDirection = FlowDirection.TopDown;
            flpCategories.Location = new Point(12, 26);
            flpCategories.Name = "flpCategories";
            flpCategories.Size = new Size(250, 288);
            flpCategories.TabIndex = 3;
            // 
            // flpProducts
            // 
            flpProducts.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            flpProducts.AutoScroll = true;
            flpProducts.FlowDirection = FlowDirection.TopDown;
            flpProducts.Location = new Point(268, 26);
            flpProducts.Name = "flpProducts";
            flpProducts.Size = new Size(850, 1084);
            flpProducts.TabIndex = 4;
            flpProducts.WrapContents = false;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            ClientSize = new Size(1162, 1266);
            Controls.Add(flpProducts);
            Controls.Add(flpCategories);
            Name = "MainForm";
            Text = "Form1";
            Load += MainForm_Load;
            ResumeLayout(false);
        }

        #endregion
        private FlowLayoutPanel flpCategories;
        private FlowLayoutPanel flpProducts;
    }
}
