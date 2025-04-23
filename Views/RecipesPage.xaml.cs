using PlanToPlate.Models;
using System.Threading.Tasks;

namespace PlanToPlate.Views;

public partial class RecipesPage : ContentPage
{
    public User loggedInUser { get; set; }

    public RecipesPage(User user)
	{
		InitializeComponent();
        loggedInUser = user;
    }

    #region Clicked Events
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
    #endregion

    #region Nav Bar
    private void recipesButton_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new RecipesPage(loggedInUser));
        Navigation.RemovePage(this);
    }

    private void shoppingListButton_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new ShoppingListPage(loggedInUser));
        Navigation.RemovePage(this);
    }

    private void homeButton_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new HomePage(loggedInUser));
        Navigation.RemovePage(this);
    }

    private void scheduleMealsButton_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new ScheduleMealsPage(loggedInUser));
        Navigation.RemovePage(this);
    }

    private void settingsButton_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new SettingsPage(loggedInUser));
        Navigation.RemovePage(this);
    }
    #endregion

}