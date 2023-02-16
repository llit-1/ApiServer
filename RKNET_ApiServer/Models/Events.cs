using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace RKNET_ApiServer.Models
{
    // КЛАСС ОБРАБОТКИ РАЗЛИЧНЫХ СОБЫТИЙ И РАССЫЛКИ СООБЩЕНИЙ ЧЕРЕЗ ХАБЫ
    public static class Events
    {
        // -------------------------------------------------------------
        // БД
        // -------------------------------------------------------------
        private static DB.RknetDbContext RKNetDbContext()
        {
            var sqliteBuilder = new DbContextOptionsBuilder<DB.RknetDbContext>();
            sqliteBuilder.UseSqlite(ApiServer.Configuration.GetConnectionString("sqlite"));
            return new DB.RknetDbContext(sqliteBuilder.Options);
        }
        private static DB.MSSQLDBContext MSSQLDbContext()
        {
            var mssqlBuilder = new DbContextOptionsBuilder<DB.MSSQLDBContext>();
            mssqlBuilder.UseSqlServer(ApiServer.Configuration.GetConnectionString("mssql"));
            return new DB.MSSQLDBContext(mssqlBuilder.Options);
        }

        // -------------------------------------------------------------
        // КОНСТРУКТОР
        // -------------------------------------------------------------
        static Events()
        {
            // минутный таймер
            var timerMinute = new System.Timers.Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);
            timerMinute.Elapsed += EveryMinute;
            timerMinute.AutoReset = true;
            timerMinute.Start();

            // часовой таймер
            var timerHour = new System.Timers.Timer(TimeSpan.FromHours(1).TotalMilliseconds);
            timerHour.Elapsed += Hourly;
            timerHour.AutoReset = true;
            timerHour.Start();
          
            RemoveOldDeliveryStops();            
        }
        // -------------------------------------------------------------
        // События по таймеру
        // -------------------------------------------------------------

        // действия по расписанию (проверяеится ежеминутно)
        private static void EveryMinute(Object? data, System.Timers.ElapsedEventArgs arg)
        {
            var dateTime = DateTime.Now;

            if (dateTime.Hour == 1 & dateTime.Minute == 0)
            {
                CashClientsUpdate();
                RemoveOldDeliveryStops();
                NullOrdersNotification();
            }
        }

        // запуск автообновления кассовых клиентов
        private static void Hourly(Object? data, System.Timers.ElapsedEventArgs arg)
        {
            CheckClientsUpdate();
        }

        

        // -------------------------------------------------------------
        // Логи запросов
        // -------------------------------------------------------------
        public static void Logging(string ActionName, System.Security.Claims.ClaimsPrincipal User, HttpRequest Request)
        {
            var log = new RequestLog();
            var userClaim = User.Claims.FirstOrDefault(c => c.Type == "client_id");

            if (userClaim != null)
                log.Client = userClaim.Value;
            else
                log.Client = "неавторизованный клиент";

            log.Action = ActionName;
            log.Url = string.Format("{0}://{1}{2}", Request.Scheme, Request.Host, Request.Path);

            SignalR.EventsHub.Current.Clients.All.SendAsync("Logging", log);
        }
        public static void Logging(string Message)
        {
            var log = new RequestLog();
            log.Url = Message;

            SignalR.EventsHub.Current.Clients.All.SendAsync("Logging", JsonConvert.SerializeObject(log));
        }
        public static void Logging(RequestLog reqLog)
        {
            // объект на странице получается без подобъектов, все поля с мальенькой буквы
            //SignalR.EventsHub.Current.Clients.All.SendAsync("Logging", reqLog);

            // Формирует текстовую строку Json с подобъектами с учетом регистра
            SignalR.EventsHub.Current.Clients.All.SendAsync("Logging", JsonConvert.SerializeObject(reqLog));

            // Пишем лог в БД            
            if (reqLog.Client.ToLower().Contains("yandex") || reqLog.Client.ToLower().Contains("delivery") || reqLog.Status.Code == 0)
            {
                var requestLog = new RKNet_Model.MSSQL.RequestLog();
                requestLog.Client = reqLog.Client;
                requestLog.Request = reqLog.Action;
                requestLog.Url = reqLog.Url;
                requestLog.ResultCode = reqLog.Status.Code;
                requestLog.ResultMessage = reqLog.Status.Name;

                var mssql = MSSQLDbContext();
                mssql.RequestLogs.Add(requestLog);
                mssql.SaveChanges();
            }
        }

        public static void LogginAgregatorError(zabbix_lib.AgregatorError agregatorError)
        {
            // Пишем лог в БД
            var mssql = MSSQLDbContext();
            mssql.AgregatorErrors.Add(agregatorError);
            mssql.SaveChanges();
            
            // Отправляем лог в Zabbix
            zabbix_lib.ZabbixSender.Send(agregatorError);
        }

        // -------------------------------------------------------------
        // Кассовые клиенты
        // -------------------------------------------------------------        
        // обновление данных всех подключённых кассовых клиентов и вывод на web страничку
        public static void CashClientsUpdate()
        {            
            var mssqldb = MSSQLDbContext();
            foreach (var client in SignalR.CashesHub.cashClients)
            {
                client.YandexCount = mssqldb.MarketOrders
                    .Where(o => o.TTCode == client.TTCode)
                    .Where(o => o.OrderTypeCode == 1)
                    .Where(o => o.Created.Date == DateTime.Now.Date)
                    .Count();

                client.DeliveryCount = mssqldb.MarketOrders
                    .Where(o => o.TTCode == client.TTCode)
                    .Where(o => o.OrderTypeCode == 2)
                    .Where(o => o.Created.Date == DateTime.Now.Date)
                    .Count();
            }
            SignalR.EventsHub.Current.Clients.All.SendAsync("CashClientsToWeb");
        }
        
        // обновление данных конкретного кассового клиента и вывод на web страничку
        public static void CashClientsUpdate(string clientId)
        {
            var client = SignalR.CashesHub.cashClients.FirstOrDefault(c => c.ClientId == clientId);
            if (client != null)
            {
                var mssqldb = MSSQLDbContext();
                client.YandexCount = mssqldb.MarketOrders
                    .Where(o => o.TTCode == client.TTCode)
                    .Where(o => o.OrderTypeCode == 1)
                    .Where(o => o.Created.Date == DateTime.Now.Date)
                    .Count();

                client.DeliveryCount = mssqldb.MarketOrders
                    .Where(o => o.TTCode == client.TTCode)
                    .Where(o => o.OrderTypeCode == 2)
                    .Where(o => o.Created.Date == DateTime.Now.Date)
                    .Count();

                client.isOnline = true;
                client.LastSeen = DateTime.Now;
                UpdateCashClientsTable(client);
            }
            SignalR.EventsHub.Current.Clients.All.SendAsync("CashClientsToWeb");                        
        }

        // Автообновление одного клиента
        public static void AutoUpdateClient(string clientId)
        {
            SignalR.CashesHub.Current.Clients.Client(clientId).SendAsync("AutoUpdate");            
        }

        // Автообновление всех клиентов
        public static void AutoUpdateClient()
        {
            SignalR.CashesHub.Current.Clients.All.SendAsync("AutoUpdate");
        }

        // Обновление таблицы кассовых клиентов CashClients
        public static void UpdateCashClientsTable(RKNet_Model.CashClient.CashClient client)
        {
            var rknetdb = RKNetDbContext();

            // получаем данные об обновлении клиента
            var dbClient = rknetdb.CashClients.FirstOrDefault(c => c.CashIp == client.CashIp);
            if (dbClient != null)
            {
                client.UpdateToVersion = dbClient.UpdateToVersion;
            }

            // проверяем состояние обновления клиента
            var isNeedUpdate = false;
            if (client.UpdateToVersion != null)
            {
                if (client.Version != client.UpdateToVersion)
                {
                    isNeedUpdate = true;
                }
                else
                {
                    client.UpdateToVersion = null;
                }
            }

            // оставляем единственную актуальную запись в таблице по кассе клиента
            if (dbClient != null)
            {
                var clientsToDelete = rknetdb.CashClients.Where(c => c.Id != dbClient.Id).Where(c => c.CashIp == client.CashIp);
                rknetdb.CashClients.RemoveRange(clientsToDelete);

                dbClient.ClientId = client.ClientId;
                dbClient.TTCode = client.TTCode;
                dbClient.TTName = client.TTName;
                dbClient.CashId = client.CashId;
                dbClient.CashName = client.CashName;
                dbClient.CashIp = client.CashIp;
                dbClient.YandexCount = client.YandexCount;
                dbClient.DeliveryCount = client.DeliveryCount;
                dbClient.PrinterName = client.PrinterName;
                dbClient.Version = client.Version;
                dbClient.isOnline = client.isOnline;
                dbClient.LastSeen = client.LastSeen;
                dbClient.UpdateToVersion = client.UpdateToVersion;
                dbClient.Comment = client.Comment;

                rknetdb.CashClients.Update(dbClient);
                rknetdb.SaveChanges();
            }
            else
            {
                rknetdb.CashClients.Add(client);
                rknetdb.SaveChanges();
            }                   
            

            // обновляем клиент при необходимости
            if (isNeedUpdate)
            {
                //SignalR.CashesHub.Current.Clients.Client(client.ClientId).SendAsync("AutoUpdate");
            }
        }
        
        // Проверка наличия обновлений клиентов
        public static void CheckClientsUpdate()
        {
            try
            {
                var rknetdb = RKNetDbContext();
                var apiSettings = rknetdb.ApiServerSettings.FirstOrDefault();
                if (apiSettings != null)
                {
                    if (apiSettings.CashClientsAutoUpdate)
                    {
                        var actualVersion = rknetdb.CashClientVersions.FirstOrDefault(v => v.isActual);
                        if (actualVersion != null)
                        {
                            foreach (var client in rknetdb.CashClients)
                            {
                                if (client.Version != actualVersion.Version)
                                {
                                    client.UpdateToVersion = actualVersion.Version;
                                    rknetdb.CashClients.Update(client);
                                    rknetdb.SaveChanges();
                                    if (client.isOnline)
                                    {
                                        AutoUpdateClient(client.ClientId);
                                    }
                                }

                            }

                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Models.Logging.LocalLog($"ошибка Events -> CheckClientsUpdate: {ex.Message}");
            }
        }

        // -------------------------------------------------------------
        // Заказы с доставкой
        // -------------------------------------------------------------
        public static void NewOrder(RKNet_Model.MSSQL.MarketOrder order)
        {
            try
            {
                var rknetdb = RKNetDbContext();
                var mssql = MSSQLDbContext();

                // отправляем заказ на кассы точки
                var tt = rknetdb.TTs.Include(t => t.CashStations).FirstOrDefault(t => t.Code == order.TTCode);
                foreach (var cash in tt.CashStations)
                {
                    var cashClients = SignalR.CashesHub.cashClients.Where(c => c.CashId == cash.Id).ToList();
                    foreach (var cashClient in cashClients)
                    {
                        SignalR.CashesHub.Current.Clients.Client(cashClient.ClientId).SendAsync("NewOrder", order);
                    }
                }

                // обновляем список клиентов на web странице сервера
                CashClientsUpdate();

                // добавляем запись в таблицу логов заказов
                var orderLog = new RKNet_Model.MSSQL.OrderLog();
                orderLog.TTName = order.TTName;
                orderLog.TTCode = order.TTCode;
                orderLog.OrderId = order.Id;
                orderLog.OrderTypeName = order.OrderTypeName;
                orderLog.OrderNumber = order.OrderNumber;
                orderLog.StatusName = order.StatusName;
                orderLog.StatusCode = order.StatusCode;

                mssql.Orderlogs.Add(orderLog);
                mssql.SaveChanges();
            }  
            catch(Exception ex)
            {
                Models.Logging.LocalLog($"ошибка RKNET_ApiServer.Models.Events.NewOrder: {ex.Message}");
            }
        }
        public static void OrderAccepted(bool isAccepted, RKNet_Model.MSSQL.MarketOrder order)
        {
            try
            {
                var mssql = MSSQLDbContext();

                // добавляем запись в таблицу логов заказов
                var orderLog = new RKNet_Model.MSSQL.OrderLog();
                orderLog.TTName = order.TTName;
                orderLog.TTCode = order.TTCode;
                orderLog.OrderId = order.Id;
                orderLog.OrderTypeName = order.OrderTypeName;
                orderLog.OrderNumber = order.OrderNumber;
                orderLog.StatusName = order.StatusName;
                orderLog.StatusCode = order.StatusCode;

                if (!isAccepted)
                    orderLog.Comment = "отменён на тт";

                mssql.Orderlogs.Add(orderLog);
                mssql.SaveChanges();
            }
            catch(Exception ex)
            {
                Models.Logging.LocalLog($"ошибка RKNET_ApiServer.Models.Events.OrderAccepted: {ex.Message}");
            }
        }
        public static void OrderCancel(RKNet_Model.MSSQL.MarketOrder order)
        {
            try
            {
                var rknetdb = RKNetDbContext();

                // отправляем отмену заказа на кассы точки
                var tt = rknetdb.TTs.Include(t => t.CashStations).FirstOrDefault(t => t.Code == order.TTCode);
                foreach (var cash in tt.CashStations)
                {
                    var cashClients = SignalR.CashesHub.cashClients.Where(c => c.CashId == cash.Id).ToList();
                    foreach (var cashClient in cashClients)
                    {
                        SignalR.CashesHub.Current.Clients.Client(cashClient.ClientId).SendAsync("OrderCancel", order);
                    }
                }

                // обновляем список клиентов на web странице сервера
                CashClientsUpdate();

                // добавляем запись в таблицу логов заказов
                var mssql = MSSQLDbContext();

                var orderLog = new RKNet_Model.MSSQL.OrderLog();
                orderLog.TTName = order.TTName;
                orderLog.TTCode = order.TTCode;
                orderLog.OrderId = order.Id;
                orderLog.OrderTypeName = order.OrderTypeName;
                orderLog.OrderNumber = order.OrderNumber;
                orderLog.StatusName = order.StatusName;
                orderLog.StatusCode = order.StatusCode;
                orderLog.Comment = "отменён агрегатором: " + order.CancelReason;

                mssql.Orderlogs.Add(orderLog);
                mssql.SaveChanges();
            }
            catch(Exception ex)
            {
                Models.Logging.LocalLog($"ошибка RKNET_ApiServer.Models.Events.OrderCancel: {ex.Message}");
            }
        }
        public static void OrderUpdate(RKNet_Model.MSSQL.MarketOrder order)
        {
            try
            {
                var rknetdb = RKNetDbContext();

                // отправляем заказ на кассы точки
                var tt = rknetdb.TTs.Include(t => t.CashStations).FirstOrDefault(t => t.Code == order.TTCode);
                foreach (var cash in tt.CashStations)
                {
                    var cashClients = SignalR.CashesHub.cashClients.Where(c => c.CashId == cash.Id).ToList();
                    foreach (var cashClient in cashClients)
                    {
                        SignalR.CashesHub.Current.Clients.Client(cashClient.ClientId).SendAsync("OrderUpdate", order);
                    }
                }

                var mssql = MSSQLDbContext();

                // добавляем запись в таблицу логов заказов
                var orderLog = new RKNet_Model.MSSQL.OrderLog();
                orderLog.TTName = order.TTName;
                orderLog.TTCode = order.TTCode;
                orderLog.OrderId = order.Id;
                orderLog.OrderTypeName = order.OrderTypeName;
                orderLog.OrderNumber = order.OrderNumber;
                orderLog.StatusName = order.StatusName;
                orderLog.StatusCode = order.StatusCode;
                orderLog.Comment = "заказ изменён агрегатором";

                mssql.Orderlogs.Add(orderLog);
                mssql.SaveChanges();
            }
            catch(Exception ex)
            {
                Models.Logging.LocalLog($"ошибка RKNET_ApiServer.Models.Events.OrderUpdate: {ex.Message}");
            }
        }        
        public static void OrderFinished(RKNet_Model.MSSQL.MarketOrder order)
        {
            try
            {
                var mssql = MSSQLDbContext();

                // добавляем запись в таблицу логов заказов

                var orderLog = new RKNet_Model.MSSQL.OrderLog();
                orderLog.TTName = order.TTName;
                orderLog.TTCode = order.TTCode;
                orderLog.OrderId = order.Id;
                orderLog.OrderTypeName = order.OrderTypeName;
                orderLog.OrderNumber = order.OrderNumber;
                orderLog.StatusName = order.StatusName;
                orderLog.StatusCode = order.StatusCode;
                orderLog.Comment = "заказ завершен";

                mssql.Orderlogs.Add(orderLog);
                mssql.SaveChanges();
            }
            catch(Exception ex)
            {
                Models.Logging.LocalLog($"ошибка RKNET_ApiServer.Models.Events.OrderFinished: {ex.Message}");
            }
        }
        
        // -------------------------------------------------------------
        // Стопы ресторана
        // -------------------------------------------------------------
        private static void RemoveOldDeliveryStops()
        {
            try
            {
                var mssql = MSSQLDbContext();
                var removeItems = mssql.DeliveryItemStops
                    .Where(s => s.Created.Date != DateTime.Now.Date)
                    .Where(s => s.Cancelled == null);

                foreach (var stop in removeItems)
                {
                    stop.Cancelled = DateTime.Now;
                }

                mssql.DeliveryItemStops.UpdateRange(removeItems);
                mssql.SaveChanges();
            }
            catch(Exception ex)
            {
                Models.Logging.LocalLog($"ошибка RKNET_ApiServer.Models.Events.RemoveOldDeliveryStops: {ex.Message}");
            }
        }

        // -------------------------------------------------------------
        // Событие отсуствия заказов за предыдущий день
        // -------------------------------------------------------------
        public static void NullOrdersNotification()
        {
            try
            {
                var rknetdb = RKNetDbContext();
                var mssql = MSSQLDbContext();
                var yesterday = DateTime.Now.AddDays(-1).Date;

                // проверяем наличие заказов Яндекса
                foreach (var tt in rknetdb.TTs.Where(t => t.YandexEda))
                {
                    var orders = mssql.MarketOrders.Where(o => o.TTCode == tt.Code).Where(o => o.Created.Date == yesterday).Where(o => o.OrderTypeCode == 1);
                    if (orders.Count() == 0)
                    {
                        var agregatorError = new zabbix_lib.AgregatorError
                        {
                            agregatorName = "Яндекс Еда",
                            ttName = tt.Name,
                            restaurantId = tt.Code.ToString(),
                            errorMessage = $"За {yesterday.ToString("dd.MM.yyyy")} не было заказов, возможно ресторан отключен от платформы."
                        };
                        LogginAgregatorError(agregatorError);
                    }
                }

                // проверяем наличие заказов Delivery
                foreach (var tt in rknetdb.TTs.Where(t => t.DeliveryClub))
                {
                    var orders = mssql.MarketOrders.Where(o => o.TTCode == tt.Code).Where(o => o.Created.Date == yesterday).Where(o => o.OrderTypeCode == 2);
                    if (orders.Count() == 0)
                    {
                        var agregatorError = new zabbix_lib.AgregatorError
                        {
                            agregatorName = "Delivery Club",
                            ttName = tt.Name,
                            restaurantId = tt.Code.ToString(),
                            errorMessage = $"За {yesterday.ToString("dd.MM.yyyy")} не было заказов, возможно ресторан отключен от платформы."
                        };
                        LogginAgregatorError(agregatorError);
                    }
                }
            }
            catch(Exception ex)
            {
                Models.Logging.LocalLog($"ошибка RKNET_ApiServer.Models.Events.NullOrdersNotification: {ex.Message}");
            }
        }
    }
}
