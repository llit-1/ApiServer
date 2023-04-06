using Microsoft.EntityFrameworkCore;
using RKNET_ApiServer.OAuth2;
using RKNET_ApiServer.Middleware;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using RKNET_ApiServer.Logger;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Reflection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using RKNET_ApiServer.Models;
using Microsoft.AspNetCore.Http.Features;

Logging.LocalLog("-----------------------------------------------------------");
Logging.LocalLog($"RKNet Api сервер v{ApiServer.version} запущен");
Logging.LocalLog("-----------------------------------------------------------");

// создаем папку RKNet на диске C, если её там нет
string rkentDir = "C:\\RKNetData";
if (!Directory.Exists(rkentDir))
{
    Directory.CreateDirectory(rkentDir);
}

var builder = WebApplication.CreateBuilder(args);
RKNET_ApiServer.Models.ApiServer.Configuration = builder.Configuration;

//builder.Logging.ClearProviders();
//builder.Logging.AddProvider(new OnlineLoggerProvider());

//builder.Services.AddHttpLogging(options => {
//options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
//});


// If using Kestrel:
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});

// If using IIS:
builder.Services.Configure<IISServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});


// проверка тест-режима (0 - не тест)
bool isTest = builder.Configuration.GetSection("TestMode")["test"] != "0";
// подключаем БД sqlite
var sqliteString = builder.Configuration.GetConnectionString("sqlite");
builder.Services.AddDbContext<RKNET_ApiServer.DB.RknetDbContext>(options => options.UseSqlite(sqliteString));

// подключаем БД mssql
string? mssqlString;
if (isTest)
{
    mssqlString = builder.Configuration.GetConnectionString("mssqltest");
}
else
{
    mssqlString = builder.Configuration.GetConnectionString("mssql");
}
 
builder.Services.AddDbContext<RKNET_ApiServer.DB.MSSQLDBContext>(options => 
{
    options.UseSqlServer(mssqlString,
        sqlServerOptionsAction: mssqlOptions =>
        {
            mssqlOptions.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);            
        });
 });
    

// Add services to the container.
builder.Services.AddControllersWithViews()
    // настройки Json
    .AddNewtonsoftJson(options => 
    { 
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore; // поля со значением null в ответных json объектах не передаются
    });

// OAuth авторизация фреймворка IdentityServer4
// Сервер авторизации OAuth
builder.Services.AddIdentityServer(options => 
{ 
    //options.Endpoints.EnableDiscoveryEndpoint = false;    
})
    .AddClientStore<ClientStore>()
    .AddInMemoryApiResources(Resources.GetApiResources())
    .AddInMemoryApiScopes(Scopes.GetApiScopes())
    //.AddTestUsers(Users.Get())
    .AddDeveloperSigningCredential();

builder.Services.AddTransient<IdentityServer4.Hosting.IEndpointRouter, CustomEndpointRouter>(); // меняем конечные точки подключения OAuth 2.0


string? hostUrl;
// Параметры приложения для авторизации с использование сервера OAuth
if (isTest)
{
    hostUrl = builder.Configuration.GetSection("Host")["test"];
}
else
{
    hostUrl = builder.Configuration.GetSection("Host")["default"];
}
if (builder.Environment.IsDevelopment())
{
    hostUrl = "https://localhost:5224";
}


// Swagger
builder.Services.AddSwaggerGen(config =>
{
    // заголовок и версия
    config.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "RKNet ApiServer", Version = RKNET_ApiServer.Models.ApiServer.version });
    
    // разедление на группы методов
    config.TagActionsBy(api =>
    {
        if (api.GroupName != null)
        {
            return new[] { api.GroupName };
        }

        var controllerActionDescriptor = api.ActionDescriptor as ControllerActionDescriptor;
        if (controllerActionDescriptor != null)
        {
            return new[] { controllerActionDescriptor.ControllerName };
        }

        throw new InvalidOperationException("Unable to determine tag for endpoint.");
    });
    config.DocInclusionPredicate((name, api) => true);



    // подключаем авторизацию в свагере
    var tokenHost = hostUrl;
    if (hostUrl == "http://api.rknet-server.shzhleb.ru")
    {
        tokenHost = "https://api.ludilove.ru";
    }
    config.AddSecurityDefinition("OAuth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            ClientCredentials = new OpenApiOAuthFlow
            {                
                TokenUrl = new Uri($"{tokenHost}/swagger/security/oauth/token"),
                Scopes = new Dictionary<string, string>
                    {
                        { "read" , "доступ на чтение" },
                        { "write" , "доступ на запись" }
                    },
            }
        },
        Description = "OAuth 2.0 авторизация в RKNet Api"
    });

    config.AddSecurityDefinition("Basic", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "basic",
        In = ParameterLocation.Header,
        Description = "Basic авторизация в RKNet Api"
    });


    config.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="OAuth2"
                }
            },
            new string[]{}
        }
    });

    // подключение xml файла для работы с комментариями swagger
    var xmlfile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlpath = Path.Combine(AppContext.BaseDirectory, xmlfile);
    config.IncludeXmlComments(xmlpath);

});


builder.Services.AddAuthentication("Bearer")
.AddIdentityServerAuthentication("Bearer", options =>
{
    options.ApiName = "RKNetApi";
    options.Authority = hostUrl;
    options.RequireHttpsMetadata = false;
});


// базовая авторизация
builder.Services.AddAuthentication()
    .AddScheme<AuthenticationSchemeOptions, RKNET_ApiServer.BasicAuth.BasicAuthenticationHandler>("BasicAuthentication", options => { });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BasicAuthentication", new AuthorizationPolicyBuilder("BasicAuthentication").RequireAuthenticatedUser().Build());
});

// Политики доступа к контроллерам и методам
builder.Services.AddAuthorization(options =>
{
    // меню
    options.AddPolicy("menu", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "menu");
    });

    // яндекс еда
    options.AddPolicy("YandexRead", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "read");
    });
    options.AddPolicy("YandexWrite", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "write");
    });

    // кассовые клиенты
    options.AddPolicy("cashClients", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "cashClients");
    });
});

// SignalR
builder.Services.AddSignalR(options =>
{
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);    
});

// Синхронизация меню
builder.Services.AddHostedService<RKNET_ApiServer.HostedServices.MenuSyncService>();

// Выдача потокового видео
builder.Services.AddScoped<RKNET_ApiServer.Services.IStreamVideoService, RKNET_ApiServer.Services.StreamVideoService>();


var app = builder.Build();


// Статический контекст хабов
RKNET_ApiServer.SignalR.CashesHub.Current = app.Services.GetService<Microsoft.AspNetCore.SignalR.IHubContext<RKNET_ApiServer.SignalR.CashesHub>>();
RKNET_ApiServer.SignalR.EventsHub.Current = app.Services.GetService<Microsoft.AspNetCore.SignalR.IHubContext<RKNET_ApiServer.SignalR.EventsHub>>();


// КОНВЕЙЕР обработки запросов и ответов /////////////////////////////////////////////////////////////////////////////////////////////////////////
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

// swagger
app.UseSwagger();
app.UseSwaggerUI(config => 
{
    config.RoutePrefix = "swagger";
    config.SwaggerEndpoint("v1/swagger.json", "RKNet ApiServer");
    config.SwaggerEndpoint("yandexEda.json", "Яндекс ЕДА");
    config.SwaggerEndpoint("deliveryClub.json", "Delivery Club");
});


app.UseStaticFiles();

app.UseRouting();

app.UseLogRequests(); // миддлваре лдогирования запросов и ответов

//app.UseHttpLogging();

app.UseIdentityServer();

app.UseAuthorization();

app.UseAuthentication();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<RKNET_ApiServer.SignalR.CashesHub>("/casheshub");
    endpoints.MapHub<RKNET_ApiServer.SignalR.EventsHub>("/eventshub");
    endpoints.MapControllers(); // подключаем маршрутизацию на контроллеры    
});


app.Run();

