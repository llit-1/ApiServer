using Microsoft.EntityFrameworkCore;
using RKNET_ApiServer.DB.Models;

namespace RKNET_ApiServer.DB
{
    public class MSSQLDBContext : DbContext
    {
        public MSSQLDBContext(DbContextOptions<MSSQLDBContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<RKNet_Model.MSSQL.Log> Logs { get; set; } // логи действий пользователей
        public DbSet<RKNet_Model.MSSQL.SkuStop> SkuStops { get; set; } // стоп-листы продаж (блокировка позиций на кассах)
        public DbSet<RKNet_Model.MSSQL.MarketOrder> MarketOrders { get; set; } // заказы с доставкой
        public DbSet<RKNet_Model.MSSQL.OrderLog> Orderlogs { get; set; } // логи изменения статусов заказов
        public DbSet<RKNet_Model.MSSQL.RequestLog> RequestLogs { get; set; } // логи запросов к Апи серверу
        public DbSet<RKNet_Model.MSSQL.DeliveryItemStop> DeliveryItemStops { get; set; } // стопы блюд с доставкой по ТТ
        public DbSet<zabbix_lib.AgregatorError> AgregatorErrors { get; set; } // логи ошибок агрегаторов
        public DbSet<SaleObjectsAgregator> SaleObjectsAgregators  { get; set; } // продажи агрегаторов по позициям

    }
}
