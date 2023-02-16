using Microsoft.AspNetCore.Mvc;

namespace RKNET_ApiServer.Api.Yandex
{
    public partial class Actions
    {
        /// <summary>
        /// Информация по заказу
        /// </summary>
        /// <remarks>
        /// Выдача актуальной информации о заказе в системе ресторана
        /// </remarks>
        /// <param name="orderId">id заказа в таблице mssql.MarketOrders</param>
        /// <returns></returns>
        [HttpGet("Yandex/order/{orderId}")]
        public IActionResult OrderInfo(string orderId)
        {
            var isLogging = true;
            var requestName = "получение информации по заказу";

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

            // получаем заказ Яндекса из строки json в БД
            var yandexOrder = Newtonsoft.Json.JsonConvert.DeserializeObject<Api.Yandex.Models.Order>(order.YandexOrder);
            return new ObjectResult(yandexOrder);
        }
    }
}
