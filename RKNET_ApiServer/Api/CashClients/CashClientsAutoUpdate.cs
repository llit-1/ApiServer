using Microsoft.AspNetCore.Mvc;

namespace RKNET_ApiServer.Api.CashClients
{
    public partial class Actions
    {
        [HttpGet]
        public IActionResult CashClientsAutoUpdate(bool isEnabled)
        {
            var result = new RKNet_Model.Result<string>();
            try
            {
                var apiSettings = rknetdb.ApiServerSettings.FirstOrDefault();
                apiSettings.CashClientsAutoUpdate = isEnabled;
                rknetdb.ApiServerSettings.Update(apiSettings);
                rknetdb.SaveChanges();                
            }
            catch (Exception ex)
            {
                result.Ok = false;
                result.ErrorMessage = ex.Message;
                result.ExceptionText = ex.ToString();
            }
            result.Data = "Настройки автообновления успешно обновлены";
            return new ObjectResult(result);
        }
    }
}
