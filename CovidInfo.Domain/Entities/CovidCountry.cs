using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace CovidInfo.Domain.Entities {
    public class CovidCountry {

        [BsonId]
        [BsonIgnoreIfDefault]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Country { get; set; }
        public int? Cases { get; set; }
        public int? Deaths { get; set; }
        public int? Recovered { get; set; }
        public int? CasesToday { get; set; }
        public int? DeathsToday { get; set; }
        public int? Suspects { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? Date { get; set; }
        public DateTime InsertedDate = DateTime.Now;
    }
}
