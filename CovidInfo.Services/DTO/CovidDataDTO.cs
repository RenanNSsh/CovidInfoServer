using System.Text.Json.Serialization;

namespace CovidInfo.Services.DTO {
    class CovidDataDTO {
        [JsonPropertyName("data")]

        public CovidCountryDTO Data { get; set; }
    }
}
