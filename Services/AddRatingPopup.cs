using CommunityToolkit.Maui.Views;

namespace PlanToPlate.Services
{
    public class AddRatingPopup : Popup
    {
        public AddRatingPopup(string recipeName)
        {
            var label = new Label { Text = $"Rate the recipe: {recipeName}" };
            var typeOfRatingPicker = new Picker
            {
                Title = "Select Rating Type",
                ItemsSource = new List<string> { "Ease", "Taste", "Time" }
            };
            var ratingPicker = new Picker
            {
                Title = "Select Rating Score",
                ItemsSource = Enumerable.Range(1, 5).Select(i => i.ToString()).ToList()
            };
            var timePicker = new Picker
            {
                Title = "Optional: Select time length",
                ItemsSource = new List<string> { "Less than 15 minutes", "15 minutes", "30 minutes", "45 minutes", "1 hour", "2 hours", "3 hours", "4 hours", "5 hours", "6 hours", "7 hours", "8 hours", "9 hours", "10 hours", "Longer than 10 hours" },
                IsVisible = false
            };
            typeOfRatingPicker.SelectedIndexChanged += (s, e) =>
            {
                if (typeOfRatingPicker.SelectedItem?.ToString() == "Time")
                {
                    timePicker.IsVisible = true;
                }
                else
                {
                    timePicker.IsVisible = false;
                }
            };
            var ratingCommentEntry = new Entry
            {
                Placeholder = "Optional: Add a comment about your rating",
            };
            var confirmButton = new Button { Text = "Submit Rating" };
            confirmButton.Clicked += (s, e) =>
            {
                string ratingType = typeOfRatingPicker.SelectedItem?.ToString() ?? string.Empty;
                int ratingScore = int.Parse(ratingPicker.SelectedItem?.ToString() ?? "0");
                string ratingComment = ratingCommentEntry.Text ?? string.Empty;
                string timeTaken = timePicker.IsVisible ? timePicker.SelectedItem?.ToString() ?? string.Empty : string.Empty;
                var ratingInfo = (ratingType, ratingScore, ratingComment, timeTaken);
                //onRatingSelected(ratingType, ratingScore);
                Close(ratingInfo);
            };
            Content = new VerticalStackLayout
            {
                Children = { label, typeOfRatingPicker, ratingPicker, timePicker, ratingCommentEntry, confirmButton },
                Padding = 20,
                Spacing = 10
            };
            this.Size = new Size(300, 400);
            this.Color = Colors.White;
        }
    }
}
