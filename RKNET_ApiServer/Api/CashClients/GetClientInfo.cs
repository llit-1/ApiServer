using Microsoft.AspNetCore.Mvc;

namespace RKNET_ApiServer.Api.CashClients
{
    public partial class Actions
    {
        [HttpGet]
        public IActionResult GetClientInfo(string clientId)
        {
            var result = new RKNet_Model.Result<RKNet_Model.CashClient.CashClient>();
            var cashClient = rknetdb.CashClients.FirstOrDefault(c => c.ClientId == clientId);
            if(cashClient == null)
            {
                result.Ok = false;
                result.ErrorMessage = "информация о кассовом клиенте отсутствует в базе данных подключённых клиентов";
            }
            else
            {
                result.Data = cashClient;
            }
            return new ObjectResult(result);
        }
    }
}
