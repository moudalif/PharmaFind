using PharmaFind.ViewModels;

namespace PharmaFind.Views;

public partial class AdminDashboardPage : BasePage
{
    public AdminDashboardPage(AdminDashboardViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
    }
}
