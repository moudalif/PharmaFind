namespace PharmaFind.Views;

/// <summary>Écran de lancement : bref, puis ouverture directe de l'espace visiteur (aucune inscription requise).</summary>
public partial class LaunchPage : ContentPage
{
    private bool _started;

    public LaunchPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_started) return;
        _started = true;

        await Logo.ScaleToAsync(1.08, 350, Easing.CubicOut);
        await Logo.ScaleToAsync(1.0, 250, Easing.CubicIn);
        await Task.Delay(600);
        await Shell.Current.GoToAsync(Routes.VisitorHome);
    }
}
