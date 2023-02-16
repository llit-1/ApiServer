namespace RKNET_ApiServer.Api.Yandex.Models
{
    public class PromoItems
    {
        public List<PromoItem> promoItems = new List<PromoItem>();

        // Подклассы
        public class PromoItem
        {
            public string id;
            public string promoId;
        }
    }    
}
