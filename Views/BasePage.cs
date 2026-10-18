using PharmaFind.ViewModels;

namespace PharmaFind.Views;

/// <summary>Page de base : relie le ViewModel et déclenche son chargement à l'apparition.</summary>
public class BasePage : ContentPage
{
    protected BasePage(BaseViewModel viewModel)
    {
        BindingContext = viewModel;
    }

    protected BaseViewModel ViewModel => (BaseViewModel)BindingContext;

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ViewModel.OnAppearingAsync();
    }
}
