using System;
using System.Xml;
using System.Xml.Serialization;

namespace RKNET_ApiServer.Api.R_Keeper.Rk7XML.Request
{
    public class SetDishRests
    {
        [XmlRoot(ElementName = "RK7Query")]
        public class RK7Query
        {
            // Конструктор запроса
            public RK7Query(string dishCode, bool stop)
            {
                rk7cmd.CMD = "SetDishRests";
                rk7cmd.dishRest.code = dishCode;
                rk7cmd.dishRest.prohibited = stop;
            }

            // Беспараметрический конструктор - необходим для сериализации в XML
            private RK7Query() { }

            // Элемент RK7CMD
            [XmlElement(ElementName = "RK7Command")]
            public RK7Command rk7cmd = new RK7Command();

            [Serializable]
            public class RK7Command
            {
                // Атрибуты
                [XmlAttribute]
                public string CMD { get; set; }

                [XmlElement(ElementName = "DishRest")]
                public DishRest dishRest = new DishRest();

                [Serializable]
                public class DishRest
                {
                    [XmlAttribute]
                    public string code { get; set; }
                    [XmlAttribute]
                    public bool prohibited { get; set; }
                }
            }
        }
    }
}
