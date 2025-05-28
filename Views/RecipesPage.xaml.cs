using CommunityToolkit.Maui.Views;
using PlanToPlate.Models;
using PlanToPlate.Services;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;

namespace PlanToPlate.Views;

public partial class RecipesPage : ContentPage
{
    public User loggedInUser { get; set; }
    private Dictionary<Recipe, float> currentListOfRecipesAndRatings = new Dictionary<Recipe, float>();
    private string currentRatingFilter = null;
    private string currentDeviceFilter = null;
    private string currentMealTypeFilter = null;
    private string currentIngredientFilter = null;
    public RecipesPage(User user)
	{
		InitializeComponent();
        loggedInUser = user;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        List<Recipe> recipes = await DatabaseService.GetAllRecipes(loggedInUser.UserId);
        currentListOfRecipesAndRatings = await DatabaseService.GetRecipesAndRatings(recipes);
        sortByPicker.Items.Add("Name");
        sortByPicker.Items.Add("Rating");
        sortByPicker.Items.Add("Device");
        sortByPicker.Items.Add("Type");
        refreshRecipesTable();
    }

    #region Clicked Events
    private async void addRecipeButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddRecipePage(loggedInUser, null));
    }

    private async void clearButton_Clicked(object sender, EventArgs e)
    {
        searchRecipesEntry.Text = string.Empty;
        List<Recipe> recipes = await DatabaseService.GetAllRecipes(loggedInUser.UserId);
        currentListOfRecipesAndRatings = await DatabaseService.GetRecipesAndRatings(loggedInUser.UserId);
        refreshRecipesTable();
    }

    private void searchButton_Clicked(object sender, EventArgs e)
    {
        string searchTerm = searchRecipesEntry.Text.Trim().ToLower();
        if(!string.IsNullOrWhiteSpace(searchTerm))
        {
            Dictionary<Recipe, float> recipeResults = new Dictionary<Recipe, float>();
            foreach (var (recipe, rating) in currentListOfRecipesAndRatings)
            {
                if (recipe.RecipeName.ToLower().Contains(searchTerm))
                {
                    recipeResults.Add(recipe, rating);
                }
                else if (recipe.CookingDevice.ToLower().Contains(searchTerm))
                {
                    recipeResults.Add(recipe, rating);
                }
                else if (recipe.RecipeType.ToLower().Contains(searchTerm))
                {
                    recipeResults.Add(recipe, rating);
                }
                else if (recipe.Ingredients != null)
                {
                    foreach (var ingredient in recipe.Ingredients)
                    {
                        if (ingredient.Key.ToString().ToLower().Contains(searchTerm))
                        {
                            recipeResults.Add(recipe, rating);
                            break;
                        }
                    }
                }
            }
            currentListOfRecipesAndRatings = recipeResults;
            refreshRecipesTable();
        }
    }

    private async void viewRecipeButton_Clicked(Recipe recipe)
    {
        await Navigation.PushAsync(new ViewRecipePage(loggedInUser, recipe));
    }

    private async void filterByButton_Clicked(object sender, EventArgs e)
    {
        List<string> devices = new List<string>();
        List<Recipe> recipes = await DatabaseService.GetAllRecipes(loggedInUser.UserId);
        foreach (Recipe recipe in recipes)
        {
            if (!devices.Contains(recipe.CookingDevice))
            {
                devices.Add(recipe.CookingDevice);
            }
        }

        List<string> capitalizedIngredients = new List<string>();
        List<Ingredient> ingredients = await DatabaseService.GetIngredients(loggedInUser.UserId);
        foreach (Ingredient ingredient in ingredients)
        {
            string ingredientName = ingredient.IngredientName.ToLower();
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            string capitalizedIngredientName = textInfo.ToTitleCase(ingredientName);
            if (!capitalizedIngredients.Contains(capitalizedIngredientName))
            {
                capitalizedIngredients.Add(capitalizedIngredientName);
            }
        }

        var filterPopup = new FilterRecipesPopup(devices, capitalizedIngredients, currentRatingFilter, currentDeviceFilter, currentMealTypeFilter, currentIngredientFilter);
        var result = await this.ShowPopupAsync(filterPopup);
        if(result is ValueTuple<string, string, string, string> filterByInfo)
        {
            string selectedRating = filterByInfo.Item1;
            string selectedDevice = filterByInfo.Item2;
            string selectedMealType = filterByInfo.Item3;
            string selectedIngredient = filterByInfo.Item4;
            if (selectedRating != null)
            {
                currentRatingFilter = selectedRating;
                switch (selectedRating)
                {
                    case "None":
                        filterByRating(-1, -1); break;
                    case "0-1":
                        filterByRating(0, 1); break;
                    case "1-2":
                        filterByRating(1, 2); break;
                    case "2-3":
                        filterByRating(2, 3); break;
                    case "3-4":
                        filterByRating(3, 4); break;
                    case "4-5":
                        filterByRating(4, 5); break;
                }
            } else { currentRatingFilter = null; }

            if (selectedDevice != null)
            {
                currentDeviceFilter = selectedDevice;
                currentListOfRecipesAndRatings = currentListOfRecipesAndRatings.Where(i => i.Key.CookingDevice == selectedDevice).ToDictionary(i => i.Key, i => i.Value);
            } else { currentDeviceFilter = null; }

            if (selectedMealType != null)
            {
                currentMealTypeFilter = selectedMealType;
                currentListOfRecipesAndRatings = currentListOfRecipesAndRatings.Where(i => i.Key.RecipeType == selectedMealType).ToDictionary(i => i.Key, i => i.Value);
            } else { currentMealTypeFilter = null; }

            if (selectedIngredient != null)
            {
                currentIngredientFilter = selectedIngredient;
                Dictionary<Recipe, float> recipesToAdd = new Dictionary<Recipe, float>();
                foreach (var (recipe, rating) in currentListOfRecipesAndRatings)
                {
                    foreach (var ingredient in recipe.Ingredients)
                    {
                        if (ingredient.Key.ToString().ToLower() == selectedIngredient.ToLower())
                        {
                            recipesToAdd.Add(recipe, rating);
                            break;
                        }
                    }
                }
                currentListOfRecipesAndRatings = recipesToAdd;
            } else { currentIngredientFilter = null; }

            refreshRecipesTable();
        }
    }
    private void sortByPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        switch (sortByPicker.SelectedIndex)
        {
            case 0:
                currentListOfRecipesAndRatings = currentListOfRecipesAndRatings.OrderBy(i => i.Key.RecipeName).ToDictionary(i => i.Key, i => i.Value);
                refreshRecipesTable();
                break;
            case 1:
                currentListOfRecipesAndRatings = currentListOfRecipesAndRatings.OrderByDescending(i => i.Value).ToDictionary(i => i.Key, i => i.Value);
                refreshRecipesTable();
                break;
            case 2:
                currentListOfRecipesAndRatings = currentListOfRecipesAndRatings.OrderBy(i => i.Key.CookingDevice).ToDictionary(i => i.Key, i => i.Value);
                refreshRecipesTable();
                break;
            case 3:
                List<string> typeOrder = new List<string> { "Breakfast", "Lunch", "Dinner" };
                currentListOfRecipesAndRatings = currentListOfRecipesAndRatings.OrderBy(i => typeOrder.IndexOf(i.Key.RecipeType)).ToDictionary(i => i.Key, i => i.Value);
                refreshRecipesTable();
                break;
        }
    }
    #endregion

    #region Methods
    private void refreshRecipesTable()
    {
        if (currentListOfRecipesAndRatings.Count == 0)
        {
            noRecipesFoundMessage.IsVisible = true;
            recipesGridLabels.IsVisible = false;
            recipesGridContent.Children.Clear();
            recipesGridContent.IsVisible = false;
        }
        else
        {
            noRecipesFoundMessage.IsVisible = false;
            recipesGridLabels.IsVisible = true;
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

    private void filterByRating(int minNum, int maxNum)
    {
        if(minNum == -1 && maxNum == -1)
        {
            currentListOfRecipesAndRatings = currentListOfRecipesAndRatings.Where(i => i.Value == -1).ToDictionary(i => i.Key, i => i.Value);
        } else
        {
            currentListOfRecipesAndRatings = currentListOfRecipesAndRatings.Where(i => i.Value >= minNum && i.Value < maxNum).ToDictionary(i => i.Key, i => i.Value);
        }

        refreshRecipesTable();
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