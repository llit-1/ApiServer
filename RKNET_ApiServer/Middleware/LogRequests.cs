using RKNET_ApiServer.Models;
using System.Text.RegularExpressions;


namespace RKNET_ApiServer.Middleware
{
    public class LogRequests
    {
        private readonly RequestDelegate _next;
        public LogRequests(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var reqLog = new RequestLog();
            try
            {
                // REQUEST
                reqLog.Url = string.Format("{0}://{1}{2}", context.Request.Scheme, context.Request.Host, context.Request.Path);
                reqLog.Path = context.Request.Path;
                
                // запрошенное действие по ссылке из справочника               
                reqLog.Action = RequestLog.Actions.FirstOrDefault(a => Regex.IsMatch(reqLog.Path, a.Key, RegexOptions.IgnoreCase)).Value;

                if(reqLog.Action == "Yandex/order/{orderId}")
                {
                    switch (context.Request.Method)
                    {
                        case "GET":
                            reqLog.Action = "ЯндексЕда: получение информации по заказу";
                            break;
                        case "PUT":
                            reqLog.Action = "ЯндексЕда: обновление заказа";
                            break;
                        case "DELETE":
                            reqLog.Action = "ЯндексЕда: удаление заказа";
                            break;
                    }
                }

                if (reqLog.Action == "DeliveryClub/orders/{restaurantId}/{id}")
                {
                    switch (context.Request.Method)
                    {
                        case "GET":
                            reqLog.Action = "Delivery Club: получение информации по заказу";
                            break;
                        case "PUT":
                            reqLog.Action = "Delivery Club: изменение статуса заказа";
                            break;
                    }
                }

                // получаем тело запроса
                var requestBodyStream = new MemoryStream();
                var originalRequestBody = context.Request.Body;

                await context.Request.Body.CopyToAsync(requestBodyStream);
                requestBodyStream.Seek(0, SeekOrigin.Begin);

                var requestBody = new StreamReader(requestBodyStream).ReadToEnd();

                requestBodyStream.Seek(0, SeekOrigin.Begin);
                context.Request.Body = requestBodyStream;

                // получаем client_id
                var token = context.Request.Headers.FirstOrDefault(h => h.Key == "Authorization").Value.ToString();

                if (token.Length > 7)
                {
                    token = token.Substring(7, token.Length - 7);
                }                

                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();

                //var queryString = Logger.QueryStringHelper.QueryStringToDict(requestBody);
                //var json = Json.Net.JsonNet.Serialize(queryString);
                //var body = Newtonsoft.Json.JsonConvert.DeserializeObject<reqBody>(json);

                if (requestBody.Length <= 1000)
                {
                    //reqLog.ReqBody = requestBody;
                }

                // из токена
                if (handler.CanReadToken(token))
                {
                    var jwtSecurityToken = handler.ReadJwtToken(token);
                    reqLog.Client = jwtSecurityToken.Claims.FirstOrDefault(c => c.Type == "client_id").Value;                    
                }
                else // базовая авторизация
                {
                    var authorizationHeader = context.Request.Headers["Authorization"].ToString();
                    var authHeaderRegex = new Regex(@"Basic (.*)");

                    if (authHeaderRegex.IsMatch(authorizationHeader))
                    {
                        var authBase64 = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(authHeaderRegex.Replace(authorizationHeader, "$1")));
                        var authSplit = authBase64.Split(Convert.ToChar(":"), 2);
                        var authUsername = authSplit[0];
                        reqLog.Client = authUsername;
                        //var authPassword = authSplit.Length > 1 ? authSplit[1] : throw new Exception("Unable to get password");
                    }                    
                }
                // из тела запроса
                //else
                //{
                //    if(body.client_id != null)
                //    {
                //        reqLog.Client = body.client_id;
                //        reqLog.ReqBody = requestBody.Replace(body.client_secret, "*********");                        
                //    }                    
                //}
                


                // RESPONSE
                await _next(context);
                reqLog.Status = new HttpStatus(context.Response.StatusCode);
                
                /*
                string responseBody = string.Empty;
                using (var swapStream = new MemoryStream())
                {

                    var originalResponseBody = context.Response.Body;
                    context.Response.Body = swapStream;
                    await _next(context);
                    swapStream.Seek(0, SeekOrigin.Begin);

                    responseBody = new StreamReader(swapStream).ReadToEnd(); //тело ответа

                    swapStream.Seek(0, SeekOrigin.Begin);
                    await swapStream.CopyToAsync(originalResponseBody);
                    context.Response.Body = originalResponseBody;

                    reqLog.Status = new HttpStatus(context.Response.StatusCode);                    
                    
                    var webPage = context.Response.Headers.FirstOrDefault(h => h.Key == "WEB");
                    if (webPage.Key != "WEB")
                    {
                        var result = Newtonsoft.Json.JsonConvert.DeserializeObject<RKNet_Model.Result<ResponseResult>>(responseBody);
                        if (result != null)
                        {
                            reqLog.Action = result.ActionName;
                        }
                    }
                    else
                    {
                        reqLog.Action = webPage.Value;
                    }
                }               
                */

                // проверяем на исключения перед логгированием               
                var logEnable = true;

                foreach (var except in RequestLog.LogExeptions)
                {                    
                    if (reqLog.Path.StartsWith(except))
                    {
                        logEnable = false;
                        break;
                    }
                }

                // логгиреум
                if (logEnable)
                {
                    Events.Logging(reqLog);
                }
            }
            catch (Exception ex)
            {
                Events.Logging(ex.Message);
            }            
        }

        
        // класс для десериалихации из тела ответа только выборочных параметров
        private class ResponseResult
        {
            public bool Ok = true; // статус выполнения (успешно или ошибка)

            public string ActionName = string.Empty; // Наименование запрошенного действия Api                
        }

        // класс запроса токена (для дессериализации)
        private class reqBody
        {
            public string grant_type;
            public string scope;
            public string client_id;
            public string client_secret;
        }
    }

    public static class LogRequestsExtensions
    {
        public static IApplicationBuilder UseLogRequests(this IApplicationBuilder app)
        {
            return app.UseMiddleware<LogRequests>();
        }
    }
}
