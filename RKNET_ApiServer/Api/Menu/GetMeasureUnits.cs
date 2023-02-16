using Microsoft.AspNetCore.Mvc;

namespace RKNET_ApiServer.Api.Menu
{
    public partial class Actions
    {
        [HttpGet]
        public IActionResult GetMeasureUnits()
        {
            var measureUnitsResult = new RKNet_Model.Result<List<RKNet_Model.Menu.MeasureUnit>>();
            try
            {
                measureUnitsResult.Data = rknetdb.MeasureUnits.ToList();                
            }
            catch (Exception ex)
            {
                measureUnitsResult.Ok = false;
                measureUnitsResult.ErrorMessage = ex.Message;
                measureUnitsResult.ExceptionText = ex.ToString();
            }
            return new ObjectResult(measureUnitsResult);
        }
    }
}
