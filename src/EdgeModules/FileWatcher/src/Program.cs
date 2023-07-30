
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
                #if DEBUG
                services.AddSingleton<IModuleClientWrapper, DebugModuleClientWrapper>();
                services.AddSingleton<IFileUploadService, DebugUploadService>();
                #else
                services.AddSingleton<IModuleClientWrapper, ModuleClientWrapper>();
                services.AddSingleton<IFileUploadService, BlobStorageUploadService>();
                #endif
            })
            .Build();

        host.Run();
    }
}