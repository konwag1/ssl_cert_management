using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SSLCertificatesBlazor;
using SSLCertificatesBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

// Zaktualizuj bazowy adres URL na adres Twojego API
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7094") });
builder.Services.AddScoped<SSLCertificateService>();

await builder.Build().RunAsync();


