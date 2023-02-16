using IdentityServer4.Models;

namespace RKNET_ApiServer.OAuth2
{
    public class Scopes
    {
        public static IEnumerable<ApiScope> GetApiScopes()
        {
            return new[]
            {
                new ApiScope("read", "Read Access"),
                new ApiScope("write", "Write Access"),
                new ApiScope("menu", "Работа с функционалом Меню"),
                new ApiScope("cashClients", "Работа с кассовыми клиентами")
            };
        }
    }
}
