using Common;
using CoreQuery;

var builder = WebApplication.CreateBuilder(args);

builder.AddCommonLogging("core-query");
builder.WebHost.UseUrls(builder.Configuration["Urls"] ?? "http://0.0.0.0:5203");

builder.Services.Configure<CoreQuerySettings>(builder.Configuration.GetSection(CoreQuerySettings.SectionName));
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection(DatabaseSettings.SectionName));
builder.Services.AddCommonServices(builder.Configuration.GetValue<string>("Database:ConnectionString"));
builder.Services.AddCoreQueryServices();

var app = builder.Build();

app.UseCommonHttp();
app.UseCommonRequestLogging();
app.UseCommonExceptionHandling();
app.MapCoreQueryApi();

await app.RunAsync();
