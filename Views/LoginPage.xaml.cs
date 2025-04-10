namespace PlanToPlate.Views;

public partial class LoginPage : ContentPage
{
    private bool createAccount = false;
	public LoginPage()
	{
		InitializeComponent();
	}

    private async void LoginButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new HomePage());
        Navigation.RemovePage(this);
    }

    private async void createAccountButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new HomePage());
        Navigation.RemovePage(this);
    }

    private void createAccountLink_Clicked(object sender, EventArgs e)
    {
        if(createAccount == false)
        {
            createAccount = true;
            emailLabel.IsVisible = true;
            emailEntry.IsVisible = true;
            confirmPasswordLabel.IsVisible = true;
            confirmPasswordEntry.IsVisible = true;
            loginLabel.Text = "Create Account";
            LoginButton.IsVisible = false;
            createAccountButton.IsVisible = true;
            createAccountLink.Text = "Already have an account? Log in";
        }
        else
        {
            createAccount = false;
            emailLabel.IsVisible = false;
            emailEntry.IsVisible = false;
            confirmPasswordLabel.IsVisible = false;
            confirmPasswordEntry.IsVisible = false;
            loginLabel.Text = "Log in";
            LoginButton.IsVisible = true;
            createAccountButton.IsVisible = false;
            createAccountLink.Text = "Don't have an account? Create one";
        }

    }
}