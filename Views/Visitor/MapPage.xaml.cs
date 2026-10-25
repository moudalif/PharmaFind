using System.ComponentModel;
using PharmaFind.Services.Maps;
using PharmaFind.ViewModels;

namespace PharmaFind.Views;

public partial class MapPage : BasePage
{
    private readonly MapViewModel _vm;

    public MapPage(MapViewModel vm) : base(vm)
    {
        InitializeComponent();
        _vm = vm;
        _vm.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RefreshMap();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MapViewModel.MapHtml))
            RefreshMap();
    }

    private void RefreshMap()
    {
        if (!string.IsNullOrEmpty(_vm.MapHtml))
        {
            MapWebView.Source = new HtmlWebViewSource { Html = _vm.MapHtml };
        }
    }

    /// <summary>Intercepte les URL émises par la carte (marqueur touché, erreur de chargement).</summary>
    private void OnMapNavigating(object? sender, WebNavigatingEventArgs e)
    {
        var prefix = LeafletMapProvider.HostPrefix;
        if (!e.Url.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return;

        e.Cancel = true;
        var raw = e.Url[prefix.Length..].TrimEnd('/');
        var qIdx = raw.IndexOf('?');
        var path = qIdx >= 0 ? raw[..qIdx] : raw;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (path.StartsWith("pharmacy/") && int.TryParse(path["pharmacy/".Length..], out var id))
                _vm.OnMarkerSelected(id);
            else if (path.StartsWith("error/"))
                _vm.OnMapFailed();
            else if (path == "map/tap")
                _vm.OnMapTapped();
        });
    }
}
