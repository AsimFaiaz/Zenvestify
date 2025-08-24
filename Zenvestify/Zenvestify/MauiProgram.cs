using Microsoft.Extensions.Logging;
using Zenvestify.Services;
using Zenvestify.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Zenvestify
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
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
