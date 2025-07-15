using System.Windows.Input;
using Receptify.Models;

namespace Receptify.Views;

public partial class RecipeDetailPage : ContentPage
{
    private int _recipeId;
    public Recipe Recipe { get; set; }
    public List<Ingredient> Ingredients { get; set; } = new();
    public List<Step> Steps { get; set; } = new();
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
        Ingredients = await DatabaseService.GetIngredientsByRecipeIdAsync(_recipeId);
        Steps = (await DatabaseService.GetStepsByRecipeIdAsync(_recipeId)).OrderBy(s => s.Order).ToList();
        var tags = await DatabaseService.GetTagsForRecipeAsync(_recipeId);

        TagList = tags.Count > 0 ? string.Join(", ", tags.Select(t => t.Name)) : "Bez oznaka";

        OnPropertyChanged(nameof(Recipe));
        OnPropertyChanged(nameof(Ingredients));
        OnPropertyChanged(nameof(Steps));
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
        var ingredients = await DatabaseService.GetIngredientsByRecipeIdAsync(_recipeId);

        foreach (var ing in ingredients)
        {
            await DatabaseService.AddShoppingItemAsync(new ShoppingItem
            {
                Text = ing.Text,
                IsChecked = false
            });
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

        await DatabaseService.DeleteIngredientsByRecipeIdAsync(_recipeId);
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
