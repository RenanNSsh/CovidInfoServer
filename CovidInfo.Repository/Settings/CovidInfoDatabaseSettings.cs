using System;
using System.Collections.Generic;
using System.Text;

namespace CovidInfo.Repository.Settings {
    public class CovidInfoDatabaseSettings: ICovidInfoDatabaseSettings {
        public string CovidCountryCollectionName { get; set; }
        public string CountryCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public interface ICovidInfoDatabaseSettings {
        string CovidCountryCollectionName { get; set; }
        string CountryCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }

}
