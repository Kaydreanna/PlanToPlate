namespace PlanToPlate.Views;

public partial class ViewRecipePage : ContentPage
{
	public ViewRecipePage()
	{
		InitializeComponent();
	}

    private async void closeRecipeButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}