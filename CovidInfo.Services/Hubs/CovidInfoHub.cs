using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CovidInfo.Services.Hubs {
    public class CovidInfoHub: Hub {

        private readonly CovidService _service;

        public CovidInfoHub(CovidService service) {
            _service = service;
        }   

        public async Task GetCovidInfo(string country) {
            Console.WriteLine(country);
            var covidInfo = await _service.GetCovidInfo(country);
            await Clients.All.SendAsync("ReceiveCovidInfo", covidInfo);
        }
    }
}
