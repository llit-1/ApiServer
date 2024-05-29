using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RKNET_ApiServer.Api.Yandex.Models;

namespace RKNET_ApiServer.Api.Yandex
{
    public partial class Actions
    {
        /// <summary>
        /// Обновление заказа
        /// </summary>
        /// <remarks>
        /// обновление заказа в системе ресторана
        /// </remarks>
        /// /// <param name="orderId">id заказа в таблице mssql.MarketOrders</param>
        /// <param name="newOrder">объект Api.Yandex.Models.OrderPost (по модели Яндекса)</param>
        /// <returns></returns>
        [HttpPut("Yandex/order/{orderId}")]
        public IActionResult OrderUpdate(string orderId, Api.Yandex.Models.Order updatedYandexOrder)
        {
            var isLogging = true;
            var requestName = "обновление заказа";            

            // заголовки ответа для Яндекса
            Response.Headers.Add("Cache-Control", "private, max-age=0, no-cache, no-store");
            Response.Headers.Add("Expires", DateTime.UtcNow.AddSeconds(60).ToString("r"));
            Response.Headers.Add("ETag", RandomString(30));
            Response.Headers.Add("Vary", "User-Agent");
            Response.Headers.Add("Pragma", "no-cache");

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
                if (isLogging) if (isLogging) ErrorLog(requestName, string.Empty, errorMessage);
                return new ObjectResult(errors);
            }

            // Тело ответа
            var response = new Api.Yandex.Models.Order.ResponseUpdate();

            // проверяем существование заказа в базе
            int Id;
            var correctId = int.TryParse(orderId, out Id);
            if(!correctId)
            {
                var errorMessage = $"передан некорректный идентификатор заказа id={orderId}";
                var errors = new List<Api.Yandex.Models.Error>();
                errors.Add(new Api.Yandex.Models.Error
                {
                    code = 100,
                    description = errorMessage
                });

                Response.StatusCode = 400;
                if (isLogging) ErrorLog(requestName, string.Empty, errorMessage);
                return new ObjectResult(errors);
            }
            var currentOrder = mssqldb.MarketOrders.FirstOrDefault(o => o.Id == Id);
            if (currentOrder == null)
            {
                var errorMessage = $"заказ с идентификатором id={orderId} отсутствует в базе данных";
                var errors = new List<Api.Yandex.Models.Error>();
                errors.Add(new Api.Yandex.Models.Error
                {
                    code = 100,
                    description = errorMessage
                });

                Response.StatusCode = 404;
                if (isLogging) ErrorLog(requestName, string.Empty, errorMessage);
                return new ObjectResult(errors);
            }

            var restaurantId = currentOrder.TTCode.ToString();

            // проверяем соответствие типа заказа агрегатору
            if (currentOrder.OrderTypeCode != 1)
            {
                var errorMessage = $"заказ с идентификатором id={Id} не является заказом Яндекс Еды";

                Response.StatusCode = 500;
                if (isLogging) ErrorLog(requestName, restaurantId, errorMessage);
                return new ObjectResult(errorMessage);
            }

            // проверяем что заказ еще не выдан
            if (currentOrder.StatusCode == RKNet_Model.MSSQL.MarketOrder.OrderStatuses.Yandex.TAKEN_BY_COURIER.Code | 
                currentOrder.StatusCode == RKNet_Model.MSSQL.MarketOrder.OrderStatuses.Yandex.DELIVERED.Code)
            {
                var errorMessage = $"заказ находится в статусе: {currentOrder.StatusName}";
                var errors = new List<Api.Yandex.Models.Error>();
                errors.Add(new Api.Yandex.Models.Error
                {
                    code = 100,
                    description = errorMessage
                });

                Response.StatusCode = 400;
                if (isLogging) ErrorLog(requestName, restaurantId, errorMessage);
                return new ObjectResult(errors);
            }

            // проверяем что тип заказа с доставкой Яндекса
            if (updatedYandexOrder.discriminator != "yandex")
            {
                var errorMessage = $"неверный дискриминатор ({updatedYandexOrder.discriminator}), принимаются только заказы с Доставкой Яндекса (YandexOrder)";
                var errors = new List<Api.Yandex.Models.Error>();
                errors.Add(new Api.Yandex.Models.Error
                {
                    code = 100,
                    description = errorMessage
                });

                Response.StatusCode = 400;
                if (isLogging) ErrorLog(requestName, restaurantId, errorMessage);
                return new ObjectResult(errors);
            }                        

            // проверяем наличие позиции в меню доставки и на сервере справочников Р-Кипер, соотвестиве названий
            //var menu = new R_Keeper.Actions(rknetdb).GetRkMenu();
            //var rkCodes = RkCodes(menu.Data);

            foreach (var orderItem in updatedYandexOrder.items)
            {
                int itemId;
                var isItemOk = int.TryParse(orderItem.id, out itemId);

                // id блюда имеет неверный формат (не целое число)
                if (!isItemOk)
                {
                    var errorMessage = $"передан неверный id блюда {orderItem.name}: {orderItem.id}";
                    var errors = new List<Api.Yandex.Models.Error>();
                    errors.Add(new Api.Yandex.Models.Error
                    {
                        code = 100,
                        description = errorMessage
                    });

                    Response.StatusCode = 400;
                    if (isLogging) ErrorLog(requestName, restaurantId, errorMessage);
                    return new ObjectResult(errors);
                }

                var menuItem = rknetdb.MenuItems.FirstOrDefault(i => i.Id == int.Parse(orderItem.id));
                // нет блюда в меню
                if (menuItem == null)
                {
                    var errorMessage = $"блюдо {orderItem.name} отсутствует в меню";
                    var errors = new List<Api.Yandex.Models.Error>();
                    errors.Add(new Api.Yandex.Models.Error
                    {
                        code = 100,
                        description = errorMessage
                    });

                    Response.StatusCode = 400;
                    if (isLogging) ErrorLog(requestName, restaurantId, errorMessage);
                    return new ObjectResult(errors);
                }
            }
            
            // тело ответа
            try
            {                
                var oldYandexOrder = Newtonsoft.Json.JsonConvert.DeserializeObject<Api.Yandex.Models.Order>(currentOrder.YandexOrder);

                // если изменился ресторан заказа
                if (oldYandexOrder.restaurantId != updatedYandexOrder.restaurantId)
                {
                    var errorMessage = $"изменился id ресторана: {oldYandexOrder.restaurantId} -> {updatedYandexOrder.restaurantId}. Перенос заказа в другой ресторан невозможен.";
                    var errors = new List<Api.Yandex.Models.Error>();
                    errors.Add(new Api.Yandex.Models.Error
                    {
                        code = 100,
                        description = errorMessage
                    });

                    Response.StatusCode = 400;
                    if (isLogging) ErrorLog(requestName, restaurantId, errorMessage);
                    return new ObjectResult(errors);
                }

                // проверяем наличие позиций заказа в текущих стопах
                var orderItemIds = updatedYandexOrder.items.Select(i => i.id).ToList();
                var menuItems = rknetdb.MenuItems.Where(i => orderItemIds.Contains(i.Id.ToString())).ToList();
                var stops = mssqldb.SkuStops.Where(s => s.Finished == "0").ToList();
                var tt = rknetdb.TTs.FirstOrDefault(t => t.Code == currentOrder.TTCode);
                foreach (var stop in stops)
                {
                    var blockedItem = menuItems.FirstOrDefault(i => i.rkCode == int.Parse(stop.SkuRkCode));
                    var stopCashes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RKNet_Model.MSSQL.SkuStopState>>(stop.CashStates);
                    var stopTTIDs = stopCashes.Select(s => s.TTId);

                    if (blockedItem != null & stopTTIDs.Contains(tt.Id))
                    {
                        var errorMessage = $"блюдо {stop.SkuName} находится в стоп-листе";
                        var errors = new List<Api.Yandex.Models.Error>();
                        errors.Add(new Api.Yandex.Models.Error
                        {
                            code = 100,
                            description = errorMessage
                        });

                        Response.StatusCode = 400;
                        if (isLogging) ErrorLog(requestName, restaurantId, errorMessage);
                        return new ObjectResult(errors);
                    }
                }

                // обновляем заказ
                //currentOrder.Updated = DateTime.Now;
                currentOrder.StatusCode = RKNet_Model.MSSQL.MarketOrder.OrderStatuses.Yandex.POSTPONED.Code;
                currentOrder.StatusName = RKNet_Model.MSSQL.MarketOrder.OrderStatuses.Yandex.POSTPONED.Name;
                currentOrder.StatusComment = "заказ был обновлен Яндексом";
                currentOrder.StatusUpdatedAt = DateTime.Now;
                if (updatedYandexOrder.paymentInfo != null)
                {
                    currentOrder.OrderSum = updatedYandexOrder.paymentInfo.itemsCost;
                }
                currentOrder.YandexOrder = Newtonsoft.Json.JsonConvert.SerializeObject(updatedYandexOrder);
                currentOrder.OrderItems = Newtonsoft.Json.JsonConvert.SerializeObject(PublicMethods.OrderItems(updatedYandexOrder, rknetdb));
                mssqldb.MarketOrders.Update(currentOrder);
                mssqldb.SaveChanges();

                RKNET_ApiServer.Models.Events.OrderUpdate(currentOrder);

                return new ObjectResult(response);                                
            }
            catch (Exception ex)
            {                
                var errors = new List<Api.Yandex.Models.Error>();
                errors.Add(new Api.Yandex.Models.Error
                {
                    code = 100,
                    description = ex.Message
                });

                Response.StatusCode = 500;
                if (isLogging) ErrorLog(requestName, restaurantId, ex.Message);
                return new ObjectResult(errors);
            }
        }        
    }
}
