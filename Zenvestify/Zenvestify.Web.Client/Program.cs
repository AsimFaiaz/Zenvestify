using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Zenvestify.Shared.Services;
using Zenvestify.Web.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add device-specific services used by the Zenvestify.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

await builder.Build().RunAsync();
