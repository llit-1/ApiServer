using System.Text;
using System.Net;
using RKNet_Model;

namespace RKNET_ApiServer.Api.R_Keeper.Rk7XML
{
    public class RK7
    {
        // xml запрос-ответ
        public Result<string> SendRequest(string ip, string xml_request, string port, string user, string password)
        {
            var result = new Result<string>();

            string xml_answer = null;
            byte[] postBytes = Encoding.UTF8.GetBytes(xml_request);

            // Игнорируем неверные сертификаты сервера
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            // Формируем строку подключения
            string uri = "https://" + ip + ":" + port + "/rk7api/v0/xmlinterface.xml";

            // Параметры HTTP запроса
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Credentials = new NetworkCredential(user, password);
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
            request.Method = "POST";
            request.ContentType = "application/xml";
            request.Accept = "application/xml";

            // Выполнение запроса
            try
            {
                request.ContentLength = postBytes.Length;
                Stream requestStream = request.GetRequestStream();

                requestStream.Write(postBytes, 0, postBytes.Length);
                requestStream.Close();

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                xml_answer = new StreamReader(response.GetResponseStream()).ReadToEnd();
                result.Data = xml_answer;                
            }
            catch (Exception e)
            {
                result.Ok = false;
                result.ErrorMessage = e.Message;
                result.ExceptionText = e.ToString();
            }

            return result;
        }
    }
}
