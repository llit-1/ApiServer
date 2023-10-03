using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RKNET_ApiServer.DB.Models;
using RKNet_Model.MSSQL;
using System.Collections.Generic;

namespace RKNET_ApiServer.Api.Yandex
{
    public partial class Actions
    {
        /// <summary>
        /// Отмена заказа
        /// </summary>
        /// <remarks>
        /// Отмена заказа в системе ресторана
        /// </remarks>
        /// <param name="orderId">id заказа в таблице mssql.MarketOrders</param>
        /// <returns></returns>
        [HttpDelete("Yandex/order/{orderId}")]
        public IActionResult OrderDelete(string orderId, Api.Yandex.Models.CancelRequest cancelRequest)
        {
            var isLogging = true;
            var requestName = "удаление заказа";

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

            // отменяем заказ
            try
            {                
                var cancelReason = "отменен Яндексом";                
                if (!string.IsNullOrEmpty(cancelRequest.comment))
                {
                    cancelReason = cancelRequest.comment;
                }

                order.StatusName = RKNet_Model.MSSQL.MarketOrder.OrderStatuses.Yandex.CANCELLED.Name;
                order.StatusCode = RKNet_Model.MSSQL.MarketOrder.OrderStatuses.Yandex.CANCELLED.Code;
                order.StatusUpdatedAt = DateTime.Now;

                if (string.IsNullOrEmpty(order.CancelReason))
                {
                    order.CancelReason = cancelReason;
                }                    

                mssqldb.MarketOrders.Update(order);
                mssqldb.SaveChanges();

                // событие отмены заказа
                RKNET_ApiServer.Models.Events.OrderCancel(order);
                //Запись в SaleObjectsAgregators
                //Проверяем если в базе уже есть этот заказ
                var saleObjectsAgregators = mssqldb.SaleObjectsAgregators.Where(c => c.OrderNumber == order.OrderNumber);
                if (saleObjectsAgregators.ToList().Count != 0)
                {
                    foreach (var item in saleObjectsAgregators)
                    {
                        item.Deleted = 3;
                        mssqldb.SaleObjectsAgregators.Update(item);
                    }
                    mssqldb.SaveChanges();
                }
                // пропускаем если в базе уже есть этот заказ
                else
                {
                    List<MarketOrder.OrderItem> orderItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MarketOrder.OrderItem>>(order.OrderItems);
                    foreach (var item in orderItems)
                    {
                        SaleObjectsAgregator saleObjectsAgregator = new SaleObjectsAgregator();
                        saleObjectsAgregator.Midserver = order.FirstMidserver;
                        saleObjectsAgregator.Code = item.RkCode;
                        saleObjectsAgregator.SumWithDiscount = item.MenuPrice;
                        saleObjectsAgregator.SumWithoutDiscount = item.MenuPrice;
                        saleObjectsAgregator.Quantity = item.Quantity;
                        saleObjectsAgregator.Date = order.Created;
                        saleObjectsAgregator.OrderType = 1014626;
                        saleObjectsAgregator.OrderNumber = order.OrderNumber;
                        if (order.YandexOrder != null)
                        {
                            saleObjectsAgregator.Currency = 1010536;
                        }
                        else
                        {
                            saleObjectsAgregator.Currency = 1013070;
                        }
                        RKNet_Model.Rk7XML.CashStation cashStation = rknetdb.CashStations
                                                        .Include(c => c.TT)
                                                        .AsNoTracking()
                                                        .FirstOrDefault(c => c.Midserver == order.FirstMidserver);
                        saleObjectsAgregator.Restaurant = cashStation?.TT?.Restaurant_Sifr;
                        saleObjectsAgregator.Deleted = 3;
                        saleObjectsAgregator.Hour = order.Created.Hour;
                        saleObjectsAgregator.Time = order.Created.Hour * 10000 + order.Created.Minute * 100 + order.Created.Second;
                        mssqldb.SaleObjectsAgregators.Add(saleObjectsAgregator);
                    }
                    mssqldb.SaveChanges();
                }
                return StatusCode(200);
            }
            catch (Exception ex)
            {
                var errors = new List<Api.Yandex.Models.Error>();
                errors.Add(new Api.Yandex.Models.Error
                {
                    code = 100,
                    description = ex.Message
                });

                Response.StatusCode = 500;
                if (isLogging) ErrorLog(requestName, string.Empty, ex.Message);
                return new ObjectResult(errors);
            }
        }
    }
}
