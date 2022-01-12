﻿using NLog.Web;
using Nuages.Localization.Storage.Config.Sources;

namespace Nuages.Identity.UI;

/// <summary>
/// The Main function can be used to run the ASP.NET Core application locally using the Kestrel webserver.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class LocalEntryPoint
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(config =>
            {
                config.AddJsonFile("appsettings.local.json", true, true);
 
                config.AddJsonFileTranslation("/wwwroot/locales/fr-CA.json");
                config.AddJsonFileTranslation("/wwwroot/locales/en-CA.json");
            })
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
}