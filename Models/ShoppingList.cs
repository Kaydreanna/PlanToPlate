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
        public List<Ingredient> Ingredients { get; set; }
        public List<Ingredient> ListForShopping { get; set; }

    }
}
