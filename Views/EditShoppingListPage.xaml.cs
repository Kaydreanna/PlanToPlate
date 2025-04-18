namespace PlanToPlate.Views;

public partial class EditShoppingListPage : ContentPage
{
	public EditShoppingListPage()
	{
		InitializeComponent();
	}

    private async void closeButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private void saveButton_Clicked(object sender, EventArgs e)
    {

    }

    private void cancelButton_Clicked(object sender, EventArgs e)
    {

    }
}