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
        public static async Task<User> AuthenticateUser(string email, string password)
        {
            await Init();
            var user = await _db.Table<User>().Where(u => u.Email == email && u.Password == password).FirstOrDefaultAsync();
            return user;
        }
        public static async Task<User> CreateUserAccount(string email, string password)
        {
            await Init();
            var user = new User()
            {
                Email = email,
                Password = password
            };
            await _db.InsertAsync(user);
            return user;
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
                Password = "Test",
                Email = "test@email.com"
            };
            await _db.InsertAsync(user1);

            int user1Id = user1.UserId;

            Recipe oatmeal = new Recipe()
            {
                UserId = user1Id,
                RecipeName = "Oatmeal",
                RecipeType = "Breakfast",
                CookingDevice = "Microwave",
                Ingredients = new Dictionary<Ingredient, (decimal Quantity, string Unit)>()
                {
                    { new Ingredient() { IngredientName = "Milk" }, (1, "Cup") },
                    { new Ingredient() { IngredientName = "Water" }, (1, "Cup") },
                    { new Ingredient() { IngredientName = "Vanilla Extract" }, (0.5m, "tsp") },
                    { new Ingredient() { IngredientName = "Rolled Oats" }, (1, "Cup") },
                    { new Ingredient() { IngredientName = "Cinnamon" }, (0.5m, "tsp") },
                    { new Ingredient() { IngredientName = "Honey" }, (1, "Tbsp") },
                },
                Instructions = new Dictionary<int, string>()
                {
                    {1, "Combine Milk, Water, and Vanilla Extract in a microwave safe bowl. Stir in Rolled Oats" },
                    {2, "Microwave on high for 2-3 minutes, stirring halfway through" },
                    {3, "Stir in Cinnamon and Honey" },
                    {4, "Let sit for 1 minute before serving" }
                }
            };

            Recipe kielbassaPasta = new Recipe()
            {
                UserId = user1Id,
                RecipeName = "Kielbassa Pasta",
                RecipeType = "Dinner",
                CookingDevice = "Stovetop",
                Ingredients = new Dictionary<Ingredient, (decimal Quantity, string Unit)>()
                {
                    { new Ingredient() { IngredientName = "Bowtie Pasta" }, (16, "oz") },
                    { new Ingredient() { IngredientName = "Kielbassa" }, (1, "Whole") },
                    { new Ingredient() { IngredientName = "Olive Oil" }, (2, "Tbsp") },
                    { new Ingredient() { IngredientName = "Minced Garlic" }, (2, "tsp") },
                    { new Ingredient() { IngredientName = "Onion" }, (1, "Medium") },
                    { new Ingredient() { IngredientName = "Red Bell Pepper" }, (1, "Whole") },
                    { new Ingredient() { IngredientName = "Parmesan Cheese" }, (0.5m, "Cup") },
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
                await _db.InsertAsync(oatmeal);
                await _db.InsertAsync(kielbassaPasta);
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
