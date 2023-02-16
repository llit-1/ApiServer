using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace RKNET_ApiServer.Api.Menu
{
    public partial class Actions
    {
        [HttpPost]
        public IActionResult AddMenuCategory(RKNet_Model.Menu.Category cat)
        {
            var result = new RKNet_Model.Result<string>();

            try
            {
                if (string.IsNullOrEmpty(cat.Name))
                {
                    result.Ok = false;
                    result.ErrorMessage = "Имя категории не может быть пустым";
                    return new ObjectResult(result);
                }

                var category = new RKNet_Model.Menu.Category();                

                category.Name = cat.Name;
                category.Image = cat.Image;
                category.UpdateImageDate();
                category.ParentCategoryId = cat.ParentCategoryId;                

                var nameExist = rknetdb.MenuCategorys.FirstOrDefault(c => c.Name == category.Name && c.ParentCategoryId == category.ParentCategoryId);
                if(nameExist != null)
                {
                    result.Ok = false;                    
                    result.ErrorMessage = "Категория с именем \"" + nameExist.Name + "\" уже существует в данном разделе. Выберите другое имя.";
                    return new ObjectResult(result);
                }
                                
                

                rknetdb.MenuCategorys.Add(category);
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
