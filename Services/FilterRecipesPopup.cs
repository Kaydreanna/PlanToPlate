using CommunityToolkit.Maui.Views;
using PlanToPlate.Models;

namespace PlanToPlate.Services
{
    public class FilterRecipesPopup : Popup
    {
        public FilterRecipesPopup(List<string> deviceOptions, List<string> ingredientOptions, string selectedRating, string selectedDevice, string selectedMealType, string selectedIngredient)
        {
            var label = new Label { Text = "Filter Recipes By:" };
            var ratingPicker = new Picker
            {
                Title = "Rating",
                ItemsSource = new List<string>
                {
                    "None", "0-1", "1-2", "2-3", "3-4", "4-5"
                }
            };
            if(!string.IsNullOrEmpty(selectedRating))
            {
                ratingPicker.SelectedItem = selectedRating;
            }

            var devicePicker = new Picker
            {
                Title = "Device",
                ItemsSource = deviceOptions
            };
            if (!string.IsNullOrEmpty(selectedDevice))
            {
                devicePicker.SelectedItem = selectedDevice;
            }

            var mealTypePicker = new Picker
            {
                Title = "Meal Type",
                ItemsSource = new List<string>
                {
                    "Breakfast", "Lunch", "Dinner"
                }
            };
            if (!string.IsNullOrEmpty(selectedMealType))
            {
                mealTypePicker.SelectedItem = selectedMealType;
            }

            var ingredientPicker = new Picker
            {
                Title = "Ingredient",
                ItemsSource = ingredientOptions
            };
            if (!string.IsNullOrEmpty(selectedIngredient))
            {
                ingredientPicker.SelectedItem = selectedIngredient;
            }

            var comfirmButton = new Button { Text = "Apply Filters" };
            comfirmButton.Clicked += (s, e) =>
            {
                string selectedRating = ratingPicker.SelectedItem?.ToString();
                string selectedDevice = devicePicker.SelectedItem?.ToString();
                string selectedMealType = mealTypePicker.SelectedItem?.ToString();
                string selectedIngredient = ingredientPicker.SelectedItem?.ToString();
                var filterInfo = (selectedRating, selectedDevice, selectedMealType, selectedIngredient);
                Close(filterInfo);
            };
            Content = new VerticalStackLayout
            {
                Children = { label, ratingPicker, devicePicker, mealTypePicker, ingredientPicker, comfirmButton },
                Padding = 20,
                Spacing = 10
            };
            this.Size = new Size(300, 400);
            this.Color = Colors.White;
        }
    }
}
