using System.Threading.Tasks;

namespace PlanToPlate.Views;

public partial class RecipesPage : ContentPage
{
	public RecipesPage()
	{
		InitializeComponent();
	}
    private async void addRecipeButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new AddRecipePage());
    }

    private void clearButton_Clicked(object sender, EventArgs e)
    {

    }

    private void searchButton_Clicked(object sender, EventArgs e)
    {

    }

    private async void viewRecipeButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new ViewRecipePage());
    }

    #region Nav Bar
    private void recipesButton_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new RecipesPage());
        Navigation.RemovePage(this);
    }

    private void shoppingListButton_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new ShoppingListPage());
        Navigation.RemovePage(this);
    }

    private void homeButton_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new HomePage());
        Navigation.RemovePage(this);
    }

    private void scheduleMealsButton_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new ScheduleMealsPage());
        Navigation.RemovePage(this);
    }

    private void settingsButton_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new SettingsPage());
        Navigation.RemovePage(this);
    }
    #endregion

}