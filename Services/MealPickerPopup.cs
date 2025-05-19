using CommunityToolkit.Maui.Views;

namespace PlanToPlate.Services
{
    public class MealPickerPopup : Popup
    {
        public MealPickerPopup(IEnumerable<string> meals)
        {
            var label = new Label { Text = "Pick a meal to schedule" };
            var picker = new Picker { ItemsSource = (System.Collections.IList)meals };
            var confirmButton = new Button { Text = "Confirm" };

            confirmButton.Clicked += (s, e) =>
            {
                Close(picker.SelectedItem?.ToString());
            };

            Content = new VerticalStackLayout
            {
                Children = { label, picker, confirmButton },
                Padding = 20,
                Spacing = 10
            };
            this.Size = new Size(300, 200);
            //this.Color = Colors.Navy;
        }
    }
}
