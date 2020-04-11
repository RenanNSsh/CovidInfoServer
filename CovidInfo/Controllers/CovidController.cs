using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CovidInfo.Domain.Entities;
using CovidInfo.Services;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CovidInfo.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class CovidController : ControllerBase {


        private readonly ILogger<CovidController> _logger;
        private readonly CovidService _service;

        public CovidController(ILogger<CovidController> logger, CovidService service) {
            _logger = logger;
            _service = service;
        }

        [HttpGet]
        public string Get() {
            return "ola";

        }


        [HttpPost("{country}/{date}")]
        public async Task<CovidCountry> GetCountry(string country, string date) {

            CovidCountry covid = await _service.InsertCountryData(country, date);
            return covid;
        }

        [HttpPost("{country}")]
        public async Task<string> GetCountries(string country) {

            await _service.InsertAllCountryData(country);
            return "Tarefa realizada com sucesso";
        }


        [HttpPost]
        public async Task<string> InsertAllCountriesData() {
            await _service.InsertAllCountriesData();
            return "Inserido com Sucesso";
        }

        [HttpPut("today/{country}")]
        public async Task<string> UpdateCountryToday(string country) {
            await _service.UpdateTodayData(country);
            return "Tarefa realizada com sucesso";
        
        }

        [HttpGet("today/{country}")]
        public async Task<JsonResult> GetCountryToday(string country) {
            var covidInfo = await _service.GetCovidInfo(country);
            if (covidInfo == null) {
                return new JsonResult(new object());
            }
            return new JsonResult(covidInfo);

        }

        [HttpGet("{country}")]
        public async Task<List<CovidCountry>> GetAllCountry(string country, [FromQuery(Name = "startDate")] DateTime startDate, [FromQuery(Name = "endDate")] DateTime endDate) {
            var covidInfo = await _service.GetAllCovidInfo(country, startDate,endDate);
            return covidInfo;

        }

        [HttpPut("countries")]
        public async Task<List<Country>> UpdateCountries() {
            var covidInfo = await _service.UpdateCountries();
            return covidInfo;

        }

        [HttpGet("countries")]
        public async Task<List<Country>> GetCountries() {
            var covidInfo = await _service.GetCountries();
            return covidInfo;

        }


        [HttpPost("run")]
        public async Task<string> UpdateAllCovidInfo() {
            var countries = await _service.GetCountries();

            foreach (var country in countries) {
                await _service.UpdateTodayData(country.Name);
            }

            return "finalizada atualizacao";

        }
    }
}
