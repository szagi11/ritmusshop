// File: ViewModels/InventoryItemViewModel.cs
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

            // Lookup: OptionItem.Bvin (a kiválasztott érték ID-ja) → olvasható név (pl. "M")
            // Az OptionDTO-ban van Items lista, minden Item-nek Bvin + Name (a "n" mező).
            var labelLookup = options
                .Where(o => o.Items != null)
                .SelectMany(o => o.Items)
                .Where(item => !string.IsNullOrEmpty(item.Bvin))
                .GroupBy(item => item.Bvin)
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
        /// A variáns olvasható nevének összeállítása a Selections listából.
        /// Selections = [{OptionBvin: "...", SelectionData: "ITEM_BVIN"}, ...]
        /// A SelectionData az OptionItemDTO.Bvin értéke, amit a labelLookup-ban
        /// keresünk. Több opció (pl. méret + szín) esetén vesszővel összefűzzük.
        /// </summary>
        private static string BuildVariantDisplayName(
            VariantDTO v, Dictionary<string, string> labelLookup)
        {
            if (v.Selections == null || v.Selections.Count == 0)
                return v.Sku ?? "(névtelen)";

            var labels = v.Selections
                .Select(s => s.SelectionData)
                .Where(id => !string.IsNullOrEmpty(id))
                .Select(id => labelLookup.TryGetValue(id!, out var lbl) ? lbl : id!)
                .Where(lbl => !string.IsNullOrWhiteSpace(lbl))
                .ToList();

            return labels.Count > 0
                ? string.Join(", ", labels)
                : v.Sku ?? "(névtelen)";
        }
    }

    public class VariantViewModel
    {
        public string VariantBvin { get; set; } = null!;
        public string Sku { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public ProductInventoryDTO? Inventory { get; set; }

        /// <summary>Tömeges műveletekhez: be van-e pipálva a sora.</summary>
        public bool IsSelected { get; set; }

        public int QuantityOnHand => Inventory?.QuantityOnHand ?? 0;
    }
}