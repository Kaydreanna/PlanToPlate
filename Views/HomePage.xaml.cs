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
        var secondaryDarkColor = (Color)Microsoft.Maui.Controls.Application.Current.Resources["SecondaryDark"];
        List<ScheduledMeals> scheduledMeals = await DatabaseService.GetScheduledMeals(loggedInUser.UserId, startDate, endDate);
        DateTime displayDate = startDate;
        for (int i = 0; i < 7; i++)
        {
            List<ScheduledMeals> displayDatesMeals = scheduledMeals.FindAll(m => m.Date == displayDate);
            ScheduledMeals breakfast = displayDatesMeals.Find(m => m.MealType == "Breakfast");
            ScheduledMeals lunch = displayDatesMeals.Find(m => m.MealType == "Lunch");
            ScheduledMeals dinner = displayDatesMeals.Find(m => m.MealType == "Dinner");

            Label dateLabel = new Label
            {
                Text = displayDate.ToString("MM/dd"),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Padding = new Thickness(0, 10),
            };
            Border borderedDateLabel = new Border
            {
                StrokeThickness = 1,
                BackgroundColor = secondaryDarkColor,
                Content = dateLabel,
                Padding = 5,
            };
            Button breakfastButton = new Button
            {
                Text = breakfast != null ? await DatabaseService.GetRecipeName(breakfast) : "-",
                LineBreakMode = LineBreakMode.WordWrap,
                FontSize = 8,
                HeightRequest = 20,
                CornerRadius = 0,
                Command = new Command(async () =>
                {
                    if(breakfast != null)
                    {
                        scheduledRecipeButton_Clicked(breakfast.RecipeId);
                    } else
                    {
                        scheduledRecipeButton_Clicked(-1);
                    }
                })
            };
            Button lunchButton = new Button
            {
                Text = lunch != null ? await DatabaseService.GetRecipeName(lunch) : "-",
                LineBreakMode = LineBreakMode.WordWrap,
                FontSize = 8,
                HeightRequest = 20,
                CornerRadius = 0,
                Command = new Command(async () =>
                {
                    if (lunch != null)
                    {
                        scheduledRecipeButton_Clicked(lunch.RecipeId);
                    }
                    else
                    {
                        scheduledRecipeButton_Clicked(-1);
                    }
                })
            };
            Button dinnerButton = new Button
            {
                Text = dinner != null ? await DatabaseService.GetRecipeName(dinner) : "-",
                LineBreakMode = LineBreakMode.WordWrap,
                FontSize = 8,
                HeightRequest = 20,
                CornerRadius = 0,
                Command = new Command(async () =>
                {
                    if (dinner != null)
                    {
                        scheduledRecipeButton_Clicked(dinner.RecipeId);
                    }
                    else
                    {
                        scheduledRecipeButton_Clicked(-1);
                    }
                })
            };

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
