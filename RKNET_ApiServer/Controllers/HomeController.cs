using Microsoft.AspNetCore.Mvc;
using RKNET_ApiServer.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;


namespace RKNET_ApiServer.Controllers
{
    //[Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private DB.MSSQLDBContext mssql;
        private DB.RknetDbContext rknetdb;

        public HomeController(ILogger<HomeController> logger, DB.RknetDbContext rknetContext, DB.MSSQLDBContext mssqlContext)
        {
            _logger = logger;
            rknetdb = rknetContext;
            mssql = mssqlContext;
        }           
        

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CashClients()
        {
            return View();
        }
        public IActionResult ClientsTable()
        {
            return PartialView(SignalR.CashesHub.cashClients);
        }

        public IActionResult Swagger()
        {
            return View();
        }




        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}