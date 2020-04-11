using System;
using System.Text.Json.Serialization;

namespace CovidInfo.Services.DTO {
    public class CovidCountryDTO {

        [JsonPropertyName("country")]
        public String Country { get; set; }

        [JsonPropertyName("uf")]
        public String Uf { get; set; }

        [JsonPropertyName("state")]
        public String State { get; set; }
        [JsonPropertyName("cases")]
        public int Cases { get; set; }
        [JsonPropertyName("confirmed")]
        public int Confirmed { get; set; }
        [JsonPropertyName("deaths")]
        public int Deaths { get; set; }
        [JsonPropertyName("recovered")]
        public int Recovered { get; set; }
        [JsonPropertyName("suspects")]
        public int Suspects { get; set; }


        [JsonPropertyName("datetime")]
        public DateTime Date { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}
