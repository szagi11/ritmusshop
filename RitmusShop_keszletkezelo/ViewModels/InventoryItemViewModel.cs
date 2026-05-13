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
        public string CategoryDisplay { get; set; } = string.Empty;
        public ProductInventoryDTO? MainInventory { get; set; }
        public List<VariantViewModel> Variants { get; set; } = new();
        public bool IsSelected { get; set; }

        public bool HasVariants => Variants.Count > 0;

        public int TotalQuantityOnHand =>
            HasVariants
                ? Variants.Sum(v => v.Inventory?.QuantityOnHand ?? 0)
                : (MainInventory?.QuantityOnHand ?? 0);

        public int TotalQuantityReserved =>
            HasVariants
                ? Variants.Sum(v => v.Inventory?.QuantityReserved ?? 0)
                : (MainInventory?.QuantityReserved ?? 0);

        public int TotalAvailable => TotalQuantityOnHand - TotalQuantityReserved;

        public static InventoryItemViewModel Build(
            ProductDTO product,
            List<VariantDTO> variants,
            List<ProductInventoryDTO> inventoryRows,
            List<OptionDTO> options,
            List<CategorySnapshotDTO> productCategories,
            List<CategorySnapshotDTO> allCategories)
        {
            variants ??= new List<VariantDTO>();
            inventoryRows ??= new List<ProductInventoryDTO>();
            options ??= new List<OptionDTO>();
            productCategories ??= new List<CategorySnapshotDTO>();
            allCategories ??= new List<CategorySnapshotDTO>();

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
                CategoryDisplay = ResolveCategoryDisplay(productCategories, allCategories),
                MainInventory = inventoryRows
                    .FirstOrDefault(i => string.IsNullOrEmpty(i.VariantId))
            };

            vm.Variants = variants
                .Select(v => new VariantViewModel
                {
                    VariantBvin = v.Bvin ?? string.Empty,
                    Sku = v.Sku ?? string.Empty,
                    DisplayName = BuildVariantDisplayName(v, labelLookup),
                    Inventory = inventoryRows.FirstOrDefault(i => i.VariantId == v.Bvin),
                    Source = v
                })
                .ToList();

            return vm;
        }

        public IEnumerable<ProductInventoryDTO> GetSelectedInventories()
        {
            if (!HasVariants)
            {
                if (IsSelected && MainInventory != null)
                    yield return MainInventory;
            }
            else
            {
                foreach (var variant in Variants.Where(v => v.IsSelected && v.Inventory != null))
                    yield return variant.Inventory!;
            }
        }

        public bool HasAnySelection() => GetSelectedInventories().Any();
        public int CountSelected() => GetSelectedInventories().Count();

        // Kereső
        public bool MatchesFilter(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return true;
            return ContainsCi(ProductName, query)
                || ContainsCi(Sku, query)
                || Variants.Any(v => ContainsCi(v.Sku, query) || ContainsCi(v.DisplayName, query));
        }

        private static bool ContainsCi(string? haystack, string needle) =>
            haystack != null && haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;


        internal static string ResolveCategoryDisplay(
            List<CategorySnapshotDTO> productCats,
            List<CategorySnapshotDTO> allCats)
        {
            if (productCats.Count == 0) return string.Empty;

            var allByBvin = allCats
                .Where(c => !string.IsNullOrEmpty(c.Bvin))
                .ToDictionary(c => c.Bvin!, c => c);

            int Depth(CategorySnapshotDTO cat)
            {
                int depth = 1;
                var current = cat;
                int safety = 0;
                while (!string.IsNullOrEmpty(current.ParentId)
                    && allByBvin.TryGetValue(current.ParentId, out var parent))
                {
                    depth++;
                    current = parent;
                    if (++safety > 10) break;
                }
                return depth;
            }

            var withDepth = productCats
                .Select(c => (Cat: c, Depth: Depth(c)))
                .OrderByDescending(x => x.Depth)
                .ToList();

            var second = withDepth.FirstOrDefault(x => x.Depth == 2);
            if (second.Cat != null) return second.Cat.Name ?? string.Empty;

            var first = withDepth.FirstOrDefault();
            return first.Cat?.Name ?? string.Empty;
        }

        internal static string BuildVariantDisplayName(
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
                : (!string.IsNullOrEmpty(v.Sku) ? v.Sku : "(ismeretlen)");
        }

        internal static string NormalizeGuid(string? raw)
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

        /// <summary>
        /// Az eredeti VariantDTO; a halasztott opciók-betöltés után a
        /// DisplayName újraépítéséhez kell.
        /// </summary>
        public VariantDTO? Source { get; set; }

        public int QuantityOnHand => Inventory?.QuantityOnHand ?? 0;
        public int QuantityReserved => Inventory?.QuantityReserved ?? 0;
        public int Available => QuantityOnHand - QuantityReserved;
    }
}