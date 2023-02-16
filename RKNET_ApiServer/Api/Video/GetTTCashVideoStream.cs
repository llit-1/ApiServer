using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace RKNET_ApiServer.Api.Video
{
    public partial class Actions
    {
        /// <summary>
        /// Поток видео с касс ТТ
        /// </summary>
        /// <remarks>
        /// Выдча потока видео с кассы точки
        /// </remarks>
        /// <param name="ttCode">код тт</param>
        /// <param name="position">дата и время начала видео в формате "dd-MM-yyTHH-mm-ss"</param>
        /// <param name="duration">продолжительность фрагмента в секундах</param>
        /// <param name="auth">ключ доступа</param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetTTCashVideoStream(string ttCode, string position, string duration, string auth)
        {
            var result = new RKNet_Model.Result<string>();

            try
            {
                // проверяем ключ доступа
                var key = (int.Parse(ttCode) * int.Parse(duration) * 5155 - 6).ToString();
                if(auth != key)
                {
                    result.Ok = false;
                    result.ErrorMessage = $"неверный ключ доступа";
                    return new ObjectResult(result);
                }

                var username = "rknet";
                var password = "Adm4Rknet";
                //var cameraId = "897791c3-319f-9c51-3954-62244d349f18";
                var serverAddress = "nx.ludilove.ru:7001";

                var https = HttpContext.Request.IsHttps;
                var protocol = "http";
                if (https) protocol = "https";

                var serverString = protocol + "://" + serverAddress;

                // код точки
                var tt = rknetdb.TTs.FirstOrDefault(t => t.Code == int.Parse(ttCode));
                if(tt == null)
                {
                    result.Ok = false;
                    result.ErrorMessage = $"тт с кодом {ttCode} не обнаружена в базе данных";
                    return new ObjectResult(result);
                }

                // id кассовой камеры на тт
                var cashCam = rknetdb.NxCameras.Include(c => c.CamGroup).Include(c => c.TT).Where(c => c.CamGroup.Id == 4).FirstOrDefault(c => c.TT.Code == int.Parse(ttCode));

                if(cashCam == null)
                {
                    result.Ok = false;
                    result.ErrorMessage = $"для точки {tt.Name} не задана кассовая камера в настройках на Портале";
                    return new ObjectResult(result);
                }

                var cameraId = cashCam.Guid;

                // дата и время начала видео
                DateTime positionDate;
                string format = "dd-MM-yyTHH-mm-ss";

                var isDateOk = DateTime.TryParseExact(position, format, System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None, out positionDate);
                if(!isDateOk)
                {
                    result.Ok = false;
                    result.ErrorMessage = $"передано некорректное значение времени начала видео: {position}";
                    return new ObjectResult(result);
                }
                
                // продолжительность фрагмента
                int dur;
                var isDurationOk = int.TryParse(duration, out dur);
                if(!isDurationOk)
                {
                    result.Ok = false;
                    result.ErrorMessage = $"передано некорректное значение продолжительности фрагмента видео: {duration}";
                    return new ObjectResult(result);
                }

                // авторизуемся на NX сервере
                var getNonce = new NxApi.getNonce(serverString);

                if(getNonce.error != "0")
                {
                    result.Ok = false;
                    result.ErrorMessage = $"ошибка запроса к серверу NX: {getNonce.errorString}";
                    return new ObjectResult(result);
                }

                var realm = getNonce.reply.realm;
                var nonce = getNonce.reply.nonce;

                var digest = CreateMD5(username + ":" + realm + ":" + password);
                var partial_ha2 = CreateMD5("GET" + ":");
                var simplified_ha2 = CreateMD5(digest + ":" + nonce + ":" + partial_ha2);
                var authKey = Convert.ToBase64String(Encoding.Default.GetBytes(username + ":" + nonce + ":" + simplified_ha2));

                //hls
                //var cameraURL = protocol + "://" + serverAddress + "/hls/" + cameraId + ".m3u8?auth=" + authKey;

                //direct dowload mkv or ts
                //var dateTime = DateTime.Now.ToString("yyyy-MM-ddT10:00");
                //var cameraURL = $"{protocol}://{serverAddress}/hls/{cameraId}.mkv?pos={dateTime}&duration=10&auth={authKey}";

                //http
                var dateTime = positionDate.ToString("yyyy-MM-ddTHH:mm:ss");
                var cameraURL = $"{protocol}://{serverAddress}/media/{cameraId}.mp4?pos={dateTime}&duration={dur}&rt&auth={authKey}";




                //result.Data = cameraURL;
                //return new ObjectResult(result);

                var stream = await streamingService.GetVideoStream(cameraURL);
                return new FileStreamResult(stream, "video/mp4");
            }
            catch (Exception ex)
            {
                result.Ok = false;
                result.ErrorMessage = ex.Message;
                result.ExceptionText = ex.ToString();
                return new ObjectResult(result);
            }                       
        }
    }
}
