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
        ///     Create a <see cref="SecretV1"/> for deploying a Glider Gun Remote node.
        /// </summary>
        /// <param name="resources">
        ///     The Kubernetes resource template service.
        /// </param>
        /// <param name="options">
        ///     The current options for the deployment tool.
        /// </param>
        /// <returns>
        ///     The configured <see cref="SecretV1"/>.
        /// </returns>
        public static SecretV1 DeployGliderGunRemoteSecret(this KubeResources resources, ProgramOptions options)
        {
            if (resources == null)
                throw new ArgumentNullException(nameof(resources));
            
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            return resources.OpaqueSecret(
                name: resources.Names.DeployGliderGunRemoteSecret(options),
                kubeNamespace: options.KubeNamespace,
                labels: new Dictionary<string, string>
                {
                    ["glider-gun.job.name"] = options.JobName,
                    ["glider-gun.job.type"] = "deploy.glider-gun.remote"
                },
                data: new Dictionary<string, string>
                {
                    ["id_rsa"] = Convert.ToBase64String(
                        File.ReadAllBytes(options.SshPrivateKeyFile)
                    ),
                    ["id_rsa.pub"] = Convert.ToBase64String(
                        File.ReadAllBytes(options.SshPublicKeyFile ?? options.SshPrivateKeyFile + ".pub")
                    )
                }
            );
        }

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
                name: resources.Names.DeployGliderGunRemoteJob(options),
                kubeNamespace: options.KubeNamespace,
                spec: resources.Specs.DeployGliderGunRemoteJob(options),
                labels: new Dictionary<string, string>
                {
                    ["glider-gun.job.name"] = options.JobName,
                    ["glider-gun.job.type"] = "deploy.glider-gun.remote"
                }
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
                            ["glider-gun.job.name"] = options.JobName,
                            ["glider-gun.job.type"] = "deploy.glider-gun.remote"
                        }
                    },
                    Spec = new PodSpecV1
                    {
                        RestartPolicy = "Never",
                        ImagePullSecrets = specs.ImagePullSecrets(options),
                        Containers = new List<ContainerV1>
                        {
                            new ContainerV1
                            {
                                Image = $"tintoyddr.azurecr.io/glider-gun/remote/node:{options.ImageTag}",
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
                                        Name = "secrets",
                                        MountPath = "/secrets",
                                        ReadOnly = true
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
                                    Path = ToUnixPath(
                                        Path.Combine(options.WorkingDirectory, "state")
                                    )
                                }
                            },
                            new VolumeV1
                            {
                                Name = "secrets",
                                Secret = new SecretVolumeSourceV1
                                {
                                    SecretName = specs.Names.SafeId(options.JobName)
                                }
                            }
                        }
                    }
                }
            };
        }

        /// <summary>
        ///     Compute the name for the Secret used to deploy a Glider Gun Remote node.
        /// </summary>
        /// <param name="names">
        ///     The Kubernetes resource-naming service.
        /// </param>
        /// <param name="options">
        ///     The current options for the deployment tool.
        /// </param>
        /// <returns>
        ///     The secret name.
        /// </returns>
        public static string DeployGliderGunRemoteSecret(this KubeNames names, ProgramOptions options)
        {
            if (names == null)
                throw new ArgumentNullException(nameof(names));
            
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            
            return names.SafeId(options.JobName);
        }

        /// <summary>
        ///     Compute the name for the Job used to deploy a Glider Gun Remote node.
        /// </summary>
        /// <param name="names">
        ///     The Kubernetes resource-naming service.
        /// </param>
        /// <param name="options">
        ///     The current options for the deployment tool.
        /// </param>
        /// <returns>
        ///     The secret name.
        /// </returns>
        public static string DeployGliderGunRemoteJob(this KubeNames names, ProgramOptions options)
        {
            if (names == null)
                throw new ArgumentNullException(nameof(names));
            
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            
            return names.SafeId(options.JobName);
        }

        /// <summary>
        ///     Generate a list of <see cref="LocalObjectReferenceV1"/>s representing image-pull secrets.
        /// </summary>
        /// <param name="specs">
        ///     The Kubernetes specification template service.
        /// </param>
        /// <param name="options">
        ///     The current options for the deployment tool.
        /// </param>
        /// <returns>
        ///     The list of <see cref="LocalObjectReferenceV1"/>s, or <c>null</c> if <see cref="ProgramOptions.ImagePullSecretName"/> has not been specified.
        /// </returns>
        public static List<LocalObjectReferenceV1> ImagePullSecrets(this KubeSpecs specs, ProgramOptions options)
        {
            if (specs == null)
                throw new ArgumentNullException(nameof(specs));
            
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            
            if (String.IsNullOrWhiteSpace(options.ImagePullSecretName))
                return null;

            return new List<LocalObjectReferenceV1>
            {
                new LocalObjectReferenceV1
                {
                    Name = options.ImagePullSecretName
                }
            };
        }

        /// <summary>
        ///     Convert the specified path to UNIX format.
        /// </summary>
        /// <param name="path">
        ///     The path to convert.
        /// </param>
        /// <returns>
        ///     The converted path.
        /// </returns>
        static string ToUnixPath(string path)
        {
            if (path == null)
                return path;

            return path.Replace('\\', '/');
        }
    }
}
