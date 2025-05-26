using PlanToPlate.Models;
using PlanToPlate.Services;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace PlanToPlate.Views;

public partial class AddRecipePage : ContentPage
{
    public User loggedInUser { get; set; }
    private Recipe recipeToEdit { get; set; }
    private int ingredientRowNum { get; set; }
    private int instructionRowNum { get; set; }
    public AddRecipePage(User user, Recipe editRecipe)
	{
		InitializeComponent();
        loggedInUser = user;
        recipeToEdit = editRecipe;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        typePicker.Items.Add("Type");
        typePicker.Items.Add("Breakfast");
        typePicker.Items.Add("Lunch");
        typePicker.Items.Add("Dinner");

        devicePicker.Items.Add("Device");
        devicePicker.Items.Add("None");
        devicePicker.Items.Add("Air Fryer");
        devicePicker.Items.Add("Instant Pot");
        devicePicker.Items.Add("Microwave");
        devicePicker.Items.Add("Oven");
        devicePicker.Items.Add("Stovetop");
        devicePicker.Items.Add("Slow Cooker");

        if(recipeToEdit != null)
        {
            addNewRecipeLabel.Text = "Edit Recipe";
            nameEntry.Text = recipeToEdit.RecipeName;
            typePicker.SelectedItem = recipeToEdit.RecipeType;
            devicePicker.SelectedItem = recipeToEdit.CookingDevice;
            displayExistingIngredientsAndInstructions();
        } else
        {
            addNewRecipeLabel.Text = "Add New Recipe";
            typePicker.SelectedIndex = 0;
            devicePicker.SelectedIndex = 0;
            ingredientRowNum = 1;
            instructionRowNum = 1;
            displayBlankIngredient("2", "Cups", "Milk");
            displayBlankIngredient("1/2", "tsp", "Vanilla Extract");
            displayBlankIngredient("1", "Stick", "Butter");
            displayBlankInstruction();
        }
    }

    private async void closeRecipeButton_Clicked(object sender, EventArgs e)
    {
        bool confirmClose = await DisplayAlert("Discard This Recipe?", "Are you sure you want to close this recipe? Your changes won't be saved if you exit.", "Yes", "No");
        if(confirmClose)
        {
            await Navigation.PopAsync();
        }
    }

    private void addIngredientButton_Clicked(object sender, EventArgs e)
    {
        ingredientsGrid.RowDefinitions.Add(new RowDefinition());
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

        ingredientRowNum++;
    }

    private void addInstructionButton_Clicked(object sender, EventArgs e)
    {
        instructionsGrid.RowDefinitions.Add(new RowDefinition());
        Grid.SetRow(addInstructionButton, instructionRowNum + 1);

        Label instructionNumLabel = new Label
        {
            Text = $"{instructionRowNum}.",
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(0, 15, 15, 0)
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

        instructionRowNum++;
    }
    private async void saveButton_Clicked(object sender, EventArgs e)
    {
        if (validInput())
        {
            Dictionary<string, (string, string)> ingredientsToAdd = new Dictionary<string, (string, string)>();
            Dictionary<int, string> instructionsToAdd = new Dictionary<int, string>();
            List<string> ingredientNames = new List<string>();

            var ingredientEntries = ingredientsGrid.Children.OfType<Entry>().GroupBy(e => Grid.GetRow(e)).OrderBy(group => group.Key);
            foreach (var newIngredient in ingredientEntries)
            {
                var ingredientInfo = newIngredient.OrderBy(e => Grid.GetColumn(e)).ToList();
                var inputIngredientAmount = ingredientInfo[0].Text;
                var ingredientMeasurement = ingredientInfo[1].Text;
                var ingredientName = ingredientInfo[2].Text;
                ingredientNames.Add(ingredientName);
                if (inputIngredientAmount == null && ingredientMeasurement == null && ingredientName == null)
                {
                    continue;
                }else if (inputIngredientAmount != null && ingredientName != null)
                {
                    if(ingredientsToAdd.ContainsKey(ingredientName.Trim()))
                    {
                        await DisplayAlert("Error", $"The ingredient '{ingredientName.Trim()}' was added more than once.", "OK");
                        return;
                    }
                    ingredientsToAdd.Add(ingredientName.Trim(), (inputIngredientAmount, ingredientMeasurement));
                }
                else
                {
                    await DisplayAlert("Error", "Please ensure every ingredient has a quantity and a name.", "OK");
                    return;
                }
            }
            await DatabaseService.AddIngredients(loggedInUser.UserId, ingredientNames);

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

            if(recipeToEdit == null)
            {
                await DatabaseService.AddRecipe(newRecipe);
            } else
            {
                await DatabaseService.UpdateRecipe(recipeToEdit, newRecipe);
            }
            await Navigation.PopAsync();
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

    private void displayExistingIngredientsAndInstructions()
    {
        ingredientRowNum = 1;
        foreach (var ingredient in recipeToEdit.Ingredients)
        {
            ingredientsGrid.RowDefinitions.Add(new RowDefinition());
            Entry quantityEntry = new Entry
            {
                Text = ingredient.Value.Quantity,
                Margin = new Thickness(0, 5)
            };
            Entry unitEntry = new Entry
            {
                Text = ingredient.Value.Unit,
                Margin = new Thickness(5)
            };
            Entry nameEntry = new Entry
            {
                Text = ingredient.Key,
                Margin = new Thickness(0, 5)
            };

            ingredientsGrid.Children.Add(quantityEntry);
            ingredientsGrid.SetRow(quantityEntry, ingredientRowNum);
            ingredientsGrid.SetColumn(quantityEntry, 1); 
            
            ingredientsGrid.Children.Add(unitEntry);
            ingredientsGrid.SetRow(unitEntry, ingredientRowNum);
            ingredientsGrid.SetColumn(unitEntry, 2);
            
            ingredientsGrid.Children.Add(nameEntry);
            ingredientsGrid.SetRow(nameEntry, ingredientRowNum);
            ingredientsGrid.SetColumn(nameEntry, 3);

            ingredientRowNum++;
        }
        
        instructionRowNum = 1;
        foreach (var instruction in recipeToEdit.Instructions)
        {
            instructionsGrid.RowDefinitions.Add(new RowDefinition());
            Label instructionNumLabel = new Label
            {
                Text = $"{instruction.Key}.",
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Start,
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(0, 15, 15, 0)
            };
            Editor instructionsEntry = new Editor
            {
                Text = instruction.Value,
                AutoSize = EditorAutoSizeOption.TextChanges
            };

            instructionsGrid.Children.Add(instructionNumLabel);
            instructionsGrid.SetRow(instructionNumLabel, instructionRowNum);
            instructionsGrid.SetColumn(instructionNumLabel, 0);

            instructionsGrid.Children.Add(instructionsEntry);
            instructionsGrid.SetRow(instructionsEntry, instructionRowNum);
            instructionsGrid.SetColumn(instructionsEntry, 1);

            instructionRowNum++;
        }
    }

    private void displayBlankIngredient(string quantity, string unit, string name)
    {
        ingredientsGrid.RowDefinitions.Add(new RowDefinition());
        Entry quantityEntry = new Entry
        {
            Placeholder = quantity,
            Margin = new Thickness(0, 5)
        };
        Entry unitEntry = new Entry
        {
            Placeholder = unit,
            Margin = new Thickness(5)
        };
        Entry nameEntry = new Entry
        {
            Placeholder = name,
            Margin = new Thickness(0, 5)
        };

        ingredientsGrid.Children.Add(quantityEntry);
        ingredientsGrid.SetRow(quantityEntry, ingredientRowNum);
        ingredientsGrid.SetColumn(quantityEntry, 1);

        ingredientsGrid.Children.Add(unitEntry);
        ingredientsGrid.SetRow(unitEntry, ingredientRowNum);
        ingredientsGrid.SetColumn(unitEntry, 2);

        ingredientsGrid.Children.Add(nameEntry);
        ingredientsGrid.SetRow(nameEntry, ingredientRowNum);
        ingredientsGrid.SetColumn(nameEntry, 3);

        ingredientRowNum++;
    }

    private void displayBlankInstruction()
    {
        instructionsGrid.RowDefinitions.Add(new RowDefinition());
        Label instructionNumLabel = new Label
        {
            Text = $"1.",
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(0, 15, 15, 0)
        };
        Editor instructionsEntry = new Editor
        {
            AutoSize = EditorAutoSizeOption.TextChanges,
        };

        instructionsGrid.Children.Add(instructionNumLabel);
        instructionsGrid.SetRow(instructionNumLabel, instructionRowNum);
        instructionsGrid.SetColumn(instructionNumLabel, 0);

        instructionsGrid.Children.Add(instructionsEntry);
        instructionsGrid.SetRow(instructionsEntry, instructionRowNum);
        instructionsGrid.SetColumn(instructionsEntry, 1);

        instructionRowNum++;
    }
}