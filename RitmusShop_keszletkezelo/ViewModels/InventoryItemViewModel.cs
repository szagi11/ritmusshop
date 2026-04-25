using System;
using System.Collections.Generic;
using System.Linq;
using Hotcakes.CommerceDTO.v1.Catalog;

namespace RitmusShop_keszletkezelo.ViewModels
{
    public class InventoryItemViewModel
    {
        public string ProductBvin { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string Sku { get; set; } = null!;
        public ProductInventoryDTO? MainInventory { get; set; }
        public List<VariantViewModel> Variants { get; set; } = new();
        public bool IsSelected { get; set; }

        public bool HasVariants => Variants.Count > 0;

        public int TotalQuantityOnHand =>
            HasVariants
                ? Variants.Sum(v => v.Inventory?.QuantityOnHand ?? 0)
                : (MainInventory?.QuantityOnHand ?? 0);

        public static InventoryItemViewModel Build(
            ProductDTO product,
            List<VariantDTO> variants,
            List<ProductInventoryDTO> inventoryRows,
            List<OptionDTO> options)
        {
            variants ??= new List<VariantDTO>();
            inventoryRows ??= new List<ProductInventoryDTO>();
            options ??= new List<OptionDTO>();

            // GUID-normalizált lookup tábla: a Hotcakes API a SelectionData-t
            // kötőjelek nélkül, az OptionItem.Bvin-t kötőjelekkel adja vissza,
            // ezért egységes "N" formátumra ("32 karakter, kötőjel nélkül")
            // hozzuk mindkét oldalt, hogy összepasszintsanak.
            // IsLabel=true elemek placeholderek ("- Válasszon -"), kihagyjuk.
            var labelLookup = options
                .Where(o => o.Items != null)
                .SelectMany(o => o.Items)
                .Where(item => !item.IsLabel && !string.IsNullOrEmpty(item.Bvin))
                .GroupBy(item => NormalizeGuid(item.Bvin))
                .ToDictionary(g => g.Key, g => g.First().Name ?? string.Empty);

            var vm = new InventoryItemViewModel
            {
                ProductBvin = product.Bvin ?? string.Empty,
                ProductName = product.ProductName ?? string.Empty,
                Sku = product.Sku ?? string.Empty,
                MainInventory = inventoryRows
                    .FirstOrDefault(i => string.IsNullOrEmpty(i.VariantId))
            };

            vm.Variants = variants
                .Select(v => new VariantViewModel
                {
                    VariantBvin = v.Bvin ?? string.Empty,
                    Sku = v.Sku ?? string.Empty,
                    DisplayName = BuildVariantDisplayName(v, labelLookup),
                    Inventory = inventoryRows.FirstOrDefault(i => i.VariantId == v.Bvin)
                })
                .ToList();

            return vm;
        }

        /// <summary>
        /// A variáns olvasható neve a Selections lista alapján.
        /// A SelectionData-t normalizáljuk, hogy ugyanabban a formátumban
        /// keressük, mint amilyenben a labelLookup kulcsai vannak.
        /// </summary>
        private static string BuildVariantDisplayName(
            VariantDTO v, Dictionary<string, string> labelLookup)
        {
            if (v.Selections == null || v.Selections.Count == 0)
                return "(névtelen)";

            var labels = v.Selections
                .Select(s => NormalizeGuid(s.SelectionData))
                .Where(id => !string.IsNullOrEmpty(id))
                .Select(id => labelLookup.TryGetValue(id, out var lbl) && !string.IsNullOrWhiteSpace(lbl)
                    ? lbl
                    : null)
                .Where(lbl => lbl != null)
                .ToList();

            return labels.Count > 0
                ? string.Join(", ", labels!)
                : "(ismeretlen)";
        }

        /// <summary>
        /// Egységes GUID formára hozza az inputot. A Hotcakes API egyes helyeken
        /// kötőjelekkel ("D" formátum, 36 char), máshol kötőjelek nélkül 
        /// ("N" formátum, 32 char) adja vissza a GUID-okat. Itt mindkét formát
        /// 32 karakteres kisbetűs alakra hozzuk.
        /// </summary>
        private static string NormalizeGuid(string? raw)
        {
            if (string.IsNullOrEmpty(raw)) return string.Empty;
            if (Guid.TryParse(raw, out var g)) return g.ToString("N");
            return raw.Replace("-", string.Empty).ToLowerInvariant();
        }
    }

    public class VariantViewModel
    {
        public string VariantBvin { get; set; } = null!;
        public string Sku { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public ProductInventoryDTO? Inventory { get; set; }
        public bool IsSelected { get; set; }

        public int QuantityOnHand => Inventory?.QuantityOnHand ?? 0;
    }
}