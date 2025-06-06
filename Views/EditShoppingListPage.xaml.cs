using PlanToPlate.Models;
using PlanToPlate.Services;
using System.Threading.Tasks;

namespace PlanToPlate.Views;

public partial class EditShoppingListPage : ContentPage
{
    private ShoppingList shoppingList { get; set; }
    private Dictionary<string, bool> listOfIngredients { get; set; }
    private Dictionary<string, Button> ingredientButtons = new Dictionary<string, Button>();
    public EditShoppingListPage(ShoppingList shoppingListToEdit)
	{
		InitializeComponent();
        shoppingList = shoppingListToEdit;
        listOfIngredients = shoppingList.IngredientList;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        shoppingListDatesLabel.Text = $"{shoppingList.StartDate.ToString("MM/dd/yyyy")} - {shoppingList.EndDate.ToString("MM/dd/yyyy")}";
        displayIngredients();
    }

    #region Clicked Events
    private async void closeButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void saveButton_Clicked(object sender, EventArgs e)
    {
        bool ingredientsInShoppingList = false;
        foreach(string ingredient in listOfIngredients.Keys)
        {
            if (listOfIngredients[ingredient])
            {
                ingredientsInShoppingList = true;
                break;
            }
        }
        if (!ingredientsInShoppingList)
        {
            bool deleteList = await DisplayAlert("Empty Shopping List", "This shopping list is empty. Would you like to delete it?", "Yes", "No");
            if(deleteList)
            {
                await DatabaseService.DeleteShoppingList(shoppingList);
                await Navigation.PopAsync();
            }
        }
        await DatabaseService.UpdateShoppingList(shoppingList, listOfIngredients);
        await Navigation.PopAsync();
    }
    private async void cancelButton_Clicked(object sender, EventArgs e)
    {
        bool closePage = await DisplayAlert("Close Page", "Are you sure you want to close this page? Any unsaved changes will be lost.", "Yes", "No");
        if(closePage)
        {
            await Navigation.PopAsync();
        }
    }
    #endregion

    #region Methods
    private void displayIngredients()
    {
        var tertiaryColor = (Color)Microsoft.Maui.Controls.Application.Current.Resources["Tertiary"];
        var secondaryColor = (Color)Microsoft.Maui.Controls.Application.Current.Resources["Secondary"];
        shoppingListIngredientsGrid.RowDefinitions.Clear();
        int rowNum = 0;
        int colNum = 0;
        shoppingListIngredientsGrid.RowDefinitions.Add(new RowDefinition());

        var sortedIngredients = listOfIngredients.OrderByDescending(kv => kv.Value).ThenBy(kv => kv.Key).ToList();
        ingredientButtons.Clear();
        foreach (var ingredient in sortedIngredients)
        {
            string ingredientName = ingredient.Key;
            bool needsBought = ingredient.Value;

            if (colNum == 3)
            {
                colNum = 0;
                shoppingListIngredientsGrid.RowDefinitions.Add(new RowDefinition());
                rowNum++;
            }
            Button ingredientButton = new Button
            {
                Text = ingredientName,
                TextColor = tertiaryColor,
                BorderWidth = 1,
                BorderColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Center,
                LineBreakMode = LineBreakMode.WordWrap,
                Margin = new Thickness(5)
            };
            ingredientButton.BindingContext = ingredientName;
            ingredientButton.Clicked += (s, e) => {toggleIngredientButton(ingredientButton.BindingContext.ToString());};
            ingredientButton.BackgroundColor = listOfIngredients[ingredientName] ? secondaryColor : Colors.White;
            shoppingListIngredientsGrid.Children.Add(ingredientButton);
            shoppingListIngredientsGrid.SetColumn(ingredientButton, colNum);
            shoppingListIngredientsGrid.SetRow(ingredientButton, rowNum);
            ingredientButtons.Add(ingredientName, ingredientButton);

            colNum++;
        }
    }

    private void toggleIngredientButton(string ingredientName)
    {
        listOfIngredients[ingredientName] = !listOfIngredients[ingredientName];
        if (listOfIngredients[ingredientName])
        {
            ingredientButtons[ingredientName].BackgroundColor = (Color)Microsoft.Maui.Controls.Application.Current.Resources["Secondary"];
        }
        else
        {
            ingredientButtons[ingredientName].BackgroundColor = Colors.White;
        }
    }
    #endregion

}