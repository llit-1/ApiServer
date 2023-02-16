using Microsoft.AspNetCore.Mvc;

namespace RKNET_ApiServer.Api.Menu
{
    public partial class Actions
    {
        [HttpGet]
        public IActionResult GetMenuItemImage(int Id)
        {
            var result = new RKNet_Model.Result<byte[]>();

            try
            {
                var menuItem = rknetdb.MenuItems.FirstOrDefault(i => i.Id == Id);
                if (menuItem == null)
                {
                    result.Ok = false;
                    result.ErrorMessage = "позиции меню не найдено";
                }
                else
                {
                    if (menuItem.Image == null)
                    {
                        var defItemImage = rknetdb.Files.FirstOrDefault(f => f.Name == "MenuItemImage.jpg");
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
                        result.Data = menuItem.Image;
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
