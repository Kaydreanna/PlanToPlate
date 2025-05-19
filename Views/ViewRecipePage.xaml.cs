using CommunityToolkit.Maui.Views;
using PlanToPlate.Models;
using PlanToPlate.Services;
using System;
using System.Threading.Tasks;

namespace PlanToPlate.Views;

public partial class ViewRecipePage : ContentPage
{
    public User loggedInUser { get; set; }
    public Recipe selectedRecipe { get; set; }
    public ViewRecipePage(User user, Recipe recipe)
	{
		InitializeComponent();
        loggedInUser = user;
        selectedRecipe = recipe;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        recipeNameLabel.Text = selectedRecipe.RecipeName;
        typeLabel.Text = selectedRecipe.RecipeType;
        deviceLabel.Text = selectedRecipe.CookingDevice;
        displayIngredients();
    }

    private async void closeRecipeButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void displayIngredients()
    {
        int ingredientRowNum = 1;
        foreach (var ingredient in selectedRecipe.Ingredients)
        {
            ingredientsGrid.RowDefinitions.Add(new RowDefinition());
            Label ingredientLabel = new Label
            {
                Text = $"{ingredient.Value.Quantity} {ingredient.Value.Unit} {ingredient.Key}",
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                FontSize = 16
            };
            ingredientsGrid.Children.Add(ingredientLabel);
            ingredientsGrid.SetRow(ingredientLabel, ingredientRowNum);
            ingredientsGrid.SetColumn(ingredientLabel, 1);
            ingredientRowNum++;
        }
        int instructionRowNum = 1;
        foreach (var instruction in selectedRecipe.Instructions)
        {
            instructionsGrid.RowDefinitions.Add(new RowDefinition());
            Label instructionNumLabel = new Label
            {
                Text = $"{instruction.Key}.",
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Start,
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(0, 15, 15, 0)
            };
            Label instructionsLabel = new Label
            {
                Text = $"{instruction.Value}",
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                FontSize = 16,
                Margin = new Thickness(0, 15, 15, 0)
            };

            instructionsGrid.Children.Add(instructionNumLabel);
            instructionsGrid.SetRow(instructionNumLabel, instructionRowNum);
            instructionsGrid.SetColumn(instructionNumLabel, 1);

            instructionsGrid.Children.Add(instructionsLabel);
            instructionsGrid.SetRow(instructionsLabel, instructionRowNum);
            instructionsGrid.SetColumn(instructionsLabel, 2);

            instructionRowNum++;
        }
    }

    private async void editButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new AddRecipePage(loggedInUser, selectedRecipe));
    }

    private async void deleteButton_Clicked(object sender, EventArgs e)
    {
        bool confirmDelete = await DisplayAlert("Delete Recipe", "Are you sure you want to delete this recipe?", "Yes", "No");
        if(confirmDelete)
        {
            await DatabaseService.DeleteRecipe(selectedRecipe.RecipeId);
            await Navigation.PopModalAsync();
        }
    }

    private async void scheduleMealButton_Clicked(object sender, EventArgs e)
    {
        await displayScheduleMealPopup();
    }

    public async Task displayScheduleMealPopup()
    {
        var pickerPopup = new DatePickerPopup();
        var result = await this.ShowPopupAsync(pickerPopup);

        if(result is ValueTuple<DateTime, string> selection)
        {
            DateTime selectedDate = selection.Item1;
            string selectedMealType = selection.Item2;
            ScheduledMeals newMeal = new ScheduledMeals
            {
                UserId = loggedInUser.UserId,
                RecipeId = selectedRecipe.RecipeId,
                Date = selectedDate,
                MealType = selectedMealType
            };
            await DatabaseService.ScheduleMeal(newMeal);
        }
    }
}