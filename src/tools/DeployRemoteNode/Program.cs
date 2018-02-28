using HTTPlease;
using KubeClient;
using KubeClient.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GliderGun.Tools.DeployRemoteNode
{
    using KubeTemplates;
	using Microsoft.Extensions.Logging;

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
                using (AutoResetEvent done = new AutoResetEvent(initialState: false))
                {
                    KubeApiClient client = serviceProvider.GetRequiredService<KubeApiClient>();
                    KubeResources kubeResources = serviceProvider.GetRequiredService<KubeResources>();
                    
                    string jobName = kubeResources.Names.DeployGliderGunRemoteJob(options);

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

                    string secretName = kubeResources.Names.DeployGliderGunRemoteSecret(options);

                    SecretV1 existingSecret = await client.SecretsV1().Get(secretName);
                    if (existingSecret != null)
                    {
                        Log.Information("Found existing secret {SecretName} in namespace {KubeNamespace}; deleting...",
                            existingSecret.Metadata.Name,
                            existingSecret.Metadata.Namespace
                        );

                        await client.SecretsV1().Delete(secretName);

                        Log.Information("Deleted existing secret {SecretName}.",
                            existingSecret.Metadata.Name,
                            existingSecret.Metadata.Namespace
                        );
                    }

                    Log.Information("Creating deployment secret {SecretName}...", secretName);
                    
                    SecretV1 deploymentSecret = kubeResources.DeployGliderGunRemoteSecret(options);
                    try
                    {
                        deploymentSecret = await client.SecretsV1().Create(deploymentSecret);
                    }
                    catch (HttpRequestException<StatusV1> createSecretFailed)
                    {
                        Log.Error(createSecretFailed, "Failed to create Kubernetes Secret {SecretName} for deployment ({Reason}): {ErrorMessage}",
                            secretName,
                            createSecretFailed.Response.Reason,
                            createSecretFailed.Response.Message
                        );

                        return ExitCodes.JobFailed;
                    }

                    Log.Information("Created deployment secret {SecretName}.", deploymentSecret.Metadata.Name);

                    // Watch for job's associated pod to start, then monitor the pod's log until it completes.
                    IDisposable jobLogWatch = null;
                    IDisposable jobPodWatch = client.PodsV1().WatchAll(
                        labelSelector: $"job-name={jobName}",
                        kubeNamespace: options.KubeNamespace
                    ).Subscribe(
                        podEvent =>
                        {
                            if (jobLogWatch != null)
                                return;

                            PodV1 jobPod = podEvent.Resource;
                            if (jobPod.Status.Phase != "Pending")
                            {
                                Log.Information("Job {JobName} has started.", jobName);

                                Log.Verbose("Hook up log monitor for Pod {PodName} of Job {JobName}...",
                                    jobPod.Metadata.Name,
                                    jobName
                                );

                                jobLogWatch = client.PodsV1().StreamLogs(
                                    name: jobPod.Metadata.Name,
                                    kubeNamespace: jobPod.Metadata.Namespace
                                ).Subscribe(
                                    logEntry =>
                                    {
                                        Log.Information("[{PodName}] {LogEntry}", jobPod.Metadata.Name, logEntry);
                                    },
                                    error =>
                                    {
                                        if (error is HttpRequestException<StatusV1> requestError)
                                        {
                                            Log.Error(requestError, "Kubernetes API request error ({Reason}): {ErrorMessage:l}",
                                                requestError.Response.Reason,
                                                requestError.Response.Message
                                            );
                                        }
                                        else
                                            Log.Error(error, "JobLog Error");
                                    },
                                    () =>
                                    {
                                        Log.Information("[{PodName}] <end of log>", jobPod.Metadata.Name);
                                        
                                        done.Set();
                                    }
                                );

                                Log.Information("Monitoring log for Pod {PodName} of Job {JobName}.",
                                    jobPod.Metadata.Name,
                                    jobName
                                );
                            }
                        },
                        error =>
                        {
                            Log.Error(error, "PodWatch Error");
                        },
                        () =>
                        {
                            Log.Information("PodWatch End");
                        }
                    );

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

                    TimeSpan timeout = TimeSpan.FromSeconds(options.Timeout);
                    Log.Information("Waiting up to {TimeoutSeconds} seconds for deployment job {JobName} to complete.",
                        timeout.TotalSeconds,
                        jobName
                    );
                    if (!done.WaitOne(timeout))
                    {
                        using (jobPodWatch)
                        using (jobLogWatch)
                        {
                            Log.Error("Timed out after waiting {TimeoutSeconds} seconds for deployment job {JobName} to complete.",
                                timeout.TotalSeconds,
                                jobName
                            );

                            return ExitCodes.JobTimeout;
                        }
                    }

                    jobPodWatch?.Dispose();
                    jobLogWatch?.Dispose();

                    deploymentJob = await client.JobsV1().Get(jobName);
                    if (deploymentJob == null)
                    {
                        Log.Error("Cannot find deployment job {JobName} in namespace {KubeNamespace}.",
                            deploymentJob.Metadata.Name,
                            deploymentJob.Metadata.Namespace
                        );

                        return ExitCodes.UnexpectedError;
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

                    if (deploymentJob.Status.Succeeded > 0)
                    {
                        Log.Information("Deployment job {JobName} completed successfully.",
                            deploymentJob.Metadata.Name
                        );
                    }
                }

                Log.Information("Done.");

                return ExitCodes.Success;
            }
            catch (HttpRequestException<StatusV1> kubeRequestError)
            {
                Log.Error(kubeRequestError, "A Kubernetes API request failed while deploying the remote node ({Reason}): {ErrorMessage}",
                    kubeRequestError.Response.Reason,
                    kubeRequestError.Response.Message
                );

                return ExitCodes.JobFailed;
            }
            catch (Exception unexpectedError)
            {
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
                .WriteTo.LiterateConsole(
                    outputTemplate: "[{Level:u3}] {Message:l}{NewLine}{Exception}"
                );

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

				// Propagate log-level configuration to MEL-style loggers (such as those used by KubeClient / HTTPlease).
				loggers.SetMinimumLevel(
					options.Verbose ? LogLevel.Debug : LogLevel.Information
				);
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
            ///     The deployment job failed to complete within the timeout period.
            /// </summary>
            public const int JobTimeout = 3;

            /// <summary>
            ///     An unexpected error occurred during program execution.
            /// </summary>
            public const int UnexpectedError = 5;
        }
    }
}
