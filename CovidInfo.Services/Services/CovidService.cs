using CovidInfo.Domain.Entities;
using CovidInfo.Repository.Settings;
using CovidInfo.Services.DTO;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace CovidInfo.Services {
    public class CovidService {
        private static readonly HttpClient client = new HttpClient();
        private readonly IMongoCollection<CovidCountry> _collection;
        private readonly IMongoCollection<Country> _countryCollection;
        public CovidService(ICovidInfoDatabaseSettings settings) {
            var mongoClient = new MongoClient(settings.ConnectionString);
            var database = mongoClient.GetDatabase(settings.DatabaseName);
            _collection = database.GetCollection<CovidCountry>(settings.CovidCountryCollectionName);
            _countryCollection = database.GetCollection<Country>(settings.CountryCollectionName);

        }

        public async Task<CovidCountry> InsertCountryData(string country, string date) {
            var countryData = await client.GetStreamAsync($"https://covid19-brazil-api.now.sh/api/report/v1/{country}/{date}");
            var countryDTO = await JsonSerializer.DeserializeAsync<CovidContryListDTO>(countryData);
            var covidCountryDbList = countryDTO.Data;
            CovidCountry covidDB = new CovidCountry();
            covidCountryDbList.ForEach(covidCountryDb => {
                covidDB.Date = covidCountryDb.Date;
                covidDB.Cases = covidDB.Cases != null ? covidDB.Cases + covidCountryDb.Cases : covidCountryDb.Cases;
                covidDB.Deaths = covidDB.Deaths != null ? covidDB.Deaths + covidCountryDb.Deaths : covidCountryDb.Deaths;
                covidDB.Suspects = covidDB.Suspects != null ? covidDB.Suspects + covidCountryDb.Suspects : covidCountryDb.Suspects;
            });
            await _collection.InsertOneAsync(covidDB);
            return covidDB;
        }

        public async Task InsertAllCountryData(string country) {
            List<CovidCountryDTO> covidCountryDbList;

            var date = new DateTime(2020, 01, 02);
            var casesYesterday = -1;
            var deathsYesterday = -1;

            do {
                var formatedDate = date.ToString("yyyyMMdd");
                Stream countryData;

                try {
                    countryData = await client.GetStreamAsync($"https://covid19-brazil-api.now.sh/api/report/v1/{country}/{formatedDate}");
                }
                catch (HttpRequestException) {
                    break;
                }

                var countryDTO = await JsonSerializer.DeserializeAsync<CovidContryListDTO>(countryData);
                covidCountryDbList = countryDTO.Data;

                CovidCountry covidDB = new CovidCountry() {
                    Country = country.ToLower()
                };

                covidCountryDbList.ForEach(covidCountryDb => {
                    covidDB.Date = covidCountryDb.Date;
                    covidDB.Cases = covidDB.Cases != null ? covidDB.Cases + covidCountryDb.Cases : covidCountryDb.Cases;
                    covidDB.Deaths = covidDB.Deaths != null ? covidDB.Deaths + covidCountryDb.Deaths : covidCountryDb.Deaths;
                    covidDB.Suspects = covidDB.Suspects != null ? covidDB.Suspects + covidCountryDb.Suspects : covidCountryDb.Suspects;
                });

                covidDB.CasesToday = covidDB.Cases != null && casesYesterday != -1 ? covidDB.Cases - casesYesterday : null;
                covidDB.DeathsToday = covidDB.Deaths != null && deathsYesterday != -1 ? covidDB.Deaths - deathsYesterday : null;

                if (covidDB.Date != null) {
                    await _collection.InsertOneAsync(covidDB);
                }

                casesYesterday = covidDB.Cases != null ? (int)covidDB.Cases : casesYesterday;
                deathsYesterday = covidDB.Deaths != null ? (int)covidDB.Deaths : deathsYesterday;

                date = date.AddDays(1);

            } while (DateTime.Compare(date, DateTime.Now) < 0);
            await UpdateTodayData(country);
        }


            public async Task InsertAllCountriesData() {
                _collection.DeleteMany(FilterDefinition<CovidCountry>.Empty);
                var countries = await GetCountries();

                foreach(var country in countries) {
                    await InsertAllCountryData(country.Name);
                }
            }

            public async Task UpdateTodayData(string country) {
            Stream covidData;
            try {
                covidData = await client.GetStreamAsync($"https://covid19-brazil-api.now.sh/api/report/v1/{country}");
            }
            catch (Exception) {
                return;
            }
            var covidDataDTO = await JsonSerializer.DeserializeAsync<CovidDataDTO>(covidData);
            CovidCountryDTO covidCountryDb = covidDataDTO.Data;
            CovidCountry covidDB = new CovidCountry() {
                Cases = covidCountryDb.Confirmed,
                Deaths = covidCountryDb.Deaths,
                Recovered = covidCountryDb.Recovered,
                Country = covidCountryDb.Country.ToLower(),
                UpdatedAt = covidCountryDb.UpdatedAt,
                Date = DateTime.Today
            };

            if (covidDB.Cases != null) {
                var filterBuilder = Builders<CovidCountry>.Filter;
                var today = DateTime.Today;
                var yesterDay = today.AddDays(-1);
                var filter = filterBuilder.Gte(x => x.Date, yesterDay) &
                    filterBuilder.Lt(x => x.Date, today) &
                    filterBuilder.Eq(x => x.Country, country);

                var results = await _collection.Find<CovidCountry>(filter).Limit(1).ToListAsync();
                var yesterdayInfo = results.FirstOrDefault();

                covidDB.CasesToday = yesterdayInfo != null  && yesterdayInfo.Cases != null ? covidDB.Cases - yesterdayInfo.Cases : null;
                covidDB.DeathsToday = yesterdayInfo != null && yesterdayInfo.Deaths != null ? covidDB.Deaths - yesterdayInfo.Deaths : null;

                var filterReplace = filterBuilder.Gte(x => x.Date, today) &
                    filterBuilder.Lte(x => x.Date, DateTime.Now) &
                    filterBuilder.Eq(x => x.Country, country);

                await _collection.ReplaceOneAsync(filterReplace, covidDB,new ReplaceOptions() { IsUpsert = true});
            }

        }

        public async Task<List<CovidCountry>> GetCovidInfoAll(string country) {
            var documents = await _collection.Find(x => x.Country == country).ToListAsync();
            return documents;

        }

        public async Task<CovidCountry> GetCovidInfo(string country) {
            var todayStart = DateTime.Today;
            var todayNow = DateTime.Now;
            var documentList = await GetCovidInfo(country, todayStart, todayNow, 1);
            return documentList.FirstOrDefault();
        }

        public async Task<List<CovidCountry>> GetCovidInfo(string country, DateTime dateStart, DateTime dateEnd, int limit) {
            var filterBuilder = Builders<CovidCountry>.Filter;

            var filter = filterBuilder.Gte(x => x.Date, dateStart) &
                filterBuilder.Lte(x => x.Date, dateEnd) &
                filterBuilder.Eq(x => x.Country, country.ToLower());

            var documents = await _collection.Find(filter).Limit(limit).SortBy(x=> x.Date).ToListAsync();
            return documents;
        }


            public async Task<List<CovidCountry>> GetAllCovidInfo(string country, DateTime startDate, DateTime endDate) {
            return await GetCovidInfo(country, startDate, endDate, 0);
        }



        public async Task<List<Country>> UpdateCountries() {
            var countryData = await client.GetStreamAsync($"https://covid19-brazil-api.now.sh/api/report/v1/countries");
            var covidCountryListDTO = await JsonSerializer.DeserializeAsync<CovidContryListDTO>(countryData);
            var covidCountryList = covidCountryListDTO.Data;
            List<Country> countryList = covidCountryList.Select(covidCountry => new Country() {
                Name = covidCountry.Country,
                Br = covidCountry.Country == "US" ? "Estados Unidos" : null
            }).ToList();
            await _countryCollection.InsertManyAsync(countryList);
            return countryList;


        }

        public async Task<List<Country>> GetCountries() {
            var documents = await _countryCollection.Find(_ => true).ToListAsync();
            return documents;
        }
    }

}
