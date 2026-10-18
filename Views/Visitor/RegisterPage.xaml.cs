using PharmaFind.ViewModels;

namespace PharmaFind.Views;

public partial class RegisterPage : BasePage
{
    public RegisterPage(RegisterViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
    }
}
