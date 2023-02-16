using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace RKNET_ApiServer.Api.R_Keeper
{
    [Authorize]    
    [ApiController]
    [ApiExplorerSettings(GroupName = "R_Keeper")]
    [Route("R_Keeper/[action]")]
    public partial class Actions : ControllerBase
    {
        private static DB.RknetDbContext rknetdb;

        public Actions(DB.RknetDbContext rknetdbContext)
        {
            rknetdb = rknetdbContext;
        }
    }
}
