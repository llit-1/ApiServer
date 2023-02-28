using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace RKNET_ApiServer.Api.Yandex
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
        [HttpGet("Yandex/menu/{restaurantId}/composition")]
        public IActionResult MenuComposition(string restaurantId)
        {
            var isLogging = true;
            var requestName = "получение меню";

            // заголовки ответа для Яндекса
            Response.Headers.Add("Cache-Control", "private, max-age=0, no-cache, no-store");
            Response.Headers.Add("Expires", DateTime.UtcNow.AddSeconds(60).ToString("r"));
            Response.Headers.Add("ETag", RandomString(30));
            Response.Headers.Add("Vary", "User-Agent");
            Response.Headers.Add("Pragma", "no-cache");

            // Получаем данные ресторана
            var result = GetTT(restaurantId);
            if (!result.Ok)
            {
                var errors = new List<Api.Yandex.Models.Error>();
                errors.Add(new Api.Yandex.Models.Error
                {
                    code = 100,
                    description = result.ErrorMessage
                });

                Response.StatusCode = 404;
                if (isLogging) if (isLogging) ErrorLog(requestName, restaurantId, result.ErrorMessage);
                return new ObjectResult(errors);
            }

            var tt = result.Data;

            // Получаем client_id
            var clientResult = GetClientId(Request);
            if (!clientResult.Ok)
            {
                var errorMessage = "не распознано имя Api клиента";
                var errors = new List<Api.Yandex.Models.Error>();
                errors.Add(new Api.Yandex.Models.Error
                {
                    code = 100,
                    description = errorMessage
                });

                Response.StatusCode = 500;
                if (isLogging) if (isLogging) ErrorLog(requestName, restaurantId, errorMessage);
                return new ObjectResult(errors);
            }

            // МЕНЮ

            var menu = new RKNET_ApiServer.Api.Yandex.Models.Menu();
            var hostUrl = $"{HttpContext.Request.Scheme}://api.ludilove.ru"; //{HttpContext.Request.Host}";            

            // категории
            var categories = rknetdb.MenuCategorys.ToList();
            foreach(var cat in categories)
            {
                var newCat = new RKNET_ApiServer.Api.Yandex.Models.Menu.Category();
                
                newCat.id = cat.Id.ToString();
                newCat.name = cat.Name;
                newCat.parentId = cat.ParentCategoryId.ToString();

                var img = new RKNET_ApiServer.Api.Yandex.Models.Menu.CategoryImage();
                img.updatedAt = cat.ImageUpdated;
                img.url = hostUrl + "/Yandex/menu/catImage/" + cat.Id + "/image.jpg";

                newCat.images.Add(img);
                menu.categories.Add(newCat);                
            }

            // позиции
            var items = rknetdb.MenuItems.Include(i => i.ParentCategory).Include(i => i.MeasureUnit).ToList();
            foreach(var item in items)
            {
                var newItem = new RKNET_ApiServer.Api.Yandex.Models.Menu.Item();

                newItem.id = item.Id.ToString();
                newItem.categoryId = item.ParentCategory.Id.ToString();
                newItem.name = item.marketName;
                newItem.description = item.Description;
                newItem.price = item.rkDeliveryPrice;
                newItem.measure = item.Measure;
                newItem.measureUnit = item.MeasureUnit.Name;

                var img = new RKNET_ApiServer.Api.Yandex.Models.Menu.ItemImage();                               
                img.hash = item.ImageHash;
                img.url = hostUrl + "/Yandex/menu/itemImage/" + item.Id + "/image.jpg";

                newItem.images.Add(img);
                menu.items.Add(newItem);
            }

            // последнее изменение меню
            var lastChange = rknetdb.LastChanges.FirstOrDefault(l => l.Name == "Меню");
            if(lastChange != null)
            {
                menu.lastChange = lastChange.Date;
            }                       

            return new ObjectResult(menu);
        }

        // ссылка для скачивания изображения категории
        /// <summary>
        /// Изображение категории
        /// </summary>
        /// <remarks>
        /// Ссылка для скачивания изображения категории в виде файла. Анонимный доступ.
        /// </remarks>
        /// <param name="id">id категории меню в БД</param>
        /// <param name="filename">произвольное имя скачиваемого файла (image.jpg для Яндекса)</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("Yandex/menu/catImage/{id}/{filename}")]
        public IActionResult CategoryImage(int id, string filename)
        {
            var cat = rknetdb.MenuCategorys.FirstOrDefault(c => c.Id == id);

            if(cat != null)
            {
                if(cat.Image != null)
                {
                    return File(cat.Image, "image/jpeg", filename);
                }
            }

            var defCatImage = rknetdb.Files.FirstOrDefault(f => f.Name == "MenuCategoryImage.jpg");
            if(defCatImage != null)
            {
                if(defCatImage.Data != null)
                {
                    return File(defCatImage.Data, "image/jpeg", filename);
                }
            }
            return null;
        }

        // ссылка для скачивания изображения позиции
        /// <summary>
        /// Изображение позиции
        /// </summary>
        /// <remarks>
        /// Ссылка для скачивания изображения позиции в виде файла. Анонимный доступ.
        /// </remarks>
        /// <param name="id">id позиции меню в БД</param>
        /// <param name="filename">произвольное имя скачиваемого файла (image.jpg для Яндекса)</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("Yandex/menu/itemImage/{id}/{filename}")]
        public IActionResult ItemImage(int id, string filename)
        {
            var item = rknetdb.MenuItems.FirstOrDefault(i => i.Id == id);

            if (item != null)
            {
                if (item.Image != null)
                {
                    return File(item.Image, "image/jpeg", filename);
                }
            }

            var defItemImage = rknetdb.Files.FirstOrDefault(f => f.Name == "MenuItemImage.jpg");
            if (defItemImage != null)
            {
                if (defItemImage.Data != null)
                {
                    return File(defItemImage.Data, "image/jpeg", filename);
                }
            }
            return null;
        }                   
    }
}
