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

    (bool,List<int>) parsedArgs = ParseArgs(args);

    if (parsedArgs.Item1)
    {
        services.GetRequiredService<App>().CreateReports();
    }
    else
    {
        // Run the application
        List<int> parsedInts = parsedArgs.Item2;
        if (parsedInts.Count == 0)
        {
            services.GetRequiredService<App>().Run();
        }
        else if (parsedInts.Count == 1)
        {
            services.GetRequiredService<App>().Run(parsedInts[0]);
        }
        else
        {
            services.GetRequiredService<App>().Run(parsedInts[0], parsedInts[1]);
        }
    }
}
catch(Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
    Console.ReadLine();
}


(bool, List<int>) ParseArgs(string[] args)
{
    List<int> ints = new();
    if (args[0] == "CreateReports")
    {
        return (true, ints);
    }
    else
    {
        try
        {
            if (args.Length >= 1)
            {
                ints.Add(int.Parse(args[0]));
            }

            if (args.Length >= 2)
            {
                ints.Add(int.Parse(args[1]));
            }
        }
        catch
        {
            string argStr = "";
            foreach (string arg in args)
            {
                argStr += arg + " ";
            }
            Console.WriteLine($"Problem parsing one or more of the args passed in. Expected 0-2 integer arguments. Args: {argStr}");
            Console.ReadLine();
        }
    }

    return (false, ints);
}