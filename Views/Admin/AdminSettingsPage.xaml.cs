using PharmaFind.ViewModels;

namespace PharmaFind.Views;

public partial class AdminSettingsPage : BasePage
{
    public AdminSettingsPage(AdminSettingsViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
    }
}
