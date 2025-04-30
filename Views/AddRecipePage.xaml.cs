using PlanToPlate.Models;
using PlanToPlate.Services;
using System.Threading.Tasks;

namespace PlanToPlate.Views;

public partial class AddRecipePage : ContentPage
{
    public User loggedInUser { get; set; }
    private int ingredientRowNum = 3;
    private int instructionRowNum = 1;
    public AddRecipePage(User user)
	{
		InitializeComponent();
        loggedInUser = user;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        typePicker.Items.Add("Type");
        typePicker.Items.Add("Breakfast");
        typePicker.Items.Add("Lunch");
        typePicker.Items.Add("Dinner");
        typePicker.SelectedIndex = 0;
        devicePicker.Items.Add("Device");
        devicePicker.Items.Add("None");
        devicePicker.Items.Add("Air Fryer");
        devicePicker.Items.Add("Instant Pot");
        devicePicker.Items.Add("Microwave");
        devicePicker.Items.Add("Oven");
        devicePicker.Items.Add("Stovetop");
        devicePicker.Items.Add("Slow Cooker");
        devicePicker.SelectedIndex = 0;
    }

    private async void closeRecipeButton_Clicked(object sender, EventArgs e)
    {
        bool confirmClose = await DisplayAlert("Discard This Recipe?", "Are you sure you want to close this recipe? Your changes won't be saved if you exit.", "Yes", "No");
        if(confirmClose)
        {
            await Navigation.PopModalAsync();
        }
    }

    private void addIngredientButton_Clicked(object sender, EventArgs e)
    {
        ingredientsGrid.RowDefinitions.Add(new RowDefinition());
        ingredientRowNum++;
        Grid.SetRow(addIngredientButton, ingredientRowNum + 1);

        Entry ingredientAmount = new Entry
        {
            
            Margin = new Thickness(0, 5)
        };
        Entry ingredientMeasurement = new Entry
        {
            Margin = new Thickness(5)
        };
        Entry ingredientName = new Entry
        {
            Margin = new Thickness(0, 5)
        };

        ingredientsGrid.Children.Add(ingredientAmount);
        Grid.SetRow(ingredientAmount, ingredientRowNum);
        Grid.SetColumn(ingredientAmount, 1);

        ingredientsGrid.Children.Add(ingredientMeasurement);
        Grid.SetRow(ingredientMeasurement, ingredientRowNum);
        Grid.SetColumn(ingredientMeasurement, 2);

        ingredientsGrid.Children.Add(ingredientName);
        Grid.SetRow(ingredientName, ingredientRowNum);
        Grid.SetColumn(ingredientName, 3);
    }

    private void addInstructionButton_Clicked(object sender, EventArgs e)
    {
        instructionsGrid.RowDefinitions.Add(new RowDefinition());
        instructionRowNum++;
        Grid.SetRow(addInstructionButton, instructionRowNum + 1);

        Label instructionNumLabel = new Label
        {
            Text = $"{instructionRowNum}.",
            HorizontalOptions = LayoutOptions.End,
            Margin = new Thickness(5)
        };
        Editor instructionEditor = new Editor
        {
            AutoSize = EditorAutoSizeOption.TextChanges,
        };

        instructionsGrid.Children.Add(instructionNumLabel);
        Grid.SetRow(instructionNumLabel, instructionRowNum);
        Grid.SetColumn(instructionNumLabel, 0);

        instructionsGrid.Children.Add(instructionEditor);
        Grid.SetRow(instructionEditor, instructionRowNum);
        Grid.SetColumn(instructionEditor, 1);
    }
    private async void saveButton_Clicked(object sender, EventArgs e)
    {
        if (validInput())
        {
            Dictionary<string, (string, string)> ingredientsToAdd = new Dictionary<string, (string, string)>();
            Dictionary<int, string> instructionsToAdd = new Dictionary<int, string>();

            var ingredientEntries = ingredientsGrid.Children.OfType<Entry>().GroupBy(e => Grid.GetRow(e)).OrderBy(group => group.Key);
            foreach (var newIngredient in ingredientEntries)
            {
                var ingredientInfo = newIngredient.OrderBy(e => Grid.GetColumn(e)).ToList();
                var inputIngredientAmount = ingredientInfo[0].Text;
                var ingredientMeasurement = ingredientInfo[1].Text;
                var ingredientName = ingredientInfo[2].Text;
                if (inputIngredientAmount == null && ingredientMeasurement == null && ingredientName == null)
                {
                    continue;
                }else if (inputIngredientAmount != null && ingredientName != null)
                {
                    ingredientsToAdd.Add(ingredientName, (inputIngredientAmount, ingredientMeasurement));
                }
                else
                {
                    await DisplayAlert("Error", "Please ensure every ingredient has a quantity and a name.", "OK");
                    return;
                }
            }

            var instructionsEntries = instructionsGrid.Children.GroupBy(view => Grid.GetRow((BindableObject)view)).OrderBy(group => group.Key);
            foreach (var newInstruction in instructionsEntries)
            {
                var ingredientInfo = newInstruction.OrderBy(e => Grid.GetColumn((BindableObject)e)).ToList();
                if(ingredientInfo.Count() < 2)
                {
                    continue;
                }
                var instructionNum = ingredientInfo[0] as Label;
                var instructionText = ingredientInfo[1] as Editor;
                if (instructionNum != null && instructionText != null)
                {
                    if(!string.IsNullOrEmpty(instructionText.Text))
                    {
                        instructionsToAdd.Add(Convert.ToInt32(instructionNum.Text.TrimEnd('.')), instructionText.Text);
                    }
                }
            }

            Recipe newRecipe = new Recipe();
            newRecipe.UserId = loggedInUser.UserId;
            newRecipe.RecipeName = nameEntry.Text;
            newRecipe.RecipeType = typePicker.SelectedItem.ToString();
            newRecipe.CookingDevice = devicePicker.SelectedItem.ToString();
            newRecipe.Ingredients = ingredientsToAdd;
            newRecipe.Instructions = instructionsToAdd;
            await DatabaseService.AddRecipe(newRecipe);
            await Navigation.PopModalAsync();
        }
    }

    private bool validInput()
    {
        if (string.IsNullOrEmpty(nameEntry.Text))
        {
            DisplayAlert("Error", "Please enter a recipe name.", "OK");
            return false;
        }
        if (typePicker.SelectedIndex == 0)
        {
            DisplayAlert("Error", "Please select a recipe type.", "OK");
            return false;
        }
        if (devicePicker.SelectedIndex == 0)
        {
            DisplayAlert("Error", "Please select a cooking device.", "OK");
            return false;
        }

        bool ingredientsExist = false;
        foreach (var child in ingredientsGrid.Children)
        {
            if (child is Entry entry && !string.IsNullOrEmpty(entry.Text))
            {
                ingredientsExist = true;
            }
        }
        if (!ingredientsExist)
        {
            DisplayAlert("Error", "Please enter at least one ingredient.", "OK");
            return false;
        }

        bool instructionsExist = false;
        foreach (var child in instructionsGrid.Children)
        {
            if (child is Editor editor && !string.IsNullOrEmpty(editor.Text))
            {
                instructionsExist = true;
            }
        }
        if(!instructionsExist)
        {
            DisplayAlert("Error", "Please enter at least one instruction.", "OK");
            return false;
        }

        return true;
    }

}