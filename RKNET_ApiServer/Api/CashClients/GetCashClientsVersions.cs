using Microsoft.AspNetCore.Mvc;

namespace RKNET_ApiServer.Api.CashClients
{
    public partial class Actions
    {
        [HttpGet]
        public IActionResult GetCashClientsVersions()
        {
            var result = new RKNet_Model.Result<List<RKNet_Model.CashClient.ClientVersion>>();
            result.Data = new List<RKNet_Model.CashClient.ClientVersion>();
            try
            {
                var dbVersions = rknetdb.CashClientVersions.Select(v => new { v.Version, v.isActual });
                foreach (var dbVersion in dbVersions)
                {
                    var version = new RKNet_Model.CashClient.ClientVersion()
                    {
                        Version = dbVersion.Version,
                        isActual = dbVersion.isActual
                    };
                    result.Data.Add(version);
                }
            }
            catch (Exception ex)
            {
                result.Ok = false;
                result.ErrorMessage = ex.Message;
                result.ExceptionText = ex.ToString();
            }                       
            return new ObjectResult(result);
        }
    }
}
