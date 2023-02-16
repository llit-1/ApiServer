using Microsoft.AspNetCore.Mvc;

namespace RKNET_ApiServer.Api.Menu
{
    public partial class Actions
    {
        [HttpGet]
        public IActionResult GetCashStopsByTT(string ttCode)
        {
            var result = new RKNet_Model.Result<List<RKNet_Model.MSSQL.SkuStop>>();
            result.Data = new List<RKNet_Model.MSSQL.SkuStop>();

            try
            {
                if (!string.IsNullOrEmpty(ttCode))
                {
                    var activeStops = mssql.SkuStops.Where(s => s.Finished == "0");
                    foreach(var stop in activeStops)
                    {
                        var cashStates = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RKNet_Model.MSSQL.SkuStopState>>(stop.CashStates);
                        var ttCashState = cashStates.FirstOrDefault(s => s.TTCode == int.Parse(ttCode));
                        if(ttCashState != null)
                        {
                            if(ttCashState.blocked)
                            {
                                result.Data.Add(stop);
                            }
                        }
                    }
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
