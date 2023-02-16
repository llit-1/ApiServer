using System;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace NxApi
{
    public class getNonce
    {
        public string error;
        public string errorString;
        public Reply reply;

        public class Reply
        {
            public string realm;
            public string nonce;
        }

        // запрос на сервер
        private getNonce NXRequest(string serverString)
        {
            var result = new getNonce();

            // Игнорируем неверные сертификаты сервера
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            //var authString = Convert.ToBase64String(Encoding.Default.GetBytes(nxSystem.Login + ":" + nxSystem.Password));


            var request = new WebClient();
            //request.Headers["Authorization"] = "Basic " + authString;

            var connectionString = serverString + "/api/getNonce";

            try
            {
                var stream = request.OpenRead(connectionString);

                var serializer = new JsonSerializer();
                using (var sr = new StreamReader(stream))
                {
                    var jsonString = sr.ReadToEnd();
                    result = JsonConvert.DeserializeObject<getNonce>(jsonString);
                }

                stream.Flush();
                stream.Close();
                request.Dispose();
            }

            catch (Exception e)
            {
                result.error = "1";
                result.errorString = e.ToString();
            }

            return result;
        }

        // конструкторы
        private getNonce()
        {
            
        }

        public getNonce(string NxServerAdressAndPort)
        {
            var request = NXRequest(NxServerAdressAndPort);
            error = request.error;

            if(error == "0")
            {
                reply = request.reply;
            }
            else
            {
                errorString = request.errorString;
            }

        }


    }
}
