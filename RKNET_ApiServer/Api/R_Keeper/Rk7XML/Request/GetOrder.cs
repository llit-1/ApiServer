using System;
using System.Xml;
using System.Xml.Serialization;

namespace RKNET_ApiServer.Api.R_Keeper.Rk7XML.Request
{
    // Получение информации по заказу
    public class GetOrder
    {
        [XmlRoot(ElementName = "RK7Query")]
        public class RK7Query
        {
            // Конструктор запроса
            public RK7Query(string orderGuid)
            {
                rk7cmd.CMD = "GetOrder";
                rk7cmd.order.guid = orderGuid;
            }

            // Беспараметрический конструктор - необходим для сериализации в XML
            private RK7Query() { }

            // Элемент RK7CMD
            [XmlElement(ElementName = "RK7Command")]
            public RK7Command rk7cmd = new RK7Command();

            [Serializable]
            public class RK7Command
            {
                // Атрибуты
                [XmlAttribute]
                public string CMD { get; set; }

                [XmlElement(ElementName = "Order")]
                public Order order = new Order();

                [Serializable]
                public class Order
                {
                    [XmlAttribute]
                    public string guid { get; set; }
                }
            }
        }
    }
}
