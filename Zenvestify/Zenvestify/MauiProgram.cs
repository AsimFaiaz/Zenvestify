using CommunityToolkit.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zenvestify.Services;
using Zenvestify.Shared.Services;

namespace Zenvestify
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                })
                .ConfigureEssentials(essentials =>
                {
                    essentials.UseVersionTracking();
                });

            // Add device-specific services used by the Zenvestify.Shared project
            builder.Services.AddSingleton<IFormFactor, FormFactor>();

			builder.Services.AddHttpClient();

			builder.Services.AddScoped<AuthService>();

			builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
