using Audit.Core;
using Audit.Net.RavenDB.Examples;
using Azure.Identity;
using Audit.NET.RavenDB.ConfigurationApi;

var builder = WebApplication.CreateBuilder(args);

var keyVaultEndpoint = new Uri(Environment.GetEnvironmentVariable("VaultUri")!);
builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());
builder.Services.AddRavenDb(builder.Configuration.GetSection("Raven"));

builder.Services.AddOptions();

Audit.Core.Configuration.Setup()
    .UseRavenDB(builder.Configuration.GetSection("Raven:Urls").Get<string[]>(), ServiceRegistrationExtensions.GetRavenDbCertificate(), builder.Configuration["Raven:DatabaseName"]);

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

Audit.Core.Configuration.AddCustomAction(ActionType.OnEventSaving, scope =>
{
    var httpContextAccessor = app.Services.GetService<IHttpContextAccessor>();
    if (httpContextAccessor?.HttpContext?.User?.Identity?.Name != null)
        scope.SetCustomField("Username", httpContextAccessor.HttpContext?.User?.Identity?.Name);
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
