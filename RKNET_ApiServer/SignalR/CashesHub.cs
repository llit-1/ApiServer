using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using RKNET_ApiServer.Models;
using RKNET_ApiServer.Api.Yandex.Models;

namespace RKNET_ApiServer.SignalR
{
    public class CashesHub : Hub
    {
        // контекст хаба, присваивается в Program при старте приложения
        public static IHubContext<CashesHub>? Current { get; set; }

        // список подключённых к хабу клиентов, сопоставленных с ip адресами касс
        public static List<RKNet_Model.CashClient.CashClient> cashClients = new List<RKNet_Model.CashClient.CashClient>();

        // БД
        DB.RknetDbContext rknetdb;
        DB.MSSQLDBContext mssql;
       
        // Конструктор
        public CashesHub(DB.RknetDbContext rknetdbContext, DB.MSSQLDBContext mssqlContext)
        {
            rknetdb = rknetdbContext;
            mssql = mssqlContext;
        }

        // Подключение клиентов к хабу
        public override async Task OnConnectedAsync()
        {            
            try
            {
                // получаем ip из заголовка запроса
                var httpContext = Context.GetHttpContext();
                var cashIp = httpContext.Request.Headers["CashClientIp"].ToString();
                var clientVersion = httpContext.Request.Headers["CashClientVersion"].ToString();
                if (string.IsNullOrEmpty(cashIp))
                {
                    Context.Abort(); // клиентов с неизвестным ip отбрасываем
                }

                // для тестирования во время отладки
                if (cashIp == "10.150.0.200")
                    cashIp = "10.140.31.85";

                // определяем кассу подключившегося клиента
                var cash = rknetdb.CashStations.Include(c => c.TT).FirstOrDefault(c => c.Ip == cashIp);

                if (cash == null)
                {
                    Context.Abort(); // отказываем в подключении клиенту с неизвестной кассы
                    Models.Events.Logging($"отказ в подключении кассы (нет в настройках ТТ на Портале): {cashIp}");
                }
                else
                {
                    // добавим подключившегося клиента в список активных клиентов
                    var client = new RKNet_Model.CashClient.CashClient
                    {
                        ClientId = Context.ConnectionId,
                        TTName = cash.TT.Name,
                        TTCode = cash.TT.Code,
                        CashName = cash.Name,
                        CashId = cash.Id,
                        CashIp = cashIp,
                        Version = clientVersion,
                        isOnline = true,
                        LastSeen = DateTime.Now
                    };

                    // удаляем ранее подключившиеся дубликаты соединений по одному клиенту из списка
                    var existClients = cashClients.Where(c => c.CashIp == client.CashIp).ToList();
                    foreach (var exClient in existClients)
                    {
                        cashClients.Remove(exClient);
                    }

                    // добавляем подключившегося клиента в список
                    cashClients.Add(client);

                    // обновляем таблицу кассовых клиентов и проверяем на обновления
                    Models.Events.UpdateCashClientsTable(client);

                    await Clients.Caller.SendAsync("ClientInfo", client.TTName);
                    Models.Events.CashClientsUpdate(client.ClientId);
                }
            }
            catch (Exception Ex)
            {
                Logging.LocalLog($"ошибка RKNET_ApiServer.SignalR.CashesHub.OnConnectedAsync: {Ex.Message}");
            }
            finally
            {
                await base.OnConnectedAsync();
            }                        
        }

        // Отключение клиентов от хаба
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                var client = cashClients.FirstOrDefault(c => c.ClientId == Context.ConnectionId);
                if (client != null)
                {
                    cashClients.Remove(client);
                    client.isOnline = false;
                    client.LastSeen = DateTime.Now;
                    Models.Events.UpdateCashClientsTable(client);
                }

                await base.OnDisconnectedAsync(exception);
                EventsHub.Current.Clients.All.SendAsync("CashClientsToWeb");
            }
            catch (Exception ex)
            {
                Logging.LocalLog($"ошибка RKNET_ApiServer.SignalR.CashesHub.OnDisconnectedAsync: {ex.Message}");
            }
            
        }
        
        // --------------------------------------------------
        // Входящие запросы
        // --------------------------------------------------
        public async Task GetOrders()
        {
            try
            {
                var client = cashClients.FirstOrDefault(c => c.ClientId == Context.ConnectionId);
                if (client != null)
                {
                    var orders = mssql.MarketOrders
                    .Where(o => o.Created.Date == DateTime.Now.Date)
                    .Where(o => o.TTCode == client.TTCode)
                    .ToList();
                    await Clients.Caller.SendAsync("GetOrders", orders);
                    Models.Events.CashClientsUpdate(Context.ConnectionId);

                    // отправляем обновление заказов на дополнительные кассы ТТ
                    var tt = rknetdb.TTs.Include(t => t.CashStations).FirstOrDefault(t => t.Code == client.TTCode);
                    if (tt != null)
                    {
                        foreach (var cash in tt.CashStations)
                        {
                            if (cash.Ip != client.CashIp)
                            {
                                var additionalClient = cashClients.FirstOrDefault(c => c.CashIp == cash.Ip);
                                if (additionalClient != null)
                                {
                                    await Current.Clients.Client(additionalClient.ClientId).SendAsync("GetOrders", orders);
                                    Models.Events.CashClientsUpdate(additionalClient.ClientId);
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Logging.LocalLog($"ошибка RKNET_ApiServer.SignalR.CashesHub.GetOrders: {ex.Message}");
            }            
        }
        public async Task OrderAccept(bool isAccepted, int orderId)
        {
            try
            {
                var order = mssql.MarketOrders.FirstOrDefault(o => o.Id == orderId);
                if (order != null)
                {
                    // принят на кассе
                    if (isAccepted)
                    {
                        switch (order.OrderTypeCode)
                        {
                            // Яндекс
                            case 1:
                                //var yaStatus = RKNet_Model.MSSQL.MarketOrder.OrderStatuses.YandexStatuses.FirstOrDefault(s => s.Code == 2);
                                var yaStatus = RKNet_Model.MSSQL.MarketOrder.OrderStatuses.Yandex.READY;
                                order.StatusCode = yaStatus.Code;
                                order.StatusName = yaStatus.Name;
                                order.StatusUpdatedAt = DateTime.Now;
                                break;

                            // Delivery
                            case 2:
                                var dcStatus = RKNet_Model.MSSQL.MarketOrder.OrderStatuses.DeliveryClub.ready;
                                order.StatusCode = dcStatus.Code;
                                order.StatusName = dcStatus.Name;
                                order.StatusUpdatedAt = DateTime.Now;
                                break;
                        }
                    }
                    // отклонен на кассе
                    else
                    {
                        switch (order.OrderTypeCode)
                        {
                            // Яндекс
                            case 1:
                                var yaStatus = RKNet_Model.MSSQL.MarketOrder.OrderStatuses.Yandex.CANCELLED;
                                order.StatusCode = yaStatus.Code;
                                order.StatusName = yaStatus.Name;
                                order.StatusComment = "новый заказ был отклонён на тт";
                                order.StatusUpdatedAt = DateTime.Now;
                                order.CancelReason = "отменён на тт";
                                break;
                            // Delivery
                            case 2:
                                var dcStatus = RKNet_Model.MSSQL.MarketOrder.OrderStatuses.DeliveryClub.restaurant_cancelled;
                                order.StatusCode = dcStatus.Code;
                                order.StatusName = dcStatus.Name;
                                order.StatusComment = "новый заказ был отклонён на тт";
                                order.StatusUpdatedAt = DateTime.Now;
                                order.CancelReason = "отменён на тт";
                                break;
                        }                        
                    }
                    mssql.MarketOrders.Update(order);
                    mssql.SaveChanges();

                    // генерируем событие отмены заказа
                    RKNET_ApiServer.Models.Events.OrderAccepted(isAccepted, order);
                }
            }
            catch (Exception ex)
            {
                var log = new Models.RequestLog();
                log.Action = "подтверждение заказа на кассе";
                log.Status = new Models.HttpStatus(500);
                log.Path = ex.Message;
                Models.Events.Logging(log);

                Logging.LocalLog($"ошибка RKNET_ApiServer.SignalR.CashesHub.OrderAccept: {ex.Message}");
            }            
            await GetOrders();
        }
        public async Task OrderFinish(int orderId)
        {
            try
            {
                var order = mssql.MarketOrders.FirstOrDefault(o => o.Id == orderId);
                if (order != null)
                {
                    switch (order.OrderTypeCode)
                    {
                        // Яндекс
                        case 1:
                            var yaStatus = RKNet_Model.MSSQL.MarketOrder.OrderStatuses.Yandex.TAKEN_BY_COURIER;
                            order.StatusCode = yaStatus.Code;
                            order.StatusName = yaStatus.Name;
                            order.StatusUpdatedAt = DateTime.Now;
                            break;
                        // Delivery
                        case 2:
                            var dcStatus = RKNet_Model.MSSQL.MarketOrder.OrderStatuses.DeliveryClub.picked_up;
                            order.StatusCode = dcStatus.Code;
                            order.StatusName = dcStatus.Name;
                            order.StatusUpdatedAt = DateTime.Now;
                            break;
                    }
                    mssql.MarketOrders.Update(order);
                    mssql.SaveChanges();

                    // генерируем событие отмены заказа
                    RKNET_ApiServer.Models.Events.OrderFinished(order);
                }
            }
            catch (Exception ex)
            {
                var log = new Models.RequestLog();
                log.Action = "подтверждение заказа на кассе";
                log.Status = new Models.HttpStatus(500);
                log.Path = ex.Message;
                Models.Events.Logging(log);

                Logging.LocalLog($"ошибка RKNET_ApiServer.SignalR.CashesHub.OrderFinished: {ex.Message}");
            }
            await GetOrders();            
        }
        public async Task PrinterInfo(string printerName)
        {
            try
            {
                var client = cashClients.FirstOrDefault(c => c.ClientId == Context.ConnectionId);
                if (client != null)
                {
                    client.PrinterName = printerName;
                    await EventsHub.Current.Clients.All.SendAsync("CashClientsToWeb");
                }
            }
            catch (Exception ex)
            {
                Logging.LocalLog($"ошибка RKNET_ApiServer.SignalR.CashesHub.PrinterInfo: {ex.Message}");
            }
        }
        public async Task PingReceived()
        {
            try
            {
                var client = cashClients.FirstOrDefault(c => c.ClientId == Context.ConnectionId);
                if (client != null)
                {
                    await Clients.Caller.SendAsync("ResponsePingReceived");
                }
            }
            catch(Exception ex)
            {
                Logging.LocalLog($"ошибка RKNET_ApiServer.SignalR.CashesHub.PingReceived: {ex.Message}");
            }
        }
    }
}
