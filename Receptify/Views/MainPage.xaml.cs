namespace Receptify.Views;

public partial class MainPage : ContentPage
{
    int count = 0;

    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnAddRecipeClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddRecipePage());
    }

    private async void OnViewRecipesClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RecipeListPage());
    }
}
