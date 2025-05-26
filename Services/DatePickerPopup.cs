using CommunityToolkit.Maui.Views;

namespace PlanToPlate.Services
{
    public class DatePickerPopup : Popup
    {
        public DatePickerPopup(DateTime? initialDate = null, IEnumerable<string>? mealTypes = null)
        {
            var dateLabel = new Label { Text = "Pick a date to schedule this meal on:" };
            var datePicker = new DatePicker { Date = initialDate ?? DateTime.Today};

            var mealTypeLabel = new Label { Text = "Select a meal type:" };
            mealTypes ??= new List<string> { "Breakfast", "Lunch", "Dinner" };
            var mealTypePicker = new Picker { ItemsSource = mealTypes.ToList() };

            var errorLabel = new Label { TextColor = Colors.Red, IsVisible = false };

            var confirmButton = new Button { Text = "Confirm" };
            confirmButton.Clicked += async (s, e) =>
            {
                if (mealTypePicker.SelectedItem == null)
                {
                    errorLabel.Text = "Please select a meal type.";
                    errorLabel.IsVisible = true;
                    return;
                } else
                {
                    Close((datePicker.Date, mealTypePicker.SelectedItem.ToString()));
                }
            };

            Content = new VerticalStackLayout
            {
                Children = { dateLabel, datePicker, mealTypeLabel, mealTypePicker, errorLabel, confirmButton },
                Padding = 20,
                Spacing = 10
            };
            this.Size = new Size(300, 300);
        }
    }
}
