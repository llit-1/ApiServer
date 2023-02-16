using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace RKNET_ApiServer.Api.R_Keeper.Rk7XML.Response
{
    // получение информации о кассовом сервере
    public class GetSystemInfo2Response
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


            // Элементы
            [XmlElement(ElementName = "SystemInfo")]
            public SystemInfo systemInfo = new SystemInfo();

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
        public class SystemInfo
        {
            [XmlAttribute]
            public string ProcessID { get; set; }
            [XmlAttribute]
            public string RestCode { get; set; }
            [XmlAttribute]
            public string uptime { get; set; }         

            [XmlElement(ElementName = "CashGroup")]
            public CashGroup cashGroup = new CashGroup();

            [XmlElement(ElementName = "Restaurant")]
            public Restaurant restaurant = new Restaurant();

            [XmlElement(ElementName = "CommonShift")]
            public CommonShift commonShift = new CommonShift();
        }

        [Serializable]
        public class CashGroup
        {
            [XmlAttribute]
            public string id { get; set; }
            [XmlAttribute]
            public string code { get; set; }
            [XmlAttribute]
            public string name { get; set; }            
        }

        [Serializable]
        public class Restaurant
        {
            [XmlAttribute]
            public string id { get; set; }
            [XmlAttribute]
            public string code { get; set; }
            [XmlAttribute]
            public string name { get; set; }
        }

        [Serializable]
        public class CommonShift
        {
            [XmlAttribute]
            public string ShiftDate { get; set; }
            [XmlAttribute]
            public string ShiftNum { get; set; }
            [XmlAttribute]
            public string ShiftStartTime { get; set; }
        }
    }
}

