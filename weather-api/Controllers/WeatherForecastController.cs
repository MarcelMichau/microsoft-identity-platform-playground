using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace WeatherApi.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class WeatherForecastController(ITokenAcquisition tokenAcquisition, IHttpClientFactory clientFactory)
    : ControllerBase
{
    private static readonly string[] ScopesToAccessDownstreamApi =
        ["api://57ffc7f3-78f6-41ee-ba0b-c7592a21502d/Summary.Read"];

    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    [HttpGet]
    [Authorize(Policy = "RequireAnyRole")]
    [AuthorizeForScopes(Scopes = new[] { DelegatedPermissions.ReadBasicWeather })]
    public IEnumerable<WeatherForecast> Get()
    {
        return GenerateForecasts(5);
    }

    [HttpGet("lots")]
    [Authorize(Roles = AppRoles.ReadLotsOfWeather)]
    [AuthorizeForScopes(Scopes = new[] { DelegatedPermissions.ReadLotsOfWeather })]
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

    [HttpGet("special")]
    [AuthorizeForScopes(Scopes = new[] { DelegatedPermissions.ReadSpecialWeather })]
    public async Task<IEnumerable<WeatherForecast>> GetSpecial()
    {
        var accessToken = await tokenAcquisition.GetAccessTokenForUserAsync(ScopesToAccessDownstreamApi);

        var client = clientFactory.CreateClient();
        client.SetBearerToken(accessToken);

        var specialSummary = await client.GetStringAsync("https://localhost:6001/Summary");

        var forecasts = GenerateForecasts(5).ToList();

        foreach (var forecast in forecasts)
        {
            forecast.Summary = specialSummary;
        }

        return forecasts;
    }
}