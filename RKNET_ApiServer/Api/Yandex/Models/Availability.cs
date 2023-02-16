namespace RKNET_ApiServer.Api.Yandex.Models
{
    public class Availability
    {
        public List<ItemAvailability> items = new List<ItemAvailability>();
        public List<ModifierAvailability> modifiers = new List<ModifierAvailability>();

        // Подклассы ------------------------------------------------------------------------------
        public class ItemAvailability
        {
            public string itemId;
            public float stock;
        }

        public class ModifierAvailability
        {
            public string modifierId;
            public int stock;
        }
    }    
}
