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
Logging.LocalLog($"RKNet Api ������ v{ApiServer.version} �������");
Logging.LocalLog("-----------------------------------------------------------");

// ������� ����� RKNet �� ����� C, ���� � ��� ���
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


// �������� ����-������ (0 - �� ����)
bool isTest = builder.Configuration.GetSection("TestMode")["test"] != "0";
// ���������� �� sqlite
var sqliteString = builder.Configuration.GetConnectionString("sqlite");
builder.Services.AddDbContext<RKNET_ApiServer.DB.RknetDbContext>(options => options.UseSqlite(sqliteString));

// ���������� �� mssql
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
    // ��������� Json
    .AddNewtonsoftJson(options => 
    { 
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore; // ���� �� ��������� null � �������� json �������� �� ����������
    });

// OAuth ����������� ���������� IdentityServer4
// ������ ����������� OAuth
builder.Services.AddIdentityServer(options => 
{ 
    //options.Endpoints.EnableDiscoveryEndpoint = false;    
})
    .AddClientStore<ClientStore>()
    .AddInMemoryApiResources(Resources.GetApiResources())
    .AddInMemoryApiScopes(Scopes.GetApiScopes())
    //.AddTestUsers(Users.Get())
    .AddDeveloperSigningCredential();

builder.Services.AddTransient<IdentityServer4.Hosting.IEndpointRouter, CustomEndpointRouter>(); // ������ �������� ����� ����������� OAuth 2.0


string? hostUrl;
// ��������� ���������� ��� ����������� � ������������� ������� OAuth
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
    // ��������� � ������
    config.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "RKNet ApiServer", Version = RKNET_ApiServer.Models.ApiServer.version });
    
    // ���������� �� ������ �������
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



    // ���������� ����������� � �������
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
                        { "read" , "������ �� ������" },
                        { "write" , "������ �� ������" }
                    },
            }
        },
        Description = "OAuth 2.0 ����������� � RKNet Api"
    });

    config.AddSecurityDefinition("Basic", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "basic",
        In = ParameterLocation.Header,
        Description = "Basic ����������� � RKNet Api"
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

    // ����������� xml ����� ��� ������ � ������������� swagger
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


// ������� �����������
builder.Services.AddAuthentication()
    .AddScheme<AuthenticationSchemeOptions, RKNET_ApiServer.BasicAuth.BasicAuthenticationHandler>("BasicAuthentication", options => { });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BasicAuthentication", new AuthorizationPolicyBuilder("BasicAuthentication").RequireAuthenticatedUser().Build());
});

// �������� ������� � ������������ � �������
builder.Services.AddAuthorization(options =>
{
    // ����
    options.AddPolicy("menu", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "menu");
    });

    // ������ ���
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

    // �������� �������
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

// ������������� ����
builder.Services.AddHostedService<RKNET_ApiServer.HostedServices.MenuSyncService>();

// ������ ���������� �����
builder.Services.AddScoped<RKNET_ApiServer.Services.IStreamVideoService, RKNET_ApiServer.Services.StreamVideoService>();


var app = builder.Build();


// ����������� �������� �����
RKNET_ApiServer.SignalR.CashesHub.Current = app.Services.GetService<Microsoft.AspNetCore.SignalR.IHubContext<RKNET_ApiServer.SignalR.CashesHub>>();
RKNET_ApiServer.SignalR.EventsHub.Current = app.Services.GetService<Microsoft.AspNetCore.SignalR.IHubContext<RKNET_ApiServer.SignalR.EventsHub>>();


// �������� ��������� �������� � ������� /////////////////////////////////////////////////////////////////////////////////////////////////////////
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
    config.SwaggerEndpoint("yandexEda.json", "������ ���");
    config.SwaggerEndpoint("deliveryClub.json", "Delivery Club");
});


app.UseStaticFiles();

app.UseRouting();

app.UseLogRequests(); // ��������� ������������ �������� � �������

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
    endpoints.MapControllers(); // ���������� ������������� �� �����������    
});


app.Run();

