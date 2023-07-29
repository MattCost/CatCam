
using CatCam.EdgeModules.FileWatcher;
using CatCam.EdgeModules.FileWatcher.Services;

internal class Program
{
    private static void Main(string[] args)
    {
        IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services => 
            {
                services.AddHostedService<ModuleBackgroundService>();
                services.AddSingleton<IModuleClientWrapper, DebugModuleClientWrapper>();
                // services.AddSingleton<IModuleClientWrapper, ModuleClientWrapper>();
            })
            .Build();

        host.Run();
    }
}