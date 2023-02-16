using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace RKNET_ApiServer.Api.DeliveryClub
{
    public partial class Actions
    {
        /// <summary>
        /// Стопы
        /// </summary>
        /// <remarks>
        /// Выдача позиций меню, недоступных для заказа на теущий момент
        /// </remarks>
        /// <param name="restaurantId">код тт</param>
        /// <returns></returns>
        [HttpGet("DeliveryClub/stopLists/{restaurantId}")]
        public IActionResult StopLists(string restaurantId)
        {
            var isLogging = true;
            var requestName = "получение стопов";

            // Получаем данные ресторана
            var result = GetTT(restaurantId);
            if (!result.Ok)
            {
                Response.StatusCode = 500;
                if (isLogging) ErrorLog(requestName, restaurantId, result.ErrorMessage);
                return new ObjectResult(result.ErrorMessage);
            }

            var tt = result.Data;

            // отключенные позиции в РК
            var stopList = new Api.DeliveryClub.Models.StopList();
            var menuItems = rknetdb.MenuItems.ToList();

            foreach (var item in menuItems.Where(i => !i.Enabled))
            {
                var dcItem = new Api.DeliveryClub.Models.StopList.StopListItem();
                dcItem.id = item.Id.ToString();
                dcItem.name = item.marketName;
                stopList.stopList.Add(dcItem);
            }

            // стопы на кассах
            var stops = mssqldb.SkuStops.Where(s => s.Finished == "0").ToList();


            foreach (var stop in stops)
            {
                var blockedItem = menuItems.Where(i => i.Enabled).FirstOrDefault(i => i.rkCode == int.Parse(stop.SkuRkCode));
                var stopCashes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RKNet_Model.MSSQL.SkuStopState>>(stop.CashStates);
                var stopTTIDs = stopCashes.Select(s => s.TTId);

                if (blockedItem != null & stopTTIDs.Contains(tt.Id))
                {
                    var dcItem = new Api.DeliveryClub.Models.StopList.StopListItem();
                    dcItem.id = blockedItem.Id.ToString();
                    dcItem.name = blockedItem.marketName;
                    stopList.stopList.Add(dcItem);
                }
            }

            // стопы доставки
            var deliveryStops = mssqldb.DeliveryItemStops
                .Where(s => s.Created.Date == DateTime.Now.Date)
                .Where(s => s.Cancelled == null)
                .Where(s => s.TTCode == tt.Code);

            foreach (var item in deliveryStops)
            {
                var dcItem = new Api.DeliveryClub.Models.StopList.StopListItem();
                dcItem.id = item.ItemId.ToString();
                dcItem.name = item.ItemMarketName;
                stopList.stopList.Add(dcItem);
            }

            return new ObjectResult(stopList);
        }
    }
}
