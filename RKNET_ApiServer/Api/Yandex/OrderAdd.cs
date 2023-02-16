using IdentityServer4.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RKNet_Model;
using RKNet_Model.MSSQL;

namespace RKNET_ApiServer.Api.Yandex
{
    public partial class Actions
    {
        bool isLogging;
        string requestName;
        Errors errors;

        /// <summary>
        /// Создание заказа
        /// </summary>
        /// <remarks>
        /// Создание заказа в системе ресторана
        /// </remarks>
        /// <param name="newOrder">объект Api.Yandex.Models.OrderPost (по модели Яндекса)</param>
        /// <returns></returns>
        [HttpPost("Yandex/order")]
        public IActionResult OrderCreate(Api.Yandex.Models.Order newOrder)
        {
            isLogging = true;
            requestName = "размещение заказа";
            errors = new Errors();

            AddHeaders();
                     
            try
            {
                CheckDuplicates(newOrder);                
                var tt = TTData(newOrder);
                var clientId = ClientId();

                CheckRkDisabledItems(newOrder);
                CheckCashStops(newOrder, tt);
                CheckDeliveryStops(newOrder, tt);
                CheckDeliveryType(newOrder);                

                if(errors.criticalErrors.Count == 0)
                {
                    Response.StatusCode = 200;

                    var result = SaveOrder(newOrder, tt);                    

                    if (result.Ok)
                    {
                        var marketOrder = result.Data;                        
                        var response = new Api.Yandex.Models.Order.Response();
                        response.result = "ok";
                        response.orderId = result.Data.Id.ToString();

                        if (marketOrder.StatusCode == MarketOrder.OrderStatuses.Yandex.CANCELLED.Code)
                        {
                            ErrorLog(requestName, marketOrder.TTCode.ToString(), marketOrder.CancelReason, Newtonsoft.Json.JsonConvert.SerializeObject(response), newOrder.eatsId);
                        }
                        else
                        {
                            foreach (var err in errors.infoErrors)
                            {
                                if (isLogging) ErrorLog(requestName, newOrder.restaurantId, err, Newtonsoft.Json.JsonConvert.SerializeObject(response), newOrder.eatsId);
                            }
                        }

                        return new ObjectResult(response);
                    }
                    else
                    {
                        var yaErrors = new List<Api.Yandex.Models.Error>();
                        yaErrors.Add(new Api.Yandex.Models.Error
                        {
                            code = 100,
                            description = result.ErrorMessage
                        });

                        Response.StatusCode = 501;
                        if (isLogging) ErrorLog(requestName, newOrder.restaurantId, result.ErrorMessage, Newtonsoft.Json.JsonConvert.SerializeObject(yaErrors), newOrder.eatsId);
                        return new ObjectResult(yaErrors);
                    }
                }
                else
                {
                    var yaErrors = new List<Api.Yandex.Models.Error>();
                    yaErrors.Add(new Api.Yandex.Models.Error
                    {
                        code = 100,
                        description = errors.criticalErrors.First()
                    });

                    Response.StatusCode = 400;
                    if (isLogging) ErrorLog(requestName, newOrder.restaurantId, errors.criticalErrors.First(), Newtonsoft.Json.JsonConvert.SerializeObject(yaErrors), newOrder.eatsId);
                    return new ObjectResult(yaErrors);
                }
            }
            catch(Exception ex)
            {
                var errorMessage = $"ошибка RKNET_ApiServer.Api.Yandex.Actions.OrderCreate: {ex.Message}";
                var yaErrors = new List<Api.Yandex.Models.Error>();
                yaErrors.Add(new Api.Yandex.Models.Error
                {
                    code = 100,
                    description = errorMessage
                });

                Response.StatusCode = 500;
                if (isLogging) { ErrorLog(requestName, newOrder.restaurantId, errorMessage,Newtonsoft.Json.JsonConvert.SerializeObject(yaErrors), newOrder.eatsId); }
                RKNET_ApiServer.Models.Logging.LocalLog(errorMessage);

                return new ObjectResult(yaErrors);
            }                
        }

        //----------------------------------------------------------
        // КЛАССЫ
        //----------------------------------------------------------
        private class Errors
        {
            public List<string> infoErrors = new List<string>();        // заказ принимается со статусом "новый"
            public List<string> cancelErrors = new List<string>();      // заказ принимается со статусом "отменён"
            public List<string> criticalErrors = new List<string>();    // заказ не принимается
        }

        //----------------------------------------------------------
        // МЕТОДЫ
        //----------------------------------------------------------

        // Размещение заказа в БД
        private RKNet_Model.Result<MarketOrder> SaveOrder(Models.Order newOrder, RKNet_Model.TT.TT tt)
        {
            var result = new RKNet_Model.Result<MarketOrder>();
            try
            {

                var marketOrder = new RKNet_Model.MSSQL.MarketOrder();
                var orderItems = OrderItems(newOrder);               

                marketOrder.Created = DateTime.Now;
                marketOrder.TTName = tt.Name;
                marketOrder.TTCode = tt.Code;
                if (newOrder.paymentInfo != null)
                {
                    marketOrder.OrderSum = newOrder.paymentInfo.itemsCost;
                }
                marketOrder.OrderTypeName = RKNet_Model.MSSQL.MarketOrder.OrderTypes.Yandex.Name;
                marketOrder.OrderTypeCode = RKNet_Model.MSSQL.MarketOrder.OrderTypes.Yandex.Code;
                marketOrder.OrderNumber = newOrder.eatsId;
                marketOrder.OrderItems = Newtonsoft.Json.JsonConvert.SerializeObject(orderItems);
                marketOrder.YandexOrder = Newtonsoft.Json.JsonConvert.SerializeObject(newOrder);
                marketOrder.StatusUpdatedAt = DateTime.Now;

                if(errors.cancelErrors.Count == 0)
                {
                    marketOrder.StatusName = RKNet_Model.MSSQL.MarketOrder.OrderStatuses.Yandex.NEW.Name;
                    marketOrder.StatusCode = RKNet_Model.MSSQL.MarketOrder.OrderStatuses.Yandex.NEW.Code;
                }
                else
                {
                    marketOrder.StatusName = RKNet_Model.MSSQL.MarketOrder.OrderStatuses.Yandex.CANCELLED.Name;
                    marketOrder.StatusCode = RKNet_Model.MSSQL.MarketOrder.OrderStatuses.Yandex.CANCELLED.Code;

                    marketOrder.StatusComment = "автоматически отменён";
                    marketOrder.CancelReason = errors.cancelErrors.First();
                }                
                
                mssqldb.MarketOrders.Add(marketOrder);
                mssqldb.SaveChanges();

                if (marketOrder.Id > 0)
                {
                    // событие поступление нового заказа - отсюда идут уведомления на кассу
                    RKNET_ApiServer.Models.Events.NewOrder(marketOrder);
                    result.Data = marketOrder;
                }
                else
                {
                    result.Ok = false;
                    result.ErrorMessage = $"ошибка размещения заказа в БД: не присвоен внутренний Id заказу";                                        
                }
            }
            catch (Exception ex)
            {
                result.Ok = false;
                result.ErrorMessage = $"ошибка размещения заказа в БД: {ex.Message}";
            }

            return result;
        }

        // Список позиций заказа
        private List<RKNet_Model.MSSQL.MarketOrder.OrderItem> OrderItems(Api.Yandex.Models.Order newOrder)
        {
            var orderItems = new List<RKNet_Model.MSSQL.MarketOrder.OrderItem>();
            try
            {                
                var menu = new R_Keeper.Actions(rknetdb).GetRkMenu();
                var rkCodes = RkCodes(menu.Data);

                var firstItem = true;
                foreach (var yandexItem in newOrder.items)
                {
                    int itemId;
                    var isItemOk = int.TryParse(yandexItem.id, out itemId);
                    var menuItem = new RKNet_Model.Menu.Item();

                    // проверяем корректность переданного id блюда (целое число)
                    if (isItemOk)
                    {
                        menuItem = rknetdb.MenuItems.FirstOrDefault(i => i.Id == itemId);
                    }
                    // пытаемся обработать заказ по названию позиции
                    else
                    {
                        menuItem = rknetdb.MenuItems.FirstOrDefault(i => i.marketName == yandexItem.name);
                        if (menuItem != null & firstItem)
                        {
                            errors.infoErrors.Add($"переданы неверные id блюд по заказу eatsId={newOrder.eatsId} ({yandexItem.name}: {yandexItem.id} вместо {menuItem.Id}, всего {newOrder.items.Count} позиций)");                            
                            firstItem = false;
                        }
                    }

                    // добавляем позицию в OrderItems
                    if (menuItem != null)
                    {                                    
                        var orderItem = new RKNet_Model.MSSQL.MarketOrder.OrderItem();
                        orderItem.MenuItemId = menuItem.Id;
                        orderItem.RkName = menuItem.rkName;
                        orderItem.MarketName = yandexItem.name;
                        orderItem.MarketPrice = (int)yandexItem.price;
                        orderItem.MenuPrice = (int)menuItem.rkDeliveryPrice;
                        orderItem.Quantity = (int)yandexItem.quantity;
                        orderItem.TotalCost = orderItem.MarketPrice * orderItem.Quantity;
                        orderItems.Add(orderItem);

                        // название позиции в меню отличается от названия в заказе
                        if (yandexItem.name != menuItem.marketName)
                        {
                            errors.infoErrors.Add($"блюдо {yandexItem.name} в меню ресторана имеет другое название: {menuItem.marketName}");
                        }

                        // блюдо было удалено или отключено в РК
                        if (!rkCodes.Contains(menuItem.rkCode))
                        {
                            errors.cancelErrors.Add($"блюдо {yandexItem.name} не активно в Р-Кипер");
                        }
                    }                    
                    else
                    {
                        errors.cancelErrors.Add($"блюдо {yandexItem.name} отсутствует в меню доставки");
                    }                                      
                }
            }
            catch(Exception ex)
            {
                RKNET_ApiServer.Models.Logging.LocalLog($"ошибка RKNET_ApiServer.Api.Yandex.Actions.OrderItems (OrderAdd): {ex.Message}");
            }
            return orderItems;
        }               

        // Cписок кодов позиций из меню Р-Кипер
        private List<int> RkCodes(List<RKNet_Model.Menu.rkMenuItem> rkItems)
        {
            var rkCodes = new List<int>();
            foreach (var item in rkItems)
            {
                if (!item.isCategory)
                {
                    rkCodes.Add(item.rkCode);
                }
                else
                {
                    var subItems = RkCodes(item.rkMenuItems);
                    rkCodes.AddRange(subItems);
                }
            }
            return rkCodes;
        }
        
        // Проеряем заказ на дубли
        private void CheckDuplicates(Api.Yandex.Models.Order newOrder)
        {
            var orderExist = mssqldb.MarketOrders.FirstOrDefault(o => o.YandexOrder.Contains(newOrder.eatsId));
            if (orderExist != null)
            {
                errors.criticalErrors.Add($"заказ с идентификатором eatsId={newOrder.eatsId} уже содержится в базе данных");
            }
        }

        // ТТ
        private RKNet_Model.TT.TT TTData(Api.Yandex.Models.Order newOrder)
        {
            var result = GetTT(newOrder.restaurantId);
            if (!result.Ok)
            {
                errors.criticalErrors.Add(result.ErrorMessage);
                return null;
            }
            else
            {
                return result.Data;
            }
        }

        // Получем ID клиента
        private string ClientId()
        {
            var clientResult = GetClientId(Request);
            if (!clientResult.Ok)
            {
                RKNET_ApiServer.Models.Logging.LocalLog($"ошибка RKNET_ApiServer.Api.Yandex.Actions.ClientId (OrderAdd): не распознано имя Api клиента");
                return string.Empty;
            }
            else
            {
                return clientResult.Data;
            }
        }

        // Отключённые позиции в РК
        private void CheckRkDisabledItems(Api.Yandex.Models.Order newOrder)
        {
            var disabledItems = rknetdb.MenuItems.Where(i => !i.Enabled).Select(i => i.marketName).ToList();
            foreach (var orderItem in newOrder.items)
            {
                if (disabledItems.Contains(orderItem.name))
                {
                    errors.cancelErrors.Add($"блюдо {orderItem.name} отключено в Р-Кипер");
                }
            }
        }
    
        // СТОПы на кассах
        private void CheckCashStops(Api.Yandex.Models.Order newOrder, RKNet_Model.TT.TT tt)
        {
            var orderItemIds = newOrder.items.Select(i => i.id).ToList();
            var menuItems = rknetdb.MenuItems.Where(i => orderItemIds.Contains(i.Id.ToString())).ToList();
            var stops = mssqldb.SkuStops.Where(s => s.Finished == "0").ToList();
            foreach (var stop in stops)
            {
                var blockedItem = menuItems.FirstOrDefault(i => i.rkCode == int.Parse(stop.SkuRkCode));
                var stopCashes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RKNet_Model.MSSQL.SkuStopState>>(stop.CashStates);
                var stopTTIDs = stopCashes.Select(s => s.TTId);

                if (blockedItem != null & stopTTIDs.Contains(tt.Id))
                {
                    errors.cancelErrors.Add( $"блюдо {stop.SkuName} находится в стоп-листе на кассах");
                }
            }
        }

        // СТОПы доставки
        private void CheckDeliveryStops(Api.Yandex.Models.Order newOrder, RKNet_Model.TT.TT tt)
        {
            var deliveryStops = mssqldb.DeliveryItemStops
                    .Where(s => s.Created.Date == DateTime.Now.Date)
                    .Where(s => s.Cancelled == null)
                    .Where(s => s.TTCode == tt.Code)
                    .Select(s => s.ItemMarketName).ToList();

            foreach (var orderItem in newOrder.items)
            {
                if (deliveryStops.Contains(orderItem.name))
                {
                    errors.cancelErrors.Add($"блюдо {orderItem.name} находится в стоп-листе ресторана");
                }
            }
        }

        // Кто доставляет
        private void CheckDeliveryType(Api.Yandex.Models.Order newOrder)
        {
            if (newOrder.discriminator != "yandex")
            {
                errors.cancelErrors.Add("неверный дискриминатор, принимаются заказы только с Доставкой Яндекса");
            }
        }
    }
}
