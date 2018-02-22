using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace GliderGun
{
    using KubeTemplates;

    /// <summary>
    ///     Extension methods for registering Glider Gun Kubernetes template services.
    /// </summary>
    public static class KubeTemplateRegistrationExtensions
    {
        /// <summary>
        ///     Register services for Kubernetes resource / specification templates.
        /// </summary>
        /// <param name="services">
        ///     The service collection to configure.
        /// </param>
        /// <param name="options">
        ///     Options that control how Kubernetes templates are created.
        /// </param>
        /// <returns>
        ///     The configured service collection.
        /// </returns>
        public static IServiceCollection AddKubeTemplates(this IServiceCollection services, KubeTemplateOptions options)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            
            services.AddKubeTemplates();
            services.AddSingleton(
                Options.Create(options)
            );

            return services;
        }

        /// <summary>
        ///     Register services for Kubernetes resource / specification templates.
        /// </summary>
        /// <param name="services">
        ///     The service collection to configure.
        /// </param>
        /// <returns>
        ///     The configured service collection.
        /// </returns>
        public static IServiceCollection AddKubeTemplates(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            
            services.AddScoped<KubeNames>();
            services.AddScoped<KubeResources>();
            services.AddScoped<KubeSpecs>();

            return services;
        }
    }
}