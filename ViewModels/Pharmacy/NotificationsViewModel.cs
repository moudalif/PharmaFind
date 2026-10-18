using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PharmaFind.Models;
using PharmaFind.Services;

namespace PharmaFind.ViewModels;

/// <summary>Alertes (stock faible, mise à jour) et signalements des visiteurs concernant la pharmacie.</summary>
public partial class NotificationsViewModel(
    ViewModelContext context,
    IStockService stock,
    IReportService reports) : BaseViewModel(context)
{
    public ObservableCollection<AppNotification> Notifications { get; } = [];
    public ObservableCollection<PharmacyReport> Reports { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowReports))]
    private bool showAlerts = true;

    [ObservableProperty] private int unreadCount;

    public bool ShowReports => !ShowAlerts;

    protected override bool ReloadOnAppearing => true;

    protected override async Task LoadAsync()
    {
        await RunLoadAsync(async () =>
        {
            var pharmacy = await stock.GetMyPharmacyAsync();
            Notifications.Clear();
            foreach (var n in await stock.GetNotificationsAsync()) Notifications.Add(n);
            Reports.Clear();
            foreach (var r in await reports.GetForPharmacyAsync(pharmacy.Id)) Reports.Add(r);
            UnreadCount = Notifications.Count(n => !n.IsRead);
        });
    }

    [RelayCommand]
    private void ShowTab(string tab) => ShowAlerts = tab == "alerts";

    [RelayCommand]
    private async Task MarkAllRead()
    {
        await stock.MarkNotificationsReadAsync();
        UnreadCount = 0;
    }

    [RelayCommand]
    private async Task Open(AppNotification notification)
    {
        notification.IsRead = true;
        UnreadCount = Notifications.Count(n => !n.IsRead);
        if (notification.StockId is { } id)
            await Nav.GoToAsync(Routes.StockEdit, new Dictionary<string, object> { [NavParams.StockId] = id });
        else if (notification.ReportId is not null)
            ShowAlerts = false;
    }

    [RelayCommand]
    private Task OpenReport(PharmacyReport report)
        => Dialogs.AlertAsync(report.ReasonText,
            $"{report.Comment}\n\nConcerne : {report.Subject}\nReçu {report.CreatedText} · statut : {ReportTexts.Status(report.Status)}\n\n" +
            "Corrigez l'information concernée dans vos stocks ou votre fiche. L'administration suit le traitement du signalement.");
}
