using Receptify.Models;

namespace Receptify.Views;

public partial class RecipeDetailPage : ContentPage
{
    private int _recipeId;

    public RecipeDetailPage(int recipeId)
    {
        InitializeComponent();
        _recipeId = recipeId;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadRecipeDetails();
    }

    private async void LoadRecipeDetails()
    {
        await DatabaseService.Init();

        var recipe = await DatabaseService.GetRecipeByIdAsync(_recipeId);
        var ingredients = await DatabaseService.GetIngredientsByRecipeIdAsync(_recipeId);
        var steps = await DatabaseService.GetStepsByRecipeIdAsync(_recipeId);
        var tags = await DatabaseService.GetTagsForRecipeAsync(_recipeId);

        TitleLabel.Text = recipe.Title;
        TimeLabel.Text = recipe.CookingTimeMinutes.ToString();
        TagsLabel.Text = tags.Count > 0 ? string.Join(", ", tags.Select(t => t.Name)) : "Bez oznaka";

        IngredientsList.ItemsSource = ingredients;
        StepsList.ItemsSource = steps.OrderBy(s => s.Order).ToList();
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

        // Izbriši sve povezane podatke
        await DatabaseService.DeleteIngredientsByRecipeIdAsync(_recipeId);
        await DatabaseService.DeleteStepsByRecipeIdAsync(_recipeId);
        await DatabaseService.DeleteRecipeTagsAsync(_recipeId);
        await DatabaseService.DeleteRecipeAsync(_recipeId);

        await DisplayAlert("Obrisano", "Recept je uspješno izbrisan.", "OK");

        // Vrati se na listu
        await Navigation.PopAsync();
    }

}
