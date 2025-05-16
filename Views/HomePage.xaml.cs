using PlanToPlate.Models;
using PlanToPlate.Services;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;

namespace PlanToPlate.Views;

public partial class HomePage : ContentPage
{
    public User loggedInUser { get; set; }
    private Recipe todaysBreakfast = null;
    private Recipe todaysLunch = null;
    private Recipe todaysDinner = null;
    private DateTime startDate { get; set; }
    private DateTime endDate { get; set; }
    public HomePage(User user)
    {
        InitializeComponent();
        loggedInUser = user;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        displayTodaysMealPlan();
        DateTime today = DateTime.Today;
        dateLabel.Text = today.ToString("MMMM dd, yyyy");
        DayOfWeek dayOfWeek = today.DayOfWeek;
        switch (dayOfWeek)
        {
            case DayOfWeek.Sunday:
                startDate = today;
                break;
            case DayOfWeek.Monday:
                startDate = today.AddDays(-1);
                break;
            case DayOfWeek.Tuesday:
                startDate = today.AddDays(-2);
                break;
            case DayOfWeek.Wednesday:
                startDate = today.AddDays(-3);
                break;
            case DayOfWeek.Thursday:
                startDate = today.AddDays(-4);
                break;
            case DayOfWeek.Friday:
                startDate = today.AddDays(-5);
                break;
            case DayOfWeek.Saturday:
                startDate = today.AddDays(-6);
                break;
        }
        endDate = startDate.AddDays(6);
        displayedScheduledMeals();
    }

    #region Clicked Events  
    private void previousDatesButton_Clicked(object sender, EventArgs e)
    {
        startDate = startDate.AddDays(-7);
        endDate = endDate.AddDays(-7);
        displayedScheduledMeals();
    }

    private void futureDatesButton_Clicked(object sender, EventArgs e)
    {
        startDate = startDate.AddDays(7);
        endDate = endDate.AddDays(7);
        displayedScheduledMeals();
    }

    private void breakfastButton_Clicked(object sender, EventArgs e)
    {
        if(todaysBreakfast != null)
        {
            scheduledRecipeButton_Clicked(todaysBreakfast.RecipeId);
        }
        else
        {
            scheduledRecipeButton_Clicked(-1);
        }
    }

    private void lunchButton_Clicked(object sender, EventArgs e)
    {
        if(todaysLunch != null)
        {
            scheduledRecipeButton_Clicked(todaysLunch.RecipeId);
        }
        else
        {
            scheduledRecipeButton_Clicked(-1);
        }
    }

    private void dinnerButton_Clicked(object sender, EventArgs e)
    {
        if(todaysDinner != null)
        {
            scheduledRecipeButton_Clicked(todaysDinner.RecipeId);
        }
        else
        {
            scheduledRecipeButton_Clicked(-1);
        }
    }
 
    private async void scheduledRecipeButton_Clicked(int recipeId)
    {
        if(recipeId != -1)
        {
            Recipe selectedRecipe = await DatabaseService.GetRecipe(loggedInUser.UserId, recipeId);
            await Navigation.PushModalAsync(new ViewRecipePage(loggedInUser, selectedRecipe));
        }
    }
    #endregion

    #region Methods  
    private async void displayTodaysMealPlan()
    {
        DateTime today = DateTime.Today;
        List<ScheduledMeals> todaysMeals = await DatabaseService.GetScheduledMeals(loggedInUser.UserId, today, today);
        ScheduledMeals breakfast = todaysMeals.Find(m => m.MealType == "Breakfast");
        if (breakfast != null)
        {
            todaysBreakfast = await DatabaseService.GetRecipe(loggedInUser.UserId, breakfast.RecipeId);
            breakfastButton.Text = todaysBreakfast.RecipeName;
        }
        else
        {
            breakfastButton.Text = "-";
            todaysBreakfast = null;
        }
        ScheduledMeals lunch = todaysMeals.Find(m => m.MealType == "Lunch");
        if (lunch != null)
        {
            todaysLunch = await DatabaseService.GetRecipe(loggedInUser.UserId, lunch.RecipeId);
            lunchButton.Text = todaysLunch.RecipeName;
        }
        else
        {
            lunchButton.Text = "-";
            todaysLunch = null;
        }
        ScheduledMeals dinner = todaysMeals.Find(m => m.MealType == "Dinner");
        if (dinner != null)
        {
            todaysDinner = await DatabaseService.GetRecipe(loggedInUser.UserId, dinner.RecipeId);
            dinnerButton.Text = todaysDinner.RecipeName;
        }
        else
        {
            dinnerButton.Text = "-";
            todaysDinner = null;
        }
    }

    private async void displayedScheduledMeals()
    {
        var primaryColor = (Color)Microsoft.Maui.Controls.Application.Current.Resources["Primary"];
        var secondaryColor = (Color)Microsoft.Maui.Controls.Application.Current.Resources["Secondary"];
        var secondaryDarkColor = (Color)Microsoft.Maui.Controls.Application.Current.Resources["SecondaryDark"];
        var tertiaryColor = (Color)Microsoft.Maui.Controls.Application.Current.Resources["Tertiary"];
        List<ScheduledMeals> scheduledMeals = await DatabaseService.GetScheduledMeals(loggedInUser.UserId, startDate, endDate);
        DateTime displayDate = startDate;
        for (int i = 0; i < 7; i++)
        {
            List<ScheduledMeals> displayDatesMeals = scheduledMeals.FindAll(m => m.Date == displayDate);
            ScheduledMeals breakfast = displayDatesMeals.Find(m => m.MealType == "Breakfast");
            ScheduledMeals lunch = displayDatesMeals.Find(m => m.MealType == "Lunch");
            ScheduledMeals dinner = displayDatesMeals.Find(m => m.MealType == "Dinner");

            Border borderedDateLabel = new Border
            {
                StrokeThickness = 1,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                HeightRequest = 30,
                Padding = 0,
                Margin = 0,
                Content = new Label
                {
                    Text = displayDate.ToString("MM/dd"),
                    FontSize = 18,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0)
                }
            };
            Button breakfastButton = new Button
            {
                Text = breakfast != null ? await DatabaseService.GetRecipeName(breakfast) : "-",
                LineBreakMode = LineBreakMode.WordWrap,
                FontSize = 10,
                TextColor = breakfast != null ? tertiaryColor : Colors.Black,
                HeightRequest = 20,
                CornerRadius = 0,
                BorderWidth = 1,
                BorderColor = Colors.Black,
                Padding = 0,
                Margin = 0,
            };
            breakfastButton.BindingContext = displayDate;
            breakfastButton.Clicked += async (sender, e) =>
            {
                if (breakfast != null)
                {
                    string breakfastName = await DatabaseService.GetRecipeName(breakfast);
                    bool deleteMeal = await DisplayAlert("Delete Meal?", $"Would you like to delete {breakfastName} from {breakfast.Date.ToString("MM/dd/yyyy")}?", "Yes", "No");
                    if (deleteMeal)
                    {
                        await DatabaseService.DeleteScheduleMeal(breakfast);
                        await DatabaseService.UpdateShoppingLists(loggedInUser.UserId, breakfast.Date);
                        displayedScheduledMeals();
                    }
                }
                else
                {
                    DateTime buttonsDate = (DateTime)breakfastButton.BindingContext;
                    bool addMeal = await DisplayAlert("Add Meal?", $"Would you like to add a breakfast to {buttonsDate.ToString("MM/dd/yyyy")}?", "Yes", "No");
                    if (addMeal)
                    {
                        await DisplayAlert("Add Meal", "This feature is not yet implemented.", "OK");
                    }
                }
            };

            Button lunchButton = new Button
            {
                Text = lunch != null ? await DatabaseService.GetRecipeName(lunch) : "-",
                LineBreakMode = LineBreakMode.WordWrap,
                FontSize = 10,
                TextColor = lunch != null ? tertiaryColor : Colors.Black,
                HeightRequest = 20,
                CornerRadius = 0,
                BorderWidth = 1,
                BorderColor = Colors.Black,
                Padding = 0,
                Margin = 0,
            };
            lunchButton.BindingContext = displayDate;
            lunchButton.Clicked += async (sender, e) =>
            {
                if (lunch != null)
                {
                    string lunchName = await DatabaseService.GetRecipeName(lunch);
                    bool deleteMeal = await DisplayAlert("Delete Meal?", $"Would you like to delete {lunchName} from {lunch.Date.ToString("MM/dd/yyyy")}?", "Yes", "No");
                    if (deleteMeal)
                    {
                        await DatabaseService.DeleteScheduleMeal(lunch);
                        await DatabaseService.UpdateShoppingLists(loggedInUser.UserId, lunch.Date);
                        displayedScheduledMeals();
                    }
                }
                else
                {
                    DateTime buttonsDate = (DateTime)lunchButton.BindingContext;
                    bool addMeal = await DisplayAlert("Add Meal?", $"Would you like to add a lunch to {buttonsDate.ToString("MM/dd/yyyy")}?", "Yes", "No");
                    if (addMeal)
                    {
                        await DisplayAlert("Add Meal", "This feature is not yet implemented.", "OK");
                    }
                }
            };

            Button dinnerButton = new Button
            {
                Text = dinner != null ? await DatabaseService.GetRecipeName(dinner) : "-",
                LineBreakMode = LineBreakMode.WordWrap,
                FontSize = 10,
                TextColor = dinner != null ? tertiaryColor : Colors.Black,
                HeightRequest = 20,
                CornerRadius = 0,
                BorderWidth = 1,
                BorderColor = Colors.Black,
                Padding = 0,
                Margin = 0,
            };
            dinnerButton.BindingContext = displayDate;
            dinnerButton.Clicked += async (sender, e) =>
            {
                if (dinner != null)
                {
                    string dinnerName = await DatabaseService.GetRecipeName(dinner);
                    bool deleteMeal = await DisplayAlert("Delete Meal?", $"Would you like to delete {dinnerName} from {dinner.Date.ToString("MM/dd/yyyy")}?", "Yes", "No");
                    if (deleteMeal)
                    {
                        await DatabaseService.DeleteScheduleMeal(dinner);
                        await DatabaseService.UpdateShoppingLists(loggedInUser.UserId, dinner.Date);
                        displayedScheduledMeals();
                    }
                }
                else
                {
                    DateTime buttonsDate = (DateTime)dinnerButton.BindingContext;
                    bool addMeal = await DisplayAlert("Add Meal?", $"Would you like to add a dinner to {buttonsDate.ToString("MM/dd/yyyy")}?", "Yes", "No");
                    if (addMeal)
                    {
                        await DisplayAlert("Add Meal", "This feature is not yet implemented.", "OK");
                    }
                }
            };

            if (displayDate == DateTime.Today)
            {
                borderedDateLabel.BackgroundColor = primaryColor;
                breakfastButton.BackgroundColor = secondaryDarkColor;
                lunchButton.BackgroundColor = secondaryDarkColor;
                dinnerButton.BackgroundColor = secondaryDarkColor;
            } else
            {
                borderedDateLabel.BackgroundColor = secondaryDarkColor;
                breakfastButton.BackgroundColor = secondaryColor;
                lunchButton.BackgroundColor = secondaryColor;
                dinnerButton.BackgroundColor = secondaryColor;
            }

            mealPlanCalendar.Children.Add(borderedDateLabel);
            mealPlanCalendar.SetColumn(borderedDateLabel, i);
            mealPlanCalendar.SetRow(borderedDateLabel, 0);
            mealPlanCalendar.Children.Add(breakfastButton);
            mealPlanCalendar.SetColumn(breakfastButton, i);
            mealPlanCalendar.SetRow(breakfastButton, 1);
            mealPlanCalendar.Children.Add(lunchButton);
            mealPlanCalendar.SetColumn(lunchButton, i);
            mealPlanCalendar.SetRow(lunchButton, 2);
            mealPlanCalendar.Children.Add(dinnerButton);
            mealPlanCalendar.SetColumn(dinnerButton, i);
            mealPlanCalendar.SetRow(dinnerButton, 3);


            displayDate = displayDate.AddDays(1);
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
