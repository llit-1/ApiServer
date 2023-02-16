using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace RKNET_ApiServer.Api.DeliveryClub
{
    public partial class Actions
    {
        // Меню
        /// <summary>
        /// Меню
        /// </summary>
        /// <remarks>
        /// Выдача актуального на текущий момент меню ресторана
        /// </remarks>
        /// <param name="restaurantId">код тт</param>
        /// <returns></returns>
        [HttpGet("deliveryclub/menus/{restaurantId}")]
        public IActionResult Menus(string restaurantId)
        {
            var isLogging = true;
            var requestName = "получение меню";

            // Получаем данные ресторана
            var result = GetTT(restaurantId);
            if (!result.Ok)
            {
                Response.StatusCode = 500;
                if (isLogging) ErrorLog(requestName, restaurantId, result.ErrorMessage);
                return new ObjectResult(result.ErrorMessage);
            }

            var tt = result.Data;

            // меню
            var DeliveryClubMenu = new RKNET_ApiServer.Api.DeliveryClub.Models.Menu();
            var hostUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";

            // последнее изменение меню
            var lastChange = rknetdb.LastChanges.FirstOrDefault(l => l.Name == "Меню");
            if (lastChange != null)
            {
                var date = DateTime.Parse(lastChange.Date);

                DeliveryClubMenu.lastUpdatedAt = date.ToString("yyy-MM-ddTHH:mm:ss+0300");
            }

            // категории
            var categories = rknetdb.MenuCategorys.ToList();
            foreach (var cat in categories)
            {
                var newCat = new RKNET_ApiServer.Api.DeliveryClub.Models.Menu.MenuItems.Category();

                newCat.id = cat.Id.ToString();
                newCat.name = cat.Name;
                newCat.parentId = cat.ParentCategoryId.ToString();
                
                DeliveryClubMenu.menuItems.categories.Add(newCat);
            }

            // позиции
            var items = rknetdb.MenuItems.Include(i => i.ParentCategory).Include(i => i.MeasureUnit).ToList();
            foreach (var item in items)
            {
                var newItem = new RKNET_ApiServer.Api.DeliveryClub.Models.Menu.MenuItems.Product();

                newItem.id = item.Id.ToString();
                newItem.categoryId = item.ParentCategory.Id.ToString();
                newItem.name = item.marketName;                
                newItem.price = item.rkDeliveryPrice;
                newItem.description = item.Description;
                newItem.imageUrl = hostUrl + "/Yandex/menu/itemImage/" + item.Id + "/image.jpg";

                switch (item.MeasureUnit.Id)
                {
                    // граммы
                    case 1:
                        newItem.weight = item.Measure.ToString();
                        break;
                    // миллилитры
                    case 2:
                        newItem.volume = item.Measure.ToString();
                        break;
                    default:
                        break;
                }
                DeliveryClubMenu.menuItems.products.Add(newItem);
            }
                       
            return new ObjectResult(DeliveryClubMenu);
        }
    }
}
