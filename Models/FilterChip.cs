using CommunityToolkit.Mvvm.ComponentModel;

namespace PharmaFind.Models;

/// <summary>Puce de filtre sélectionnable (rayon, statut, forme…). Value porte la valeur métier.</summary>
public partial class FilterChip(string label, object? value) : ObservableObject
{
    public string Label { get; } = label;
    public object? Value { get; } = value;

    [ObservableProperty]
    private bool isSelected;

    public static List<FilterChip> Create(params (string Label, object? Value)[] items)
        => items.Select(i => new FilterChip(i.Label, i.Value)).ToList();
}
