using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanToPlate.Models
{
    public class Rating
    {
        [PrimaryKey, AutoIncrement]
        public int RatingId { get; set; }
        [Indexed]
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        [Indexed]
        public int RecipeId { get; set; }
    }
}
