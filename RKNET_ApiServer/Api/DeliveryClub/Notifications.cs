using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace RKNET_ApiServer.Api.DeliveryClub
{
    public partial class Actions
    {
        /// <summary>
        /// Уведомления
        /// </summary>
        /// <remarks>
        /// уведомления со стороны Delivery Club
        /// </remarks>
        /// <param name="restaurantId">код тт</param>
        /// <param name="notification">уведомление</param>
        /// <returns></returns>
        [HttpPost("deliveryclub/notifications/{restaurantId}")]
        public IActionResult NewNotification(string restaurantId, Api.DeliveryClub.Models.Notification notification)
        {
            var isLogging = true;
            var requestName = "отправка уведомления";

            // Получаем данные ресторана
            var result = GetTT(restaurantId);
            if (!result.Ok)
            {
                Response.StatusCode = 500;
                if (isLogging) ErrorLog(requestName, restaurantId, result.ErrorMessage);
                return new ObjectResult(result.ErrorMessage);
            }

            var tt = result.Data;

            // Получаем заказ из базы

            int orderId;
            var isIdCorrect = int.TryParse(notification.orderId, out orderId);

            if (!isIdCorrect)
            {
                var errorMessage = $"некорректный идентификатор заказа: Id={notification.orderId}";

                Response.StatusCode = 500;
                if (isLogging) ErrorLog(requestName, restaurantId, errorMessage);
                return new ObjectResult(errorMessage);
            }

            var order = mssqldb.MarketOrders.FirstOrDefault(o => o.Id == int.Parse(notification.orderId));
            if (order == null)
            {
                var errorMessage = $"заказ с Id={notification.orderId} отсутствует в базе данных";

                Response.StatusCode = 500;
                if (isLogging) ErrorLog(requestName, restaurantId, errorMessage);
                return new ObjectResult(errorMessage);
            }

            if (order.OrderTypeCode != 2)
            {
                var errorMessage = $"заказ с Id={notification.orderId} не является заказом DeliveryClub";

                Response.StatusCode = 500;
                if (isLogging) ErrorLog(requestName, restaurantId, errorMessage);
                return new ObjectResult(errorMessage);
            }

            if (order.TTCode != tt.Code)
            {
                var errorMessage = $"ресторан, указанный в уведомлении ({tt.Name}), не соотвествует ресторану заказа ({order.TTName})";

                Response.StatusCode = 500;
                if (isLogging) ErrorLog(requestName, restaurantId, errorMessage);
                return new ObjectResult(errorMessage);
            }

            // УВЕДОМЛЕНИЕ
            try
            {                               
                // запись уведомления в базу
                var notifications = new Dictionary<Guid, Api.DeliveryClub.Models.Notification>();

                if (order.DeliveryNotifications != null)
                {
                    notifications = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<Guid, Api.DeliveryClub.Models.Notification>>(order.DeliveryNotifications);
                }

                var notificationId = Guid.NewGuid();
                notifications.Add(notificationId, notification);

                var json = Newtonsoft.Json.JsonConvert.SerializeObject(notifications);
                order.DeliveryNotifications = json;

                mssqldb.MarketOrders.Update(order);
                mssqldb.SaveChanges();

                // ответ на запрос
                var response = new Api.DeliveryClub.Models.NotificationResponse
                {
                    id = notificationId.ToString(),
                    orderId = notification.orderId,
                    type = notification.type
                };

                Response.StatusCode = 201;
                return new ObjectResult(response);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                if (isLogging) ErrorLog(requestName, restaurantId, ex.Message);
                return new ObjectResult(ex.Message);
            }
        }
    }
}
