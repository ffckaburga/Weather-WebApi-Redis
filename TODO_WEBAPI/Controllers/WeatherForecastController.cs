using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TODO_WEBAPI.Models;
using Microsoft.Extensions.Caching.Distributed;
using static Pipelines.Sockets.Unofficial.SocketConnection;

namespace TODO_WEBAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherForecastController : ControllerBase
    {

        private readonly IDistributedCache _distributedCache;
        public WeatherForecastController(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        // tek şehir
        [HttpGet]
        public HavaDurumu Get(string city, string country)
        {
            using (var client = new System.Net.WebClient())
            {
                client.Encoding = System.Text.Encoding.UTF8;
                string responseString = client.DownloadString("http://api.openweathermap.org/data/2.5/weather?q=" + city + "&units=metric&appid=1ef396acb73a49b95b2a829b1b085e31");
                HavaDurumu hava = JsonConvert.DeserializeObject<HavaDurumu>(responseString);
                hava.weather[0].icon = @"http://openweathermap.org/img/w/" + hava.weather[0].icon + ".png";
                return hava;
            }
        }


        [HttpGet]
        [Route("redisSingle")]

        public async Task<IActionResult> RedisSingle(string city)
        {

            HavaDurumu myHava = new HavaDurumu();
            bool IsCached = false;
            string myHavaString = string.Empty;
            myHavaString = await _distributedCache.GetStringAsync(city);
            if (!string.IsNullOrEmpty(myHavaString))
            {
                // loaded data from the redis cache.
                myHava = JsonConvert.DeserializeObject<HavaDurumu>(myHavaString);
                IsCached = true;
            }
            else
            {
                System.Net.WebClient client = new System.Net.WebClient();
                client.Encoding = System.Text.Encoding.UTF8;
                string responseString = client.DownloadString("http://api.openweathermap.org/data/2.5/weather?q=" + city + "&units=metric&appid=1ef396acb73a49b95b2a829b1b085e31");
                HavaDurumu hava = JsonConvert.DeserializeObject<HavaDurumu>(responseString);
                hava.weather[0].icon = @"http://openweathermap.org/img/w/" + hava.weather[0].icon + ".png";

                // loading from code (in real-time from database)
                // then saving to the redis cache 
                myHava = hava;
                IsCached = false;
                await _distributedCache.SetStringAsync(city, responseString);
            }
            return Ok(new { IsCached, myHava });
        }


        [HttpPost]
        [Route("redisMultiple")]

        public async Task<IActionResult> RedisMultiple([FromBody] List<Location> loc)
        {

            List<HavaDurumu> myHavaDurumuList = new List<HavaDurumu>();

            foreach (var item in loc)
            {
                var redisResponse = await _distributedCache.GetStringAsync(item.city);

                if (String.IsNullOrEmpty(redisResponse))
                {
                    using (var client = new System.Net.WebClient())
                    {
                        client.Encoding = System.Text.Encoding.UTF8;
                        var responseString = client.DownloadString("http://api.openweathermap.org/data/2.5/weather?q=" + item.city + "&units=metric&appid=1ef396acb73a49b95b2a829b1b085e31");

                        if (responseString != null)
                        {
                            HavaDurumu hava = JsonConvert.DeserializeObject<HavaDurumu>(responseString);
                            hava.weather[0].icon = @"http://openweathermap.org/img/w/" + hava.weather[0].icon + ".png";
                            myHavaDurumuList.Add(hava);
                        }
                        await _distributedCache.SetStringAsync(item.city, responseString);
                    }
                }
                else
                {
                    var havaDurumu = JsonConvert.DeserializeObject<HavaDurumu>(redisResponse);
                    myHavaDurumuList.Add(havaDurumu);
                }
            }
            return Ok(new { myHavaDurumuList });
        }

        

        //// birden fazla şehir
        //[HttpPost]
        //public List<HavaDurumu> GetWeather([FromBody] List<Location> loc)
        //{
        //    List<HavaDurumu> havaDurumuList = new List<HavaDurumu>();
        //    foreach (var item in loc)
        //    {
        //        using (var client = new System.Net.WebClient())
        //        {
        //            client.Encoding = System.Text.Encoding.UTF8;

        //            var responseString = client.DownloadString("http://api.openweathermap.org/data/2.5/weather?q=" + item.city + "&units=metric&appid=1ef396acb73a49b95b2a829b1b085e31");

        //            if (responseString != null)
        //            {
        //                var hava = JsonConvert.DeserializeObject<HavaDurumu>(responseString);
        //                hava.weather[0].icon = @"http://openweathermap.org/img/w/" + hava.weather[0].icon + ".png";
        //                havaDurumuList.Add(hava);
        //            }

        //        }

        //    }
        //    return havaDurumuList;
        //}



    }
}

//var cachedHava = await _distributedCache.GetAsync("_myHava");
//if (!string.IsNullOrEmpty(cachedHava))
//{
//    // loaded data from the redis cache.
//    myHava = JsonSerializer.Deserialize<List<string>>(cachedHava);
//    IsCached = true;
//}
//else
//{
//    // loading from code (in real-time from database)
//    // then saving to the redis cache 
//    myHava = hava;
//    IsCached = false;
//    cachedTodosString = JsonSerializer.Serialize<List<string>>(todos);
//    await _distributedCache.SetStringAsync("_todos", cachedTodosString);
//}
//return Ok(new { IsCached, myTodos });