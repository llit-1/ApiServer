namespace RKNET_ApiServer.Api.DeliveryClub.Models
{
    public class OrderWithId
    {
        public string id { get; set; }
        public string status { get; set; }
        public string? shortCode { get; set; }
        public string? pinCode { get; set; }
    }
}
