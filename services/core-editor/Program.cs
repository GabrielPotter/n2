using Common;
using CoreEditor;

var builder = WebApplication.CreateBuilder(args);

builder.AddCommonLogging("core-editor");
builder.WebHost.UseUrls(builder.Configuration["Urls"] ?? "http://0.0.0.0:5202");

builder.Services.Configure<CoreEditorSettings>(builder.Configuration.GetSection(CoreEditorSettings.SectionName));
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection(DatabaseSettings.SectionName));
builder.Services.AddCommonServices(builder.Configuration.GetValue<string>("Database:ConnectionString"));
builder.Services.AddKeycloakAuthentication(builder.Configuration);
builder.Services.AddCoreEditorServices();

var app = builder.Build();

app.UseCommonHttp();
app.UseCommonRequestLogging();
app.UseCommonExceptionHandling();
app.UseKeycloakAuthentication();
app.MapCoreEditorApi();

await app.RunAsync();
