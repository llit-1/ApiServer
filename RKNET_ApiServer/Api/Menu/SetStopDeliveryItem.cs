using Microsoft.AspNetCore.Mvc;

namespace RKNET_ApiServer.Api.Menu
{
    public partial class Actions
    {
        [HttpGet]
        public IActionResult SetStopDeliveryItem(int ttId, int itemId, string userLogin)
        {
            var result = new RKNet_Model.Result<string>();
            
            try
            {
                var tt = rknetdb.TTs.FirstOrDefault(tt => tt.Id == ttId);
                if (tt == null)
                {
                    result.Ok = false;
                    result.ErrorMessage = $"Точка с Id = {ttId} не найдена в базе данных.";
                    return new ObjectResult(result);
                }

                var item = rknetdb.MenuItems.FirstOrDefault(item => item.Id == itemId);
                if (item == null)
                {
                    result.Ok = false;
                    result.ErrorMessage = $"Позиция с Id = {itemId} не найдена в базе данных.";
                    return new ObjectResult(result);
                }

                var user = rknetdb.Users.FirstOrDefault(u => u.Login == userLogin);
                if (user == null)
                {
                    result.Ok = false;
                    result.ErrorMessage = $"Пользователь с логином \"{userLogin}\" не найден в базе данных.";
                    return new ObjectResult(result);
                }

                var stopContains = mssql.DeliveryItemStops
                    .Where(s => s.Created.Date == DateTime.Now.Date)
                    .Where(s => s.Cancelled == null)
                    .Where(s => s.ItemId == item.Id)
                    .Where(s => s.TTCode == tt.Code);                    

                if (stopContains.Count() > 0)
                {
                    result.Ok = false;
                    result.ErrorMessage = $"Позиция \"{item.marketName}\" уже была ранее поставлена на СТОП по ТТ {tt.Name}.";
                    return new ObjectResult(result);
                }

                var deliveryStop = new RKNet_Model.MSSQL.DeliveryItemStop
                {
                    UserName = user.Name,
                    TTCode = tt.Code,
                    TTName = tt.Name,
                    ItemId = item.Id,
                    ItemRkCode = item.rkCode,
                    ItemMarketName = item.marketName
                };

                mssql.DeliveryItemStops.Add(deliveryStop);
                mssql.SaveChanges();                
            }
            catch (Exception ex)
            {
                result.Ok = false;
                result.ErrorMessage = ex.Message;
                result.ExceptionText = ex.ToString();

                Models.Logging.LocalLog($"ошибка Api.Menu.SetStopDeliveryItem: {ex.Message}");
            }
            return new ObjectResult(result);
        }
    }
}
