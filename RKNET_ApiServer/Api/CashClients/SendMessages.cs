using Microsoft.AspNetCore.Mvc;
using RKNET_ApiServer.Api.Yandex.Models;

namespace RKNET_ApiServer.Api.CashClients
{
    public partial class Actions
    {
        [HttpPost]
        public IActionResult SendMessages(MessageOrder messageOrder)
        {
            RKNET_ApiServer.Models.Events.NewMessage(messageOrder);
            return Ok();
        }
    }
}