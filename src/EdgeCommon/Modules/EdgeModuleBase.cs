using System.Reflection;
using System.Runtime.Loader;
using CatCam.EdgeCommon.Messages.Events;
using CatCam.EdgeCommon.Services.Clients;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;

namespace CatCam.EdgeCommon.Modules;


public abstract class EdgeModuleBase<TConfiguration> : IEdgeModule
    where TConfiguration : EdgeModuleConfigurationBase, new()
{
    private readonly ILogger<EdgeModuleBase<TConfiguration>> _logger;
    protected ILogger Logger => _logger;

    private readonly IModuleClientWrapper _moduleClient;
    protected IModuleClientWrapper ModuleClient => _moduleClient;

    protected TConfiguration Configuration { get; set; } = new TConfiguration();

    protected CancellationTokenSource CancellationTokenSource { get; set; } = new CancellationTokenSource();

    protected bool IsInitialized { get; private set; }

    private Dictionary<string, MessageHandler> _inputMessageHandlers = new Dictionary<string, MessageHandler>();

    public event FatalErrorEventOccurred? FatalErrorOccurred; 

    public EdgeModuleBase(ILogger<EdgeModuleBase<TConfiguration>> logger, IModuleClientWrapper moduleClient)
    {
        _logger = logger;
        _moduleClient = moduleClient;
    }

    public virtual async Task Initialize(CancellationToken token)
    {
        Logger.LogTrace("Entering Initialize");
        try
        {
            await _moduleClient.OpenAsync(token);
            var moduleTwin = await _moduleClient.GetTwinAsync(token) ?? throw new Exception("Init failed");

            Logger.LogDebug("ModuleTwin received. {Twin}", moduleTwin.ToJson());

            await OnDesiredPropertiesUpdate(moduleTwin.Properties.Desired, _moduleClient);

            await _moduleClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertiesUpdate, _moduleClient, token);

            foreach (var handler in _inputMessageHandlers)
            {
                await _moduleClient.SetInputMessageHandlerAsync(handler.Key, handler.Value, _moduleClient);
            }
            IsInitialized = true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception in Initialize!");
        }
    }

    protected virtual async Task OnDesiredPropertiesUpdate(TwinCollection desiredProperties, object userContext)
    {
        var newConfig = System.Text.Json.JsonSerializer.Deserialize<TConfiguration>(desiredProperties.ToJson());
        if (newConfig is null)
        {
            Logger.LogError("Unable to deserialize twinProperties {Properties} to Module Configuration", desiredProperties.ToJson());
            return;
        }
        newConfig.Validate();

        if (IsInitialized)
        {
            await StopProcessing(CancellationToken.None);
        }

        Configuration = newConfig;

        await ProcessUpdatedConfiguration();

        if(IsInitialized)
        {
            await StartProcessing(CancellationToken.None);
        }
    }

    protected virtual async Task ProcessUpdatedConfiguration()
    {
        await Task.CompletedTask;
    }

    protected void SetupMessageHandler(string name, MessageHandler messageHandler)
    {
        if(string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException(nameof(name));
        }
        if(messageHandler is null)
        {
            throw new ArgumentNullException(nameof(messageHandler));
        }
        
        _inputMessageHandlers[name] = messageHandler;
    }

    public virtual async Task StartProcessing(CancellationToken token)
    {
        Logger.LogTrace("Creating new CancellationTokenSource");
        CancellationTokenSource = new CancellationTokenSource();
        await Task.CompletedTask;
    }

    public virtual async Task StopProcessing(CancellationToken token)
    {
        Logger.LogTrace("Cancelling token");
        CancellationTokenSource.Cancel();
        await Task.CompletedTask;
    }

    private CancellationTokenSource _moduleCTS = new CancellationTokenSource();
    public async Task WaitUntilComplete(CancellationToken token)
    {
        // create a new token source, to ensure it's not cancelled.
        _moduleCTS = new CancellationTokenSource();

        // Subscribe to the default unloading event, and if that happens, cancel our token.
        AssemblyLoadContext.Default.Unloading += (ctx) => _moduleCTS.Cancel();

        static void SetComplete(object? s)
        {
            if (s is TaskCompletionSource<bool> tcs)
            {
                tcs.TrySetResult(true);
            }
        }

        // Create a new Task Completion Source.
        var tcs = new TaskCompletionSource<bool>();

        // Call our SetComplete helper when either of the tokens are cancelled.
        _moduleCTS.Token.Register(SetComplete, tcs);
        token.Register(SetComplete, tcs);

        // Wait for the task to complete, which will only happen when a token gets cancelled.
        await tcs.Task;
    }

    public virtual async Task Dispose()
    {
        await _moduleClient.CloseAsync(CancellationToken.None);
        IsInitialized = false;
    }

    protected void OnFatalErrorOccurred(Exception exception, string message, bool shutdown = true)
    {
        FatalErrorOccurred?.Invoke(this, new FatalErrorEventArgs(exception, message));
        if(shutdown)
        {
            _moduleCTS.Cancel();
        }
    }
}