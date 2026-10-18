using PharmaFind.ViewModels;

namespace PharmaFind.Views;

public partial class AdminMedicineEditPage : BasePage
{
    public AdminMedicineEditPage(AdminMedicineEditViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
    }
}
