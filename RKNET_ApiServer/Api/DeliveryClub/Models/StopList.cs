namespace RKNET_ApiServer.Api.DeliveryClub.Models
{
    public class StopList
    {
        public List<StopListItem> stopList { get; set; }
        public StopList()
        {
            stopList = new List<StopListItem>();
        }

        public class StopListItem
        {
            public string type { get; set; }
            public string id { get; set; }
            public string? name { get; set; }
            public StopListItem()
            {
                type = "product";
            }
        }


             
    }
}
