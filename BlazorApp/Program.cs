using BlazorApp;
using BlazorApp.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using System.Net.Http.Headers;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<FirebaseService>();
builder.Services.AddScoped<FirebaseAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<FirebaseAuthStateProvider>());
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<VisitorService>();
builder.Services.AddScoped<AttendanceService>();
builder.Services.AddMudServices();
builder.Services.AddSingleton<CacheService>();

builder.Services.AddScoped(sp =>
{
    var client = new HttpClient
    {
        BaseAddress = new Uri("https://securityapi-osz0.onrender.com/")
        //BaseAddress = new Uri("http://localhost:5142/")
    };
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    return client;
});

await builder.Build().RunAsync();
