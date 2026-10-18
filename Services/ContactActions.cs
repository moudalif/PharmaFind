using Microsoft.Maui.ApplicationModel.Communication;
using PharmaFind.Helpers;
using PharmaFind.Models;

namespace PharmaFind.Services;

/// <summary>Appel téléphonique et itinéraire, délégués aux applications du téléphone.</summary>
public static class ContactActions
{
    public static async Task CallAsync(Pharmacy pharmacy, IDialogService dialogs)
    {
        if (!PhoneFormatter.IsValid(pharmacy.Phone))
        {
            await dialogs.AlertAsync("Numéro indisponible", "Le numéro de cette pharmacie n'est pas renseigné correctement.");
            return;
        }
        try
        {
            if (PhoneDialer.Default.IsSupported)
                PhoneDialer.Default.Open(PhoneFormatter.ForDialer(pharmacy.Phone));
            else
                await dialogs.AlertAsync("Appel impossible", $"Composez le {pharmacy.PhoneDisplay} depuis un téléphone.");
        }
        catch (Exception)
        {
            await dialogs.AlertAsync("Appel impossible", $"Composez le {pharmacy.PhoneDisplay}.");
        }
    }

    /// <summary>Ouvre l'application de navigation : elle calcule la distance routière et le temps de trajet.</summary>
    public static async Task OpenRouteAsync(Pharmacy pharmacy, IDialogService dialogs)
    {
        if (!pharmacy.HasCoordinates)
        {
            await dialogs.AlertAsync("Itinéraire indisponible",
                $"Cette pharmacie n'a pas encore de coordonnées GPS. Repère : {pharmacy.Landmark}, {pharmacy.FullLocationText}.");
            return;
        }
        try
        {
            var opened = await Microsoft.Maui.ApplicationModel.Map.Default.TryOpenAsync(
                new Microsoft.Maui.Devices.Sensors.Location(pharmacy.Latitude!.Value, pharmacy.Longitude!.Value),
                new MapLaunchOptions
                {
                    Name = pharmacy.Name,
                    NavigationMode = Microsoft.Maui.ApplicationModel.NavigationMode.Driving
                });
            if (!opened)
                await dialogs.AlertAsync("Itinéraire indisponible", "Aucune application de navigation n'est disponible sur cet appareil.");
        }
        catch (Exception)
        {
            await dialogs.AlertAsync("Itinéraire indisponible", "Le service de navigation ne répond pas.");
        }
    }
}
