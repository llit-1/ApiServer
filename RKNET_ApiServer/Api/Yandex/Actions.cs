using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace RKNET_ApiServer.Api.Yandex
{
    [Authorize]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Яндекс ЕДА")]
    public partial class Actions : ControllerBase
    {
        private DB.RknetDbContext rknetdb;
        private DB.MSSQLDBContext mssqldb;

        public Actions(DB.RknetDbContext rknetdbContext, DB.MSSQLDBContext mssqldbContext)
        {
            rknetdb = rknetdbContext;
            mssqldb = mssqldbContext;
        }

        // добавление заголовков HTTP
        private RKNet_Model.Result<string> AddHeaders()
        {
            var result = new RKNet_Model.Result<string>();

            try
            {
                Response.Headers.Add("Cache-Control", "private, max-age=0, no-cache, no-store");
                Response.Headers.Add("Expires", DateTime.UtcNow.AddSeconds(60).ToString("r"));
                Response.Headers.Add("ETag", RandomString(30));
                Response.Headers.Add("Vary", "User-Agent");
                Response.Headers.Add("Pragma", "no-cache");
            }
            catch(Exception ex)
            {
                result.Ok = false;
                result.ErrorMessage = ex.Message;
                result.ExceptionText = ex.ToString();
            }
            return result;
        }

        // получение данных ресторана (тт)
        private RKNet_Model.Result<RKNet_Model.TT.TT> GetTT(string restaurantId)
        {
            var result = new RKNet_Model.Result<RKNet_Model.TT.TT>();

            // проверяем корректность restaurantId
            int ttCode;
            var isCorrectId = int.TryParse(restaurantId, out ttCode);
            if (!isCorrectId)
            {
                result.Ok = false;
                result.ErrorMessage = $"ресторан с restaurantId = {restaurantId} не найден"; // нет точки в базе
                return result;
            }

            // тт нет в базе
            var tt = rknetdb.TTs.FirstOrDefault(t => t.Code == ttCode);

            if (tt == null)
            {
                result.Ok = false;
                result.ErrorMessage = $"ресторан с restaurantId = {restaurantId} не найден";
                return result;
            }

            // тт не подключена к Яндекс Еде
            if (!tt.YandexEda)
            {
                result.Ok = false;
                result.ErrorMessage = $"ресторан с restaurantId = {restaurantId} не подключен к Яндекс Еде";
                return result;
            }
            result.Data = tt;
            return result;
        }

        // генерация случайной строки для заголовка ETag
        private string RandomString(int length)
        {
            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
            var rString = new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());

            return rString;
        }

        // имя Api клиента по токену из заголовка запроса
        private RKNet_Model.Result<string> GetClientId(HttpRequest request)
        {
            var result = new RKNet_Model.Result<string>();

            // получаем токен из заголовка запроса
            var token = request.Headers["Authorization"].ToString();
            token = token.Substring(7, token.Length - 7);

            // получаем имя клиента из токена
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            if (handler.CanReadToken(token))
            {
                var jwtSecurityToken = handler.ReadJwtToken(token);
                result.Data = jwtSecurityToken.Claims.FirstOrDefault(c => c.Type == "client_id").Value;
            }
            else
            {
                result.Ok = false;
                result.ErrorMessage = "не распознано имя Api клиента";                              
            }
            return result;
        }

        // логгирование ошибок
        private void ErrorLog(string requestName, string restaurantId, string errorMessage)
        {
            // имя точки
            var result = GetTT(restaurantId);
            var ttName = string.Empty;
            try
            {
                if (result.Ok) ttName = result.Data.Name;

                // тело запроса
                var requestBody = string.Empty;

                Request.Body.Seek(0, SeekOrigin.Begin);
                using (StreamReader stream = new StreamReader(Request.Body))
                {
                    requestBody = stream.ReadToEnd();
                }

                // лог
                var agregatorError = new zabbix_lib.AgregatorError();
                agregatorError.agregatorName = "ЯндексЕда";
                agregatorError.restaurantId = restaurantId;
                agregatorError.ttName = ttName;
                agregatorError.requestName = requestName;
                agregatorError.requestUrl = $"https://{Request.Host.ToUriComponent()}{Request.Path}";
                agregatorError.requestBody = requestBody;
                agregatorError.responseCode = Response.StatusCode;
                agregatorError.errorMessage = errorMessage;

                RKNET_ApiServer.Models.Events.LogginAgregatorError(agregatorError);
            }
            catch (Exception ex)
            {
                RKNET_ApiServer.Models.Logging.LocalLog($"ошибка RKNET_ApiServer.Api.Yandex.Actions.ErrorLog: {ex.Message}");
            }
        }

        private void ErrorLog(string requestName, string restaurantId, string errorMessage, string? responseBody)
        {
            // имя точки
            var result = GetTT(restaurantId);
            var ttName = string.Empty;
            try
            {
                if (result.Ok) ttName = result.Data.Name;

                // тело запроса
                var requestBody = string.Empty;

                Request.Body.Seek(0, SeekOrigin.Begin);
                using (StreamReader stream = new StreamReader(Request.Body))
                {
                    requestBody = stream.ReadToEnd();
                }

                // лог
                var agregatorError = new zabbix_lib.AgregatorError();
                agregatorError.agregatorName = "ЯндексЕда";
                agregatorError.restaurantId = restaurantId;
                agregatorError.ttName = ttName;
                agregatorError.requestName = requestName;
                agregatorError.requestUrl = $"https://{Request.Host.ToUriComponent()}{Request.Path}";
                agregatorError.requestBody = requestBody;
                agregatorError.responseCode = Response.StatusCode;
                agregatorError.responseBody = responseBody;
                agregatorError.errorMessage = errorMessage;

                RKNET_ApiServer.Models.Events.LogginAgregatorError(agregatorError);
            }
            catch (Exception ex)
            {
                RKNET_ApiServer.Models.Logging.LocalLog($"ошибка RKNET_ApiServer.Api.Yandex.Actions.ErrorLog: {ex.Message}");
            }
        }

        private void ErrorLog(string requestName, string restaurantId, string errorMessage, string? responseBody, string orderNumber)
        {
            // имя точки
            var result = GetTT(restaurantId);
            var ttName = string.Empty;
            try
            {
                if (result.Ok) ttName = result.Data.Name;

                // тело запроса
                var requestBody = string.Empty;
            
                Request.Body.Seek(0, SeekOrigin.Begin);
                using (StreamReader stream = new StreamReader(Request.Body))
                {
                    requestBody = stream.ReadToEnd();
                }
           
                // лог
                var agregatorError = new zabbix_lib.AgregatorError();
                agregatorError.agregatorName = "ЯндексЕда";
                agregatorError.restaurantId = restaurantId;
                agregatorError.ttName = ttName;
                agregatorError.orderNumber = orderNumber;
                agregatorError.requestName = requestName;
                agregatorError.requestUrl = $"https://{Request.Host.ToUriComponent()}{Request.Path}";
                agregatorError.requestBody = requestBody;
                agregatorError.responseCode = Response.StatusCode;
                agregatorError.responseBody = responseBody;
                agregatorError.errorMessage = errorMessage;

                RKNET_ApiServer.Models.Events.LogginAgregatorError(agregatorError);
            }
            catch (Exception ex)
            {
                RKNET_ApiServer.Models.Logging.LocalLog($"ошибка RKNET_ApiServer.Api.Yandex.Actions.ErrorLog: {ex.Message}");
            }
        }
    }
}
