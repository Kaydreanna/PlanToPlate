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
        public int ListId { get; private set; }
        [Indexed]
        public int UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string IngredientsJson { get; set; }
        [Ignore]
        public List<Ingredient> Ingredients 
        { 
            get => string.IsNullOrEmpty(IngredientsJson)
                ? new List<Ingredient>()
                : JsonConvert.DeserializeObject<List<Ingredient>>(IngredientsJson); 
            set => IngredientsJson = JsonConvert.SerializeObject(value); 
        }
        public string ListForShoppingJson { get; set; }
        [Ignore]
        public List<Ingredient> ListForShopping 
        { 
            get => string.IsNullOrEmpty(ListForShoppingJson)
                ? new List<Ingredient>()
                : JsonConvert.DeserializeObject<List<Ingredient>>(ListForShoppingJson); 
            set => ListForShoppingJson = JsonConvert.SerializeObject(value); 
        }

    }
}
