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
        User userToLogIn = await DatabaseService.AuthenticateUser(emailOrUsernameEntry.Text, passwordEntry.Text);
        if (userToLogIn == null)
        {
            await DisplayAlert("Error", "Invalid email or password", "OK");
            return;
        }
        else
        {
            await Navigation.PushAsync(new HomePage(userToLogIn));
            Navigation.RemovePage(this);
        }
    }

    private async void createAccountButton_Clicked(object sender, EventArgs e)
    {
        bool validUsername = await DatabaseService.UniqueUsername(usernameEntry.Text);
        bool validEmail = validateEmail(emailEntry.Text);
        List<string> whatsWrongWithThePassword = validatePassword(passwordEntry.Text, confirmPasswordEntry.Text);
        bool emailExists = await DatabaseService.EmailExists(emailEntry.Text);

        if (!validUsername)
        {
            await DisplayAlert("Error", "Username already exists. Please choose a different one.", "OK");
            return;
        } else if (!validEmail)
        {
            await DisplayAlert("Error", "Please enter a valid email address.", "OK");
            return;
        } else if (whatsWrongWithThePassword.Count != 0)
        {
            string errorMessage = string.Join(Environment.NewLine, whatsWrongWithThePassword);
            await DisplayAlert("Error", $"Please make sure the following problems are resolved:{Environment.NewLine}{errorMessage}", "OK");
            return;
        } else if (emailExists)
        {
            await DisplayAlert("Error", "An account with this email already exists. Please log in or use a different email.", "OK");
            return;
        } else
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

    private List<string> validatePassword(string password, string confirmPassword)
    {
        List<string> whatsWrongWithThePassword = new List<string>();
        if (password != confirmPassword)
        {
            whatsWrongWithThePassword.Add("Passwords do not match");
        }
        if (password.Length < 8)
        {
            whatsWrongWithThePassword.Add("Password must be at least 8 characters long");
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
        if (!hasUpperChar)
        {
            whatsWrongWithThePassword.Add("Password must contain at least one upper case letter");
        }
        if (!hasLowerChar)
        {
            whatsWrongWithThePassword.Add("Password must contain at least one lower case letter");
        }
        if (!hasNumber)
        {
            whatsWrongWithThePassword.Add("Password must contain at least one number");
        }
        if (!hasSymbols)
        {
            whatsWrongWithThePassword.Add("Password must contain at least one symbol");
        }

        return whatsWrongWithThePassword;
    }
    #endregion
}