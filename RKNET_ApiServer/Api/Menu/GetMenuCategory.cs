using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RKNET_ApiServer.Api.Menu
{
    public partial class Actions
    {
        // Категория меню
        [HttpGet]
        public IActionResult GetMenuCategory(int Id, bool withImage) // если Id == 0, то вызвращается корневой раздел меню
        {
            var result = new RKNet_Model.Result<RKNet_Model.Menu.Category>();            
            try
            {
                // корневая категория
                if (Id == 0)
                {
                    var category = new RKNet_Model.Menu.Category();
                    // с картинками
                    if (withImage)
                    {
                        category.Categories = rknetdb.MenuCategorys.Where(c => c.ParentCategoryId == null).ToList();
                        category.Items = rknetdb.MenuItems.Where(i => i.ParentCategory == null).ToList();
                    }
                    // без картинок
                    else
                    {                        
                        category.Categories = rknetdb.MenuCategorys
                            .Where(c => c.ParentCategoryId == null)
                            .Select(c => new RKNet_Model.Menu.Category { Id = c.Id, Name = c.Name }).ToList();

                        category.Items = rknetdb.MenuItems
                            .Where(i => i.ParentCategory == null)
                            .Select(i => new RKNet_Model.Menu.Item { Id = i.Id, marketName = i.marketName, Description = i.Description, Enabled = i.Enabled }).ToList();                       
                    }
                    result.Data = category;
                }   
                // не корневая категория меню
                else
                {
                    // вместе с картинками
                    if (withImage)
                    {
                        var category = rknetdb.MenuCategorys.Include(c => c.Categories).Include(c => c.Items).FirstOrDefault(c => c.Id == Id);
                        if (category != null) result.Data = category;
                        else
                        {
                            result.Ok = false;
                            result.ErrorMessage = "указанная категория не обнаружена в базе данных";
                        }
                    }
                    else
                    {
                        // без картинок                    
                        var category = rknetdb.MenuCategorys
                            .Select(c => new RKNet_Model.Menu.Category { Id = c.Id, Name = c.Name, ParentCategoryId = c.ParentCategoryId })
                            .FirstOrDefault(c => c.Id == Id);

                        if (category != null)
                        {
                            // добавляем подкатегории
                            var subCatItems = rknetdb.MenuCategorys
                                .Where(c => c.ParentCategoryId == category.Id)
                                .Select(c => new RKNet_Model.Menu.Category { Id = c.Id, Name = c.Name, ParentCategoryId = c.ParentCategoryId });
                            category.Categories.AddRange(subCatItems);

                            // добавляем позиции меню
                            var menuItems = rknetdb.MenuItems
                                .Where(i => i.ParentCategory.Id == category.Id)
                                .Select(i => new RKNet_Model.Menu.Item
                                {
                                    Id = i.Id,
                                    ImageHash = i.ImageHash,
                                    Measure = i.Measure,
                                    MeasureUnit = i.MeasureUnit,
                                    ParentCategory = i.ParentCategory,
                                    rkCode = i.rkCode,
                                    rkDeliveryPrice = i.rkDeliveryPrice,
                                    rkName = i.rkName,
                                    marketName = i.marketName,
                                    Description = i.Description,
                                    Enabled = i.Enabled
                                });

                            category.Items.AddRange(menuItems);
                            result.Data = category;
                        }
                        else
                        {
                            result.Ok = false;
                            result.ErrorMessage = "указанная категория не обнаружена в базе данных";
                        }
                    }


                    
                }
            }
            catch(Exception e)
            {
                result.Ok = false;
                result.ErrorMessage = e.Message;
                result.ExceptionText = e.ToString();
            }            
            return new ObjectResult(result);
        }
    }
}
