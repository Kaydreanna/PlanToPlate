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
            Label dateLabel = new Label
            {
                Text = displayDate.ToString("MM/dd"),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Padding = new Thickness(0, 10),
                FontSize = 14
            };
            Border borderedDateLabel = new Border
            {
                StrokeThickness = 1,
                BackgroundColor = secondaryColor,
                Content = dateLabel,
                Padding = 5,
                Margin = new Thickness(0, 15, 0, 5)
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
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                FontSize = 10,
                BackgroundColor = Colors.Transparent,
                TextColor = tertiaryColor,
                Padding = 0,
                Margin = 0,
                Command = new Command(async () =>
                {
                    if (breakfast != null)
                    {
                        await DisplayAlert("Scheduled Meal", $"{breakfastName} is scheduled on {breakfast.Date.ToString("MM/dd/yyyy")}", "OK");
                    }
                })
            };
            Button lunchLabel = new Button
            {
                Text = lunchName,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                FontSize = 10,
                BackgroundColor = Colors.Transparent,
                TextColor = tertiaryColor,
                Padding = 0,
                Margin = new Thickness(0, 5),
                Command = new Command(async () =>
                {
                    if (lunch != null)
                    {
                        await DisplayAlert("Scheduled Meal", $"{lunchName} is scheduled on {lunch.Date.ToString("MM/dd/yyyy")}", "OK");
                    }
                })
            };
            Button dinnerLabel = new Button
            {
                Text = dinnerName,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                FontSize = 10,
                BackgroundColor = Colors.Transparent,
                TextColor = tertiaryColor,
                Padding = 0,
                Margin = 0,
                Command = new Command(async () =>
                {
                    if (dinner != null)
                    {
                        await DisplayAlert("Scheduled Meal", $"{dinnerName} is scheduled on {dinner.Date.ToString("MM/dd/yyyy")}", "OK");
                    }
                })
            };
            scheduledMealsGrid.Children.Add(borderedDateLabel);
            scheduledMealsGrid.SetRow(borderedDateLabel, rowNum);
            scheduledMealsGrid.SetColumn(borderedDateLabel, columnNum);

            scheduledMealsGrid.Children.Add(breakfastButton);
            scheduledMealsGrid.SetRow(breakfastButton, rowNum + 1);

            scheduledMealsGrid.SetColumn(breakfastButton, columnNum);

            scheduledMealsGrid.Children.Add(lunchLabel);
            scheduledMealsGrid.SetRow(lunchLabel, rowNum + 2);
            scheduledMealsGrid.SetColumn(lunchLabel, columnNum);

            scheduledMealsGrid.Children.Add(dinnerLabel);
            scheduledMealsGrid.SetRow(dinnerLabel, rowNum + 3);
            scheduledMealsGrid.SetColumn(dinnerLabel, columnNum);

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