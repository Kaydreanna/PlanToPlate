using System.Threading.Tasks;

namespace PlanToPlate.Views;

public partial class HomePage : ContentPage
{
	public HomePage()
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
}