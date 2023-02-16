using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace RKNET_ApiServer.Api.R_Keeper.Rk7XML.Request
{
    // Удаление блюд из заказа
    public class OrderDishesDelete
    {
        [XmlRoot(ElementName = "RK7Query")]
        public class RK7Query
        {
            // Беспараметрический конструктор - необходим для сериализации в XML
            private RK7Query()
            {

            }

            public RK7Query(string guid, List<RK7CMD.Session.Dish> DishDeleteList)
            {
                rk7cmd.CMD = "SaveOrder";
                rk7cmd.dontcheckLicense = "true";
                rk7cmd.order.guid = guid;
                rk7cmd.session.DishList = DishDeleteList;
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
                    [XmlAttribute]
                    public string uni { get; set; }

                    [XmlElement(ElementName = "Dish")]
                    public List<Dish> DishList = new List<Dish>();

                    [Serializable]
                    public class Dish
                    {
                        [XmlAttribute]
                        public string uni { get; set; }
                        [XmlAttribute]
                        public string code { get; set; }
                        [XmlAttribute]
                        public string price { get; set; }

                        [XmlElement(ElementName = "Void")]
                        public Void delete = new Void();

                        [Serializable]
                        public class Void
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
}
