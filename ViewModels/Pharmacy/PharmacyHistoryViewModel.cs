using System.Collections.ObjectModel;
using PharmaFind.Models;
using PharmaFind.Services;

namespace PharmaFind.ViewModels;

/// <summary>Historique des modifications de stock et de fiche, pour la traçabilité.</summary>
public partial class PharmacyHistoryViewModel(ViewModelContext context, IStockService stock) : BaseViewModel(context)
{
    public ObservableCollection<StockChange> Changes { get; } = [];

    protected override bool ReloadOnAppearing => true;

    protected override async Task LoadAsync()
    {
        await RunLoadAsync(async () =>
        {
            Changes.Clear();
            foreach (var c in await stock.GetHistoryAsync()) Changes.Add(c);
            if (Changes.Count == 0) State = ViewState.Empty;
        });
    }
}
