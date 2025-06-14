using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Receptify.Models;

namespace Receptify.Views
{
    public partial class AddRecipePage : ContentPage
    {
        public ObservableCollection<EditableItem> Ingredients { get; set; } = new ObservableCollection<EditableItem>();
        public ObservableCollection<StepItem> Steps { get; set; } = new ObservableCollection<StepItem>();

        public ObservableCollection<string> Tags { get; set; } = new ObservableCollection<string>();

        public AddRecipePage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        private void OnAddIngredientClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NewIngredientEntry.Text))
            {
                Ingredients.Add(new EditableItem(NewIngredientEntry.Text.Trim()));
                NewIngredientEntry.Text = string.Empty;
            }
        }
        private void OnRemoveIngredientClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is EditableItem item)
                Ingredients.Remove(item);
        }


        private void OnAddStepClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NewStepEntry.Text))
            {
                Steps.Add(new StepItem
                {
                    Text = NewStepEntry.Text.Trim(),
                    Index = Steps.Count + 1
                });
                NewStepEntry.Text = string.Empty;
            }
        }

        private void OnRemoveStepClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is StepItem item)
            {
                Steps.Remove(item);

                // Update brojeva
                for (int i = 0; i < Steps.Count; i++)
                {
                    Steps[i].Index = i + 1;
                }
            }
        }




        private void OnAddTagClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NewTagEntry.Text))
            {
                Tags.Add(NewTagEntry.Text.Trim());
                NewTagEntry.Text = string.Empty;
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            string title = TitleEntry.Text?.Trim();
            string cookingTime = CookingTimeEntry.Text?.Trim();

            if (string.IsNullOrWhiteSpace(title) || Ingredients.Count == 0 || Steps.Count == 0)
            {
                await DisplayAlert("Greška", "Molimo unesite naziv, sastojke i upute.", "OK");
                return;
            }

            // Ovdje dodaj poziv za spremanje recepta u bazu ili servis
            await DisplayAlert("Uspjeh", "Recept je uspješno dodan!", "OK");

            // Očisti formu
            TitleEntry.Text = string.Empty;
            CookingTimeEntry.Text = string.Empty;
            Ingredients.Clear();
            Steps.Clear();
            Tags.Clear();
        }
    }
}
