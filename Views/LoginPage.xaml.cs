using PlanToPlate.Models;
using PlanToPlate.Services;
using System.Threading.Tasks;

namespace PlanToPlate.Views;

public partial class LoginPage : ContentPage
{
    private bool createAccount = false;
	public LoginPage()
	{
		InitializeComponent();
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        //await DatabaseService.ClearStartingData();

        bool testUser = await DatabaseService.IsThereATestUser();
        if (testUser == false)
        {
            await DatabaseService.LoadStartingData();
        }
    }

    #region Clicked Events
    private async void LoginButton_Clicked(object sender, EventArgs e)
    {
        User userToLogIn = await DatabaseService.AuthenticateUser("test@email.com", "Test");
        await Navigation.PushAsync(new HomePage(userToLogIn));
        Navigation.RemovePage(this);

        //Commented out login function for testing purposes

        //User userToLogIn = await DatabaseService.AuthenticateUser(emailOrUsernameEntry.Text, passwordEntry.Text);
        //if (userToLogIn == null)
        //{
        //    await DisplayAlert("Error", "Invalid email or password", "OK");
        //    return;
        //}
        //else
        //{
        //    await Navigation.PushAsync(new HomePage(userToLogIn));
        //    Navigation.RemovePage(this);
        //}
    }

    private async void createAccountButton_Clicked(object sender, EventArgs e)
    {
        bool validUsername = await DatabaseService.UniqueUsername(usernameEntry.Text);
        bool validEmail = validateEmail(emailEntry.Text);
        bool validPassword = validatePassword(passwordEntry.Text, confirmPasswordEntry.Text);

        if(!validUsername)
        {
            await DisplayAlert("Error", "Username already exists. Please choose a different one.", "OK");
            return;
        } else if (!validEmail)
        {
            await DisplayAlert("Error", "Please enter a valid email address.", "OK");
            return;
        } else if (!validPassword)
        {
            await DisplayAlert("Error", $"Please ensure passwords match and have the following:{Environment.NewLine}8 or more characters{Environment.NewLine}One or more upper case letters{Environment.NewLine}One or more lower case letters{Environment.NewLine}At least one symbol", "OK");
            return;
        }
        else
        {
            User newUser = await DatabaseService.CreateUserAccount(usernameEntry.Text, emailEntry.Text, passwordEntry.Text);
            await Navigation.PushAsync(new HomePage(newUser));
            Navigation.RemovePage(this);
        }
    }

    private void createAccountLink_Clicked(object sender, EventArgs e)
    {
        if(createAccount == false)
        {
            createAccount = true;
            usernameLabel.IsVisible = true;
            usernameEntry.IsVisible = true;
            emailLabel.IsVisible = true;
            emailEntry.IsVisible = true;
            confirmPasswordLabel.IsVisible = true;
            confirmPasswordEntry.IsVisible = true;
            loginLabel.Text = "Create Account";
            emailOrUsernameLabel.IsVisible = false;
            emailOrUsernameEntry.IsVisible = false;
            LoginButton.IsVisible = false;
            createAccountButton.IsVisible = true;
            createAccountLink.Text = "Already have an account? Log in";
        }
        else
        {
            createAccount = false;
            usernameLabel.IsVisible = false;
            usernameEntry.IsVisible = false;
            emailLabel.IsVisible = false;
            emailEntry.IsVisible = false;
            confirmPasswordLabel.IsVisible = false;
            confirmPasswordEntry.IsVisible = false;
            loginLabel.Text = "Log in";
            emailOrUsernameLabel.IsVisible = true;
            emailOrUsernameEntry.IsVisible = true;
            LoginButton.IsVisible = true;
            createAccountButton.IsVisible = false;
            createAccountLink.Text = "Don't have an account? Create one";
        }

    }
    #endregion

    #region Methods
    private bool validateEmail(string email)
    {
        string trimmedEmail = email.Trim();
        try
        {
            var addr = new System.Net.Mail.MailAddress(trimmedEmail);
            string domainPart = addr.Host;
            return addr.Address == trimmedEmail && domainPart.Contains('.') && domainPart.IndexOf('.') != domainPart.Length - 1;
        }
        catch
        {
            return false;
        }
    }

    private bool validatePassword(string password, string confirmPassword)
    {
        if(password != confirmPassword)
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
    #endregion
}