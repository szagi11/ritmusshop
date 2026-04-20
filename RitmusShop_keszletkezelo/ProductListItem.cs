using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RitmusShop_keszletkezelo
{
    public partial class ProductListItem : UserControl
    {
        private string _productId; // Eltároljuk az azonosítót a méretek lekéréséhez

        public ProductListItem()
        {
            InitializeComponent();
            pnlVariants.Visible = false;
            this.Height = 40;
            btnExpand.Text = "🔽 Mutat";
        }

        // Ezt a metódust hívjuk meg kintről, amikor betöltjük a terméket
        public void Setup(string productId, string name, string sku)
        {
            _productId = productId;
            lblProductName.Text = name;
            lblSku.Text = sku;
        }

        private void btnExpand_Click(object sender, EventArgs e)
        {

            int alapMagassag = 40;
            int kinyitottMagassag = 250;


            if (pnlVariants.Visible)
            {
                pnlVariants.Visible = false;
                this.Size = new Size(this.Width, alapMagassag);
                btnExpand.Text = "🔽 Mutat";
            }
            else
            {
                pnlVariants.Visible = true;
                this.Size = new Size(this.Width, kinyitottMagassag);
                btnExpand.Text = "🔼 Elrejt";

                if (this.Parent != null)
                {
                    this.Parent.PerformLayout();
                }
            }
        }
    }
}
