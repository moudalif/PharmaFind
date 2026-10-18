using Microsoft.Maui.Controls.Shapes;
using PharmaFind.Helpers;

namespace PharmaFind.Controls;

/// <summary>
/// Pastille de statut réutilisable. Accepte tout statut connu de <see cref="StatusPalette"/>
/// (disponibilité, validation, signalement, compte, rôle, délivrance, fraîcheur, ouvert/fermé).
/// Le texte est toujours affiché : la couleur n'est jamais le seul porteur d'information.
/// </summary>
public class Badge : Border
{
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value), typeof(object), typeof(Badge), null,
        propertyChanged: (b, _, _) => ((Badge)b).Refresh());

    public static readonly BindableProperty TextOverrideProperty = BindableProperty.Create(
        nameof(TextOverride), typeof(string), typeof(Badge), null,
        propertyChanged: (b, _, _) => ((Badge)b).Refresh());

    private readonly Label _label;

    public Badge()
    {
        StrokeThickness = 0;
        StrokeShape = new RoundRectangle { CornerRadius = 11 };
        Padding = new Thickness(9, 3);
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Center;
        _label = new Label { FontSize = 12, FontAttributes = FontAttributes.Bold, LineBreakMode = LineBreakMode.NoWrap };
        Content = _label;
        Refresh();
    }

    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>Texte à afficher à la place du libellé par défaut (couleurs conservées).</summary>
    public string? TextOverride
    {
        get => (string?)GetValue(TextOverrideProperty);
        set => SetValue(TextOverrideProperty, value);
    }

    private void Refresh()
    {
        // IsVisible n'est jamais modifié ici : il reste libre pour une liaison dans le XAML.
        if (Value is null)
        {
            _label.Text = string.Empty;
            BackgroundColor = Colors.Transparent;
            Padding = new Thickness(0);
            return;
        }
        var (text, fg, bg) = StatusPalette.For(Value);
        _label.Text = TextOverride ?? text;
        _label.TextColor = fg;
        BackgroundColor = bg;
        Padding = new Thickness(9, 3);
    }
}
