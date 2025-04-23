using PlanToPlate.Models;

namespace PlanToPlate.Views;

public partial class SettingsPage : ContentPage
{
    public User loggedInUser { get; set; }
    public SettingsPage(User user)
	{
		InitializeComponent();
        loggedInUser = user;
    }

    #region Clicked Events
    private void saveChangeUsernameButton_Clicked(object sender, EventArgs e)
    {

    }

    private void saveChangePasswordButton_Clicked(object sender, EventArgs e)
    {

    }

    private void logoutButton_Clicked(object sender, EventArgs e)
    {

    }

    private void deleteAccountButton_Clicked(object sender, EventArgs e)
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