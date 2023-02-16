using IdentityServer4.Configuration;
using IdentityServer4.Hosting;

namespace RKNET_ApiServer.OAuth2
{
    internal class CustomEndpointRouter : IEndpointRouter
    {
        // Дополнительные пути получения токена по OAuth 2.0
        Dictionary<string, string> TOKEN_ENDPOINTS = new Dictionary<string, string>()
        {
            {"Яндекс Еда",  "/yandex/security/oauth/token" },
            { "Для документации", "/swagger/security/oauth/token"}
        };

        private readonly IEnumerable<IdentityServer4.Hosting.Endpoint> _endpoints;
        private readonly IdentityServerOptions _options;
        private readonly ILogger _logger;

        public CustomEndpointRouter(IEnumerable<IdentityServer4.Hosting.Endpoint> endpoints, IdentityServerOptions options, ILogger<CustomEndpointRouter> logger)
        {
            _endpoints = endpoints;
            _options = options;
            _logger = logger;
        }

        public IEndpointHandler Find(HttpContext context)
        {            
            if (context == null) throw new ArgumentNullException(nameof(context));

            // Проверяем строку запроса к токену на соотвествие дополнительным путям получения токена и переадресуем запрос на путь по умолчанию IdentityServer4
            foreach(var customEndpoint in TOKEN_ENDPOINTS)
            {
                if (context.Request.Path.Equals(customEndpoint.Value, StringComparison.OrdinalIgnoreCase))
                {
                    var tokenEndPoint = GetEndPoint(EndpointNames.Token);
                    return GetEndpointHandler(tokenEndPoint, context);
                }                
                else
                {
                    foreach (var endpoint in _endpoints)
                    {
                        var path = endpoint.Path;
                        if (context.Request.Path.Equals(path, StringComparison.OrdinalIgnoreCase))
                        {                            
                            var endpointName = endpoint.Name;
                            _logger.LogDebug("Request path {path} matched to endpoint type {endpoint}", context.Request.Path, endpointName);

                            return GetEndpointHandler(endpoint, context);
                        }
                    }
                }
            }
            
            _logger.LogTrace("No endpoint entry found for request path: {path}", context.Request.Path);
            return null;
        }

        private IdentityServer4.Hosting.Endpoint GetEndPoint(string endPointName)
        {
            IdentityServer4.Hosting.Endpoint endpoint = null;
            foreach (var ep in _endpoints)
            {
                if (ep.Name == endPointName)
                {
                    endpoint = ep;
                    break;
                }
            }
            return endpoint;
        }

        private IEndpointHandler GetEndpointHandler(IdentityServer4.Hosting.Endpoint endpoint, Microsoft.AspNetCore.Http.HttpContext context)
        {
            if (_options.Endpoints.IsEndpointEnabled(endpoint))
            {
                var handler = context.RequestServices.GetService(endpoint.Handler) as IEndpointHandler;
                if (handler != null)
                {
                    _logger.LogDebug("Endpoint enabled: {endpoint}, successfully created handler: {endpointHandler}", endpoint.Name, endpoint.Handler.FullName);
                    return handler;
                }
                else
                {
                    _logger.LogDebug("Endpoint enabled: {endpoint}, failed to create handler: {endpointHandler}", endpoint.Name, endpoint.Handler.FullName);
                }
            }
            else
            {
                _logger.LogWarning("Endpoint disabled: {endpoint}", endpoint.Name);
            }

            return null;
        }
    }

    internal static class EndpointOptionsExtensions
    {
        public static bool IsEndpointEnabled(this EndpointsOptions options, IdentityServer4.Hosting.Endpoint endpoint)
        {
            switch (endpoint?.Name)
            {
                case EndpointNames.Authorize:
                    return options.EnableAuthorizeEndpoint;
                case EndpointNames.CheckSession:
                    return options.EnableCheckSessionEndpoint;
                case EndpointNames.Discovery:
                    return options.EnableDiscoveryEndpoint;
                case EndpointNames.EndSession:
                    return options.EnableEndSessionEndpoint;
                case EndpointNames.Introspection:
                    return options.EnableIntrospectionEndpoint;
                case EndpointNames.Revocation:
                    return options.EnableTokenRevocationEndpoint;
                case EndpointNames.Token:
                    return options.EnableTokenEndpoint;
                case EndpointNames.UserInfo:
                    return options.EnableUserInfoEndpoint;
                default:
                    // fall thru to true to allow custom endpoints
                    return true;
            }
        }
    }

    public static class EndpointNames
    {
        public const string Authorize = "Authorize";
        public const string Token = "Token";
        public const string DeviceAuthorization = "DeviceAuthorization";
        public const string Discovery = "Discovery";
        public const string Introspection = "Introspection";
        public const string Revocation = "Revocation";
        public const string EndSession = "Endsession";
        public const string CheckSession = "Checksession";
        public const string UserInfo = "Userinfo";
    }
}
