using System.Collections.ObjectModel;
using System.ComponentModel;
using Receptify.Models;
using System.Text.RegularExpressions;

namespace Receptify.Views;

public partial class AddRecipePage : ContentPage
{
    public ObservableCollection<IngredientItem> Ingredients { get; set; } = new();
    public ObservableCollection<StepItem> Steps { get; set; } = new();
    public ObservableCollection<TagItem> Tags { get; set; } = new();

    public AddRecipePage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        LoadTags();
    }

    private async void LoadTags()
    {
        await DatabaseService.Init();
        var allTags = await DatabaseService.GetAllTagsAsync();
        Tags.Clear();
        foreach (var tag in allTags)
        {
            Tags.Add(new TagItem { Name = tag.Name, IsSelected = false });
        }
    }

    private void OnAddIngredientClicked(object sender, EventArgs e)
    {
        if (NewIngredientEntry == null || string.IsNullOrWhiteSpace(NewIngredientEntry.Text))
            return;

        Ingredients.Add(new IngredientItem { Text = NewIngredientEntry.Text.Trim() });
        NewIngredientEntry.Text = string.Empty;
        
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
        if (NewStepEntry == null || string.IsNullOrWhiteSpace(NewStepEntry.Text))
            return;

        Steps.Add(new StepItem
        {
            StepNumber = $"{Steps.Count + 1}.",
            Description = NewStepEntry.Text.Trim()
        });

        NewStepEntry.Text = string.Empty;
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
        else if (int.TryParse(lower, out int minutes))
            totalMinutes += minutes;

        return totalMinutes;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        TitleEntry.BackgroundColor = Colors.Transparent;
        CookingTimeEntry.BackgroundColor = Colors.Transparent;
        NewIngredientEntry.BackgroundColor = Colors.Transparent;
        NewStepEntry.BackgroundColor = Colors.Transparent;

        bool isValid = true;

        if (string.IsNullOrWhiteSpace(TitleEntry.Text))
        {
            TitleEntry.BackgroundColor = Colors.MistyRose;
            isValid = false;
        }

        if (string.IsNullOrWhiteSpace(CookingTimeEntry.Text))
        {
            CookingTimeEntry.BackgroundColor = Colors.MistyRose;
            isValid = false;
        }

        if (Ingredients.Count == 0)
        {
            NewIngredientEntry.BackgroundColor = Colors.MistyRose;
            isValid = false;
        }

        if (Steps.Count == 0 || Steps.All(s => string.IsNullOrWhiteSpace(s.Description)))
        {
            NewStepEntry.BackgroundColor = Colors.MistyRose;
            isValid = false;
        }

        if (!isValid)
        {
            await DisplayAlert("Greška", "Molimo ispunite obavezna polja označena crvenom bojom.", "OK");
            return;
        }

        int cookingMinutes = ParseCookingTimeToMinutes(CookingTimeEntry.Text);

        if (cookingMinutes == 0)
        {
            await DisplayAlert("Greška", "Vrijeme kuhanja mora biti broj u minutama.", "OK");
            return;
        }

        await DatabaseService.Init();

        var recipe = new Recipe
        {
            Title = TitleEntry.Text.Trim(),
            CookingTimeMinutes = ParseCookingTimeToMinutes(CookingTimeEntry.Text.Trim())
        };

        await DatabaseService.AddRecipeAsync(recipe);

        foreach (var ing in Ingredients)
        {
            await DatabaseService.AddIngredientAsync(new Ingredient
            {
                Text = ing.Text.Trim(),
                RecipeId = recipe.Id
            });
        }

        for (int i = 0; i < Steps.Count; i++)
        {
            var stepDesc = Steps[i].Description?.Trim();
            if (!string.IsNullOrWhiteSpace(stepDesc))
            {
                await DatabaseService.AddStepAsync(new Step
                {
                    Description = stepDesc,
                    Order = i + 1,
                    RecipeId = recipe.Id
                });
            }
        }

        var selectedTags = Tags.Where(t => t.IsSelected).ToList();

        foreach (var tagItem in selectedTags)
        {
            var tagInDb = await DatabaseService.GetTagByNameAsync(tagItem.Name);
            if (tagInDb != null)
            {
                await DatabaseService.AddRecipeTagAsync(new RecipeTag
                {
                    RecipeId = recipe.Id,
                    TagId = tagInDb.Id
                });
            }
        }

        await DisplayAlert("Uspjeh", "Recept spremljen!", "OK");

        TitleEntry.Text = "";
        CookingTimeEntry.Text = "";
        Ingredients.Clear();
        Steps.Clear();
        foreach (var tag in Tags)
        {
            tag.IsSelected = false;
        }

        await Shell.Current.GoToAsync("//list");
    }
}
