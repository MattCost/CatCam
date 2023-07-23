using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Web;
using Microsoft.Identity.Abstractions;
using Microsoft.AspNetCore.Authorization;
using CatCam.Common.Models;

namespace webapp.Pages;

[Authorize]
[AuthorizeForScopes(Scopes = new string[] { "api://catcam-api/api.access" })]
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IDownstreamApi _downstreamApi;

    public List<WeatherForecast> TheWeather = new List<WeatherForecast>();

    public IndexModel(ILogger<IndexModel> logger, IDownstreamApi downstreamApi)
    {
        _logger = logger;
        _downstreamApi = downstreamApi;
    }

    public async Task OnGetAsync()
    {
        _logger.LogTrace("Entering OnGetAsync");
        TheWeather = await _downstreamApi.CallApiForUserAsync<List<WeatherForecast>>
        (
                "TestAPI",
                options => options.RelativePath = "WeatherForecast"
        );
    }
}
