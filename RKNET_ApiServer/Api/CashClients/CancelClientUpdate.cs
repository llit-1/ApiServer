using Microsoft.AspNetCore.Mvc;

namespace RKNET_ApiServer.Api.CashClients
{
    public partial class Actions
    {
        [HttpGet]
        // отмена обновления одного клиента
        public IActionResult CancelClientUpdate(string clientId)
        {
            var result = new RKNet_Model.Result<string>();
            var cashClient = rknetdb.CashClients.FirstOrDefault(c => c.ClientId == clientId);
            
            if(cashClient == null)
            {
                result.Ok = false;
                result.ErrorMessage = "не обнаружено запланированного обновления для данного клиента";
                return new ObjectResult(result);
            }

            cashClient.UpdateToVersion = null;
            rknetdb.CashClients.Update(cashClient);
            rknetdb.SaveChanges();

            result.Data = "Обновление успешно отменено.";
            return new ObjectResult(result);
        }
    }
}
