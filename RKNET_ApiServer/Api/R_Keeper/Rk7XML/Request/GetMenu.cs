using System;
using System.Xml;
using System.Xml.Serialization;

namespace RKNET_ApiServer.Api.R_Keeper.Rk7XML.Request
{
    // Список Меню
    public class GetMenu
    {
        [XmlRoot(ElementName = "RK7Query")]
        public class GetRefData
        {
            // Конструктор запроса
            public GetRefData()
            {
                rk7cmd.CMD = "GetRefData"; // Получить данные справочников
                rk7cmd.RefName = "CATEGLIST"; // Справочник категорий меню
                rk7cmd.OnlyActive = "true"; // Только действующие (активные) категории
                rk7cmd.WithMacroProp = "1"; // Включить генерируемые свойства (в них содержатся цены)
                rk7cmd.MacroPropTags = "0"; // Генерируемы свойства в виде категорий или по отдельности (цены только с 0 приходят)
                rk7cmd.WithChildItems = "3"; // Включить все дочерние элементы (позции меню)
                                             // Фильтр, получаем только то, что указано
                rk7cmd.PropMask = "RIChildItems.(Code, Name, RIChildItems.Code, RIChildItems.Name, RIChildItems.PRICETYPES*, RIChildItems.(Code, Name, RIChildItems.Code, RIChildItems.Name, RIChildItems.PRICETYPES*, RIChildItems.(Code, Name, RIChildItems.Code, RIChildItems.Name, RIChildItems.PRICETYPES*)))";
            }

            // Беспараметрический конструктор - необходим для сериализации в XML
            //private GetRefData() { }

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
                public string RefName { get; set; }
                [XmlAttribute]
                public string OnlyActive { get; set; }
                [XmlAttribute]
                public string WithMacroProp { get; set; }
                [XmlAttribute]
                public string MacroPropTags { get; set; }
                [XmlAttribute]
                public string WithChildItems { get; set; }
                [XmlAttribute]
                public string PropMask { get; set; }
            }
        }
    }
}
