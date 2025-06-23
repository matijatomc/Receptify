using System.Collections.ObjectModel;
using System.Windows.Input;
using Receptify.Models;

namespace Receptify.Views;

public partial class RecipeListPage : ContentPage
{
    public ObservableCollection<RecipeDisplay> AllRecipes { get; set; } = new();
    public ObservableCollection<RecipeDisplay> FilteredRecipes { get; set; } = new();
    public ObservableCollection<Tag> AllTags { get; set; } = new();
    private List<Tag> SelectedTags = new();

    public ICommand RecipeTappedCommand { get; }
    public ICommand ToggleTagCommand { get; }


    public RecipeListPage()
    {
        InitializeComponent();
        BindingContext = this;
        RecipeTappedCommand = new Command<RecipeDisplay>(OnRecipeTapped);
        ToggleTagCommand = new Command<Tag>(OnToggleTag);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        await DatabaseService.Init();

        AllTags.Clear();
        var tags = await DatabaseService.GetAllTagsAsync();
        foreach (var tag in tags)
        {
            tag.IsSelected = false;
            AllTags.Add(tag);
        }
            

        AllRecipes.Clear();
        var recipeList = await DatabaseService.GetAllRecipesAsync();

        foreach (var recipe in recipeList)
        {
            var recipeTags = await DatabaseService.GetTagsForRecipeAsync(recipe.Id);
            AllRecipes.Add(new RecipeDisplay
            {
                Id = recipe.Id,
                Title = recipe.Title,
                CookingTime = recipe.CookingTime,
                Tags = recipeTags.Select(t => t.Name).ToList()
            });
        }

        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var query = SearchEntry.Text?.Trim().ToLower() ?? string.Empty;

        var filtered = AllRecipes.Where(r =>
            (string.IsNullOrWhiteSpace(query) || r.Title.ToLower().Contains(query)) &&
            (!SelectedTags.Any() || SelectedTags.All(tag => r.Tags.Contains(tag.Name)))
        ).ToList();

        FilteredRecipes.Clear();
        foreach (var r in filtered)
            FilteredRecipes.Add(r);
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFilters();
    }

    private async void OnRecipeTapped(RecipeDisplay selected)
    {
        if (selected != null)
        {
            await Navigation.PushAsync(new RecipeDetailPage(selected.Id));
        }
    }
    private void OnToggleTag(Tag tag)
    {
        tag.IsSelected = !tag.IsSelected;

        if (tag.IsSelected)
            SelectedTags.Add(tag);
        else
            SelectedTags.Remove(tag);

        ApplyFilters();
    }

}
