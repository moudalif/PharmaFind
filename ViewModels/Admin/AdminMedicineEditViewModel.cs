using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PharmaFind.Models;
using PharmaFind.Services;

namespace PharmaFind.ViewModels;

/// <summary>Ajout ou modification d'un médicament du référentiel. Unicité nom + dosage + forme.</summary>
public partial class AdminMedicineEditViewModel(ViewModelContext context, IMedicineService medicines) : BaseViewModel(context), IQueryAttributable
{
    private int _medicineId;

    public ObservableCollection<string> Forms { get; } = [];
    public ObservableCollection<string> Categories { get; } = [];
    public List<string> DeliveryOptions { get; } = ["Sans ordonnance", "Sur ordonnance", "À vérifier"];

    [ObservableProperty] private bool isNew;
    [ObservableProperty] private string brandName = string.Empty;
    [ObservableProperty] private string inn = string.Empty;
    [ObservableProperty] private string dosage = string.Empty;
    [ObservableProperty] private string? form;
    [ObservableProperty] private string? category;
    [ObservableProperty] private string description = string.Empty;
    [ObservableProperty] private string? delivery = "À vérifier";
    [ObservableProperty] private bool isActive = true;
    [ObservableProperty] private string? errorText;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
        => _medicineId = ReadInt(query, NavParams.MedicineId) ?? 0;

    protected override async Task LoadAsync()
    {
        await RunLoadAsync(async () =>
        {
            Forms.Clear();
            foreach (var f in await medicines.GetFormsAsync()) Forms.Add(f);
            Categories.Clear();
            foreach (var c in await medicines.GetCategoriesAsync()) Categories.Add(c);

            IsNew = _medicineId == 0;
            Title = IsNew ? "Nouveau médicament" : "Modifier le médicament";
            if (IsNew) return;

            var m = await medicines.GetByIdAsync(_medicineId);
            if (m is null)
            {
                StateMessage = "Médicament introuvable.";
                State = ViewState.Error;
                return;
            }
            BrandName = m.BrandName;
            Inn = m.Inn;
            Dosage = m.Dosage;
            Form = m.Form;
            Category = m.Category;
            Description = m.Description;
            Delivery = DeliveryOptions[(int)m.Delivery];
            IsActive = m.IsActive;
        });
    }

    [RelayCommand]
    private async Task Save()
    {
        ErrorText = null;
        if (BrandName.Trim().Length < 2) { ErrorText = "Le nom commercial est obligatoire."; return; }
        if (Inn.Trim().Length < 3) { ErrorText = "La DCI est obligatoire."; return; }
        if (string.IsNullOrWhiteSpace(Dosage)) { ErrorText = "Le dosage est obligatoire (ex. 500 mg)."; return; }
        if (string.IsNullOrEmpty(Form)) { ErrorText = "Choisissez une forme."; return; }

        var medicine = new Medicine
        {
            Id = _medicineId,
            BrandName = BrandName.Trim(),
            Inn = Inn.Trim(),
            Dosage = Dosage.Trim(),
            Form = Form,
            Category = Category ?? string.Empty,
            Description = Description.Trim(),
            Delivery = (DeliveryCondition)Math.Max(0, DeliveryOptions.IndexOf(Delivery ?? "À vérifier")),
            IsActive = IsActive
        };

        if (await RunActionAsync(() => medicines.SaveAsync(medicine)))
            await Nav.GoBackAsync();
    }

    [RelayCommand]
    private async Task AddForm()
    {
        var value = await Dialogs.PromptAsync("Nouvelle forme", "Ex. : Suppositoire, Crème, Collyre", "Forme");
        if (string.IsNullOrWhiteSpace(value)) return;
        value = value.Trim();
        if (!Forms.Contains(value)) Forms.Add(value);
        Form = value;
    }

    [RelayCommand]
    private async Task AddCategory()
    {
        var value = await Dialogs.PromptAsync("Nouvelle catégorie", "Ex. : Dermatologie", "Catégorie");
        if (string.IsNullOrWhiteSpace(value)) return;
        value = value.Trim();
        if (!Categories.Contains(value)) Categories.Add(value);
        Category = value;
    }
}
