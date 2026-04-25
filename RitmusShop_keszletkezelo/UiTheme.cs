// File: UiTheme.cs
using System.Drawing;

namespace RitmusShop_keszletkezelo
{
    /// <summary>
    /// Központi designtokenek — minden szín és font egy helyen.
    /// Ha finomítani kell, itt módosítsd.
    /// </summary>
    public static class UiTheme
    {
        // Színek a JPG-n látható palettából
        public static readonly Color Background = Color.FromArgb(245, 242, 236);
        public static readonly Color CardBackground = Color.White;
        public static readonly Color CardBorder = Color.FromArgb(220, 215, 205);
        public static readonly Color Accent = Color.FromArgb(184, 146, 60);
        public static readonly Color AccentLight = Color.FromArgb(245, 232, 200);
        public static readonly Color TextPrimary = Color.FromArgb(42, 37, 32);
        public static readonly Color TextSecondary = Color.FromArgb(120, 110, 95);
        public static readonly Color BulkBarBackground = Color.FromArgb(250, 247, 240);

        // Tipográfia
        public static readonly Font HeadingFont = new Font("Cambria", 18F, FontStyle.Bold);
        public static readonly Font SubheadingFont = new Font("Cambria", 12F, FontStyle.Bold);
        public static readonly Font BodyFont = new Font("Segoe UI", 10F);
        public static readonly Font ButtonFont = new Font("Segoe UI", 9F, FontStyle.Regular);
    }
}
