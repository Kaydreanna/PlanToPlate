using PlanToPlate.Models;
using PlanToPlate.Services;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;

namespace PlanToPlate.Views;

public partial class RecipesPage : ContentPage
{
    public User loggedInUser { get; set; }
    private List<Recipe> currentListOfRecipes = new List<Recipe>();

    public RecipesPage(User user)
	{
		InitializeComponent();
        loggedInUser = user;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        currentListOfRecipes = await DatabaseService.GetRecipes(loggedInUser.UserId, null, null);
        refreshRecipesTable();
        ratingPicker.Items.Add("Options coming soon!");
        ratingPicker.SelectedIndex = 0;
        populateDevicePicker();
        devicePicker.SelectedIndex = 0;
        typePicker.Items.Add("Type");
        typePicker.Items.Add("Breakfast");
        typePicker.Items.Add("Lunch");
        typePicker.Items.Add("Dinner");
        typePicker.SelectedIndex = 0;
        populateIngredientPicker();
        ingredientPicker.SelectedIndex = 0;
    }

    #region Clicked Events
    private async void addRecipeButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new AddRecipePage(loggedInUser));
    }

    private async void clearButton_Clicked(object sender, EventArgs e)
    {
        typePicker.SelectedIndex = 0;
        devicePicker.SelectedIndex = 0;
        ingredientPicker.SelectedIndex = 0;
        searchRecipesEntry.Text = string.Empty;
        currentListOfRecipes = await DatabaseService.GetRecipes(loggedInUser.UserId, null, null);
        refreshRecipesTable();
    }

    private async void searchButton_Clicked(object sender, EventArgs e)
    {
        currentListOfRecipes = await DatabaseService.SearchRecipes(loggedInUser.UserId, searchRecipesEntry.Text);
        refreshRecipesTable();
    }

    private async void viewRecipeButton_Clicked(Recipe recipe)
    {
        await Navigation.PushModalAsync(new ViewRecipePage(recipe));
    }
    #endregion

    #region Methods
    private void refreshRecipesTable()
    {
        if (currentListOfRecipes.Count == 0)
        {
            noRecipesFoundMessage.IsVisible = true;
            recipesGridLabels.IsVisible = false;
            ratingPicker.IsEnabled = false;
            devicePicker.IsEnabled = false;
            typePicker.IsEnabled = false;
            ingredientPicker.IsEnabled = false;
            recipesGridContent.Children.Clear();
            recipesGridContent.IsVisible = false;
        }
        else
        {
            noRecipesFoundMessage.IsVisible = false;
            recipesGridLabels.IsVisible = true;
            ratingPicker.IsEnabled = true;
            devicePicker.IsEnabled = true;
            typePicker.IsEnabled = true;
            ingredientPicker.IsEnabled = true;
            recipesGridContent.IsVisible = true;
            recipesGridContent.Children.Clear();
            int rowNum = 0;
            var tertiaryColor = (Color)Application.Current.Resources["Tertiary"];
            foreach (Recipe recipe in currentListOfRecipes)
            {
                recipesGridContent.RowDefinitions.Add(new RowDefinition());
                Button recipeNameButton = new Button
                {
                    Text = recipe.RecipeName,
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = 14,
                    TextColor = tertiaryColor,
                    BackgroundColor = Colors.Transparent,
                    LineBreakMode = LineBreakMode.WordWrap,
                    Margin = 5,
                    Command = new Command(() => viewRecipeButton_Clicked(recipe)),
                };
                Border borderedRecipeName = new Border
                {
                    StrokeThickness = 1,
                    BackgroundColor = Colors.Transparent,
                    Content = recipeNameButton,
                    Padding = 5,
                    Margin = 0
                };

                Label recipeRatingLabel = new Label
                {
                    Text = "Ratings Coming Soon!",
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = 14
                };
                Border borderedRecipeRating = new Border
                {
                    StrokeThickness = 1,
                    BackgroundColor = Colors.Transparent,
                    Content = recipeRatingLabel,
                    Padding = 5,
                    Margin = 0
                };

                Label recipeDeviceLabel = new Label
                {
                    Text = recipe.CookingDevice,
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = 14
                };
                Border borderedRecipeDevice = new Border
                {
                    StrokeThickness = 1,
                    BackgroundColor = Colors.Transparent,
                    Content = recipeDeviceLabel,
                    Padding = 5,
                    Margin = 0
                };

                Label recipeTypeLabel = new Label
                {
                    Text = recipe.RecipeType,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = 14
                };
                Border borderedRecipeType = new Border
                {
                    StrokeThickness = 1,
                    BackgroundColor = Colors.Transparent,
                    Content = recipeTypeLabel,
                    Padding = 5,
                    Margin = 0
                };

                recipesGridContent.Children.Add(borderedRecipeName);
                Grid.SetRow(borderedRecipeName, rowNum);
                Grid.SetColumn(borderedRecipeName, 0);

                recipesGridContent.Children.Add(borderedRecipeRating);
                Grid.SetRow(borderedRecipeRating, rowNum);
                Grid.SetColumn(borderedRecipeRating, 1);

                recipesGridContent.Children.Add(borderedRecipeDevice);
                Grid.SetRow(borderedRecipeDevice, rowNum);
                Grid.SetColumn(borderedRecipeDevice, 2);

                recipesGridContent.Children.Add(borderedRecipeType);
                Grid.SetRow(borderedRecipeType, rowNum);
                Grid.SetColumn(borderedRecipeType, 3);

                rowNum++;
            }

        }
    }

    private async void populateDevicePicker()
    {
        devicePicker.Items.Clear();
        devicePicker.Items.Add("Device");
        List<Recipe> recipes = await DatabaseService.GetRecipes(loggedInUser.UserId, null, null);
        foreach (Recipe recipe in recipes)
        {
            if (!devicePicker.Items.Contains(recipe.CookingDevice))
            {
                devicePicker.Items.Add(recipe.CookingDevice);
            }
        }
    }

    private async void populateIngredientPicker()
    {
        ingredientPicker.Items.Clear();
        ingredientPicker.Items.Add("Ingredient");
        List<Ingredient> ingredients = await DatabaseService.GetIngredients(loggedInUser.UserId);
        foreach (Ingredient ingredient in ingredients)
        {
            string ingredientName = ingredient.IngredientName.ToLower();
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            string capitalizedIngredientName = textInfo.ToTitleCase(ingredientName);
            if (!ingredientPicker.Items.Contains(capitalizedIngredientName))
            {
                ingredientPicker.Items.Add(capitalizedIngredientName);
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

    private async void typePicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        if(typePicker.SelectedIndex != 0)
        {
            if (typePicker.SelectedItem is string selectedItem)
            {
                devicePicker.SelectedIndex = 0;
                ingredientPicker.SelectedIndex = 0;
                currentListOfRecipes = await DatabaseService.GetRecipes(loggedInUser.UserId, "Type", selectedItem);
                refreshRecipesTable();
            }
        }
    }

    private async void ingredientPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        if(ingredientPicker.SelectedIndex != 0)
        {
            if (ingredientPicker.SelectedItem is string selectedItem)
            {
                devicePicker.SelectedIndex = 0;
                typePicker.SelectedIndex = 0;
                currentListOfRecipes = await DatabaseService.GetRecipes(loggedInUser.UserId, "Ingredient", selectedItem);
                refreshRecipesTable();
            }
        }
    }

    private void ratingPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        //TODO: Add rating filter
    }

    private async void devicePicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        if(devicePicker.SelectedIndex != 0)
        {
            if (devicePicker.SelectedItem is string selectedItem)
            {
                typePicker.SelectedIndex = 0;
                ingredientPicker.SelectedIndex = 0;
                currentListOfRecipes = await DatabaseService.GetRecipes(loggedInUser.UserId, "Device", selectedItem);
                refreshRecipesTable();
            }
        }
    }
}