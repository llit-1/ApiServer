using Microsoft.AspNetCore.Mvc;

namespace RKNET_ApiServer.Api.Orders
{
    public partial class Actions
    {
        [HttpGet]
        public IActionResult GetTTOrders(int ttCode)
        {
            var orders = mssqldb.MarketOrders
                .Where(o => o.Created.Date == DateTime.Now.Date)
                .Where(o => o.TTCode == ttCode)
                .ToList();
            return new ObjectResult(orders);
        }

    }
}
