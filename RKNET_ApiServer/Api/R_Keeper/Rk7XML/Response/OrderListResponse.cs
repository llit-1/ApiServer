using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace RKNET_ApiServer.Api.R_Keeper.Rk7XML.Response
{
    // Список заказов
    public class OrderListResponse
    {
        // RK7QueryResult
        [Serializable]
        public class RK7QueryResult
        {
            [XmlAttribute]
            public string ServerVersion { get; set; }
            [XmlAttribute]
            public string XmlVersion { get; set; }
            [XmlAttribute]
            public string NetName { get; set; }
            [XmlAttribute]
            public string Status { get; set; }
            [XmlAttribute]
            public string Processed { get; set; }

            // Элементы
            [XmlElement(ElementName = "CommandResult")]
            public CommandResult commandResult = new CommandResult();

            // Десериализация RK7QueryResult
            public static RK7QueryResult DeSerializeQueryResult(string xml_answer)
            {
                if (xml_answer == null)
                {
                    return null;
                }

                var serializer = new XmlSerializer(typeof(RK7QueryResult));

                using (TextReader reader = new StringReader(xml_answer))
                {
                    return (RK7QueryResult)serializer.Deserialize(reader);
                }
            }
        }

        // Классы
        [Serializable]
        public class CommandResult
        {
            [XmlAttribute]
            public string CMD { get; set; }
            [XmlAttribute]
            public string lastversion { get; set; }
            [XmlAttribute]
            public string Status { get; set; }
            [XmlAttribute]
            public string ErrorText { get; set; }
            [XmlAttribute]
            public string DateTime { get; set; }
            [XmlAttribute]
            public string WorkTime { get; set; }

            [XmlElement(ElementName = "Visit")]
            public List<Visit> VisitList = new List<Visit>();
        }

        [Serializable]
        public class Visit
        {
            [XmlAttribute]
            public string GuestTypeCode { get; set; }
            [XmlAttribute]
            public string PersistentComment { get; set; }
            [XmlAttribute]
            public string NonPersistentComment { get; set; }

            [XmlElement(ElementName = "Orders")]
            public Orders orders = new Orders();
        }

        [Serializable]
        public class Orders
        {
            [XmlElement(ElementName = "Order")]
            public List<Order> OrderList = new List<Order>();

        }

        [Serializable]
        public class Order
        {
            [XmlAttribute]
            public string guid { get; set; }
            [XmlAttribute]
            public string TableCode { get; set; }
        }
    }
}
