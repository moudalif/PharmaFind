using PharmaFind.ViewModels;

namespace PharmaFind.Views;

public partial class AdminUsersPage : BasePage
{
    public AdminUsersPage(AdminUsersViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
    }
}
