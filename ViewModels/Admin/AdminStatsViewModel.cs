using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PharmaFind.Models;
using PharmaFind.Services;

namespace PharmaFind.ViewModels;

/// <summary>Statistiques globales et qualité des données (valeurs fictives dans la maquette).</summary>
public partial class AdminStatsViewModel(ViewModelContext context, IAdminService admin) : BaseViewModel(context)
{
    public ObservableCollection<ChartBar> SearchesPerDay { get; } = [];
    public ObservableCollection<ChartBar> TopSearched { get; } = [];
    public ObservableCollection<ChartBar> ByDistrict { get; } = [];
    public ObservableCollection<ChartBar> Freshness { get; } = [];

    [ObservableProperty] private DashboardStats? stats;

    protected override async Task LoadAsync()
    {
        await RunLoadAsync(async () =>
        {
            Stats = await admin.GetStatsAsync();
            Fill(SearchesPerDay, await admin.GetSearchesPerDayAsync());
            Fill(TopSearched, await admin.GetTopSearchedAsync());
            Fill(ByDistrict, await admin.GetPharmaciesByDistrictAsync());
            Fill(Freshness, await admin.GetFreshnessDistributionAsync());
        });
    }

    private static void Fill(ObservableCollection<ChartBar> target, IEnumerable<ChartBar> source)
    {
        target.Clear();
        foreach (var b in source) target.Add(b);
    }
}
