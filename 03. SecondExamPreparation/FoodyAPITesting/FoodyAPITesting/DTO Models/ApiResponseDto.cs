using System.Text.Json.Serialization;

namespace FoodyAPITesting.DTO_Models
{
    public class ApiResponseDto
    {
        [JsonPropertyName("msg")]
        public string Message { get; set; }

        [JsonPropertyName("foodId")]
        public string? FoodId { get; set; }
    }


}
