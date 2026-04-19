namespace RitmusShop_keszletkezelo
{
    partial class Form1
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
            btnLoadCategories = new Button();
            lstCategories = new ListBox();
            SuspendLayout();
            // 
            // btnLoadCategories
            // 
            btnLoadCategories.Location = new Point(108, 905);
            btnLoadCategories.Name = "btnLoadCategories";
            btnLoadCategories.Size = new Size(138, 65);
            btnLoadCategories.TabIndex = 0;
            btnLoadCategories.Text = "Kategóriák betöltése";
            btnLoadCategories.UseVisualStyleBackColor = true;
            btnLoadCategories.Click += btnLoadCategories_Click;
            // 
            // lstCategories
            // 
            lstCategories.FormattingEnabled = true;
            lstCategories.Location = new Point(14, 20);
            lstCategories.Name = "lstCategories";
            lstCategories.Size = new Size(459, 804);
            lstCategories.TabIndex = 1;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1162, 1266);
            Controls.Add(lstCategories);
            Controls.Add(btnLoadCategories);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
        }

        #endregion

        private Button btnLoadCategories;
        private ListBox lstCategories;
    }
}
