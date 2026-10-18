using PharmaFind.ViewModels;

namespace PharmaFind.Views;

public partial class LoginPage : BasePage
{
    public LoginPage(LoginViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
    }
}
