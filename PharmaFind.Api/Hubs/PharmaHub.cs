using Microsoft.AspNetCore.SignalR;
using PharmaFind.Api.Models;

namespace PharmaFind.Api.Hubs;

public interface IPharmaClient
{
    Task StockUpdated(StockItem item);
    Task StockRemoved(int stockId, int pharmacyId);
    Task PharmacyStatusChanged(int pharmacyId, ValidationStatus newStatus, string? reason);
    Task NotificationReceived(AppNotification notification);
    Task ReportSubmitted(PharmacyReport report);
    Task MedicineUpdated(Medicine medicine);
}

public class PharmaHub : Hub<IPharmaClient>
{
    public async Task JoinPharmacyGroup(int pharmacyId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"pharmacy_{pharmacyId}");
    }

    public async Task LeavePharmacyGroup(int pharmacyId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"pharmacy_{pharmacyId}");
    }

    public async Task JoinAdminGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "admin");
    }

    public async Task LeaveAdminGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "admin");
    }
}
