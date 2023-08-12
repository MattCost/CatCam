using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;

namespace CatCam.EdgeCommon.Services.Clients;

public interface IModuleClientWrapper
{
    public Task OpenAsync(CancellationToken cancellationToken);
    public Task CloseAsync(CancellationToken cancellationToken);
    public Task<Twin> GetTwinAsync(CancellationToken cancellationToken);
    public Task SetDesiredPropertyUpdateCallbackAsync(DesiredPropertyUpdateCallback callback, object userContext, CancellationToken cancellationToken);
    public Task SendEventAsync(string outputName, Message message, CancellationToken cancellationToken);    
    public void SetConnectionStatusChangesHandler(ConnectionStatusChangesHandler statusChangesHandler);    
    public Task SetInputMessageHandlerAsync(string name, MessageHandler messageHandler, object userContext);
    public Task SendMessage(string channel, object message, CancellationToken cancellationToken);

}