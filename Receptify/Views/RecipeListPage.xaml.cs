using System.Collections.ObjectModel;
using System.Windows.Input;
using Receptify.Models;

namespace Receptify.Views;

public partial class RecipeListPage : ContentPage
{
    public ObservableCollection<RecipeDisplay> AllRecipes { get; set; } = new();
    public ObservableCollection<RecipeDisplay> FilteredRecipes { get; set; } = new();
    public ObservableCollection<TagItem> AllTags { get; set; } = new();
    public ObservableCollection<TagItem> EditableTags { get; set; } = new();

    private List<TagItem> SelectedTags = new();
    private string selectedSortOption = "Bez sortiranja";

    public ICommand RecipeTappedCommand { get; }
    public ICommand ToggleTagCommand { get; }

    public bool IsEmpty => FilteredRecipes.Count == 0;

    public RecipeListPage()
    {
        InitializeComponent();
        BindingContext = this;
        RecipeTappedCommand = new Command<RecipeDisplay>(OnRecipeTapped);
        ToggleTagCommand = new Command<TagItem>(OnToggleTag);
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
        SelectedTags.Clear();
        var tags = await DatabaseService.GetAllTagsAsync();
        foreach (var tag in tags)
        {
            var tagItem = new TagItem
            {
                Id = tag.Id,
                Name = tag.Name,
                IsSelected = false
            };
            AllTags.Add(tagItem);
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
                CookingTimeMinutes = recipe.CookingTimeMinutes,
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
        );

        switch (selectedSortOption)
        {
            case "Vrijeme (najkraće prvo)":
                filtered = filtered.OrderBy(r => r.CookingTimeMinutes);
                break;
            case "Vrijeme (najduže prvo)":
                filtered = filtered.OrderByDescending(r => r.CookingTimeMinutes);
                break;
            case "Naziv (A-Z)":
                filtered = filtered.OrderBy(r => r.Title);
                break;
            case "Naziv (Z-A)":
                filtered = filtered.OrderByDescending(r => r.Title);
                break;
        }

        FilteredRecipes.Clear();
        foreach (var r in filtered)
            FilteredRecipes.Add(r);

        OnPropertyChanged(nameof(IsEmpty));
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFilters();
    }

    private void OnSortOptionChanged(object sender, EventArgs e)
    {
        selectedSortOption = SortPicker.SelectedItem?.ToString() ?? "Bez sortiranja";
        ApplyFilters();
    }

    private async void OnRecipeTapped(RecipeDisplay selected)
    {
        if (selected != null)
        {
            await Navigation.PushAsync(new RecipeDetailPage(selected.Id));
        }
    }

    private void OnToggleTag(TagItem tag)
    {
        tag.IsSelected = !tag.IsSelected;

        if (tag.IsSelected)
            SelectedTags.Add(tag);
        else
            SelectedTags.Remove(tag);

        ApplyFilters();
    }

    private void OnEditTagsClicked(object sender, EventArgs e)
    {
        EditableTags.Clear();
        foreach (var tag in AllTags)
        {
            EditableTags.Add(new TagItem
            {
                Id = tag.Id,
                Name = tag.Name
            });
        }
        EditTagsPopup.IsVisible = true;
    }

    private async void OnCancelEditTags(object sender, EventArgs e)
    {
        EditTagsPopup.IsVisible = false;
        await LoadDataAsync();
    }

    private async void OnSaveTags(object sender, EventArgs e)
    {
        await DatabaseService.Init();

        foreach (var tagItem in EditableTags)
        {
            var trimmedName = tagItem.Name?.Trim();

            if (string.IsNullOrWhiteSpace(trimmedName))
                continue;

            if (tagItem.Id > 0)
            {
                var existingTag = await DatabaseService.GetTagByIdAsync(tagItem.Id);
                if (existingTag != null)
                {
                    existingTag.Name = trimmedName;
                    await DatabaseService.UpdateTagAsync(existingTag);
                }
            }
            else
            {
                await DatabaseService.AddTagAsync(new Tag { Name = trimmedName });
            }
        }

        EditTagsPopup.IsVisible = false;
        await LoadDataAsync();
    }

    private async void OnDeleteTagClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var tagItem = button?.CommandParameter as TagItem;
        if (tagItem == null) return;

        bool confirm = await DisplayAlert("Potvrda", $"Želiš li izbrisati oznaku '{tagItem.Name}'?", "Da", "Ne");
        if (confirm && tagItem.Id > 0)
        {
            await DatabaseService.DeleteTagAsync(tagItem.Id);
            EditableTags.Remove(tagItem);
        }
    }
}