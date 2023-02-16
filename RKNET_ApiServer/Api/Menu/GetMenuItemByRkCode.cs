using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace RKNET_ApiServer.Api.Menu
{
    public partial class Actions
    {
        [HttpGet]
        public IActionResult GetMenuItemByRkCode(int rkCode, bool withImage)
        {
            var result = new RKNet_Model.Result<RKNet_Model.Menu.Item>();
            try
            {
                var item = new RKNet_Model.Menu.Item();
                // с картинкой позиции, но без картинки родительского каталога
                if (withImage)
                {
                    item = rknetdb.MenuItems
                    .Include(i => i.ParentCategory)
                    .Include(i => i.MeasureUnit)
                    .Select(i => new RKNet_Model.Menu.Item
                    {
                        Id = i.Id,
                        Description = i.Description,
                        Enabled = i.Enabled,
                        marketName = i.marketName,
                        Image = i.Image,
                        ImageHash = i.ImageHash,
                        MeasureUnit = i.MeasureUnit,
                        Measure = i.Measure,
                        ParentCategory = new RKNet_Model.Menu.Category { Id = i.ParentCategory.Id, Name = i.ParentCategory.Name, ParentCategoryId = i.ParentCategory.ParentCategoryId, ImageUpdated = i.ParentCategory.ImageUpdated },
                        rkCode = i.rkCode,
                        rkDeliveryPrice = i.rkDeliveryPrice,
                        rkName = i.rkName
                    })
                    .FirstOrDefault(i => i.rkCode == rkCode);
                }
                // без картинок
                else
                {
                    item = rknetdb.MenuItems
                    .Include(i => i.ParentCategory)
                    .Include(i => i.MeasureUnit)
                    .Select(i => new RKNet_Model.Menu.Item
                    {
                        Id = i.Id,
                        Description = i.Description,
                        Enabled = i.Enabled,
                        marketName = i.marketName,
                        ImageHash = i.ImageHash,
                        MeasureUnit = i.MeasureUnit,
                        Measure = i.Measure,
                        ParentCategory = new RKNet_Model.Menu.Category { Id = i.ParentCategory.Id, Name = i.ParentCategory.Name, ParentCategoryId = i.ParentCategory.ParentCategoryId, ImageUpdated = i.ParentCategory.ImageUpdated },
                        rkCode = i.rkCode,
                        rkDeliveryPrice = i.rkDeliveryPrice,
                        rkName = i.rkName
                    })
                    .FirstOrDefault(i => i.rkCode == rkCode);
                }

                if (item == null)
                {
                    result.Ok = false;
                    result.ErrorMessage = $"Позиции с rkCode={rkCode} не обнаружено в базе данных";
                }
                else
                {
                    result.Data = item;
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
