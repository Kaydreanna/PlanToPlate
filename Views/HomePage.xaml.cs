using PlanToPlate.Models;
using PlanToPlate.Services;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using CommunityToolkit.Maui.Views;

namespace PlanToPlate.Views;

public partial class HomePage : ContentPage
{
    public User loggedInUser { get; set; }
    private Recipe todaysBreakfast = null;
    private Recipe todaysLunch = null;
    private Recipe todaysDinner = null;
    private DateTime startDate { get; set; }
    private DateTime endDate { get; set; }
    private ShoppingList shoppingList { get; set; }
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
        editViewOrCreateShoppingListButton();
    }

    #region Clicked Events  
    private void previousDatesButton_Clicked(object sender, EventArgs e)
    {
        startDate = startDate.AddDays(-7);
        endDate = endDate.AddDays(-7);
        displayedScheduledMeals();
        editViewOrCreateShoppingListButton();
    }

    private void futureDatesButton_Clicked(object sender, EventArgs e)
    {
        startDate = startDate.AddDays(7);
        endDate = endDate.AddDays(7);
        displayedScheduledMeals();
        editViewOrCreateShoppingListButton();
    }

    private void breakfastButton_Clicked(object sender, EventArgs e)
    {
        if(todaysBreakfast != null)
        {
            scheduledRecipeButton_Clicked(todaysBreakfast.RecipeId);
        }
    }

    private void lunchButton_Clicked(object sender, EventArgs e)
    {
        if(todaysLunch != null)
        {
            scheduledRecipeButton_Clicked(todaysLunch.RecipeId);
        }
    }

    private void dinnerButton_Clicked(object sender, EventArgs e)
    {
        if(todaysDinner != null)
        {
            scheduledRecipeButton_Clicked(todaysDinner.RecipeId);
        }
    }
 
    private async void scheduledRecipeButton_Clicked(int recipeId)
    {
        Recipe selectedRecipe = await DatabaseService.GetRecipe(loggedInUser.UserId, recipeId);
        await Navigation.PushAsync(new ViewRecipePage(loggedInUser, selectedRecipe));
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
                    scheduledRecipeButton_Clicked(breakfast.RecipeId);
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
                    scheduledRecipeButton_Clicked(lunch.RecipeId);
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
                    scheduledRecipeButton_Clicked(dinner.RecipeId);
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

    private async void editViewOrCreateShoppingListButton()
    {
        bool shoppingListExists = await existingShoppingList(startDate, endDate);
        if (shoppingListExists)
        {
            viewOrCreateShoppingListButton.Text = "View Shopping List";
            viewOrCreateShoppingListButton.Clicked += async (sender, e) =>
            {
                await Navigation.PushAsync(new EditShoppingListPage(shoppingList));
            };
        }
        else
        {
            viewOrCreateShoppingListButton.Text = "Create Shopping List";
            viewOrCreateShoppingListButton.Clicked += async (sender, e) =>
            {
                ShoppingList newShoppingList = await DatabaseService.CreateShoppingList(loggedInUser.UserId, startDate, endDate);
                await Navigation.PushAsync(new EditShoppingListPage(newShoppingList));
            };

        }
    }

    private async Task<bool> existingShoppingList(DateTime startDate, DateTime endDate)
    {
        List<ShoppingList> shoppingLists = await DatabaseService.GetAllShoppingLists(loggedInUser.UserId);
        foreach(ShoppingList list in shoppingLists)
        {
            if (list.StartDate == startDate && list.EndDate == endDate)
            {
                shoppingList = list;
                return true;
            }
        }
        return false;
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
