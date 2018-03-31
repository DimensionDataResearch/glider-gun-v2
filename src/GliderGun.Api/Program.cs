using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GliderGun.Api
{
    /// <summary>
    ///     The Glider Gun API host.
    /// </summary>
    public static class Program
    {
        /// <summary>
        ///     The main program entry-point.
        /// </summary>
        /// <param name="args">
        ///     Command-line arguments.
        /// </param>
        public static void Main(string[] args) => CreateWebHostBuilder(args).Build().Run();

        /// <summary>
        ///     Create a web host builder for the Glider Gun API.
        /// </summary>
        /// <param name="args">
        ///     Command-line arguments.
        /// </param>
        /// <returns>
        ///     The configured <see cref="IWebHostBuilder"/>.
        /// </returns>
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(configuration =>
                {
                    configuration.AddUserSecrets<Startup>();
                    configuration.AddEnvironmentVariables(
                        prefix: "GLIDERGUN_"
                    );
                })
                .ConfigureLogging((context, logging) =>
                {
                    Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Information()
                        .Enrich.FromLogContext()
                        .Enrich.WithDemystifiedStackTraces()
                        .WriteTo.Debug()
                        .WriteTo.LiterateConsole()
                        .CreateLogger();

                    logging.ClearProviders();
                    logging.AddSerilog(Log.Logger);
                })
                .UseStartup<Startup>();
    }
}
