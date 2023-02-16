using Microsoft.AspNetCore.Mvc;

namespace RKNET_ApiServer.Api.Menu
{
    public partial class Actions
    {
        [HttpGet]
        public IActionResult RemoveStopDeliveryItem(int ttId, int itemId, string userLogin)
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

                var user = rknetdb.Users.FirstOrDefault(u => u.Login == userLogin);
                if (user == null)
                {
                    result.Ok = false;
                    result.ErrorMessage = $"Пользователь с логином \"{userLogin}\" не найден в базе данных.";
                    return new ObjectResult(result);
                }

                var stopsToRemove = mssql.DeliveryItemStops
                    .Where(s => s.Created.Date == DateTime.Now.Date)
                    .Where(s => s.Cancelled == null)
                    .Where(s => s.ItemId == itemId)
                    .Where(s => s.TTCode == tt.Code);

                if (stopsToRemove.Count() == 0)
                {
                    result.Ok = false;
                    result.ErrorMessage = $"Данная позиция отсутсвует в списке стопов.";
                    return new ObjectResult(result);
                }

                foreach(var stop in stopsToRemove)
                {
                    stop.Cancelled = DateTime.Now;
                }

                mssql.DeliveryItemStops.UpdateRange(stopsToRemove);
                mssql.SaveChanges();
            }
            catch (Exception ex)
            {
                result.Ok = false;
                result.ErrorMessage = ex.Message;
                result.ExceptionText = ex.ToString();

                Models.Logging.LocalLog($"ошибка Api.Menu.RemoveStopDeliveryItem: {ex.Message}");
            }
            return new ObjectResult(result);
        }
    }
}
