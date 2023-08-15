

using System.Runtime.Serialization.Formatters;
using CatCam.EdgeCommon.Modules;
using CatCam.EdgeCommon.Services.Clients;
using CatCam.EdgeCommon.Services.FileUpload;
using CatCam.EdgeModules.FileWatcher;
using CatCat.EdgeCommon.Services.Clients;

internal class Program
{
    private static async Task Main(string[] args)
    {
        IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices( (context, services) => 
            {
                ConfigureService(context.HostingEnvironment, context.Configuration, services);
            })
            .ConfigureServices( services => services.AddHostedService<EdgeModuleWorker>())
            .Build();

        await host.RunAsync();
    }

    private static void ConfigureService(IHostEnvironment hostEnvironment, IConfiguration configuration, IServiceCollection services)
    {
        services.AddSingleton<IEdgeModule, FileWatcherEdgeModule>();
        
        if(hostEnvironment.IsDevelopment())
        {
            services.Configure<FileWatcherConfiguration>((options) =>
            {
                options.WatchPath = "/tmp/watchMe";
                options.AlwaysUpload = true;
                options.SendMessage = true;
            });
            services.AddSingleton<IModuleClientWrapper, DebugModuleClientWrapper<FileWatcherConfiguration>>();
            services.AddSingleton<IFileUploadService, DebugUploadService>();
        }

        if(hostEnvironment.IsProduction())
        {
            services.AddSingleton<IModuleClientWrapper, ModuleClientWrapper>();
            services.AddSingleton<IDeviceClientWrapper, DeviceClientWrapper>();
            services.AddSingleton<IFileUploadService, BlobStorageUploadService>();
       }
    }
}