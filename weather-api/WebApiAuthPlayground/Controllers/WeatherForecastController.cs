using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Identity.Web;

namespace WebApiAuthPlayground.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet]
        [Authorize(Roles = AppRoles.ReadWeather)]
        [AuthorizeForScopes(Scopes = new[] { "Api.ReadWrite" })]
        public IEnumerable<WeatherForecast> Get()
        {
            return GenerateForecasts(5);
        }

        [HttpGet("lots")]
        [Authorize(Roles = AppRoles.ReadLotsOfWeather)]
        [AuthorizeForScopes(Scopes = new []{ "Api.ReadWrite" })]
        public IEnumerable<WeatherForecast> GetLots()
        {
            return GenerateForecasts(20);
        }

        private static IEnumerable<WeatherForecast> GenerateForecasts(int count)
        {
            var rng = new Random();
            return Enumerable.Range(1, count).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = Summaries[rng.Next(Summaries.Length)]
                })
                .ToArray();
        }
    }
}
