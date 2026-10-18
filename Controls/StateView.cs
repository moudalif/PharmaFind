using System.Windows.Input;
using PharmaFind.Models;

namespace PharmaFind.Controls;

/// <summary>
/// Affiche les états hors contenu : chargement, liste vide, erreur serveur, absence de connexion.
/// À placer dans la même cellule de Grid que le contenu : il le recouvre quand State ≠ Content.
/// </summary>
public class StateView : ContentView
{
    public static readonly BindableProperty StateProperty = BindableProperty.Create(
        nameof(State), typeof(ViewState), typeof(StateView), ViewState.Content,
        propertyChanged: (b, _, _) => ((StateView)b).Refresh());

    public static readonly BindableProperty MessageProperty = BindableProperty.Create(
        nameof(Message), typeof(string), typeof(StateView), null,
        propertyChanged: (b, _, _) => ((StateView)b).Refresh());

    public static readonly BindableProperty EmptyTitleProperty = BindableProperty.Create(
        nameof(EmptyTitle), typeof(string), typeof(StateView), "Aucun résultat",
        propertyChanged: (b, _, _) => ((StateView)b).Refresh());

    public static readonly BindableProperty EmptyMessageProperty = BindableProperty.Create(
        nameof(EmptyMessage), typeof(string), typeof(StateView), null,
        propertyChanged: (b, _, _) => ((StateView)b).Refresh());

    public static readonly BindableProperty RetryCommandProperty = BindableProperty.Create(
        nameof(RetryCommand), typeof(ICommand), typeof(StateView), null,
        propertyChanged: (b, _, n) => ((StateView)b)._retry.Command = (ICommand?)n);

    public static readonly BindableProperty EmptyActionTextProperty = BindableProperty.Create(
        nameof(EmptyActionText), typeof(string), typeof(StateView), null,
        propertyChanged: (b, _, _) => ((StateView)b).Refresh());

    public static readonly BindableProperty EmptyActionCommandProperty = BindableProperty.Create(
        nameof(EmptyActionCommand), typeof(ICommand), typeof(StateView), null,
        propertyChanged: (b, _, n) => ((StateView)b)._emptyAction.Command = (ICommand?)n);

    private readonly ActivityIndicator _spinner = new() { IsRunning = true, WidthRequest = 36, HeightRequest = 36 };
    private readonly Image _image = new() { WidthRequest = 64, HeightRequest = 64 };
    private readonly Label _title = new() { FontSize = 18, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center };
    private readonly Label _message = new() { FontSize = 14, HorizontalTextAlignment = TextAlignment.Center, TextColor = Color.FromArgb("#4B5563") };
    private readonly Button _retry = new() { Text = "Réessayer", HorizontalOptions = LayoutOptions.Center, Margin = new Thickness(0, 8, 0, 0) };
    private readonly Button _emptyAction = new() { HorizontalOptions = LayoutOptions.Center, Margin = new Thickness(0, 8, 0, 0) };

    public StateView()
    {
        BackgroundColor = Color.FromArgb("#F5F7F9");
        Content = new VerticalStackLayout
        {
            Spacing = 10,
            Padding = new Thickness(32),
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            Children = { _spinner, _image, _title, _message, _retry, _emptyAction }
        };
        Refresh();
    }

    public ViewState State { get => (ViewState)GetValue(StateProperty); set => SetValue(StateProperty, value); }
    public string? Message { get => (string?)GetValue(MessageProperty); set => SetValue(MessageProperty, value); }
    public string EmptyTitle { get => (string)GetValue(EmptyTitleProperty); set => SetValue(EmptyTitleProperty, value); }
    public string? EmptyMessage { get => (string?)GetValue(EmptyMessageProperty); set => SetValue(EmptyMessageProperty, value); }
    public ICommand? RetryCommand { get => (ICommand?)GetValue(RetryCommandProperty); set => SetValue(RetryCommandProperty, value); }
    public string? EmptyActionText { get => (string?)GetValue(EmptyActionTextProperty); set => SetValue(EmptyActionTextProperty, value); }
    public ICommand? EmptyActionCommand { get => (ICommand?)GetValue(EmptyActionCommandProperty); set => SetValue(EmptyActionCommandProperty, value); }

    private void Refresh()
    {
        IsVisible = State != ViewState.Content;
        _spinner.IsVisible = _spinner.IsRunning = State == ViewState.Loading;
        _image.IsVisible = State is ViewState.Empty or ViewState.Error or ViewState.Offline;
        _retry.IsVisible = State is ViewState.Error or ViewState.Offline;
        _emptyAction.IsVisible = State == ViewState.Empty && !string.IsNullOrEmpty(EmptyActionText);
        _emptyAction.Text = EmptyActionText;

        switch (State)
        {
            case ViewState.Loading:
                _title.Text = string.Empty;
                _message.Text = Message ?? "Chargement…";
                break;
            case ViewState.Empty:
                _image.Source = "state_empty.png";
                _title.Text = EmptyTitle;
                _message.Text = EmptyMessage ?? Message;
                break;
            case ViewState.Offline:
                _image.Source = "state_offline.png";
                _title.Text = "Pas de connexion internet";
                _message.Text = Message ?? "Vérifiez vos données mobiles ou le Wi-Fi, puis réessayez.";
                break;
            case ViewState.Error:
                _image.Source = "state_error.png";
                _title.Text = "Un problème est survenu";
                _message.Text = Message ?? "Le service ne répond pas pour le moment. Réessayez dans quelques instants.";
                break;
        }
        _title.IsVisible = !string.IsNullOrEmpty(_title.Text);
        _message.IsVisible = !string.IsNullOrEmpty(_message.Text);
    }
}
