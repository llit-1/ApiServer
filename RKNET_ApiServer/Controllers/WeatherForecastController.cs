using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using RKNET_ApiServer.Models;
using System.Net;
using System.Net.Http;

namespace RKNET_ApiServer.Controllers
{
    [BasicAuth.BasicAuthorization]
    //[Authorize(Policy = "YandexRead")]
    public class WeatherForecastController : Controller
    {
        public IActionResult Index()
        {
            return new ObjectResult("Ok");
        }
    }
}
