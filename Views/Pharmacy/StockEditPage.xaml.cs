using PharmaFind.ViewModels;

namespace PharmaFind.Views;

public partial class StockEditPage : BasePage
{
    public StockEditPage(StockEditViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
    }
}
