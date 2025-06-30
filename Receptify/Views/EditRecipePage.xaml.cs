using Receptify.Models;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Receptify.Views;

public partial class EditRecipePage : ContentPage
{
    private int _recipeId;
    public ObservableCollection<IngredientItem> Ingredients { get; set; } = new();
    public ObservableCollection<StepItem> Steps { get; set; } = new();
    public ObservableCollection<TagItem> Tags { get; set; } = new();

    public EditRecipePage(int recipeId)
    {
        InitializeComponent();
        _recipeId = recipeId;
        BindingContext = this;
        LoadRecipeData();
    }

    private async void LoadRecipeData()
    {
        await DatabaseService.Init();

        var recipe = await DatabaseService.GetRecipeByIdAsync(_recipeId);
        var ingredients = await DatabaseService.GetIngredientsByRecipeIdAsync(_recipeId);
        var steps = await DatabaseService.GetStepsByRecipeIdAsync(_recipeId);
        var allTags = await DatabaseService.GetAllTagsAsync();
        var selectedTags = await DatabaseService.GetTagsForRecipeAsync(_recipeId);

        TitleEntry.Text = recipe.Title;
        CookingTimeEntry.Text = recipe.CookingTimeMinutes.ToString();

        Ingredients.Clear();
        foreach (var ing in ingredients)
            Ingredients.Add(new IngredientItem { Text = ing.Text });

        Steps.Clear();
        for (int i = 0; i < steps.Count; i++)
            Steps.Add(new StepItem { StepNumber = (i + 1).ToString() + ".", Description = steps[i].Description });

        Tags.Clear();
        foreach (var tag in allTags)
        {
            Tags.Add(new TagItem
            {
                Name = tag.Name,
                IsSelected = selectedTags.Any(st => st.Name == tag.Name)
            });
        }
    }

    private void OnAddIngredientClicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(NewIngredientEntry.Text))
        {
            Ingredients.Add(new IngredientItem { Text = NewIngredientEntry.Text.Trim() });
            NewIngredientEntry.Text = string.Empty;
        }
    }

    private void OnDeleteIngredientClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var ingredient = button?.CommandParameter as IngredientItem;
        if (ingredient != null)
        {
            Ingredients.Remove(ingredient);
        }
    }

    private void OnAddStepClicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(NewStepEntry.Text))
        {
            Steps.Add(new StepItem
            {
                StepNumber = $"{Steps.Count + 1}.",
                Description = NewStepEntry.Text.Trim()
            });

            NewStepEntry.Text = string.Empty;
        }
    }

    private void OnDeleteStepClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var step = button?.CommandParameter as StepItem;

        if (step != null)
        {
            Steps.Remove(step);

            for (int i = 0; i < Steps.Count; i++)
            {
                Steps[i].StepNumber = (i + 1).ToString() + ".";
            }
        }
    }

    private async void OnAddTagClicked(object sender, EventArgs e)
    {
        var trimmed = NewTagEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(trimmed) || Tags.Any(t => t.Name == trimmed))
            return;

        var tag = new Tag { Name = trimmed };
        await DatabaseService.AddTagAsync(tag);

        Tags.Add(new TagItem { Name = trimmed, IsSelected = false });
        NewTagEntry.Text = "";
    }

    private int ParseCookingTimeToMinutes(string input)
    {
        int totalMinutes = 0;
        var lower = input.ToLower();

        var match = Regex.Match(lower, @"(?:(\d+)h)?\s*(\d+)?\s*min");
        if (match.Success)
        {
            if (int.TryParse(match.Groups[1].Value, out int hours))
                totalMinutes += hours * 60;

            if (int.TryParse(match.Groups[2].Value, out int minutes))
                totalMinutes += minutes;
        }
        else if (lower.Contains("h"))
        {
            int hours = int.Parse(lower.Split('h')[0].Trim());
            totalMinutes += hours * 60;
        }
        else if (lower.Contains("min"))
        {
            int minutes = int.Parse(lower.Split("min")[0].Trim());
            totalMinutes += minutes;
        }

        return totalMinutes;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TitleEntry.Text) || string.IsNullOrWhiteSpace(CookingTimeEntry.Text) || Ingredients.Count == 0 || Steps.Count == 0)
        {
            await DisplayAlert("Greška", "Molimo ispunite sva obavezna polja.", "OK");
            return;
        }

        int cookingMinutes = ParseCookingTimeToMinutes(CookingTimeEntry.Text);

        if (cookingMinutes == 0)
        {
            await DisplayAlert("Greška", "Vrijeme kuhanja mora biti broj u minutama.", "OK");
            return;
        }

        var recipe = await DatabaseService.GetRecipeByIdAsync(_recipeId);
        recipe.Title = TitleEntry.Text.Trim();
        recipe.CookingTimeMinutes = cookingMinutes;
        await DatabaseService.UpdateRecipeAsync(recipe);

        await DatabaseService.DeleteIngredientsByRecipeIdAsync(_recipeId);
        await DatabaseService.DeleteStepsByRecipeIdAsync(_recipeId);
        await DatabaseService.DeleteRecipeTagsAsync(_recipeId);

        foreach (var ing in Ingredients)
        {
            await DatabaseService.AddIngredientAsync(new Ingredient
            {
                Text = ing.Text.Trim(),
                RecipeId = _recipeId
            });
        }

        for (int i = 0; i < Steps.Count; i++)
        {
            await DatabaseService.AddStepAsync(new Step
            {
                Description = Steps[i].Description,
                Order = i + 1,
                RecipeId = _recipeId
            });
        }

        foreach (var tag in Tags.Where(t => t.IsSelected))
        {
            var tagInDb = await DatabaseService.GetTagByNameAsync(tag.Name);
            if (tagInDb != null)
            {
                await DatabaseService.AddRecipeTagAsync(new RecipeTag
                {
                    RecipeId = _recipeId,
                    TagId = tagInDb.Id
                });
            }
        }

        await DisplayAlert("Uspjeh", "Recept je uspješno uređen.", "OK");
        await Navigation.PopAsync();
    }
}