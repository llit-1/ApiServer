namespace RKNET_ApiServer.Api.Yandex.Models
{
    public class Places
    {
        public List<Place> places = new List<Place>();
        public class Place
        {
            public string id;
            public string title;
            public string address;
        }        
    }
}
