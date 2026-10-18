using Microsoft.Extensions.Logging;

namespace PharmaFind;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        builder.Services
            .AddPharmaFindServices()
            .AddPharmaFindPages();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}
