using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanToPlate.Models
{
    public class Taste : Rating
    {
        public int TasteScore { get; set; }
        public string TasteComment { get; set;
    }
}
