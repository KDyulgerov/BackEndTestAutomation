using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IdeaAPITesting.Models
{
    public class ApiResponseDTO
    {
        [JsonPropertyName("msg")]
        public string Msg { get; set; }

        [JsonPropertyName("id")]
        public string IdeaId { get; set; }

        /*[JsonPropertyName("idea")]
        public IdeaData Idea { get; set; }*/
    }

    /*public class IdeaData
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }
    }*/
}
