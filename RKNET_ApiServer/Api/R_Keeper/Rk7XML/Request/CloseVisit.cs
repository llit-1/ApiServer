using System;
using System.Xml;
using System.Xml.Serialization;

namespace RKNET_ApiServer.Api.R_Keeper.Rk7XML.Request
{
    // Удаление заказа (закрытие визита)
    public class CloseVisit
    {
        [XmlRoot(ElementName = "RK7Query")]
        public class RK7Query
        {
            // Конструктор запроса
            public RK7Query(string visitId)
            {
                rk7cmd.CMD = "CloseVisit";
                rk7cmd.VisitID = visitId;
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
                [XmlAttribute]
                public string VisitID { get; set; }
            }
        }
    }
}
