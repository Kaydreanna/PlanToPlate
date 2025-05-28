using Newtonsoft.Json;
using PlanToPlate.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanToPlate.Services
{
    class DatabaseService
    {
        #region Login Methods
        public static async Task<User> AuthenticateUser(string emailOrUsername, string password)
        {
            await Init();
            var user = await _db.Table<User>().Where(u => u.Email == emailOrUsername && u.Password == password).FirstOrDefaultAsync();
            if(user == null)
            {
                user = await _db.Table<User>().Where(u => u.Username == emailOrUsername && u.Password == password).FirstOrDefaultAsync();
            }
            return user;
        }
        public static async Task<User> CreateUserAccount(string username, string email, string password)
        {
            await Init();
            var user = new User()
            {
                Username = username,
                Email = email,
                Password = password
            };
            await _db.InsertAsync(user);
            return user;
        }
        public static async Task<bool> UniqueUsername(string username)
        {
            await Init();
            var users = await _db.Table<User>().ToListAsync();
            foreach(User user in users)
            {
                if (user.Username == username)
                {
                    return false;
                }
            }
            return true;
        }

        public static async Task<bool> EmailExists(string email)
        {
            await Init();
            var user = await _db.Table<User>().Where(u => u.Email == email).FirstOrDefaultAsync();
            return user != null;
        }
        #endregion

        #region Recipe Methods
        public static async Task<List<Recipe>> GetAllRecipes(int userId)
        {
            await Init();
            List<Recipe> recipes = await _db.Table<Recipe>().Where(i => i.UserId == userId).ToListAsync();
            return recipes;
        }

        public static async Task<Dictionary<Recipe, float>> GetRecipesAndRatings(int userId)
        {
            await Init();
            List<Recipe> recipes = await _db.Table<Recipe>().Where(i => i.UserId == userId).ToListAsync();
            Dictionary<Recipe, float> recipeRatings = new Dictionary<Recipe, float>();
            foreach (Recipe recipe in recipes)
            {
                var ratings = await GetRatingScores(recipe.RecipeId);
                recipeRatings.Add(recipe, ratings.overall);
            }
            return recipeRatings;
        }

        public static async Task<Dictionary<Recipe, float>> GetRecipesAndRatings(List<Recipe> recipes)
        {
            await Init();
            Dictionary<Recipe, float> recipeRatings = new Dictionary<Recipe, float>();
            foreach (Recipe recipe in recipes)
            {
                var ratings = await GetRatingScores(recipe.RecipeId);
                recipeRatings.Add(recipe, ratings.overall);
            }
            return recipeRatings;
        }
        public static async Task<(float overall, int ease, int taste, int timing)> GetRatingScores(int recipeId)
        {
            await Init();

            List<Ease> easeRatings = await _db.Table<Ease>().Where(i => i.RecipeId == recipeId).ToListAsync();
            List<Taste> tasteRatings = await _db.Table<Taste>().Where(i => i.RecipeId == recipeId).ToListAsync();
            List<Timing> timingRatings = await _db.Table<Timing>().Where(i => i.RecipeId == recipeId).ToListAsync();

            float overallRating;
            int numOfRatings = easeRatings.Count + tasteRatings.Count + timingRatings.Count;
            if (numOfRatings == 0)
            {
                return (-1, -1, -1, -1);
            }
            else
            {
                overallRating = 0;
            }

            int totalEaseRating = easeRatings.Count > 0 ? 0 : -1;
            int totalTasteRating = tasteRatings.Count > 0 ? 0 : -1;
            int totalTimingRating = timingRatings.Count > 0 ? 0 : -1;


            foreach (Ease easeRating in easeRatings)
            {
                overallRating += easeRating.EaseScore;
                totalEaseRating += easeRating.EaseScore;
            }
            foreach (Taste tasteRating in tasteRatings)
            {
                overallRating += tasteRating.TasteScore;
                totalTasteRating += tasteRating.TasteScore;
            }
            foreach (Timing timingRating in timingRatings)
            {
                overallRating += timingRating.TimeScore;
                totalTimingRating += timingRating.TimeScore;
            }

            overallRating /= numOfRatings;

            return (overallRating, totalEaseRating, totalTasteRating, totalTimingRating);
        }

        public static async Task<(List<Ease> easeRatings, List<Taste> tasteRatings, List<Timing> timingRatings)> GetRatings(int recipeId)
        {
            await Init();
            List<Ease> easeRatings = await _db.Table<Ease>().Where(i => i.RecipeId == recipeId).ToListAsync();
            List<Taste> tasteRatings = await _db.Table<Taste>().Where(i => i.RecipeId == recipeId).ToListAsync();
            List<Timing> timingRatings = await _db.Table<Timing>().Where(i => i.RecipeId == recipeId).ToListAsync();
            return (easeRatings, tasteRatings, timingRatings);
        }

        public static async Task DeleteEaseRating(Ease rating)
        {
            await Init();
            await _db.DeleteAsync(rating);
        }

        public static async Task DeleteTasteRating(Taste rating)
        {
            await Init();
            await _db.DeleteAsync(rating);
        }

        public static async Task DeleteTimingRating(Timing rating)
        {
            await Init();
            await _db.DeleteAsync(rating);
        }

        public static async Task<List<Ingredient>> GetIngredients(int userId)
        {
            await Init();
            List<Ingredient> ingredients = await _db.Table<Ingredient>().Where(i => i.UserId == userId).OrderBy(i => i.IngredientName).ToListAsync();
            return ingredients;
        }

        public static async Task AddIngredients(int userId, List<string> ingredientNames)
        {
            await Init();
            var existingIngredients = (await _db.Table<Ingredient>().Where(i => i.UserId == userId).ToListAsync()).Select(i => i.IngredientName.ToLower()).ToHashSet();
            foreach (string ing in ingredientNames)
            {
                if(ing == null)
                {
                    continue;
                }
                string lowerCaseIng = ing.Trim().ToLower();
                if(string.IsNullOrEmpty(lowerCaseIng) || existingIngredients.Contains(lowerCaseIng))
                {
                        continue;
                }
                Ingredient ingredient = new Ingredient() { IngredientName = lowerCaseIng, UserId = 0 };
                await _db.InsertAsync(ingredient);
                existingIngredients.Add(lowerCaseIng);
            }
        }

        public static async Task AddRecipe(Recipe newRecipe)
        {
            await Init();
            await _db.InsertAsync(newRecipe);
        }

        public static async Task UpdateRecipe(Recipe recipeToUpdate, Recipe updatedRecipe)
        {
            await Init();
            recipeToUpdate.RecipeName = updatedRecipe.RecipeName;
            recipeToUpdate.RecipeType = updatedRecipe.RecipeType;
            recipeToUpdate.CookingDevice = updatedRecipe.CookingDevice;
            recipeToUpdate.Ingredients = updatedRecipe.Ingredients;
            recipeToUpdate.Instructions = updatedRecipe.Instructions;
            await _db.UpdateAsync(recipeToUpdate);
        }

        public static async Task DeleteRecipe(int recipeId)
        {
            await Init();
            await _db.DeleteAsync<Recipe>(recipeId);
        }

        public static async Task AddEaseRating(Ease easeRating)
        {
            await Init();
            await _db.InsertAsync(easeRating);
        }

        public static async Task AddTasteRating(Taste tasteRating)
        {
            await Init();
            await _db.InsertAsync(tasteRating);
        }

        public static async Task AddTimingRating(Timing timingRating)
        {
            await Init();
            await _db.InsertAsync(timingRating);
        }
        #endregion

        #region Shopping List Methods
        public static async Task<List<ShoppingList>> GetAllShoppingLists(int userId)
        {
            await Init();
            List<ShoppingList> unsortedShoppingLists = await _db.Table<ShoppingList>().Where(i => i.UserId == userId).ToListAsync();
            return unsortedShoppingLists.OrderByDescending(list => list.StartDate).ToList();
        }

        public static async Task<ShoppingList> GetShoppingListById(int shoppingListId)
        {
            await Init();
            return await _db.Table<ShoppingList>().Where(i => i.ListId == shoppingListId).FirstOrDefaultAsync();
        }

        public static async Task<List<ScheduledMeals>> GetScheduledMealsByDate(int userId, DateTime startDate, DateTime endDate)
        {
            await Init();
            return await _db.Table<ScheduledMeals>().Where(i => i.UserId == userId && i.Date >= startDate && i.Date <= endDate).ToListAsync();
        }

        public static async Task<List<string>> GetRecipeIngredients(int recipeId)
        {
            await Init();
            var recipe = await _db.Table<Recipe>().Where(i => i.RecipeId == recipeId).FirstOrDefaultAsync();
            if (recipe != null)
            {
                return recipe.Ingredients.Keys.ToList();
            }
            return new List<string>();
        }

        public static async Task CreateShoppingList(ShoppingList shoppingList)
        {
            await Init();
            await _db.InsertAsync(shoppingList);
        }

        public static async Task<ShoppingList> CreateShoppingList(int userId, DateTime startDate, DateTime endDate)
        {
            await Init();
            List<ScheduledMeals> scheduledMeals = await _db.Table<ScheduledMeals>().Where(i => i.UserId == userId && i.Date >= startDate && i.Date <= endDate).ToListAsync();
            Dictionary<string, bool> ingredientsToAddToShoppingList = new Dictionary<string, bool>();
            foreach (ScheduledMeals meal in scheduledMeals)
            {
                List<string> ingredients = await DatabaseService.GetRecipeIngredients(meal.RecipeId);
                foreach (string ingredient in ingredients)
                {
                    if(!ingredientsToAddToShoppingList.ContainsKey(ingredient))
                    {
                        ingredientsToAddToShoppingList.Add(ingredient, false);
                    }
                }
            }

            ShoppingList newShoppingList = new ShoppingList
            {
                UserId = userId,
                StartDate = startDate,
                EndDate = endDate,
                IngredientList = new Dictionary<string, bool>()
            };

            await _db.InsertAsync(newShoppingList);
            await UpdateShoppingList(newShoppingList, ingredientsToAddToShoppingList);

            return newShoppingList;
        }

        public static async Task UpdateShoppingList(ShoppingList shoppingList, Dictionary<string, bool> ingredients)
        {
            await Init();
            shoppingList.IngredientList = ingredients;
            shoppingList.IngredientListJson = JsonConvert.SerializeObject(ingredients);
            await _db.UpdateAsync(shoppingList);
        }

        public static async Task UpdateShoppingLists(int userId, DateTime mealDate)
        {
            List<ShoppingList> allShoppingLists = await GetAllShoppingLists(userId);
            foreach (ShoppingList shoppingList in allShoppingLists)
            {
                if (mealDate >= shoppingList.StartDate && mealDate <= shoppingList.EndDate)
                {
                    Dictionary<string, bool> updatedIngredientList = new Dictionary<string, bool>();
                    List<Recipe> scheduledRecipes = new List<Recipe>();
                    List<ScheduledMeals> scheduledMeals = await _db.Table<ScheduledMeals>().Where(i => i.UserId == shoppingList.UserId && i.Date >= shoppingList.StartDate && i.Date <= shoppingList.EndDate).ToListAsync();
                    //Loop through all the scheduled meals that fall within the shopping list date range
                    foreach (ScheduledMeals meal in scheduledMeals)
                    {
                        //Get ingredients for each meal
                        List<string> mealIngredients = await GetRecipeIngredients(meal.RecipeId);
                        //Loop through each ingredient
                        foreach (string ingredient in mealIngredients)
                        {
                            //Check if the ingredient is already in the updated list
                            if (!updatedIngredientList.ContainsKey(ingredient))
                            {
                                //If it is not, check if the ingredient is in the shopping list
                                if (shoppingList.IngredientList.ContainsKey(ingredient))
                                {
                                    //If so, add it to the updated list with its matching value
                                    updatedIngredientList[ingredient] = shoppingList.IngredientList[ingredient];
                                }
                                //If it is not in the shopping list, add it to the updated list with a value of false
                                else
                                {
                                    updatedIngredientList[ingredient] = false;
                                }
                            }
                        }
                    }
                    shoppingList.IngredientList = updatedIngredientList;
                    shoppingList.IngredientListJson = JsonConvert.SerializeObject(updatedIngredientList);
                    await _db.UpdateAsync(shoppingList);
                }
            }
        }

        public static async Task DeleteShoppingList(ShoppingList shoppingList)
        {
            await Init();
            await _db.DeleteAsync(shoppingList);
        }
        #endregion

        #region Home Methods
        public static async Task<Recipe> GetRecipe(int userId, int recipeId)
        {
            await Init();
            return await _db.Table<Recipe>().Where(i => i.UserId == userId && i.RecipeId == recipeId).FirstOrDefaultAsync();
        }
        #endregion

        #region Schedule Meals Methods
        public static async Task<int> GetRecipeId(int userId, string recipeName)
        {
            await Init();
            var recipe = await _db.Table<Recipe>().Where(r => r.RecipeName == recipeName && r.UserId == userId).FirstOrDefaultAsync();
            if (recipe != null)
            {
                return recipe.RecipeId;
            }
            else
            {
                return -1; // or throw an exception
            }
        }

        public static async Task ScheduleMeal(ScheduledMeals scheduledMeal)
        {
            await Init();
            await _db.InsertAsync(scheduledMeal);
        }

        public static async Task<List<ScheduledMeals>> GetScheduledMeals(int userId, DateTime startDate, DateTime endDate)
        {
            await Init();
            List<ScheduledMeals> scheduledMeals = await _db.Table<ScheduledMeals>().Where(i => i.UserId == userId && i.Date >= startDate && i.Date <= endDate).ToListAsync();
            return scheduledMeals;
        }

        public static async Task<List<ScheduledMeals>> GetScheduledMeals(int userId, DateTime date)
        {
            await Init();
            List<ScheduledMeals> scheduledMeals = await _db.Table<ScheduledMeals>().Where(i => i.UserId == userId && i.Date == date).ToListAsync();
            return scheduledMeals;
        }

        public static async Task DeleteScheduleMeal(ScheduledMeals meal)
        {
            await Init();
            await _db.DeleteAsync(meal);
        }

        public static async Task<string> GetRecipeName(ScheduledMeals meal)
        {
            await Init();
            if (meal == null)
            {
                return null;
            } else
            {
                var recipeId = meal.RecipeId;
                var recipe = await _db.Table<Recipe>().Where(r => r.RecipeId == recipeId).FirstOrDefaultAsync();
                return recipe.RecipeName;
            }
        }
        #endregion

        #region Settings Methods
        public static async Task ChangePassword(User user, string newPassword)
        {
            await Init();
            user.Password = newPassword;
            await _db.UpdateAsync(user);
        }

        public static async Task ChangeUsername(User user, string newUsername)
        {
            await Init();
            user.Username = newUsername;
            await _db.UpdateAsync(user);
        }

        public static async Task DeleteAccount(User user)
        {
            await Init();
            await _db.DeleteAsync(user);
        }
        #endregion

        #region Starting Data
        public static async Task ClearStartingData()
        {
            await Init();
            await _db.DeleteAllAsync<ScheduledMeals>();
            await _db.DeleteAllAsync<Ease>();
            await _db.DeleteAllAsync<Ingredient>();
            await _db.DeleteAllAsync<Rating>();
            await _db.DeleteAllAsync<Recipe>();
            await _db.DeleteAllAsync<ShoppingList>();
            await _db.DeleteAllAsync<Taste>();
            await _db.DeleteAllAsync<Timing>();
            await _db.DeleteAllAsync<User>();
        }

        public static async Task<bool> IsThereATestUser()
        {
            await Init();

            var count = await _db.Table<User>().CountAsync();

            return count > 0;
        }

        public static async Task LoadStartingData()
        {
            await Init();

            User user1 = new User()
            {
                Username = "Test",
                Password = "Test",
                Email = "test@email.com"
            };
            await _db.InsertAsync(user1);

            int user1Id = user1.UserId;

            List<string> ingredientsToAdd = new List<string> { "Milk", "Water", "Vanilla Extract", "Rolled Oats", "Cinnamon", "Honey", "Bowtie Pasta", "Kielbasa", "Olive Oil", "Minced Garlic", "Onion", "Red Bell Pepper", "Parmasen Cheese" };
            foreach (string ing in ingredientsToAdd)
            {
                string lowerCaseIng = ing.ToLower();
                Ingredient ingredient = new Ingredient() { IngredientName = lowerCaseIng, UserId = user1Id };
                await _db.InsertAsync(ingredient);
            }

            ShoppingList user1ShoppingList = new ShoppingList()
            {
                UserId = user1Id,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(6),
                IngredientList = new Dictionary<string, bool>{ 
                    { "Milk", false }, 
                    { "Water", false }, 
                    { "Vanilla Extract", false }, 
                    { "Rolled Oats", false }, 
                    { "Cinnamon", false }, 
                    { "Honey", false }, 
                    { "Bowtie Pasta", false }, 
                    { "Kielbasa", false }, 
                    { "Olive Oil", false }, 
                    { "Minced Garlic", false }, 
                    { "Onion", false }, 
                    { "Red Bell Pepper", false },
                    { "Parmasen Cheese", false } 
                }
            };
            await _db.InsertAsync(user1ShoppingList);

            Recipe oatmeal = new Recipe()
            {
                UserId = user1Id,
                RecipeName = "Oatmeal",
                RecipeType = "Breakfast",
                CookingDevice = "Microwave",
                Ingredients = new Dictionary<string, (string Quantity, string Unit)>()
                {
                    { "Milk", ("1", "Cup") },
                    { "Water", ("1", "Cup") },
                    { "Vanilla Extract", ("1/2", "tsp") },
                    { "Rolled Oats", ("1", "Cup") },
                    { "Cinnamon", ("1/2", "tsp") },
                    { "Honey", ("1", "Tbsp") },
                },
                Instructions = new Dictionary<int, string>()
                {
                    {1, "Combine Milk, Water, and Vanilla Extract in a microwave safe bowl. Stir in Rolled Oats" },
                    {2, "Microwave on high for 2-3 minutes, stirring halfway through" },
                    {3, "Stir in Cinnamon and Honey" },
                    {4, "Let sit for 1 minute before serving" }
                }
            };
            await _db.InsertAsync(oatmeal);

            Ease oatmealEaseRating = new Ease()
            {
                UserId = user1Id,
                RecipeId = oatmeal.RecipeId,
                Date = new DateTime(2023, 10, 1),
                EaseScore = 4,
                EaseComment = "This was easy to make, but I had to microwave it twice to get the oats soft enough."
            };

            Timing oatmealTimingRating = new Timing()
            {
                UserId = user1Id,
                RecipeId = oatmeal.RecipeId,
                Date = new DateTime(2023, 10, 1),
                TimeScore = 5,
                TimeComment = "It's awesome to have a meal in a couple minutes!",
                AmountOfTime = "Less than 15 minutes"
            };

            Taste oatmealTasteRating = new Taste()
            {
                UserId = user1Id,
                RecipeId = oatmeal.RecipeId,
                Date = new DateTime(2023, 10, 1),
                TasteScore = 3,
                TasteComment = "It was alright, but I think I would like it better with some fruit."
            };
            await _db.InsertAsync(oatmealEaseRating);
            await _db.InsertAsync(oatmealTimingRating);
            await _db.InsertAsync(oatmealTasteRating);

            Recipe kielbassaPasta = new Recipe()
            {
                UserId = user1Id,
                RecipeName = "Kielbassa Pasta",
                RecipeType = "Dinner",
                CookingDevice = "Stovetop",
                Ingredients = new Dictionary<string, (string Quantity, string Unit)>()
                {
                    { "Bowtie Pasta", ("16", "oz") },
                    { "Kielbassa", ("1", "Whole") },
                    { "Olive Oil", ("2", "Tbsp") },
                    { "Minced Garlic", ("2", "tsp") },
                    { "Onion", ("1", "Medium") },
                    { "Red Bell Pepper", ("1", "Whole") },
                    { "Parmesan Cheese", ("1/2", "Cup") },
                },
                Instructions = new Dictionary<int, string>()
                {
                    {1, "Cook Pasta according to package instructions" },
                    {2, "In a large skillet, heat 1 Tbsp Olive Oil over medium heat. Add Onion and Red Bell Pepper and cook until softened" },
                    {3, "Add Garlic and cook for one minute or until fragrant. Add Kielbassa and cook until browned" },
                    {4, "Strain pasta and add Kielbasa mixture to pot." },
                    {5, "Add 1 Tbsp of oil and Parmesan Cheese to the pot and toss to combine" }
                }
            };
            await _db.InsertAsync(kielbassaPasta);

            Ease kielbassaPastaEaseRating = new Ease()
            {
                UserId = user1Id,
                RecipeId = kielbassaPasta.RecipeId,
                Date = new DateTime(2023, 10, 1),
                EaseScore = 5,
                EaseComment = "This was super easy to make! I love one pot meals."
            };
            Timing kielbassaPastaTimingRating = new Timing()
            {
                UserId = user1Id,
                RecipeId = kielbassaPasta.RecipeId,
                Date = new DateTime(2023, 10, 1),
                TimeScore = 4,
                TimeComment = "I love meals that take less than an hour to make! Chopping the veggies and kielbassa before had could make it even faster!",
                AmountOfTime = "45 minutes"
            };
            Timing kielbassaPastaTimingRating1 = new Timing()
            {
                UserId = user1Id,
                RecipeId = kielbassaPasta.RecipeId,
                Date = new DateTime(2023, 11, 6),
                TimeScore = 5,
                TimeComment = "Chopping everything before hand made it so much quicker!",
                AmountOfTime = "30 minutes"
            };
            await _db.InsertAsync(kielbassaPastaEaseRating);
            await _db.InsertAsync(kielbassaPastaTimingRating);
            await _db.InsertAsync(kielbassaPastaTimingRating1);
        }
        #endregion

        private static SQLiteAsyncConnection? _db;
        static async Task Init()
        {
            if (_db != null)
            {
                return;
            }
            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "recipes.db");

            //Use the folowing if changes are made to a database

            //if (File.Exists(databasePath))
            //{
            //    File.Delete(databasePath);
            //}

            _db = new SQLiteAsyncConnection(databasePath);

            await _db.CreateTableAsync<ScheduledMeals>();
            await _db.CreateTableAsync<Ease>();
            await _db.CreateTableAsync<Ingredient>();
            await _db.CreateTableAsync<Rating>();
            await _db.CreateTableAsync<Recipe>();
            await _db.CreateTableAsync<ShoppingList>();
            await _db.CreateTableAsync<Taste>();
            await _db.CreateTableAsync<Timing>();
            await _db.CreateTableAsync<User>();
        }
    }
}


//CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title.ToLower()); 