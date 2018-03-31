using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GliderGun.Api
{
    using Data;

    /// <summary>
    ///     Startup logic for the Glider Gun API.
    /// </summary>
    public class Startup
    {
        /// <summary>
        ///     Create a new <see cref="Startup"/>.
        /// </summary>
        /// <param name="configuration">
        ///     The application configuration.
        /// </param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        /// <summary>
        ///     The application configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        ///     Configure application services.
        /// </summary>
        /// <param name="services">
        ///     The application service collection.
        /// </param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            
            services.AddOptions();

            services.AddDataProtection(dataProtection =>
            {
                dataProtection.ApplicationDiscriminator = "GliderGun.v2";
            });
            
            services.AddEntityFrameworkSqlServer();
            services.AddDbContext<DataContext>(
                dataContext => dataContext.UseSqlServer(
                    connectionString: Configuration["Data:ConnectionString"]
                )
            );

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddJsonOptions(json =>
                {
                    json.SerializerSettings.Converters.Add(
                        new StringEnumConverter()
                    );
                });
        }

        /// <summary>
        ///     Configure the application pipeline.
        /// </summary>
        /// <param name="app">
        ///     The application pipeline builder.
        /// </param>
        /// <param name="loggers">
        ///     The application logger factory.
        /// </param>
        /// <param name="environment">
        ///     The application hosting environment.
        /// </param>
        /// <param name="appLifetime">
        ///     The application lifetime service.
        /// </param>
        public void Configure(IApplicationBuilder app, ILoggerFactory loggers, IHostingEnvironment environment, IApplicationLifetime appLifetime)
        {
            loggers.AddConsole();

            if (environment.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseMvc();

            appLifetime.ApplicationStopped.Register(Serilog.Log.CloseAndFlush);
        }
    }
}
