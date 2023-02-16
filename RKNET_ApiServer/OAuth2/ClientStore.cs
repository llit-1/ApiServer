using IdentityServer4.Models;
using IdentityServer4.Stores;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RKNET_ApiServer.OAuth2
{
    public class ClientStore : IClientStore
    {
        private readonly DB.RknetDbContext rknetdb;        

        public ClientStore(DB.RknetDbContext rknetdbContext)
        {
            rknetdb = rknetdbContext;
        }

        public Task<Client> FindClientByIdAsync(string clientId)
        {
            var clients = new List<Client>();
            var organizations = rknetdb.Organizations.ToList();
            
            // клиент Портала
            var portalClient = new Client
            {
                ClientId = "RKNetWebPortal",
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = new List<Secret> { new Secret("Secret+Portal=2022".Sha256()) },
                AllowedScopes = new List<string> { "menu" }
            };
            clients.Add(portalClient);

            // клиент для Кассовых клиентов
            var cashClient = new Client
            {
                ClientId = "RKNetCashClients",
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = new List<Secret> { new Secret("Secret+Clients=2022".Sha256()) },
                AllowedScopes = new List<string> { "cashClients" }
            };
            clients.Add(cashClient);            

            //// клиенты из таблицы Организаций
            //foreach (var organization in organizations)
            //{
            //    // Яндекс Еда
            //    if (organization.YandexLogin != null)
            //    {
            //        var yandexClient = new Client();
            //        yandexClient.ClientId = organization.YandexLogin;
            //        yandexClient.ClientSecrets = new List<Secret> { new Secret(organization.YandexPassword.Sha256()) };
            //        yandexClient.AllowedGrantTypes = GrantTypes.ClientCredentials;
            //        yandexClient.AllowedScopes = new List<string> { "read", "write" };
            //        clients.Add(yandexClient);
            //    }
            //}

            // клиент Яндекс Еды
            var apiSettings = rknetdb.ApiServerSettings.FirstOrDefault();
            if (apiSettings != null)
            {
                if (!string.IsNullOrEmpty(apiSettings.YandexEdaLogin) && !string.IsNullOrEmpty(apiSettings.YandexEdaPassword))
                {
                    var yandexClient = new Client();

                    yandexClient.ClientId = apiSettings.YandexEdaLogin;
                    yandexClient.AllowedGrantTypes = GrantTypes.ClientCredentials;
                    yandexClient.ClientSecrets = new List<Secret> { new Secret(apiSettings.YandexEdaPassword.Sha256()) };                    
                    yandexClient.AllowedScopes = new List<string> { "read", "write" };

                    clients.Add(yandexClient);                    
                }
            }



            var client = clients.FirstOrDefault(c => c.ClientId == clientId);
            return Task.FromResult(client);          
        }
    }
}
