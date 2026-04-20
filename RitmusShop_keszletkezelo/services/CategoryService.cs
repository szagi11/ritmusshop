using RitmusShop_keszletkezelo.DTO;

namespace RitmusShop_keszletkezelo.Services
{
    public class CategoryService
    {

        public List<string> GetLeafBvinsForMainGroup(
            List<CategoryDto> allCategories, string groupName)
        {
            // 1. Összes "Tánccipő" nevű kategória (lesz 2: Amatőr/Tánccipő és Verseny/Tánccipő)
            var groupCategories = allCategories
                .Where(c => c.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var result = new List<string>();

            foreach (var group in groupCategories)
            {

                CollectLeaves(group.Bvin, allCategories, result);
            }

            return result;
        }

        private void CollectLeaves(string parentBvin, List<CategoryDto> all, List<string> result)
        {
            var children = all.Where(c => c.ParentId == parentBvin).ToList();

            if (children.Count == 0)
            {
                result.Add(parentBvin);
                return;
            }

            foreach (var child in children)
            {
                CollectLeaves(child.Bvin, all, result);
            }
        }
    }
}