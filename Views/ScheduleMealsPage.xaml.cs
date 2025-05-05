using PlanToPlate.Models;
using PlanToPlate.Services;
using System.Globalization;
using System.Linq.Expressions;

namespace PlanToPlate.Views;

public partial class ScheduleMealsPage : ContentPage
{
    public User loggedInUser { get; set; }
    public ScheduleMealsPage(User user)
	{
		InitializeComponent();
        loggedInUser = user;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        typePicker.Items.Add("Type");
        typePicker.Items.Add("Breakfast");
        typePicker.Items.Add("Lunch");
        typePicker.Items.Add("Dinner");
        typePicker.SelectedIndex = 0;
        populateRecipePicker();
        recipePicker.SelectedIndex = 0;
    }

    #region Clicked Events
    private async void scheduleRecipeButton_Clicked(object sender, EventArgs e)
    {
        if (validInputs())
        {
            int recipeId = await DatabaseService.GetRecipeId(recipePicker.SelectedItem.ToString(), loggedInUser.UserId);
            if (recipeId == -1)
            {
                await DisplayAlert("Error", "Recipe not found. Please try again.", "OK");
                return;
            }
            else
            {
                await DatabaseService.ScheduleMeal(new ScheduledMeals
                {
                    UserId = loggedInUser.UserId,
                    Date = scheduleMealDatePicker.Date,
                    MealType = typePicker.SelectedItem.ToString(),
                    RecipeId = recipeId
                });
                typePicker.SelectedIndex = 0;
                recipePicker.SelectedIndex = 0;
                await DisplayAlert("Meal Scheduled", $"{typePicker.SelectedItem.ToString()} has been scheduled on {scheduleMealDatePicker.Date.ToString("MM/dd/yyyy")}", "OK");
            }
        }
    }

    private async void showDatesButton_Clicked(object sender, EventArgs e)
    {
        scheduledMealsGrid.Children.Clear();
        scheduledMealsGrid.ColumnDefinitions.Clear();
        scheduledMealsGrid.RowDefinitions.Clear();
        List<ScheduledMeals> scheduledMeals = await DatabaseService.GetScheduledMeals(loggedInUser.UserId, startDatePicker.Date, endDatePicker.Date);
        if (scheduledMeals.Count == 0)
        {
            await DisplayAlert("Error", "No scheduled meals found for the selected dates.", "OK");
        }
        else
        {
            int numOfDays = endDatePicker.Date.Subtract(startDatePicker.Date).Days;
            if(numOfDays >= 7)
            {
                for(int i = 0; i < 7; i++)
                {
                    scheduledMealsGrid.ColumnDefinitions.Add(new ColumnDefinition());
                }
            } else
            {
                for(int i = 0; i < numOfDays; i++)
                {
                    scheduledMealsGrid.ColumnDefinitions.Add(new ColumnDefinition());
                }
            }
            var secondaryDarkColor = (Color)Application.Current.Resources["SecondaryDark"];
            int columnNum = 0;
            int rowNum = 0;
            scheduledMealsGrid.RowDefinitions.Add(new RowDefinition());
            scheduledMealsGrid.RowDefinitions.Add(new RowDefinition());
            scheduledMealsGrid.RowDefinitions.Add(new RowDefinition());
            scheduledMealsGrid.RowDefinitions.Add(new RowDefinition());
            for (int i = 0; i < numOfDays; i++)
            {
                if (columnNum == 7)
                {
                    columnNum = 0;
                    scheduledMealsGrid.RowDefinitions.Add(new RowDefinition());
                    scheduledMealsGrid.RowDefinitions.Add(new RowDefinition());
                    scheduledMealsGrid.RowDefinitions.Add(new RowDefinition());
                    scheduledMealsGrid.RowDefinitions.Add(new RowDefinition());
                    rowNum = rowNum + 4;
                }

                Label dateLabel = new Label
                {
                    Text = startDatePicker.Date.AddDays(i).ToString("MM/dd"),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Padding = new Thickness(0, 10),
                    FontSize = 14
                };
                Border borderedDateLabel = new Border
                {
                    StrokeThickness = 1,
                    BackgroundColor = secondaryDarkColor,
                    Content = dateLabel,
                    Padding = 5,
                    Margin = new Thickness(0, 15, 0, 5)
                };


                Label breakfastLabel = new Label
                {
                    Text = "Breakfast",
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = 10
                };
                Label lunchLabel = new Label
                {
                    Text = "Lunch",
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = 10,
                    Margin = new Thickness(0, 5)
                };
                Label dinnerLabel = new Label
                {
                    Text = "Dinner",
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = 10
                };

                scheduledMealsGrid.Children.Add(borderedDateLabel);
                scheduledMealsGrid.SetRow(borderedDateLabel, rowNum);
                scheduledMealsGrid.SetColumn(borderedDateLabel, columnNum);

                scheduledMealsGrid.Children.Add(breakfastLabel);
                scheduledMealsGrid.SetRow(breakfastLabel, rowNum + 1);
                scheduledMealsGrid.SetColumn(breakfastLabel, columnNum);

                scheduledMealsGrid.Children.Add(lunchLabel);
                scheduledMealsGrid.SetRow(lunchLabel, rowNum + 2);
                scheduledMealsGrid.SetColumn(lunchLabel, columnNum);

                scheduledMealsGrid.Children.Add(dinnerLabel);
                scheduledMealsGrid.SetRow(dinnerLabel, rowNum + 3);
                scheduledMealsGrid.SetColumn(dinnerLabel, columnNum);

                columnNum++;
            }
        }
    }
    #endregion

    #region Methods
    private async void populateRecipePicker()
    {
        recipePicker.Items.Clear();
        recipePicker.Items.Add("Recipe");
        List<Recipe> recipes = await DatabaseService.GetAllRecipes(loggedInUser.UserId);
        foreach (Recipe recipe in recipes)
        {
            string recipeName = recipe.RecipeName;
            if (!recipePicker.Items.Contains(recipeName))
            {
                recipePicker.Items.Add(recipeName);
            }
        }
    }

    private bool validInputs()
    {
        if (typePicker.SelectedIndex == 0)
        {
            DisplayAlert("Error", "Please select a meal type.", "OK");
            return false;
        }
        if (recipePicker.SelectedIndex == 0)
        {
            DisplayAlert("Error", "Please select a recipe.", "OK");
            return false;
        }
        return true;
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