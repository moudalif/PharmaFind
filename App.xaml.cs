using Microsoft.Extensions.DependencyInjection;

namespace PharmaFind;

public partial class App : Application
{
    private readonly IServiceProvider _services;

    public App(IServiceProvider services)
    {
        InitializeComponent();
        // La charte est conçue pour un fond clair ; le mode sombre est prévu pour une version ultérieure.
        UserAppTheme = AppTheme.Light;
        _services = services;
    }

    // Le Shell est créé ici, après le chargement des ressources (couleurs, styles) par InitializeComponent.
    protected override Window CreateWindow(IActivationState? activationState)
        => new(_services.GetRequiredService<AppShell>()) { Title = "PharmaFind" };
}
