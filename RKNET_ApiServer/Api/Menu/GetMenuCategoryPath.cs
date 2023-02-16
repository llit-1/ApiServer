using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RKNET_ApiServer.Api.Menu
{
    public partial class Actions
    {
        // Путь к категории меню
        [HttpGet]        
        public IActionResult GetMenuCategoryPath(int Id)
        {
            var result = new RKNet_Model.Result<List<RKNet_Model.Menu.Category>>();

            try
            {
                //var curCategory = rknetdb.MenuCategorys.FirstOrDefault(c => c.Id == Id);
                //var parentCategory = rknetdb.MenuCategorys.Include(c => c.Categories).FirstOrDefault(c => c.Categories.Contains(curCategory));
                //var categoryPath = new List<RKNet_Model.Menu.Category>();

                //while (parentCategory != null)
                //{
                //    categoryPath.Add(new RKNet_Model.Menu.Category // формируем коллекцию пути из категорий, но без картинок, позиций и подкатегорий
                //    {
                //        Id = parentCategory.Id,
                //        Name = parentCategory.Name,
                //        ParentCategoryId = parentCategory.ParentCategoryId,
                //    });
                //    parentCategory = rknetdb.MenuCategorys.Include(c => c.Categories).FirstOrDefault(c => c.Categories.Contains(parentCategory));
                //}
                //categoryPath.Reverse();
                //result.Data = categoryPath;


                // без картинок
                var curCategory = rknetdb.MenuCategorys
                    .Select(c => new RKNet_Model.Menu.Category { Id = c.Id, Name = c.Name, ParentCategoryId = c.ParentCategoryId })
                    .FirstOrDefault(c => c.Id == Id);

                var categoryPath = new List<RKNet_Model.Menu.Category>();

                if (curCategory != null)
                {
                    var parentCategory = rknetdb.MenuCategorys
                    .Include(c => c.Categories)
                    .Select(c => new RKNet_Model.Menu.Category { Id = c.Id, Name = c.Name, ParentCategoryId = c.ParentCategoryId })
                    .FirstOrDefault(c => c.Id == curCategory.ParentCategoryId);

                    // формируем коллекцию пути из категорий, но без картинок, позиций и подкатегорий
                    while (parentCategory != null)
                    {
                        categoryPath.Add(parentCategory);
                        parentCategory = rknetdb.MenuCategorys
                            .Include(c => c.Categories)
                            .Select(c => new RKNet_Model.Menu.Category { Id = c.Id, Name = c.Name, ParentCategoryId = c.ParentCategoryId })
                            .FirstOrDefault(c => c.Id == parentCategory.ParentCategoryId);
                    }
                    categoryPath.Reverse();
                }
                result.Data = categoryPath;
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
