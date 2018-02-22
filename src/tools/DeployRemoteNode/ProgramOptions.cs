using CommandLine;

namespace GliderGun.Tools.DeployRemoteNode
{
    /// <summary>
    ///     Program options for the remote-node deployment tool.
    /// </summary>
    class ProgramOptions
    {
        /// <summary>
        ///     The path of the working directory (for job state and output).
        /// </summary>
        [Option('w', "work-dir", Required = true, HelpText = "The path of the working directory (for job state and output).")]
        public string WorkingDirectory { get; set; }

        /// <summary>
        ///     The path of the SSH private key file to use for initial communications with target hosts.
        /// </summary>
        [Option('k', "ssh-key-file", Required = true, HelpText = "The path of the SSH private key file to use for initial communications with target hosts.")]
        public string SshPrivateKeyFile { get; set; }

        /// <summary>
        ///     The path of the SSH public key file to use for initial communications with target hosts.
        /// </summary>
        [Option("ssh-public-key-file", Default = null, HelpText = "The path of the SSH public key file to use for initial communications with target hosts. If not specified, a '.pub' extension will be appended to the name of the SSH private key file.")]
        public string SshPublicKeyFile { get; set; }

        /// <summary>
        ///     The name of the DNS sub-domain for host registration.
        /// </summary>
        [Option('d', "dns-subdomain", Required = true, HelpText = "The name of the DNS sub-domain for host registration.")]
        public string DnsSubdomain { get; set; }

        /// <summary>
        ///     The tag for the deployment image to use.
        /// </summary>
        [Option('i', "image-tag", Default = "latest", HelpText = "The tag for the deployment image to use.")]
        public string ImageTag { get; set; }

        /// <summary>
        ///     The job timeout, in minutes.
        /// </summary>
        [Option('t', "timeout", Default = 10.0 * 60, HelpText = "The job timeout, in minutes.")]
        public double Timeout { get; set; }

        /// <summary>
        ///     The Kubernetes client configuration file to use (defaults to ~/.kube/config).
        /// </summary>
        [Option("kube-config-file", Default = null, HelpText = "The Kubernetes client configuration file to use (defaults to ~/.kube/config).")]
        public string KubeConfigFile { get; set; }

        /// <summary>
        ///     The name of a specific Kubernetes client configuration context to use (if not specified, the current context will be used).
        /// </summary>
        [Option("kube-context", Default = null, HelpText = "The name of a specific Kubernetes client configuration context to use (if not specified, the current context will be used).")]
        public string KubeContextName { get; set; }

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
