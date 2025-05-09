using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanToPlate.Models
{
    public class Ingredient
    {
        [PrimaryKey, AutoIncrement]
        public int IngredientId { get; private set; }
        public int UserId { get; set; }
        public string IngredientName { get; set; }
    }
}
