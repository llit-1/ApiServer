using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace RKNET_ApiServer.Api.R_Keeper.Rk7XML.Response
{
    // Информация по заказу
    public class GetOrderResponse
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
            public string ErrorText { get; set; }
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
            public string Status { get; set; }
            [XmlAttribute]
            public string ErrorText { get; set; }
            [XmlAttribute]
            public string DateTime { get; set; }
            [XmlAttribute]
            public string WorkTime { get; set; }

            [XmlElement(ElementName = "Order")]
            public Order order = new Order();
        }

        [Serializable]
        public class Order
        {
            [XmlAttribute]
            public string visit { get; set; }
            [XmlAttribute]
            public string guid { get; set; }

            [XmlElement(ElementName = "Session")]
            public List<Session> SessionList = new List<Session>();
        }

        [Serializable]
        public class Session
        {
            [XmlAttribute]
            public string uni { get; set; }

            [XmlElement(ElementName = "Dish")]
            public List<Dish> DishList = new List<Dish>();
        }

        [Serializable]
        public class Dish
        {
            [XmlAttribute]
            public string code { get; set; }
            [XmlAttribute]
            public string uni { get; set; }
            [XmlAttribute]
            public string price { get; set; }
            [XmlAttribute]
            public string quantity { get; set; }
        }
    }
}
