using KubeClient.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace GliderGun.Tools.DeployRemoteNode
{
    using KubeTemplates;

    /// <summary>
    ///     Deployment-related extension methods for <see cref="KubeResources"/> and <see cref="KubeSpecs"/>.
    /// </summary>
    static class KubeTemplateDeploymentExtensions
    {
        /// <summary>
        ///     Create a <see cref="JobV1"/> for deploying a Glider Gun Remote node.
        /// </summary>
        /// <param name="resources">
        ///     The Kubernetes resource template service.
        /// </param>
        /// <param name="options">
        ///     The current options for the deployment tool.
        /// </param>
        /// <returns>
        ///     The configured <see cref="JobV1"/>.
        /// </returns>
        public static JobV1 DeployGliderGunRemoteJob(this KubeResources resources, ProgramOptions options)
        {
            if (resources == null)
                throw new ArgumentNullException(nameof(resources));
            
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            return resources.Job(
                name: resources.Names.SafeId(options.JobName),
                spec: resources.Specs.DeployGliderGunRemoteJob(options),
                kubeNamespace: options.KubeNamespace
            );
        }

        /// <summary>
        ///     Create a <see cref="JobSpecV1"/> for deploying a Glider Gun Remote node.
        /// </summary>
        /// <param name="specs">
        ///     The Kubernetes specification template service.
        /// </param>
        /// <param name="options">
        ///     The current options for the deployment tool.
        /// </param>
        /// <returns>
        ///     The configured <see cref="JobSpecV1"/>.
        /// </returns>
        public static JobSpecV1 DeployGliderGunRemoteJob(this KubeSpecs specs, ProgramOptions options)
        {
            if (specs == null)
                throw new ArgumentNullException(nameof(specs));
            
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            return new JobSpecV1
            {
                ActiveDeadlineSeconds = options.Timeout,
                Completions = 1,
                Template = new PodTemplateSpecV1
                {
                    Metadata = new ObjectMetaV1
                    {
                        Name = "deploy-remote",
                        Labels = new Dictionary<string, string>
                        {
                            ["glider-gun.job.type"] = "deploy.glider-gun.remote"
                        }
                    },
                    Spec = new PodSpecV1
                    {
                        RestartPolicy = "Never",
                        Containers = new List<ContainerV1>
                        {
                            new ContainerV1
                            {
                                Image = "tintoyddr.azurecr.io/glider-gun/remote/node:latest",
                                ImagePullPolicy = "IfNotPresent",
                                Env = new List<EnvVarV1>
                                {
                                    new EnvVarV1
                                    {
                                        Name = "TF_VAR_dns_subdomain",
                                        Value = options.DnsSubdomain
                                    },
                                    new EnvVarV1
                                    {
                                        Name = "TF_VAR_ssh_key_file",
                                        Value = "/secrets/id_rsa"
                                    },
                                    new EnvVarV1
                                    {
                                        Name = "TF_VAR_ssh_public_key_file",
                                        Value = "/secrets/id_rsa.pub"
                                    }
                                },
                                VolumeMounts = new List<VolumeMountV1>
                                {
                                    new VolumeMountV1
                                    {
                                        Name = "state",
                                        MountPath = "/state"
                                    },
                                    new VolumeMountV1
                                    {
                                        Name = "ssh-key",
                                        MountPath = "/secrets/id_rsa"
                                    },
                                    new VolumeMountV1
                                    {
                                        Name = "ssh-public-key",
                                        MountPath = "/secrets/id_rsa.pub"
                                    }
                                }
                            }
                        },
                        Volumes = new List<VolumeV1>
                        {
                            new VolumeV1
                            {
                                Name = "state",
                                HostPath = new HostPathVolumeSourceV1
                                {
                                    Path = Path.GetFullPath(
                                        Path.Combine(options.WorkingDirectory, "state")
                                    )
                                }
                            },
                            new VolumeV1
                            {
                                Name = "ssh-key",
                                HostPath = new HostPathVolumeSourceV1
                                {
                                    Path = Path.GetFullPath(options.SshPrivateKeyFile)
                                }
                            },
                            new VolumeV1
                            {
                                Name = "ssh-public-key",
                                HostPath = new HostPathVolumeSourceV1
                                {
                                    Path = Path.GetFullPath(
                                        options.SshPublicKeyFile ?? options.SshPrivateKeyFile + ".pub"
                                    )
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
