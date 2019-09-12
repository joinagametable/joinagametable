using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JoinAGameTable.Extensions;
using JoinAGameTable.Models;
using JoinAGameTable.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JoinAGameTable
{
    /// <summary>
    /// This class is used to configure and start the ASP.NET Core server.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Build a new instance.
        /// </summary>
        /// <param name="hostingEnvironment">The current running environment</param>
        public Startup(IHostingEnvironment hostingEnvironment)
        {
            // Initialize configuration builder
            var configurationBuilder = new ConfigurationBuilder().SetBasePath(hostingEnvironment.ContentRootPath);

            // Use local configuration files
            configurationBuilder = configurationBuilder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            configurationBuilder = configurationBuilder.AddJsonFile($"appsettings.{hostingEnvironment.EnvironmentName}.json", optional: true);

            // Use etcd configuration if needed
            var envEnabled = Environment.GetEnvironmentVariable("JAGT_ETCD_CFG_ENABLE");
            if (!string.IsNullOrEmpty(envEnabled))
            {
                // Retrieves configuration from environment variables
                var envUrls = Environment.GetEnvironmentVariable("JAGT_ETCD_CFG_URLS")?.Split(",");
                var envUsername = Environment.GetEnvironmentVariable("JAGT_ETCD_CFG_USERNAME");
                var endPassword = Environment.GetEnvironmentVariable("JAGT_ETCD_CFG_PASSWORD");
                var endRootPath = Environment.GetEnvironmentVariable("JAGT_ETCD_CFG_ROOTPATH");
                var endIgnoreCertificateError = bool.Parse(Environment.GetEnvironmentVariable("JAGT_ETCD_CFG_IGNORE_CERT_ERROR") ?? "false");

                // Add etcd configuration
                configurationBuilder = configurationBuilder.AddEtcdConfiguration(new EtcdConfigurationOptions
                {
                    Urls = envUrls ?? new[] {"http://127.0.0.1:4001"},
                    Username = envUsername,
                    Password = endPassword,
                    RootPath = endRootPath ?? "/",
                    IgnoreCertificateError = endIgnoreCertificateError
                });
            }

            // Use environment variables
            configurationBuilder = configurationBuilder.AddEnvironmentVariables("JAGT");

            // Build configuration
            Configuration = configurationBuilder.Build();
        }

        /// <summary>
        /// Current application configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container
        /// </summary>
        /// <param name="services">Handle to the service collection to use</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Enable logging
            services.AddLogging();

            // Enable distributed memory cache
            services.AddDistributedMemoryCache();

            // Enable authentication
            services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.Name = "Session";
                    options.Events.OnValidatePrincipal = PrincipalValidator.ValidateAsync;
                    options.LoginPath = new PathString("/authentication");
                    options.LogoutPath = new PathString("/logout");
                });

            // Enable CSRF protection
            services.AddAntiforgery(options => { options.Cookie.Name = "Security"; });

            // Enable database
            services.AddDbContextPool<AppDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"))
            );

            // Enable MVC with localization
            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddMvcLocalization()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization();

            // Enable localization / globalization via portable object files (.po)
            services.AddPortableObjectLocalization(options => options.ResourcesPath = "Localizations");

            // Set current accepted localization
            services.Configure<RequestLocalizationOptions>(
                opts =>
                {
                    var supportedCultures = new List<CultureInfo>
                    {
                        new CultureInfo("en"),
                        new CultureInfo("fr")
                    };

                    // Which culture to use by default?
                    opts.DefaultRequestCulture = new RequestCulture("en");

                    // Formatting numbers, dates, etc.
                    opts.SupportedCultures = supportedCultures;

                    // UI strings that we have localized.
                    opts.SupportedUICultures = supportedCultures;

                    // Change cookie name
                    opts.RequestCultureProviders.OfType<CookieRequestCultureProvider>().First().CookieName = "Language";
                });

            services.AddSingleton(typeof(ILogger<FileStorageService>), typeof(Logger<FileStorageService>));
            services.AddSingleton<IFileStorageService, FileStorageService>();
            services.AddSingleton<IImageManipulationService, ImageManipulationService>();
            services.AddSingleton<IMarkdownService, MarkdownService>();
            services.AddScoped<ISendMailService, SendMailService>();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="applicationBuilder">The application builder</param>
        /// <param name="hostingEnvironment">The current running environment</param>
        public void Configure(IApplicationBuilder applicationBuilder,
                              IHostingEnvironment hostingEnvironment)
        {
            if (hostingEnvironment.IsDevelopment())
            {
                // When the app runs in the Development environment:
                //   Use the Developer Exception Page to report app runtime errors
                //   Use the Database Error Page to report database runtime errors
                applicationBuilder.UseDeveloperExceptionPage();
            }
            else
            {
                // When the app doesn't run in the Development environment:
                //   Enable the Exception Handler Middleware to catch exceptions
                //     thrown in the following middlewares
                //   Use the HTTP Strict Transport Security Protocol (HSTS)
                //     Middleware
                applicationBuilder.UseExceptionHandler("/error");
                applicationBuilder.UseStatusCodePagesWithReExecute("/error", "?httpErrorCode={0}");
                applicationBuilder.UseHsts();
            }

            // UseRequestLocalization initializes a RequestLocalizationOptions object. On every request the list
            // of RequestCultureProvider in the RequestLocalizationOptions is enumerated and the first provider
            // that can successfully determine the request culture is used. The default providers come from the
            // RequestLocalizationOptions class:
            //   1. QueryStringRequestCultureProvider
            //   2. CookieRequestCultureProvider
            //   3. AcceptLanguageHeaderRequestCultureProvider
            // If none of the providers can determine the request culture, the DefaultRequestCulture is used
            var reqLocalizationOptions = applicationBuilder.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            applicationBuilder.UseRequestLocalization(reqLocalizationOptions.Value);

            // Return static files and end the pipeline.
            applicationBuilder.UseStaticFiles();

            // When a front-end like Nginx or Traefik is used
            applicationBuilder.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            // Enable authentication
            applicationBuilder.UseAuthentication();

            // Add MVC to the request pipeline
            applicationBuilder.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
