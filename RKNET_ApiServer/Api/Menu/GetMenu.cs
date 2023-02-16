using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace RKNET_ApiServer.Api.Menu
{
    public partial class Actions
    {
        [HttpGet]
        public IActionResult GetMenu()
        {
            var result = new RKNet_Model.Result<RKNet_Model.Menu.Category>();
            try 
            {
                var cats = rknetdb.MenuCategorys.Include(c => c.Categories).Include(c => c.Items);
                var items = rknetdb.MenuItems.Where(i => i.ParentCategory == null).ToList();
                
                result.Data = new RKNet_Model.Menu.Category();
                
                if (cats != null)
                    result.Data.Categories = cats.ToList();
                if (items != null)
                    result.Data.Items = items.ToList();
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
