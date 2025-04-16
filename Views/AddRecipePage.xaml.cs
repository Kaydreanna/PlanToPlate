using System.Threading.Tasks;

namespace PlanToPlate.Views;

public partial class AddRecipePage : ContentPage
{
	public AddRecipePage()
	{
		InitializeComponent();
	}


    private async void closeRecipeButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private void addIngredientButton_Clicked(object sender, EventArgs e)
    {

    }

    private void addInstructionButton_Clicked(object sender, EventArgs e)
    {

    }

    private void saveButton_Clicked(object sender, EventArgs e)
    {

    }
}