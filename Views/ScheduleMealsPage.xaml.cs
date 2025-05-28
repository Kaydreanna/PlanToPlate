using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
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
        displayedMonth = DateTime.Today;
        displayScheduledMeals(displayedMonth);
    }

    #region Clicked Events
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
    private async void displayScheduledMeals(DateTime selectedMonth)
    {
        scheduledMealsGrid.Children.Clear();
        scheduledMealsGrid.RowDefinitions.Clear();

        var primaryColor = (Color)Application.Current.Resources["Primary"];
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
                var mealsOnDate = await DatabaseService.GetScheduledMeals(loggedInUser.UserId, (DateTime)breakfastButton.BindingContext);
                ScheduledMeals breakfast = mealsOnDate.Find(m => m.MealType == "Breakfast");
                var breakfastName = breakfast != null ? await DatabaseService.GetRecipeName(breakfast) : "-";
                if (breakfast != null)
                {
                    bool deleteMeal = await DisplayAlert("Delete Meal?", $"Would you like to delete {breakfastName} from {breakfast.Date.ToString("MM/dd/yyyy")}?", "Yes", "No");
                    if (deleteMeal)
                    {
                        await DatabaseService.DeleteScheduleMeal(breakfast);
                        await DatabaseService.UpdateShoppingLists(loggedInUser.UserId, breakfast.Date);
                        updateMealButton((DateTime)((Button)sender).BindingContext, "Breakfast");
                    }
                }
                else
                {
                    string recipeToShcedule = await displayScheduleMealPopup();
                    if(string.IsNullOrEmpty(recipeToShcedule))
                    {
                        await DisplayAlert("Error", "Unable to schedule a meal on selected date.", "OK");
                    } else
                    {
                        int recipeId = await DatabaseService.GetRecipeId(loggedInUser.UserId, recipeToShcedule);
                        ScheduledMeals scheduledMeal = new ScheduledMeals
                        {
                            UserId = loggedInUser.UserId,
                            Date = (DateTime)breakfastButton.BindingContext,
                            MealType = "Breakfast",
                            RecipeId = recipeId
                        };
                        await DatabaseService.ScheduleMeal(scheduledMeal);
                        await DatabaseService.UpdateShoppingLists(loggedInUser.UserId, (DateTime)breakfastButton.BindingContext);
                        updateMealButton((DateTime)((Button)sender).BindingContext, "Breakfast");
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
                var mealsOnDate = await DatabaseService.GetScheduledMeals(loggedInUser.UserId, (DateTime)lunchButton.BindingContext);
                ScheduledMeals lunch = mealsOnDate.Find(m => m.MealType == "Lunch");
                var lunchName = lunch != null ? await DatabaseService.GetRecipeName(lunch) : "-";
                if (lunch != null)
                {
                    bool deleteMeal = await DisplayAlert("Delete Meal?", $"Would you like to delete {lunchName} from {lunch.Date.ToString("MM/dd/yyyy")}?", "Yes", "No");
                    if (deleteMeal)
                    {
                        await DatabaseService.DeleteScheduleMeal(lunch);
                        await DatabaseService.UpdateShoppingLists(loggedInUser.UserId, lunch.Date);
                        updateMealButton((DateTime)((Button)sender).BindingContext, "Lunch");
                    }
                }
                else
                {
                    string recipeToShcedule = await displayScheduleMealPopup();
                    if (string.IsNullOrEmpty(recipeToShcedule))
                    {
                        await DisplayAlert("Error", "Unable to schedule a meal on selected date.", "OK");
                    }
                    else
                    {

                        int recipeId = await DatabaseService.GetRecipeId(loggedInUser.UserId, recipeToShcedule);
                        ScheduledMeals scheduledMeal = new ScheduledMeals
                        {
                            UserId = loggedInUser.UserId,
                            Date = (DateTime)lunchButton.BindingContext,
                            MealType = "Lunch",
                            RecipeId = recipeId
                        };
                        await DatabaseService.ScheduleMeal(scheduledMeal);
                        await DatabaseService.UpdateShoppingLists(loggedInUser.UserId, (DateTime)lunchButton.BindingContext);
                        updateMealButton((DateTime)((Button)sender).BindingContext, "Lunch");
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
                var mealsOnDate = await DatabaseService.GetScheduledMeals(loggedInUser.UserId, (DateTime)lunchButton.BindingContext);
                ScheduledMeals dinner = mealsOnDate.Find(m => m.MealType == "Dinner");
                var dinnerName = dinner != null ? await DatabaseService.GetRecipeName(dinner) : "-";
                if (dinner != null)
                {
                    bool deleteMeal = await DisplayAlert("Delete Meal?", $"Would you like to delete {dinnerName} from {dinner.Date.ToString("MM/dd/yyyy")}?", "Yes", "No");
                    if (deleteMeal)
                    {
                        await DatabaseService.DeleteScheduleMeal(dinner);
                        await DatabaseService.UpdateShoppingLists(loggedInUser.UserId, dinner.Date);
                        updateMealButton((DateTime)((Button)sender).BindingContext, "Dinner");
                    }
                }
                else
                {
                    string recipeToShcedule = await displayScheduleMealPopup();
                    if (string.IsNullOrEmpty(recipeToShcedule))
                    {
                        await DisplayAlert("Error", "Unable to schedule a meal on selected date.", "OK");
                    }
                    else
                    {
                        int recipeId = await DatabaseService.GetRecipeId(loggedInUser.UserId, recipeToShcedule);
                        ScheduledMeals scheduledMeal = new ScheduledMeals
                        {
                            UserId = loggedInUser.UserId,
                            Date = (DateTime)dinnerButton.BindingContext,
                            MealType = "Dinner",
                            RecipeId = recipeId
                        };
                        await DatabaseService.ScheduleMeal(scheduledMeal);
                        await DatabaseService.UpdateShoppingLists(loggedInUser.UserId, (DateTime)dinnerButton.BindingContext);
                        updateMealButton((DateTime)((Button)sender).BindingContext, "Dinner");
                    }
                }
            };

            if (displayDate == DateTime.Today)
            {
                borderedDateLabel.BackgroundColor = primaryColor;
                breakfastButton.BackgroundColor = secondaryDarkColor;
                lunchButton.BackgroundColor = secondaryDarkColor;
                dinnerButton.BackgroundColor = secondaryDarkColor;
            }
            else
            {
                borderedDateLabel.BackgroundColor = secondaryDarkColor;
                breakfastButton.BackgroundColor = secondaryColor;
                lunchButton.BackgroundColor = secondaryColor;
                dinnerButton.BackgroundColor = secondaryColor;
            }

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

    public async Task<string> displayScheduleMealPopup()
    {
        List<Recipe> recipes = await DatabaseService.GetAllRecipes(loggedInUser.UserId);
        List<string> recipeNames = recipes.Select(r => r.RecipeName).ToList();
        var pickerPopup = new MealPickerPopup(recipeNames);
        string selectedItem = await this.ShowPopupAsync(pickerPopup) as string;
        return selectedItem;
    }

    private async void updateMealButton(DateTime dayToUpdate, string mealType)
    {
        List<ScheduledMeals> mealsOnDate = await DatabaseService.GetScheduledMeals(loggedInUser.UserId, dayToUpdate);
        ScheduledMeals meal = mealsOnDate.Find(m => m.MealType == mealType);
        string mealName = meal != null ? await DatabaseService.GetRecipeName(meal) : "-";

        int mealRowOffset = mealType switch
        {
            "Breakfast" => 1,
            "Lunch" => 2,
            "Dinner" => 3,
            _ => 0
        };

        var buttonToUpdate = scheduledMealsGrid.Children
            .Where(btn => btn is Button 
            && ((Button)btn).BindingContext is DateTime dt 
            && dt.Date == dayToUpdate.Date
            && Grid.GetRow((Button)btn) % 4 == mealRowOffset)
            .Cast<Button>()
            .FirstOrDefault();

        if (buttonToUpdate != null)
        {
            buttonToUpdate.Text = mealName;
            buttonToUpdate.TextColor = meal != null ? (Color)Application.Current.Resources["Tertiary"] : Colors.Black;
        }
        else
        {
            await DisplayAlert("Error", "Unable to update the scheduled meal for the selected date.", "OK");
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