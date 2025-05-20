using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanToPlate.Models
{
    class Timing : Rating
    {
        public int TimeScore { get; set; }
        public string TimeComment { get; set; }
        public int AmountOfTime { get; set; }
        public string TimeUnit { get; set; }
    }
}