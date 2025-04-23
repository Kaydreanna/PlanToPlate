using PlanToPlate.Models;
using System.Threading.Tasks;

namespace PlanToPlate.Views;

public partial class HomePage : ContentPage
{
    public User loggedInUser { get; set; }
    public HomePage(User user)
	{
		InitializeComponent();
        loggedInUser = user;
    }

    #region Clicked Events
    private void breakfastButton_Clicked(object sender, EventArgs e)
    {

    }

    private void lunchButton_Clicked(object sender, EventArgs e)
    {

    }

    private void dinnerButton_Clicked(object sender, EventArgs e)
    {

    }

    private void previousDatesButton_Clicked(object sender, EventArgs e)
    {

    }

    private void futureDatesButton_Clicked(object sender, EventArgs e)
    {

    }

    private void scheduledRecipeButton_Clicked(object sender, EventArgs e)
    {

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