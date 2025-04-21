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
        public Dictionary<Ingredient, (decimal Quantity, string Unit)> Ingredients { get; set; }
        public Dictionary<int, string> Instructions { get; set; }
    }
}
