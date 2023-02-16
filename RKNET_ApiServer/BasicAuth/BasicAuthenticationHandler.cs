using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Security.Claims;


namespace RKNET_ApiServer.BasicAuth
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly DB.RknetDbContext rknetdb;
        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options, 
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            DB.RknetDbContext rknetdbContext
            ) : base(options, logger, encoder, clock)
        {
            rknetdb = rknetdbContext;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            Response.Headers.Add("WWW-Authenticate", "Basic");

            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return Task.FromResult(AuthenticateResult.Fail("Authorization header missing."));
            }

            // Get authorization key
            var authorizationHeader = Request.Headers["Authorization"].ToString();
            var authHeaderRegex = new Regex(@"Basic (.*)");

            if (!authHeaderRegex.IsMatch(authorizationHeader))
            {
                return Task.FromResult(AuthenticateResult.Fail("Authorization code not formatted properly."));
            }

            var authBase64 = Encoding.UTF8.GetString(Convert.FromBase64String(authHeaderRegex.Replace(authorizationHeader, "$1")));
            var authSplit = authBase64.Split(Convert.ToChar(":"), 2);
            var authUsername = authSplit[0];
            var authPassword = authSplit.Length > 1 ? authSplit[1] : throw new Exception("Unable to get password");

            // проверяем акаунты в таблице организаций
            //var organizations = rknetdb.Organizations.ToList();

            //foreach (var organization in organizations)
            //{
            //    // Delivery Club
            //    if (organization.DeliveryClubLogin != null)
            //    {
            //        if (authUsername == organization.DeliveryClubLogin && authPassword == organization.DeliveryClubPassword)
            //        {
            //            var authenticatedUser = new AuthenticatedUser("BasicAuthentication", true, organization.DeliveryClubLogin);
            //            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(authenticatedUser));
            //            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name)));
            //        }
            //    }
            //}

            // проверяем логин и пароль Delivery Club в настройках Апи сервера
            var apiSettings = rknetdb.ApiServerSettings.FirstOrDefault();
            if(apiSettings != null)
            {
                if(!string.IsNullOrEmpty(apiSettings.DeliveryClubLogin) && !string.IsNullOrEmpty(apiSettings.DeliveryClubPassword))
                {
                    if (authUsername == apiSettings.DeliveryClubLogin && authPassword == apiSettings.DeliveryClubPassword)
                    {
                        var authenticatedUser = new AuthenticatedUser("BasicAuthentication", true, apiSettings.DeliveryClubLogin);
                        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(authenticatedUser));
                        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name)));
                    }
                }
            }

            // авторизация для доступа из Power Bi
            if (authUsername == "powerbi" && authPassword == "'ythubz-'ythubz=59")
            {
                var authenticatedUser = new AuthenticatedUser("BasicAuthentication", true, "powerbi");
                var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(authenticatedUser));
                return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name)));
            }

            return Task.FromResult(AuthenticateResult.Fail("Неверные данные авторизации"));
        }
    }
}
