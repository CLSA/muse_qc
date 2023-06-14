using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MuseQCApp.Helpers;
using MuseQCApp.Interfaces;
using MuseQCApp.Modules;

namespace MuseQCApp;

/// <summary>
/// A class to setup the dependency injection that is used throughout the app
/// </summary>
public class DependencyInjectionBuilder
{
    public static IHostBuilder CreateHostBuilder(string[] args) => 
        Host.CreateDefaultBuilder(args)
        .ConfigureServices((_, services) =>
        {
            // NOTE: Some of the implementations require various fields to be added to the configuration
            //       The required configuration parameters should be documented in the class near the top
            //       of the file in a comment. Please check that the expected configuration parameters
            //       are in the appsettings.production.json to ensure everything works as expected.
            services
                .AddSingleton<IGoogleBucket, GoogleBucket>()
                .AddSingleton<IFileLocations, FileLocations>()
                .AddSingleton<ISiteLookup, SiteLookup>()
                .AddSingleton<IMuseQualityRunner, MuseQualityRunner>()
                .AddSingleton<IQualityReport, QualityReport>()
                .AddSingleton<ICleanUp, CleanUp>()
                .AddSingleton<ConfigHelper>()
                .AddTransient<App>();
        });
}
