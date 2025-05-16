using CommunityToolkit.Maui.Core;
using Microsoft.Maui.Platform;
using PlanToPlate.Models;
using PlanToPlate.Services;
using System.Globalization;
using System.Linq.Expressions;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace PlanToPlate.Views;

public partial class ScheduleMealsPage : ContentPage
{
    public User loggedInUser { get; set; }
    private DateTime displayedMonth { get; set; }
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
        displayedMonth = DateTime.Today;
        displayScheduledMeals(displayedMonth);
    }

    #region Clicked Events
    private async void scheduleRecipeButton_Clicked(object sender, EventArgs e)
    {
        if (validInputs())
        {
            int recipeId = await DatabaseService.GetRecipeId(recipePicker.SelectedItem.ToString(), loggedInUser.UserId);
            ScheduledMeals conflictingMeal = await existingMeal();
            if (recipeId == -1)
            {
                await DisplayAlert("Error", "Recipe not found. Please try again.", "OK");
                return;
            }
            else
            {
                if (conflictingMeal != null)
                {
                    bool replaceMeal = await DisplayAlert("Existing Meal", $"A {typePicker.SelectedItem.ToString()} recipe is already scheduled for this day. Would you like to replace it?", "Yes", "No");
                    if (replaceMeal)
                    {
                        await DatabaseService.DeleteScheduleMeal(conflictingMeal);
                    }
                    else
                    {
                        return;
                    }
                }
                await DatabaseService.ScheduleMeal(new ScheduledMeals
                {
                    UserId = loggedInUser.UserId,
                    Date = scheduleMealDatePicker.Date,
                    MealType = typePicker.SelectedItem.ToString(),
                    RecipeId = recipeId
                });
                await CommunityToolkit.Maui.Alerts.Snackbar.Make($"{recipePicker.SelectedItem.ToString()} has been scheduled on {scheduleMealDatePicker.Date.ToString("MM/dd/yyyy")}", duration: TimeSpan.FromSeconds(2), visualOptions: new SnackbarOptions {BackgroundColor = Colors.Black, TextColor = Colors.White}).Show();
                await DatabaseService.UpdateShoppingLists(loggedInUser.UserId, scheduleMealDatePicker.Date);
                typePicker.SelectedIndex = 0;
                recipePicker.SelectedIndex = 0;
                displayScheduledMeals(displayedMonth);
            }
        }
    }

    private void nextMonthImageButton_Clicked(object sender, EventArgs e)
    {
        displayedMonth = displayedMonth.AddMonths(1);
        displayScheduledMeals(displayedMonth);
    }

    private void previousMonthImageButton_Clicked(object sender, EventArgs e)
    {
        displayedMonth = displayedMonth.AddMonths(-1);
        displayScheduledMeals(displayedMonth);
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

    private async void displayScheduledMeals(DateTime selectedMonth)
    {
        scheduledMealsGrid.Children.Clear();
        scheduledMealsGrid.RowDefinitions.Clear();
        var secondaryColor = (Color)Application.Current.Resources["Secondary"];
        var secondaryDarkColor = (Color)Application.Current.Resources["SecondaryDark"];
        var tertiaryColor = (Color)Application.Current.Resources["Tertiary"];
        DateTime startOfMonth = new DateTime(selectedMonth.Year, selectedMonth.Month, 1);
        DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
        List<ScheduledMeals> scheduledMeals = await DatabaseService.GetScheduledMeals(loggedInUser.UserId, startOfMonth, endOfMonth);

        scheduledMealsGrid.RowDefinitions.Add(new RowDefinition());
        scheduledMealsGrid.RowDefinitions.Add(new RowDefinition());
        scheduledMealsGrid.RowDefinitions.Add(new RowDefinition());
        scheduledMealsGrid.RowDefinitions.Add(new RowDefinition());
        int columnNum = 0;
        int rowNum = 0;

        monthAndYearLabel.Text = selectedMonth.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
        DateTime firstDayOfMonth = new DateTime(selectedMonth.Year, selectedMonth.Month, 1);
        DayOfWeek dayOfWeek = firstDayOfMonth.DayOfWeek;
        DateTime displayDate = firstDayOfMonth;
        switch(dayOfWeek)
        {
            case DayOfWeek.Sunday:
                columnNum = 0; break;
            case DayOfWeek.Monday:
                columnNum = 1; break;
            case DayOfWeek.Tuesday:
                columnNum = 2; break;
            case DayOfWeek.Wednesday:
                columnNum = 3; break;
            case DayOfWeek.Thursday:
                columnNum = 4; break;
            case DayOfWeek.Friday:
                columnNum = 5; break;
            case DayOfWeek.Saturday:
                columnNum = 6; break;
        }

        int daysInMonth = DateTime.DaysInMonth(selectedMonth.Year, selectedMonth.Month);
        for (int i = 0; i < daysInMonth; i++)
        {
            if(columnNum == 7)
            {
                columnNum = 0;
                scheduledMealsGrid.RowDefinitions.Add(new RowDefinition());
                scheduledMealsGrid.RowDefinitions.Add(new RowDefinition());
                scheduledMealsGrid.RowDefinitions.Add(new RowDefinition());
                scheduledMealsGrid.RowDefinitions.Add(new RowDefinition());
                rowNum = rowNum + 4;
            }

            Border borderedDateLabel = new Border
            {
                StrokeThickness = 1,
                BackgroundColor = secondaryDarkColor,
                Padding = 5,
                Margin = new Thickness(0, 5, 0, 0),
                Content = new Label
                {
                    Text = displayDate.ToString("MM/dd"),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Padding = new Thickness(0, 10),
                    FontSize = 14
                }
            };

            List<ScheduledMeals> mealsOnDate = scheduledMeals.FindAll(m => m.Date.Date == displayDate.Date);
            ScheduledMeals breakfast = mealsOnDate.Find(m => m.MealType == "Breakfast");
            string breakfastName = breakfast != null ? await DatabaseService.GetRecipeName(breakfast) : "-";
            ScheduledMeals lunch = mealsOnDate.Find(m => m.MealType == "Lunch");
            string lunchName = lunch != null ? await DatabaseService.GetRecipeName(lunch) : "-";
            ScheduledMeals dinner = mealsOnDate.Find(m => m.MealType == "Dinner");
            string dinnerName = dinner != null ? await DatabaseService.GetRecipeName(dinner) : "-";

            Button breakfastButton = new Button
            {
                Text = breakfastName,
                LineBreakMode = LineBreakMode.WordWrap,
                FontSize = 10,
                TextColor = breakfast != null ? tertiaryColor : Colors.Black,
                CornerRadius = 0,
                BorderWidth = 1,
                BorderColor = Colors.Black,
                BackgroundColor = secondaryColor,
                Padding = 0,
                Margin = 0,
            };
            breakfastButton.BindingContext = displayDate;
            breakfastButton.Clicked += async (sender, e) =>
            {
                if (breakfast != null)
                {
                    bool deleteMeal = await DisplayAlert("Delete Meal?", $"Would you like to delete {breakfastName} from {breakfast.Date.ToString("MM/dd/yyyy")}?", "Yes", "No");
                    if (deleteMeal)
                    {
                        await DatabaseService.DeleteScheduleMeal(breakfast);
                        await DatabaseService.UpdateShoppingLists(loggedInUser.UserId, breakfast.Date);
                        displayScheduledMeals(displayedMonth);
                    }
                }
                else
                {
                    DateTime buttonsDate = (DateTime)breakfastButton.BindingContext;
                    bool addMeal = await DisplayAlert("Add Meal?", $"Would you like to add a breakfast to {buttonsDate.ToString("MM/dd/yyyy")}?", "Yes", "No");
                    if (addMeal)
                    {
                        await mainContent.ScrollToAsync(0, 0, true);
                        scheduleMealDatePicker.Date = buttonsDate;
                        typePicker.SelectedItem = "Breakfast";
                    }
                }
            };

            Button lunchButton = new Button
            {
                Text = lunchName,
                LineBreakMode = LineBreakMode.WordWrap,
                FontSize = 10,
                TextColor = breakfast != null ? tertiaryColor : Colors.Black,
                CornerRadius = 0,
                BorderWidth = 1,
                BorderColor = Colors.Black,
                BackgroundColor = secondaryColor,
                Padding = 0,
                Margin = 0,
            };
            lunchButton.BindingContext = displayDate;
            lunchButton.Clicked += async (sender, e) =>
            {
                if (lunch != null)
                {
                    bool deleteMeal = await DisplayAlert("Delete Meal?", $"Would you like to delete {lunchName} from {lunch.Date.ToString("MM/dd/yyyy")}?", "Yes", "No");
                    if (deleteMeal)
                    {
                        await DatabaseService.DeleteScheduleMeal(lunch);
                        await DatabaseService.UpdateShoppingLists(loggedInUser.UserId, lunch.Date);
                        displayScheduledMeals(displayedMonth);
                    }
                }
                else
                {
                    DateTime buttonsDate = (DateTime)lunchButton.BindingContext;
                    bool addMeal = await DisplayAlert("Add Meal?", $"Would you like to add a lunch to {buttonsDate.ToString("MM/dd/yyyy")}?", "Yes", "No");
                    if (addMeal)
                    {
                        await mainContent.ScrollToAsync(0, 0, true);
                        scheduleMealDatePicker.Date = buttonsDate;
                        typePicker.SelectedItem = "Lunch";
                    }
                }
            };

            Button dinnerButton = new Button
            {
                Text = dinnerName,
                LineBreakMode = LineBreakMode.WordWrap,
                FontSize = 10,
                TextColor = dinner != null ? tertiaryColor : Colors.Black,
                CornerRadius = 0,
                BorderWidth = 1,
                BorderColor = Colors.Black,
                BackgroundColor = secondaryColor,
                Padding = 0,
                Margin = 0,
            };
            dinnerButton.BindingContext = displayDate;
            dinnerButton.Clicked += async (sender, e) =>
            {
                if (dinner != null)
                {
                    bool deleteMeal = await DisplayAlert("Delete Meal?", $"Would you like to delete {dinnerName} from {dinner.Date.ToString("MM/dd/yyyy")}?", "Yes", "No");
                    if (deleteMeal)
                    {
                        await DatabaseService.DeleteScheduleMeal(dinner);
                        await DatabaseService.UpdateShoppingLists(loggedInUser.UserId, dinner.Date);
                        displayScheduledMeals(displayedMonth);
                    }
                }
                else
                {
                    DateTime buttonsDate = (DateTime)dinnerButton.BindingContext;
                    bool addMeal = await DisplayAlert("Add Meal?", $"Would you like to add a dinner to {buttonsDate.ToString("MM/dd/yyyy")}?", "Yes", "No");
                    if (addMeal)
                    {
                        await mainContent.ScrollToAsync(0, 0, true);
                        scheduleMealDatePicker.Date = buttonsDate;
                        typePicker.SelectedItem = "Dinner";
                    }
                }
            };

            scheduledMealsGrid.Children.Add(borderedDateLabel);
            scheduledMealsGrid.SetRow(borderedDateLabel, rowNum);
            scheduledMealsGrid.SetColumn(borderedDateLabel, columnNum);

            scheduledMealsGrid.Children.Add(breakfastButton);
            scheduledMealsGrid.SetRow(breakfastButton, rowNum + 1);

            scheduledMealsGrid.SetColumn(breakfastButton, columnNum);

            scheduledMealsGrid.Children.Add(lunchButton);
            scheduledMealsGrid.SetRow(lunchButton, rowNum + 2);
            scheduledMealsGrid.SetColumn(lunchButton, columnNum);

            scheduledMealsGrid.Children.Add(dinnerButton);
            scheduledMealsGrid.SetRow(dinnerButton, rowNum + 3);
            scheduledMealsGrid.SetColumn(dinnerButton, columnNum);

            columnNum++;
            displayDate = displayDate.AddDays(1);
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

    private async Task<ScheduledMeals> existingMeal()
    {
        List<ScheduledMeals> scheduledMeals = await DatabaseService.GetScheduledMeals(loggedInUser.UserId, scheduleMealDatePicker.Date);
        if (scheduledMeals.Count > 0)
        {
            var existingMeal = scheduledMeals.FirstOrDefault(m => m.MealType == typePicker.SelectedItem.ToString());
            if (existingMeal != null)
            {
                return existingMeal;
            }
        }
        return null;
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