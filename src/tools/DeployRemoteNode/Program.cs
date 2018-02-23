using HTTPlease;
using KubeClient;
using KubeClient.Models;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GliderGun.Tools.DeployRemoteNode
{
    using KubeTemplates;

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

                using (ServiceProvider serviceProvider = BuildServiceProvider(options))
                {
                    KubeApiClient client = serviceProvider.GetRequiredService<KubeApiClient>();
                    KubeResources kubeResources = serviceProvider.GetRequiredService<KubeResources>();

                    
                    string jobName = kubeResources.Names.SafeId(options.JobName);
                    
                    JobV1 existingJob = await client.JobsV1().Get(jobName);
                    if (existingJob != null)
                    {
                        Log.Information("Found existing job {JobName} in namespace {KubeNamespace}; deleting...",
                            existingJob.Metadata.Name,
                            existingJob.Metadata.Namespace
                        );

                        await client.JobsV1().Delete(jobName,
                            propagationPolicy: DeletePropagationPolicy.Foreground
                        );

                        Log.Information("Deleted existing job {JobName}.",
                            existingJob.Metadata.Name,
                            existingJob.Metadata.Namespace
                        );
                    }

                    Log.Information("Creating deployment job {JobName}...", jobName);

                    JobV1 deploymentJob = kubeResources.DeployGliderGunRemoteJob(options);
                    
                    try
                    {
                        deploymentJob = await client.JobsV1().Create(deploymentJob);
                    }
                    catch (HttpRequestException<StatusV1> createJobFailed)
                    {
                        Log.Error(createJobFailed, "Failed to create Kubernetes Job {JobName} for deployment ({Reason}): {ErrorMessage}",
                            jobName,
                            createJobFailed.Response.Reason,
                            createJobFailed.Response.Message
                        );

                        return ExitCodes.JobFailed;
                    }

                    Log.Information("Created deployment job {JobName}.", deploymentJob.Metadata.Name);

                    while (deploymentJob != null)
                    {
                        await Task.Delay(
                            TimeSpan.FromSeconds(2) // Poll period
                        );

                        Log.Verbose("Polling status for deployment job {JobName} in namespace {KubeNamespace}...",
                            deploymentJob.Metadata.Name,
                            deploymentJob.Metadata.Namespace
                        );

                        deploymentJob = await client.JobsV1().Get(jobName);
                        if (deploymentJob == null)
                        {
                            Log.Error("Cannot find deployment job {JobName} in namespace {KubeNamespace}.",
                                deploymentJob.Metadata.Name,
                                deploymentJob.Metadata.Namespace
                            );

                            return ExitCodes.UnexpectedError;
                        }
                        
                        if (deploymentJob.Status.Active > 0)
                        {
                            Log.Verbose("Deployment job {JobName} is still active.",
                                deploymentJob.Metadata.Name
                            );

                            continue;
                        }
                        
                        if (deploymentJob.Status.Succeeded > 0)
                        {
                            Log.Information("Deployment job {JobName} completed successfully.",
                                deploymentJob.Metadata.Name
                            );

                            break;
                        }
                        
                        if (deploymentJob.Status.Failed > 0)
                        {
                            Log.Error("Deployment job {JobName} failed.",
                                deploymentJob.Metadata.Name
                            );
                            foreach (JobConditionV1 jobCondition in deploymentJob.Status.Conditions)
                            {
                                Log.Error("Deployment job {JobName} failed ({Reason}): {ErrorMessage}.",
                                    deploymentJob.Metadata.Name,
                                    jobCondition.Reason,
                                    jobCondition.Message
                                );
                            }

                            return ExitCodes.JobFailed;
                        } 
                    }
                }

                Log.Information("Done.");

                return ExitCodes.Success;
            }
            catch (Exception unexpectedError)
            {
                Console.WriteLine(unexpectedError);
                Log.Error(unexpectedError, "An unexpected error occurred while deploying the remote node.");

                return ExitCodes.UnexpectedError;
            }
            finally
            {
                Log.CloseAndFlush();
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
        ///     Build the application service provider.
        /// </summary>
        /// <param name="options">
        ///     Program options.
        /// </param>
        /// <returns>
        ///     The service provider, as a <see cref="ServiceProvider"/>.
        /// </returns>
        static ServiceProvider BuildServiceProvider(ProgramOptions options)
        {
            var services = new ServiceCollection();

            services.AddOptions();
            services.AddLogging(loggers =>
            {
                loggers.AddSerilog(Log.Logger);
            });

            services.AddKubeClientOptionsFromKubeConfig(
                kubeConfigFileName: options.KubeConfigFile,
                kubeContextName: options.KubeContextName,
                defaultNamespace: options.KubeNamespace
            );
            services.AddKubeClient();

            services.AddKubeTemplates(new KubeTemplateOptions
            {
                DefaultNamespace = options.KubeNamespace
            });

            return services.BuildServiceProvider();
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
            ///     The deployment job failed.
            /// </summary>
            public const int JobFailed = 2;

            /// <summary>
            ///     An unexpected error occurred during program execution.
            /// </summary>
            public const int UnexpectedError = 5;
        }
    }
}
