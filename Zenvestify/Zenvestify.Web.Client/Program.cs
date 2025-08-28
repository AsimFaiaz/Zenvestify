using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Zenvestify.Shared.Services;
using Zenvestify.Web.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddSingleton<IFormFactor, FormFactor>();

builder.Services.AddScoped(sp => new HttpClient
{
	BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

builder.Services.AddScoped<AuthService>();

var host = builder.Build();

var auth = host.Services.GetRequiredService<AuthService>();
await auth.LoadTokenAsync();

Console.WriteLine("[CLIENT.Program.cs] Token after LoadTokenAsync = " + (auth.Token ?? "NULL"));

await host.RunAsync();
//await builder.Build().RunAsync();
