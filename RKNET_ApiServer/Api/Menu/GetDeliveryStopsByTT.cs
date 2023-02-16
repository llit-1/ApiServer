using Microsoft.AspNetCore.Mvc;

namespace RKNET_ApiServer.Api.Menu
{
    public partial class Actions
    {
        [HttpGet]
        public IActionResult GetDeliveryStopsByTT(string ttCode)
        {
            var result = new RKNet_Model.Result<List<RKNet_Model.MSSQL.DeliveryItemStop>>();
            result.Data = new List<RKNet_Model.MSSQL.DeliveryItemStop>();

            try
            {
                if(!string.IsNullOrEmpty(ttCode))
                {                   
                    var stops = mssql.DeliveryItemStops
                    .Where(s => s.Created.Date == DateTime.Now.Date)
                    .Where(s => s.Cancelled == null)
                    .Where(s => s.TTCode == int.Parse(ttCode))
                    .ToList();

                    if (stops != null) result.Data = stops;
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
