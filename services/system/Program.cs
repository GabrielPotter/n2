using Common;
using SystemService;

var builder = WebApplication.CreateBuilder(args);

builder.AddCommonLogging("system");
builder.WebHost.UseUrls(builder.Configuration["Urls"] ?? "http://0.0.0.0:5204");

builder.Services.Configure<SystemSettings>(builder.Configuration.GetSection(SystemSettings.SectionName));
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection(DatabaseSettings.SectionName));
builder.Services.AddCommonServices(builder.Configuration.GetValue<string>("Database:ConnectionString"));
builder.Services.AddSystemServices();

var app = builder.Build();

app.UseCommonHttp();
app.UseCommonRequestLogging();
app.UseCommonExceptionHandling();
app.MapSystemApi();

await app.RunAsync();
