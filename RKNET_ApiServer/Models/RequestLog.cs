namespace RKNET_ApiServer.Models
{
    public class RequestLog
    {
        public string Datetime = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss");
        public string Client = "---";
        public string Action = "---";
        public HttpStatus Status = new HttpStatus(0);
        public string Url = "---";
        public string Path = "---";
        public string ReqBody = string.Empty;


        // справочник путей - исключений из логирования
        public static List<string> LogExeptions = new List<string>()
        {            
            "/.well-known",
            "/eventshub",
            "/casheshub",
            "/Home/ClientsTable",
            "/Menu",
            "/R_Keeper"
        };

        // справочник API (методы и контрорллеры)
        public static Dictionary<string, string> Actions = new Dictionary<string, string>() 
        {
            // RKNet ApiServer
            {@"^/$",                        "RKNet ApiServer: страница логов" },
            {@"^/Home/Swagger$",            "RKNet ApiServer: страница документации" },

            // Авторизация
            {@"^/connect/token$",               "API Server: авторизация" },
            {@"^/yandex/security/oauth/token$", "ЯндексЕда: авторизация" },
            {@"^/swagger/security/oauth/token$","Swagger: авторизация" },  
            
            // Р-Кипер
            {@"^/R_Keeper/GetRkMenu$",         "Р-Кипер: получение меню Р-Кипер"},
            {@"^/R_Keeper/GetRkMenuXml$",      "Р-Кипер: получение меню Р-Кипер в Xml"},

            // Меню
            {@"^/Menu/AddMenuCategory$",       "Меню: добавление категории меню"},
            {@"^/Menu/AddMenuItem$",           "Меню: добавление позиции меню"},
            {@"^/Menu/DeleteMenuCategory$",    "Меню: удаление категории меню"},
            {@"^/Menu/DeleteMenuItem$",        "Меню: удаление позиции меню"},
            {@"^/Menu/EditMenuCategory$",      "Меню: сохранение категории меню"},
            {@"^/Menu/EditMenuItem$",          "Меню: сохранение позиции меню"},
            {@"^/Menu/GetMeasureUnits$",       "Меню: получение коллекции единиц измерения для позиций"},
            {@"^/Menu/GetMenu$",               "Меню: получение меню доставки"},
            {@"^/Menu/GetMenuCategory$",       "Меню: получение категории меню"},
            {@"^/Menu/GetMenuCategoryImage$",  "Меню: получение изображения категории меню"},
            {@"^/Menu/GetMenuCategoryPath$",   "Меню: получения пути к категории меню"},
            {@"^/Menu/GetMenuItem$",           "Меню: редактор позиции меню"},

            // Яндекс
            {@"^/Yandex/menu/\w*/availability$",    "ЯндексЕда: получение стопов"},
            {@"^/Yandex/menu/\w*/composition$",     "ЯндексЕда: получение меню"},
            {@"^/Yandex/menu/\w*/promos$",          "ЯндексЕда: получение акционных блюд"},            
            {@"^/Yandex/order$",                    "ЯндексЕда: размещение заказа"},
            {@"^/Yandex/order/\w+$",                "Yandex/order/{orderId}"},
            {@"^/Yandex/order/\w*/status$",         "ЯндексЕда: получения статуса заказа"},
            {@"^/Yandex/restaurants$",              "ЯндексЕда: получение списка ресторанов"},

            // Delivery Club
            {@"^/DeliveryClub/menus/\w+$",           "Delivery Club: получение меню"},
            {@"^/DeliveryClub/stopLists/\w+$",       "Delivery Club: получение стопов"},
            {@"^/DeliveryClub/orders/\w+$",          "Delivery Club: размещение заказа"},
            {@"^/DeliveryClub/orders/\w*/\w+$",      "DeliveryClub/orders/{restaurantId}/{id}"},
            {@"^/DeliveryClub/adjustments/\w+$",     "Delivery Club: создание корректировки"},
            {@"^/DeliveryClub/notifications/\w+$",   "Delivery Club: отправка уведомления"},

        };
                   
    }
}
