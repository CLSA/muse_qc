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
    Console.WriteLine(args.Length);

    List<int> parsedArgs = ParseArgs(args);

    // Run the application
    if (parsedArgs.Count == 0)
    {
        services.GetRequiredService<App>().Run();
    }
    else if (parsedArgs.Count == 1)
    {
        services.GetRequiredService<App>().Run(parsedArgs[0]);
    }
    else
    {
        services.GetRequiredService<App>().Run(parsedArgs[0], parsedArgs[1]);
    }
}
catch(Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
    Console.ReadLine();
}


List<int> ParseArgs(string[] args)
{
    List<int> parsedArgs = new();
    try
    {
        if (args.Length >= 1)
        {
            parsedArgs.Add(int.Parse(args[0]));
        }

        if (args.Length >= 2)
        {
            parsedArgs.Add(int.Parse(args[1]));
        }
    }
    catch 
    {
        Console.WriteLine("Problem parsing one or more of the args passed in. Expected 0-2 integer arguments");
    }

    return parsedArgs;
}