using Microsoft.AspNetCore.Mvc;

namespace RKNET_ApiServer.Api.Menu
{
    public partial class Actions
    {
        [HttpPost]
        public IActionResult AddMenuItem(RKNet_Model.Menu.Item item)
        {
            var result = new RKNet_Model.Result<string>();

            try
            {                
                if (item.rkCode == 0 | string.IsNullOrEmpty(item.rkName))
                {
                    result.Ok = false;
                    result.ErrorMessage = "Не выбрано блюдо Р-Кипер для позиции";
                    return new ObjectResult(result);
                }                

                if(string.IsNullOrEmpty(item.marketName))
                {
                    result.Ok = false;
                    result.ErrorMessage = "Не указано маркетинговое имя позиции";
                    return new ObjectResult(result);
                }

                if(item.Measure == 0)
                {
                    result.Ok = false;
                    result.ErrorMessage = "Не указано количество для позиции";
                    return new ObjectResult(result);
                }

                if(item.MeasureUnit.Id == 0)
                {
                    result.Ok = false;
                    result.ErrorMessage = "Не выбрана единица измерения количества";
                    return new ObjectResult(result);
                }


                item.ImageHash = ShaHashString(item.Image);
                item.ParentCategory = rknetdb.MenuCategorys.FirstOrDefault(c => c.Id == item.ParentCategory.Id);
                item.MeasureUnit = rknetdb.MeasureUnits.FirstOrDefault(u => u.Id == item.MeasureUnit.Id);

                var nameExist = rknetdb.MenuItems.FirstOrDefault(i => i.marketName == item.marketName && i.ParentCategory == item.ParentCategory);
                if (nameExist != null)
                {
                    result.Ok = false;
                    result.ErrorMessage = "Позиция с именем \"" + nameExist.marketName + "\" уже существует в данном разделе. Выберите другое имя.";
                    return new ObjectResult(result);
                }


                rknetdb.MenuItems.Add(item);
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
