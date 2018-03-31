using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GliderGun.DBTool
{
    using Data;

    /// <summary>
    ///     The Glider Gun database tool.
    /// </summary>
    static class Program
    {
        /// <summary>
        ///     The main program entry-point.
        /// </summary>
        /// <param name="commandLineArguments">
        ///     The program's command-line arguments.
        /// </param>
        /// <returns>
        ///     The program exit-code.
        /// </returns>
        static async Task<int> Main(string[] commandLineArguments)
        {
            // Show help if no arguments are specified.
            bool showHelp = commandLineArguments.Length == 0;
            if (showHelp)
                commandLineArguments = new[] { "--help" };

            ProgramOptions options = ProgramOptions.Parse(commandLineArguments);
            if (options == null)
                return showHelp ? ExitCodes.Success : ExitCodes.InvalidArguments;

            ConfigureLogging(options);

            try
            {
                using (CancellationTokenSource cancellationSource = new CancellationTokenSource())
                using (ServiceProvider serviceProvider = BuildServiceProvider(options))
                using (DataContext dataContext = serviceProvider.GetRequiredService<DataContext>())
                {
                    cancellationSource.CancelAfter(
                        TimeSpan.FromSeconds(30)
                    );

                    Log.Information("Configuring the Glider Gun database...");

                    await dataContext.Database.EnsureCreatedAsync(cancellationSource.Token);
                    await dataContext.Database.MigrateAsync(cancellationSource.Token);

                    Log.Information("Database exists and is up-to-date.");
                }

                return ExitCodes.Success;
            }
            catch (Exception unexpectedError)
            {
                Log.Error(unexpectedError, "Unexpected error.");

                return ExitCodes.UnexpectedError;
            }
        }

        /// <summary>
        ///     Configure the global application logger.
        /// </summary>
        /// <param name="options">
        ///     Program options.
        /// </param>
        static void ConfigureLogging(ProgramOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.LiterateConsole(
                    outputTemplate: "[{Level:u3}] {Message:l}{NewLine}{Exception}"
                );

            if (options.Verbose)
                loggerConfiguration.MinimumLevel.Verbose();

            Log.Logger = loggerConfiguration.CreateLogger();
        }

        /// <summary>
        ///     Build a service provider that can be used to resolve components and services used by the database tool.
        /// </summary>
        /// <param name="options">
        ///     Program options.
        /// </param>
        /// <returns>
        ///     The configured <see cref="ServiceProvider"/>.
        /// </returns>
        static ServiceProvider BuildServiceProvider(ProgramOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            IServiceCollection services = new ServiceCollection();

            services.AddOptions();
            services.AddEntityFrameworkSqlServer();
            services.AddDbContext<DataContext>(
                dataContext => dataContext.UseSqlServer(
                    connectionString: $"Data Source={options.ServerName},{options.ServerPort};Initial Catalog={options.DatabaseName};UID={options.UserName};PWD={options.Password}"
                )
            );

            return services.BuildServiceProvider();
        }

        /// <summary>
        ///     Global initialisation.
        /// </summary>
        static Program()
        {
            if (SynchronizationContext.Current == null)
            {
                SynchronizationContext.SetSynchronizationContext(
                    new SynchronizationContext()
                );
            }
        }

        /// <summary>
        ///     Well-known program exit codes.
        /// </summary>
        public static class ExitCodes
        {
            /// <summary>
            ///     Program completed successfully.
            /// </summary>
            public const int Success = 0;

            /// <summary>
            ///     One or more command-line arguments were missing or invalid.
            /// </summary>
            public const int InvalidArguments = 1;

            /// <summary>
            ///     An unexpected error occurred during program execution.
            /// </summary>
            public const int UnexpectedError = 5;
        }
    }
}
