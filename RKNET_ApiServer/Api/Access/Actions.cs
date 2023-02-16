using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;


namespace RKNET_ApiServer.Api.Access
{
    [Authorize]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Права доступа")]
    [Route("Access/[action]")]
    public partial class Actions : ControllerBase
    {
        private DB.RknetDbContext rknetdb;

        public Actions(DB.RknetDbContext rknetdbContext)
        {
            rknetdb = rknetdbContext;
        }
    }
}
