using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace RKNET_ApiServer.Api.DeliveryClub
{
    public partial class Actions
    {
        // Меню
        /// <summary>
        /// Меню
        /// </summary>
        /// <remarks>
        /// Информация по заказу
        /// </remarks>
        /// <param name="restaurantId">код тт</param>
        /// /// <param name="id">id заказа в таблице mssql.MarketOrders</param>
        /// <returns></returns>
        [HttpGet("deliveryclub/orders/{restaurantId}/{id}")]
        public IActionResult OrderInfo(string restaurantId, string id)
        {
            var isLogging = true;
            var requestName = "получение информации по заказу";

            // Получаем данные ресторана
            var result = GetTT(restaurantId);
            if (!result.Ok)
            {
                Response.StatusCode = 500;
                if (isLogging) ErrorLog(requestName, restaurantId, result.ErrorMessage);
                return new ObjectResult(result.ErrorMessage);
            }

            var tt = result.Data;

            // ИНФОРМАЦИЯ ПО ЗАКАЗУ

            // проверяем корректность переданного id заказа
            int Id;
            var isOk = int.TryParse(id, out Id);
            if (!isOk)
            {
                var errorMessage = $"некорректный идентификатор заказа id = {id}";

                Response.StatusCode = 500;
                if (isLogging) ErrorLog(requestName, restaurantId, errorMessage);
                return new ObjectResult(errorMessage);
            }

            // проверяем наличие заказа в БД
            var order = mssqldb.MarketOrders.FirstOrDefault(o => o.Id == Id);
            if (order == null)
            {
                var errorMessage = $"заказ с идентификатором id = {id} отсутствует в базе данных";

                Response.StatusCode = 500;
                if (isLogging) ErrorLog(requestName, restaurantId, errorMessage);
                return new ObjectResult(errorMessage);
            }

            // проверяем соотвествие типа заказа агрегатору
            if (order.OrderTypeCode != 2)
            {
                var errorMessage = $"заказ с идентификатором id = {id} не является заказом Delivery Club";
                
                Response.StatusCode = 500;
                if (isLogging) ErrorLog(requestName, restaurantId, errorMessage);
                return new ObjectResult(errorMessage);
            }          

            // получаем текущий статус заказа            
            var status = RKNet_Model.MSSQL.MarketOrder.OrderStatuses.DeliveryClubStatuses.FirstOrDefault(s => s.Code == order.StatusCode);
            if (status == null)
            {
                var errorMessage = "остуствует статус у заказа";

                Response.StatusCode = 500;
                if (isLogging) ErrorLog(requestName, restaurantId, errorMessage);
                return new ObjectResult(errorMessage);
            }

            var response = new Api.DeliveryClub.Models.OrderWithRejectingReason();

            response.id = order.Id.ToString();
            response.status = status.Value;

            if (!string.IsNullOrEmpty(order.CancelReason))
            {
                var reason = new Models.RejectingReason 
                {
                    code = "other",
                    message = order.CancelReason
                };
                response.rejectingReason = reason;
            }                

            return new ObjectResult(response);   
        }
    }
}
