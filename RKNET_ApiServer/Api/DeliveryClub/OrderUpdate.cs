using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RKNET_ApiServer.Api.Yandex.Models;
using RKNET_ApiServer.DB.Models;
using RKNET_ApiServer.SignalR;
using RKNet_Model.MSSQL;

namespace RKNET_ApiServer.Api.DeliveryClub
{
    public partial class Actions
    {
        /// <summary>
        /// Обновление заказа
        /// </summary>
        /// <remarks>
        /// Обновление заказа в системе ресторана
        /// </remarks>
        /// <param name="restaurantId">код тт</param>
        /// <param name="id">id заказа в таблице mssql.MarketOrders</param>
        /// <returns></returns>
        [HttpPut("deliveryclub/orders/{restaurantId}/{id}")]
        public IActionResult OrderUpdate(string restaurantId, string id, Api.DeliveryClub.Models.OrderStatus newStatus)
        {
            var isLogging = true;
            var requestName = "изменение статуса заказа";

            // Получаем данные ресторана
            var result = GetTT(restaurantId);
            if (!result.Ok)
            {
                Response.StatusCode = 500;
                if (isLogging) ErrorLog(requestName, restaurantId, result.ErrorMessage);
                return new ObjectResult(result.ErrorMessage);
            }

            var tt = result.Data;


            // ОБНОВЛЕНИЕ СТАТУСА

            var response = new Api.DeliveryClub.Models.OrderWithId();

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
            
            // проеверяем существование переданного статуса в модели
            var status = RKNet_Model.MSSQL.MarketOrder.OrderStatuses.DeliveryClubStatuses.FirstOrDefault(s => s.Value == newStatus.status);
            if (status == null)
            {
                var errorMessage = $"статус \"{newStatus}\" не обнаружен в списке доступных стастусов модели";

                Response.StatusCode = 500;
                if (isLogging) ErrorLog(requestName, restaurantId, errorMessage);
                return new ObjectResult(errorMessage);
            }

            // проверяем допустимость установки статуса, переданного агрегатором
            if (status.Code != 6 && status.Code != 7 && status.Code != 9 && status.Code != 10)
            {
                var errorMessage = $"недопустимо изменение статуса заказа с \"{order.StatusName}\" на \"{status.Name}\" со стороны DeliveryClub";

                Response.StatusCode = 500;
                if (isLogging) ErrorLog(requestName, restaurantId, errorMessage);
                return new ObjectResult(errorMessage);
            }

            try 
            {

                order.StatusCode = status.Code;
                order.StatusName = status.Name;
                order.StatusComment = null;
                order.StatusUpdatedAt = DateTime.Now;

                // устанавливаем причину отмены заказа
                if(status.Code == 8 | status.Code == 9)
                {
                    if (string.IsNullOrEmpty(order.CancelReason))
                    {
                        order.CancelReason = status.Name;
                    }
                    //Запись в SaleObjectsAgregators
                    PublicMethods.SaleObjectsAgregatorSave(mssqldb, rknetdb, order, 3);
                }                

                mssqldb.MarketOrders.Update(order);
                mssqldb.SaveChanges();

                // событие отмены или изменения заказа
                if (order.StatusCode == RKNet_Model.MSSQL.MarketOrder.OrderStatuses.DeliveryClub.customer_cancelled.Code)
                {                    
                    RKNET_ApiServer.Models.Events.OrderCancel(order);
                }
                else
                {
                    RKNET_ApiServer.Models.Events.OrderUpdate(order);
                }
                // событие Доставки
                if (status.Code == 7)
                {
                    //Запись в SaleObjectsAgregators
                    PublicMethods.SaleObjectsAgregatorSave(mssqldb, rknetdb, order, 0);
                }

                response.id = order.Id.ToString();
                response.status = status.Value;

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
