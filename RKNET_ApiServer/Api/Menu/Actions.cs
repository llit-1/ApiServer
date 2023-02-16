using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;
using System.Text;

namespace RKNET_ApiServer.Api.Menu
{
    [Authorize]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Меню доставки")]
    [Route("Menu/[action]")]
    public partial class Actions :ControllerBase
    {
        private DB.RknetDbContext rknetdb;
        private DB.MSSQLDBContext mssql;

        public Actions(DB.RknetDbContext rknetdbContext, DB.MSSQLDBContext mssqlContext)
        {
            rknetdb = rknetdbContext;
            mssql = mssqlContext;
        }

        // дата и время в формате RFC3339 последнего изменения меню
        private IActionResult SetLastChange()
        {
            var result = new RKNet_Model.Result<string>();

            try
            {
                var menuLastChange = rknetdb.LastChanges.FirstOrDefault(l => l.Name == "Меню");

                if (menuLastChange == null)
                {
                    menuLastChange = new RKNet_Model.LastChange();
                    menuLastChange.Name = "Меню";
                    menuLastChange.Date = DateTime.Now.ToString("yyy-MM-ddTHH:mm:ss.ffffff+03:00");
                    rknetdb.LastChanges.Add(menuLastChange);
                }
                else
                {
                    menuLastChange.Name = "Меню";
                    menuLastChange.Date = DateTime.Now.ToString("yyy-MM-ddTHH:mm:ss.ffffff+03:00");
                    rknetdb.LastChanges.Update(menuLastChange);
                }
                rknetdb.SaveChanges();
                result.Data = "ok";
            }
            catch (Exception ex)
            {
                result.Ok = false;
                result.ErrorMessage = ex.Message;
                result.ExceptionText = ex.ToString();
            }

            return new ObjectResult(result);
        }

        // получаем строку, закодированную в SHA1
        private static string ShaHashString(byte[] input)
        {
            if (input == null)
                return null;

            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(input);
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    // can be "x2" if you want lowercase
                    sb.Append(b.ToString("X2"));
                }

                return sb.ToString();
            }
        }
    }
}
