using PharmaFind.ViewModels;

namespace PharmaFind.Views;

public partial class MedicineDetailPage : BasePage
{
    public MedicineDetailPage(MedicineDetailViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
    }
}
