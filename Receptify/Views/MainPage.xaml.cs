namespace Receptify.Views;

public partial class MainPage : ContentPage
{

    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnAddRecipeClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//add");
    }

    private async void OnViewRecipesClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//list");
    }
}
