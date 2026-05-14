using Catalog;
using Common;

var builder = WebApplication.CreateBuilder(args);

builder.AddCommonLogging("catalog");
builder.WebHost.UseUrls(builder.Configuration["Urls"] ?? "http://0.0.0.0:5201");

builder.Services.Configure<CatalogSettings>(builder.Configuration.GetSection(CatalogSettings.SectionName));
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection(DatabaseSettings.SectionName));
builder.Services.AddCommonServices(builder.Configuration.GetValue<string>("Database:ConnectionString"));
builder.Services.AddKeycloakAuthentication(builder.Configuration);
builder.Services.AddCatalogServices();

var app = builder.Build();

app.UseCommonHttp();
app.UseCommonRequestLogging();
app.UseCommonExceptionHandling();
app.UseKeycloakAuthentication();
app.MapCatalogApi();

await app.RunAsync();
