namespace RKNET_ApiServer.Api.DeliveryClub.Models
{
    public class Adjustment
    {
        public string orderId { get; set; }
        public string? reason { get; set; }
        public List<AdjustmentProduct>? products { get; set; }
        public List<AdjustmentIngredient>? ingredients { get; set; }
        public int amount { get; set; }
        public int orderTotalPrice { get; set; }


        public class AdjustmentProduct
        {
            public string id { get; set; }
        }
        public class AdjustmentIngredient
        {
            public string id { get; set; }
            public string productId { get; set; }
        }
    }
}
