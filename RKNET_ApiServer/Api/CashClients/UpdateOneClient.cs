using Microsoft.AspNetCore.Mvc;

namespace RKNET_ApiServer.Api.CashClients
{
    public partial class Actions
    {
        [HttpGet]
        // обновление одного кассового клиента
        public IActionResult UpdateOneClient(string clientId, string version)
        {
            var result = new RKNet_Model.Result<string>();

            // проверяем наличие запрошенной версии в таблице версий
            var versions = rknetdb.CashClientVersions.Where(v => v.Version == version).Count();
            if (versions == 0)
            {
                result.Ok = false;
                result.ErrorMessage = "указанной версии не существует";
                return new ObjectResult(result);
            }

            try
            {
                var client = rknetdb.CashClients.FirstOrDefault(c => c.ClientId == clientId);
                if(client == null)
                {
                    result.Ok = false;
                    result.ErrorMessage = "клиент с указанным id отстутсвует в базе данных CashClients";
                }
                else
                {
                    client.UpdateToVersion = version;

                    rknetdb.CashClients.Update(client);
                    rknetdb.SaveChanges();

                    result.Data = "Задание на обновление до версии " + version + " успешно создано.";

                    // вызов события обновления на нужном клиенте через сигнал
                    Models.Events.AutoUpdateClient(clientId);
                }
                
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
