using PharmaFind.ViewModels;

namespace PharmaFind.Views;

public partial class SearchPage : BasePage
{
    public SearchPage(SearchViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Clavier ouvert directement : la recherche est l'action principale de cet écran.
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(250), () => SearchEntry.Focus());
    }
}
