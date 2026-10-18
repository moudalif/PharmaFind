using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PharmaFind.Helpers;
using PharmaFind.Models;
using PharmaFind.Services;

namespace PharmaFind.ViewModels;

/// <summary>Choix de la position : GPS (avec consentement) ou saisie d'une ville, d'un arrondissement ou d'un quartier.</summary>
public partial class LocationPickerViewModel(
    ViewModelContext context,
    ILocationService location,
    SearchContext search) : BaseViewModel(context)
{
    public ObservableCollection<KnownPlace> Places { get; } = [];

    [ObservableProperty] private string query = string.Empty;
    [ObservableProperty] private string? gpsError;

    public string CurrentLabel => search.LocationLabel;

    protected override Task LoadAsync()
    {
        Filter();
        return Task.CompletedTask;
    }

    partial void OnQueryChanged(string value) => Filter();

    private void Filter()
    {
        Places.Clear();
        foreach (var p in location.GetKnownPlaces().Where(p => TextSearch.Matches(Query, p.Name, p.District, p.City)))
            Places.Add(p);
        State = Places.Count == 0 ? ViewState.Empty : ViewState.Content;
    }

    [RelayCommand]
    private async Task UseGps()
    {
        GpsError = null;
        IsWorking = true;
        var result = await location.GetCurrentLocationAsync();
        IsWorking = false;

        if (result.Location is { } loc)
        {
            search.CurrentLocation = loc;
            await Nav.GoBackAsync();
            return;
        }
        GpsError = result.Error switch
        {
            LocationError.PermissionDenied => "Accès à la localisation refusé. Choisissez un quartier dans la liste ci-dessous, ou autorisez la localisation dans les réglages du téléphone.",
            LocationError.Disabled => "La localisation est désactivée sur le téléphone.",
            _ => "Position introuvable pour le moment."
        };
    }

    [RelayCommand]
    private async Task SelectPlace(KnownPlace place)
    {
        search.CurrentLocation = new UserLocation
        {
            Latitude = place.Latitude,
            Longitude = place.Longitude,
            Label = $"{place.Name}, {place.City}",
            Source = LocationSource.Manuelle
        };
        await Nav.GoBackAsync();
    }
}
