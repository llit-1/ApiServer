namespace RKNET_ApiServer.Api.Yandex.Models
{
    public class Menu
    {
        public List<Category> categories { get; set; } = new List<Category>();
        public List<Item> items { get; set; } = new List<Item>();
        public string lastChange { get; set; } = String.Empty;

        // Подклассы -----------------------------------------------------------------
        public class Category
        {
            public string id;
            public string? parentId;
            public string name;
            public int sortOrder;
            public List<CategoryImage> images = new List<CategoryImage>();
        }

        public class CategoryImage
        {
            public string url;
            public string updatedAt;
        }

        public class Item
        {
            public string id;
            public string categoryId;
            public string name;
            public string? description;
            public double price;
            public int vat;
            public bool isCatchweight = false;
            public int measure;
            public float? weightQuantum;
            public string measureUnit;
            public int sortOrder;
            public List<ModifierGroup> modifierGroups = new List<ModifierGroup>();
            public List<ItemImage> images = new List<ItemImage>();
        }

        public class ItemImage
        {
            public string hash;
            public string url;
        }

        public class ModifierGroup
        {
            public string id;
            public string name;
            public List<Modifier> modifiers = new List<Modifier>();
            public int minSelectedModifiers;
            public int maxSelectedModifiers;
            public int sortOrder;
        }

        public class Modifier
        {
            public string id;
            public string name;
            public double price;
            public int? vat;
            public int minAmount;
            public int maxAmount;
        }
    }    
}
