using PharmaFind.ViewModels;

namespace PharmaFind.Views;

public partial class PharmacyInfoPage : BasePage
{
    public PharmacyInfoPage(PharmacyInfoViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
    }
}
