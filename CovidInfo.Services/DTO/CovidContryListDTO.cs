using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CovidInfo.Services.DTO {
    public class CovidContryListDTO {

        [JsonPropertyName("data")]

        public List<CovidCountryDTO> Data { get; set; }
    }
}
