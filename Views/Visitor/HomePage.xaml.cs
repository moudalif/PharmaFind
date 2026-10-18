using PharmaFind.ViewModels;

namespace PharmaFind.Views;

public partial class HomePage : BasePage
{
    public HomePage(HomeViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
    }
}
