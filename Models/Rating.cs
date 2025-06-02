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

        public static float GetAverageRating(List<int> ratings)
        {
            if (ratings == null || ratings.Count == 0)
                return -1;
            float total = 0;
            foreach (var rating in ratings)
            {
                total += rating;
            }
            return total / ratings.Count;
        }
    }
}
