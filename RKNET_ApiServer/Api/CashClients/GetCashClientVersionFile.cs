using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace RKNET_ApiServer.Api.CashClients
{
    public partial class Actions
    {
        // скачивание файла обновления (для клиентов до версии 1.2.85) - удалить
        [HttpGet]
        public IActionResult GetCashClientVersionFile(string version)
        {
            var result = new RKNet_Model.Result<RKNet_Model.CashClient.ClientVersion>();
            var clientVersion = rknetdb.CashClientVersions.FirstOrDefault(v => v.Version == version);
            if(clientVersion != null)
            {
                result.Data = clientVersion;
            }
            else
            {
                result.Ok = false;
                result.ErrorMessage = "для указанной версии отсутсвуют файлы обновления в базе данных";
            }
            return new ObjectResult(result);
        }

        // скачивание файла обновления (работает с версии клиента 1.2.85)
        [HttpGet]
        public IActionResult GetCashClientUpdateFile(string version)
        {
            var clientVersion = rknetdb.CashClientVersions.FirstOrDefault(v => v.Version == version);
            if (clientVersion != null)
            {
                if (clientVersion.UpdateFile != null)
                {
                    return File(clientVersion.UpdateFile, "application/zip");
                }
            }
            return null;
        }
    }
}
