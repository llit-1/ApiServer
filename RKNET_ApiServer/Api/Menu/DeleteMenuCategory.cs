using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace RKNET_ApiServer.Api.Menu
{
    public partial class Actions
    {
        [HttpGet]
        public IActionResult DeleteMenuCategory(int Id)
        {
            var result = new RKNet_Model.Result<string>();
            try
            {
                var cat = rknetdb.MenuCategorys
                    .Include(c => c.Categories)
                    .Include(c => c.Items)
                    .FirstOrDefault(c => c.Id == Id);
                if(cat == null)
                {
                    result.Ok = false;
                    result.ErrorMessage = "Категория с Id=" + Id + " не найдена в базе данных.";
                    return new ObjectResult(result);
                }
                else
                {
                    //rknetdb.MenuItems.RemoveRange(cat.Items);                    
                    //rknetdb.MenuCategorys.Remove(cat);
                    CategoryRecoursiveDelete(cat);

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


        // рекурсивное удаление всех подкатегорий и вложенных позиций меню
        private void CategoryRecoursiveDelete(RKNet_Model.Menu.Category Category)
        {            
            foreach (var cat in Category.Categories)
            {
                var categ = rknetdb.MenuCategorys.Include(c => c.Categories).Include(c => c.Items).FirstOrDefault(c => c.Id == cat.Id);
                CategoryRecoursiveDelete(categ);
            }
            rknetdb.MenuItems.RemoveRange(Category.Items);
            rknetdb.MenuCategorys.Remove(Category);
        }
    }
}
