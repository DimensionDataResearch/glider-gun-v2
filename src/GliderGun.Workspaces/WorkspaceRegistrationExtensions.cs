using Microsoft.Extensions.DependencyInjection;
using System;

namespace GliderGun.Workspaces
{
    /// <summary>
    ///     Extension methods for registering workspace management components.
    /// </summary>
    public static class WorkspaceRegistrationExtensions
    {
        /// <summary>
        ///     Register the Glider Gun workspace manager components.
        /// </summary>
        /// <param name="services">
        ///     The service collection to configure.
        /// </param>
        /// <returns>
        ///     The configured service collection.
        /// </returns>
        public static IServiceCollection AddWorkspaceManager(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddScoped<WorkspaceManager>();

            return services;
        }

        /// <summary>
        ///     Register the Glider Gun workspace manager components and configure workspace management options.
        /// </summary>
        /// <param name="services">
        ///     The service collection to configure.
        /// </param>
        /// <param name="configureOptions">
        ///     A delegate that configures the <see cref="WorkspaceOptions"/>.
        /// </param>
        /// <returns>
        ///     The configured service collection.
        /// </returns>
        public static IServiceCollection AddWorkspaceManager(this IServiceCollection services, Action<WorkspaceOptions> configureOptions)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configureOptions == null)
                throw new ArgumentNullException(nameof(configureOptions));
            
            services.AddWorkspaceManager();
            services.Configure<WorkspaceOptions>(configureOptions);

            return services;
        }
    }
}