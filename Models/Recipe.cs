﻿using Newtonsoft.Json;
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
        public int RecipeId { get; set; }
        [Indexed]
        public int UserId { get; set; }
        public string RecipeName { get; set; }
        public string RecipeType { get; set; }
        public string CookingDevice { get; set; }
        public string IngredientsJson { get; set; }
        [Ignore]
        public Dictionary<string, (string Quantity, string Unit)> Ingredients 
        {
            get => string.IsNullOrEmpty(IngredientsJson)
                    ? new Dictionary<string, (string, string)>()
                    : JsonConvert.DeserializeObject<Dictionary<string, (string, string)>>(IngredientsJson);
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
