using System.Collections.ObjectModel;

namespace Receptify.Views;

public partial class ShoppingListPage : ContentPage
{
    public ObservableCollection<ShoppingItem> ShoppingItems { get; set; } = new();
    public bool IsEmpty => ShoppingItems.Count == 0;
    public bool IsNotEmpty => ShoppingItems.Count != 0;

    public ShoppingListPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadShoppingItemsAsync();
    }

    private async Task LoadShoppingItemsAsync()
    {
        await DatabaseService.Init();
        ShoppingItems.Clear();
        var items = await DatabaseService.GetShoppingItemsAsync();
        foreach (var item in items)
            ShoppingItems.Add(item);
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(IsNotEmpty));
    }

    private async void OnDeleteCheckedClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Potvrda", "Želiš li izbrisati odabrane namjernice?", "Da", "Ne");
        if (!confirm)
            return;
        var checkedItems = ShoppingItems.Where(i => i.IsChecked).ToList();
        foreach (var item in checkedItems)
        {
            await DatabaseService.DeleteShoppingItemAsync(item.Id);
            ShoppingItems.Remove(item);
        }

        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(IsNotEmpty));
    }

    private async void OnDeleteAllClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Potvrda", "Želiš li izbrisati cijeli popis?", "Da", "Ne");
        if (confirm)
        {
            await DatabaseService.DeleteAllShoppingItemsAsync();
            ShoppingItems.Clear();
        }

        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(IsNotEmpty));
    }
}
