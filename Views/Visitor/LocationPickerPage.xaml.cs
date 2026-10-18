using PharmaFind.ViewModels;

namespace PharmaFind.Views;

public partial class LocationPickerPage : BasePage
{
    public LocationPickerPage(LocationPickerViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
    }
}
