using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace RKNET_ApiServer.Api.R_Keeper.Rk7XML.Request
{
    public class Serialize
    {
        // Сериализация XML строки из класса запроса
        public static string ToString<TType>(TType sourceObject)
        {
            if (sourceObject == null)
            {
                return string.Empty;
            }

            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            // Используем XmlSerializer для перобразования в XML строку
            var xmlserializer = new XmlSerializer(typeof(TType));
            //var stringWriter = new StringWriter();
            var settings = new XmlWriterSettings();
            settings.Encoding = Encoding.UTF8;

            using (StringWriter writer = new Utf8StringWriter())
            {
                xmlserializer.Serialize(writer, sourceObject, ns);
                return writer.ToString();
            }
        }

        // Переопределение кодировки XML с Utf-16 на Utf-8
        public class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
        }
    }
}
