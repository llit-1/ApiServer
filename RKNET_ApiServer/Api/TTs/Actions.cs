using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;


namespace RKNET_ApiServer.Api.TTs
{
    [Authorize]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Торговые точки")]
    [Route("TTs/[action]")]
    public partial class Actions : ControllerBase
    {
        private DB.RknetDbContext rknetdb;

        public Actions(DB.RknetDbContext rknetdbContext)
        {
            rknetdb = rknetdbContext;
        }
    }
}
