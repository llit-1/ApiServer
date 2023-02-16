using System;
using System.Xml;
using System.Xml.Serialization;

namespace RKNET_ApiServer.Api.R_Keeper.Rk7XML.Request
{
    // Представления данных с касс, настраиваемые в менеджерке
    public class CashLayouts
    {
        // GetPrintLayout
        [XmlRoot(ElementName = "RK7Query")]
        public class GetPrintLayout
        {
            // Конструктор запроса
            public GetPrintLayout(string LayoutCode, LayoutFormat Format)
            {
                rk7cmd.CMD = "GetPrintLayout";
                rk7cmd.format = Format.ToString();

                rk7cmd.layout = new RK7CMD.Layout();
                rk7cmd.layout.code = LayoutCode;
            }

            // Беспараметрический конструктор - необходим для сериализации в XML
            private GetPrintLayout() { }

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
                public string format { get; set; }

                // Элементы
                [XmlElement(ElementName = "Layout")]
                public Layout layout { get; set; }

                // Классы элементов
                [Serializable]
                public class Layout
                {
                    [XmlAttribute]
                    public string code { get; set; }
                }
            }

            // Типы возвращаемых значений схем отчетов
            public enum LayoutFormat : int
            {
                xml = 1,
                txt = 2,
                xls = 3,
                odt = 4,
                ods = 5,
                pdf = 6,
                html = 7
            }
        }
    }
}
