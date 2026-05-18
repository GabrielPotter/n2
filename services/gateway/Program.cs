using Common;
using Gateway;

var builder = WebApplication.CreateBuilder(args);

builder.AddCommonLogging("gateway");
builder.WebHost.UseUrls(builder.Configuration["Urls"] ?? "http://0.0.0.0:5100");

builder.Services.Configure<GatewaySettings>(builder.Configuration.GetSection(GatewaySettings.SectionName));
builder.Services.Configure<GatewayClientsSettings>(builder.Configuration.GetSection(GatewayClientsSettings.SectionName));
builder.Services.AddCommonServices();
builder.Services.AddKeycloakAuthentication(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:8080",
                "http://127.0.0.1:8080")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddGatewayClients();
builder.Services.AddGatewayServices();

var app = builder.Build();

app.UseCommonHttp();
app.UseCommonRequestLogging();
app.UseCommonExceptionHandling();
app.UseKeycloakAuthentication();
app.UseCors();
app.MapSystemApi();
app.MapUserApi();

await app.RunAsync();
