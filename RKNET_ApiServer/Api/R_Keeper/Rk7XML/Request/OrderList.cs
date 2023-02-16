using System;
using System.Xml;
using System.Xml.Serialization;

namespace RKNET_ApiServer.Api.R_Keeper.Rk7XML.Request
{
    // Список заказов
    public class OrderList
    {
        [XmlRoot(ElementName = "RK7Query")]
        public class GetOrderList
        {
            // Беспараметрический конструктор - необходим для сериализации в XML
            public GetOrderList()
            {
                rk7cmd.CMD = "GetOrderList";
                rk7cmd.onlyOpened = "true";
            }

            // Элемент RK7CMD
            [XmlElement(ElementName = "RK7Command")]
            public RK7Command rk7cmd = new RK7Command();

            [Serializable]
            public class RK7Command
            {
                // Атрибуты
                [XmlAttribute]
                public string CMD { get; set; }
                [XmlAttribute]
                public string onlyOpened { get; set; }
            }
        }
    }
}
