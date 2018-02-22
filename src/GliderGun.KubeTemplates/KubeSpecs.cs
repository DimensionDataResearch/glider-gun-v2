using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace GliderGun.KubeTemplates
{
    using KubeClient.Models;

    /// <summary>
    ///     Factory methods for common Kubernetes resource specifications.
    /// </summary>
    public class KubeSpecs
    {
        /// <summary>
        ///     Create a new <see cref="KubeResources"/>.
        /// </summary>
        /// <param name="names">
        ///     The Kubernetes resource-naming strategy.
        /// </param>
        /// <param name="kubeOptions">
        ///     Application-level Kubernetes settings.
        /// </param>
        /// <param name="provisioningOptions">
        ///     Application-level provisioning options.
        /// </param>
        public KubeSpecs(KubeNames names, IOptions<KubeTemplateOptions> kubeOptions)
        {
            if (names == null)
                throw new ArgumentNullException(nameof(names));

            if (kubeOptions == null)
                throw new ArgumentNullException(nameof(kubeOptions));

            Names = names;
            KubeOptions = kubeOptions.Value;
        }

        /// <summary>
        ///     Application-level Kubernetes settings.
        /// </summary>
        public KubeNames Names { get; }

        /// <summary>
        ///     Application-level Kubernetes template settings.
        /// </summary>
        public KubeTemplateOptions KubeOptions { get; }

        // TODO: Create specification generators.
    }
}
