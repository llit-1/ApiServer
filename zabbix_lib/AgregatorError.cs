using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zabbix_lib
{
    // Модель лога ошибки (передается в zabbix в виде json строки)
    public class AgregatorError
    {
        public int id { get; set; }                 // Уникальный идентификатор лога
        public DateTime dateTime { get; set; }      // Дата и время поступления запроса агрегатора
        public string agregatorName { get; set; }   // Имя агрегатора ("Яндекс Еда" или "Delivery Club")
        public string ttName { get; set; }          // Имя ТТ (пример: "Комендантский 69")
        public string orderNumber { get; set; }     // Номер заказа в системе агрегатора (если есть)
        public string restaurantId { get; set; }    // Код точки в нашей базе (restaurantId у агрегтора)       
        public string requestName { get; set; }     // Имя запроса (пример: "ЯндексЕда: размещение заказа")
        public string requestUrl { get; set; }      // Url запроса                        
        public string requestBody { get; set; }     // Тело запроса с данными, переданными агрегатором (по факту там и содержатся ошибки)
        public int responseCode { get; set; }       // Код ответа нашего сервера
        public string responseBody { get; set; }    // Тело ответа нашего сервера
        public string errorMessage { get; set; }    // Расшифровка ошибки

        public AgregatorError()
        {
            dateTime = DateTime.Now;
        }
    }
}
