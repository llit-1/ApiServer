using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace RKNET_ApiServer.Api.R_Keeper.Rk7XML.Response
{
    // Наполнение и сохранение заказа
    public class OrderSaveResponse
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
    }
}
