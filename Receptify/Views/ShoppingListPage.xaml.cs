using System.Collections.ObjectModel;

namespace Receptify.Views;

public partial class ShoppingListPage : ContentPage
{
    public ObservableCollection<ShoppingItem> ShoppingItems { get; set; } = new();
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
        var items = await DatabaseService.GetShoppingListAsync();
        foreach (var item in items)
            ShoppingItems.Add(item);
        OnPropertyChanged(nameof(IsNotEmpty));
    }

    private async void OnItemCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (sender is CheckBox cb && cb.BindingContext is ShoppingItem item)
        {
            item.IsChecked = e.Value;
            await DatabaseService.UpdateShoppingItemAsync(item);
        }
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

        OnPropertyChanged(nameof(IsNotEmpty));
    }
}
