using Microsoft.AspNetCore.Authorization;

namespace RKNET_ApiServer.BasicAuth
{
    public class BasicAuthorizationAttribute : AuthorizeAttribute
    {
        public BasicAuthorizationAttribute()
        {
            Policy = "BasicAuthentication";
        }
    }
}
