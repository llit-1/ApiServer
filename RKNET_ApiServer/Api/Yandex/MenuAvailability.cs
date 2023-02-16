using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace RKNET_ApiServer.Api.Yandex
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
        [HttpGet("Yandex/menu/{restaurantId}/availability")]        
        public IActionResult MenuAvailability(string restaurantId)
        {
            var isLogging = true;
            var requestName = "получение стопов";

            // заголовки ответа для Яндекса
            Response.Headers.Add("Cache-Control", "private, max-age=0, no-cache, no-store");
            Response.Headers.Add("Expires", DateTime.UtcNow.AddSeconds(60).ToString("r"));
            Response.Headers.Add("ETag", RandomString(30));
            Response.Headers.Add("Vary", "User-Agent");
            Response.Headers.Add("Pragma", "no-cache");

            // Получаем данные ресторана
            var result = GetTT(restaurantId);
            if (!result.Ok)
            {
                var errors = new List<Api.Yandex.Models.Error>();
                errors.Add(new Api.Yandex.Models.Error
                {
                    code = 100,
                    description = result.ErrorMessage
                });

                Response.StatusCode = 404;
                if (isLogging) if (isLogging) ErrorLog(requestName, restaurantId, result.ErrorMessage);
                return new ObjectResult(errors);
            }

            var tt = result.Data;            

            // Получаем client_id
            var clientResult = GetClientId(Request);
            if (!clientResult.Ok)
            {
                var errorMessage = "не распознано имя Api клиента";
                var errors = new List<Api.Yandex.Models.Error>();
                errors.Add(new Api.Yandex.Models.Error
                {
                    code = 100,
                    description = errorMessage
                });

                Response.StatusCode = 500;
                if (isLogging) if (isLogging) ErrorLog(requestName, restaurantId, errorMessage);
                return new ObjectResult(errors);
            }
            
            // СТОПЫ

            // отключенные позиции в Р-Кипер
            var availability = new Api.Yandex.Models.Availability();
            var menuItems = rknetdb.MenuItems.ToList();

            foreach(var item in menuItems.Where(i => !i.Enabled))
            {
                var yaItem = new Api.Yandex.Models.Availability.ItemAvailability();
                yaItem.itemId = item.Id.ToString();
                yaItem.stock = 0;
                availability.items.Add(yaItem);
            }

            // стопы на кассах
            var stops = mssqldb.SkuStops.Where(s => s.Finished == "0").ToList();
            

            foreach(var stop in stops)
            {                
                var blockedItem = menuItems.Where(i => i.Enabled).FirstOrDefault(i => i.rkCode == int.Parse(stop.SkuRkCode));
                var stopCashes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RKNet_Model.MSSQL.SkuStopState>>(stop.CashStates);
                var stopTTIDs = stopCashes.Select(s => s.TTId);

                if(blockedItem != null & stopTTIDs.Contains(tt.Id))
                {
                    var yaItem = new Api.Yandex.Models.Availability.ItemAvailability();
                    yaItem.itemId = blockedItem.Id.ToString();
                    yaItem.stock = 0;
                    availability.items.Add(yaItem);
                }
            }

            // стопы доставки
            var deliveryStops = mssqldb.DeliveryItemStops
                .Where(s => s.Created.Date == DateTime.Now.Date)
                .Where(s => s.Cancelled == null)
                .Where(s => s.TTCode == tt.Code);

            foreach(var item in deliveryStops)
            {
                var yaItem = new Api.Yandex.Models.Availability.ItemAvailability();
                yaItem.itemId = item.ItemId.ToString();
                yaItem.stock = 0;
                availability.items.Add(yaItem);
            }

            return new ObjectResult(availability);
        }
    }
}
