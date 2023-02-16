using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace RKNET_ApiServer.HostedServices
{
    //--------------------------------------------------------------------------------------
    // Служба синхронизации меню доставки с меню Р-Кипер
    //--------------------------------------------------------------------------------------
    public class MenuSyncService : IHostedService
    {
        protected IServiceProvider serviceProvider;
        System.Timers.Timer timer;

        // Конструктор
        public MenuSyncService(IServiceProvider services)
        {
            serviceProvider = services;

            // синхронизируем меню каждые 10 минут
            timer = new System.Timers.Timer();
            timer.Interval = TimeSpan.FromMinutes(10).TotalMilliseconds;
            timer.Elapsed += MenuSync;
        }

        // Запуск службы
        public Task StartAsync(CancellationToken cancellationToken)
        {                  
            timer.Start();
            return Task.CompletedTask;
        }

        // Остановка службы
        public Task StopAsync(CancellationToken cancellationToken)
        {          
            timer.Stop();
            return Task.CompletedTask;
        }

        //--------------------------------------------------------------------------------------
        // Методы 
        //--------------------------------------------------------------------------------------
        private void MenuSync(Object? data, System.Timers.ElapsedEventArgs arg)
        {
            var log = new Models.RequestLog();
            log.Client = "MenuSyncService";

            //log.Action = "синхронизация меню...";
            //Models.ApiServer.Logging(log);
           
            using (var scope = serviceProvider.CreateScope())
            {
                try
                {
                    var rknetdb = (DB.RknetDbContext)scope.ServiceProvider.GetRequiredService(typeof(DB.RknetDbContext));


                    var menuItems = rknetdb.MenuItems.ToList();
                    var rkMenu = new Api.R_Keeper.Actions(rknetdb).GetRkMenu();
                    
                    if (rkMenu.Data == null || !rkMenu.Ok)
                        return;                    

                    var rkItems = RkItems(rkMenu.Data);

                    foreach (var item in menuItems)
                    {
                        // проверяем изменения в свойствах блюда
                        var rkItem = rkItems.FirstOrDefault(i => i.rkCode == item.rkCode);

                        if (rkItem != null)
                        {
                            // включение отключенного блюда
                            if (!item.Enabled)
                                item.Enabled = true;

                            // обновляем имя блюда из РК
                            if (item.rkName != rkItem.rkName)
                                item.rkName = rkItem.rkName;

                            // обновлем цену доставки из РК
                            if (item.rkDeliveryPrice != rkItem.deliveryPrice)
                                item.rkDeliveryPrice = rkItem.deliveryPrice;                            
                        }                            
                        else
                        {
                            // блюдо отключено или удалено в РК
                            if (item.Enabled)
                                item.Enabled = false;
                        }
                    }

                    rknetdb.MenuItems.UpdateRange(menuItems);
                    rknetdb.SaveChanges();                    
                }
                catch (Exception ex)
                {
                    Models.Logging.LocalLog($"ошибка HostedServices.MenuSyncService.MenuSync: {ex.Message}");
                }
            }
        }

        // получения списка позиций из меню р-кипер
        private List<RKNet_Model.Menu.rkMenuItem> RkItems(List<RKNet_Model.Menu.rkMenuItem> rootItems)
        {
            var rkItems = new List<RKNet_Model.Menu.rkMenuItem>();
            foreach (var item in rootItems)
            {
                if (!item.isCategory)
                {
                    rkItems.Add(item);
                }
                else
                {
                    var subItems = RkItems(item.rkMenuItems);
                    rkItems.AddRange(subItems);
                }
            }
            return rkItems;
        }
    }
}
