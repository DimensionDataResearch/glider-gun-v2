using CommandLine;

namespace GliderGun.TemplateTestHarness
{
    /// <summary>
    ///     Options for the test-harness program.
    /// </summary>
    class ProgramOptions
    {
        /// <summary>
        ///     The name of the template image to use.
        /// </summary>
        [Option('i', "image", Required = true, HelpText = "The name of the template image to use.")]
        public string Image { get; set; }

        /// <summary>
        ///     The name of the JSON file containing template parameters.
        /// </summary>
        [Option('p', "parameters-from", Required = true, HelpText = "The name of the JSON file containing template parameters.")]
        public string ParametersFrom { get; set; }

        /// <summary>
        ///     The job timeout, in minutes.
        /// </summary>
        [Option('t', "timeout", Default = 10.0 * 60, HelpText = "The job timeout, in minutes.")]
        public double Timeout { get ;set ;}

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
