using System;
using System.Xml;
using System.Xml.Serialization;

namespace RKNET_ApiServer.Api.R_Keeper.Rk7XML.Request
{
    // Создание заказа
    public class NewOrder
    {
        [XmlRoot(ElementName = "RK7Query")]
        public class CreateOrder
        {
            // Конструктор запроса
            public CreateOrder(string DateOut, string CustomerAndPhone, string TableCode, string CreatorCode, string WaiterCode, string GuestTypeId)
            {
                rk7cmd.CMD = "CreateOrder"; // Создать новый заказ
                rk7cmd.openTime = DateOut; // Дата выдачи заказа
                rk7cmd.duration = "T00:01:00"; // Время действия заказа, если меньше минуты,то заказ после создания нигде не виден, и в него не добавляются блюда
                rk7cmd.persistentComment = CustomerAndPhone; // Имя клиента и телефон
                rk7cmd.nonPersistentComment = DateTime.Now.ToString("dd.MM.yyyy"); // Дата создания заказа
                rk7cmd.promoCode = "в работе"; // Статус заказа
                rk7cmd.table.code = TableCode; // Код стола, на который оформляется заказ
                rk7cmd.creator.code = CreatorCode; // Код работника из менеджерки, от имени которого будет создан заказ
                rk7cmd.waiter.code = WaiterCode; // Код официанта, который как бы обслуживает заказ
                rk7cmd.guestType.id = GuestTypeId; // Id типа гостей, в данном случае тип гостей - "предзаказ"
            }

            // Беспараметрический конструктор - необходим для сериализации в XML
            private CreateOrder() { }

            // Элемент RK7CMD
            [XmlElement(ElementName = "RK7CMD")]
            public RK7CMD rk7cmd = new RK7CMD();

            [Serializable]
            public class RK7CMD
            {
                // Атрибуты
                [XmlAttribute]
                public string CMD { get; set; }
                [XmlAttribute]
                public string openTime { get; set; }
                [XmlAttribute]
                public string duration { get; set; }
                [XmlAttribute]
                public string persistentComment { get; set; }
                [XmlAttribute]
                public string nonPersistentComment { get; set; }
                [XmlAttribute]
                public string promoCode { get; set; }

                // Элементы
                [XmlElement]
                public Table table = new Table();

                [XmlElement]
                public Creator creator = new Creator();

                [XmlElement]
                public Waiter waiter = new Waiter();

                [XmlElement]
                public GuestType guestType = new GuestType();

                // Классы элементов
                [Serializable]
                public class Table
                {
                    [XmlAttribute]
                    public string code { get; set; }
                }

                [Serializable]
                public class Creator
                {
                    [XmlAttribute]
                    public string code { get; set; }
                }

                [Serializable]
                public class Waiter
                {
                    [XmlAttribute]
                    public string code { get; set; }
                }

                [Serializable]
                public class GuestType
                {
                    [XmlAttribute]
                    public string id { get; set; }
                }
            }
        }
    }
}
