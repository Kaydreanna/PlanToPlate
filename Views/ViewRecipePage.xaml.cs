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
    private float overallRating;
    private int easeRating;
    private int tasteRating;
    private int timeRating;
    public ViewRecipePage(User user, Recipe recipe)
	{
		InitializeComponent();
        loggedInUser = user;
        selectedRecipe = recipe;
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        (float overall, int ease, int taste, int time) = await DatabaseService.GetRatingScores(selectedRecipe.RecipeId);
        overallRating = overall;
        easeRating = ease;
        tasteRating = taste;
        timeRating = time;
        diaplayOverallRating();
        recipeNameLabel.Text = selectedRecipe.RecipeName;
        typeLabel.Text = selectedRecipe.RecipeType;
        deviceLabel.Text = selectedRecipe.CookingDevice;
        displayRatings();
        displayIngredients();
    }

    #region Clicked Events
    private async void closeRecipeButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
    private async void editButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddRecipePage(loggedInUser, selectedRecipe));
    }
    private async void deleteButton_Clicked(object sender, EventArgs e)
    {
        bool confirmDelete = await DisplayAlert("Delete Recipe", "Are you sure you want to delete this recipe?", "Yes", "No");
        if(confirmDelete)
        {
            await DatabaseService.DeleteRecipe(selectedRecipe.RecipeId);
            await Navigation.PopAsync();
        }
    }
    private async void scheduleMealButton_Clicked(object sender, EventArgs e)
    {
        await displayScheduleMealPopup();
    }
    private void viewRatingsButton_Clicked(object sender, EventArgs e)
    {
        ratingsGrid.IsVisible = !ratingsGrid.IsVisible;
    }
    #endregion

    #region Methods
    private void diaplayOverallRating()
    {
        double roundedOverall = Math.Round(overallRating * 2, MidpointRounding.AwayFromZero) / 2;
        if(roundedOverall < 0)
        {
            noRatingsFoundMessage.IsVisible = true;
            viewRatingsButton.IsVisible = false;
            return;
        } else
        {
            noRatingsFoundMessage.IsVisible = false;
            viewRatingsButton.IsVisible = true;
        }
        if(roundedOverall >= 1)
        {
            star1.IsVisible = true;
        }
        if(roundedOverall >= 2)
        {
            star2.IsVisible = true;
        }
        if(roundedOverall >= 3)
        {
            star3.IsVisible = true;
        }
        if(roundedOverall >= 4)
        {
            star4.IsVisible = true;
        }
        if (roundedOverall >= 5)
        {
            star5.IsVisible = true;
        }
        if(roundedOverall % 2 != 0)
        {
            halfStar.IsVisible = true;
        }
    }

    private async void displayRatings()
    {
        (List<Ease> easeRatings, List<Taste> tasteRatings, List<Timing> timingRatings) = await DatabaseService.GetRatings(selectedRecipe.RecipeId);
        int rowNum = 0;
        foreach (Ease ease in easeRatings)
        {
            easeRatingsGrid.RowDefinitions.Add(new RowDefinition());
            Label dateLabel = new Label
            {
                Text = ease.Date.ToString("MM/d/yy"),
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                FontSize = 16
            };
            Label ratingLabel = new Label
            {
                Text = $"{ease.EaseScore} / 5",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                FontSize = 16
            };
            Label commentLabel = new Label
            {
                Text = ease.EaseComment.ToString(),
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                FontSize = 16
            };
            easeRatingsGrid.Children.Add(dateLabel);
            easeRatingsGrid.SetColumn(dateLabel, 0);
            easeRatingsGrid.SetRow(dateLabel, rowNum);
            easeRatingsGrid.Children.Add(ratingLabel);
            easeRatingsGrid.SetColumn(ratingLabel, 1);
            easeRatingsGrid.SetRow(ratingLabel, rowNum);
            easeRatingsGrid.Children.Add(commentLabel);
            easeRatingsGrid.SetColumn(commentLabel, 2);
            easeRatingsGrid.SetRow(commentLabel, rowNum);
            rowNum++;
        }
        easeRatingsGrid.SetRow(addEaseRatingButton, rowNum);
        rowNum = 0;
        foreach (Taste taste in tasteRatings)
        {
            tasteRatingsGrid.RowDefinitions.Add(new RowDefinition());
            Label dateLabel = new Label
            {
                Text = taste.Date.ToString("MM/d/yy"),
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                FontSize = 16
            };
            Label ratingLabel = new Label
            {
                Text = $"{taste.TasteScore} / 5",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                FontSize = 16
            };
            Label commentLabel = new Label
            {
                Text = taste.TasteComment.ToString(),
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                FontSize = 16
            };
            tasteRatingsGrid.Children.Add(dateLabel);
            tasteRatingsGrid.SetColumn(dateLabel, 0);
            tasteRatingsGrid.SetRow(dateLabel, rowNum);
            tasteRatingsGrid.Children.Add(ratingLabel);
            tasteRatingsGrid.SetColumn(ratingLabel, 1);
            tasteRatingsGrid.SetRow(ratingLabel, rowNum);
            tasteRatingsGrid.Children.Add(commentLabel);
            tasteRatingsGrid.SetColumn(commentLabel, 2);
            tasteRatingsGrid.SetRow(commentLabel, rowNum);
            rowNum++;
        }
        tasteRatingsGrid.SetRow(addTasteRatingButton, rowNum);
        rowNum = 0;
        foreach (Timing timing in timingRatings)
        {
            timingRatingsGrid.RowDefinitions.Add(new RowDefinition());
            Label dateLabel = new Label
            {
                Text = timing.Date.ToString("MM/d/yy"),
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                FontSize = 16
            };
            Label ratingLabel = new Label
            {
                Text = $"{timing.TimeScore} / 5",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                FontSize = 16
            };
            Label timeLengthLabel = new Label
            {
                Text = $"{timing.AmountOfTime} {timing.TimeUnit}",
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                FontSize = 16
            };
            Label commentLabel = new Label
            {
                Text = timing.TimeComment.ToString(),
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                FontSize = 16
            };
            timingRatingsGrid.Children.Add(dateLabel);
            timingRatingsGrid.SetColumn(dateLabel, 0);
            timingRatingsGrid.SetRow(dateLabel, rowNum);
            timingRatingsGrid.Children.Add(ratingLabel);
            timingRatingsGrid.SetColumn(ratingLabel, 1);
            timingRatingsGrid.SetRow(ratingLabel, rowNum);
            timingRatingsGrid.Children.Add(timeLengthLabel);
            timingRatingsGrid.SetColumn(timeLengthLabel, 2);
            timingRatingsGrid.SetRow(timeLengthLabel, rowNum);
            timingRatingsGrid.Children.Add(commentLabel);
            timingRatingsGrid.SetColumn(commentLabel, 3);
            timingRatingsGrid.SetRow(commentLabel, rowNum);
            rowNum++;
        }
        timingRatingsGrid.SetRow(addTimingRatingButton, rowNum);
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

    public async Task displayScheduleMealPopup()
    {
        var pickerPopup = new DatePickerPopup();
        var result = await this.ShowPopupAsync(pickerPopup);

        if(result is ValueTuple<DateTime, string> selection)
        {
            DateTime selectedDate = selection.Item1;
            string selectedMealType = selection.Item2;
            List<ScheduledMeals> scheduledMeals = await DatabaseService.GetScheduledMeals(loggedInUser.UserId, selectedDate);
            ScheduledMeals conflictingMeal = null;
            foreach(ScheduledMeals meal in scheduledMeals)
            {
                if (meal.MealType == selectedMealType)
                {
                    conflictingMeal = meal;
                    break;
                }
            }

            if (conflictingMeal != null)
            {
                bool confirmReplace = await DisplayAlert("Conflicting Meal", "You already have a meal scheduled for this date and meal type. Do you want to replace it?", "Yes", "No");
                if (confirmReplace)
                {
                    await DatabaseService.DeleteScheduleMeal(conflictingMeal);
                } else
                {
                    return;
                }
            }

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
    #endregion

    private void addEaseRating_Clicked(object sender, EventArgs e)
    {

    }

    private void addTasteRating_Clicked(object sender, EventArgs e)
    {

    }

    private void addTimingRating_Clicked(object sender, EventArgs e)
    {

    }
}