using PlanToPlate.Views;

namespace PlanToPlate
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            var mainPage = new LoginPage();
            var navPage = new NavigationPage(mainPage);
            MainPage = navPage;
        }

        //protected override Window CreateWindow(IActivationState? activationState)
        //{
        //    return new Window(new AppShell());
        //}
    }
}