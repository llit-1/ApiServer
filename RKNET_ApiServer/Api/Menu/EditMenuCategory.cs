using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RKNET_ApiServer.Api.Menu
{
    public partial class Actions
    {
        [HttpPost]
        public IActionResult EditMenuCategory(RKNet_Model.Menu.Category category)
        {
            var result = new RKNet_Model.Result<string>();
            try
            {
                var cat = rknetdb.MenuCategorys.FirstOrDefault(c => c.Id == category.Id);
                if(cat == null)
                {
                    result.Ok = false;
                    result.ErrorMessage = "категории с Id = " + category.Id + " нет в базе данных";
                    return new ObjectResult(result);
                }

                cat.Name = category.Name;
                cat.Image = category.Image;
                cat.ImageUpdated = category.ImageUpdated;

                rknetdb.MenuCategorys.Update(cat);
                
                rknetdb.SaveChanges();
                SetLastChange();
                
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
    }
}
