
using CatCam.EdgeCommon.Messages.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CatCam.EdgeCommon.Modules;

public class EdgeModuleWorker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IEdgeModule _edgeModule;
    public EdgeModuleWorker(ILogger<EdgeModuleWorker> logger, IEdgeModule edgeModule)
    {
        _logger = logger;
        _edgeModule = edgeModule;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        _edgeModule.FatalErrorOccurred += LogFatalError;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _edgeModule.Initialize(stoppingToken);
                await _edgeModule.StartProcessing(stoppingToken);
                await _edgeModule.WaitUntilComplete(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception escaped edge module");
            }
            finally
            {
                await _edgeModule.StopProcessing(CancellationToken.None);
                await _edgeModule.Dispose();
            }

            if(!stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug("Module will restart");
            }
        }
        _logger.LogDebug("Module stopping");
    }

    private void LogFatalError(object sender, FatalErrorEventArgs e)
    {
        _logger.LogCritical(e.Exception, e.Message);
    }
}