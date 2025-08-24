using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Zenvestify.Shared.Services;
using Zenvestify.Web.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add device-specific services used by the Zenvestify.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

// Register HttpClient for Blazor WASM
builder.Services.AddScoped(sp => new HttpClient
{
	BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

builder.Services.AddScoped<AuthService>();

await builder.Build().RunAsync();
