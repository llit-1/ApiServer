using Microsoft.AspNetCore.Mvc;

namespace RKNET_ApiServer.Api.Yandex
{
    public partial class Actions
    {
        /// <summary>
        /// Статус заказа
        /// </summary>
        /// <remarks>
        /// Выдача актуального статуса заказе в системе ресторана
        /// </remarks>
        /// <param name="orderId">id заказа в таблице mssql.MarketOrders</param>
        /// <returns></returns>
        [HttpGet("Yandex/order/{orderId}/status")]
        public IActionResult OrderStatus(string orderId)
        {
            var isLogging = true;
            var requestName = "получения статуса заказа";

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

            // проверяем корректность переданного id заказа
            int Id;
            var isCorrectId = int.TryParse(orderId, out Id);
            if (!isCorrectId)
            {
                var errorMessage = $"некорректный идентификатор заказа id={orderId}";
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

            // проверяем наличие заказа в БД
            var order = mssqldb.MarketOrders.FirstOrDefault(o => o.Id == Id);
            if (order == null)
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

            // проверяем соотвествие типа заказа агрегатору
            if (order.OrderTypeCode != 1)
            {
                var errorMessage = $"заказ с идентификатором id = {Id} не является заказом Яндекс Еды";

                Response.StatusCode = 500;
                if (isLogging) ErrorLog(requestName, string.Empty, errorMessage);
                return new ObjectResult(errorMessage);
            }

            // получаем текущий статус заказа
            var status = RKNet_Model.MSSQL.MarketOrder.OrderStatuses.YandexStatuses.FirstOrDefault(s => s.Code == order.StatusCode);
            if (status == null)
            {
                var errorMessage = "остуствует статус у заказа";
                var errors = new List<Api.Yandex.Models.Error>();
                errors.Add(new Api.Yandex.Models.Error
                {
                    code = 100,
                    description = errorMessage
                });

                Response.StatusCode = 500;
                if (isLogging) ErrorLog(requestName, string.Empty, errorMessage);
                return new ObjectResult(errors);
            }

            var yandexStatus = new Api.Yandex.Models.Status();
            yandexStatus.status = status.Value;

            if (status.Code == RKNet_Model.MSSQL.MarketOrder.OrderStatuses.Yandex.CANCELLED.Code)
            {
                yandexStatus.comment = order.CancelReason;
            }
            else
            {
                
                yandexStatus.comment = order.StatusComment;
            }
            
            if(order.StatusUpdatedAt.HasValue)
            {
                yandexStatus.updatedAt = order.StatusUpdatedAt.Value.ToString("yyy-MM-ddTHH:mm:ss.ffffff+03:00");
            }
            
            return new ObjectResult(yandexStatus);
        }
    }
}
