using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MuseQCApp;

Console.WriteLine("Starting Muse QC process!");

// Setup Dependency injection
using IHost host = DependencyInjectionBuilder.CreateHostBuilder(args).Build();
using var scope = host.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
    // Run the application
    services.GetRequiredService<App>().Run();
}
catch(Exception ex)
{
    Console.WriteLine($"An error occured: {ex.Message}");
    Console.ReadLine();
}