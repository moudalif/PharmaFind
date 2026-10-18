using PharmaFind.ViewModels;

namespace PharmaFind.Views;

public partial class PharmacyDashboardPage : BasePage
{
    public PharmacyDashboardPage(PharmacyDashboardViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
    }
}
