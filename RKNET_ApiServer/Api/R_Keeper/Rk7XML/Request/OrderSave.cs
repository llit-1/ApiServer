using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace RKNET_ApiServer.Api.R_Keeper.Rk7XML.Request
{
    // Наполнение и сохранение заказа
    public class OrderSave
    {
        [XmlRoot(ElementName = "RK7Query")]
        public class SaveOrder
        {
            // Беспараметрический конструктор - необходим для сериализации в XML
            private SaveOrder()
            {

            }

            public SaveOrder(string Guid, List<RK7CMD.Session.Dish> DishList)
            {
                rk7cmd.CMD = "SaveOrder";
                rk7cmd.dontcheckLicense = "true";
                rk7cmd.order.guid = Guid;
                rk7cmd.session.DishList = DishList;
            }

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
                public string dontcheckLicense { get; set; }

                [XmlElement(ElementName = "Order")]
                public Order order = new Order();

                [Serializable]
                public class Order
                {
                    [XmlAttribute]
                    public string guid { get; set; }
                }

                [XmlElement(ElementName = "Session")]
                public Session session = new Session();

                [Serializable]
                public class Session
                {
                    [XmlElement(ElementName = "Dish")]
                    public List<Dish> DishList = new List<Dish>();

                    [Serializable]
                    public class Dish
                    {
                        [XmlAttribute]
                        public string code { get; set; }
                        [XmlAttribute]
                        public string quantity { get; set; }

                    }
                }
            }
        }
    }
}
