using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TODO_WEBAPI.Models;

namespace TODO_WEBAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }


        // tek şehir
        [HttpGet]
        public HavaDurumu Get(string city, string country)
        {
            using (var client = new System.Net.WebClient())
            {
                client.Encoding = System.Text.Encoding.UTF8;
                var responseString = client.DownloadString("http://api.openweathermap.org/data/2.5/weather?q=" + city + "," + country + "&units=metric&appid=1ef396acb73a49b95b2a829b1b085e31");
                var hava = JsonConvert.DeserializeObject<HavaDurumu>(responseString);
                hava.weather[0].icon = @"http://openweathermap.org/img/w/" + hava.weather[0].icon + ".png";
                return hava;
            }
        }

        // birden fazla şehir
        [HttpPost]
        public List<HavaDurumu> GetWeather([FromBody] List<Location> loc)
        {
            List<HavaDurumu> havaDurumuList = new List<HavaDurumu>();
            foreach (var item in loc)
            {
                using (var client = new System.Net.WebClient())
                {
                    client.Encoding = System.Text.Encoding.UTF8;

                    var responseString = client.DownloadString("http://api.openweathermap.org/data/2.5/weather?q=" + item.city + "," + item.country + "&units=metric&appid=1ef396acb73a49b95b2a829b1b085e31");

                    if (responseString != null)
                    {
                        var hava = JsonConvert.DeserializeObject<HavaDurumu>(responseString);
                        hava.weather[0].icon = @"http://openweathermap.org/img/w/" + hava.weather[0].icon + ".png";
                        havaDurumuList.Add(hava);
                    }

                }

            }
            return havaDurumuList;
        }



    }
}
