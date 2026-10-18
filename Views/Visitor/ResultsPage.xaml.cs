using PharmaFind.ViewModels;

namespace PharmaFind.Views;

public partial class ResultsPage : BasePage
{
    public ResultsPage(ResultsViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
    }
}
