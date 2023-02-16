using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace RKNET_ApiServer.Api.DeliveryClub
{    
    public partial class Actions
    {
        /// <summary>
        /// Корретикровка заказа
        /// </summary>
        /// <remarks>
        /// корректировка в сторону уменьшения кол-ва позиций в заказе
        /// </remarks>
        /// <param name="restaurantId">код тт</param>
        /// <param name="adjustment">корректировка заказа</param>
        /// <returns></returns>
        [HttpPost("deliveryclub/adjustments/{restaurantId}")]
        public IActionResult NewAdjustment(string restaurantId, Api.DeliveryClub.Models.Adjustment adjustment)
        {
            var isLogging = true;
            var requestName = "создание корректировки";

            // Получаем данные ресторана
            var result = GetTT(restaurantId);
            if(!result.Ok)
            {                
                Response.StatusCode = 500;
                if (isLogging) ErrorLog(requestName, restaurantId, result.ErrorMessage);
                return new ObjectResult(result.ErrorMessage);
            }

            var tt = result.Data;

            // Получаем заказ из базы
            int orderId;
            var isIdCorrect = int.TryParse(adjustment.orderId, out orderId);
            
            if(!isIdCorrect)
            {
                var errorMessage = $"некорректный идентификатор заказа: Id={adjustment.orderId}";

                Response.StatusCode = 500;
                if (isLogging) ErrorLog(requestName, restaurantId, result.ErrorMessage);
                return new ObjectResult(errorMessage);
            }

            var order = mssqldb.MarketOrders.FirstOrDefault(o => o.Id == int.Parse(adjustment.orderId));
            if(order == null)
            {
                var errorMessage = $"заказ с Id={adjustment.orderId} отсутствует в базе данных";

                Response.StatusCode = 500;
                if (isLogging) ErrorLog(requestName, restaurantId, result.ErrorMessage);
                return new ObjectResult(errorMessage);
            }

            if(order.OrderTypeCode != 2)
            {
                var errorMessage = $"заказ с Id={adjustment.orderId} не является заказом DeliveryClub";

                Response.StatusCode = 500;
                if (isLogging) ErrorLog(requestName, restaurantId, result.ErrorMessage);
                return new ObjectResult(errorMessage);
            }

            if(order.TTCode != tt.Code)
            {
                var errorMessage = $"ресторан, указанный в корректировке, не соотвествует ресторану заказа";

                Response.StatusCode = 500;
                if (isLogging) ErrorLog(requestName, restaurantId, result.ErrorMessage);
                return new ObjectResult(errorMessage);
            }

            if (order.StatusCode > 5)
            {
                var errorMessage = $"корректировка заказа в статусе {order.StatusName} невозможна";

                Response.StatusCode = 500;
                if (isLogging) ErrorLog(requestName, restaurantId, result.ErrorMessage);
                return new ObjectResult(errorMessage);
            }

            // КОРРЕКТИРОВКА
            try
            {
                var orderItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RKNet_Model.MSSQL.MarketOrder.OrderItem>>(order.OrderItems);

                // удаление позиций
                foreach (var product in adjustment.products)
                {
                    var item = orderItems.FirstOrDefault(i => i.MenuItemId == int.Parse(product.id));
                    if (item != null)
                    {
                        orderItems.Remove(item);
                    }
                }
                order.OrderItems = Newtonsoft.Json.JsonConvert.SerializeObject(orderItems);

                // изменение суммы заказа
                order.OrderSum = adjustment.orderTotalPrice;

                // изменение статуса заказа (только для клиента)
                if(order.StatusCode != 1)
                {
                    order.StatusCode = RKNet_Model.MSSQL.MarketOrder.OrderStatuses.DeliveryClub.POSTPONED.Code;
                    order.StatusName = RKNet_Model.MSSQL.MarketOrder.OrderStatuses.DeliveryClub.POSTPONED.Name;
                }                

                // запись корректировки в базу
                var adjustments = new Dictionary<Guid, Api.DeliveryClub.Models.Adjustment>();

                if (order.DeliveryAdjustments != null)
                {
                    adjustments = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<Guid, Api.DeliveryClub.Models.Adjustment>>(order.DeliveryAdjustments);
                }

                var id = Guid.NewGuid();
                adjustments.Add(id, adjustment);

                var json = Newtonsoft.Json.JsonConvert.SerializeObject(adjustments);
                order.DeliveryAdjustments = json;

                mssqldb.MarketOrders.Update(order);
                mssqldb.SaveChanges();

                if (order.StatusCode != 1)
                {
                    RKNET_ApiServer.Models.Events.OrderUpdate(order);
                }
                else
                {
                    RKNET_ApiServer.Models.Events.NewOrder(order);
                }

                // ответ на запрос с индексом корректирвки в массиве
                var response = new Api.DeliveryClub.Models.AdjustmentResponse
                {
                    id = id.ToString()
                };

                Response.StatusCode = 201;
                return new ObjectResult(response);
            }
            catch(Exception ex)
            {
                Response.StatusCode = 500;
                if (isLogging) ErrorLog(requestName, restaurantId, ex.Message);
                return new ObjectResult(ex.Message);
            }                        
        }
    }
}
