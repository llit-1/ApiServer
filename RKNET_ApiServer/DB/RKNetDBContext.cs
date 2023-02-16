using Microsoft.EntityFrameworkCore;


namespace RKNET_ApiServer.DB
{
    public class RknetDbContext : DbContext
    {
        public RknetDbContext(DbContextOptions<RknetDbContext> options) : base(options)
        {           
            Database.EnsureCreated();   // создаем базу данных при первом обращении
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {           

            // переопределяем ключи для связанных полей в таблицах (ParentCategoryId вместо CategoryId)
            /*
            modelBuilder.Entity<RKNet_Model.Menu.Item>()
                .HasOne(i => i.ParentCategory)
                .WithMany(i => i.Items)
                .HasForeignKey(i => i.ParentCategoryId);
            */
            modelBuilder.Entity<RKNet_Model.Menu.Category>()
               .HasMany(c => c.Items)
               .WithOne(c => c.ParentCategory);

            modelBuilder.Entity<RKNet_Model.Menu.Category>()
               .HasMany(c => c.Categories)
               .WithOne()
               .HasForeignKey(c => c.ParentCategoryId);
               //.OnDelete(DeleteBehavior.Cascade);

            /*
            modelBuilder.Entity<RKNet_Model.Menu.Category>()
                .Property(c => c.CategoryId)
                .HasColumnName("ParentCategoryId");
            */
        }


        public DbSet<RKNet_Model.Account.User> Users { get; set; } // Таблица пользователей  
        public DbSet<RKNet_Model.Account.Role> Roles { get; set; } // Таблица ролей        
        public DbSet<RKNet_Model.Account.Group> Groups { get; set; } // Таблица групп

        public DbSet<RKNet_Model.TT.TT> TTs { get; set; } // Торговые точки
        public DbSet<RKNet_Model.TT.Type> TTtypes { get; set; } // Типы торговых точек
        public DbSet<RKNet_Model.TT.Organization> Organizations { get; set; } // Юр. лица торговых точек

        public DbSet<RKNet_Model.VMS.NX.NxSystem> NxSystems { get; set; } // Системы NX  
        public DbSet<RKNet_Model.VMS.NX.NxCamera> NxCameras { get; set; } // Камеры NX 
        public DbSet<RKNet_Model.VMS.CamGroup> CamGroups { get; set; } // Группы камер             

        public DbSet<RKNet_Model.VMS.Zone> zones { get; set; } // Зоны для анализа на камерах видеонаблюдения        

        public DbSet<RKNet_Model.ttOrders.OrderType> OrderTypes { get; set; } // типы заказов тт        

        public DbSet<RKNet_Model.RKSettings> RKSettings { get; set; } // настройки р-кипер
        public DbSet<RKNet_Model.Rk7XML.CashStation> CashStations { get; set; } // кассовые станции р-кипер

        public DbSet<RKNet_Model.Library.RootFolder> RootFolders { get; set; } // корневые каталоги библиотеки знаний
        public DbSet<RKNet_Model.Reports.UserReport> UserReports { get; set; } // ссылки на отчеты Power Bi
        public DbSet<RKNet_Model.Reports.AllReport> AllReports { get; set; } // ссылки на отчеты по всем ТТ


        public DbSet<RKNet_Model.Menu.Category> MenuCategorys { get; set; } // группа меню для доставки
        public DbSet<RKNet_Model.Menu.Item> MenuItems { get; set; } // позиция меню для доставки
        public DbSet<RKNet_Model.Menu.MeasureUnit> MeasureUnits { get; set; } // справочник мер количества

        public DbSet<RKNet_Model.LastChange> LastChanges { get; set; } // последние обновления разделов
        public DbSet<RKNet_Model.DbFile> Files { get; set; } // файлы различных типов в БД

        public DbSet<RKNet_Model.CashClient.ClientVersion> CashClientVersions { get; set; } // версии и файлы кассовых клиентов
        public DbSet<RKNet_Model.CashClient.CashClient> CashClients { get; set; } // таблица состояний кассовых клиентов

        public DbSet<RKNet_Model.ApiServerSettings> ApiServerSettings { get; set; } // настройки Api сервера

    }
}
