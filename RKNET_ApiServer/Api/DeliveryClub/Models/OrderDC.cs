namespace RKNET_ApiServer.Api.DeliveryClub.Models
{
    public class OrderDC
    {
        public string originalOrderId { get; set; }
        public bool preOrder { get; set; }
        public string createdAt { get; set; }
        public Customer customer { get; set; }
        public Payment payment { get; set; }
        public string expeditionType { get; set; }
        public Delivery? delivery { get; set; }
        public Pickup? pickup { get; set; }
        public List<OrderProduct> products { get; set; }
        public List<OrderPromotion>? promotions { get; set; }
        public string? comment { get; set; }
        public Price price { get; set; }
        public int personsQuantity { get; set; }
        public CallCenter callCenter { get; set; }
        public Courier? courier { get; set; }
        public PartnerDiscountInfo? partnerDiscountInfo { get; set; }
        public OrderDC()
        {
            customer = new Customer();
            payment = new Payment();
            products = new List<OrderProduct>();
            price = new Price();
            callCenter = new CallCenter();
        }

        public class Customer
        {
            public string name { get; set; }
            public string? phone { get; set; }
            public string? email { get; set; }
        }
        public class Payment
        {
            public string type { get; set; }
            public string? requiredMoneyChange { get; set; }
        }
        public class Delivery
        {
            public string expectedTime { get; set; }
            public Address address { get; set; }
            public Delivery()
            {
                address = new Address();
            }

            public class Address
            {
                public string? subway { get; set; }
                public string? region { get; set; }
                public City city { get; set; }
                public Street? street { get; set; }
                public string? houseNumber { get; set; }
                public string? flatNumber { get; set; }
                public string? entrance { get; set; }
                public string? intercom { get; set; }
                public string? floor { get; set; }
                public Coordinates coordinates { get; set; }
                public Address()
                {
                    city = new City();
                    coordinates = new Coordinates();
                }

                public class City
                {
                    public string name { get; set; }
                    public string? code { get; set; }
                }
                public class Street
                {
                    public string name { get; set; }
                    public string? code { get; set; }
                }
                public class Coordinates
                {
                    public string latitude { get; set; }
                    public string longitude { get; set; }
                }
            }
        }
        public class Pickup
        {
            public string expectedTime { get; set; }
            public string taker { get; set; }
        }
        public class OrderProduct
        {
            public string id { get; set; }
            public string name { get; set; }
            public string price { get; set; }
            public string quantity { get; set; }
            public string? promotionId { get; set; }
            public List<OrderIngredient>? ingredients { get; set; } = new List<OrderIngredient>();

            public class OrderIngredient
            {
                public string id { get; set; }
                public string name { get; set; }
                public int price { get; set; }
                public string? groupName { get; set; }
            }
        }
        public class OrderPromotion
        {
            public string id { get; set; }
            public string name { get; set; }
        }
        public class Price
        {
            public int total { get; set; }
            public int deliveryFee { get; set; }
            public int discount { get; set; }
        }
        public class CallCenter
        {
            public string phone { get; set; }
        }
        public class Courier
        {
            public string name { get; set; }
            public string phone { get; set; }
        }
        public class PartnerDiscountInfo
        {
            public string totalDiscount { get; set; }
            public string partnerPayment { get; set; }
            public string dcPayment { get; set; }
        }
    }
}
