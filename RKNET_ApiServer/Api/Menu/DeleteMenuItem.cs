using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace RKNET_ApiServer.Api.Menu
{
    public partial class Actions
    {
        [HttpGet]
        public IActionResult DeleteMenuItem(int Id)
        {
            var result = new RKNet_Model.Result<string>();
            try
            {
                var item = rknetdb.MenuItems.FirstOrDefault(i => i.Id == Id);
                if (item == null)
                {
                    result.Ok = false;
                    result.ErrorMessage = "Позиция меню с Id=" + Id + " не найдена в базе данных.";
                    return new ObjectResult(result);
                }
                else
                {
                    rknetdb.MenuItems.Remove(item);
                    rknetdb.SaveChanges();
                    SetLastChange();
                    result.Data = "ok";
                }
            }
            catch (Exception ex)
            {
                result.Ok = false;
                result.ErrorMessage = ex.Message;
                result.ExceptionText = ex.ToString();
            }
            return new ObjectResult(result);
        }        
    }
}
