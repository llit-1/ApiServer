using System;
using System.Xml;
using System.Xml.Serialization;

namespace RKNET_ApiServer.Api.R_Keeper.Rk7XML.Request
{
    // Удаление заказа (VoidOrder - одна команда делает все сразу по guid ордера)
    public class VoidOrder
    {
        [XmlRoot(ElementName = "RK7Query")]
        public class RK7Query
        {
            // Конструктор запроса
            public RK7Query(string orderGuid, string cashCode, string managerCode, string deleteCode)
            {
                rk7cmd.CMD = "VoidOrder";
                rk7cmd.order.guid = orderGuid;
                rk7cmd.station.code = cashCode;
                rk7cmd.manager.code = managerCode;
                rk7cmd.deleteReason.code = deleteCode;
            }

            // Беспараметрический конструктор - необходим для сериализации в XML
            private RK7Query() { }

            // Элемент RK7CMD
            [XmlElement(ElementName = "RK7CMD")]
            public RK7CMD rk7cmd = new RK7CMD();

            [Serializable]
            public class RK7CMD
            {
                // Атрибуты
                [XmlAttribute]
                public string CMD { get; set; }

                [XmlElement(ElementName = "Order")]
                public Order order = new Order();

                [XmlElement(ElementName = "Station")]
                public Station station = new Station();

                [XmlElement(ElementName = "Manager")]
                public Manager manager = new Manager();

                [XmlElement(ElementName = "DeleteReason")]
                public DeleteReason deleteReason = new DeleteReason();

                [Serializable]
                public class Order
                {
                    [XmlAttribute]
                    public string guid { get; set; }
                }

                [Serializable]
                public class Station
                {
                    [XmlAttribute]
                    public string code { get; set; }
                }

                [Serializable]
                public class Manager
                {
                    [XmlAttribute]
                    public string code { get; set; }
                }

                [Serializable]
                public class DeleteReason
                {
                    [XmlAttribute]
                    public string code { get; set; }
                }
            }
        }
    }
}
