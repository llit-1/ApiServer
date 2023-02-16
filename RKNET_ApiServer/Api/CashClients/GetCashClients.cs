using Microsoft.AspNetCore.Mvc;

namespace RKNET_ApiServer.Api.CashClients
{
    public partial class Actions
    {
        [HttpGet]
        public IActionResult GetCashClients()
        {
            var result = new RKNet_Model.Result<List<RKNet_Model.CashClient.CashClient>>();            
            result.Data = rknetdb.CashClients.ToList();
            return new ObjectResult(result);
        }
    }
}
