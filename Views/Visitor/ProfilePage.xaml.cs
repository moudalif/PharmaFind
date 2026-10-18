using PharmaFind.ViewModels;

namespace PharmaFind.Views;

public partial class ProfilePage : BasePage
{
    public ProfilePage(ProfileViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
    }
}
