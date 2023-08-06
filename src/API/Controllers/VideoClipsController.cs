using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using CatCam.Common.Services.Storage;

namespace CatCap.API.Controllers;

[AllowAnonymous]
// [Authorize]
[ApiController]
[Route("api/[controller]")]
public class VideoClipsController : ControllerBase
{

    private readonly ILogger<VideoClipsController> _logger;
    private readonly IClipBrowser _clipBrowser;
    private readonly IHttpClientFactory _httpClientFactory;

    public VideoClipsController(ILogger<VideoClipsController> logger, IClipBrowser  clipBrowser, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _clipBrowser = clipBrowser;
    }

    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<string>>> Get()
    {
        try
        {
            var output = await _clipBrowser.GetClipNames();
            return Ok(output);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Exception trying to get clip filenames");
            return Problem();
        }
    }

    [HttpGet("camera/{cameraName}")]
    public async Task<ActionResult<IEnumerable<string>>> Get(string cameraName)
    {
        try
        {
            var output = await _clipBrowser.GetClipNames(cameraName);
            return Ok(output);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Exception trying to get clip sas uri");
            return Problem();
        }
    }

    [HttpGet("camera/{cameraName}/clip{clipName}/url")]
    public async Task<ActionResult<string>> Get(string cameraName, string clipName)
    {
        try
        {
            var fileUri = await _clipBrowser.GetClipSasUri(cameraName, clipName);
            return Ok(fileUri);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Exception trying to get clip sas uri");
            return Problem();
        }
    }

//"https://catcamdevsta.blob.core.windows.net/uploads/dev-pi-01/dogcam/20230731_080705.avi?sv=2023-01-03&se=2023-07-31T13%3A55%3A01Z&sr=b&sp=r&sig=g%2BNL5u%2FohDGlJMEM%2Bfv9DJvx7J%2FKnEptk0ziRp4x5Bo%3D"
    [HttpGet("camera/{cameraName}/clip{clipName}/download")]
    public async Task Download(string cameraName, string clipName)
    {
        try
        {
            var fileUri = await _clipBrowser.GetClipSasUri(cameraName, clipName);
            var client = _httpClientFactory.CreateClient();
            var streamResponse = await client.GetStreamAsync(fileUri);

            this.Response.StatusCode = 200;
            this.Response.Headers.Add( HeaderNames.ContentDisposition, $"attachment; filename=\"{clipName}\"" );
            this.Response.Headers.Add( HeaderNames.ContentType, "application/octet-stream"  );
    
            var outputStream = this.Response.Body;
            const int bufferSize = 1 << 10;
            var buffer = new byte[bufferSize];
            while ( true ) {
                var bytesRead = await streamResponse.ReadAsync(buffer.AsMemory(0, bufferSize));
                if ( bytesRead == 0 ) break;
                await outputStream.WriteAsync(buffer.AsMemory(0, bytesRead));
            }
            await outputStream.FlushAsync();

        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Exception trying to get clip sas uri");
        }
    }



}