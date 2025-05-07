using PlanToPlate.Models;
using PlanToPlate.Services;

namespace PlanToPlate.Views;

public partial class ShoppingListPage : ContentPage
{
    public User loggedInUser { get; set; }
    private Dictionary<int, bool> shoppingListVisibility = new Dictionary<int, bool>();
    private Dictionary<int, Grid> shoppingListGridLayout = new Dictionary<int, Grid>();
    public ShoppingListPage(User user)
	{
		InitializeComponent();
        loggedInUser = user;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        displayShoppingLists();
    }

    #region Clicked Events
    private async void createShoppingListButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new EditShoppingListPage());
    }
    #endregion

    #region Methods
    private async void displayShoppingLists()
    {
        List<ShoppingList> shoppingLists = await DatabaseService.GetAllShoppingLists(loggedInUser.UserId);
        if(shoppingLists.Count > 0)
        {
            noShoppingListsFoundLabel.IsVisible = false;
            pastShoppingListsGrid.IsVisible = true;
            var primaryColor = (Color)Microsoft.Maui.Controls.Application.Current.Resources["Primary"];

            int rowNum = 0;
            foreach(ShoppingList shoppingList in shoppingLists)
            {
                pastShoppingListsGrid.RowDefinitions.Add(new RowDefinition());
                shoppingListVisibility[shoppingList.ListId] = false;
                Button shoppingListButton = new Button
                {
                    Text = $"{shoppingList.StartDate.ToString("MM/dd/yyyy")} to {shoppingList.EndDate.ToString("MM/dd/yyyy")}",
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.Center,
                    //Padding = new Thickness(100, 0),
                    //Margin = new Thickness(200, 10)
                };
                shoppingListButton.BindingContext = shoppingList.ListId;
                shoppingListButton.Clicked += (s, e) => toggleVisibility((int)shoppingListButton.BindingContext);
                ImageButton shareImageButton = new ImageButton
                {
                    Source = "shareicon.png",
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Center,
                    BackgroundColor = primaryColor,
                    WidthRequest = 30,
                    HeightRequest = 30
                };
                shareImageButton.BindingContext = shoppingList.ListId;
                shareImageButton.Clicked += (s, e) => toggleVisibility((int)shoppingListButton.BindingContext);

                pastShoppingListsGrid.Children.Add(shoppingListButton);
                pastShoppingListsGrid.SetColumn(shoppingListButton, 1);
                pastShoppingListsGrid.SetRow(shoppingListButton, rowNum);
                pastShoppingListsGrid.Children.Add(shareImageButton);
                pastShoppingListsGrid.SetColumn(shareImageButton, 1);
                pastShoppingListsGrid.SetRow(shareImageButton, rowNum);

                Grid shoppingListDetailsGrid = createShoppingListDetailsGrid(shoppingList);
                shoppingListGridLayout[shoppingList.ListId] = shoppingListDetailsGrid;

                rowNum++;
            }
        }
        else
        {
            noShoppingListsFoundLabel.IsVisible = true;
            pastShoppingListsGrid.IsVisible = false;
        }
    }

    private Grid createShoppingListDetailsGrid(ShoppingList shoppingList)
    {
        if(shoppingListGridLayout.ContainsKey(shoppingList.ListId))
        {
            return shoppingListGridLayout[shoppingList.ListId];
        } else
        {
            Grid shoppingListDetailsGrid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(),
                    new ColumnDefinition(),
                    new ColumnDefinition()
                }
            };
            int columnNum = 0;
            int rowNum = 0;
            foreach(Ingredient ingredient in shoppingList.ListForShopping)
            {
                if(columnNum == 2)
                {
                    shoppingListDetailsGrid.RowDefinitions.Add(new RowDefinition());
                    columnNum = 0;
                    rowNum++;
                }
                Label ingredientLabel = new Label
                {
                    Text = ingredient.IngredientName,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, 10)
                };
                shoppingListDetailsGrid.Children.Add(ingredientLabel);
                shoppingListDetailsGrid.SetRow(ingredientLabel, rowNum);
                shoppingListDetailsGrid.SetColumn(ingredientLabel, columnNum);
                columnNum++;
            }
            return shoppingListDetailsGrid;
        }
    }

    private void toggleVisibility(int shoppingListId)
    {
        shoppingListVisibility[shoppingListId] = !shoppingListVisibility[shoppingListId];

        if(shoppingListGridLayout.ContainsKey(shoppingListId))
        {
            var shoppingListDetailsGrid = shoppingListGridLayout[shoppingListId];
            shoppingListDetailsGrid.IsVisible = !shoppingListDetailsGrid.IsVisible;
            if (shoppingListDetailsGrid.IsVisible)
            {
                shoppingListDetailsGrid.HeightRequest = -1;
            }
            else
            {
                shoppingListDetailsGrid.HeightRequest = 0;
            }
        }
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