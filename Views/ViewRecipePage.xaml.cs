using PlanToPlate.Models;

namespace PlanToPlate.Views;

public partial class ViewRecipePage : ContentPage
{
    public Recipe selectedRecipe { get; set; }
    public ViewRecipePage(Recipe recipe)
	{
		InitializeComponent();
        selectedRecipe = recipe;
    }

    private async void closeRecipeButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}