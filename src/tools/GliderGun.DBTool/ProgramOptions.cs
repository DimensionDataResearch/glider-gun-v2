using CommandLine;

namespace GliderGun.DBTool
{
    /// <summary>
    ///     Program options for the database tool.
    /// </summary>
    class ProgramOptions
    {
        /// <summary>
        ///     The name of the target database server.
        /// </summary>
        [Option('s', "server", Required = true, HelpText = "The name of the target database server.")]
        public string ServerName { get; set; }

        /// <summary>
        ///     The port on the target database server.
        /// </summary>
        [Option('p', "port", Default = 1443, HelpText = "The port on the target database server.")]
        public int ServerPort { get; set; }

        /// <summary>
        ///     The name of the target database.
        /// </summary>
        [Option('d', "database", Required = true, HelpText = "The name of the target database.")]
        public string DatabaseName { get; set; }

        [Option("user", HelpText = "The name of the user for authenticating to the database server.")]
        public string UserName { get; set; }

        [Option("password", HelpText = "The password for authenticating to the database server.")]
        public string Password { get; set; }

        /// <summary>
        ///     Enable verbose logging.
        /// </summary>
        [Option('v', "verbose", Default = false, HelpText = "Enable verbose logging.")]
        public bool Verbose { get; set; }

        /// <summary>
        ///     Parse program options from command-line arguments.
        /// </summary>
        /// <param name="commandLineArguments">
        ///     The command-line arguments
        /// </param>
        /// <returns>
        ///     The parsed <see cref="ProgramOptions"/>, or <c>null</c> if the command-line arguments could not be parsed.
        /// </returns>
        public static ProgramOptions Parse(string[] commandLineArguments)
        {
            ProgramOptions options = null;

            Parser.Default.ParseArguments<ProgramOptions>(commandLineArguments)
                .WithParsed(parsedOptions => options = parsedOptions);

            return options;
        }
    }
}
