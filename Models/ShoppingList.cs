using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanToPlate.Models
{
    public class ShoppingList
    {
        [PrimaryKey, AutoIncrement]
        public int ListId { get; set; }
        [Indexed]
        public int UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string IngredientListJson { get; set; }
        [Ignore]
        public Dictionary<string, bool> IngredientList 
        { 
            get => string.IsNullOrEmpty(IngredientListJson)
                ? new Dictionary<string, bool>()
                : JsonConvert.DeserializeObject<Dictionary<string, bool>>(IngredientListJson); 
            set => IngredientListJson = JsonConvert.SerializeObject(value); 
        }
    }
}
