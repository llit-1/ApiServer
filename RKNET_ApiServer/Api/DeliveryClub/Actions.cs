using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace RKNET_ApiServer.Api.DeliveryClub
{
    [BasicAuth.BasicAuthorization]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Delivery Club")]
    public partial class Actions : ControllerBase
    {
        private DB.RknetDbContext rknetdb;
        private DB.MSSQLDBContext mssqldb;

        public Actions(DB.RknetDbContext rknetdbContext, DB.MSSQLDBContext mssqldbContext)
        {
            rknetdb = rknetdbContext;
            mssqldb = mssqldbContext;
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

            // получаем client_id
            var clientName = User.Identity.Name;
            if (clientName == null)
            {
                result.Ok = false;
                result.ErrorMessage = "не распознано имя Api клиента";
                return result;                
            }

            var tt = rknetdb.TTs.FirstOrDefault(t => t.Code == ttCode);

            // тт нет в базе
            if (tt == null)
            {
                result.Ok = false;
                result.ErrorMessage = $"ресторан с restaurantId = {restaurantId} не найден";
                return result;
            }            

            // тт не подключена к Delivery Club
            if (!tt.DeliveryClub)
            {
                result.Ok = false;
                result.ErrorMessage = $"ресторан с restaurantId = {restaurantId} не подключен к Delivery Club";
                return result;
            }
            result.Data = tt;
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
                agregatorError.agregatorName = "DeliveryClub";
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
                agregatorError.agregatorName = "DeliveryClub";
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
                agregatorError.agregatorName = "DeliveryClub";
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
