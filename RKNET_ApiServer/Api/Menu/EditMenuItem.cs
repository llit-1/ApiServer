using Microsoft.AspNetCore.Mvc;

namespace RKNET_ApiServer.Api.Menu
{
    public partial class Actions
    {
        [HttpPost]
        public IActionResult EditMenuItem(RKNet_Model.Menu.Item item)
        {
            var result = new RKNet_Model.Result<string>();
            try
            {
                var itm = rknetdb.MenuItems.FirstOrDefault(i => i.Id == item.Id);
                if (itm == null)
                {
                    result.Ok = false;
                    result.ErrorMessage = "позиции с Id = " + item.Id + " нет в базе данных";
                    return new ObjectResult(result);
                }

                if (item.rkCode == 0 | string.IsNullOrEmpty(item.rkName))
                {
                    result.Ok = false;
                    result.ErrorMessage = "Не выбрано блюдо Р-Кипер для позиции";
                    return new ObjectResult(result);
                }

                itm.rkName = item.rkName;
                itm.rkCode = item.rkCode;
                itm.rkDeliveryPrice = item.rkDeliveryPrice;
                itm.marketName = item.marketName;
                itm.Description = item.Description;
                itm.Measure = item.Measure;
                itm.MeasureUnit = rknetdb.MeasureUnits.FirstOrDefault(u => u.Id == item.MeasureUnit.Id);
                itm.Image = item.Image;
                itm.ImageHash = ShaHashString(itm.Image);

                rknetdb.MenuItems.Update(itm);

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
