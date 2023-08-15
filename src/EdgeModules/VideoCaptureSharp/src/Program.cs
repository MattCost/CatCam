

using System.Diagnostics;
using System.Runtime.Serialization.Formatters;
using CatCam.EdgeCommon.Modules;
using CatCam.EdgeCommon.Services.Clients;
using CatCam.EdgeCommon.Services.FileUpload;
using CatCam.EdgeModules.VideoCaptureSharp;
using CatCat.EdgeCommon.Services.Clients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("Hello World");
        IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                ConfigureService(context.HostingEnvironment, context.Configuration, services);
            })
            .ConfigureServices(services => services.AddHostedService<EdgeModuleWorker>())
            .Build();

        await host.RunAsync();
    }

    private static void ConfigureService(IHostEnvironment hostEnvironment, IConfiguration configuration, IServiceCollection services)
    {
        services.AddSingleton<IEdgeModule, VideoCaptureEdgeModule>();

        if (hostEnvironment.IsDevelopment())
        {
            services.Configure<VideoCaptureConfiguration>((options) =>
            {
                options.CameraName = "Fake Camera";
                options.CameraUrl = "127.0.0.1";
            });
            services.AddSingleton<IModuleClientWrapper, DebugModuleClientWrapper<VideoCaptureConfiguration>>();
            services.AddSingleton<IFileUploadService, DebugUploadService>();
        }

        if (hostEnvironment.IsProduction())
        {
            services.AddSingleton<IModuleClientWrapper, ModuleClientWrapper>();
            services.AddSingleton<IDeviceClientWrapper, DeviceClientWrapper>();
        }
    }
}