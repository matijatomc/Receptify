using Receptify.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Receptify.Views;

public partial class RecipeDetailPage : ContentPage
{
    private int _recipeId;
    public Recipe Recipe { get; set; }
    public ObservableCollection<IngredientItem> Ingredients { get; set; } = new();
    public ObservableCollection<Step> Steps { get; set; } = new();
    public string TagList { get; set; } = string.Empty;

    public ICommand ToggleFavoriteCommand { get; }

    public RecipeDetailPage(int recipeId)
    {
        InitializeComponent();
        _recipeId = recipeId;
        ToggleFavoriteCommand = new Command(OnToggleFavorite);
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadRecipeDetails();
    }

    private async void LoadRecipeDetails()
    {
        await DatabaseService.Init();

        Recipe = await DatabaseService.GetRecipeByIdAsync(_recipeId);
        var recipeIngredients = await DatabaseService.GetRecipeIngredientsAsync(_recipeId);
        var allIngredients = await DatabaseService.GetAllIngredientsAsync();
        var tags = await DatabaseService.GetTagsForRecipeAsync(_recipeId);
        var steps = (await DatabaseService.GetStepsByRecipeIdAsync(_recipeId))
                    .OrderBy(s => s.Order)
                    .ToList();

        Ingredients.Clear();
        foreach (var ri in recipeIngredients)
        {
            var ing = allIngredients.FirstOrDefault(i => i.Id == ri.IngredientId);
            if (ing != null)
            {
                Ingredients.Add(new IngredientItem
                {
                    Name = ing.Name,
                    Unit = ing.Unit,
                    Quantity = ri.Quantity,
                    IngredientId = ing.Id
                });
            }
        }

        Steps.Clear();
        foreach (var s in steps)
            Steps.Add(s);

        TagList = tags.Count > 0 ? string.Join(", ", tags.Select(t => t.Name)) : "Bez oznaka";

        OnPropertyChanged(nameof(Recipe));
        OnPropertyChanged(nameof(TagList));
        OnPropertyChanged(nameof(FavoriteIcon));
    }

    public string FavoriteIcon => Recipe?.IsFavorite == true ? "heart_filled.png" : "heart_outline.png";

    private async void OnToggleFavorite()
    {
        if (Recipe != null)
        {
            Recipe.IsFavorite = !Recipe.IsFavorite;
            await DatabaseService.UpdateRecipeAsync(Recipe);
            OnPropertyChanged(nameof(FavoriteIcon));
        }
    }

    private async void OnGenerateShoppingListClicked(object sender, EventArgs e)
    {
        await DatabaseService.Init();

        var recipeIngredients = await DatabaseService.GetRecipeIngredientsAsync(_recipeId);
        var allIngredients = await DatabaseService.GetAllIngredientsAsync();

        foreach (var ri in recipeIngredients)
        {
            var ing = allIngredients.FirstOrDefault(i => i.Id == ri.IngredientId);
            if (ing == null) continue;

            var existing = await DatabaseService.GetShoppingItemByIngredientIdAsync(ing.Id);

            if (existing != null)
            {
                existing.Quantity += ri.Quantity;
                existing.Name = ing.Name;
                existing.Unit = ing.Unit;

                await DatabaseService.UpdateShoppingItemAsync(existing);
            }
            else
            {
                var newItem = new ShoppingItem
                {
                    IngredientId = ing.Id,
                    Name = ing.Name,
                    Unit = ing.Unit,
                    Quantity = ri.Quantity,
                    IsChecked = false
                };

                await DatabaseService.AddShoppingItemAsync(newItem);
            }
        }

        await Shell.Current.GoToAsync("//shopping");
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new EditRecipePage(_recipeId));
    }

    private async void OnDeleteRecipeClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Potvrda", "Jeste li sigurni da želite izbrisati ovaj recept?", "Da", "Odustani");

        if (!confirm)
            return;

        await DatabaseService.Init();

        await DatabaseService.DeleteRecipeIngredientsAsync(_recipeId);
        await DatabaseService.DeleteStepsByRecipeIdAsync(_recipeId);
        await DatabaseService.DeleteRecipeTagsAsync(_recipeId);
        await DatabaseService.DeleteRecipeAsync(_recipeId);

        await DisplayAlert("Obrisano", "Recept je uspješno izbrisan.", "OK");

        await Navigation.PopAsync();
    }

    private async void OnSaveNoteClicked(object sender, EventArgs e)
    {
        Recipe.Rating = (int)RatingSlider.Value;
        await DatabaseService.UpdateRecipeAsync(Recipe);
        await DisplayAlert("Spremljeno", "Bilješka i ocjena su spremljeni.", "OK");
    }
}
