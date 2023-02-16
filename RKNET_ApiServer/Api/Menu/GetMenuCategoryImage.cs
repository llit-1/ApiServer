using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using Microsoft.AspNetCore.Authorization;

namespace RKNET_ApiServer.Api.Menu
{
    public partial class Actions
    {
        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetMenuCategoryImage(int Id)
        {
            var result = new RKNet_Model.Result<byte[]>();
            
            try 
            {
                var category = rknetdb.MenuCategorys.FirstOrDefault(c => c.Id == Id);
                if(category == null)
                {
                    result.Ok = false;
                    result.ErrorMessage = "категории не существует";
                }
                else
                {
                    if(category.Image == null)
                    {
                        var defItemImage = rknetdb.Files.FirstOrDefault(f => f.Name == "MenuCategoryImage.jpg");
                        if (defItemImage != null)
                        {
                            if (defItemImage.Data != null)
                            {
                                result.Data = defItemImage.Data;
                            }
                            else
                            {
                                result.Ok = false;
                                result.ErrorMessage = "картинки нет в базе данных";
                            }
                        }
                        else
                        {
                            result.Ok = false;
                            result.ErrorMessage = "картинки нет в базе данных";
                        }
                    }
                    else
                    {
                        result.Data = category.Image;
                    }                    
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
