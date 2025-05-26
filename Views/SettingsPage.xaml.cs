using PlanToPlate.Models;
using PlanToPlate.Services;
using static SQLite.SQLite3;
using System;

namespace PlanToPlate.Views;

public partial class SettingsPage : ContentPage
{
    public User loggedInUser { get; set; }
    public SettingsPage(User user)
	{
		InitializeComponent();
        loggedInUser = user;
    }

    #region Clicked Events
    private async void saveChangeUsernameButton_Clicked(object sender, EventArgs e)
    {
        if(loggedInUser.Password == changeUsernamePasswordEntry.Text && loggedInUser.Username == currentUsernameEntry.Text)
        {
            bool validUsername = await DatabaseService.UniqueUsername(newUsernameEntry.Text);
            if(validUsername)
            {
                await DatabaseService.ChangeUsername(loggedInUser, newUsernameEntry.Text);
                currentUsernameEntry.Text = string.Empty;
                newUsernameEntry.Text = string.Empty;
                changeUsernamePasswordEntry.Text = string.Empty;
                await DisplayAlert("Username Changed", "Username was successfully changed!", "OK");
            }
            else
            {
                await DisplayAlert("Error", "Username already exists. Please choose a different one.", "OK");
                return;
            }
        } else
        {
            await DisplayAlert("Incorrect Username or Password", "Please enter the correct username and password before trying to change the username.", "OK");
        }
    }

    private async void saveChangePasswordButton_Clicked(object sender, EventArgs e)
    {
        if(loggedInUser.Password == currentPasswordEntry.Text)
        {
            bool validPassword = validatePassword(newPasswordEntry.Text, confirmPasswordEntry.Text);
            if(validPassword)
            {
                await DatabaseService.ChangePassword(loggedInUser, confirmPasswordEntry.Text);
                currentPasswordEntry.Text = string.Empty;
                newPasswordEntry.Text = string.Empty;
                confirmPasswordEntry.Text = string.Empty;
                await DisplayAlert("Password Changed", "Password was successfully changed!", "OK");
            }
            else
            {
                await DisplayAlert("Error", $"Please ensure passwords match and have the following:{Environment.NewLine}8 or more characters{Environment.NewLine}One or more upper case letters{Environment.NewLine}One or more lower case letters{Environment.NewLine}At least one symbol", "OK");
            }

        } else
        {
            await DisplayAlert("Incorrect Password", "Please enter the current password before trying to change it.", "OK");
        }
    }

    private async void logoutButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LoginPage());
        Navigation.RemovePage(this);
    }

    private async void deleteAccountButton_Clicked(object sender, EventArgs e)
    {
        bool confirmDelete = await DisplayAlert("Delete Account?", "Are you sure you want to delete your account? You will not be able to undo this action.", "Confirm", "Cancel");
        if(confirmDelete)
        {
            await Navigation.PushAsync(new LoginPage());
            Navigation.RemovePage(this);
        }
    }
    #endregion

    private bool validatePassword(string password, string confirmPassword)
    {
        if (password != confirmPassword)
        {
            return false;
        }
        if (password.Length < 8)
        {
            return false;
        }
        bool hasUpperChar = false;
        bool hasLowerChar = false;
        bool hasNumber = false;
        bool hasSymbols = false;
        foreach (char c in password)
        {
            if (char.IsUpper(c))
                hasUpperChar = true;
            else if (char.IsLower(c))
                hasLowerChar = true;
            else if (char.IsDigit(c))
                hasNumber = true;
            else
                hasSymbols = true;
        }
        return hasUpperChar && hasLowerChar && hasNumber && hasSymbols;
    }

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