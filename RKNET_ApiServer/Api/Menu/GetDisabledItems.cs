using Microsoft.AspNetCore.Mvc;

namespace RKNET_ApiServer.Api.Menu
{    
    public partial class Actions
    {
        [HttpGet]
        public IActionResult GetDisabledItems()
        {
            var result = new RKNet_Model.Result<List<RKNet_Model.Menu.Item>>();
            try
            {
                result.Data = rknetdb.MenuItems.Where(i => !i.Enabled).ToList();
            }
            catch(Exception ex)
            {
                result.Ok = false;
                result.ErrorMessage = ex.Message;
            }            
            return new ObjectResult(result);
        }
    }
}
