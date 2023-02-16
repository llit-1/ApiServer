using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace RKNET_ApiServer.Api.Access
{
    public partial class Actions
    {
        [HttpGet]
        public IActionResult GetUser(string userLogin)
        {
            var result = new RKNet_Model.Result<RKNet_Model.Account.User>();
            var user = rknetdb.Users
                .Include(u => u.TTs)
                .FirstOrDefault(u => u.Login.ToLower() == userLogin.ToLower());

            if(user == null)
            {
                result.Ok = false;
                result.ErrorMessage = $"Пользоавтель с логином {userLogin} не найден в базе данных.";
                return new ObjectResult(result);
            }

            result.Data = user;            
            return new ObjectResult(result);
        }
    }
}
