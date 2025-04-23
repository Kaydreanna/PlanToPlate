using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanToPlate.Models
{
    public class Recipe
    {
        [PrimaryKey, AutoIncrement]
        public int RecipeId { get; private set; }
        [Indexed]
        public int UserId { get; set; }
        public string RecipeName { get; set; }
        public string RecipeType { get; set; }
        public string CookingDevice { get; set; }
        public string IngredientsJson { get; set; }
        [Ignore]
        public Dictionary<Ingredient, (decimal Quantity, string Unit)> Ingredients 
        {
            get => string.IsNullOrEmpty(IngredientsJson)
                ? new Dictionary<Ingredient, (decimal, string)>()
                : JsonConvert.DeserializeObject<Dictionary<Ingredient, (decimal, string)>>(IngredientsJson);
            set => IngredientsJson = JsonConvert.SerializeObject(value); 
        }
        public string InstructionsJson { get; set; }
        [Ignore]
        public Dictionary<int, string> Instructions
        {
            get => string.IsNullOrEmpty(InstructionsJson)
                ? new Dictionary<int, string>()
                : JsonConvert.DeserializeObject<Dictionary<int, string>>(InstructionsJson);
            set => InstructionsJson = JsonConvert.SerializeObject(value);
        }
    }
}
