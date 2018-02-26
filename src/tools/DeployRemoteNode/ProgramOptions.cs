using CommandLine;

namespace GliderGun.Tools.DeployRemoteNode
{
    /// <summary>
    ///     Program options for the remote-node deployment tool.
    /// </summary>
    class ProgramOptions
    {
        /// <summary>
        ///     The path of the state directory on the Kubernetes host (for job state and output).
        /// </summary>
        /// <remarks>
        ///     The job name will be appended to this directory.
        /// </remarks>
        [Option('s', "state-dir", Required = true, HelpText = "The path of the state directory on the Kubernetes host (for job state and output).")]
        public string StateDirectory { get; set; }

        /// <summary>
        ///     The local path of the SSH private key file to use for initial communications with target hosts.
        /// </summary>
        [Option('k', "ssh-key-file", Required = true, HelpText = "The local path of the SSH private key file to use for initial communications with target hosts.")]
        public string SshPrivateKeyFile { get; set; }

        /// <summary>
        ///     The local path of the SSH public key file to use for initial communications with target hosts.
        /// </summary>
        [Option("ssh-public-key-file", Default = null, HelpText = "The local path of the SSH public key file to use for initial communications with target hosts.")]
        public string SshPublicKeyFile { get; set; }

        /// <summary>
        ///     The name of the DNS sub-domain for host registration.
        /// </summary>
        [Option('d', "dns-subdomain", Required = true, HelpText = "The name of the DNS sub-domain for host registration.")]
        public string DnsSubdomain { get; set; }

        /// <summary>
        ///     The name of the deployment image to use.
        /// </summary>
        [Option("image-name", Default = "tintoyddr.azurecr.io/glider-gun/remote/node", HelpText = "The name of the deployment image to use.")]
        public string ImageName { get; set; }

        /// <summary>
        ///     The tag of the deployment image to use.
        /// </summary>
        [Option("image-tag", Default = "latest", HelpText = "The tag of the deployment image to use.")]
        public string ImageTag { get; set; }

        /// <summary>
        ///     The name of a secret to use for when pulling the deployment image.
        /// </summary>
        [Option("image-pull-secret", Default = null, HelpText = "The name of a secret to use for when pulling the deployment image.")]
        public string ImagePullSecretName { get; set; }

        /// <summary>
        ///     A name for the new deployment job.
        /// </summary>
        [Option('n', "job-name", Default = "deploy-glider-gun-remote", HelpText = "A name for the new deployment job.")]
        public string JobName { get; set; }

        /// <summary>
        ///     The name of the target Kubernetes namespace.
        /// </summary>
        [Option("kube-namespace", Default = "default", HelpText = "The name of the target Kubernetes namespace.")]
        public string KubeNamespace { get; set; }

        /// <summary>
        ///     The job timeout, in minutes.
        /// </summary>
        [Option('t', "timeout", Default = 10 * 60, HelpText = "The job timeout, in minutes.")]
        public int Timeout { get; set; }

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
