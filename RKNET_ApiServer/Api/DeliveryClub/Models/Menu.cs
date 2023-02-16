namespace RKNET_ApiServer.Api.DeliveryClub.Models
{
    public class Menu
    {
        public string lastUpdatedAt { get; set; }
        public MenuItems menuItems { get; set; }

        public Menu()
        {
            menuItems = new MenuItems();
        }

        public class MenuItems
        {
            public List<Shedule>? shedules { get; set; }
            public List<Category> categories { get; set; }
            public List<Product> products { get; set; }

            public MenuItems()
            {
                categories = new List<Category>();
                products = new List<Product>();
            }

            public class Shedule
            {
                public List<string> categoryIds { get; set; }
                public List<RegularSchedule> regularSchedules { get; set; }

                public Shedule()
                {
                    categoryIds = new List<string>();
                    regularSchedules = new List<RegularSchedule>();
                }

                public class RegularSchedule
                {
                    public string from { get; set; }
                    public string till { get; set; }
                    public string weekDay { get; set; }
                }
            }
            public class Category
            {
                public string id { get; set; }
                public string? parentId { get; set; }
                public List<string>? deliveryTypes { get; set; }
                public string name { get; set; }
            }
            public class Product
            {
                public string id { get; set; }
                public string categoryId { get; set; }
                public List<string>? deliveryTypes { get; set; }
                public string name { get; set; }
                public string? description { get; set; }
                public int price { get; set; }
                public int? vat { get; set; }
                public string? imageUrl { get; set; }
                public bool? byWeight { get; set; }
                public string? weight { get; set; }
                public List<Ingredient>? ingredients { get; set; }
                public List<IngredientsGroup>? ingredientsGroups { get; set; }
                public int? energyValue { get; set; }
                public string? volume { get; set; }

                public class Ingredient
                {
                    public string id { get; set; }
                    public string name { get; set; }
                    public int price { get; set; }
                    public int? vat { get; set; }
                }
                public class IngredientsGroup
                {
                    public string name { get; set; }
                    public List<Ingredient> ingredients { get; set; }
                    public IngredientsGroup()
                    {
                        ingredients = new List<Ingredient>();
                    }
                }
            }
        }

        

        
    }
}
