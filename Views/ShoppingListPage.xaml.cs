using PlanToPlate.Models;
using PlanToPlate.Services;
using System.Globalization;
using System.Threading.Tasks;

namespace PlanToPlate.Views;

public partial class ShoppingListPage : ContentPage
{
    public User loggedInUser { get; set; }
    private Dictionary<int, bool> shoppingListVisibility = new Dictionary<int, bool>();
    private Dictionary<int, VerticalStackLayout> shoppingListGridLayout = new Dictionary<int, VerticalStackLayout>();
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
        if (startDatePicker.Date > endDatePicker.Date)
        {
            await DisplayAlert("Error", "Start date must be before end date.", "OK");
            return;
        }
        List<ScheduledMeals> mealsForShoppingList = await DatabaseService.GetScheduledMealsByDate(loggedInUser.UserId, startDatePicker.Date, endDatePicker.Date);
        if(mealsForShoppingList.Count == 0)
        {
            await DisplayAlert("Error", "No meals scheduled for the selected dates. Please plan some meals then come back to make a shopping list.", "OK");
            return;
        }
        ShoppingList newShoppingList = new ShoppingList
        {
            UserId = loggedInUser.UserId,
            StartDate = startDatePicker.Date,
            EndDate = endDatePicker.Date,
            IngredientList = new Dictionary<string, bool>()
        };
        foreach (ScheduledMeals meal in mealsForShoppingList)
        {
            List<string> ingredients = await DatabaseService.GetRecipeIngredients(meal.RecipeId);
            foreach(string ingredient in ingredients)
            {
                newShoppingList.IngredientList.Add(ingredient, true);
            }
        }
        await Navigation.PushModalAsync(new EditShoppingListPage(newShoppingList));
    }
    #endregion

    #region Methods
    private async void displayShoppingLists()
    {
        pastShoppingListsGrid.RowDefinitions.Clear();
        pastShoppingListsGrid.Children.Clear();

        List<ShoppingList> shoppingLists = await DatabaseService.GetAllShoppingLists(loggedInUser.UserId);
        if(shoppingLists.Count > 0)
        {
            noShoppingListsFoundLabel.IsVisible = false;
            pastShoppingListsGrid.IsVisible = true;
            var secondaryColor = (Color)Microsoft.Maui.Controls.Application.Current.Resources["Secondary"];
            var secondaryDarkColor = (Color)Microsoft.Maui.Controls.Application.Current.Resources["SecondaryDark"];
            var tertiaryColor = (Color)Microsoft.Maui.Controls.Application.Current.Resources["Tertiary"];

            int rowNum = 0;
            foreach(ShoppingList shoppingList in shoppingLists)
            {
                pastShoppingListsGrid.RowDefinitions.Add(new RowDefinition());
                pastShoppingListsGrid.RowDefinitions.Add(new RowDefinition());
                shoppingListVisibility[shoppingList.ListId] = false;
                var buttonGrid = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = GridLength.Star},
                        new ColumnDefinition { Width = GridLength.Auto}
                    },
                    VerticalOptions = LayoutOptions.Center,
                    BackgroundColor = secondaryColor,
                    Padding = 0,
                    Margin = 0
                };
                Button shoppingListButton = new Button
                {
                    Text = $"{shoppingList.StartDate.ToString("MM/dd/yyyy")} to {shoppingList.EndDate.ToString("MM/dd/yyyy")}",
                    TextColor = tertiaryColor,
                    BackgroundColor = Colors.Transparent,
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = 18,
                    CornerRadius = 0,
                    Padding = 0,
                    Margin = 0
                };
                shoppingListButton.BindingContext = shoppingList.ListId;
                shoppingListButton.Clicked += (s, e) =>
                {
                    if(buttonGrid.BackgroundColor == secondaryColor)
                    {
                        buttonGrid.BackgroundColor = secondaryDarkColor;
                    }
                    else
                    {
                        buttonGrid.BackgroundColor = secondaryColor;
                    }
                    toggleVisibility((int)shoppingListButton.BindingContext);
                };
                
                ImageButton shareImageButton = new ImageButton
                {
                    Source = "shareicon.png",
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Center,
                    WidthRequest = 30,
                    HeightRequest = 30,
                    BackgroundColor = Colors.Transparent,
                    Padding = 0,
                    Margin = 0
                };
                shareImageButton.BindingContext = shoppingList.ListId;
                shareImageButton.Clicked += (s, e) => toggleVisibility((int)shareImageButton.BindingContext);

                buttonGrid.Children.Add(shoppingListButton);
                buttonGrid.SetColumn(shoppingListButton, 0);
                buttonGrid.Children.Add(shareImageButton);
                buttonGrid.SetColumn(shareImageButton, 1);
                pastShoppingListsGrid.Children.Add(buttonGrid);
                pastShoppingListsGrid.SetColumn(buttonGrid, 1);
                pastShoppingListsGrid.SetRow(buttonGrid, rowNum);

                var container = new VerticalStackLayout
                {
                    IsVisible = false
                };

                Grid shoppingListDetailsGrid = createShoppingListDetailsGrid(shoppingList);

                container.Children.Add(shoppingListDetailsGrid);
                pastShoppingListsGrid.Children.Add(container);
                pastShoppingListsGrid.SetColumn(container, 1);
                pastShoppingListsGrid.SetRow(container, rowNum + 1);
                shoppingListGridLayout[shoppingList.ListId] = container;

                rowNum+= 2;
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
        Grid shoppingListDetailsGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(),
                new ColumnDefinition(),
                new ColumnDefinition()
            }
        };
        shoppingListDetailsGrid.RowDefinitions.Add(new RowDefinition());
        int columnNum = 0;
        int rowNum = 0;
        foreach (var ingredient in shoppingList.IngredientList)
        {
            if (shoppingList.IngredientList[ingredient.Key] == false)
            {
                continue;
            }
            if (columnNum == 3)
            {
                shoppingListDetailsGrid.RowDefinitions.Add(new RowDefinition());
                columnNum = 0;
                rowNum++;
            }
            Label ingredientLabel = new Label
            {
                Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(ingredient.Key.ToLower()),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 10)
            };
            shoppingListDetailsGrid.Children.Add(ingredientLabel);
            shoppingListDetailsGrid.SetRow(ingredientLabel, rowNum);
            shoppingListDetailsGrid.SetColumn(ingredientLabel, columnNum);
            columnNum++;
        }
        ImageButton editImageButton = new ImageButton
        {
            Source = "editicon.png",
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            WidthRequest = 5,
            HeightRequest = 5,
            BackgroundColor = Colors.Transparent,
            Padding = 0,
            Margin = 0
        };
        editImageButton.BindingContext = shoppingList.ListId;
        editImageButton.Clicked += async (s, e) => await Navigation.PushModalAsync(new EditShoppingListPage(shoppingList));
        shoppingListDetailsGrid.Children.Add(editImageButton);
        if(columnNum == 3)
        {
            shoppingListDetailsGrid.RowDefinitions.Add(new RowDefinition());
            rowNum++;
            shoppingListDetailsGrid.SetRow(editImageButton, rowNum);
            shoppingListDetailsGrid.SetColumn(editImageButton, 2);
        }
        else
        {
            shoppingListDetailsGrid.SetRow(editImageButton, rowNum);
            shoppingListDetailsGrid.SetColumn(editImageButton, 2);
        }

        return shoppingListDetailsGrid;
    }

    private async Task toggleVisibility(int shoppingListId)
    {
        shoppingListVisibility[shoppingListId] = !shoppingListVisibility[shoppingListId];

        if(shoppingListGridLayout.ContainsKey(shoppingListId))
        {
            var shoppingListDetailsGrid = shoppingListGridLayout[shoppingListId];
            shoppingListDetailsGrid.IsVisible = shoppingListVisibility[shoppingListId];
            shoppingListDetailsGrid.HeightRequest = shoppingListDetailsGrid.IsVisible ? -1 : 0;
            pastShoppingListsGrid.InvalidateMeasure();
            
        } else
        {
            //ShoppingList shoppingList = await DatabaseService.GetShoppingListById(shoppingListId);
            //createShoppingListDetailsGrid(shoppingList);
            await DisplayAlert("Error", "Shopping list not found.", "OK");
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