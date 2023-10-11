using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MuseQCApp.Constants;
using MuseQCApp.FileLogger;
using MuseQCApp.Helpers;
using MuseQCApp.Interfaces;
using MuseQCApp.Modules;
using MuseQCDBAccess.Data;
using MuseQCDBAccess.DbAccess;

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
                // Add single classes to DP
                .AddTransient<App>()
                .AddSingleton<MysqlDBData>()
                // Setup single classes used by Interfaces
                // TODO: This is messy, clean up
                .AddSingleton<ConfigHelper>()
                // Setup Interfaces
                .AddSingleton<IGoogleBucket, GoogleBucket>()
                .AddSingleton<ISiteLookup, SiteLookup>()
                .AddSingleton<IMuseQualityRunner, MuseQualityRunner>()
                .AddSingleton<IDataAccess, MySqlDataAccess>()
                .AddSingleton<IMuseQualityDecisions, MuseQualityDecisionsV1>()
                // Setup and add logger
                .AddLogging(options => {
                    string documentsDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    string appName = AppDomain.CurrentDomain.FriendlyName;
                    string logFileDirectory = Path.Combine(documentsDir, appName, "Logs");
                    if (Directory.Exists(logFileDirectory) == false)
                    {
                        Directory.CreateDirectory(logFileDirectory);
                    }
                    string currentdate = DateTime.Now.ToString(DateConstants.DateFormat);
                    string logFilePath = Path.Combine(logFileDirectory, $"log_{currentdate}.txt");

                    LogLevel minLogLevel = LogLevel.Information;
                    options.SetMinimumLevel(minLogLevel);
                    // Add file logger
                    options.AddFile(logFilePath, new FileLoggerConfiguration()
                    {
                        LogLevel = minLogLevel
                    });
                })
                // Add default logger so that ILogger can be retreived and does not need a class type
                // ie so we can do  GetService<ILogger>() instead of GetService<ILogger<ClassType>>()
                .AddTransient(provider => provider.GetService<ILoggerFactory>().CreateLogger("MuseQCApp"));
        });
}
