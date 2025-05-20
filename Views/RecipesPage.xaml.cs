using PlanToPlate.Models;
using PlanToPlate.Services;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;

namespace PlanToPlate.Views;

public partial class RecipesPage : ContentPage
{
    public User loggedInUser { get; set; }
    //private List<Recipe> currentListOfRecipes = new List<Recipe>();
    private Dictionary<Recipe, float> currentListOfRecipesAndRatings = new Dictionary<Recipe, float>();

    public RecipesPage(User user)
	{
		InitializeComponent();
        loggedInUser = user;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        List<Recipe> recipes = await DatabaseService.GetRecipes(loggedInUser.UserId, null, null);
        currentListOfRecipesAndRatings = await DatabaseService.GetRecipesAndRatings(loggedInUser.UserId, recipes);
        await refreshRecipesTable();
        ratingPicker.Items.Add("Rating");
        ratingPicker.Items.Add("None");
        ratingPicker.Items.Add("0-1");
        ratingPicker.Items.Add("1-2");
        ratingPicker.Items.Add("2-3");
        ratingPicker.Items.Add("3-4");
        ratingPicker.Items.Add("4-5");
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
        await Navigation.PushAsync(new AddRecipePage(loggedInUser, null));
    }

    private async void clearButton_Clicked(object sender, EventArgs e)
    {
        ratingPicker.SelectedIndex = 0;
        typePicker.SelectedIndex = 0;
        devicePicker.SelectedIndex = 0;
        ingredientPicker.SelectedIndex = 0;
        searchRecipesEntry.Text = string.Empty;
        List<Recipe> recipes = await DatabaseService.GetRecipes(loggedInUser.UserId, null, null);
        currentListOfRecipesAndRatings = await DatabaseService.GetRecipesAndRatings(loggedInUser.UserId, recipes);
        await refreshRecipesTable();
    }

    private async void searchButton_Clicked(object sender, EventArgs e)
    {
        List<Recipe> recipes = await DatabaseService.SearchRecipes(loggedInUser.UserId, searchRecipesEntry.Text);
        currentListOfRecipesAndRatings = await DatabaseService.GetRecipesAndRatings(loggedInUser.UserId, recipes);
        await refreshRecipesTable();
    }

    private async void viewRecipeButton_Clicked(Recipe recipe)
    {
        await Navigation.PushAsync(new ViewRecipePage(loggedInUser, recipe));
    }
    #endregion

    #region Methods
    private async Task refreshRecipesTable()
    {
        if (currentListOfRecipesAndRatings.Count == 0)
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
            foreach (var (recipe, rating) in currentListOfRecipesAndRatings)
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
                    Padding = 5,
                    Margin = 5,
                    Command = new Command(() => viewRecipeButton_Clicked(recipe)),
                };
                Border borderedNameButton = new Border
                {
                    StrokeThickness = 1,
                    Content = recipeNameButton
                };

                string displayOverallRating;
                if (rating != -1)
                {
                    displayOverallRating = rating % 1 == 0 ? rating.ToString("0") : rating.ToString("n2");
                } else
                {
                    displayOverallRating = "None";
                }
                Border borderedRecipeRating = new Border
                {
                    StrokeThickness = 1,
                    BackgroundColor = Colors.Transparent,
                    Padding = 5,
                    Margin = 0,
                    Content = new Label
                    {
                        Text = displayOverallRating,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        FontSize = 14
                    }
                };

                Border borderedRecipeDevice = new Border
                {
                    StrokeThickness = 1,
                    BackgroundColor = Colors.Transparent,
                    Padding = 5,
                    Margin = 0,
                    Content = new Label
                    {
                        Text = recipe.CookingDevice,
                        HorizontalOptions = LayoutOptions.Start,
                        VerticalOptions = LayoutOptions.Center,
                        FontSize = 14
                    }
                };

                Border borderedRecipeType = new Border
                {
                    StrokeThickness = 1,
                    BackgroundColor = Colors.Transparent,
                    Padding = 5,
                    Margin = 0,
                    Content = new Label
                    {
                        Text = recipe.RecipeType,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        FontSize = 14
                    }
                };

                recipesGridContent.Children.Add(borderedNameButton);
                Grid.SetRow(borderedNameButton, rowNum);
                Grid.SetColumn(borderedNameButton, 0);

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

    private async void filterByRating(int minNum, int maxNum)
    {
        Dictionary<Recipe, float> filteredRecipes = new Dictionary<Recipe, float>();
        if(minNum == -1 && maxNum == -1)
        {
            filteredRecipes = currentListOfRecipesAndRatings.Where(i => i.Value == -1).ToDictionary(i => i.Key, i => i.Value);
        } else
        {
            filteredRecipes = currentListOfRecipesAndRatings.Where(i => i.Value >= minNum && i.Value < maxNum).ToDictionary(i => i.Key, i => i.Value);
        }

        currentListOfRecipesAndRatings = filteredRecipes;
        await refreshRecipesTable();
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
                List<Recipe> recipes = await DatabaseService.GetRecipes(loggedInUser.UserId, "Type", selectedItem);
                currentListOfRecipesAndRatings = await DatabaseService.GetRecipesAndRatings(loggedInUser.UserId, recipes);
                await refreshRecipesTable();
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
                List<Recipe> recipes = await DatabaseService.GetRecipes(loggedInUser.UserId, "Ingredient", selectedItem);
                currentListOfRecipesAndRatings = await DatabaseService.GetRecipesAndRatings(loggedInUser.UserId, recipes);
                await refreshRecipesTable();
            }
        }
    }

    private void ratingPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        int ratingFilter = ratingPicker.SelectedIndex;
        switch (ratingFilter)
        {
            case 0:
                break;
            case 1:
                filterByRating(-1, -1); break;
            case 2:
                filterByRating(0, 1); break;
            case 3:
                filterByRating(1, 2); break;
            case 4:
                filterByRating(2, 3); break;
            case 5:
                filterByRating(3, 4); break;
            case 6:
                filterByRating(4, 5); break;
        }
    }

    private async void devicePicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        if(devicePicker.SelectedIndex != 0)
        {
            if (devicePicker.SelectedItem is string selectedItem)
            {
                typePicker.SelectedIndex = 0;
                ingredientPicker.SelectedIndex = 0;
                List<Recipe> recipes = await DatabaseService.GetRecipes(loggedInUser.UserId, "Device", selectedItem);
                currentListOfRecipesAndRatings = await DatabaseService.GetRecipesAndRatings(loggedInUser.UserId, recipes);
                await refreshRecipesTable();
            }
        }
    }
}