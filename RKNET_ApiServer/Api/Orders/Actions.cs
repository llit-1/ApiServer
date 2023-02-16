using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;


namespace RKNET_ApiServer.Api.Orders
{
    [Authorize]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Заказы")]
    [Route("Orders/[action]")]
    public partial class Actions
    {
        private DB.RknetDbContext rknetdb;
        private DB.MSSQLDBContext mssqldb;

        public Actions(DB.RknetDbContext rknetdbContext, DB.MSSQLDBContext mSSQLDBContext)
        {
            rknetdb = rknetdbContext;
            mssqldb = mSSQLDBContext;
        }
    }
}
