using CatCam.EdgeCommon.Messages.Events;

namespace CatCam.EdgeCommon.Modules;

public interface IEdgeModule
{
    public Task Initialize(CancellationToken token);
    public Task Dispose();
    public Task StartProcessing(CancellationToken token);
    public Task StopProcessing(CancellationToken token);
    public Task WaitUntilComplete(CancellationToken token);

    event FatalErrorEventOccurred? FatalErrorOccurred;
}