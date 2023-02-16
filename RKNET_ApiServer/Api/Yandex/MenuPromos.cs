using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace RKNET_ApiServer.Api.Yandex
{
    public partial class Actions
    {
        /// <summary>
        /// Акционные блюда 
        /// </summary>
        /// <remarks>
        /// Выдача акционных блюд в связке с меню
        /// </remarks>
        /// <param name="restaurantId">код тт</param>
        /// <returns></returns>
        [HttpGet("Yandex/menu/{restaurantId}/promos")]        
        public IActionResult MenuPromos(string restaurantId)
        {
            var isLogging = true;
            var requestName = "получение акционных блюд";
            
            var headersResult = AddHeaders();

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

            // Акционные блюда
            var promoItems = new Api.Yandex.Models.PromoItems();
            return new ObjectResult(promoItems);
        }
    }
}
