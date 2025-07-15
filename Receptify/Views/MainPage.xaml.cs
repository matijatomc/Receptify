using Receptify.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Receptify.Views;

public partial class MainPage : ContentPage
{
    public ObservableCollection<RecipeDisplay> FavoritePreview { get; set; } = new();
    public ICommand FavoriteTappedCommand { get; }

    public bool IsEmpty => FavoritePreview.Count == 0;
    public bool IsNotEmpty => FavoritePreview.Count != 0;

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;

        FavoriteTappedCommand = new Command<RecipeDisplay>(OnFavoriteTapped);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadFavoritesAsync();
    }

    private async Task LoadFavoritesAsync()
    {
        await DatabaseService.Init();
        var all = await DatabaseService.GetAllRecipesAsync();

        var favorites = all
            .Where(r => r.IsFavorite)
            .Take(3)
            .ToList();

        FavoritePreview.Clear();
        foreach (var fav in favorites)
        {
            var tags = await DatabaseService.GetTagsForRecipeAsync(fav.Id);
            FavoritePreview.Add(new RecipeDisplay
            {
                Id = fav.Id,
                Title = fav.Title,
                CookingTimeMinutes = fav.CookingTimeMinutes,
                Rating = fav.Rating,
                IsFavorite = fav.IsFavorite,
                Tags = tags.Select(t => t.Name).ToList()
            });
        }

        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(IsNotEmpty));
    }

    private async void OnAddRecipeClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//add");
    }

    private async void OnViewRecipesClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//list");
    }

    private async void OnViewShoppingListClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//shopping");
    }

    private async void OnViewAllFavoritesClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//list?favorites=true");
    }

    private async void OnFavoriteTapped(RecipeDisplay selected)
    {
        if (selected != null)
        {
            await Navigation.PushAsync(new RecipeDetailPage(selected.Id));
        }
    }

}
