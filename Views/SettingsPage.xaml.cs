namespace PlanToPlate.Views;

public partial class SettingsPage : ContentPage
{
	public SettingsPage()
	{
		InitializeComponent();
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
}