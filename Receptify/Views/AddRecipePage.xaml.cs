using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Receptify.Models;

namespace Receptify.Views;

public partial class AddRecipePage : ContentPage
{
    private bool _isSelectingSuggestion = false;
    public ObservableCollection<IngredientItem> Ingredients { get; set; } = new();
    public ObservableCollection<Ingredient> AllIngredients { get; set; } = new();
    public ObservableCollection<Ingredient> FilteredIngredients { get; set; } = new();
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
        LoadIngredients();
    }

    private async void LoadTags()
    {
        var allTags = await DatabaseService.GetAllTagsAsync();
        Tags.Clear();
        foreach (var tag in allTags)
        {
            Tags.Add(new TagItem { Name = tag.Name, IsSelected = false });
        }
    }

    private async void LoadIngredients()
    {
        var ingredients = await DatabaseService.GetAllIngredientsAsync();
        AllIngredients.Clear();
        foreach (var ing in ingredients)
            AllIngredients.Add(ing);
    }

    private void UpdateSuggestions(string? query)
    {
        AllIngredients ??= new ObservableCollection<Ingredient>();
        FilteredIngredients ??= new ObservableCollection<Ingredient>();

        IEnumerable<Ingredient> source;

        if (string.IsNullOrWhiteSpace(query))
        {
            source = AllIngredients
                .Where(i => !string.IsNullOrWhiteSpace(i?.Name))
                .OrderBy(i => i.Name)
                .Take(100);
        }
        else
        {
            var q = query.Trim().ToLower();
            source = AllIngredients
                .Where(i => !string.IsNullOrWhiteSpace(i?.Name) &&
                            i.Name.ToLower().StartsWith(q))
                .OrderBy(i => i.Name)
                .Take(50);
        }

        FilteredIngredients.Clear();
        foreach (var item in source)
            FilteredIngredients.Add(item);

        IngredientSuggestions.IsVisible = FilteredIngredients.Any();
    }

    private void OnIngredientNameChanged(object sender, TextChangedEventArgs e)
    {
        UpdateSuggestions(e.NewTextValue);
    }

    private void OnIngredientEntryFocused(object sender, FocusEventArgs e)
    {
        UpdateSuggestions(IngredientNameEntry.Text);
    }

    private async void OnIngredientEntryUnfocused(object sender, FocusEventArgs e)
    {
        await Task.Delay(80);
        if (_isSelectingSuggestion) return;

        IngredientSuggestions.IsVisible = false;
    }

    private void OnIngredientSuggestionTapped(object sender, EventArgs e)
    {
        try
        {
            _isSelectingSuggestion = true;

            if (sender is Label lbl && lbl.BindingContext is Ingredient selected)
            {
                IngredientNameEntry.Text = selected.Name;
                UnitEntry.Text = selected.Unit;

                IngredientSuggestions.IsVisible = false;
                FilteredIngredients.Clear();

                QuantityEntry?.Focus();
            }
        }
        finally
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(80);
                _isSelectingSuggestion = false;
            });
        }
    }

    private async void OnAddIngredientClicked(object sender, EventArgs e)
    {
        try
        {
            var name = IngredientNameEntry?.Text?.Trim();
            var unit = UnitEntry?.Text?.Trim();
            var quantityText = QuantityEntry?.Text?.Trim();

            if (!string.IsNullOrEmpty(quantityText))
                quantityText = quantityText.Replace('.', ',');

            if (!double.TryParse(quantityText, out double quantity))
            {
                await DisplayAlert("Greška", "Količina mora biti broj.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                await DisplayAlert("Greška", "Unesi naziv sastojka.", "OK");
                return;
            }

            AllIngredients ??= new ObservableCollection<Ingredient>();
            FilteredIngredients ??= new ObservableCollection<Ingredient>();

            var existing = AllIngredients.FirstOrDefault(i =>
                !string.IsNullOrEmpty(i?.Name) &&
                i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            Ingredient ingredient;

            if (existing != null)
            {
                ingredient = existing;

                if (string.IsNullOrWhiteSpace(unit))
                {
                    unit = ingredient.Unit;
                    UnitEntry.Text = unit;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(unit))
                {
                    await DisplayAlert("Greška", "Unesi mjernu jedinicu za novu namirnicu.", "OK");
                    return;
                }

                ingredient = new Ingredient { Name = name, Unit = unit };

                var rows = await DatabaseService.AddIngredientAsync(ingredient);

                if (ingredient.Id <= 0 || rows <= 0)
                {
                    var all = await DatabaseService.GetAllIngredientsAsync();
                    var fromDb = all.FirstOrDefault(i =>
                        !string.IsNullOrEmpty(i?.Name) &&
                        i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

                    if (fromDb == null)
                    {
                        await DisplayAlert("Greška", "Ne mogu dohvatiti novododanu namirnicu iz baze.", "OK");
                        return;
                    }

                    ingredient = fromDb;
                }

                if (!AllIngredients.Any(i => i.Id == ingredient.Id))
                    AllIngredients.Add(ingredient);
            }

            if (ingredient.Id <= 0)
            {
                await DisplayAlert("Greška", "ID sastojka nije valjan.", "OK");
                return;
            }

            Ingredients.Add(new IngredientItem
            {
                Name = ingredient.Name,
                Unit = ingredient.Unit,
                Quantity = quantity,
                IngredientId = ingredient.Id
            });

            IngredientNameEntry.Text = "";
            UnitEntry.Text = "";
            QuantityEntry.Text = "";
            IngredientSuggestions.IsVisible = false;
            FilteredIngredients.Clear();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Greška", $"Neuspješno dodavanje sastojka.\nDetalji: {ex.Message}", "OK");
        }
    }

    private async void OnEditIngredientClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var ingredient = button?.CommandParameter as IngredientItem;
        if (ingredient == null) return;

        string defaultText = $"{ingredient.Quantity};{ingredient.Unit};{ingredient.Name}";
        string result = await DisplayPromptAsync("Uredi sastojak", "Unesi kao: količina;jedinica;naziv", "Spremi", "Odustani", "npr. 1.5;kg;Pašta", -1, Keyboard.Text, defaultText);

        if (string.IsNullOrWhiteSpace(result)) return;

        var parts = result.Split(';');

        if (parts.Length != 3 || !double.TryParse(parts[0].Replace('.', ','), out double quantity))
        {
            await DisplayAlert("Greška", "Unos nije ispravan. Format mora biti: količina;jedinica;naziv", "OK");
            return;
        }

        string unit = parts[1].Trim();
        string name = parts[2].Trim();

        ingredient.Quantity = quantity;
        ingredient.Unit = unit;
        ingredient.Name = name;

        if (ingredient.IngredientId != null)
        {
            var dbIngredient = await DatabaseService.GetIngredientByIdAsync(ingredient.IngredientId.Value);
            if (dbIngredient != null)
            {
                dbIngredient.Name = name;
                dbIngredient.Unit = unit;
                await DatabaseService.UpdateIngredientAsync(dbIngredient);
            }
        }

        Ingredients.Remove(ingredient);
        Ingredients.Add(ingredient);
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
        if (string.IsNullOrWhiteSpace(NewStepEntry.Text))
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
                Steps[i].StepNumber = $"{i + 1}.";
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
            await DisplayAlert("Greška", "Dodaj barem jedan sastojak.", "OK");
            return;
        }

        if (!isValid)
        {
            await DisplayAlert("Greška", "Molimo ispuni označena polja.", "OK");
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
            CookingTimeMinutes = cookingMinutes
        };

        await DatabaseService.AddRecipeAsync(recipe);

        foreach (var ing in Ingredients)
        {
            var ri = new RecipeIngredient
            {
                RecipeId = recipe.Id,
                IngredientId = ing.IngredientId.Value,
                Quantity = ing.Quantity
            };

            await DatabaseService.AddRecipeIngredientAsync(ri);
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
