using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace RKNET_ApiServer.Api.Yandex
{
    public partial class Actions
    {
        /// <summary>
        /// Список ТТ
        /// </summary>
        /// <remarks>
        /// Выдача списка заведений партнёра
        /// </remarks>        
        /// <returns></returns>
        [HttpGet("Yandex/restaurants")]
        public IActionResult RestaurantList()
        {
            var isLogging = true;
            var requestName = "получение списка ресторанов";

            // заголовки ответа для Яндекса
            Response.Headers.Add("Cache-Control", "private, max-age=0, no-cache, no-store");
            Response.Headers.Add("Expires", DateTime.UtcNow.AddSeconds(60).ToString("r"));
            Response.Headers.Add("ETag", RandomString(30));
            Response.Headers.Add("Vary", "User-Agent");
            Response.Headers.Add("Pragma", "no-cache");

            var restaurants = new Api.Yandex.Models.Places();

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

            // получаем список точек с включенной Яндекс Едой
            var tts = rknetdb.TTs.Where(t => t.YandexEda).ToList();

            // формируем ответ для Яндекса
            foreach(var tt in tts)
            {
                var place = new Api.Yandex.Models.Places.Place();
                place.id = tt.Code.ToString();
                place.title = tt.Name;
                place.address = tt.Address;

                restaurants.places.Add(place);
            }

            return new ObjectResult(restaurants);
        }
    }
}
