using Microsoft.AspNetCore.Mvc;

namespace RKNET_ApiServer.Api.CashClients
{
    public partial class Actions
    {        
        [HttpGet]        
        // обновление всех кассовых клиентов
        public IActionResult UpdateAllClients(string version)
        {
            var result = new RKNet_Model.Result<string>();

            // проверяем наличие запрошенной версии в таблице версий
            var versions = rknetdb.CashClientVersions.Where(v => v.Version == version).Count();
            if(versions == 0)
            {
                result.Ok = false;
                result.ErrorMessage = "указанной версии не существует";
                return new ObjectResult(result);
            }

            try
            {
                var cashClients = rknetdb.CashClients;
                foreach (var client in cashClients)
                {
                    if (client.Version != version)
                        client.UpdateToVersion = version;
                    else
                        client.UpdateToVersion = null;
                }

                rknetdb.CashClients.UpdateRange(cashClients);
                rknetdb.SaveChanges();
                               
                // вызов события обновления на клиентах
                Models.Events.AutoUpdateClient();

                result.Data = "Задание на обновление всех кассовых клиентов до версии " + version + " успешно создано.";
                return new ObjectResult(result);
            }
            catch (Exception ex)
            {
                result.Ok = false;
                result.ErrorMessage = ex.Message;
                result.ExceptionText = ex.ToString();
                return new ObjectResult(result);
            }
            
        }        
    }
}
