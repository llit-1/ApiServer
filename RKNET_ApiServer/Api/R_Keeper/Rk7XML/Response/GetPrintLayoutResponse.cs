using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace RKNET_ApiServer.Api.R_Keeper.Rk7XML.Response
{
    // ответы на GetPrintLayout
    public class GetPrintLayoutResponse
    {
        // Предзаказы
        public class PreordersLayout
        {
            public Preorders preOrders;

            public PreordersLayout(string xml_answer)
            {
                var queryResult = RK7QueryResult.DeSerializeQueryResult(xml_answer);
                preOrders = Preorders.DeSerializePreorders(queryResult.layoutResult.xmltext);
            }


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
                public string CMD { get; set; }
                [XmlAttribute]
                public string ErrorText { get; set; }
                [XmlAttribute]
                public string DateTime { get; set; }
                [XmlAttribute]
                public string WorkTime { get; set; }
                [XmlAttribute]
                public string Processed { get; set; }


                [XmlElement(ElementName = "LayoutResult")]
                public LayoutResult layoutResult = new LayoutResult();

                [Serializable]
                public class LayoutResult
                {
                    [XmlText]
                    public string xmltext { get; set; }
                }

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

            [Serializable]
            public class Preorders
            {
                [XmlAttribute]
                public string count { get; set; }

                [XmlElement(ElementName = "Order")]
                public List<Order> OrdersList = new List<Order>();

                [Serializable]
                public class Order
                {
                    [XmlAttribute]
                    public string DateOut { get; set; }
                    [XmlAttribute]
                    public string TTName { get; set; }
                    [XmlAttribute]
                    public string Customer { get; set; }
                    [XmlAttribute]
                    public string TotalSum { get; set; }
                    [XmlAttribute]
                    public string Prepay { get; set; }
                    [XmlAttribute]
                    public string DateCreate { get; set; }
                    [XmlAttribute]
                    public string Creator { get; set; }
                    [XmlAttribute]
                    public string Status { get; set; }
                    [XmlAttribute]
                    public string Guid { get; set; }
                    [XmlAttribute]
                    public string VisitID { get; set; }
                    [XmlAttribute]
                    public string CashNetName { get; set; }

                    [XmlElement(ElementName = "Dishes")]
                    public Dishes dishes = new Dishes();

                    [Serializable]
                    public class Dishes
                    {
                        [XmlAttribute]
                        public string count { get; set; }

                        [XmlElement(ElementName = "Dish")]
                        public List<Dish> DishesList = new List<Dish>();

                        [Serializable]
                        public class Dish
                        {
                            [XmlAttribute]
                            public string Name { get; set; }
                            [XmlAttribute]
                            public string Price { get; set; }
                            [XmlAttribute]
                            public string Quantity { get; set; }
                            [XmlAttribute]
                            public string Cost { get; set; }
                            [XmlAttribute]
                            public string Code { get; set; }
                        }
                    }
                }

                // Десериализация Preorders
                public static Preorders DeSerializePreorders(string xml)
                {
                    try
                    {
                        var serializer = new XmlSerializer(typeof(Preorders));
                        using (TextReader reader = new StringReader(xml))
                        {
                            return (Preorders)serializer.Deserialize(reader);
                        }
                    }
                    catch
                    {
                        return new Preorders();
                    }
                }
            }


        }
    }
}
