using GameServerList.Common.Services;
using GameServerList.Components;
using GameServerList.Helpers;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var config = builder.Configuration;

services.AddHttpClient();
services.AddHttpContextAccessor();
services.AddMemoryCache();

services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
services.AddSingleton<SteamServerBrowserApiService>();
services.AddSingleton<SteamPlayerDetailApiService>();

services.AddRazorComponents()
    .AddInteractiveServerComponents();

services.AddMudServices();

GameList.LoadGameList();

#if !DEBUG
builder.WebHost.UseUrls("http://0.0.0.0:8080");
#endif

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    //app.UseHsts();
}

//app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();