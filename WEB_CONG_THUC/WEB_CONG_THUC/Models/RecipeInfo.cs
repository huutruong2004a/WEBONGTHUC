using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace WEB_CONG_THUC.Models
{
    public class RecipeInfo
    {
        [JsonPropertyName("DishName")]
        public string? DishName { get; set; }

        [JsonPropertyName("Ingredients")]
        public List<string>? Ingredients { get; set; }

        [JsonPropertyName("Instructions")]
        public List<string>? Instructions { get; set; }

        [JsonPropertyName("PreparationTime")]
        public string? PreparationTime { get; set; }

        [JsonPropertyName("CookingTime")]
        public string? CookingTime { get; set; }

        [JsonPropertyName("Servings")]
        public string? Servings { get; set; }
    }
}