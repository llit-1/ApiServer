using IdentityServer4.Models;

namespace RKNET_ApiServer.OAuth2
{
    public class Resources
    {
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new[]
            {
                new ApiResource
                {
                    Name = "RKNetApi",
                    DisplayName = "RKNet Api Server",
                    Description = "RKNet Апи сервер сети Люди Любят",
                    Scopes = new List<string> 
                    {
                        "read",
                        "write",
                        "menu",
                        "cashClients"
                    }
                    //ApiSecrets = new List<Secret> {new Secret("ProCodeGuide1".Sha256())},
                    //UserClaims = new List<string> {"role"}
                }
            };
        }
    }
}
