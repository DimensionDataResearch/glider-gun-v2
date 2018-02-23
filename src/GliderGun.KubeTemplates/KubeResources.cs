using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace GliderGun.KubeTemplates
{
    using KubeClient.Models;

    /// <summary>
    ///     Factory methods for common Kubernetes resources.
    /// </summary>
    public class KubeResources
    {
        /// <summary>
        ///     Create a new <see cref="KubeResources"/>.
        /// </summary>
        /// <param name="names">
        ///     The Kubernetes resource-naming strategy.
        /// </param>
        /// <param name="specs">
        ///     The factory for Kubernetes template specifications.
        /// </param>
        /// <param name="kubeOptions">
        ///     Application-level Kubernetes settings.
        /// </param>
        public KubeResources(KubeNames names, KubeSpecs specs, IOptions<KubeTemplateOptions> kubeOptions)
        {
            if (names == null)
                throw new ArgumentNullException(nameof(names));

            if (specs == null)
                throw new ArgumentNullException(nameof(specs));

            if (kubeOptions == null)
                throw new ArgumentNullException(nameof(kubeOptions));
            
            Names = names;
            Specs = specs;
            KubeOptions = kubeOptions.Value;
        }

        /// <summary>
        ///     The factory for Kubernetes specifications.
        /// </summary>
        public KubeNames Names { get; }

        /// <summary>
        ///     The factory for Kubernetes specifications.
        /// </summary>
        public KubeSpecs Specs { get; }

        /// <summary>
        ///     Application-level Kubernetes template settings.
        /// </summary>
        public KubeTemplateOptions KubeOptions { get; }

        /// <summary>
        ///     Create a new opaque <see cref="SecretV1"/>.
        /// </summary>
        /// <param name="name">
        ///     The secret name.
        /// </param>
        /// <param name="data">
        ///     The secret data.
        /// </param>
        /// <param name="labels">
        ///     An optional <see cref="Dictionary{TKey, TValue}"/> containing labels to apply to the persistent volume claim.
        /// </param>
        /// <param name="annotations">
        ///     An optional <see cref="Dictionary{TKey, TValue}"/> containing annotations to apply to the persistent volume claim.
        /// </param>
        /// <param name="kubeNamespace">
        ///     An optional target Kubernetes namespace.
        /// </param>
        /// <returns>
        ///     The configured <see cref="SecretV1"/>.
        /// </returns>
        public SecretV1 OpaqueSecret(string name, Dictionary<string, string> data, Dictionary<string, string> labels = null, Dictionary<string, string> annotations = null, string kubeNamespace = null)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'name'.", nameof(name));
            
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            return new SecretV1
            {
                ApiVersion = "v1",
                Kind = "Secret",
                Type = "Opaque",
                Metadata = new ObjectMetaV1
                {
                    Name = name,
                    Namespace = kubeNamespace,
                    Labels = labels,
                    Annotations = annotations
                },
                Data = data
            };
        }

        /// <summary>
        ///     Create a new <see cref="PersistentVolumeClaimV1"/>.
        /// </summary>
        /// <param name="name">
        ///     The deployment name.
        /// </param>
        /// <param name="spec">
        ///     A <see cref="PersistentVolumeClaimSpecV1"/> representing the persistent volume claim specification.
        /// </param>
        /// <param name="labels">
        ///     An optional <see cref="Dictionary{TKey, TValue}"/> containing labels to apply to the persistent volume claim.
        /// </param>
        /// <param name="annotations">
        ///     An optional <see cref="Dictionary{TKey, TValue}"/> containing annotations to apply to the persistent volume claim.
        /// </param>
        /// <param name="kubeNamespace">
        ///     An optional target Kubernetes namespace.
        /// </param>
        /// <returns>
        ///     The configured <see cref="PersistentVolumeClaimV1"/>.
        /// </returns>
        public PersistentVolumeClaimV1 DataVolumeClaim(string name, PersistentVolumeClaimSpecV1 spec, Dictionary<string, string> labels = null, Dictionary<string, string> annotations = null, string kubeNamespace = null)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'name'.", nameof(name));
            
            if (spec == null)
                throw new ArgumentNullException(nameof(spec));

            return new PersistentVolumeClaimV1
            {
                ApiVersion = "v1",
                Kind = "PersistentVolumeClaim",
                Metadata = new ObjectMetaV1
                {
                    Name = name,
                    Namespace = kubeNamespace,
                    Labels = labels,
                    Annotations = annotations
                },
                Spec = spec
            };
        }

        /// <summary>
        ///     Create a new <see cref="DeploymentV1Beta1"/>.
        /// </summary>
        /// <param name="name">
        ///     The deployment name.
        /// </param>
        /// <param name="spec">
        ///     A <see cref="DeploymentSpecV1Beta1"/> representing the deployment specification.
        /// </param>
        /// <param name="labels">
        ///     An optional <see cref="Dictionary{TKey, TValue}"/> containing labels to apply to the deployment.
        /// </param>
        /// <param name="annotations">
        ///     An optional <see cref="Dictionary{TKey, TValue}"/> containing annotations to apply to the deployment.
        /// </param>
        /// <param name="kubeNamespace">
        ///     An optional target Kubernetes namespace.
        /// </param>
        /// <returns>
        ///     The configured <see cref="DeploymentV1Beta1"/>.
        /// </returns>
        public DeploymentV1Beta1 Deployment(string name, DeploymentSpecV1Beta1 spec, Dictionary<string, string> labels = null, Dictionary<string, string> annotations = null, string kubeNamespace = null)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'name'.", nameof(name));
            
            if (spec == null)
                throw new ArgumentNullException(nameof(spec));

            return new DeploymentV1Beta1
            {
                ApiVersion = "apps/v1beta1",
                Kind = "Deployment",
                Metadata = new ObjectMetaV1
                {
                    Name = name,
                    Namespace = kubeNamespace,
                    Labels = labels,
                    Annotations = annotations
                },
                Spec = spec
            };
        }

        /// <summary>
        ///     Create a new <see cref="ReplicationControllerV1"/>.
        /// </summary>
        /// <param name="name">
        ///     The replication controller name.
        /// </param>
        /// <param name="spec">
        ///     A <see cref="ReplicationControllerSpecV1"/> representing the controller specification.
        /// </param>
        /// <param name="labels">
        ///     An optional <see cref="Dictionary{TKey, TValue}"/> containing labels to apply to the replication controller.
        /// </param>
        /// <param name="annotations">
        ///     An optional <see cref="Dictionary{TKey, TValue}"/> containing annotations to apply to the replication controller.
        /// </param>
        /// <param name="kubeNamespace">
        ///     An optional target Kubernetes namespace.
        /// </param>
        /// <returns>
        ///     The configured <see cref="ReplicationControllerV1"/>.
        /// </returns>
        public ReplicationControllerV1 ReplicationController(string name, ReplicationControllerSpecV1 spec, Dictionary<string, string> labels = null, Dictionary<string, string> annotations = null, string kubeNamespace = null)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'name'.", nameof(name));
            
            if (spec == null)
                throw new ArgumentNullException(nameof(spec));

            return new ReplicationControllerV1
            {
                ApiVersion = "v1",
                Kind = "ReplicationController",
                Metadata = new ObjectMetaV1
                {
                    Name = name,
                    Namespace = kubeNamespace,
                    Labels = labels,
                    Annotations = annotations
                },
                Spec = spec
            };
        }

        /// <summary>
        ///     Create a new <see cref="ServiceV1"/>.
        /// </summary>
        /// <param name="name">
        ///     The service name.
        /// </param>
        /// <param name="spec">
        ///     A <see cref="ServiceSpecV1"/> representing the service specification.
        /// </param>
        /// <param name="labels">
        ///     An optional <see cref="Dictionary{TKey, TValue}"/> containing labels to apply to the service.
        /// </param>
        /// <param name="annotations">
        ///     An optional <see cref="Dictionary{TKey, TValue}"/> containing annotations to apply to the service.
        /// </param>
        /// <param name="kubeNamespace">
        ///     An optional target Kubernetes namespace.
        /// </param>
        /// <returns>
        ///     The configured <see cref="ServiceV1"/>.
        /// </returns>
        public ServiceV1 Service(string name, ServiceSpecV1 spec, Dictionary<string, string> labels = null, Dictionary<string, string> annotations = null, string kubeNamespace = null)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'name'.", nameof(name));
            
            if (spec == null)
                throw new ArgumentNullException(nameof(spec));

            return new ServiceV1
            {
                ApiVersion = "v1",
                Kind = "Service",
                Metadata = new ObjectMetaV1
                {
                    Name = name,
                    Namespace = kubeNamespace,
                    Labels = labels,
                    Annotations = annotations
                },
                Spec = spec
            };
        }

        /// <summary>
        ///     Create a new <see cref="JobV1"/>.
        /// </summary>
        /// <param name="name">
        ///     The Job name.
        /// </param>
        /// <param name="spec">
        ///     A <see cref="JobSpecV1"/> representing the Job specification.
        /// </param>
        /// <param name="labels">
        ///     An optional <see cref="Dictionary{TKey, TValue}"/> containing labels to apply to the Job.
        /// </param>
        /// <param name="annotations">
        ///     An optional <see cref="Dictionary{TKey, TValue}"/> containing annotations to apply to the Job.
        /// </param>
        /// <param name="kubeNamespace">
        ///     An optional target Kubernetes namespace.
        /// </param>
        /// <returns>
        ///     The configured <see cref="JobV1"/>.
        /// </returns>
        public JobV1 Job(string name, JobSpecV1 spec, Dictionary<string, string> labels = null, Dictionary<string, string> annotations = null, string kubeNamespace = null)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'name'.", nameof(name));
            
            if (spec == null)
                throw new ArgumentNullException(nameof(spec));

            return new JobV1
            {
                ApiVersion = "batch/v1",
                Kind = "Job",
                Metadata = new ObjectMetaV1
                {
                    Name = name,
                    Namespace = kubeNamespace,
                    Labels = labels,
                    Annotations = annotations
                },
                Spec = spec
            };
        }
    }
}
