using PharmaFind.ViewModels;

namespace PharmaFind.Views;

public partial class CatalogPage : BasePage
{
    public CatalogPage(CatalogViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
    }
}
