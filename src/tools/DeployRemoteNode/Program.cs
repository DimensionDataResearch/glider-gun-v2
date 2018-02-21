using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace GliderGun.Tools.DeployRemoteNode
{
    /// <summary>
    ///     Tool for deploying a new Kubernetes cluster (with 1, 3, or 5 hosts) running the Glider Gun remote agent.
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

            try
            {
                SynchronizationContext.SetSynchronizationContext(
                    new SynchronizationContext()
                );

                ProgramOptions options = ProgramOptions.Parse(commandLineArguments);
                if (options == null)
                    return showHelp ? ExitCodes.Success : ExitCodes.InvalidArguments;

                ConfigureLogging(options);

                Log.Information("TODO: Implement.");

                await Task.Yield();

                Log.Information("Done.");

                return ExitCodes.Success;
            }
            catch (Exception unexpectedError)
            {
                Log.Error(unexpectedError, "An unexpected error occurred while deploying the remote node.");

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
                .WriteTo.LiterateConsole();

            if (options.Verbose)
                loggerConfiguration.MinimumLevel.Verbose();

            Log.Logger = loggerConfiguration.CreateLogger();
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
            public const int InvalidArguments = 4;

            /// <summary>
            ///     An unexpected error occurred during program execution.
            /// </summary>
            public const int UnexpectedError = 5;
        }
    }
}
