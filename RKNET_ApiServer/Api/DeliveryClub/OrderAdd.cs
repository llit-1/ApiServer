using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RKNET_ApiServer.Api.Yandex.Models;

namespace RKNET_ApiServer.Api.DeliveryClub
{
    public partial class Actions
    {
        /// <summary>
        /// Создание заказа
        /// </summary>
        /// <remarks>
        /// Создание заказа в системе ресторана
        /// </remarks>        
        /// <param name="restaurantId">код тт</param>
        /// <param name="newOrder">объект Api.DeliveryClub.Models.Order (по модели Deliveru Club)</param>
        /// <returns></returns>
        [HttpPost("deliveryclub/orders/{restaurantId}")]
        public IActionResult OrderAdd(string restaurantId, Models.OrderDC newOrder)
        {
            var isLogging = true;
            var requestName = "размещение заказа";

            try
            {
                // Получаем данные ресторана
                var result = GetTT(restaurantId);
                if (!result.Ok)
                {
                    Response.StatusCode = 500;
                    if (isLogging) ErrorLog(requestName, restaurantId, result.ErrorMessage, Newtonsoft.Json.JsonConvert.SerializeObject(result.ErrorMessage), newOrder.originalOrderId);
                    return new ObjectResult(result.ErrorMessage);
                }

                var tt = result.Data;

                // СОЗДАНИЕ ЗАКАЗА
                var response = new Api.DeliveryClub.Models.OrderWithId();

                // проверяем существование заказа с идентификатором деливери в базе (originalOrderId)
                var orderExist = mssqldb.MarketOrders.FirstOrDefault(o => o.DeliveryOrder.Contains(newOrder.originalOrderId));
                if (orderExist != null)
                {
                    var error = new Api.DeliveryClub.Models.RejectingReason
                    {
                        code = "other",
                        message = $"заказ с идентификатором originalOrderId={newOrder.originalOrderId} уже содержится в базе данных"
                    };
                    Response.StatusCode = 400;
                    if (isLogging) ErrorLog(requestName, restaurantId, error.message, Newtonsoft.Json.JsonConvert.SerializeObject(error), newOrder.originalOrderId);
                    return new ObjectResult(error);
                }

                //проверяем что тип заказа с доставкой Delivery Club
                if (newOrder.expeditionType != "pickup")
                {
                    var error = new Api.DeliveryClub.Models.RejectingReason
                    {
                        code = "other",
                        message = $"принимаются заказы только с доставкой курьером Delivery Club или самовывозом из Пекарни"
                    };

                    Response.StatusCode = 400;
                    if (isLogging) ErrorLog(requestName, restaurantId, error.message, Newtonsoft.Json.JsonConvert.SerializeObject(error), newOrder.originalOrderId);
                    return new ObjectResult(error);
                }

                // отключенные позиции в РК
                var disabledItems = rknetdb.MenuItems.Where(i => !i.Enabled).Select(i => i.Id).ToList();
                foreach (var orderItem in newOrder.products)
                {
                    if (disabledItems.Contains(int.Parse(orderItem.id)))
                    {
                        var error = new Api.DeliveryClub.Models.RejectingReason
                        {
                            code = "product_unavailable",
                            message = $"блюдо \"{orderItem.name}\" отключено в Р-Кипер"
                        };

                        Response.StatusCode = 400;
                        if (isLogging) ErrorLog(requestName, restaurantId, error.message, Newtonsoft.Json.JsonConvert.SerializeObject(error), newOrder.originalOrderId);
                        return new ObjectResult(error);
                    }
                }

                // стопы на кассах
                var orderItemIds = newOrder.products.Select(i => i.id).ToList();
                var menuItems = rknetdb.MenuItems.Where(i => orderItemIds.Contains(i.Id.ToString())).ToList();
                var stops = mssqldb.SkuStops.Where(s => s.Finished == "0").ToList();
                foreach (var stop in stops)
                {
                    var blockedItem = menuItems.FirstOrDefault(i => i.rkCode == int.Parse(stop.SkuRkCode));
                    var stopCashes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RKNet_Model.MSSQL.SkuStopState>>(stop.CashStates);
                    var stopTTIDs = stopCashes.Select(s => s.TTId);

                    if (blockedItem != null & stopTTIDs.Contains(tt.Id))
                    {
                        var error = new Api.DeliveryClub.Models.RejectingReason
                        {
                            code = "product_unavailable",
                            message = $"блюдо \"{stop.SkuName}\" находится в стоп-листе на кассах"
                        };

                        Response.StatusCode = 400;
                        if (isLogging) ErrorLog(requestName, restaurantId, error.message, Newtonsoft.Json.JsonConvert.SerializeObject(error), newOrder.originalOrderId);
                        return new ObjectResult(error);
                    }
                }

                // стопы доставки
                var deliveryStops = mssqldb.DeliveryItemStops
                    .Where(s => s.Created.Date == DateTime.Now.Date)
                    .Where(s => s.Cancelled == null)
                    .Where(s => s.TTCode == tt.Code)
                    .Select(s => s.ItemId).ToList();

                foreach (var orderItem in newOrder.products)
                {
                    if (deliveryStops.Contains(int.Parse(orderItem.id)))
                    {
                        var error = new Api.DeliveryClub.Models.RejectingReason
                        {
                            code = "product_unavailable",
                            message = $"блюдо \"{orderItem.name}\" находится в стоп-листе ресторана"
                        };

                        Response.StatusCode = 400;
                        if (isLogging) ErrorLog(requestName, restaurantId, error.message, Newtonsoft.Json.JsonConvert.SerializeObject(error), newOrder.originalOrderId);
                        return new ObjectResult(error);
                    }
                }


                // проверяем наличие позиции в меню доставки и на сервере справочников Р-Кипер, соотвествие названий
                var menu = new R_Keeper.Actions(rknetdb).GetRkMenu();
                var rkCodes = RkCodes(menu.Data);
                var orderItems = new List<RKNet_Model.MSSQL.MarketOrder.OrderItem>();

                foreach (var product in newOrder.products)
                {
                    int itemId;
                    var isItemOk = int.TryParse(product.id, out itemId);
                    // id блюда имеет неверный формат (не целое число)
                    if (!isItemOk)
                    {
                        var error = new Api.DeliveryClub.Models.RejectingReason
                        {
                            code = "product_unavailable",
                            message = $"неверный id блюда {product.name}: {product.id}"
                        };

                        Response.StatusCode = 400;
                        if (isLogging) ErrorLog(requestName, restaurantId, error.message, Newtonsoft.Json.JsonConvert.SerializeObject(error), newOrder.originalOrderId);
                        return new ObjectResult(error);
                    }
                    var menuItem = rknetdb.MenuItems.FirstOrDefault(i => i.Id == int.Parse(product.id));
                    // нет блюда в меню
                    if (menuItem == null)
                    {
                        var error = new Api.DeliveryClub.Models.RejectingReason
                        {
                            code = "product_unavailable",
                            message = $"блюдо {product.name} отсутствует в меню"
                        };

                        Response.StatusCode = 400;
                        if (isLogging) ErrorLog(requestName, restaurantId, error.message, Newtonsoft.Json.JsonConvert.SerializeObject(error), newOrder.originalOrderId);
                        return new ObjectResult(error);
                    }
                    // название позиции в меню отличнается от названия в заказе
                    if (product.name != menuItem.marketName)
                    {
                        var error = new Api.DeliveryClub.Models.RejectingReason
                        {
                            code = "product_unavailable",
                            message = $"блюдо {product.name} в меню ресторана имеет другое название: {menuItem.marketName}"
                        };

                        Response.StatusCode = 400;
                        if (isLogging) ErrorLog(requestName, restaurantId, error.message, Newtonsoft.Json.JsonConvert.SerializeObject(error), newOrder.originalOrderId);
                        return new ObjectResult(error);
                    }
                    // блюдо было удалено или отключено в РК
                    if (!rkCodes.Contains(menuItem.rkCode))
                    {
                        var error = new Api.DeliveryClub.Models.RejectingReason
                        {
                            code = "product_unavailable",
                            message = $"блюдо {product.name} не активно в Р-Кипер"
                        };

                        Response.StatusCode = 400;
                        if (isLogging) ErrorLog(requestName, restaurantId, error.message, Newtonsoft.Json.JsonConvert.SerializeObject(error), newOrder.originalOrderId);
                        return new ObjectResult(error);
                    }

                    // добавляем позицию в OrderItems                
                    var orderItem = new RKNet_Model.MSSQL.MarketOrder.OrderItem();
                    orderItem.MenuItemId = menuItem.Id;
                    orderItem.RkCode = menuItem.rkCode;
                    orderItem.RkName = menuItem.rkName;
                    orderItem.MarketName = product.name;
                    orderItem.MarketPrice = int.Parse(product.price);
                    orderItem.MenuPrice = menuItem.rkDeliveryPrice;
                    orderItem.Quantity = int.Parse(product.quantity);
                    orderItem.TotalCost = orderItem.MarketPrice * orderItem.Quantity;
                    orderItems.Add(orderItem);
                }

                // размещение заказа в БД
                try
                {
                    var marketOrder = new RKNet_Model.MSSQL.MarketOrder();

                    marketOrder.Created = DateTime.Now;
                    marketOrder.TTName = tt.Name;
                    marketOrder.TTCode = tt.Code;
                    var cashStation = rknetdb.CashStations.FirstOrDefault(x => x.TT.Code == marketOrder.TTCode);
                    if (cashStation != null)
                    {
                        marketOrder.FirstMidserver = cashStation.Midserver;
                    }
                    if (newOrder.price != null)
                    {
                        marketOrder.OrderSum = newOrder.price.total;
                    }
                    marketOrder.OrderTypeName = RKNet_Model.MSSQL.MarketOrder.OrderTypes.DeliveryClub.Name;
                    marketOrder.OrderTypeCode = RKNet_Model.MSSQL.MarketOrder.OrderTypes.DeliveryClub.Code;
                    marketOrder.OrderNumber = newOrder.originalOrderId;
                    marketOrder.OrderItems = Newtonsoft.Json.JsonConvert.SerializeObject(orderItems);
                    marketOrder.DeliveryOrder = Newtonsoft.Json.JsonConvert.SerializeObject(newOrder);
                    marketOrder.StatusName = RKNet_Model.MSSQL.MarketOrder.OrderStatuses.DeliveryClub.created.Name;
                    marketOrder.StatusCode = RKNet_Model.MSSQL.MarketOrder.OrderStatuses.DeliveryClub.created.Code;
                    marketOrder.StatusUpdatedAt = DateTime.Now;

                    mssqldb.MarketOrders.Add(marketOrder);
                    mssqldb.SaveChanges();

                    response.id = marketOrder.Id.ToString();
                    response.status = RKNet_Model.MSSQL.MarketOrder.OrderStatuses.DeliveryClub.created.Value;

                    // событие поступления нового заказа
                    RKNET_ApiServer.Models.Events.NewOrder(marketOrder);

                    Response.StatusCode = 201;
                    return new ObjectResult(response);
                }
                catch (Exception ex)
                {
                    var err = $"ошибка сохранения заказа в БД: {ex.Message}";

                    Response.StatusCode = 501;
                    if (isLogging) ErrorLog(requestName, restaurantId, err, Newtonsoft.Json.JsonConvert.SerializeObject(err), newOrder.originalOrderId);
                    return new ObjectResult(ex.Message);
                }
            }
            catch (Exception ex)
            {
                var err = ex.Message;

                Response.StatusCode = 500;
                if (isLogging) ErrorLog(requestName, restaurantId, err, Newtonsoft.Json.JsonConvert.SerializeObject(err), newOrder.originalOrderId);
                return new ObjectResult(ex.Message);
            }
            
        }

        //----------------------------------------------------------
        // получения списка позиций из меню р-кипер
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
    }
}
