using PharmaFind.ViewModels;

namespace PharmaFind.Views;

public partial class PharmacyDetailPage : BasePage
{
    public PharmacyDetailPage(PharmacyDetailViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
    }
}
