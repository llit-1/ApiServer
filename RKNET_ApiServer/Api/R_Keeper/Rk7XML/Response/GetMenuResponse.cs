using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace RKNET_ApiServer.Api.R_Keeper.Rk7XML.Response
{
    // Меню
    public class GetMenuResponse
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
            public string CMD { get; set; }
            [XmlAttribute]
            public string ErrorText { get; set; }
            [XmlAttribute]
            public string DateTime { get; set; }
            [XmlAttribute]
            public string WorkTime { get; set; }
            [XmlAttribute]
            public string Processed { get; set; }


            [XmlElement(ElementName = "RK7Reference")]
            public RK7Reference rk7Reference = new RK7Reference();



            // Десериализация RK7QueryResult
            public static RK7QueryResult DeSerializeQueryResult(string xml_answer, string deliveryPriceType)
            {
                if (xml_answer == null)
                {
                    return null;
                }

                else
                {
                    xml_answer = xml_answer.Replace(deliveryPriceType, "deliveryPrice");
                }

                var serializer = new XmlSerializer(typeof(RK7QueryResult));

                using (TextReader reader = new StringReader(xml_answer))
                {
                    return (RK7QueryResult)serializer.Deserialize(reader);
                }
            }
        }

        public class TRK7MenuItem
        {
            [XmlAttribute]
            public string Code { get; set; }
            [XmlAttribute]
            public string Name { get; set; }
            [XmlAttribute]
            public string deliveryPrice { get; set; }
            [XmlAttribute("PRICETYPES-1017607")]
            public string PRICETYPES_1017607 { get; set; }
        }

        [Serializable]
        public class ChildItems
        {
            [XmlElement(ElementName = "TCategListItem")]
            public List<TCategListItem> tCategListItemList = new List<TCategListItem>();

            [XmlElement(ElementName = "TRK7MenuItem")]
            public List<TRK7MenuItem> MenuItems = new List<TRK7MenuItem>();


        }

        [Serializable]
        public class TCategListItem
        {
            [XmlAttribute]
            public string Code { get; set; }
            [XmlAttribute]
            public string Name { get; set; }

            [XmlElement(ElementName = "RIChildItems")]
            public ChildItems childItems = new ChildItems();


        }

        [Serializable]
        public class RIChildItems
        {
            [XmlElement(ElementName = "TCategListItem")]
            public List<TCategListItem> tCategListItemList = new List<TCategListItem>();


        }

        [Serializable]
        public class RK7Reference
        {
            [XmlElement(ElementName = "RIChildItems")]
            public RIChildItems rIChildItems = new RIChildItems();



        }
    }
}
